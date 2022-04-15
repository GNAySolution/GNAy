using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class StrategyController
    {
        private static readonly string[] _timeFormats = new string[] { "HHmmss", "HHmm", "HH" };

        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ConcurrentQueue<QuoteData> _waitToReset;
        private readonly ConcurrentQueue<string> _waitToCancel;
        private readonly ConcurrentQueue<StrategyData> _waitToAdd;

        private readonly SortedDictionary<string, StrategyData> _strategyMap;
        private readonly ObservableCollection<StrategyData> _strategyCollection;

        private readonly ConcurrentDictionary<string, StrategyData> _executedMap;

        public StrategyController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToReset = new ConcurrentQueue<QuoteData>();
            _waitToCancel = new ConcurrentQueue<string>();
            _waitToAdd = new ConcurrentQueue<StrategyData>();

            _strategyMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridStrategyRule.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _strategyCollection = _appCtrl.MainForm.DataGridStrategyRule.SetAndGetItemsSource<StrategyData>();

            _executedMap = new ConcurrentDictionary<string, StrategyData>();

            _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{_strategyMap.Count + 1}";
        }

        private StrategyController() : this(null)
        { }

        private void SaveData(ICollection<StrategyData> strategies)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (strategies == null)
                {
                    strategies = _strategyCollection.ToArray();
                }

                string path = Path.Combine(_appCtrl.Config.StrategyFolder.FullName, string.Format("{0}.csv", DateTime.Now.ToString(_appCtrl.Settings.StrategyFileFormat)));
                _appCtrl.LogTrace(path, UniqueName);

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(StrategyData.CSVColumnNames);

                    foreach (StrategyData strategy in strategies)
                    {
                        try
                        {
                            sw.WriteLine(strategy.ToCSVString());
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

        private bool UpdateStatus(StrategyData strategy, QuoteData quote, DateTime start)
        {
            bool saveStrategies = false;

            lock (strategy.SyncRoot)
            {
                if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveStrategies;
                }
                else if (quote == null)
                {
                    return saveStrategies;
                }
                else if (quote.Simulate.IsSimulating())
                {
                    return saveStrategies;
                }
                
                //
            }

            return saveStrategies;
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

                foreach (StrategyData strategy in _strategyMap.Values)
                {
                    if (strategy.Symbol == quote.Symbol)
                    {
                        strategy.Quote = quote;
                    }
                }
            }

            while (_waitToCancel.Count > 0)
            {
                _waitToCancel.TryDequeue(out string primaryKey);

                if (_strategyMap.TryGetValue(primaryKey, out StrategyData strategy))
                {
                    if (strategy.StatusEnum == StrategyStatus.Enum.Cancelled)
                    {
                        _appCtrl.LogError(strategy.ToLog(), UniqueName, DateTime.Now - start);
                    }
                    //else if (strategy.StatusEnum == StrategyStatus.Enum.Executed)
                    //{
                    //    _appCtrl.LogError($"{strategy.ToLog()}|已觸發無法取消", UniqueName, DateTime.Now - start);
                    //}
                    else
                    {
                        strategy.StatusEnum = StrategyStatus.Enum.Cancelled;
                        strategy.Comment = $"手動取消";
                        _appCtrl.LogTrace(strategy.ToLog(), UniqueName, DateTime.Now - start);
                    }

                    if (_waitToCancel.Count <= 0)
                    {
                        SaveDataAsync();
                    }
                }
                else
                {
                    _appCtrl.LogError($"{primaryKey}|查無此唯一鍵", UniqueName, DateTime.Now - start);
                }
            }

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out StrategyData strategy);

                StrategyData toRemove = null;

                if (!_strategyMap.TryGetValue(strategy.PrimaryKey, out StrategyData _old))
                {
                    if (strategy.StatusEnum != StrategyStatus.Enum.Cancelled)
                    {
                        _appCtrl.LogTrace($"{strategy.ToLog()}|新增設定", UniqueName, DateTime.Now - start);
                    }
                }
                //else if (_old.StatusEnum == StrategyStatus.Enum.Executed)
                //{
                //    _appCtrl.LogWarn($"{strategy.ToLog()}|舊設定已觸發，將新增設定", UniqueName, DateTime.Now - start);
                //    _strategyMap.Remove(strategy.PrimaryKey);
                //}
                else
                {
                    _appCtrl.LogWarn($"{strategy.ToLog()}|舊設定未觸發，將進行重置", UniqueName, DateTime.Now - start);
                    _strategyMap.Remove(strategy.PrimaryKey);
                    toRemove = _old;
                }

                _strategyMap.Add(strategy.PrimaryKey, strategy);

                List<StrategyData> list = _strategyMap.Values.ToList();
                int index = list.IndexOf(strategy);

                if (index + 1 < list.Count)
                {
                    StrategyData next = list[index + 1];
                    index = _strategyCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    try
                    {
                        _strategyCollection.Insert(index, strategy);

                        if (toRemove != null)
                        {
                            _strategyCollection.Remove(toRemove);
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

            bool saveStrategies = false;

            _executedMap.Clear();

            foreach (KeyValuePair<string, StrategyData> pair in _strategyMap)
            //Parallel.ForEach(_strategyMap, pair =>
            {
                try
                {
                    if (UpdateStatus(pair.Value, pair.Value.Quote, start))
                    {
                        saveStrategies = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }

            foreach (StrategyData executed in _executedMap.Values)
            {
                try
                {
                    //
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }

            if (saveStrategies)
            {
                SaveData(_strategyCollection);
            }
        }

        public void ClearQuotes()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                Parallel.ForEach(_strategyMap.Values, strategy =>
                {
                    lock (strategy.SyncRoot)
                    {
                        strategy.Quote = null;
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
                //_appCtrl.LogTrace($"Symbol={quote.Symbol}|Page={quote.Page}");

                _waitToReset.Enqueue(quote);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("Strategy|End");
            }
        }

        public void Cancel(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace(UniqueName, $"primaryKey={primaryKey}");

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
                if (_appCtrl.Config.StrategyFolder == null)
                {
                    _appCtrl.LogError("未設定策略資料夾(Settings.StrategyFolderPath)，無法建立策略資料", UniqueName);
                    return;
                }
                //else if (_appCtrl.MainForm.ComboBoxStrategyProduct.SelectedIndex < 0)
                //{
                //    return;
                //}
                //else if (_appCtrl.MainForm.ComboBoxStrategyColumn.SelectedIndex < 0)
                //{
                //    return;
                //}
                //else if (_appCtrl.MainForm.ComboBoxStrategyCancel.SelectedIndex < 0)
                //{
                //    return;
                //}
                //else if (string.IsNullOrWhiteSpace(_appCtrl.MainForm.TextBoxStrategyRuleValue.Text))
                //{
                //    return;
                //}

                string primaryKey = _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty);

                if (string.IsNullOrWhiteSpace(primaryKey))
                {
                    primaryKey = $"{_strategyMap.Count + 1}";
                }

                //QuoteData selectedQuote = _appCtrl.MainForm.ComboBoxStrategyProduct.SelectedItem as QuoteData;

                //string rule = _appCtrl.MainForm.TextBoxStrategyRuleValue.Text.Replace(" ", string.Empty);
                //string bodyValue = string.Empty;

                //if (rule.StartsWith(Definition.IsGreaterThanOrEqualTo))
                //{
                //    bodyValue = rule.Substring(Definition.IsGreaterThanOrEqualTo.Length);
                //    rule = Definition.IsGreaterThanOrEqualTo;
                //}
                //else if (rule.StartsWith(Definition.IsGreaterThan))
                //{
                //    bodyValue = rule.Substring(Definition.IsGreaterThan.Length);
                //    rule = Definition.IsGreaterThanOrEqualTo;
                //}
                //else if (rule.StartsWith(Definition.IsEqualTo))
                //{
                //    bodyValue = rule.Substring(Definition.IsEqualTo.Length);
                //    rule = Definition.IsEqualTo;
                //}
                //else if (rule.StartsWith(Definition.IsLessThanOrEqualTo))
                //{
                //    bodyValue = rule.Substring(Definition.IsLessThanOrEqualTo.Length);
                //    rule = Definition.IsLessThanOrEqualTo;
                //}
                //else if (rule.StartsWith(Definition.IsLessThan))
                //{
                //    bodyValue = rule.Substring(Definition.IsLessThan.Length);
                //    rule = Definition.IsLessThanOrEqualTo;
                //}
                //else
                //{
                //    _appCtrl.LogError($"條件({rule})錯誤，開頭必須是大於小於等於", UniqueName);
                //    return;
                //}

                //if (decimal.TryParse(bodyValue, out decimal value))
                //{
                //    _appCtrl.MainForm.TextBoxStrategyRuleValue.Text = $"{rule}{bodyValue}";
                //}
                //else
                //{
                //    _appCtrl.LogError($"條件錯誤，無法解析({bodyValue})", UniqueName);
                //    return;
                //}

                //string duration = _appCtrl.MainForm.TextBoxStrategyTimeDuration.Text.Replace(" ", string.Empty);
                //(bool, DateTime?, DateTime?) parseResult = TimeParse(selectedQuote, duration.Split('~'));

                //if (!parseResult.Item1)
                //{
                //    return;
                //}

                //StrategyData strategy = new StrategyData(selectedQuote, _appCtrl.MainForm.ComboBoxStrategyColumn.SelectedItem as TradeColumnStrategy)
                //{
                //    Creator = nameof(AddRule),
                //    Updater = nameof(AddRule),
                //    UpdateTime = DateTime.Now,
                //    PrimaryKey = primaryKey,
                //    Rule = rule,
                //    TargetValue = value,
                //    CancelIndex = _appCtrl.MainForm.ComboBoxStrategyCancel.SelectedIndex,
                //    StrategyOR = _appCtrl.MainForm.TextBoxStrategyStrategyOR.Text.Trim(),
                //    StrategyAND = _appCtrl.MainForm.TextBoxStrategyStrategyAND.Text.Trim(),
                //    StartTime = parseResult.Item2,
                //    EndTime = parseResult.Item3,
                //};

                //if (decimal.TryParse(primaryKey, out decimal _pk))
                //{
                //    if (_pk < _strategyMap.Count)
                //    {
                //        _pk = _strategyMap.Count;
                //    }

                //    _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{_pk + 1}";
                //}

                //_waitToAdd.Enqueue(strategy);
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
                if (_strategyMap.Count > 0)
                {
                    return;
                }

                if (file == null)
                {
                    if (_appCtrl.Config.StrategyFolder == null)
                    {
                        return;
                    }

                    _appCtrl.Config.StrategyFolder.Refresh();
                    file = _appCtrl.Config.StrategyFolder.GetFiles("*.csv").LastOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).Length == _appCtrl.Settings.StrategyFileFormat.Length);
                }

                if (file == null)
                {
                    return;
                }

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (StrategyData strategy in StrategyData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
                        {
                            continue;
                        }

                        //QuoteData quote = _appCtrl.Capital.GetQuote(strategy.Symbol);

                        //if (quote == null)
                        //{
                        //    continue;
                        //}

                        //strategy.StatusEnum = StrategyStatus.Enum.Waiting;
                        //strategy.Quote = quote;
                        //strategy.ColumnValue = 0;
                        //strategy.Comment = string.Empty;

                        //string startTime = strategy.StartTime.HasValue ? strategy.StartTime.Value.ToString("HHmmss") : string.Empty;
                        //string endTime = strategy.EndTime.HasValue ? strategy.EndTime.Value.ToString("HHmmss") : string.Empty;
                        //(bool, DateTime?, DateTime?) parseResult = TimeParse(quote, startTime, endTime);

                        //if (!parseResult.Item1)
                        //{
                        //    continue;
                        //}

                        //strategy.StartTime = parseResult.Item2;
                        //strategy.EndTime = parseResult.Item3;

                        //if ((!strategy.StartTime.HasValue || strategy.StartTime.Value <= DateTime.Now) && !_appCtrl.Config.StartOnTime)
                        //{
                        //    strategy.StatusEnum = StrategyStatus.Enum.Cancelled;
                        //    strategy.Comment = "程式沒有在正常時間啟動，不執行此監控";
                        //    _appCtrl.LogError(strategy.ToLog(), UniqueName, DateTime.Now - start);
                        //}

                        strategy.Creator = nameof(RecoverSetting);
                        strategy.Updater = nameof(RecoverSetting);
                        strategy.UpdateTime = DateTime.Now;

                        if (decimal.TryParse(strategy.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        _waitToAdd.Enqueue(strategy);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(1 * 1000);
                if (_strategyCollection.Count >= nextPK)
                {
                    nextPK = _strategyCollection.Count + 1;
                }

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{nextPK}";
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
    }
}
