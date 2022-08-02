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
            else if (data.OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({data.OrderQty}) <= 0|{data.ToLog()}");
            }
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
                (string, decimal) orderPriceAfter = OrderPrice.Parse(data.OrderPriceBefore, data.Quote);

                data.OrderPriceAfter = orderPriceAfter.Item2;
                _appCtrl.LogTrace(start, $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }
            else
            {
                data.OrderPriceAfter = data.MarketPrice;
            }

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
