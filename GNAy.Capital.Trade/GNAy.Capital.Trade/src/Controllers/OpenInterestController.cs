using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class OpenInterestController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly HashSet<string> _strategyAccounts;

        private readonly ConcurrentQueue<string> _waitToAdd;

        private readonly SortedDictionary<string, OpenInterestData> _dataMap;
        private readonly ObservableCollection<OpenInterestData> _dataCollection;

        public int Count => _dataCollection.Count;
        public OpenInterestData this[string key] => _dataMap.TryGetValue(key, out OpenInterestData data) ? data : null;
        public IReadOnlyList<OpenInterestData> DataCollection => _dataCollection;

        public (DateTime, int, string, int) QuerySent { get; private set; }

        public OpenInterestController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OpenInterestController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _strategyAccounts = new HashSet<string>();

            _waitToAdd = new ConcurrentQueue<string>();

            _dataMap = new SortedDictionary<string, OpenInterestData>();
            _appCtrl.MainForm.DataGridOpenInterest.SetHeadersByBindings(OpenInterestData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridOpenInterest.SetAndGetItemsSource<OpenInterestData>();

            QuerySent = (DateTime.Now, -1, string.Empty, -1);
        }

        private OpenInterestController() : this(null)
        { }

        private void StartStrategy(string account, DateTime start)
        {
            try
            {
                if (!_appCtrl.Settings.StartFromOpenInterest || _strategyAccounts.Contains(account) || _appCtrl.Strategy == null || _appCtrl.Strategy.Count <= 0)
                {
                    return;
                }

                SortedDictionary<string, (OpenInterestData, StrategyData)> map = new SortedDictionary<string, (OpenInterestData, StrategyData)>();

                for (int i = _appCtrl.Strategy.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        StrategyData target = _appCtrl.Strategy.DataCollection[i];

                        if (target.FullAccount != account)
                        {
                            continue;
                        }

                        _strategyAccounts.Add(account);

                        if (target.OrderData != null || target.UnclosedQty != 0)
                        {
                            continue;
                        }

                        string key1 = $"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}";
                        string key2 = $"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}_{target.SendRealOrder}";
                        OpenInterestData data = this[key1];

                        if (data == null || data.PositionEnum == OrderPosition.Enum.Close || data.Quantity < target.OrderQty)
                        {
                            continue;
                        }
                        else if (!map.TryGetValue(key2, out (OpenInterestData, StrategyData) _old))
                        {
                            map[key2] = (data, target);
                            continue;
                        }
                        else if (_old.Item2.OrderQty < target.OrderQty)
                        {
                            map[key2] = (data, target);
                            continue;
                        }
                        else if (_old.Item2.OrderQty > target.OrderQty)
                        {
                            continue;
                        }

                        //TODO: 有兩個相同委託量的策略
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                foreach ((OpenInterestData, StrategyData) value in map.Values)
                {
                    try
                    {
                        _appCtrl.Strategy.CreateAndAddOrder(value.Item2, value.Item1);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void CheckStrategy(OpenInterestData data, DateTime start)
        {
            try
            {
                if (!_appCtrl.Settings.SendRealOrder)
                {
                    return;
                }
                else if (_appCtrl.Strategy == null)
                {
                    return;
                }

                StrategyData target = _appCtrl.Strategy.DataCollection.FirstOrDefault(x =>
                    x.StatusEnum != StrategyStatus.Enum.Waiting &&
                    x.StatusEnum != StrategyStatus.Enum.Cancelled &&
                    x.StatusEnum != StrategyStatus.Enum.OrderError &&
                    x.FullAccount == data.Account &&
                    x.Symbol == data.Symbol &&
                    x.BSEnum == data.BSEnum &&
                    x.DayTradeEnum == data.DayTradeEnum &&
                    x.OrderData != null &&
                    x.UnclosedQty != 0 &&
                    x.SendRealOrder);

                if (target == null)
                {
                    return;
                }

                data.Strategy = target.PrimaryKey;

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
                else if (target.UnclosedQty > data.Quantity)
                {
                    _appCtrl.LogError(start, $"計算錯誤，策略未平倉量{target.UnclosedQty} > 庫存{data.Quantity}|{target.ToLog()}", UniqueName);
                    target.UnclosedQty = data.Quantity;
                }
                else if (target.UnclosedQty == data.Quantity && target.DealPrice != data.AveragePrice)
                {
                    _appCtrl.LogTrace(start, $"成交均價校正{target.UnclosedQty} != {data.Quantity}|{target.ToLog()}", UniqueName);
                    target.DealPrice = data.AveragePrice;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private (bool, OpenInterestData) AddOrUpdate(string account, string symbol, OrderBS.Enum bs, OrderDayTrade.Enum dayTrade, string price, string quantity, DateTime start)
        {
            const string methodName = nameof(AddOrUpdate);

            try
            {
                decimal pri = decimal.Parse(price) / 100;
                int qty = int.Parse(quantity);

                if (pri == 0 || qty <= 0)
                {
                    return (false, null);
                }

                OpenInterestData data = this[$"{account}_{symbol}_{bs}_{dayTrade}"];
                bool addNew = data == null;

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

                data.PositionEnum = OrderPosition.Enum.Open;
                data.AveragePrice = pri;
                data.Quantity = qty;
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                CheckStrategy(data, start);

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

                    for (int i = _dataCollection.Count - 1; i >= 0; --i)
                    {
                        OpenInterestData data = _dataCollection[i];

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

                    StartStrategy(QuerySent.Item3, start);

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
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.No, cells[5], cells[3], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.Yes, cells[5], cells[4], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.No, cells[8], cells[6], start);
                AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.Yes, cells[8], cells[7], start);
            }

            for (int i = _dataCollection.Count - 1; i >= 0; --i)
            {
                OpenInterestData data = _dataCollection[i];

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
                data.UnclosedProfit = (data.MarketPrice - data.AveragePrice) * data.Quantity * (data.BSEnum == OrderBS.Enum.Buy ? 1 : -1);
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

                for (int i = QuerySent.Item2 + 1; i < _appCtrl.CAPOrder.DataCollection.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder.DataCollection[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.CAPOrder.GetOpenInterest(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);

                    return QuerySent;
                }

                for (int i = 0; i < _appCtrl.CAPOrder.DataCollection.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder.DataCollection[i];

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
    }
}
