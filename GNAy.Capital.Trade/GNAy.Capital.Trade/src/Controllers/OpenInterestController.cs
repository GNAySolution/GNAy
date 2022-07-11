using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class OpenInterestController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly HashSet<string> _strategyKeys;

        private readonly ConcurrentQueue<string> _waitToAdd;

        private readonly SortedDictionary<string, OpenInterestData> _dataMap;
        private readonly ObservableCollection<OpenInterestData> _dataCollection;

        public int Count => _dataCollection.Count;
        public OpenInterestData this[string key] => _dataMap.TryGetValue(key, out OpenInterestData data) ? data : null;
        public OpenInterestData this[int index] => _dataCollection[index];

        /// <summary>
        /// (時間,索引,帳號,查詢結果)
        /// </summary>
        public (DateTime, int, string, int) QuerySent { get; private set; }

        public OpenInterestController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OpenInterestController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _strategyKeys = new HashSet<string>();

            _waitToAdd = new ConcurrentQueue<string>();

            _dataMap = new SortedDictionary<string, OpenInterestData>();
            _appCtrl.MainForm.DataGridOpenInterest.SetHeadersByBindings(OpenInterestData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridOpenInterest.SetViewAndGetObservation<OpenInterestData>();

            QuerySent = (DateTime.Now, -1, string.Empty, -1);
        }

        private OpenInterestController() : this(null)
        { }

        public void FilterFullAccount(string acc, DateTime start)
        {
            _appCtrl.LogTrace(start, $"acc={acc}", UniqueName);

            _strategyKeys.Add(acc);
        }

        private void StartStrategy(OpenInterestData data, StrategyData target, DateTime start)
        {
            //TODO
        }

        private void StartStrategy(OpenInterestData data, DateTime start)
        {
            try
            {
                if (_strategyKeys.Contains(data.PrimaryKey))
                {
                    return;
                }
                else if (data.PositionEnum == OrderPosition.Enum.Close || !_appCtrl.Settings.StrategyFromOpenInterest || _appCtrl.Strategy == null || _appCtrl.Strategy.Count <= 0)
                {
                    return;
                }

                _appCtrl.LogTrace(start, $"{data.ToLog()}", UniqueName);

                string key1 = data.PrimaryKey;
                SortedDictionary<string, (OpenInterestData, StrategyData)> mapA = new SortedDictionary<string, (OpenInterestData, StrategyData)>();

                for (int i = _appCtrl.Strategy.Count - 1; i >= 0; --i) //走訪策略，找出最匹配庫存的策略
                {
                    try
                    {
                        StrategyData target = _appCtrl.Strategy[i];

                        if (target.FullAccount != data.Account)
                        {
                            continue;
                        }
                        else if (_appCtrl.Settings.StrategyNotForOpenInterest.SplitWithoutWhiteSpace(',').FirstOrDefault(x => target.PrimaryKey.StartsWith(x)) != null)
                        {
                            continue;
                        }
                        else if ($"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}" != key1)
                        {
                            continue;
                        }

                        _strategyKeys.Add(key1);

                        string key2 = $"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}_{target.SendRealOrder}";

                        if (data.Quantity < target.OrderQty)
                        {
                            continue;
                        }
                        else if (!mapA.TryGetValue(key2, out (OpenInterestData, StrategyData) _old))
                        {
                            mapA[key2] = (data, target);
                            continue;
                        }
                        else if (_old.Item2.OrderQty < target.OrderQty)
                        {
                            mapA[key2] = (data, target);
                            continue;
                        }
                        else if (_old.Item2.OrderQty > target.OrderQty)
                        {
                            continue;
                        }

                        throw new NotSupportedException($"不支援重複相同委託量({target.OrderQty})的策略|{key2}|{target.ToLog()}");
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                //if (string.IsNullOrWhiteSpace(filterStrategy))
                //{
                    foreach ((OpenInterestData, StrategyData) value in mapA.Values)
                    {
                        try
                        {
                            //if (Count > 1 && !_appCtrl.Settings.SendRealOrder && value.Item2.SendRealOrder)
                            //{
                            //    filterStrategy = value.Item2.PrimaryKey;
                            //    break;
                            //}

                            _appCtrl.Strategy.StartNow(value.Item2, value.Item1);
                        }
                        catch (Exception ex)
                        {
                            _appCtrl.LogException(start, ex, ex.StackTrace);
                        }
                    }

                    //if (string.IsNullOrWhiteSpace(filterStrategy))
                    //{
                        return;
                    //}
                //}

                //SortedDictionary<string, (OpenInterestData, StrategyData)> mapB = new SortedDictionary<string, (OpenInterestData, StrategyData)>();

                //for (int i = _appCtrl.Strategy.Count - 1; i >= 0; --i)
                //{
                //    try
                //    {
                //        StrategyData target = _appCtrl.Strategy[i];

                //        if (target.FullAccount != data.Account)
                //        {
                //            continue;
                //        }

                //        string key2 = $"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}_{bool.TrueString}";

                //        if (mapA.TryGetValue(key2, out (OpenInterestData, StrategyData) value) && value.Item2.OrderQty == target.OrderQty)
                //        {
                //            mapB[target.PrimaryKey] = (data, target);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        _appCtrl.LogException(start, ex, ex.StackTrace);
                //    }
                //}

                //if (mapB.Count == 1)
                //{
                //    return;
                //}

                //List<string> list = mapB.Keys.ToList();
                //int index = list.IndexOf(filterStrategy);

                //if (index < 0)
                //{
                //    return;
                //}

                //index = index + 1 >= list.Count ? 0 : index + 1;

                //(OpenInterestData, StrategyData) found = mapB[list[index]];

                //_appCtrl.Strategy.StartNow(found.Item2, found.Item1);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void CheckStrategy(OpenInterestData data, DateTime start)
        {
            if (!_appCtrl.Settings.SendRealOrder)
            {
                return;
            }
            else if (_appCtrl.Strategy == null)
            {
                return;
            }

            data.Strategy = string.Empty;

            for (int i = _appCtrl.Strategy.Count - 1; i >= 0; --i)
            {
                try
                {
                    StrategyData target = _appCtrl.Strategy[i];

                    if (target.StatusEnum == StrategyStatus.Enum.Waiting || target.StatusEnum == StrategyStatus.Enum.Cancelled || target.StatusEnum == StrategyStatus.Enum.Monitoring)
                    {
                        continue;
                    }
                    else if (target.FullAccount != data.Account || target.Symbol != data.Symbol || target.BSEnum != data.BSEnum || target.DayTradeEnum != data.DayTradeEnum)
                    {
                        continue;
                    }
                    else if (target.OrderData == null || target.UnclosedQty == 0 || !target.SendRealOrder)
                    {
                        continue;
                    }

                    data.Strategy = string.IsNullOrWhiteSpace(data.Strategy) ? target.PrimaryKey : $"{target.PrimaryKey},{data.Strategy}";

                    if (data.PositionEnum == OrderPosition.Enum.Close)
                    {
                        _appCtrl.LogError(start, $"計算錯誤，策略未平倉量{target.UnclosedQty} != 0|{target.ToLog()}", UniqueName);
                        target.UnclosedQty = 0;
                    }
                    else if (target.UnclosedQty < 0)
                    {
                        _appCtrl.LogError(start, $"計算錯誤，策略未平倉量{target.UnclosedQty} < 0|{target.ToLog()}", UniqueName);
                        target.UnclosedQty = 0;
                    }
                    //else if (target.UnclosedQty > data.Quantity)
                    //{
                    //    _appCtrl.LogWarn(start, $"計算錯誤(可能是委託和查詢時間太接近)，策略未平倉量{target.UnclosedQty} > 庫存{data.Quantity}|{target.ToLog()}", UniqueName);
                    //    //target.UnclosedQty = data.Quantity;
                    //}
                    //else if (target.UnclosedQty == data.Quantity && target.DealPrice != data.AveragePrice)
                    //{
                    //    _appCtrl.LogTrace(start, $"成交均價校正{target.DealPrice} != {data.AveragePrice}|{target.ToLog()}", UniqueName);
                    //    target.DealPrice = data.AveragePrice;
                    //}
                    else if (target.DealPrice != data.AveragePrice)
                    {
                        _appCtrl.LogTrace(start, $"成交均價校正{target.DealPrice} != {data.AveragePrice}|{target.ToLog()}", UniqueName);
                        target.DealPrice = data.AveragePrice;
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            }
        }

        private (bool, OpenInterestData) AddOrUpdate(string account, string symbol, OrderBS.Enum bs, OrderDayTrade.Enum dayTrade, string price, string quantity1, string quantity2, DateTime start, [CallerMemberName] string memberName = "")
        {
            try
            {
                decimal pri = decimal.Parse(price) / 100;
                int qty1 = int.Parse(quantity1);
                int qty2 = int.Parse(quantity2);
                int qty = dayTrade == OrderDayTrade.Enum.No ? qty1 - qty2 : qty2;

                if (pri == 0 || qty <= 0)
                {
                    return (false, null);
                }

                OpenInterestData data = this[$"{account}_{symbol}_{bs}_{dayTrade}"];
                bool addNew = data == null;
                bool reopened = false;

                if (addNew)
                {
                    data = new OpenInterestData()
                    {
                        MarketType = _appCtrl.CAPOrder[account].MarketType,
                        Account = account,
                        Quote = _appCtrl.CAPQuote[symbol],
                        Symbol = symbol,
                        BSEnum = bs,
                        DayTradeEnum = dayTrade,
                    };
                }
                else if (data.PositionEnum == OrderPosition.Enum.Close && data.AveragePrice != pri)
                {
                    reopened = true;
                }

                data.PositionEnum = OrderPosition.Enum.Open;
                data.AveragePrice = pri;
                data.Quantity = qty;
                data.Updater = memberName;
                data.UpdateTime = DateTime.Now;

                CheckStrategy(data, start);

                if (!_appCtrl.Settings.SendRealOrder && (reopened || string.IsNullOrWhiteSpace(data.Strategy)))
                {
                    _strategyKeys.Remove(data.PrimaryKey);
                    StartStrategy(data, start);
                }
                
                if (!addNew)
                {
                    return (addNew, data);
                }

                _dataMap[data.PrimaryKey] = data;

                List<OpenInterestData> list = _dataMap.Values.ToList();
                int index = list.IndexOf(data);

                if (index + 1 < list.Count)
                {
                    OpenInterestData next = list[index + 1];
                    index = _dataCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    try
                    {
                        _dataCollection.Insert(index, data);
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

                return (addNew, data);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            return (false, null);
        }

        public void UpdateStatus(DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out string raw);

                if (raw.StartsWith("##"))
                {
                    if (QuerySent.Item4 != 0)
                    {
                        continue;
                    }

                    foreach (OpenInterestData data in _dataMap.Values)
                    {
                        if (data.UpdateTime >= QuerySent.Item1 || data.Account != QuerySent.Item3 || data.PositionEnum == OrderPosition.Enum.Close)
                        {
                            continue;
                        }

                        data.PositionEnum = OrderPosition.Enum.Close;
                        data.UnclosedProfit = 0;
                        data.Updater = methodName;
                        data.UpdateTime = DateTime.Now;

                        _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                    }

                    if (_appCtrl.Settings.SendRealOrder && !_strategyKeys.Contains(QuerySent.Item3))
                    {
                        foreach (OpenInterestData data in _dataMap.Values)
                        {
                            if (data.Account != QuerySent.Item3 || data.PositionEnum == OrderPosition.Enum.Close)
                            {
                                continue;
                            }

                            StartStrategy(data, start);
                        }
                    }

                    continue;
                }

                string[] cells = raw.Split(',');

                if (cells.Length < 10)
                {
                    throw new ArgumentException($"cells.Length({cells.Length}) < 10|{raw}");
                }

                //完整： (含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,買方成交均價(二位小數),賣方未平倉,賣方當沖未平倉,賣方成交均價(二位小數), LOGIN_ID(V2.13.30新增)
                //格式1：(含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,賣方未平倉,賣方當沖未平倉, LOGIN_ID(V2.13.30新增)
                //格式2：(不含複式單，市場別：TM，可自行計算損益)市場別, 帳號, 商品, 買賣別, 未平倉部位, 當沖未平倉部位, 平均成本(三位小數), 一點價值, 單口手續費, 交易稅(萬分之X), LOGIN_ID(V2.13.30新增)
                //TF,OrderAccount,MTX05,1,0,1652500,0,0,0,UserID
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.No, cells[5], cells[3], cells[4], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.Yes, cells[5], cells[3], cells[4], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.No, cells[8], cells[6], cells[7], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.Yes, cells[8], cells[6], cells[7], start);
            }

            foreach (OpenInterestData data in _dataMap.Values)
            {
                if (data.Quote == null || data.Quote.DealPrice == 0 || data.Quote.Simulate != QuoteData.RealTrade)
                {
                    continue;
                }
                else if (data.PositionEnum == OrderPosition.Enum.Close)
                {
                    if (data.UnclosedProfit != 0)
                    {
                        data.UnclosedProfit = 0;
                        data.Updater = methodName;
                        data.UpdateTime = DateTime.Now;

                        _appCtrl.LogTrace(start, data.ToLog(), UniqueName);
                    }
                    continue;
                }

                data.MarketPrice = data.Quote.DealPrice;
                data.UnclosedProfit = (data.MarketPrice - data.AveragePrice) * data.Quantity * data.ProfitDirection;
                data.Updater = methodName;
                //data.UpdateTime = DateTime.Now;
            }
        }

        public void AddOrUpdateAsync(string raw)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (raw.IndexOf("NO DATA", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return;
                }

                _waitToAdd.Enqueue(raw);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        public (DateTime, int, string, int) SendNextQuery(DateTime start)
        {
            try
            {
                if (_appCtrl.CAPCenter == null)
                {
                    return QuerySent;
                }
                else if (_appCtrl.CAPOrder.Count <= 0)
                {
                    return QuerySent;
                }

                for (int i = QuerySent.Item2 + 1; i < _appCtrl.CAPOrder.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.CAPOrder.GetOpenInterest(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);

                    return QuerySent;
                }

                for (int i = 0; i < _appCtrl.CAPOrder.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.CAPOrder.GetOpenInterest(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);

                    return QuerySent;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            QuerySent = (DateTime.Now, -1, string.Empty, -1);

            return QuerySent;
        }

        public OpenInterestData StartStrategies(OpenInterestData data, string keys)
        {
            if (data.PositionEnum == OrderPosition.Enum.Close || !_appCtrl.Settings.StrategyFromOpenInterest || string.IsNullOrWhiteSpace(keys))
            {
                return null;
            }

            DateTime start = _appCtrl.StartTrace($"keys={keys}|{data.ToLog()}", UniqueName);

            try
            {
                int qtyTotal = 0;
                List<StrategyData> targets = new List<StrategyData>();

                foreach (string key in keys.ForeachSortedSet(','))
                {
                    StrategyData target = _appCtrl.Strategy[key];

                    qtyTotal += target.OrderQty;
                    targets.Add(target);
                }

                if (qtyTotal > data.Quantity)
                {
                    throw new ArgumentException($"qtyTotal({qtyTotal}) > data.Quantity({data.Quantity})|keys={keys}|{data.ToLog()}");
                }

                foreach (string key in data.Strategy.SplitWithoutWhiteSpace(','))
                {
                    _appCtrl.Strategy.ResetToZero(key);
                    _strategyKeys.Remove(key);
                }

                data.Strategy = string.Empty;

                foreach (StrategyData target in targets)
                {
                    StartStrategy(data, target, start);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            return null;
        }
    }
}
