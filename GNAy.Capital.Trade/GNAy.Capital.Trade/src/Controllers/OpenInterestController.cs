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

        private readonly SortedDictionary<string, OpenInterestData> _oiMap;
        private readonly ObservableCollection<OpenInterestData> _oiCollection;

        public int Count => _oiMap.Count;
        public OpenInterestData this[string key] => _oiMap.TryGetValue(key, out OpenInterestData data) ? data : null;
        public IReadOnlyList<OpenInterestData> OrderDetailCollection => _oiCollection;

        public OpenInterestController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OpenInterestController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _oiMap = new SortedDictionary<string, OpenInterestData>();
            _appCtrl.MainForm.DataGridOpenInterest.SetHeadersByBindings(OpenInterestData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _oiCollection = _appCtrl.MainForm.DataGridOpenInterest.SetAndGetItemsSource<OpenInterestData>();
        }

        private OpenInterestController() : this(null)
        { }

        public void GetOpenInterestAsync(string orderAcc = "", int format = 1)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    DateTime start = _appCtrl.StartTrace($"orderAcc={orderAcc}|format={format}", UniqueName);

            //    try
            //    {
            //        if (string.IsNullOrWhiteSpace(orderAcc))
            //        {
            //            foreach (OrderAccData acc in _orderAccCollection)
            //            {
            //                if (acc.MarketType != Market.EType.Futures)
            //                {
            //                    continue;
            //                }

            //                //nCode=1019|SK_ERROR_QUERY_IN_PROCESSING|GetOpenInterest_Format::1
            //                GetOpenInterestAsync(acc.FullAccount, format);
            //                Thread.Sleep(12 * 1000);
            //            }
            //        }
            //        else
            //        {
            //            int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(UserID, orderAcc, format); //查詢期貨未平倉－可指定回傳格式

            //            if (m_nCode != 0)
            //            {
            //                LogAPIMessage(start, m_nCode);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _appCtrl.LogException(start, ex, ex.StackTrace);
            //    }
            //    finally
            //    {
            //        _appCtrl.EndTrace(start, UniqueName);
            //    }
            //});
        }
    }
}
