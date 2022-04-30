using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private readonly DateTime _closeTime;

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly ConcurrentQueue<string> _waitToCancel;
        private readonly ConcurrentQueue<TriggerData> _waitToAdd;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        public int Count => _triggerMap.Count;
        public TriggerData this[string key] => _triggerMap.TryGetValue(key, out TriggerData data)? data :null;

        private readonly ConcurrentDictionary<string, TriggerData> _executedMap;

        public TriggerController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(TriggerController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _closeTime = _appCtrl.Capital.IsAMMarket ? _appCtrl.Settings.MarketClose[(int)Market.EDayNight.AM] : _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].AddDays(1);

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = _appCtrl.MainForm.ComboBoxTriggerCancel.SetAndGetItemsSource(TriggerCancel.Description);
            _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex = (int)TriggerCancel.Enum.SameSymbolSameColumn;

            _waitToCancel = new ConcurrentQueue<string>();
            _waitToAdd = new ConcurrentQueue<TriggerData>();

            _triggerMap = new SortedDictionary<string, TriggerData>();
            _appCtrl.MainForm.DataGridTriggerRule.SetHeadersByBindings(TriggerData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _triggerCollection = _appCtrl.MainForm.DataGridTriggerRule.SetAndGetItemsSource<TriggerData>();

            _executedMap = new ConcurrentDictionary<string, TriggerData>();
        }

        private TriggerController() : this(null)
        { }

        private void SaveData(ICollection<TriggerData> triggers)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (triggers == null)
                {
                    triggers = _triggerCollection.ToArray();
                }

                string path = Path.Combine(_appCtrl.Config.TriggerFolder.FullName, string.Format("{0}.csv", DateTime.Now.ToString(_appCtrl.Settings.TriggerFileFormat)));
                _appCtrl.LogTrace(start, path, UniqueName);

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(TriggerData.CSVColumnNames);

                    foreach (TriggerData trigger in triggers)
                    {
                        try
                        {
                            sw.WriteLine(trigger.ToCSVString());
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

        private void SaveDataAsync()
        {
            Task.Factory.StartNew(() => SaveData(null));
        }

        private void StartStrategy(TriggerData trigger, string primary, DateTime start)
        {
            try
            {
                StrategyData strategy = _appCtrl.Strategy[primary];

                if (strategy != null && strategy.StatusEnum == StrategyStatus.Enum.Waiting)
                {
                    _appCtrl.Strategy.StartNow(strategy.PrimaryKey);
                    return;
                }

                _appCtrl.LogError(start, $"執行策略({primary})失敗|{trigger.ToLog()}", UniqueName);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"執行策略({primary})失敗|{trigger.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void StartStrategy(TriggerData trigger, DateTime start)
        {
            if (!string.IsNullOrWhiteSpace(trigger.StrategyOR))
            {
                HashSet<string> primariesOR = new HashSet<string>(trigger.StrategyOR.Split(','));

                foreach (string primary in primariesOR)
                {
                    StartStrategy(trigger, primary, start);
                }
            }

            if (!string.IsNullOrWhiteSpace(trigger.StrategyAND))
            {
                HashSet<string> primariesAND = new HashSet<string>(trigger.StrategyAND.Split(','));

                foreach (string primary in primariesAND)
                {
                    string pk = $",{primary},";
                    bool doStrategy = true;

                    foreach (TriggerData td in _triggerMap.Values)
                    {
                        if (td == trigger)
                        {
                            continue;
                        }
                        else if (string.Format(",{0},", td.StrategyAND).Contains(pk) && (td.StatusEnum == TriggerStatus.Enum.Waiting || td.StatusEnum == TriggerStatus.Enum.Monitoring))
                        {
                            doStrategy = false;
                            break;
                        }
                    }

                    if (!doStrategy)
                    {
                        _appCtrl.LogError(start, $"執行策略({primary})失敗|{trigger.ToLog()}", UniqueName);
                        continue;
                    }

                    StartStrategy(trigger, primary, start);
                }
            }
        }

        private bool UpdateStatus(TriggerData trigger, QuoteData quote, DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            bool saveData = false;

            lock (trigger.SyncRoot)
            {
                if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                else if (quote.Simulate != QuoteData.RealTrade)
                {
                    return saveData;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    return saveData;
                }
                else if (trigger.StatusEnum != TriggerStatus.Enum.Executed && trigger.EndTime.HasValue && trigger.EndTime.Value <= DateTime.Now)
                {
                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = "觸價逾時，監控取消";
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
                else if (trigger.StatusEnum != TriggerStatus.Enum.Executed && trigger.StartTime.HasValue && trigger.StartTime.Value > _closeTime)
                {
                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = "不同盤別，暫停監控";
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Waiting && (!trigger.StartTime.HasValue || trigger.StartTime.Value <= DateTime.Now))
                {
                    string des = trigger.StatusDes;
                    trigger.StatusEnum = TriggerStatus.Enum.Monitoring;
                    _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{des} -> {trigger.StatusDes}", UniqueName);
                    saveData = true;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Monitoring)
                { }
                else
                {
                    return saveData;
                }

                object valueObj = trigger.Column.Property.GetValue(quote);

                if (trigger.Column.Property.PropertyType == typeof(DateTime))
                {
                    trigger.ColumnValue = decimal.Parse(((DateTime)valueObj).ToString(trigger.Column.Attribute.TriggerFormat));
                }
                else if (trigger.Column.Property.PropertyType == typeof(string))
                {
                    trigger.ColumnValue = decimal.Parse((string)valueObj);
                }
                else
                {
                    trigger.ColumnValue = (decimal)valueObj;
                }

                trigger.Updater = methodName;
                trigger.UpdateTime = DateTime.Now;

                if (trigger.Rule == TriggerData.IsGreaterThanOrEqualTo)
                {
                    if (trigger.ColumnValue >= trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);

                        saveData = true;
                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);
                        StartStrategy(trigger, start);
                    }
                }
                else if (trigger.Rule == TriggerData.IsLessThanOrEqualTo)
                {
                    if (trigger.ColumnValue <= trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);

                        saveData = true;
                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);
                        StartStrategy(trigger, start);
                    }
                }
                else if (trigger.Rule == TriggerData.IsEqualTo)
                {
                    if (trigger.ColumnValue == trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);

                        saveData = true;
                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);
                        StartStrategy(trigger, start);
                    }
                }
                else
                {
                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = $"條件({trigger.Rule})錯誤，必須是大於小於等於";
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                    saveData = true;
                    return saveData;
                }
            }

            return saveData;
        }

        private void CancelSameSymbolSameColumn(TriggerData executed, DateTime start)
        {
            const string methodName = nameof(CancelSameSymbolSameColumn);

            foreach (TriggerData trigger in _triggerMap.Values)
            {
                lock (trigger.SyncRoot)
                {
                    if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed || trigger.Symbol != executed.Symbol || trigger.ColumnProperty != executed.ColumnProperty)
                    {
                        continue;
                    }

                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = executed.ToLog();
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelSameSymbolAllColumns(TriggerData executed, DateTime start)
        {
            const string methodName = nameof(CancelSameSymbolAllColumns);

            foreach (TriggerData trigger in _triggerMap.Values)
            {
                lock (trigger.SyncRoot)
                {
                    if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed || trigger.Symbol != executed.Symbol)
                    {
                        continue;
                    }

                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = executed.ToLog();
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelAllSymbolsSameColumn(TriggerData executed, DateTime start)
        {
            const string methodName = nameof(CancelAllSymbolsSameColumn);

            foreach (TriggerData trigger in _triggerMap.Values)
            {
                lock (trigger.SyncRoot)
                {
                    if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed || trigger.ColumnProperty != executed.ColumnProperty)
                    {
                        continue;
                    }

                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = executed.ToLog();
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelAllSymbolsAllColumns(TriggerData executed, DateTime start)
        {
            const string methodName = nameof(CancelAllSymbolsAllColumns);

            foreach (TriggerData trigger in _triggerMap.Values)
            {
                lock (trigger.SyncRoot)
                {
                    if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed)
                    {
                        continue;
                    }

                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = executed.ToLog();
                    trigger.Updater = methodName;
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        /// <summary>
        /// Run in background.
        /// </summary>
        /// <param name="start"></param>
        public void UpdateStatus(DateTime start)
        {
            while (_waitToCancel.Count > 0)
            {
                _waitToCancel.TryDequeue(out string primaryKey);

                if (_triggerMap.TryGetValue(primaryKey, out TriggerData trigger))
                {
                    if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled)
                    {
                        _appCtrl.LogError(start, trigger.ToLog(), UniqueName);
                    }
                    else if (trigger.StatusEnum == TriggerStatus.Enum.Executed)
                    {
                        _appCtrl.LogError(start, $"已觸發無法取消|{trigger.ToLog()}", UniqueName);
                    }
                    else
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                        trigger.Comment = $"手動取消";
                        _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                    }

                    if (_waitToCancel.Count <= 0)
                    {
                        SaveDataAsync();
                    }
                }
                else
                {
                    _appCtrl.LogError(start, $"查無此唯一鍵|{primaryKey}", UniqueName);
                }
            }

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out TriggerData trigger);

                TriggerData toRemove = null;

                if (!_triggerMap.TryGetValue(trigger.PrimaryKey, out TriggerData _old))
                {
                    _appCtrl.LogTrace(start, $"新增設定|{trigger.ToLog()}", UniqueName);
                }
                else if (_old.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    _appCtrl.LogWarn(start, $"舊設定已觸發，將新增設定|{trigger.ToLog()}", UniqueName);
                    _triggerMap.Remove(trigger.PrimaryKey);
                }
                else
                {
                    _appCtrl.LogWarn(start, $"舊設定未觸發，將進行重置|{trigger.ToLog()}", UniqueName);
                    _triggerMap.Remove(trigger.PrimaryKey);
                    toRemove = _old;
                }

                _triggerMap.Add(trigger.PrimaryKey, trigger);

                List<TriggerData> list = _triggerMap.Values.ToList();
                int index = list.IndexOf(trigger);

                if (index + 1 < list.Count)
                {
                    TriggerData next = list[index + 1];
                    index = _triggerCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    try
                    {
                        _triggerCollection.Insert(index, trigger);

                        if (toRemove != null)
                        {
                            _triggerCollection.Remove(toRemove);
                        }

                        if (_waitToAdd.Count <= 0)
                        {
                            SaveDataAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                });

                if (_waitToAdd.Count <= 0)
                {
                    return;
                }
            }

            bool saveData = false;

            _executedMap.Clear();

            foreach (KeyValuePair<string, TriggerData> pair in _triggerMap)
            //Parallel.ForEach(_triggerMap, pair =>
            {
                try
                {
                    if (UpdateStatus(pair.Value, pair.Value.Quote, start))
                    {
                        saveData = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }

            foreach (TriggerData executed in _executedMap.Values)
            {
                try
                {
                    switch (executed.CancelEnum)
                    {
                        case TriggerCancel.Enum.SameSymbolSameColumn:
                            CancelSameSymbolSameColumn(executed, start);
                            break;
                        case TriggerCancel.Enum.SameSymbolAllColumns:
                            CancelSameSymbolAllColumns(executed, start);
                            break;
                        case TriggerCancel.Enum.AllSymbolsSameColumn:
                            CancelAllSymbolsSameColumn(executed, start);
                            break;
                        case TriggerCancel.Enum.AllSymbolsAllColumns:
                            CancelAllSymbolsAllColumns(executed, start);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }

            if (saveData)
            {
                SaveData(_triggerCollection);
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

        private (bool, DateTime?, DateTime?) TimeParse(QuoteData quote, DateTime start, params string[] times)
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

                if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Option)
                {
                    _appCtrl.LogError(start, $"非期貨選擇權，結束時間({times[1]})不可小於開始時間({times[0]})", UniqueName);
                    return (false, null, null);
                }
                else if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && endTime.Value.Hour >= 5)
                {
                    _appCtrl.LogError(start, $"結束時間({times[1]})不可大於等於凌晨5點", UniqueName);
                    return (false, null, null);
                }

                if (startTime.HasValue && startTime.Value < DateTime.Now && startTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option))
                {
                    startTime = startTime.Value.AddDays(1);
                    _appCtrl.LogTrace(start, $"期貨選擇權，開始時間跨日，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}", UniqueName);
                }

                if (endTime.HasValue && endTime.Value < DateTime.Now && endTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option))
                {
                    endTime = endTime.Value.AddDays(1);
                    _appCtrl.LogTrace(start, $"期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}", UniqueName);
                }

                return (true, startTime, endTime);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            return (false, null, null);
        }

        public void Cancel(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}", UniqueName);

            try
            {
                _waitToCancel.Enqueue(primaryKey.Replace(" ", string.Empty));
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

        public void AddRule(TriggerData trigger, string timeDuration)
        {
            const string methodName = nameof(AddRule);

            DateTime start = _appCtrl.StartTrace($"{trigger?.ToLog()}", UniqueName);

            try
            {
                if (_appCtrl.Config.TriggerFolder == null)
                {
                    throw new ArgumentNullException($"未設定觸價資料夾(Settings.TriggerFolderPath)，無法建立觸價資料|{trigger.ToLog()}");
                }

                trigger = trigger.Trim();

                if (string.IsNullOrWhiteSpace(trigger.PrimaryKey))
                {
                    throw new ArgumentException($"未設定唯一鍵|{trigger.ToLog()}");
                }
                else if (string.IsNullOrWhiteSpace(trigger.Rule))
                {
                    throw new ArgumentNullException($"string.IsNullOrWhiteSpace(trigger.Rule)|{trigger.ToLog()}");
                }
                else if (trigger.Quote == null)
                {
                    trigger.Quote = _appCtrl.Capital.GetQuote(trigger.Symbol);
                }

                string rule = trigger.Rule;
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
                    throw new ArgumentException($"條件({rule})錯誤，開頭必須是大於小於等於|{trigger.ToLog()}");
                }

                if (!string.IsNullOrWhiteSpace(bodyValue))
                {
                    if (decimal.TryParse(bodyValue, out decimal value))
                    {
                        trigger.Rule = rule;
                        trigger.TargetValue = value;
                    }
                    else
                    {
                        throw new ArgumentException($"條件錯誤，無法解析({bodyValue})|{trigger.ToLog()}");
                    }
                }

                if (trigger.TargetValue == 0)
                {
                    throw new ArgumentException($"條件錯誤，目標值(TargetValue)不可為0|{trigger.ToLog()}");
                }

                if (!string.IsNullOrWhiteSpace(timeDuration))
                {
                    (bool, DateTime?, DateTime?) parseResult = TimeParse(trigger.Quote, start, timeDuration.Split('~'));

                    if (!parseResult.Item1)
                    {
                        return;
                    }

                    trigger.StartTime = parseResult.Item2;
                    trigger.EndTime = parseResult.Item3;
                }

                trigger.Updater = methodName;
                trigger.UpdateTime = DateTime.Now;

                _waitToAdd.Enqueue(trigger);
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

        public void RecoverSetting(FileInfo file = null)
        {
            const string methodName = nameof(RecoverSetting);

            DateTime start = _appCtrl.StartTrace($"{file?.FullName}", UniqueName);

            try
            {
                if (_triggerMap.Count > 0)
                {
                    return;
                }

                if (file == null)
                {
                    if (_appCtrl.Config.TriggerFolder == null)
                    {
                        return;
                    }

                    _appCtrl.Config.TriggerFolder.Refresh();
                    file = _appCtrl.Config.TriggerFolder.GetFiles("*.csv").LastOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).Length == _appCtrl.Settings.TriggerFileFormat.Length);
                }

                if (file == null)
                {
                    return;
                }

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (TriggerData data in TriggerData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                {
                    try
                    {
                        TriggerData trigger = data.Trim();

                        if (string.IsNullOrWhiteSpace(trigger.PrimaryKey))
                        {
                            continue;
                        }

                        trigger.StatusEnum = TriggerStatus.Enum.Waiting;
                        trigger.Quote = _appCtrl.Capital.GetQuote(trigger.Symbol);
                        trigger.ColumnValue = 0;
                        trigger.Comment = string.Empty;

                        string st = trigger.StartTime.HasValue ? trigger.StartTime.Value.ToString("HHmmss") : string.Empty;
                        string et = trigger.EndTime.HasValue ? trigger.EndTime.Value.ToString("HHmmss") : string.Empty;
                        (bool, DateTime?, DateTime?) parseResult = TimeParse(trigger.Quote, start, st, et);

                        if (!parseResult.Item1)
                        {
                            continue;
                        }

                        trigger.StartTime = parseResult.Item2;
                        trigger.EndTime = parseResult.Item3;

                        if ((!trigger.StartTime.HasValue || trigger.StartTime.Value <= DateTime.Now) && !_appCtrl.Config.StartOnTime)
                        {
                            trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                            trigger.Comment = "程式沒有在正常時間啟動，不執行此監控";
                            _appCtrl.LogError(start, trigger.ToLog(), UniqueName);
                        }

                        trigger.Updater = methodName;
                        trigger.UpdateTime = DateTime.Now;

                        if (decimal.TryParse(trigger.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        AddRule(trigger, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalTrigger * 3);

                if (_triggerCollection.Count >= nextPK)
                {
                    nextPK = _triggerCollection.Count + 1;
                }

                if (!_triggerMap.ContainsKey($"{nextPK}"))
                {
                    _appCtrl.MainForm.InvokeRequired(delegate
                    {
                        _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{nextPK}";
                    });
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
