using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class TriggerController
    {
        public readonly DateTime CreatedTime;

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        public TriggerController()
        {
            CreatedTime = DateTime.Now;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = new ObservableCollection<string>()
            {
                Definition.TriggerCancel0.Item2,
                Definition.TriggerCancel1.Item2,
                Definition.TriggerCancel2.Item2,
                Definition.TriggerCancel3.Item2,
                Definition.TriggerCancel4.Item2,
            };
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.ItemsSource = _triggerCancelKinds.GetViewSource();
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedIndex = Definition.TriggerCancel0.Item1;

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
                if (AppCtrl.Instance.MainForm.ComboBoxTriggerProduct.SelectedIndex < 0)
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
                    AppCtrl.Instance.LogError($"條件設定錯誤，開頭必須是大於({Definition.IsGreaterThan})小於({Definition.IsLessThan})或等於({Definition.IsEqualTo})");
                    return;
                }

                if (decimal.TryParse(bodyValue, out value))
                {
                    AppCtrl.Instance.MainForm.TextBoxTriggerRuleValue.Text = $"{rule}{bodyValue}";
                }
                else
                {
                    AppCtrl.Instance.LogError($"條件設定錯誤，無法解析為數值({bodyValue})");
                    return;
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
                    CancelStr = AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedItem as string,
                    Strategy = AppCtrl.Instance.MainForm.TextBoxTriggerStrategy.Text.Trim(),
                };

                string key = $"{trigger.OrderAcc},{trigger.Symbol},{trigger.ColumnProperty}";

                if (_triggerMap.TryGetValue(key, out TriggerData _old))
                {
                    AppCtrl.Instance.LogWarn($"{trigger.ColumnName}({key})，欄位已經設定，將進行重置");
                    _triggerCollection.Remove(_old);
                }

                _triggerMap[key] = trigger;
                _triggerCollection.Add(trigger);

                SaveTriggers(AppCtrl.Instance.Config.TriggerFolder);
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

        public void SaveTriggers(DirectoryInfo folder)
        {
            if (_triggerMap.Count <= 0)
            {
                return;
            }

            DateTime now = DateTime.Now;

            try
            {
                AppCtrl.Instance.LogTrace($"SKAPI|Start|folder={folder.Name}");

                TriggerData[] triggers = _triggerMap.Values.ToArray();
                string path = Path.Combine(folder.FullName, $"Triggers_{now:MMdd_HHmm}.csv");

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(TriggerData.CSVColumnNames);

                    foreach (TriggerData q in triggers)
                    {
                        try
                        {
                            sw.WriteLine(q.ToCSVString());
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
                AppCtrl.Instance.LogTrace("SKAPI|End");
            }
        }
    }
}
