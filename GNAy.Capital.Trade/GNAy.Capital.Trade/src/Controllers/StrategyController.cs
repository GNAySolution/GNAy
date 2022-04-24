﻿using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
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

        public string Notice { get; private set; }

        private readonly ConcurrentQueue<QuoteData> _waitToReset;
        private readonly ConcurrentQueue<string> _waitToCancel;
        private readonly ConcurrentQueue<StrategyData> _waitToAdd;

        private readonly SortedDictionary<string, StrategyData> _strategyMap;
        private readonly ObservableCollection<StrategyData> _strategyCollection;

        public int Count => _strategyMap.Count;
        public StrategyData this[string key] => _strategyMap.TryGetValue(key, out StrategyData data) ? data : null;

        private readonly SortedDictionary<string, StrategyData> _orderDetailMap;
        private readonly ObservableCollection<StrategyData> _orderDetailCollection;

        public StrategyController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            Notice = string.Empty;

            _waitToReset = new ConcurrentQueue<QuoteData>();
            _waitToCancel = new ConcurrentQueue<string>();
            _waitToAdd = new ConcurrentQueue<StrategyData>();

            _strategyMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridStrategyRule.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _strategyCollection = _appCtrl.MainForm.DataGridStrategyRule.SetAndGetItemsSource<StrategyData>();

            _orderDetailMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridOrderDetail.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _orderDetailCollection = _appCtrl.MainForm.DataGridOrderDetail.SetAndGetItemsSource<StrategyData>();
        }

        private StrategyController() : this(null)
        { }

        public StrategyData GetOrderDetail(string primaryKey)
        {
            return _orderDetailMap.TryGetValue(primaryKey, out StrategyData data) ? data : null;
        }

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
                _appCtrl.LogTrace(start, path, UniqueName);

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

        private void MarketCheck(StrategyData data, Market.EGroup qGroup)
        {
            if (qGroup == Market.EGroup.TSE || qGroup == Market.EGroup.OTC || qGroup == Market.EGroup.Emerging)
            {
                if (data.MarketType != Market.EType.Stock)
                {
                    throw new ArgumentException($"qGroup={qGroup}|data.MarketType={data.MarketType}|{data.ToLog()}");
                }
            }
            else if (qGroup == Market.EGroup.Futures || qGroup == Market.EGroup.Option)
            {
                if (data.MarketType != Market.EType.Futures)
                {
                    throw new ArgumentException($"qGroup={qGroup}|data.MarketType={data.MarketType}|{data.ToLog()}");
                }
            }
        }

        private void ParentCheck(StrategyData strategy, bool readyToSend, DateTime start)
        {
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
                throw new ArgumentException($"委託口數({strategy.OrderQty}) <= 0|{strategy.ToLog()}");
            }
            else if (_orderDetailMap.ContainsKey(strategy.PrimaryKey))
            {
                throw new ArgumentException($"_orderDetailMap.ContainsKey({strategy.PrimaryKey})|{strategy.ToLog()}");
            }
            else if (strategy.Quote != null && strategy.Quote.Symbol != strategy.Symbol)
            {
                throw new ArgumentException($"策略關聯報價代碼錯誤|{strategy.Quote.Symbol} != {strategy.Symbol}|{strategy.ToLog()}");
            }
            else if (strategy.Quote == null)
            {
                strategy.Quote = _appCtrl.Capital.GetQuote(strategy.Symbol);
            }

            Market.EGroup qGroup = Market.EGroup.Option;

            if (strategy.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.Capital.GetProductInfo(strategy.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"strategy.Symbol={strategy.Symbol}|{strategy.ToLog()}");
                }
                else if (!string.IsNullOrWhiteSpace(strategy.StopLoss) || !string.IsNullOrWhiteSpace(strategy.StopWinPrice) || !string.IsNullOrWhiteSpace(strategy.MoveStopWinPrice))
                {
                    throw new ArgumentException($"商品 {strategy.Symbol} 無訂閱報價，無法進行策略監控|{strategy.ToLog()}");
                }

                qGroup = (Market.EGroup)short.Parse(product.Item2.bstrMarketNo);
                MarketCheck(strategy, qGroup);
                return;
            }

            qGroup = strategy.Quote.MarketGroupEnum;
            MarketCheck(strategy, qGroup);

            if (strategy.Quote.Simulate.IsSimulating())
            {
                throw new ArgumentException($"strategy.Quote.Simulate.IsSimulating()|{strategy.ToLog()}");
            }

            string orderPriceBefore = strategy.OrderPrice.Trim();
            (string, decimal) orderPriceAfter = OrderPrice.Parse(orderPriceBefore, strategy.Quote.DealPrice, strategy.Quote.Reference, qGroup);

            if (readyToSend)
            {
                strategy.OrderPrice = orderPriceAfter.Item1;
                _appCtrl.LogTrace(start, $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }

            if (!string.IsNullOrWhiteSpace(strategy.StopLoss))
            {
                string stopLossPriceBefore = strategy.StopLoss.Trim();
                (string, decimal) stopLossPriceAfter = OrderPrice.Parse(stopLossPriceBefore, strategy.Quote.DealPrice, strategy.Quote.Reference, qGroup);

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
                    strategy.StopLoss = stopLossPriceAfter.Item1;
                    _appCtrl.LogTrace(start, $"停損價格計算前={stopLossPriceBefore}|計算後={stopLossPriceAfter.Item1}", UniqueName);
                    Notice = $"停損價格計算前={stopLossPriceBefore}|計算後={stopLossPriceAfter.Item1}";
                }
            }

            if (!string.IsNullOrWhiteSpace(strategy.StopWinPrice))
            {
                string stopWinPriceBefore = strategy.StopWinPrice.Trim();

                if (stopWinPriceBefore.Contains("(") || stopWinPriceBefore.Contains(","))
                {
                    string[] cells = stopWinPriceBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    stopWinPriceBefore = cells[0];
                    strategy.StopWinPrice = stopWinPriceBefore;
                    strategy.StopWinQty = int.Parse(cells[1]);
                }

                (string, decimal) stopWinPriceAfter = OrderPrice.Parse(stopWinPriceBefore, strategy.Quote.DealPrice, strategy.Quote.Reference, qGroup);

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
                    strategy.StopWinPrice = stopWinPriceAfter.Item1;
                    _appCtrl.LogTrace(start, $"停利價格計算前={stopWinPriceBefore}|計算後={stopWinPriceAfter.Item1}", UniqueName);
                    Notice = $"停利價格計算前={stopWinPriceBefore}|計算後={stopWinPriceAfter.Item1}";
                }
            }

            if (strategy.StopWinQty == 0)
            {
                //滿足停利點時不減倉
            }
            else if (strategy.StopWinQty > 0)
            {
                throw new ArgumentException($"停利減倉口數({strategy.StopWinQty})應為負值或0|{strategy.ToLog()}");
            }
            else if (strategy.OrderQty + strategy.StopWinQty < 0)
            {
                throw new ArgumentException($"停利減倉口數({strategy.StopWinQty}) > 委託口數({strategy.OrderQty})|{strategy.ToLog()}");
            }

            if (!string.IsNullOrWhiteSpace(strategy.MoveStopWinPrice))
            {
                string moveStopWinPriceBefore = strategy.MoveStopWinPrice.Trim();

                if (moveStopWinPriceBefore.Contains("(") || moveStopWinPriceBefore.Contains(","))
                {
                    string[] cells = moveStopWinPriceBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    moveStopWinPriceBefore = cells[0];
                    strategy.MoveStopWinPrice = moveStopWinPriceBefore;
                    strategy.MoveStopWinQty = int.Parse(cells[1]);
                }

                //(string, decimal) moveStopWinPriceAfter = OrderPrice.Parse(moveStopWinPriceBefore, quote.DealPrice, quote.Reference, qGroup);

                //if (strategy.BSEnum == OrderBS.Enum.Buy)
                //{
                //    if (moveStopWinPriceAfter.Item2 <= orderPriceAfter.Item2)
                //    {
                //        throw new ArgumentException($"移動停利價({moveStopWinPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})");
                //    }
                //}
                //else if (strategy.BSEnum == OrderBS.Enum.Sell)
                //{
                //    if (moveStopWinPriceAfter.Item2 >= orderPriceAfter.Item2)
                //    {
                //        throw new ArgumentException($"移動停利價({moveStopWinPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})");
                //    }
                //}

                //if (readyToSend)
                //{
                //    strategy.MoveStopWinPrice = moveStopWinPriceAfter.Item1;
                //    _appCtrl.LogTrace(start, $"移動停利價格計算前={moveStopWinPriceBefore}|計算後={moveStopWinPriceAfter.Item1}", UniqueName);
                //}
            }

            if (strategy.MoveStopWinQty == 0)
            {
                //滿足移動停利點時不減倉
            }
            else if (strategy.MoveStopWinQty > 0)
            {
                throw new ArgumentException($"移動停利減倉口數({strategy.MoveStopWinQty})應為負值或0|{strategy.ToLog()}");
            }
            else if (strategy.OrderQty + strategy.StopWinQty + strategy.MoveStopWinQty < 0)
            {
                throw new ArgumentException($"移動停利減倉口數({strategy.MoveStopWinQty}) + 停利減倉口數({strategy.StopWinQty}) > 委託口數({strategy.OrderQty})|{strategy.ToLog()}");
            }
        }

        public void OrderCheck(StrategyData order, DateTime start)
        {
            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }
            else if (GetOrderDetail(order.PrimaryKey) != order)
            {
                throw new ArgumentException($"GetOrderDetail({order.PrimaryKey}) != order|{order.ToLog()}");
            }
            else if (order.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"{order.StatusEnum} != StrategyStatus.Enum.Waiting|{order.ToLog()}");
            }
            else if (order.OrderQty <= 0)
            {
                throw new ArgumentException($"委託口數({order.OrderQty}) <= 0|{order.ToLog()}");
            }
            else if (order.OrderData != null || order.StopLossData != null || order.StopWinData != null || order.MoveStopWinData != null)
            {
                throw new ArgumentException($"委託單資料結構異常|{order.OrderData != null}|{order.StopLossData != null}|{order.StopWinData != null}|{order.MoveStopWinData != null}|{order.ToLog()}");
            }
            else if (order.Quote != null && order.Quote.Symbol != order.Symbol)
            {
                throw new ArgumentException($"委託關聯報價代碼錯誤|{order.Quote.Symbol} != {order.Symbol}|{order.ToLog()}");
            }
            else if (order.Quote == null)
            {
                order.Quote = _appCtrl.Capital.GetQuote(order.Symbol);
            }

            Market.EGroup qGroup = Market.EGroup.Option;

            if (order.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.Capital.GetProductInfo(order.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"order.Symbol={order.Symbol}|{order.ToLog()}");
                }

                qGroup = (Market.EGroup)short.Parse(product.Item2.bstrMarketNo);
                MarketCheck(order, qGroup);
                return;
            }

            qGroup = order.Quote.MarketGroupEnum;
            MarketCheck(order, qGroup);

            if (order.Quote.Simulate.IsSimulating())
            {
                throw new ArgumentException($"order.Quote.Simulate.IsSimulating()|{order.ToLog()}");
            }

            order.MarketPrice = order.Quote.DealPrice;

            string orderPriceBefore = order.OrderPrice.Trim();
            (string, decimal) orderPriceAfter = OrderPrice.Parse(orderPriceBefore, order.Quote.DealPrice, order.Quote.Reference, qGroup);

            order.OrderPrice = orderPriceAfter.Item1;
            _appCtrl.LogTrace(start, $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
            Notice = $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}";
        }

        private bool UpdateStatus(StrategyData strategy, QuoteData quote, DateTime start)
        {
            bool saveData = false;

            lock (strategy.SyncRoot)
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
                else if (strategy.StatusEnum == StrategyStatus.Enum.Cancelled ||
                    strategy.StatusEnum == StrategyStatus.Enum.Finished ||
                    strategy.StatusEnum == StrategyStatus.Enum.OrderError)
                { }
                else
                {
                    strategy.MarketPrice = quote.DealPrice;
                    strategy.Updater = nameof(UpdateStatus);
                    strategy.UpdateTime = DateTime.Now;
                }

                if (strategy.StatusEnum == StrategyStatus.Enum.OrderSent || strategy.StatusEnum == StrategyStatus.Enum.OrderReport || strategy.StatusEnum == StrategyStatus.Enum.DealReport)
                {
                    StrategyData orderSent = strategy.OrderData;

                    if (strategy.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (quote.DealPrice >= decimal.Parse(strategy.StopWin) && strategy.StopWinQty > 0)
                        {
                            StrategyData stopWinOrder = strategy.CreateStopWinOrder();

                            strategy.StatusEnum = StrategyStatus.Enum.StopWinSent;
                            _appCtrl.Capital.SendFutureOrderAsync(stopWinOrder);

                            saveData = true;
                        }
                        else if (quote.DealPrice <= decimal.Parse(strategy.StopLoss))
                        {
                            StrategyData stopLossOrder = strategy.CreateStopLossOrder();

                            if (orderSent.DealQty > 0)
                            {
                                stopLossOrder.OrderQty = orderSent.DealQty;
                            }

                            strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                            _appCtrl.Capital.SendFutureOrderAsync(stopLossOrder);

                            saveData = true;
                        }
                    }
                    else if (strategy.BSEnum == OrderBS.Enum.Sell)
                    {
                        if (quote.DealPrice <= decimal.Parse(strategy.StopWin) && strategy.StopWinQty > 0)
                        {
                            StrategyData stopWinOrder = strategy.CreateStopWinOrder();

                            strategy.StatusEnum = StrategyStatus.Enum.StopWinSent;
                            _appCtrl.Capital.SendFutureOrderAsync(stopWinOrder);

                            saveData = true;
                        }
                        else if (quote.DealPrice >= decimal.Parse(strategy.StopLoss))
                        {
                            StrategyData stopLossOrder = strategy.CreateStopLossOrder();

                            if (orderSent.DealQty > 0)
                            {
                                stopLossOrder.OrderQty = orderSent.DealQty;
                            }

                            strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                            _appCtrl.Capital.SendFutureOrderAsync(stopLossOrder);

                            saveData = true;
                        }
                    }
                }
                //else if (strategy.StatusEnum == StrategyStatus.Enum.StopWinDealReport)
                //{
                    //TODO: if (strategy.BSEnum == OrderBS.Enum.Buy)
                    //{
                    //    if (quote.DealPrice <= decimal.Parse(strategy.StopLoss))
                    //    {
                    //        //
                    //    }
                    //}
                    //else if (quote.DealPrice >= decimal.Parse(strategy.StopLoss))
                    //{
                    //    //
                    //}
                //}
            }

            return saveData;
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

                foreach (StrategyData order in _orderDetailMap.Values)
                {
                    if (order.Symbol == quote.Symbol)
                    {
                        order.Quote = quote;
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
                        _appCtrl.LogError(start, strategy.ToLog(), UniqueName);
                    }
                    else if (strategy.StatusEnum == StrategyStatus.Enum.Finished ||
                        strategy.StatusEnum == StrategyStatus.Enum.OrderError ||
                        strategy.StatusEnum == StrategyStatus.Enum.StopLossDealReport ||
                        strategy.StatusEnum == StrategyStatus.Enum.StopLossError ||
                        strategy.StatusEnum == StrategyStatus.Enum.StopWinError ||
                        strategy.StatusEnum == StrategyStatus.Enum.MoveStopWinDealReport ||
                        strategy.StatusEnum == StrategyStatus.Enum.MoveStopWinError)
                    {
                        _appCtrl.LogError(start, $"已停止無法取消|{strategy.ToLog()}", UniqueName);
                        Notice = $"已停止無法取消|{strategy.ToLog()}";
                    }
                    else
                    {
                        strategy.StatusEnum = StrategyStatus.Enum.Cancelled;
                        strategy.Comment = $"手動取消";
                        _appCtrl.LogTrace(start, strategy.ToLog(), UniqueName);
                    }

                    if (_waitToCancel.Count <= 0)
                    {
                        SaveDataAsync();
                    }
                }
                else
                {
                    _appCtrl.LogError(start, $"查無此唯一鍵|{primaryKey}", UniqueName);
                    Notice = $"查無此唯一鍵|{primaryKey}";
                }
            }

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out StrategyData strategy);

                StrategyData toRemove = null;

                if (!_strategyMap.TryGetValue(strategy.PrimaryKey, out StrategyData _old))
                {
                    _appCtrl.LogTrace(start, $"新增設定|{strategy.ToLog()}", UniqueName);
                }
                else
                {
                    _appCtrl.LogWarn(start, $"重置設定|{strategy.ToLog()}", UniqueName);
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

            bool saveData = false;

            foreach (KeyValuePair<string, StrategyData> pair in _strategyMap)
            //Parallel.ForEach(_strategyMap, pair =>
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
                    Notice = ex.Message;
                }
            }

            if (saveData)
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

            try
            {
                Parallel.ForEach(_orderDetailMap.Values, order =>
                {
                    lock (order.SyncRoot)
                    {
                        order.Quote = null;
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

        private void AddOrUpdateRule(StrategyData strategy)
        {
            if (_appCtrl.Config.StrategyFolder == null)
            {
                throw new ArgumentException($"未設定策略資料夾(Settings.StrategyFolderPath)，無法建立策略|{strategy.ToLog()}");
            }

            strategy.PrimaryKey = strategy.PrimaryKey.Replace(" ", string.Empty);

            if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{strategy.ToLog()}");
            }
            //else if (_strategyMap.ContainsKey(strategy.PrimaryKey))
            //{
            //    throw new ArgumentException($"_strategyMap.ContainsKey({strategy.PrimaryKey})|{strategy.ToLog()}");
            //}
            //else if (strategy.StatusEnum != StrategyStatus.Enum.Waiting)
            //{
            //    throw new ArgumentException($"{strategy.StatusEnum} != StrategyStatus.Enum.Waiting|{strategy.ToLog()}");
            //}
            else if (_strategyMap.TryGetValue(strategy.PrimaryKey, out StrategyData _old) && _old.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"舊設定已啟動或取消，無法更新|{_old.StatusEnum} != StrategyStatus.Enum.Waiting|{_old.ToLog()}");
            }

            _waitToAdd.Enqueue(strategy);
        }

        public void AddOrder(StrategyData order)
        {
            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }

            DateTime start = _appCtrl.StartTrace($"{order?.ToLog()}", UniqueName);

            _appCtrl.MainForm.InvokeRequired(delegate
            {
                try
                {
                    _orderDetailMap.Add(order.PrimaryKey, order);
                    _orderDetailCollection.Add(order);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
                finally
                {
                    _appCtrl.EndTrace(start, UniqueName);
                }
            });
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

        public void AddRuleAsync(StrategyData strategy)
        {
            Task.Factory.StartNew(() => AddRule(strategy));
        }

        public void StartNow(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey.Trim()}", UniqueName);

            StrategyData strategy = this[primaryKey.Trim()];

            if (strategy.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"{strategy.StatusEnum} != StrategyStatus.Enum.Waiting|{strategy.ToLog()}");
            }

            ParentCheck(strategy, true, start);

            StrategyData order = strategy.CreateOrder();
            strategy.StatusEnum = StrategyStatus.Enum.OrderSent;
            _appCtrl.Capital.SendFutureOrderAsync(order);
        }

        public void StartFutureStartegy(StrategyData strategy)
        {
            DateTime start = _appCtrl.StartTrace($"{strategy?.ToLog()}", UniqueName);

            try
            {
                ParentCheck(strategy, true, start);
                AddOrUpdateRule(strategy);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 3);

                StartNow(strategy.PrimaryKey);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;

                strategy.StatusEnum = StrategyStatus.Enum.OrderError;
                strategy.OrderReport = ex.Message;
                strategy.Updater = nameof(StartFutureStartegy);
                strategy.UpdateTime = DateTime.Now;
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void StartFutureStartegyAsync(StrategyData strategy)
        {
            Task.Factory.StartNew(() => StartFutureStartegy(strategy));
        }

        public void RecoverSetting(FileInfo file = null)
        {
            DateTime start = _appCtrl.StartTrace($"{file?.FullName}", UniqueName);

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
                        //    _appCtrl.LogError(start, strategy.ToLog(), UniqueName);
                        //}

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
                Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 3);

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
