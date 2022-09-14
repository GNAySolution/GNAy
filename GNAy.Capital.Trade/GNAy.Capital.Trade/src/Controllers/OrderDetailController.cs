using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using SKCOMLib;
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
    public class OrderDetailController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ConcurrentQueue<string> _waitToAdd;

        private readonly SortedDictionary<string, StrategyData> _dataMap;
        private readonly ObservableCollection<StrategyData> _dataCollection;

        public int Count => _dataCollection.Count;
        public StrategyData this[string key] => _dataMap.TryGetValue(key, out StrategyData data) ? data : null;
        public IReadOnlyList<StrategyData> DataCollection => _dataCollection;

        public string Notice { get; private set; }

        public OrderDetailController(in AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OrderDetailController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<string>();

            _dataMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridOrderDetail.SetColumns(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridOrderDetail.SetViewAndGetObservation<StrategyData>();

            Notice = string.Empty;
        }

        private OrderDetailController() : this(null)
        { }

        private void CancelFuturesLimitStopWin(in StrategyData data, in StrategyData parent, in DateTime start)
        {
            if (parent == null || data.PositionEnum == OrderPosition.Enum.Open || data == parent.OrderData)
            {
                return;
            }
            else if (parent.OrdersSeqNoQueue.Count <= 0)
            {
                return;
            }
            else if (data == parent.StopLossData)
            { }
            else if (data == parent.MarketClosingData)
            { }
            else if (parent.StopWin1Offset == 0 && parent.StopWin1Qty < 0)
            {
                if (data != parent.StopWin1Data)
                {
                    return;
                }
            }
            else if (parent.StopWin2Offset == 0 && parent.StopWin2Qty < 0)
            {
                if (data != parent.StopWin2Data)
                {
                    return;
                }
            }
            else
            {
                _appCtrl.LogError(start, $"不明委託|{parent.OrdersSeqNoQueue.Count}={parent.OrdersSeqNoQueue.Count}|{data.ToLog()}", UniqueName);

                return;
            }

            int dealQty = 0;

            while (parent.OrdersSeqNoQueue.Count > 0)
            {
                string seqNo = parent.OrdersSeqNoQueue.Dequeue();

                if (_appCtrl.CAPOrder.CancelBySeqNo(parent.FullAccount, seqNo, start) != 0)
                {
                    ++dealQty;
                }
            }

            parent.OrdersSeqNos = string.Join(",", parent.OrdersSeqNoQueue);

            if (dealQty == 0)
            {
                return;
            }

            decimal closedProfit = (parent.StopWinPriceAAfterRaw - parent.DealPrice) * dealQty * parent.OrderData.ProfitDirection;
            int orderQty = data.OrderQty - dealQty;
            int unclosedQty = parent.UnclosedQty - dealQty;

            _appCtrl.LogTrace(start, $"已實現損益估計({closedProfit})=({parent.StopWinPriceAAfterRaw}-{parent.DealPrice})*{dealQty}*{parent.OrderData.ProfitDirection}|{parent.ToLog()}", UniqueName);

            if (data == parent.StopLossData)
            {
                //
            }
            else if (data == parent.MarketClosingData)
            {
                //TODO: 先確保減倉別減過頭
                ////負值減倉
                //order.OrderQty = (qty < 0) ? qty * -1 : UnclosedQty - qty;

                //if (order.OrderQty <= 0)
                //{
                //    //正值留倉
                //    order.OrderQty = 0;
                //}
                //else if (order.OrderQty > UnclosedQty)
                //{
                //    order.OrderQty = UnclosedQty;
                //    MarketClosingData = order;
                //}
            }
            else if (data == parent.StopWin1Data || data == parent.StopWin2Data)
            {
                //
            }

            data.OrderQty = orderQty;

            parent.ClosedProfit += closedProfit;
            parent.UnclosedQty = unclosedQty;
            parent.SumClosedProfit(closedProfit);

            if (orderQty < 0)
            {
                _appCtrl.LogError(start, $"剩餘委託量錯誤|{orderQty}={data.OrderQty}-{dealQty}|{data.ToLog()}", UniqueName);
                data.OrderQty = 0;
            }
            else
            {
                _appCtrl.LogTrace(start, $"剩餘委託量|{orderQty}={data.OrderQty}-{dealQty}|{data.ToLog()}", UniqueName);
            }

            if (unclosedQty < 0)
            {
                _appCtrl.LogError(start, $"限價停利成交量錯誤|{unclosedQty}={parent.UnclosedQty}-{dealQty}|{parent.ToLog()}", UniqueName);
                parent.UnclosedQty = 0;
            }
            else
            {
                _appCtrl.LogTrace(start, $"限價停利成交|{unclosedQty}={parent.UnclosedQty}-{dealQty}|{parent.ToLog()}", UniqueName);
            }
        }

        public void Check(in StrategyData data, in DateTime start, [CallerMemberName] in string memberName = "")
        {
            data.Trim();

            if (string.IsNullOrWhiteSpace(data.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{data.ToLog()}");
            }
            else if (this[data.PrimaryKey] != data)
            {
                throw new ArgumentException($"this[{data.PrimaryKey}] != data|{data.ToLog()}");
            }
            else if (data.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"{data.StatusEnum} != StrategyStatus.Enum.Waiting|{data.ToLog()}");
            }
            //else if (data.OrderQty <= 0)
            //{
            //    throw new ArgumentException($"委託量({data.OrderQty}) <= 0|{data.ToLog()}");
            //}
            else if (data.OrderData != null || data.StopLossData != null || data.StopWin1Data != null || data.StopWin2Data != null || data.MarketClosingData != null)
            {
                throw new ArgumentException($"委託單資料結構異常|{data.OrderData != null}|{data.StopLossData != null}|{data.StopWin1Data != null}|{data.StopWin2Data != null}|{data.MarketClosingData != null}|{data.ToLog()}");
            }

            data.SendRealOrder = _appCtrl.Settings.SendRealOrder && data.SendRealOrder;

            if (data.Quote == null)
            {
                data.Quote = _appCtrl.CAPQuote[data.Symbol];
            }

            if (data.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(data.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"{nameof(StrategyData.Symbol)}={data.Symbol}|{data.ToLog()}");
                }

                _appCtrl.Strategy.MarketCheck(data, _appCtrl.CAPQuote.CreateOrUpdate(product.Item2));

                return;
            }
            else if (data.Quote.Symbol != data.Symbol)
            {
                throw new ArgumentException($"委託關聯報價代碼錯誤|{data.Quote.Symbol} != {data.Symbol}|{data.ToLog()}");
            }

            _appCtrl.Strategy.MarketCheck(data, data.Quote);

            StrategyData parent = data.Parent;

            data.MarketPrice = data.Quote.DealPrice;

            if (parent == null)
            {
                (string, decimal) orderPriceAfter = OrderPrice.Parse(data.OrderPriceBefore, data.Quote, data.DealPrice);

                data.OrderPriceAfter = orderPriceAfter.Item2;
                _appCtrl.LogTrace(start, $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }
            else
            {
                data.OrderPriceAfter = data.MarketPrice;
            }

            CancelFuturesLimitStopWin(data, parent, start);

            data.DealPrice = data.OrderPriceAfter;
            data.DealQty = data.OrderQty;

            if (data.PositionEnum == OrderPosition.Enum.Open)
            {
                data.ClosedProfit = 0;
                data.UnclosedQty = data.DealQty;
                data.UnclosedProfit = 0;
            }

            if (parent != null)
            {
                if (data == parent.OrderData)
                {
                    parent.DealPrice = data.DealPrice;
                    parent.ClosedProfit += data.ClosedProfit;
                    parent.UnclosedQty = data.UnclosedQty;
                }
                else if (data == parent.StopLossData || data == parent.StopWin1Data || data == parent.StopWin2Data || data == parent.MarketClosingData)
                {
                    data.ClosedProfit = (data.DealPrice - parent.DealPrice) * data.DealQty * parent.OrderData.ProfitDirection;
                    data.UnclosedQty = parent.UnclosedQty - data.DealQty;
                    data.UnclosedProfit = (data.DealPrice - parent.DealPrice) * data.UnclosedQty * parent.OrderData.ProfitDirection;

                    parent.ClosedProfit += data.ClosedProfit;
                    parent.UnclosedQty = data.UnclosedQty;
                    parent.UnclosedProfit = (parent.MarketPrice - parent.DealPrice) * parent.UnclosedQty * parent.ProfitDirection;

                    if (data.DealQty != 0)
                    {
                        parent.SumClosedProfit(data.ClosedProfit);
                    }
                }

                parent.Updater = memberName;
                parent.UpdateTime = DateTime.Now;
            }
        }

        public void Add(StrategyData data)
        {
            if (string.IsNullOrWhiteSpace(data.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{data.ToLog()}");
            }

            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}", UniqueName);

            _dataMap.Add(data.PrimaryKey, data);

            _appCtrl.MainForm.InvokeSync(delegate
            {
                try
                {
                    _dataCollection.Add(data);

                    _appCtrl.MainForm.DataGridOrderDetail.ScrollToBorderEnd();
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
    }
}
