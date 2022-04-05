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
        private static string[] _timeFormats = new string[] { "HHmmss", "HHmm", "HH" };

        public readonly DateTime CreatedTime;
        private readonly AppController _appCtrl;

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly ConcurrentQueue<QuoteData> _waitToReset;
        private readonly ConcurrentQueue<TriggerData> _waitToAdd;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        private readonly ConcurrentDictionary<string, TriggerData> _executedMap;

        public TriggerController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            _appCtrl = appCtrl;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = new ObservableCollection<string>(Definition.TriggerCancelKinds);
            _appCtrl.MainForm.ComboBoxTriggerCancel.ItemsSource = _triggerCancelKinds.GetViewSource();
            _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex = Definition.TriggerCancel0.Item1;

            _waitToReset = new ConcurrentQueue<QuoteData>();
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

                string path = Path.Combine(_appCtrl.Config.TriggerFolder.FullName, $"{DateTime.Now:MMdd_HHmm}.csv");
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

        private bool UpdateStatus(string key, TriggerData trigger, QuoteData quote)
        {
            if (quote == null)
            {
                return false;
            }

            bool saveTriggers = false;

            DateTime now = DateTime.Now;

            lock (trigger.SyncRoot)
            {
                if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveTriggers;
                }
                else if (trigger.StatusIndex == Definition.TriggerStatusCancelled.Item1 || trigger.StatusIndex == Definition.TriggerStatusExecuted.Item1)
                {
                    return saveTriggers;
                }
                else if (trigger.StatusIndex == Definition.TriggerStatusWaiting.Item1 && (!trigger.StartTime.HasValue || trigger.StartTime.Value <= now))
                {
                    trigger.StatusIndex = Definition.TriggerStatusMonitoring.Item1;
                    saveTriggers = true;
                    _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{Definition.TriggerStatusWaiting.Item2} -> {trigger.StatusStr}");
                }
                else if (trigger.StatusIndex == Definition.TriggerStatusMonitoring.Item1)
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
                trigger.UpdateTime = now;

                if (trigger.Rule == Definition.IsGreaterThanOrEqualTo)
                {
                    if (trigger.ColumnValue >= trigger.TargetValue)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}|{trigger.StatusStr}");

                        _executedMap.TryAdd(key, trigger);

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsLessThanOrEqualTo)
                {
                    if (trigger.ColumnValue <= trigger.TargetValue)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}|{trigger.StatusStr}");

                        _executedMap.TryAdd(key, trigger);

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsEqualTo)
                {
                    if (trigger.ColumnValue == trigger.TargetValue)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{trigger.ColumnValue} {trigger.Rule} {trigger.TargetValue}|{trigger.StatusStr}");

                        _executedMap.TryAdd(key, trigger);

                        //
                    }
                }
                else
                {
                    trigger.StatusIndex = Definition.TriggerStatusCancelled.Item1;
                    saveTriggers = true;
                    _appCtrl.LogError($"Trigger|條件({trigger.Rule})錯誤，必須是大於小於等於");
                    return saveTriggers;
                }

                if (trigger.EndTime.HasValue && trigger.EndTime.Value <= now && trigger.StatusIndex != Definition.TriggerStatusExecuted.Item1)
                {
                    trigger.StatusIndex = Definition.TriggerStatusCancelled.Item1;
                    saveTriggers = true;
                    _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|觸價逾時，監控取消");
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

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out TriggerData trigger);

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    try
                    {
                        if (!_triggerMap.TryGetValue(trigger.PrimaryKey, out TriggerData _old))
                        { }
                        else if (_old.StatusIndex == Definition.TriggerStatusExecuted.Item1)
                        {
                            _appCtrl.LogWarn($"Trigger|{trigger.PrimaryKey}({trigger.ColumnName})，舊設定已觸發，將新增設定");
                            _triggerMap.Remove(trigger.PrimaryKey);
                        }
                        else
                        {
                            _appCtrl.LogWarn($"Trigger|{trigger.PrimaryKey}({trigger.ColumnName})，舊設定尚未觸發，將進行重置");
                            _triggerMap.Remove(trigger.PrimaryKey);
                            _triggerCollection.Remove(_old);
                        }

                        _triggerMap.Add(trigger.PrimaryKey, trigger);
                        _triggerCollection.Add(trigger);

                        SaveDataAsync();
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

            Parallel.ForEach(_triggerMap, pair =>
            {
                try
                {
                    if (UpdateStatus(pair.Key, pair.Value, pair.Value.Quote))
                    {
                        saveTriggers = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
            });

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
                decimal value = 0;

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

                if (decimal.TryParse(bodyValue, out value))
                {
                    _appCtrl.MainForm.TextBoxTriggerRuleValue.Text = $"{rule}{bodyValue}";
                }
                else
                {
                    _appCtrl.LogError($"Trigger|條件錯誤，無法解析({bodyValue})");
                    return;
                }

                string duration = _appCtrl.MainForm.TextBoxTriggerTimeDuration.Text.Replace(" ", string.Empty);
                DateTime? startTime = null;
                DateTime? endTime = null;

                string[] times = duration.Split('~');

                if (times.Length > 0 && !string.IsNullOrWhiteSpace(times[0]))
                {
                    times[0] = ToHHmmss(times[0]);

                    if (DateTime.TryParseExact(times[0], _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime _s))
                    {
                        startTime = _s;
                    }
                    else
                    {
                        _appCtrl.LogError($"Trigger|開始時間錯誤，無法解析({times[0]})({duration})");
                        return;
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
                        _appCtrl.LogError($"Trigger|結束時間錯誤，無法解析({times[1]})({duration})");
                        return;
                    }
                }

                if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && selectedQuote.Market != Definition.MarketFutures && selectedQuote.Market != Definition.MarketOptions)
                {
                    _appCtrl.LogError($"Trigger|非期貨選擇權，結束時間({times[1]})不可小於開始時間({times[0]})");
                    return;
                }
                else if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value && endTime.Value.Hour >= 5)
                {
                    _appCtrl.LogError($"Trigger|結束時間({times[1]})不可大於等於凌晨5點");
                    return;
                }

                if (startTime.HasValue && startTime.Value.Hour < 5 && (selectedQuote.Market == Definition.MarketFutures || selectedQuote.Market == Definition.MarketOptions))
                {
                    startTime = startTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"Trigger|期貨選擇權，開始時間跨日，{times[0]} -> {startTime.Value:MM/dd HH:mm:ss}");
                }
                if (endTime.HasValue && endTime.Value.Hour < 5 && (selectedQuote.Market == Definition.MarketFutures || selectedQuote.Market == Definition.MarketOptions))
                {
                    endTime = endTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"Trigger|期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}");
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
                    Strategy = _appCtrl.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                    StartTime = startTime,
                    EndTime = endTime,
                };

                _waitToAdd.Enqueue(trigger);

                if (decimal.TryParse(primaryKey, out decimal _pk))
                {
                    ++_pk;
                    _appCtrl.MainForm.TextBoxTriggerPrimaryKey.Text = $"{_pk}";
                }

                _appCtrl.MainForm.TabControlBB.SelectedIndex = 0;
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

        public void RecoverSetting(FileInfo file)
        {
            _appCtrl.LogTrace("Trigger|Start");

            try
            {
                if (file == null)
                {
                    if (_appCtrl.Config.TriggerFolder == null)
                    {
                        return;
                    }

                    _appCtrl.Config.TriggerFolder.Refresh();
                    file = _appCtrl.Config.TriggerFolder.GetFiles("*.csv").LastOrDefault();
                }

                if (file == null)
                {
                    return;
                }

                List<string> columnNames = new List<string>();

                //foreach (TriggerData trigger in TriggerData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                //{
                //    try
                //    {
                //        if (!string.IsNullOrWhiteSpace(trigger.PrimaryKey))
                //        {
                //            continue;
                //        }

                //        _triggerMap.Add(trigger.PrimaryKey, trigger);
                //        _triggerCollection.Add(trigger);

                //        //TODO
                //    }
                //    catch (Exception ex)
                //    {
                //        _appCtrl.LogException(ex, ex.StackTrace);
                //    }
                //}
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
