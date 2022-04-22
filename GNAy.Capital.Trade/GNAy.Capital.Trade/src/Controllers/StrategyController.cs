using GNAy.Capital.Models;
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

        public void OrderCheck(StrategyData order, bool readyToSend, DateTime start)
        {
            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }
            else if (order.Parent != null && GetOrderDetail(order.PrimaryKey) != order)
            {
                throw new ArgumentException($"GetOrderDetail({order.PrimaryKey}) != order|{order.ToLog()}");
            }
            else if (order.Parent != null && order.StatusDes != StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting])
            {
                throw new ArgumentException($"order.StatusDes({order.StatusDes}) != {StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting]}|{order.ToLog()}");
            }
            else if (order.OrderQty <= 0)
            {
                throw new ArgumentException($"委託口數({order.OrderQty}) <= 0|{order.ToLog()}");
            }
            else if (order.Parent != null)
            {
                if (this[order.Parent.PrimaryKey] == null)
                {
                    throw new ArgumentException($"this[{order.Parent.PrimaryKey}] == null|{order.Parent.ToLog()}");
                }
                else if (this[order.Parent.PrimaryKey] != order.Parent)
                {
                    throw new ArgumentException($"this[{order.Parent.PrimaryKey}] != order.Parent|{order.Parent.ToLog()}");
                }

                switch (order.Parent.StatusEnum)
                {
                    case StrategyStatus.Enum.OrderSent:
                    case StrategyStatus.Enum.StopLossSent:
                    case StrategyStatus.Enum.StopWinSent:
                    case StrategyStatus.Enum.MoveStopWinSent:
                        break;
                    default:
                        throw new ArgumentException($"策略單狀態錯誤|order.Parent.StatusEnum={order.Parent.StatusEnum}|{order.Parent.ToLog()}");
                }
            }

            order.Quote = _appCtrl.Capital.GetQuote(order.Symbol);
            Market.EGroup qGroup = Market.EGroup.Options;

            if (order.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.Capital.GetProductInfo(order.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"order.Symbol={order.Symbol}|{order.ToLog()}");
                }

                qGroup = (Market.EGroup)short.Parse(product.Item2.bstrMarketNo);

                if (!string.IsNullOrWhiteSpace(order.StopLoss) || !string.IsNullOrWhiteSpace(order.StopWinPrice) || !string.IsNullOrWhiteSpace(order.MoveStopWinPrice))
                {
                    throw new ArgumentException($"商品 {order.Symbol} 無訂閱報價，無法進行策略監控|{order.ToLog()}");
                }
            }
            else
            {
                qGroup = order.Quote.MarketGroupEnum;

                if (order.Quote.Simulate.IsSimulating())
                {
                    throw new ArgumentException($"order.Quote.Simulate.IsSimulating()|{order.ToLog()}");
                }

                if (readyToSend)
                {
                    order.MarketPrice = order.Quote.DealPrice;
                }

                string orderPriceBefore = order.OrderPrice.Trim();
                (string, decimal) orderPriceAfter = OrderPrice.Parse(orderPriceBefore, order.Quote.DealPrice, order.Quote.Reference, qGroup);

                if (readyToSend)
                {
                    order.OrderPrice = orderPriceAfter.Item1;
                    _appCtrl.LogTrace(start, $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                    Notice = $"委託價格計算前={orderPriceBefore}|計算後={orderPriceAfter.Item1}";
                }

                if (!string.IsNullOrWhiteSpace(order.StopLoss))
                {
                    string stopLossPriceBefore = order.StopLoss.Trim();
                    (string, decimal) stopLossPriceAfter = OrderPrice.Parse(stopLossPriceBefore, order.Quote.DealPrice, order.Quote.Reference, qGroup);

                    if (order.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (stopLossPriceAfter.Item2 >= orderPriceAfter.Item2)
                        {
                            throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{order.ToLog()}");
                        }
                    }
                    else if (stopLossPriceAfter.Item2 <= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{order.ToLog()}");
                    }

                    if (readyToSend)
                    {
                        order.StopLoss = stopLossPriceAfter.Item1;
                        _appCtrl.LogTrace(start, $"停損價格計算前={stopLossPriceBefore}|計算後={stopLossPriceAfter.Item1}", UniqueName);
                        Notice = $"停損價格計算前={stopLossPriceBefore}|計算後={stopLossPriceAfter.Item1}";
                    }
                }

                if (!string.IsNullOrWhiteSpace(order.StopWinPrice))
                {
                    string stopWinPriceBefore = order.StopWinPrice.Trim();

                    if (stopWinPriceBefore.Contains("(") || stopWinPriceBefore.Contains(","))
                    {
                        string[] cells = stopWinPriceBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        stopWinPriceBefore = cells[0];
                        order.StopWinPrice = stopWinPriceBefore;
                        order.StopWinQty = int.Parse(cells[1]);
                    }

                    (string, decimal) stopWinPriceAfter = OrderPrice.Parse(stopWinPriceBefore, order.Quote.DealPrice, order.Quote.Reference, qGroup);

                    if (order.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (stopWinPriceAfter.Item2 <= orderPriceAfter.Item2)
                        {
                            throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{order.ToLog()}");
                        }
                    }
                    else if (stopWinPriceAfter.Item2 >= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{order.ToLog()}");
                    }

                    if (readyToSend)
                    {
                        order.StopWinPrice = stopWinPriceAfter.Item1;
                        _appCtrl.LogTrace(start, $"停利價格計算前={stopWinPriceBefore}|計算後={stopWinPriceAfter.Item1}", UniqueName);
                        Notice = $"停利價格計算前={stopWinPriceBefore}|計算後={stopWinPriceAfter.Item1}";
                    }
                }

                if (order.StopWinQty == 0)
                {
                    //滿足停利點時不減倉
                }
                else if (order.StopWinQty > 0)
                {
                    throw new ArgumentException($"停利減倉口數({order.StopWinQty})應為負值或0|{order.ToLog()}");
                }
                else if (order.OrderQty + order.StopWinQty < 0)
                {
                    throw new ArgumentException($"停利減倉口數({order.StopWinQty}) > 委託口數({order.OrderQty})|{order.ToLog()}");
                }

                if (!string.IsNullOrWhiteSpace(order.MoveStopWinPrice))
                {
                    string moveStopWinPriceBefore = order.MoveStopWinPrice.Trim();

                    if (moveStopWinPriceBefore.Contains("(") || moveStopWinPriceBefore.Contains(","))
                    {
                        string[] cells = moveStopWinPriceBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        moveStopWinPriceBefore = cells[0];
                        order.MoveStopWinPrice = moveStopWinPriceBefore;
                        order.MoveStopWinQty = int.Parse(cells[1]);
                    }

                    //(string, decimal) moveStopWinPriceAfter = OrderPrice.Parse(moveStopWinPriceBefore, quote.DealPrice, quote.Reference, qGroup);

                    //if (order.BSEnum == OrderBS.Enum.Buy)
                    //{
                    //    if (moveStopWinPriceAfter.Item2 <= orderPriceAfter.Item2)
                    //    {
                    //        throw new ArgumentException($"移動停利價({moveStopWinPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})");
                    //    }
                    //}
                    //else if (order.BSEnum == OrderBS.Enum.Sell)
                    //{
                    //    if (moveStopWinPriceAfter.Item2 >= orderPriceAfter.Item2)
                    //    {
                    //        throw new ArgumentException($"移動停利價({moveStopWinPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})");
                    //    }
                    //}

                    //if (readyToSend)
                    //{
                    //    order.MoveStopWinPrice = moveStopWinPriceAfter.Item1;
                    //    _appCtrl.LogTrace(start, $"移動停利價格計算前={moveStopWinPriceBefore}|計算後={moveStopWinPriceAfter.Item1}", UniqueName);
                    //}
                }

                if (order.MoveStopWinQty == 0)
                {
                    //滿足移動停利點時不減倉
                }
                else if (order.MoveStopWinQty > 0)
                {
                    throw new ArgumentException($"移動停利減倉口數({order.MoveStopWinQty})應為負值或0|{order.ToLog()}");
                }
                else if (order.OrderQty + order.StopWinQty + order.MoveStopWinQty < 0)
                {
                    throw new ArgumentException($"移動停利減倉口數({order.MoveStopWinQty}) + 停利減倉口數({order.StopWinQty}) > 委託口數({order.OrderQty})|{order.ToLog()}");
                }
            }

            if (qGroup == Market.EGroup.TSE || qGroup == Market.EGroup.OTC || qGroup == Market.EGroup.Emerging)
            {
                if (order.MarketType != Market.EType.Stock)
                {
                    throw new ArgumentException($"qGroup={qGroup}|order.MarketType={order.MarketType}|{order.ToLog()}");
                }
            }
            else if (qGroup == Market.EGroup.Futures || qGroup == Market.EGroup.Options)
            {
                if (order.MarketType != Market.EType.Futures)
                {
                    throw new ArgumentException($"qGroup={qGroup}|order.MarketType={order.MarketType}|{order.ToLog()}");
                }
            }
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
                else if (strategy.StatusEnum == StrategyStatus.Enum.Waiting ||
                    strategy.StatusEnum == StrategyStatus.Enum.OrderSent ||
                    strategy.StatusEnum == StrategyStatus.Enum.OrderReport ||
                    strategy.StatusEnum == StrategyStatus.Enum.DealReport ||
                    strategy.StatusEnum == StrategyStatus.Enum.StopLossSent ||
                    strategy.StatusEnum == StrategyStatus.Enum.StopLossOrderReport ||
                    strategy.StatusEnum == StrategyStatus.Enum.StopWinSent ||
                    strategy.StatusEnum == StrategyStatus.Enum.StopWinOrderReport ||
                    strategy.StatusEnum == StrategyStatus.Enum.MoveStopWinSent ||
                    strategy.StatusEnum == StrategyStatus.Enum.MoveStopWinOrderReport)
                {
                    strategy.MarketPrice = quote.DealPrice;
                    strategy.Updater = nameof(UpdateStatus);
                    strategy.UpdateTime = DateTime.Now;
                }

                if (strategy.StatusEnum == StrategyStatus.Enum.DealReport || strategy.StatusEnum == StrategyStatus.Enum.StopWinDealReport)
                { }
                else
                {
                    return saveData;
                }

                if (strategy.StatusEnum == StrategyStatus.Enum.DealReport)
                {
                    StrategyData orderSent = _orderDetailMap[$"{strategy.PrimaryKey}_{StrategyStatus.Enum.OrderSent}"];

                    //if (orderSent.StatusDes != StrategyStatus.Description[(int)StrategyStatus.Enum.DealReport])
                    //{
                    //    _appCtrl.LogError(start, $"{orderSent.StatusDes} != {StrategyStatus.Description[(int)StrategyStatus.Enum.DealReport]}|{orderSent.ToLog()}", UniqueName);
                    //}

                    if (strategy.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (quote.DealPrice <= decimal.Parse(strategy.StopLoss))
                        {
                            StrategyData stopLossOrder = strategy.CreateStopLossOrder();
                            stopLossOrder.OrderQty = orderSent.DealQty;

                            strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                            _appCtrl.Capital.SendFutureOrderAsync(stopLossOrder);
                        }
                    }
                    else if (quote.DealPrice >= decimal.Parse(strategy.StopLoss))
                    {
                        StrategyData stopLossOrder = strategy.CreateStopLossOrder();
                        stopLossOrder.OrderQty = orderSent.DealQty;

                        strategy.StatusEnum = StrategyStatus.Enum.StopLossSent;
                        _appCtrl.Capital.SendFutureOrderAsync(stopLossOrder);
                    }
                }
                else if (strategy.StatusEnum == StrategyStatus.Enum.StopWinDealReport)
                {
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
                }

                strategy.Updater = nameof(UpdateStatus);
                strategy.UpdateTime = DateTime.Now;

                //
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
                    else if (strategy.StatusEnum != StrategyStatus.Enum.Waiting)
                    {
                        //TODO
                        _appCtrl.LogError(start, $"已啟動無法取消|{strategy.ToLog()}", UniqueName);
                        Notice = $"已啟動無法取消|{strategy.ToLog()}";
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

                        if (_waitToAdd.Count <= 0)
                        {
                            SaveDataAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                        Notice = ex.Message;
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
                throw new ArgumentException("未設定策略資料夾(Settings.StrategyFolderPath)，無法建立策略");
            }

            strategy.PrimaryKey = strategy.PrimaryKey.Replace(" ", string.Empty);

            if (string.IsNullOrWhiteSpace(strategy.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵");
            }
            else if (_strategyMap.ContainsKey(strategy.PrimaryKey))
            {
                throw new ArgumentException($"_strategyMap.ContainsKey({strategy.PrimaryKey})");
            }
            else if (strategy.StatusDes != StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting])
            {
                throw new ArgumentException($"strategy.StatusDes({strategy.StatusDes}) != {StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting]}");
            }

            _waitToAdd.Enqueue(strategy);
        }

        public void AddOrder(StrategyData order)
        {
            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }

            _appCtrl.MainForm.InvokeRequired(delegate
            {
                DateTime start = _appCtrl.StartTrace();

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
            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderCheck(strategy, false, start);
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

        public void StartFutureStartegy(StrategyData strategy)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderCheck(strategy, true, start);
                AddOrUpdateRule(strategy);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 2);

                StrategyData order = strategy.CreateOrder();
                strategy.StatusEnum = StrategyStatus.Enum.OrderSent;
                _appCtrl.Capital.SendFutureOrderAsync(order);
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
                Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 2);

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
