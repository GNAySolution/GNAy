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
        }

        private TriggerController() : this(null)
        { }

        private void SaveTriggers(ICollection<TriggerData> triggers)
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

        private void SaveTriggersAsync()
        {
            Task.Factory.StartNew(() => SaveTriggers(null));
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
                decimal value = 0;

                if (trigger.Column.Property.PropertyType == typeof(DateTime))
                {
                    value = decimal.Parse(((DateTime)valueObj).ToString(trigger.Column.Attribute.ValueFormat));
                }
                else if (trigger.Column.Property.PropertyType == typeof(string))
                {
                    value = decimal.Parse((string)valueObj);
                }
                else
                {
                    value = (decimal)valueObj;
                }

                if (trigger.Rule == Definition.IsGreaterThanOrEqualTo)
                {
                    if (value >= trigger.Value)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{value} {trigger.Rule} {trigger.Value}|{trigger.StatusStr}");

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsLessThanOrEqualTo)
                {
                    if (value <= trigger.Value)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{value} {trigger.Rule} {trigger.Value}|{trigger.StatusStr}");

                        //
                    }
                }
                else if (trigger.Rule == Definition.IsEqualTo)
                {
                    if (value == trigger.Value)
                    {
                        trigger.StatusIndex = Definition.TriggerStatusExecuted.Item1;
                        saveTriggers = true;
                        _appCtrl.LogTrace($"Trigger|{key}({trigger.ColumnName})|{value} {trigger.Rule} {trigger.Value}|{trigger.StatusStr}");

                        //
                    }
                }
                else
                {
                    trigger.StatusIndex = Definition.TriggerStatusCancelled.Item1;
                    saveTriggers = true;
                    _appCtrl.LogError($"Trigger|條件({trigger.Rule})錯誤，必須是大於({Definition.IsGreaterThan})小於({Definition.IsLessThan})或等於({Definition.IsEqualTo})");
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

                string key = $"{trigger.OrderAcc},{trigger.Symbol},{trigger.ColumnProperty},{trigger.Rule}";

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    try
                    {
                        if (!_triggerMap.TryGetValue(key, out TriggerData _old))
                        { }
                        else if (_old.StatusIndex == Definition.TriggerStatusExecuted.Item1)
                        {
                            _appCtrl.LogWarn($"Trigger|{key}({trigger.ColumnName})，舊設定已觸發，將新增設定");
                            _triggerMap.Remove(key);
                        }
                        else
                        {
                            _appCtrl.LogWarn($"Trigger|{key}({trigger.ColumnName})，舊設定尚未觸發，將進行重置");
                            _triggerMap.Remove(key);
                            _triggerCollection.Remove(_old);
                        }

                        _triggerMap.Add(key, trigger);
                        _triggerCollection.Add(trigger);

                        SaveTriggersAsync();
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

            if (saveTriggers)
            {
                SaveTriggers(_triggerCollection);
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
                _appCtrl.LogTrace($"Trigger|Start|Symbol={quote.Symbol}|Page={quote.Page}");

                _waitToReset.Enqueue(quote);
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

            time = time.PadRight(6, '0');

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

                QuoteData selectedQuote = _appCtrl.MainForm.ComboBoxTriggerProduct.SelectedItem as QuoteData;
                OrderAccData orderAcc = null;

                if (selectedQuote.Market == Definition.MarketFutures || selectedQuote.Market == Definition.MarketOptions)
                {
                    orderAcc = _appCtrl.MainForm.ComboBoxFuturesAccs.SelectedItem as OrderAccData;
                }
                else
                {
                    orderAcc = _appCtrl.MainForm.ComboBoxStockAccs.SelectedItem as OrderAccData;
                }

                if (orderAcc == null)
                {
                    return;
                }

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
                    _appCtrl.LogError($"Trigger|條件({rule})錯誤，開頭必須是大於({Definition.IsGreaterThan})小於({Definition.IsLessThan})或等於({Definition.IsEqualTo})");
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
                else if (endTime.HasValue && endTime.Value.Hour <= 5 && (selectedQuote.Market == Definition.MarketFutures || selectedQuote.Market == Definition.MarketOptions))
                {
                    endTime = endTime.Value.AddDays(1);
                    _appCtrl.LogTrace($"Trigger|期貨選擇權，結束時間跨日，{times[1]} -> {endTime.Value:MM/dd HH:mm:ss}");
                }

                TriggerData trigger = new TriggerData(orderAcc, selectedQuote, _appCtrl.MainForm.ComboBoxTriggerColumn.SelectedItem as TradeColumnTrigger)
                {
                    Creator = nameof(AddRule),
                    Updater = nameof(AddRule),
                    UpdateTime = DateTime.Now,
                    Rule = rule,
                    Value = value,
                    CancelIndex = _appCtrl.MainForm.ComboBoxTriggerCancel.SelectedIndex,
                    Strategy = _appCtrl.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                    StartTime = startTime,
                    EndTime = endTime,
                };

                _waitToAdd.Enqueue(trigger);
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
    }
}
