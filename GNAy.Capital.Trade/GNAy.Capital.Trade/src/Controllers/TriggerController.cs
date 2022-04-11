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
        private readonly AppController _appCtrl;

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly ConcurrentQueue<QuoteData> _waitToReset;
        private readonly ConcurrentQueue<string> _waitToCancel;
        private readonly ConcurrentQueue<TriggerData> _waitToAdd;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        private readonly ConcurrentDictionary<string, TriggerData> _executedMap;

        public TriggerController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
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

            _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{_triggerMap.Count + 1}";
        }

        private TriggerController() : this(null)
        { }

        private void SaveData(ICollection<TriggerData> triggers)
        {
            try
            {
                if (triggers == null)
                {
                    triggers = _triggerCollection.ToArray();
                }

                string path = Path.Combine(_appCtrl.Config.TriggerFolder.FullName, string.Format("{0}.csv", DateTime.Now.ToString(_appCtrl.Settings.TriggerFileFormat)));
                _appCtrl.LogTrace($"Trigger|{path}");

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
                            _appCtrl.LogException(ex, ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
        }

        private void SaveDataAsync()
        {
            Task.Factory.StartNew(() => SaveData(null));
        }

        private bool UpdateStatus(TriggerData trigger, QuoteData quote)
        {
            bool saveTriggers = false;

            lock (trigger.SyncRoot)
            {
                if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveTriggers;
                }
                else if (quote == null)
                {
                    return saveTriggers;
                }
                else if (quote.Simulate.IsSimulating())
                {
                    return saveTriggers;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Cancelled || trigger.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    return saveTriggers;
                }
                else if (trigger.StatusEnum != TriggerStatus.Enum.Executed && trigger.EndTime.HasValue && trigger.EndTime.Value <= DateTime.Now)
                {
                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = "觸價逾時，監控取消";
                    trigger.Updater = nameof(UpdateStatus);
                    trigger.UpdateTime = DateTime.Now;
                    _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}");
                    saveTriggers = true;
                    return saveTriggers;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Waiting && (!trigger.StartTime.HasValue || trigger.StartTime.Value <= DateTime.Now))
                {
                    string des = trigger.StatusDes;
                    trigger.StatusEnum = TriggerStatus.Enum.Monitoring;
                    _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}|{des} -> {trigger.StatusDes}");
                    saveTriggers = true;
                }
                else if (trigger.StatusEnum == TriggerStatus.Enum.Monitoring)
                { }
                else
                {
                    return saveTriggers;
                }

                object valueObj = trigger.Column.Property.GetValue(quote);

                if (trigger.Column.Property.PropertyType == typeof(DateTime))
                {
                    trigger.ColumnValue = decimal.Parse(((DateTime)valueObj).ToString(trigger.Column.Attribute.ValueFormat));
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
                        _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}");
                        saveTriggers = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsLessThanOrEqualTo)
                {
                    if (trigger.ColumnValue <= trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}");
                        saveTriggers = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsEqualTo)
                {
                    if (trigger.ColumnValue == trigger.TargetValue)
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Executed;
                        _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}");
                        saveTriggers = true;

                        _executedMap.TryAdd(trigger.PrimaryKey, trigger);

                        //
                    }
                }
                else
                {
                    trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                    trigger.Comment = $"條件({trigger.Rule})錯誤，必須是大於小於等於";
                    _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}");
                    saveTriggers = true;
                    return saveTriggers;
                }
            }

            return saveTriggers;
        }

        /// <summary>
        /// Run in background.
        /// </summary>
        public void UpdateStatus()
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
                        _appCtrl.LogError($"Trigger|{trigger.ToLog()}");
                    }
                    else if (trigger.StatusEnum == TriggerStatus.Enum.Executed)
                    {
                        _appCtrl.LogError($"Trigger|{trigger.ToLog()}|已觸發無法取消");
                    }
                    else
                    {
                        trigger.StatusEnum = TriggerStatus.Enum.Cancelled;
                        trigger.Comment = $"手動取消";
                        _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}");
                    }

                    if (_waitToCancel.Count <= 0)
                    {
                        SaveDataAsync();
                    }
                }
                else
                {
                    _appCtrl.LogError($"Trigger|{primaryKey}|查無此唯一鍵");
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
                        _appCtrl.LogTrace($"Trigger|{trigger.ToLog()}|新增設定");
                    }
                }
                else if (_old.StatusEnum == TriggerStatus.Enum.Executed)
                {
                    _appCtrl.LogWarn($"Trigger|{trigger.ToLog()}|舊設定已觸發，將新增設定");
                    _triggerMap.Remove(trigger.PrimaryKey);
                }
                else
                {
                    _appCtrl.LogWarn($"Trigger|{trigger.ToLog()}|舊設定未觸發，將進行重置");
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
                        _appCtrl.LogException(ex, ex.StackTrace);
                    }
                });

                if (_waitToAdd.Count <= 0)
                {
                    return;
                }
            }

            bool saveTriggers = false;

            _executedMap.Clear();

            foreach (KeyValuePair<string, TriggerData> pair in _triggerMap)
            //Parallel.ForEach(_triggerMap, pair =>
            {
                try
                {
                    if (UpdateStatus(pair.Value, pair.Value.Quote))
                    {
                        saveTriggers = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
            }

            if (_executedMap.Count >= 0)
            {
                //TODO
            }

            if (saveTriggers)
            {
                SaveData(_triggerCollection);
            }
        }

        public void ClearQuotes()
        {
            _appCtrl.LogTrace("Trigger|Start");

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
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("Trigger|End");
            }
        }

        public void Reset(QuoteData quote)
        {
            try
            {
                //_appCtrl.LogTrace($"Trigger|Start|Symbol={quote.Symbol}|Page={quote.Page}");

                _waitToReset.Enqueue(quote);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("Trigger|End");
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
                        _appCtrl.LogError($"Trigger|開始時間錯誤，無法解析({times[0]})");
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
                        _appCtrl.LogError($"Trigger|結束時間錯誤，無法解析({times[1]})");
                        return (false, null, null);
                    }
                }

                if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Options)
                {
                    _appCtrl.LogError($"Trigger|非期貨選擇權，結束時間({times[1]})不可小於開始時間({times[0]})");
                    return (false, null, null);
                }
                else if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && endTime.Value.Hour >= 5)
                {
                    _appCtrl.LogError($"Trigger|結束時間({times[1]})不可大於等於凌晨5點");
                    return (false, null, null);
                }

                if (startTime.HasValue && startTime.Value < DateTime.Now && startTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options))
                {
                    startTime = startTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"Trigger|期貨選擇權，開始時間跨日，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}");
                }

                if (endTime.HasValue && endTime.Value < DateTime.Now && endTime.Value.Hour < 5 && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options))
                {
                    endTime = endTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"Trigger|期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}");
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
            _appCtrl.LogTrace($"Trigger|Start|primaryKey={primaryKey}");

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
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("Trigger|End");
            }
        }

        public void AddRule()
        {
            _appCtrl.LogTrace("Trigger|Start");

            try
            {
                if (_appCtrl.Config.TriggerFolder == null)
                {
                    _appCtrl.LogError($"Trigger|未設定觸價資料夾(Settings.TriggerFolderPath)，無法建立觸價資料");
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
                    primaryKey = $"{_triggerMap.Count + 1}";
                }

                QuoteData selectedQuote = _appCtrl.MainForm.ComboBoxTriggerProduct.SelectedItem as QuoteData;

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
                    _appCtrl.LogError($"Trigger|條件({rule})錯誤，開頭必須是大於小於等於");
                    return;
                }

                if (decimal.TryParse(bodyValue, out decimal value))
                {
                    _appCtrl.MainForm.TextBoxTriggerRuleValue.Text = $"{rule}{bodyValue}";
                }
                else
                {
                    _appCtrl.LogError($"Trigger|條件錯誤，無法解析({bodyValue})");
                    return;
                }

                string duration = _appCtrl.MainForm.TextBoxTriggerTimeDuration.Text.Replace(" ", string.Empty);
                (bool, DateTime?, DateTime?) parseResult = TimeParse(selectedQuote, duration.Split('~'));

                if (!parseResult.Item1)
                {
                    return;
                }

                TriggerData trigger = new TriggerData(selectedQuote, _appCtrl.MainForm.ComboBoxTriggerColumn.SelectedItem as TradeColumnTrigger)
                {
                    Creator = nameof(AddRule),
                    Updater = nameof(AddRule),
                    UpdateTime = DateTime.Now,
                    PrimaryKey = primaryKey,
                    Rule = rule,
                    TargetValue = value,
                    CancelIndex = _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex,
                    StrategyOR = _appCtrl.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                    StrategyAND = _appCtrl.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                    StartTime = parseResult.Item2,
                    EndTime = parseResult.Item3,
                };

                if (decimal.TryParse(primaryKey, out decimal _pk))
                {
                    if (_pk < _triggerMap.Count)
                    {
                        _pk = _triggerMap.Count;
                    }

                    _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{_pk + 1}";
                }

                _waitToAdd.Enqueue(trigger);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("Trigger|End");
            }
        }

        public void RecoverSetting(FileInfo file = null)
        {
            _appCtrl.LogTrace("Trigger|Start");

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
                            _appCtrl.LogError($"Trigger|{trigger.ToLog()}");
                        }

                        trigger.Creator = nameof(RecoverSetting);
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
                        _appCtrl.LogException(ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(1 * 1000);
                if (_triggerCollection.Count >= nextPK)
                {
                    nextPK = _triggerCollection.Count + 1;
                }
                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{nextPK}";
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("Trigger|End");
            }
        }
    }
}
