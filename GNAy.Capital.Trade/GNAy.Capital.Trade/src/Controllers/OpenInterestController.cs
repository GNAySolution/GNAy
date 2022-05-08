using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using System;
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

        private readonly SortedDictionary<string, OpenInterestData> _dataMap;
        private readonly ObservableCollection<OpenInterestData> _dataCollection;

        public int Count => _dataMap.Count;
        public OpenInterestData this[string key] => _dataMap.TryGetValue(key, out OpenInterestData data) ? data : null;
        public IReadOnlyList<OpenInterestData> DataCollection => _dataCollection;

        public (DateTime, int, string, int) QuerySent { get; private set; }

        public OpenInterestController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OpenInterestController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _dataMap = new SortedDictionary<string, OpenInterestData>();
            _appCtrl.MainForm.DataGridOpenInterest.SetHeadersByBindings(OpenInterestData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridOpenInterest.SetAndGetItemsSource<OpenInterestData>();

            QuerySent = (DateTime.Now, -1, string.Empty, -1);
        }

        private OpenInterestController() : this(null)
        { }

        public (DateTime, int, string, int) SendNextQuery(DateTime start)
        {
            try
            {
                if (_appCtrl.Capital == null)
                {
                    return QuerySent;
                }
                else if (_appCtrl.Capital.OrderAccCount <= 0)
                {
                    return QuerySent;
                }

                for (int i = QuerySent.Item2 + 1; i < _appCtrl.Capital.OrderAccCollection.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.Capital.OrderAccCollection[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.Capital.GetOpenInterest(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);
                    return QuerySent;
                }

                for (int i = 0; i < _appCtrl.Capital.OrderAccCollection.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.Capital.OrderAccCollection[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.Capital.GetOpenInterest(acc.FullAccount);
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
                        MarketType = _appCtrl.Capital.GetOrderAcc(account).MarketType,
                        Account = account,
                        Quote = _appCtrl.Capital.GetQuote(symbol),
                        Symbol = symbol,
                        BSEnum = bs,
                    };
                }

                data.DealPrice = pri;
                data.DealQty = qty;
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                if (!addNew)
                {
                    return (addNew, data);
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    _dataMap[data.PrimaryKey] = data;

                    List<OpenInterestData> list = _dataMap.Values.ToList();
                    int index = list.IndexOf(data);

                    if (index + 1 < list.Count)
                    {
                        OpenInterestData next = list[index + 1];
                        index = _dataCollection.IndexOf(next);
                    }

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

        public void AddOrUpdateAsync(string raw)
        {
            //完整： (含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,買方成交均價(二位小數),賣方未平倉,賣方當沖未平倉,賣方成交均價(二位小數), LOGIN_ID(V2.13.30新增)
            //格式1：(含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,賣方未平倉,賣方當沖未平倉, LOGIN_ID(V2.13.30新增)
            //格式2：(不含複式單，市場別：TM，可自行計算損益)市場別, 帳號, 商品, 買賣別, 未平倉部位, 當沖未平倉部位, 平均成本(三位小數), 一點價值, 單口手續費, 交易稅(萬分之X), LOGIN_ID(V2.13.30新增)
            //TF,OrderAccount,MTX05,1,0,1652500,0,0,0,UserID

            DateTime start = _appCtrl.StartTrace();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (raw.StartsWith("##"))
                    {
                        //TODO: refresh
                        return;
                    }

                    string[] cells = raw.Split(',');

                    if (cells.Length < 10)
                    {
                        throw new ArgumentException($"cells.Length{cells.Length} < 10|{raw}");
                    }

                    AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.No, cells[5], cells[3], start);
                    AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Buy, OrderDayTrade.Enum.Yes, cells[5], cells[4], start);
                    AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.No, cells[8], cells[6], start);
                    AddOrUpdate(cells[1], cells[2], OrderBS.Enum.Sell, OrderDayTrade.Enum.Yes, cells[8], cells[7], start);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
            });
        }
    }
}
