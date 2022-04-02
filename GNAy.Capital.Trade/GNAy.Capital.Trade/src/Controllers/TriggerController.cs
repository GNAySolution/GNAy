using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private static string[] _timeFormats = new string[] { "HHmmss", "Hmmss", "HHmm", "Hmm", "HH", "H", "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm" };

        public readonly DateTime CreatedTime;

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly ConcurrentQueue<TriggerData> _waitToAdd;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        public TriggerController()
        {
            CreatedTime = DateTime.Now;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = new ObservableCollection<string>(Definition.TriggerCancelKinds);
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.ItemsSource = _triggerCancelKinds.GetViewSource();
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedIndex = Definition.TriggerCancel0.Item1;

            _waitToAdd = new ConcurrentQueue<TriggerData>();

            _triggerMap = new SortedDictionary<string, TriggerData>();
            AppCtrl.Instance.MainForm.DataGridTriggerRule.SetHeadersByBindings(TriggerData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _triggerCollection = AppCtrl.Instance.MainForm.DataGridTriggerRule.SetAndGetItemsSource<TriggerData>();
        }

        public void OnQuotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.Instance.LogException(ex, ex.StackTrace);
            }
        }

        public void AddRule()
        {
            AppCtrl.Instance.LogTrace("Start");

            try
            {
                if (AppCtrl.Instance.Config.TriggerFolder == null)
                {
                    AppCtrl.Instance.LogError($"觸價資料夾不存在，無法建立觸價資料");
                    return;
                }
                else if (AppCtrl.Instance.MainForm.ComboBoxTriggerProduct.SelectedIndex < 0)
                {
                    return;
                }
                else if (AppCtrl.Instance.MainForm.ComboBoxTriggerColumn.SelectedIndex < 0)
                {
                    return;
                }
                else if (AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedIndex < 0)
                {
                    return;
                }
                else if (string.IsNullOrWhiteSpace(AppCtrl.Instance.MainForm.TextBoxTriggerRuleValue.Text))
                {
                    return;
                }

                QuoteData selectedQuote = AppCtrl.Instance.MainForm.ComboBoxTriggerProduct.SelectedItem as QuoteData;
                OrderAccData orderAcc = null;

                if (selectedQuote.Market == Definition.MarketFutures || selectedQuote.Market == Definition.MarketOptions)
                {
                    orderAcc = AppCtrl.Instance.MainForm.ComboBoxFuturesAccs.SelectedItem as OrderAccData;
                }
                else
                {
                    orderAcc = AppCtrl.Instance.MainForm.ComboBoxStockAccs.SelectedItem as OrderAccData;
                }

                if (orderAcc == null)
                {
                    return;
                }

                string rule = AppCtrl.Instance.MainForm.TextBoxTriggerRuleValue.Text.Replace(" ", string.Empty);
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
                    AppCtrl.Instance.LogError($"條件錯誤，開頭必須是大於({Definition.IsGreaterThan})小於({Definition.IsLessThan})或等於({Definition.IsEqualTo})");
                    return;
                }

                if (decimal.TryParse(bodyValue, out value))
                {
                    AppCtrl.Instance.MainForm.TextBoxTriggerRuleValue.Text = $"{rule}{bodyValue}";
                }
                else
                {
                    AppCtrl.Instance.LogError($"條件錯誤，無法解析({bodyValue})");
                    return;
                }

                string duration = AppCtrl.Instance.MainForm.TextBoxTriggerTimeDuration.Text.Replace(" ", string.Empty);
                DateTime? startTime = null;
                DateTime? endTime = null;

                string[] times = duration.Split('~');

                if (times.Length > 0 && !string.IsNullOrWhiteSpace(times[0]))
                {
                    if (DateTime.TryParseExact(times[0], _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime _s))
                    {
                        startTime = _s;
                    }
                    else
                    {
                        AppCtrl.Instance.LogError($"開始時間錯誤，無法解析({times[0]})({duration})");
                        return;
                    }
                }

                if (times.Length > 1 && !string.IsNullOrWhiteSpace(times[1]))
                {
                    if (DateTime.TryParseExact(times[1], _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime _e))
                    {
                        endTime = _e;
                    }
                    else
                    {
                        AppCtrl.Instance.LogError($"結束時間錯誤，無法解析({times[1]})({duration})");
                        return;
                    }
                }

                TriggerData trigger = new TriggerData(orderAcc, AppCtrl.Instance.MainForm.ComboBoxTriggerColumn.SelectedItem as TradeColumnTrigger)
                {
                    Creator = nameof(AddRule),
                    Updater = nameof(AddRule),
                    UpdateTime = DateTime.Now,
                    Symbol = selectedQuote.Symbol,
                    Rule = rule,
                    Value = value,
                    CancelIndex = AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedIndex,
                    Strategy = AppCtrl.Instance.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                    StartTime = startTime,
                    EndTime = endTime,
                };

                _waitToAdd.Enqueue(trigger);
            }
            catch (Exception ex)
            {
                AppCtrl.Instance.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.Instance.LogTrace("End");
            }
        }

        /// <summary>
        /// Run in background.
        /// </summary>
        public void UpdateStatus()
        {
            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out TriggerData trigger);

                string key = $"{trigger.OrderAcc},{trigger.Symbol},{trigger.ColumnProperty}";

                AppCtrl.Instance.MainForm.InvokeRequired(delegate
                {
                    try
                    {
                        if (_triggerMap.TryGetValue(key, out TriggerData _old))
                        {
                            if (_old.StatusIndex == Definition.TriggerStatus3.Item1)
                            {
                                AppCtrl.Instance.LogWarn($"{trigger.ColumnName}({key})，舊設定已觸發，將新增設定");
                                _triggerMap.Remove(key);
                            }
                            else
                            {
                                AppCtrl.Instance.LogWarn($"{trigger.ColumnName}({key})，舊設定尚未觸發，將進行重置");
                                _triggerMap.Remove(key);
                                _triggerCollection.Remove(_old);
                            }
                        }

                        _triggerMap.Add(key, trigger);
                        _triggerCollection.Add(trigger);

                        SaveTriggersAsync();
                    }
                    catch (Exception ex)
                    {
                        AppCtrl.Instance.LogException(ex, ex.StackTrace);
                    }
                });

                if (_waitToAdd.Count <= 0)
                {
                    return;
                }
            }

            foreach (TriggerData trigger in _triggerMap.Values)
            {
                try
                {
                    if (trigger.StatusIndex == Definition.TriggerStatus1.Item1 || trigger.StatusIndex == Definition.TriggerStatus3.Item1)
                    {
                        continue;
                    }

                    //
                }
                catch (Exception ex)
                {
                    AppCtrl.Instance.LogException(ex, ex.StackTrace);
                }
            }
        }

        public void SaveTriggersAsync()
        {
            if (_triggerMap.Count <= 0)
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    AppCtrl.Instance.LogTrace("Start");

                    string path = Path.Combine(AppCtrl.Instance.Config.TriggerFolder.FullName, $"{DateTime.Now:MMdd_HHmm}.csv");
                    AppCtrl.Instance.LogTrace(path);

                    using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                    {
                        sw.WriteLine(TriggerData.CSVColumnNames);

                        foreach (TriggerData trigger in _triggerMap.Values.ToArray())
                        {
                            try
                            {
                                sw.WriteLine(trigger.ToCSVString());
                            }
                            catch (Exception ex)
                            {
                                AppCtrl.Instance.LogException(ex, ex.StackTrace);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppCtrl.Instance.LogException(ex, ex.StackTrace);
                }
                finally
                {
                    AppCtrl.Instance.LogTrace("End");
                }
            });
        }
    }
}
