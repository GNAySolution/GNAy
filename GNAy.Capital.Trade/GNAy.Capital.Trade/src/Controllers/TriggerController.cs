using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class TriggerController
    {
        private static readonly string[] _timeFormats = new string[] { "HHmmss", "HHmm", "HH" };

        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ConcurrentQueue<TriggerData> _waitToAdd;

        private readonly SortedDictionary<string, TriggerData> _dataMap;
        private readonly ObservableCollection<TriggerData> _dataCollection;
        public int Count => _dataCollection.Count;
        public TriggerData this[string key] => _dataMap.TryGetValue(key, out TriggerData data) ? data : null;
        public TriggerData this[int index] => _dataCollection[index];

        private readonly ObservableCollection<TradeColumnTrigger> _triggerColumnCollection;

        public string RecoverFile { get; private set; }

        public TriggerController(in AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(TriggerController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<TriggerData>();

            _dataMap = new SortedDictionary<string, TriggerData>();
            _appCtrl.MainForm.DataGridTriggerRule.SetColumns(TriggerData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridTriggerRule.SetViewAndGetObservation<TriggerData>();

            _triggerColumnCollection = _appCtrl.MainForm.ComboBoxTriggerColumn.SetViewAndGetObservation<TradeColumnTrigger>();
            foreach (TradeColumnTrigger column in TriggerData.QuoteColumnTriggerMap.Values)
            {
                _triggerColumnCollection.Add(column);
            }

            RecoverFile = string.Empty;
        }

        private TriggerController() : this(null)
        { }

        private void SaveData()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                string path = Path.Combine(_appCtrl.Config.TriggerFolder.FullName, $"{start.ToString(_appCtrl.Settings.TriggerFileSaveFormat)}.csv");
                _appCtrl.LogTrace(start, path, UniqueName);

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(TriggerData.CSVColumnNames);

                    foreach (TriggerData data in _dataMap.Values)
                    {
                        try
                        {
                            sw.WriteLine(data.ToCSVString());
                        }
                        catch (Exception ex)
                        {
                            _appCtrl.LogException(start, ex, ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public (LogLevel, string) Restart(in string primary, [CallerMemberName] in string memberName = "")
        {
            TriggerData data = this[primary];

            if (data == null)
            {
                return (LogLevel.Error, $"重啟觸價({primary})失敗");
            }

            lock (data.SyncRoot)
            {
                if (!_appCtrl.Settings.SendRealOrder && _appCtrl.Settings.StrategyFromOpenInterest && (!string.IsNullOrWhiteSpace(data.StrategyOpenOR) || !string.IsNullOrWhiteSpace(data.StrategyOpenAND)))
                {
                    data.Comment = $"從庫存未平倉啟動策略(StrategyFromOpenInterest)";

                    return (LogLevel.Warn, $"從庫存未平倉啟動策略(StrategyFromOpenInterest)");
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Waiting || data.StatusEnum == TriggerStatus.Enum.Monitoring)
                {
                    return (LogLevel.Trace, string.Empty);
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Cancelled && data.StartTime.HasValue && data.StartTime.Value > DateTime.Now)
                { }
                else
                {
                    decimal columnValue = data.GetColumnValue(data.Quote1);
                    decimal targetValue = data.GetTargetValue();
                    bool? matched = data.IsMatchedRule(columnValue, targetValue);

                    if (matched.HasValue && matched.Value)
                    {
                        data.Comment = $"觸價條件({columnValue:0.00####} {data.Rule} {targetValue:0.00####})已滿足，重啟觸價失敗";

                        return (LogLevel.Warn, $"觸價條件({columnValue:0.00####} {data.Rule} {targetValue:0.00####})已滿足，重啟觸價({primary})失敗");
                    }
                    else if (!matched.HasValue)
                    {
                        data.Comment = $"重啟觸價失敗";

                        return (LogLevel.Error, $"重啟觸價({primary})失敗");
                    }
                }

                for (int i = Count - 1; i >= 0; --i)
                {
                    TriggerData other = this[i];

                    if (other.StatusEnum == TriggerStatus.Enum.Executed)
                    {
                        continue;
                    }

                    foreach (string pk in other.Start.SplitWithoutWhiteSpace(','))
                    {
                        if (pk == data.PrimaryKey)
                        {
                            data.Comment = $"等{other.PrimaryKey}觸價後再啟動";

                            return (LogLevel.Warn, $"等{other.PrimaryKey}觸價後再啟動");
                        }
                    }
                }

                data.StatusEnum = TriggerStatus.Enum.Waiting;
                data.Comment = $"重啟";
                data.Updater = memberName;
                data.UpdateTime = DateTime.Now;

                //strategyAND的其他觸價條件，已滿足的改為已執行
                foreach (string strategy in data.StrategyOpenAND.SplitWithoutWhiteSpace(','))
                {
                    string pk = $",{strategy},";

                    foreach (TriggerData other in _dataMap.Values)
                    {
                        if (other == data)
                        {
                            continue;
                        }
                        else if (!$",{other.StrategyOpenAND},".Contains(pk))
                        {
                            continue;
                        }
                        else if (other.StatusEnum == TriggerStatus.Enum.Cancelled && other.StartTime.HasValue && other.StartTime.Value > DateTime.Now)
                        {
                            other.StatusEnum = TriggerStatus.Enum.Waiting;
                            other.Comment = $"重啟|{data.ToLog()}";
                            other.Updater = memberName;
                            other.UpdateTime = DateTime.Now;

                            continue;
                        }

                        decimal columnValue = other.GetColumnValue(other.Quote1);
                        decimal targetValue = other.GetTargetValue();
                        bool? matched = other.IsMatchedRule(columnValue, targetValue);

                        if (matched.HasValue && matched.Value)
                        {
                            other.StatusEnum = TriggerStatus.Enum.Executed;
                            other.Comment = $"重啟|{data.ToLog()}";
                            other.Updater = memberName;
                            other.UpdateTime = DateTime.Now;
                        }
                    }
                }
            }

            return (LogLevel.Trace, string.Empty);
        }

        private HashSet<string> OpenStrategy(in TriggerData data, in DateTime start)
        {
            _appCtrl.Strategy.StartNow(data.StrategyOpenOR, data, start);

            HashSet<string> strategyAND = new HashSet<string>();

            foreach (string strategy in data.StrategyOpenAND.SplitWithoutWhiteSpace(','))
            {
                string pk = $",{strategy},";
                bool openStrategy = true;

                foreach (TriggerData other in _dataMap.Values)
                {
                    if (other == data)
                    {
                        continue;
                    }
                    else if (!$",{other.StrategyOpenAND},".Contains(pk))
                    {
                        continue;
                    }
                    else if (other.StatusEnum != TriggerStatus.Enum.Executed)
                    {
                        openStrategy = false;
                        break;
                    }
                }

                if (!openStrategy)
                {
                    continue;
                }

                strategyAND.Add(strategy);
            }

            if (strategyAND.Count > 0)
            {
                _appCtrl.Strategy.StartNow(string.Join(",", strategyAND), data, start);
            }

            return strategyAND;
        }

        private void CancelAfterExecuted(in TriggerData executed, in DateTime start, [CallerMemberName] in string memberName = "")
        {
            foreach (string cancel in executed.Cancel.SplitWithoutWhiteSpace(','))
            {
                TriggerData data = this[cancel];

                if (data == null || data == executed)
                {
                    continue;
                }

                lock (data.SyncRoot)
                {
                    if (data.StatusEnum == TriggerStatus.Enum.Cancelled || data.StatusEnum == TriggerStatus.Enum.Executed)
                    {
                        continue;
                    }

                    data.StatusEnum = TriggerStatus.Enum.Cancelled;
                    data.Comment = executed.ToLog();
                    data.Updater = memberName;
                    data.UpdateTime = DateTime.Now;

                    _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                }
            }
        }

        private void StartAfterExecuted(in TriggerData executed)
        {
            foreach (string primary in executed.Start.SplitWithoutWhiteSpace(','))
            {
                Restart(primary);
            }
        }

        private bool Cancel(in TriggerData data, in string comment, in DateTime start, [CallerMemberName] in string memberName = "")
        {
            try
            {
                if (data.StatusEnum == TriggerStatus.Enum.Cancelled)
                {
                    _appCtrl.LogTrace(start, $"已經取消|{data.ToLog()}", UniqueName);

                    return true;
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    throw new ArgumentException($"已觸發無法取消|{data.ToLog()}");
                }

                data.StatusEnum = TriggerStatus.Enum.Cancelled;
                data.Comment = comment;
                data.Updater = memberName;
                data.UpdateTime = DateTime.Now;

                _appCtrl.LogTrace(start, data.ToLog(), UniqueName);

                return true;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return false;
        }

        public bool Cancel(in string primaryKey, in string comment = "手動取消")
        {
            DateTime start = _appCtrl.StartTrace($"{nameof(primaryKey)}={primaryKey}", UniqueName);

            try
            {
                if (!_dataMap.TryGetValue(primaryKey.Replace(" ", string.Empty), out TriggerData data))
                {
                    throw new ArgumentNullException($"查無此唯一鍵|{primaryKey}");
                }

                lock (data.SyncRoot)
                {
                    if (Cancel(data, comment, start))
                    {
                        Task.Factory.StartNew(() => SaveData());

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return false;
        }

        public void CancelAfterOrderSent(in StrategyData strategy, in DateTime start)
        {
            string pk = $",{strategy.PrimaryKey},";

            for (int i = Count - 1; i >= 0; --i)
            {
                try
                {
                    TriggerData data = this[i];

                    if ($",{data.StrategyOpenOR},".Contains(pk) || $",{data.StrategyOpenAND},".Contains(pk))
                    {
                        CancelAfterExecuted(data, start);

                        if (data.StatusEnum != TriggerStatus.Enum.Cancelled && data.StatusEnum != TriggerStatus.Enum.Executed)
                        {
                            Cancel(data, strategy.ToLog(), start);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }
        }

        private bool UpdateStatus(in TriggerData data, in QuoteData quote, in DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            bool saveData = false;

            lock (data.SyncRoot)
            {
                if (_appCtrl.CAPQuote.Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                else if (quote.Simulate != QuoteData.RealTrade)
                {
                    return saveData;
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Cancelled || data.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    return saveData;
                }
                else if (data.StatusEnum != TriggerStatus.Enum.Executed && data.EndTime.HasValue && data.EndTime.Value <= DateTime.Now)
                {
                    data.StatusEnum = TriggerStatus.Enum.Cancelled;
                    data.Comment = "觸價逾時，監控取消";
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
                else if (data.StatusEnum != TriggerStatus.Enum.Executed && data.StartTime.HasValue && data.StartTime.Value >= _appCtrl.CAPQuote.MarketCloseTime)
                {
                    data.StatusEnum = TriggerStatus.Enum.Cancelled;
                    data.Comment = "不同盤別，暫停監控";
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Waiting && (!data.StartTime.HasValue || data.StartTime.Value <= DateTime.Now))
                {
                    string des = data.StatusDes;
                    data.StatusEnum = TriggerStatus.Enum.Monitoring;
                    _appCtrl.LogTrace(start, $"{data.ToLog()}|{des} -> {data.StatusDes}", UniqueName);
                    saveData = true;
                }
                else if (data.StatusEnum == TriggerStatus.Enum.Monitoring)
                { }
                else
                {
                    return saveData;
                }

                data.ColumnValue = data.GetColumnValue(quote);
                data.TargetValue = data.GetTargetValue();
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                bool? matched = data.IsMatchedRule(data.ColumnValue, data.TargetValue);

                if (matched.HasValue && matched.Value)
                {
                    data.StatusEnum = TriggerStatus.Enum.Executed;
                    _appCtrl.LogTrace(start, $"{data.ToLog()}|{data.ColumnValue} {data.Rule} {data.TargetValue}", UniqueName);

                    saveData = true;
                    OpenStrategy(data, start);
                    //TODO: CloseStrategy(data, start);

                    //if (string.IsNullOrWhiteSpace(data.StrategyOpenAND) || strategyAND.Count > 0)
                    //{
                    //    CancelAfterExecuted(data, start);
                    //}

                    StartAfterExecuted(data);
                }
                else if (!matched.HasValue)
                {
                    data.StatusEnum = TriggerStatus.Enum.Cancelled;
                    data.Comment = $"條件({data.Rule})錯誤，必須是大於小於等於";
                    _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
            }

            return saveData;
        }

        /// <summary>
        /// Run in background.
        /// </summary>
        /// <param name="start"></param>
        public void UpdateStatus(DateTime start)
        {
            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out TriggerData data);

                TriggerData toRemove = null;

                if (!_dataMap.TryGetValue(data.PrimaryKey, out TriggerData _old))
                {
                    _appCtrl.LogTrace(start, $"新增設定|{data.ToLog()}", UniqueName);
                }
                else if (_old.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    _appCtrl.LogWarn(start, $"舊設定已觸發，將新增設定|{data.ToLog()}", UniqueName);
                    _dataMap.Remove(data.PrimaryKey);
                }
                else
                {
                    _appCtrl.LogWarn(start, $"舊設定未觸發，將進行重置|{data.ToLog()}", UniqueName);
                    _dataMap.Remove(data.PrimaryKey);
                    toRemove = _old;
                }

                _dataMap.Add(data.PrimaryKey, data);

                List<TriggerData> list = _dataMap.Values.ToList();
                int index = list.IndexOf(data);

                if (index + 1 < list.Count)
                {
                    TriggerData next = list[index + 1];
                    index = _dataCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    try
                    {
                        _dataCollection.Insert(index, data);

                        if (toRemove != null)
                        {
                            _dataCollection.Remove(toRemove);
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                });

                if (_waitToAdd.Count <= 0)
                {
                    for (int i = Count - 1; i >= 0; --i)
                    {
                        data = this[i];

                        foreach (string pk in data.Start.SplitWithoutWhiteSpace(','))
                        {
                            if (_dataMap.TryGetValue(pk, out TriggerData td) && td.StatusEnum != TriggerStatus.Enum.Executed && td.StatusEnum != TriggerStatus.Enum.Cancelled)
                            {
                                lock (td.SyncRoot)
                                {
                                    Cancel(td, $"等{data.PrimaryKey}觸價後再啟動", start);
                                }
                            }
                        }
                    }

                    SaveData();
                }
            }

            bool saveData = false;

            for (int i = Count - 1; i >= 0; --i)
            {
                try
                {
                    TriggerData data = this[i];

                    if (UpdateStatus(data, data.Quote1, start))
                    {
                        saveData = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }

            if (saveData)
            {
                SaveData();
            }
        }

        private string ToHHmmss(string time)
        {
            time = time.Replace(":", string.Empty);

            if (time.Length == 1)
            {
                time = time.PadLeft(2, '0');
            }
            else if (time.Length == 3)
            {
                time = time.PadLeft(4, '0');
            }
            else if (time.Length == 5)
            {
                time = time.PadLeft(6, '0');
            }

            return time;
        }

        private (bool, DateTime?, DateTime?) TimeParse(in QuoteData quote, in DateTime start, params string[] times)
        {
            try
            {
                DateTime? startTime = null;
                DateTime? endTime = null;

                if (times.Length > 0 && !string.IsNullOrWhiteSpace(times[0]))
                {
                    times[0] = ToHHmmss(times[0]);

                    if (DateTime.TryParseExact(times[0], _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime _s))
                    {
                        startTime = _s;
                    }
                    else
                    {
                        _appCtrl.LogError(start, $"開始時間錯誤，無法解析({times[0]})", UniqueName);

                        return (false, null, null);
                    }
                }

                if (times.Length > 1 && !string.IsNullOrWhiteSpace(times[1]))
                {
                    times[1] = ToHHmmss(times[1]);

                    if (DateTime.TryParseExact(times[1], _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime _e))
                    {
                        endTime = _e;
                    }
                    else
                    {
                        _appCtrl.LogError(start, $"結束時間錯誤，無法解析({times[1]})", UniqueName);

                        return (false, null, null);
                    }
                }

                if (quote != null)
                {
                    if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Option)
                    {
                        _appCtrl.LogError(start, $"非期貨選擇權，結束時間({times[1]})不可小於開始時間({times[0]})", UniqueName);

                        return (false, null, null);
                    }
                    else if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && endTime.Value.Hour >= _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].Hour)
                    {
                        _appCtrl.LogError(start, $"結束時間({times[1]})不可大於等於凌晨5點", UniqueName);

                        return (false, null, null);
                    }

                    if (startTime.HasValue && startTime.Value < DateTime.Now && startTime.Value.Hour < _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].Hour && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option))
                    {
                        startTime = startTime.Value.AddDays(1);
                        _appCtrl.LogTrace(start, $"期貨選擇權，開始時間跨日，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}", UniqueName);
                    }

                    if (endTime.HasValue && endTime.Value < DateTime.Now && endTime.Value.Hour < _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].Hour && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option))
                    {
                        endTime = endTime.Value.AddDays(1);
                        _appCtrl.LogTrace(start, $"期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}", UniqueName);
                    }

                    if (!_appCtrl.CAPQuote.IsAMMarket && DateTime.Now.Hour < _appCtrl.CAPQuote.MarketCloseTime.Hour && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option))
                    {
                        if (startTime.HasValue && startTime.Value.Hour >= _appCtrl.CAPQuote.MarketStartTime.Hour)
                        {
                            startTime = startTime.Value.AddDays(-1);
                            _appCtrl.LogTrace(start, $"凌晨啟動，開始時間調整，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}", UniqueName);
                        }

                        if (endTime.HasValue && endTime.Value.Hour >= _appCtrl.CAPQuote.MarketStartTime.Hour)
                        {
                            endTime = endTime.Value.AddDays(-1);
                            _appCtrl.LogTrace(start, $"凌晨啟動，結束時間調整，{times[0]} -> {endTime.Value:MM/dd HH:mm:ss}", UniqueName);
                        }
                    }
                }

                return (true, startTime, endTime);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            return (false, null, null);
        }

        public void AddRule(in TriggerData data, in string timeDuration)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}", UniqueName);

            try
            {
                if (_appCtrl.Config.TriggerFolder == null)
                {
                    throw new ArgumentNullException($"未設定觸價資料夾(Settings.TriggerFolderPath)，無法建立觸價資料|{data.ToLog()}");
                }

                data.Trim();

                if (string.IsNullOrWhiteSpace(data.PrimaryKey))
                {
                    throw new ArgumentException($"未設定唯一鍵|{data.ToLog()}");
                }
                else if (string.IsNullOrWhiteSpace(data.Rule))
                {
                    throw new ArgumentException($"string.IsNullOrWhiteSpace(data.Rule)|{data.ToLog()}");
                }
                else if (string.IsNullOrWhiteSpace(data.Symbol1))
                {
                    throw new ArgumentException($"string.IsNullOrWhiteSpace(data.Symbol1)|{data.ToLog()}");
                }

                data.Quote1 = _appCtrl.CAPQuote[data.Symbol1];

                if (!string.IsNullOrWhiteSpace(data.Symbol2))
                {
                    if (data.Symbol2 == data.Symbol1)
                    {
                        throw new ArgumentException($"data.Symbol2({data.Symbol2}) == data.Symbol1({data.Symbol1})|{data.ToLog()}");
                    }

                    data.Quote2 = _appCtrl.CAPQuote[data.Symbol2];
                }

                string rule = data.Rule;
                string bodyValue = string.Empty;

                if (rule.StartsWith(TriggerData.IsGreaterThanOrEqualTo))
                {
                    bodyValue = rule.Substring(TriggerData.IsGreaterThanOrEqualTo.Length);
                    rule = TriggerData.IsGreaterThanOrEqualTo;
                }
                else if (rule.StartsWith(TriggerData.IsGreaterThan))
                {
                    bodyValue = rule.Substring(TriggerData.IsGreaterThan.Length);
                    rule = TriggerData.IsGreaterThanOrEqualTo;
                }
                else if (rule.StartsWith(TriggerData.IsEqualTo))
                {
                    bodyValue = rule.Substring(TriggerData.IsEqualTo.Length);
                    rule = TriggerData.IsEqualTo;
                }
                else if (rule.StartsWith(TriggerData.IsLessThanOrEqualTo))
                {
                    bodyValue = rule.Substring(TriggerData.IsLessThanOrEqualTo.Length);
                    rule = TriggerData.IsLessThanOrEqualTo;
                }
                else if (rule.StartsWith(TriggerData.IsLessThan))
                {
                    bodyValue = rule.Substring(TriggerData.IsLessThan.Length);
                    rule = TriggerData.IsLessThanOrEqualTo;
                }
                else
                {
                    throw new ArgumentException($"條件({rule})錯誤，開頭必須是大於小於等於|{data.ToLog()}");
                }

                if (!string.IsNullOrWhiteSpace(bodyValue))
                {
                    data.Rule = rule;

                    if (bodyValue.Length >= 2 && char.IsLetter(bodyValue[0]) && bodyValue[1] == '2')
                    {
                        if (decimal.TryParse(bodyValue.Substring(2), out decimal offset) && offset >= 0)
                        {
                            data.Symbol2Setting = $"+{offset:0.00####}";
                        }
                        else if (offset < 0)
                        {
                            data.Symbol2Setting = $"{offset:0.00####}";
                        }
                        else
                        {
                            data.Symbol2Setting = "+0.00";
                        }
                    }
                    else if (decimal.TryParse(bodyValue, out decimal value))
                    {
                        data.TargetValue = value;
                    }
                    else
                    {
                        throw new ArgumentException($"條件錯誤，無法解析({bodyValue})|{data.ToLog()}");
                    }
                }

                if (data.TargetValue == 0 && data.Quote2 == null)
                {
                    throw new ArgumentException($"條件錯誤，目標值(TargetValue)不可為0|{data.ToLog()}");
                }

                if (!string.IsNullOrWhiteSpace(timeDuration))
                {
                    (bool, DateTime?, DateTime?) parseResult = TimeParse(data.Quote1, start, timeDuration.Split('~'));

                    if (!parseResult.Item1)
                    {
                        return;
                    }

                    data.StartTime = parseResult.Item2;
                    data.EndTime = parseResult.Item3;
                }

                _waitToAdd.Enqueue(data);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void RecoverSetting(FileInfo file = null, [CallerMemberName] in string memberName = "")
        {
            if (_dataMap.Count > 0)
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace($"{file?.FullName}", UniqueName);

            try
            {
                if (file == null)
                {
                    if (_appCtrl.Config.TriggerFolder == null)
                    {
                        return;
                    }
                    _appCtrl.Config.TriggerFolder.Refresh();

                    string loadFile = _appCtrl.Settings.TriggerFileLoadFormat;

                    if (loadFile.Contains(AppSettings.Keyword_Holiday))
                    {
                        loadFile = loadFile.Replace(AppSettings.Keyword_Holiday, _appCtrl.Config.IsHoliday(start.AddDays(1)).ToString());
                    }

                    if (loadFile.Contains(AppSettings.Keyword_DayNight))
                    {
                        loadFile = loadFile.Replace(AppSettings.Keyword_DayNight, _appCtrl.CAPQuote.IsAMMarket ? $"{Market.EDayNight.AM}" : $"{Market.EDayNight.PM}");
                    }

                    if (loadFile.Contains(AppSettings.Keyword_DayOfWeek))
                    {
                        int tradeDate = _appCtrl.CAPQuote.DataCollection.Max(x => x.TradeDateRaw);
                        DayOfWeek dow = DateTime.ParseExact(tradeDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).DayOfWeek;

                        loadFile = loadFile.Replace(AppSettings.Keyword_DayOfWeek, dow.ToString());
                    }

                    file = _appCtrl.Config.TriggerFolder.GetFiles(loadFile).LastOrDefault();

                    _appCtrl.StartTrace($"{file?.FullName}", UniqueName);
                }

                if (file == null)
                {
                    return;
                }

                RecoverFile = Path.Combine(file.Directory.Name, file.Name);

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (TriggerData data in TriggerData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                {
                    try
                    {
                        data.Trim(memberName);

                        if (string.IsNullOrWhiteSpace(data.PrimaryKey))
                        {
                            continue;
                        }

                        data.StatusEnum = TriggerStatus.Enum.Waiting;
                        data.Quote1 = _appCtrl.CAPQuote[data.Symbol1];
                        data.Quote2 = _appCtrl.CAPQuote[data.Symbol2];
                        data.ColumnValue = 0;
                        data.Comment = string.Empty;

                        string st = data.StartTime.HasValue ? data.StartTime.Value.ToString("HHmmss") : string.Empty;
                        string et = data.EndTime.HasValue ? data.EndTime.Value.ToString("HHmmss") : string.Empty;
                        (bool, DateTime?, DateTime?) parseResult = TimeParse(data.Quote1, start, st, et);

                        if (!parseResult.Item1)
                        {
                            continue;
                        }

                        data.StartTime = parseResult.Item2;
                        data.EndTime = parseResult.Item3;

                        if (_appCtrl.Settings.TriggerReadAndCancel.SplitWithoutWhiteSpace(',').FirstOrDefault(x => data.PrimaryKey.StartsWith(x)) != null)
                        {
                            data.StatusEnum = TriggerStatus.Enum.Cancelled;
                            data.Comment = "關鍵字取消";
                        }
                        else if (!_appCtrl.Settings.SendRealOrder && _appCtrl.Settings.StrategyFromOpenInterest && (!string.IsNullOrWhiteSpace(data.StrategyOpenOR) || !string.IsNullOrWhiteSpace(data.StrategyOpenAND)))
                        {
                            data.StatusEnum = TriggerStatus.Enum.Cancelled;
                            data.Comment = "從庫存未平倉啟動策略(StrategyFromOpenInterest)";
                        }
                        else if ((!data.StartTime.HasValue || data.StartTime.Value <= DateTime.Now) && !_appCtrl.CAPQuote.LoadedOnTime)
                        {
                            data.StatusEnum = TriggerStatus.Enum.Cancelled;
                            data.Comment = "沒有在開盤前執行登入動作，不執行此監控";
                            _appCtrl.LogError(start, data.ToLog(), UniqueName);
                        }

                        if (decimal.TryParse(data.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        AddRule(data, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                if (Count >= nextPK)
                {
                    nextPK = Count + 1;
                }

                if (!_dataMap.ContainsKey($"{nextPK}"))
                {
                    _appCtrl.MainForm.InvokeAsync(delegate { _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{nextPK}"; });
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }
    }
}
