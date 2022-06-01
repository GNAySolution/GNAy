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
    public class FuturesRightsController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ObservableCollection<FuturesRightsData> _dataCollection;

        public int Count => _dataCollection.Count;
        public FuturesRightsData this[int index] => _dataCollection[index];

        /// <summary>
        /// (時間,索引,帳號,查詢結果)
        /// </summary>
        public (DateTime, int, string, int) QuerySent { get; private set; }

        public FuturesRightsController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(FuturesRightsController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _appCtrl.MainForm.DataGridFuturesRights.SetHeadersByBindings(FuturesRightsData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridFuturesRights.SetAndGetItemsSource<FuturesRightsData>();

            QuerySent = (DateTime.Now, -1, string.Empty, -1);
        }

        private FuturesRightsController() : this(null)
        { }

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

                    int result = _appCtrl.CAPOrder.GetFuturesRights(acc.FullAccount);
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

                    int result = _appCtrl.CAPOrder.GetFuturesRights(acc.FullAccount);
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
