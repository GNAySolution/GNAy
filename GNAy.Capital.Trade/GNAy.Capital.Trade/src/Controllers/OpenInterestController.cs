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

        public void AddOrUpdateAsync(string raw)
        {
            //完整： (含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,買方成交均價(二位小數),賣方未平倉,賣方當沖未平倉,賣方成交均價(二位小數), LOGIN_ID(V2.13.30新增)
            //格式1：(含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,賣方未平倉,賣方當沖未平倉, LOGIN_ID(V2.13.30新增)
            //格式2：(不含複式單，市場別：TM，可自行計算損益)市場別, 帳號, 商品, 買賣別, 未平倉部位, 當沖未平倉部位, 平均成本(三位小數), 一點價值, 單口手續費, 交易稅(萬分之X), LOGIN_ID(V2.13.30新增)
            //TF,OrderAccount,MTX05,1,0,1652500,0,0,0,UserID
        }
    }
}
