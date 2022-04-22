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

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly ConcurrentQueue<QuoteData> _waitToReset;
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
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = _appCtrl.MainForm.ComboBoxTriggerCancel.SetAndGetItemsSource(TriggerCancel.Description);
            _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex = (int)TriggerCancel.Enum.SameSymbolSameColumn;

            _waitToReset = new ConcurrentQueue<QuoteData>();
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

        private bool UpdateStatus(TriggerData trigger, QuoteData quote, DateTime start)
        {
            bool saveData = false;

            lock (trigger.SyncRoot)
            {
                if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                else if (quote == null)
                {
                    return saveData;
                }
                else if (quote.Simulate.IsSimulating())
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
                    trigger.Updater = nameof(UpdateStatus);
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

                trigger.Updater = nameof(UpdateStatus);
                trigger.UpdateTime = DateTime.Now;

                if (trigger.Rule == Definition.IsGreaterThanOrEqualTo)
                {
                    if (trigger.ColumnValue >= trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);
                        saveData = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //TODO
                    }
                }
                else if (trigger.Rule == Definition.IsLessThanOrEqualTo)
                {
                    if (trigger.ColumnValue <= trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);
                        saveData = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsEqualTo)
                {
                    if (trigger.ColumnValue == trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace(start, $"{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}", UniqueName);
                        saveData = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //
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
                    trigger.Updater = nameof(CancelSameSymbolSameColumn);
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelSameSymbolAllColumns(TriggerData executed, DateTime start)
        {
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
                    trigger.Updater = nameof(CancelSameSymbolAllColumns);
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelAllSymbolsSameColumn(TriggerData executed, DateTime start)
        {
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
                    trigger.Updater = nameof(CancelAllSymbolsSameColumn);
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);
                }
            }
        }

        private void CancelAllSymbolsAllColumns(TriggerData executed, DateTime start)
        {
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
                    trigger.Updater = nameof(CancelAllSymbolsAllColumns);
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
            while (_waitToReset.Count > 0)
            {
                _waitToReset.TryDequeue(out QuoteData quote);

                foreach (TriggerData trigger in _triggerMap.Values)
                {
                    if (trigger.Symbol == quote.Symbol)
                    {
                        trigger.Quote = quote;
                    }
                }
            }

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
                    if (trigger.StatusEnum != TriggerStatus.Enum.Cancelled)
                    {
                        _appCtrl.LogTrace(start, $"新增設定|{trigger.ToLog()}", UniqueName);
                    }
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

        public void ClearQuotes()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                Parallel.ForEach(_triggerMap.Values, trigger =>
                {
                    lock (trigger.SyncRoot)
                    {
                        trigger.Quote = null;
                    }
                });
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

        public void Reset(QuoteData quote)
        {
            try
            {
                _waitToReset.Enqueue(quote);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
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

        private (bool, DateTime?, DateTime?) TimeParse(QuoteData quote, params string[] times)
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
                        _appCtrl.LogError($"開始時間錯誤，無法解析({times[0]})", UniqueName);
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
                        _appCtrl.LogError($"結束時間錯誤，無法解析({times[1]})", UniqueName);
                        return (false, null, null);
                    }
                }

                if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Options)
                {
                    _appCtrl.LogError($"非期貨選擇權，結束時間({times[1]})不可小於開始時間({times[0]})", UniqueName);
                    return (false, null, null);
                }
                else if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && endTime.Value.Hour >= 5)
                {
                    _appCtrl.LogError($"結束時間({times[1]})不可大於等於凌晨5點", UniqueName);
                    return (false, null, null);
                }

                if (startTime.HasValue && startTime.Value < DateTime.Now && startTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options))
                {
                    startTime = startTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"期貨選擇權，開始時間跨日，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}", UniqueName);
                }

                if (endTime.HasValue && endTime.Value < DateTime.Now && endTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options))
                {
                    endTime = endTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}", UniqueName);
                }

                return (true, startTime, endTime);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }

            return (false, null, null);
        }

        public void Cancel(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}", UniqueName);

            try
            {
                primaryKey = primaryKey.Replace(" ", string.Empty);

                if (string.IsNullOrWhiteSpace(primaryKey))
                {
                    return;
                }

                _waitToCancel.Enqueue(primaryKey);
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

        public void AddRule()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (_appCtrl.Config.TriggerFolder == null)
                {
                    _appCtrl.LogError(start, "未設定觸價資料夾(Settings.TriggerFolderPath)，無法建立觸價資料", UniqueName);
                    return;
                }
                else if (_appCtrl.MainForm.ComboBoxTriggerProduct.SelectedIndex < 0)
                {
                    return;
                }
                else if (_appCtrl.MainForm.ComboBoxTriggerColumn.SelectedIndex < 0)
                {
                    return;
                }
                else if (_appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex < 0)
                {
                    return;
                }
                else if (string.IsNullOrWhiteSpace(_appCtrl.MainForm.TextBoxTriggerRuleValue.Text))
                {
                    return;
                }

                string primaryKey = _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text.Replace(" ", string.Empty);

                if (string.IsNullOrWhiteSpace(primaryKey))
                {
                    _appCtrl.LogError(start, "未設定唯一鍵", UniqueName);
                    return;
                }

                QuoteData selectedQuote = (QuoteData)_appCtrl.MainForm.ComboBoxTriggerProduct.SelectedItem;

                string rule = _appCtrl.MainForm.TextBoxTriggerRuleValue.Text.Replace(" ", string.Empty);
                string bodyValue = string.Empty;

                if (rule.StartsWith(Definition.IsGreaterThanOrEqualTo))
                {
                    bodyValue = rule.Substring(Definition.IsGreaterThanOrEqualTo.Length);
                    rule = Definition.IsGreaterThanOrEqualTo;
                }
                else if (rule.StartsWith(Definition.IsGreaterThan))
                {
                    bodyValue = rule.Substring(Definition.IsGreaterThan.Length);
                    rule = Definition.IsGreaterThanOrEqualTo;
                }
                else if (rule.StartsWith(Definition.IsEqualTo))
                {
                    bodyValue = rule.Substring(Definition.IsEqualTo.Length);
                    rule = Definition.IsEqualTo;
                }
                else if (rule.StartsWith(Definition.IsLessThanOrEqualTo))
                {
                    bodyValue = rule.Substring(Definition.IsLessThanOrEqualTo.Length);
                    rule = Definition.IsLessThanOrEqualTo;
                }
                else if (rule.StartsWith(Definition.IsLessThan))
                {
                    bodyValue = rule.Substring(Definition.IsLessThan.Length);
                    rule = Definition.IsLessThanOrEqualTo;
                }
                else
                {
                    _appCtrl.LogError(start, $"條件({rule})錯誤，開頭必須是大於小於等於", UniqueName);
                    return;
                }

                if (decimal.TryParse(bodyValue, out decimal value))
                {
                    _appCtrl.MainForm.TextBoxTriggerRuleValue.Text = $"{rule}{bodyValue}";
                }
                else
                {
                    _appCtrl.LogError(start, $"條件錯誤，無法解析({bodyValue})", UniqueName);
                    return;
                }

                string duration = _appCtrl.MainForm.TextBoxTriggerTimeDuration.Text.Replace(" ", string.Empty);
                (bool, DateTime?, DateTime?) parseResult = TimeParse(selectedQuote, duration.Split('~'));

                if (!parseResult.Item1)
                {
                    return;
                }

                TriggerData trigger = new TriggerData((TradeColumnTrigger)_appCtrl.MainForm.ComboBoxTriggerColumn.SelectedItem)
                {
                    Updater = nameof(AddRule),
                    UpdateTime = DateTime.Now,
                    PrimaryKey = primaryKey,
                    Quote = selectedQuote,
                    Symbol = selectedQuote.Symbol,
                    Rule = rule,
                    TargetValue = value,
                    Cancel = _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex,
                    StrategyOR = _appCtrl.MainForm.TextBoxTriggerStrategyOR.Text.Trim(),
                    StrategyAND = _appCtrl.MainForm.TextBoxTriggerStrategyAND.Text.Trim(),
                    StartTime = parseResult.Item2,
                    EndTime = parseResult.Item3,
                };

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
            DateTime start = _appCtrl.StartTrace();

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

                foreach (TriggerData trigger in TriggerData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(trigger.PrimaryKey))
                        {
                            continue;
                        }

                        QuoteData quote = _appCtrl.Capital.GetQuote(trigger.Symbol);

                        if (quote == null)
                        {
                            continue;
                        }

                        trigger.StatusEnum = TriggerStatus.Enum.Waiting;
                        trigger.Quote = quote;
                        trigger.ColumnValue = 0;
                        trigger.Comment = string.Empty;

                        string startTime = trigger.StartTime.HasValue ? trigger.StartTime.Value.ToString("HHmmss") : string.Empty;
                        string endTime = trigger.EndTime.HasValue ? trigger.EndTime.Value.ToString("HHmmss") : string.Empty;
                        (bool, DateTime?, DateTime?) parseResult = TimeParse(quote, startTime, endTime);

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

                        trigger.Updater = nameof(RecoverSetting);
                        trigger.UpdateTime = DateTime.Now;

                        if (decimal.TryParse(trigger.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        _waitToAdd.Enqueue(trigger);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalTrigger * 2);

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
