using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using SKCOMLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public OrderDetailController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OrderDetailController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<string>();

            _dataMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridOrderDetail.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridOrderDetail.SetAndGetItemsSource<StrategyData>();

            Notice = string.Empty;
        }

        private OrderDetailController() : this(null)
        { }

        public void Check(StrategyData order, DateTime start)
        {
            order = order.Trim();

            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }
            else if (this[order.PrimaryKey] != order)
            {
                throw new ArgumentException($"this[{order.PrimaryKey}] != order|{order.ToLog()}");
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
                order.Quote = _appCtrl.CAPQuote[order.Symbol];
            }

            if (order.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(order.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"order.Symbol={order.Symbol}|{order.ToLog()}");
                }

                _appCtrl.Strategy.MarketCheck(order, _appCtrl.CAPQuote.CreateOrUpdate(product.Item2));
                return;
            }

            _appCtrl.Strategy.MarketCheck(order, order.Quote);

            if (order.Quote.Simulate != QuoteData.RealTrade)
            {
                throw new ArgumentException($"order.Quote.Simulate != QuoteData.RealTrade|{order.ToLog()}");
            }

            order.MarketPrice = order.Quote.DealPrice;

            (string, decimal) orderPriceAfter = OrderPrice.Parse(order.OrderPriceBefore, order.Quote);

            order.OrderPriceAfter = orderPriceAfter.Item2;
            _appCtrl.LogTrace(start, $"委託價計算前={order.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
            Notice = $"委託價計算前={order.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
        }

        public void Add(StrategyData order)
        {
            if (string.IsNullOrWhiteSpace(order.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{order.ToLog()}");
            }

            DateTime start = _appCtrl.StartTrace($"{order?.ToLog()}", UniqueName);

            _dataMap.Add(order.PrimaryKey, order);

            _appCtrl.MainForm.InvokeSync(delegate
            {
                try
                {
                    _dataCollection.Add(order);
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
