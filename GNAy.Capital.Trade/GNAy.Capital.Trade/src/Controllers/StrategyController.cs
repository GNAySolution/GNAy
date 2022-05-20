using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
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
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ConcurrentQueue<StrategyData> _waitToAdd;

        private readonly SortedDictionary<string, StrategyData> _dataMap;
        private readonly ObservableCollection<StrategyData> _dataCollection;

        public int Count => _dataCollection.Count;
        public StrategyData this[string key] => _dataMap.TryGetValue(key, out StrategyData data) ? data : null;
        public IReadOnlyList<StrategyData> DataCollection => _dataCollection;

        public string Notice { get; private set; }

        public StrategyController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(StrategyController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<StrategyData>();

            _dataMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridStrategyRule.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridStrategyRule.SetAndGetItemsSource<StrategyData>();

            Notice = string.Empty;
        }

        private StrategyController() : this(null)
        { }

        public void SaveData(IEnumerable<StrategyData> dataCollection, DirectoryInfo dir, string fileFormat)
        {
            DateTime start = _appCtrl.StartTrace($"{dir?.FullName}|{fileFormat}", UniqueName);

            try
            {
                string path = Path.Combine(dir.FullName, string.Format("{0}.csv", DateTime.Now.ToString(fileFormat)));
                _appCtrl.LogTrace(start, path, UniqueName);

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(StrategyData.CSVColumnNames);

                    foreach (StrategyData data in dataCollection)
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

        public void MarketCheck(StrategyData data, QuoteData quote)
        {
            if (quote.MarketGroupEnum == Market.EGroup.TSE || quote.MarketGroupEnum == Market.EGroup.OTC || quote.MarketGroupEnum == Market.EGroup.Emerging)
            {
                if (data.MarketType != Market.EType.Stock)
                {
                    throw new ArgumentException($"MarketGroupEnum={quote.MarketGroupEnum}|MarketType={data.MarketType}|{data.ToLog()}");
                }
            }
            else if (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option)
            {
                if (data.MarketType != Market.EType.Futures && data.MarketType != Market.EType.Option)
                {
                    throw new ArgumentException($"MarketGroupEnum={quote.MarketGroupEnum}|MarketType={data.MarketType}|{data.ToLog()}");
                }
                else if (quote.MarketGroupEnum == Market.EGroup.Futures)
                {
                    data.MarketType = Market.EType.Futures;
                }
                else if (quote.MarketGroupEnum == Market.EGroup.Option)
                {
                    data.MarketType = Market.EType.Option;
                }
            }
        }

        private void ParentCheck(StrategyData strategy, bool readyToSend, DateTime start)
        {
            strategy = strategy.Trim();

            if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{strategy.ToLog()}");
            }
            else if (strategy.Parent != null)
            {
                throw new ArgumentException($"strategy.Parent != null|{strategy.Parent.ToLog()}");
            }
            else if (strategy.OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({strategy.OrderQty}) <= 0|{strategy.ToLog()}");
            }
            else if (_appCtrl.OrderDetail[strategy.PrimaryKey] != null)
            {
                throw new ArgumentException($"_appCtrl.OrderDetail[{strategy.PrimaryKey}] != null|{strategy.ToLog()}");
            }
            else if (strategy.WinCloseSeconds < 0)
            {
                throw new ArgumentException($"strategy.WinCloseSeconds({strategy.WinCloseSeconds}) < 0|{strategy.ToLog()}");
            }
            else if (strategy.LossCloseSeconds < 0)
            {
                throw new ArgumentException($"strategy.LossCloseSeconds({strategy.LossCloseSeconds}) < 0|{strategy.ToLog()}");
            }
            else if (strategy.Quote != null && strategy.Quote.Symbol != strategy.Symbol)
            {
                throw new ArgumentException($"策略關聯報價代碼錯誤|{strategy.Quote.Symbol} != {strategy.Symbol}|{strategy.ToLog()}");
            }
            else if (strategy.Quote == null)
            {
                strategy.Quote = _appCtrl.CAPQuote[strategy.Symbol];
            }

            if (strategy.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(strategy.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"strategy.Symbol={strategy.Symbol}|{strategy.ToLog()}");
                }
                else if (!string.IsNullOrWhiteSpace(strategy.StopLossBefore) || !string.IsNullOrWhiteSpace(strategy.StopWinBefore) || !string.IsNullOrWhiteSpace(strategy.MoveStopWinBefore))
                {
                    throw new ArgumentException($"商品 {strategy.Symbol} 無訂閱報價，無法進行策略監控|{strategy.ToLog()}");
                }

                MarketCheck(strategy, _appCtrl.CAPQuote.CreateOrUpdate(product.Item2));

                return;
            }

            MarketCheck(strategy, strategy.Quote);

            if (_appCtrl.CAPQuote.MarketCloseTime != DateTime.MinValue)
            {
                if (strategy.WinCloseTime == DateTime.MinValue && strategy.WinCloseSeconds > 0)
                {
                    strategy.WinCloseTime = _appCtrl.CAPQuote.MarketCloseTime.AddSeconds(-strategy.WinCloseSeconds);
                }

                if (strategy.LossCloseTime == DateTime.MinValue && strategy.LossCloseSeconds > 0)
                {
                    strategy.LossCloseTime = _appCtrl.CAPQuote.MarketCloseTime.AddSeconds(-strategy.LossCloseSeconds);
                }
            }

            (string, decimal) orderPriceAfter = OrderPrice.Parse(strategy.OrderPriceBefore, strategy.Quote);

            if (readyToSend)
            {
                strategy.OrderPriceAfter = orderPriceAfter.Item2;
                _appCtrl.LogTrace(start, $"委託價計算前={strategy.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價計算前={strategy.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }

            if (!string.IsNullOrWhiteSpace(strategy.StopLossBefore))
            {
                (string, decimal) stopLossPriceAfter = OrderPrice.Parse(strategy.StopLossBefore, strategy.Quote);

                if (strategy.BSEnum == OrderBS.Enum.Buy)
                {
                    if (stopLossPriceAfter.Item2 >= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{strategy.ToLog()}");
                    }
                }
                else if (stopLossPriceAfter.Item2 <= orderPriceAfter.Item2)
                {
                    throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{strategy.ToLog()}");
                }

                if (readyToSend)
                {
                    strategy.StopLossAfter = stopLossPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停損價計算前={strategy.StopLossBefore}|計算後={stopLossPriceAfter.Item1}", UniqueName);
                    Notice = $"停損價計算前={strategy.StopLossBefore}|計算後={stopLossPriceAfter.Item1}";
                }
            }

            if (!string.IsNullOrWhiteSpace(strategy.StopWinBefore))
            {
                string stopWinBefore = strategy.StopWinBefore;

                if (stopWinBefore.Contains("(") || stopWinBefore.Contains(","))
                {
                    string[] cells = stopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    stopWinBefore = cells[0];
                    strategy.StopWinQty = int.Parse(cells[1]);
                }

                (string, decimal) stopWinPriceAfter = OrderPrice.Parse(stopWinBefore, strategy.Quote);

                if (strategy.BSEnum == OrderBS.Enum.Buy)
                {
                    if (stopWinPriceAfter.Item2 <= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{strategy.ToLog()}");
                    }
                }
                else if (stopWinPriceAfter.Item2 >= orderPriceAfter.Item2)
                {
                    throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{strategy.ToLog()}");
                }

                if (readyToSend)
                {
                    strategy.StopWinPrice = stopWinPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停利價計算前={stopWinBefore}|計算後={stopWinPriceAfter.Item1}", UniqueName);
                    Notice = $"停利價計算前={stopWinBefore}|計算後={stopWinPriceAfter.Item1}";
                }
            }

            if (strategy.StopWinQty == 0)
            { } //滿足條件但不減倉
            else if (strategy.StopWinQty > 0)
            {
                throw new ArgumentException($"停利減倉量({strategy.StopWinQty})應為負值或0|{strategy.ToLog()}");
            }
            else if (strategy.OrderQty + strategy.StopWinQty < 0)
            {
                throw new ArgumentException($"停利減倉量({strategy.StopWinQty}) > 委託量({strategy.OrderQty})|{strategy.ToLog()}");
            }

            if (!string.IsNullOrWhiteSpace(strategy.MoveStopWinBefore))
            {
                string moveStopWinBefore = strategy.MoveStopWinBefore;

                if (moveStopWinBefore.Contains("(") || moveStopWinBefore.Contains(","))
                {
                    string[] cells = moveStopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    moveStopWinBefore = cells[0];
                    strategy.MoveStopWinQty = int.Parse(cells[1]);
                }

                strategy.MoveStopWinOffset = decimal.Parse(moveStopWinBefore);

                if (strategy.BSEnum == OrderBS.Enum.Buy)
                {
                    if (strategy.MoveStopWinOffset >= 0)
                    {
                        throw new ArgumentException($"移動停利位移({strategy.MoveStopWinOffset}) >= 0|{strategy.ToLog()}");
                    }
                }
                else if (strategy.MoveStopWinOffset <= 0)
                {
                    throw new ArgumentException($"移動停利位移({strategy.MoveStopWinOffset}) <= 0|{strategy.ToLog()}");
                }
            }

            if (strategy.MoveStopWinQty == 0)
            { } //滿足條件但不減倉
            else if (strategy.MoveStopWinQty > 0)
            {
                throw new ArgumentException($"移動停利減倉量({strategy.MoveStopWinQty})應為負值或0|{strategy.ToLog()}");
            }
            else if (strategy.OrderQty + strategy.StopWinQty + strategy.MoveStopWinQty < 0)
            {
                throw new ArgumentException($"移動停利減倉量({strategy.MoveStopWinQty}) + 停利減倉量({strategy.StopWinQty}) > 委託量({strategy.OrderQty})|{strategy.ToLog()}");
            }
        }

        private bool Stop(StrategyData strategy, int qty, string comment, DateTime start)
        {
            const string methodName = nameof(Stop);

            try
            {
                if (strategy.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    throw new ArgumentException(strategy.ToLog());
                }
                else if (strategy.StatusEnum == StrategyStatus.Enum.Waiting || strategy.StatusEnum == StrategyStatus.Enum.OrderError || strategy.StopLossData != null || strategy.MarketClosingData != null || strategy.UnclosedQty <= 0)
                {
                    strategy.StatusEnum = StrategyStatus.Enum.Cancelled;
                    strategy.Comment = comment;
                    strategy.Updater = methodName;
                    strategy.UpdateTime = DateTime.Now;

                    return true;
                }

                StrategyData marketClosingOrder = strategy.CreateMarketClosingOrder();
                marketClosingOrder.OrderQty = qty;

                if (marketClosingOrder.OrderQty > 0) //負值減倉正值留倉
                {
                    marketClosingOrder.OrderQty = strategy.UnclosedQty - marketClosingOrder.OrderQty;
                }

                if (marketClosingOrder.OrderQty <= 0 || marketClosingOrder.OrderQty > strategy.UnclosedQty)
                {
                    marketClosingOrder.OrderQty = strategy.UnclosedQty;
                }

                strategy.StatusEnum = StrategyStatus.Enum.MarketClosingSent;
                strategy.Comment = comment;
                strategy.Updater = methodName;
                strategy.UpdateTime = DateTime.Now;

                _appCtrl.CAPOrder.SendAsync(marketClosingOrder);

                return true;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return false;
        }

        public bool Stop(string primaryKey, int qty = 0)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}|qty={qty}", UniqueName);

            try
            {
                if (!_dataMap.TryGetValue(primaryKey.Replace(" ", string.Empty), out StrategyData strategy))
                {
                    throw new ArgumentNullException($"查無此唯一鍵|{primaryKey}");
                }

                lock (strategy.SyncRoot)
                {
                    if (Stop(strategy, qty, "手動停止", start))
                    {
                        Task.Factory.StartNew(() => SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat));

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return false;
        }

        private void StartTrigger(StrategyData data, string primary, DateTime start)
        {
            try
            {
                (LogLevel, string) result = _appCtrl.Trigger.Restart(primary);

                if (result.Item1 == LogLevel.Trace)
                {
                    _appCtrl.LogTrace(start, $"重啟觸價({primary})|{data.ToLog()}", UniqueName);
                }
                else
                {
                    _appCtrl.Log(result.Item1, $"{result.Item2}|{data.ToLog()}", UniqueName, DateTime.Now - start);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"重啟觸價({primary})失敗|{data.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void StartStrategy(StrategyData data, string primary, DateTime start)
        {
            try
            {
                StrategyData strategy = _appCtrl.Strategy[primary];
                strategy = strategy.Reset();
                _appCtrl.Strategy.StartNow(strategy.PrimaryKey);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"執行策略({primary})失敗|{data.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void AfterStopLoss(StrategyData data, DateTime start)
        {
            if (!string.IsNullOrWhiteSpace(data.TriggerAfterStopLoss))
            {
                HashSet<string> triggers = new HashSet<string>(data.TriggerAfterStopLoss.Split(','));

                foreach (string primary in triggers)
                {
                    StartTrigger(data, primary, start);
                }
            }

            if (!string.IsNullOrWhiteSpace(data.StrategyAfterStopLoss))
            {
                HashSet<string> strategise = new HashSet<string>(data.StrategyAfterStopLoss.Split(','));

                foreach (string primary in strategise)
                {
                    StartStrategy(data, primary, start);
                }
            }
        }

        private void AfterStopWin(StrategyData data, bool isMoveStopWin, DateTime start)
        {
            if (isMoveStopWin)
            {
                if (data.MoveStopWinQty == 0)
                {
                    return;
                }
            }
            else if (data.MoveStopWinQty != 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.TriggerAfterStopWin))
            {
                HashSet<string> triggers = new HashSet<string>(data.TriggerAfterStopWin.Split(','));

                foreach (string primary in triggers)
                {
                    StartTrigger(data, primary, start);
                }
            }

            if (!string.IsNullOrWhiteSpace(data.StrategyAfterStopWin))
            {
                HashSet<string> strategise = new HashSet<string>(data.StrategyAfterStopWin.Split(','));

                foreach (string primary in strategise)
                {
                    StartStrategy(data, primary, start);
                }
            }
        }

        private bool UpdateStatus(StrategyData strategy, QuoteData quote, DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            bool saveData = false;

            lock (strategy.SyncRoot)
            {
                if (_appCtrl.CAPQuote.Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                else if (quote.Simulate != QuoteData.RealTrade)
                {
                    return saveData;
                }
                else if (quote.DealPrice == 0)
                {
                    return saveData;
                }
                else if (strategy.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    return saveData;
                }
                else if (strategy.StopLossData != null || strategy.MarketClosingData != null)
                {
                    return saveData;
                }
                else if (strategy.UnclosedQty <= 0)
                {
                    return saveData;
                }
                else if (strategy.StatusEnum == StrategyStatus.Enum.Waiting)
                {
                    strategy.MarketPrice = quote.DealPrice;
                    return saveData;
                }

                strategy.MarketPrice = quote.DealPrice;
                strategy.UnclosedProfit = (strategy.MarketPrice - strategy.DealPrice) * strategy.UnclosedQty * (strategy.BSEnum == OrderBS.Enum.Buy ? 1 : -1);
                strategy.Updater = methodName;
                strategy.UpdateTime = DateTime.Now;

                if (strategy.BSEnum == OrderBS.Enum.Buy)
                {
                    if (strategy.MoveStopWinPrice < strategy.MarketPrice)
                    {
                        strategy.MoveStopWinPrice = strategy.MarketPrice;
                    }
                }
                else if ((strategy.MoveStopWinPrice > strategy.MarketPrice || strategy.MoveStopWinPrice == 0) && strategy.MarketPrice != 0)
                {
                    strategy.MoveStopWinPrice = strategy.MarketPrice;
                }

                if (strategy.UnclosedQty > 0)
                {
                    if (strategy.UnclosedProfit > 0 && strategy.WinCloseSeconds > 0 && DateTime.Now >= strategy.WinCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        Stop(strategy, strategy.WinCloseQty, "收盤獲利減倉", start);

                        saveData = true;
                        return saveData;
                    }
                    else if (strategy.UnclosedProfit <= 0 && strategy.LossCloseSeconds > 0 && DateTime.Now >= strategy.LossCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        Stop(strategy, strategy.LossCloseQty, "收盤損失減倉", start);

                        saveData = true;
                        return saveData;
                    }
                }

                if (strategy.StatusEnum == StrategyStatus.Enum.OrderSent || strategy.StatusEnum == StrategyStatus.Enum.OrderReport || strategy.StatusEnum == StrategyStatus.Enum.DealReport || strategy.StatusEnum == StrategyStatus.Enum.OrderError)
                {
                    if (strategy.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (strategy.MarketPrice <= strategy.StopLossAfter)
                        {
                            StrategyData stopLossOrder = strategy.CreateStopLossOrder();

                            strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                            _appCtrl.CAPOrder.SendAsync(stopLossOrder);

                            saveData = true;
                            AfterStopLoss(strategy, start);
                        }
                        else if (strategy.MarketPrice >= strategy.StopWinPrice && strategy.StopWinQty <= 0)
                        {
                            StrategyData stopWinOrder = strategy.CreateStopWinOrder();

                            strategy.StatusEnum = StrategyStatus.Enum.StopWinSent;

                            if (strategy.StopWinQty == 0)
                            { } //滿足條件但不減倉
                            else
                            {
                                _appCtrl.CAPOrder.SendAsync(stopWinOrder);
                            }

                            saveData = true;
                            AfterStopWin(strategy, false, start);
                        }
                    }
                    else if (strategy.MarketPrice >= strategy.StopLossAfter) //strategy.BSEnum == OrderBS.Enum.Sell
                    {
                        StrategyData stopLossOrder = strategy.CreateStopLossOrder();

                        strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                        _appCtrl.CAPOrder.SendAsync(stopLossOrder);

                        saveData = true;
                        AfterStopLoss(strategy, start);
                    }
                    else if (strategy.MarketPrice <= strategy.StopWinPrice && strategy.StopWinQty <= 0)
                    {
                        StrategyData stopWinOrder = strategy.CreateStopWinOrder();

                        strategy.StatusEnum = StrategyStatus.Enum.StopWinSent;

                        if (strategy.StopWinQty == 0)
                        { } //滿足條件但不減倉
                        else
                        {
                            _appCtrl.CAPOrder.SendAsync(stopWinOrder);
                        }

                        saveData = true;
                        AfterStopWin(strategy, false, start);
                    }
                }
                else if (strategy.MoveStopWinQty <= 0 && strategy.MoveStopWinData == null && strategy.StopWinData != null && strategy.StopWinData.OrderQty < strategy.OrderQty)
                {
                    if (strategy.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (strategy.MarketPrice <= strategy.MoveStopWinPrice + strategy.MoveStopWinOffset)
                        {
                            StrategyData moveStopWinOrder = strategy.CreateMoveStopWinOrder();

                            strategy.StatusEnum = StrategyStatus.Enum.MoveStopWinSent;

                            if (strategy.MoveStopWinQty == 0)
                            { } //滿足條件但不減倉
                            else
                            {
                                _appCtrl.CAPOrder.SendAsync(moveStopWinOrder);
                            }

                            saveData = true;
                            AfterStopWin(strategy, true, start);
                        }
                    }
                    else if (strategy.MarketPrice >= strategy.MoveStopWinPrice + strategy.MoveStopWinOffset)
                    {
                        StrategyData moveStopWinOrder = strategy.CreateMoveStopWinOrder();

                        strategy.StatusEnum = StrategyStatus.Enum.MoveStopWinSent;

                        if (strategy.MoveStopWinQty == 0)
                        { } //滿足條件但不減倉
                        else
                        {
                            _appCtrl.CAPOrder.SendAsync(moveStopWinOrder);
                        }

                        saveData = true;
                        AfterStopWin(strategy, true, start);
                    }
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
                _waitToAdd.TryDequeue(out StrategyData strategy);

                StrategyData toRemove = null;

                if (!_dataMap.TryGetValue(strategy.PrimaryKey, out StrategyData _old))
                {
                    _appCtrl.LogTrace(start, $"新增設定|{strategy.ToLog()}", UniqueName);
                }
                else
                {
                    _appCtrl.LogWarn(start, $"重置設定|{strategy.ToLog()}", UniqueName);
                    _dataMap.Remove(strategy.PrimaryKey);
                    toRemove = _old;
                }

                _dataMap.Add(strategy.PrimaryKey, strategy);

                List<StrategyData> list = _dataMap.Values.ToList();
                int index = list.IndexOf(strategy);

                if (index + 1 < list.Count)
                {
                    StrategyData next = list[index + 1];
                    index = _dataCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    try
                    {
                        _dataCollection.Insert(index, strategy);

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
                    SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat);
                }
            }

            bool saveData = false;

            for (int i = _dataCollection.Count - 1; i >= 0; --i)
            {
                try
                {
                    StrategyData data = _dataCollection[i];

                    if (UpdateStatus(data, data.Quote, start))
                    {
                        saveData = true;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                    Notice = ex.Message;
                }
            }

            if (saveData)
            {
                SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat);
            }
        }

        private void AddOrUpdateRule(StrategyData strategy)
        {
            if (_appCtrl.Config.StrategyFolder == null)
            {
                throw new ArgumentNullException($"未設定策略資料夾(Settings.StrategyFolderPath)，無法建立策略|{strategy.ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{strategy.ToLog()}");
            }
            //else if (_dataMap.ContainsKey(strategy.PrimaryKey))
            //{
            //    throw new ArgumentException($"_dataMap.ContainsKey({strategy.PrimaryKey})|{strategy.ToLog()}");
            //}
            //else if (strategy.StatusEnum != StrategyStatus.Enum.Waiting)
            //{
            //    throw new ArgumentException($"{strategy.StatusEnum} != StrategyStatus.Enum.Waiting|{strategy.ToLog()}");
            //}
            else if (_dataMap.TryGetValue(strategy.PrimaryKey, out StrategyData _old) && _old.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"舊設定已啟動或取消，無法更新|{_old.StatusEnum} != StrategyStatus.Enum.Waiting|{_old.ToLog()}");
            }

            _waitToAdd.Enqueue(strategy);
        }

        public void AddRule(StrategyData strategy)
        {
            DateTime start = _appCtrl.StartTrace($"{strategy?.ToLog()}", UniqueName);

            try
            {
                ParentCheck(strategy, false, start);
                AddOrUpdateRule(strategy);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void StartNow(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}", UniqueName);

            StrategyData strategy = this[primaryKey.Replace(" ", string.Empty)];

            if (strategy.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"{strategy.StatusEnum} != StrategyStatus.Enum.Waiting|{strategy.ToLog()}");
            }

            ParentCheck(strategy, true, start);

            StrategyData order = strategy.CreateOrder();
            strategy.StatusEnum = StrategyStatus.Enum.OrderSent;
            _appCtrl.CAPOrder.SendAsync(order);
        }

        public void RecoverSetting(FileInfo fileStrategy = null, FileInfo fileSentOrder = null)
        {
            const string methodName = nameof(RecoverSetting);

            DateTime start = _appCtrl.StartTrace($"{fileStrategy?.FullName}", UniqueName);

            try
            {
                if (_dataMap.Count > 0)
                {
                    return;
                }

                if (fileStrategy == null)
                {
                    if (_appCtrl.Config.StrategyFolder == null)
                    {
                        return;
                    }

                    _appCtrl.Config.StrategyFolder.Refresh();
                    fileStrategy = _appCtrl.Config.StrategyFolder.GetFiles("*.csv").LastOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).Length == _appCtrl.Settings.StrategyFileFormat.Length);
                }

                if (fileStrategy == null)
                {
                    return;
                }

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (StrategyData data in StrategyData.ForeachQuoteFromCSVFile(fileStrategy.FullName, columnNames))
                {
                    try
                    {
                        StrategyData strategy = data.Trim();

                        if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
                        {
                            continue;
                        }

                        strategy = strategy.Reset();
                        strategy.MarketType = _appCtrl.CAPOrder[strategy.FullAccount].MarketType;
                        strategy.Quote = _appCtrl.CAPQuote[strategy.Symbol];
                        strategy.Updater = methodName;
                        strategy.UpdateTime = DateTime.Now;

                        if (decimal.TryParse(strategy.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        AddRule(strategy);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                if (_dataCollection.Count >= nextPK)
                {
                    nextPK = _dataCollection.Count + 1;
                }

                _appCtrl.MainForm.InvokeAsync(delegate { _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{nextPK}"; });
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
