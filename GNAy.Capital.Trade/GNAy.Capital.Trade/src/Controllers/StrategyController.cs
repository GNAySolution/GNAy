using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private readonly ConcurrentQueue<StrategyData> _waitToAdd;

        private readonly SortedDictionary<string, StrategyData> _dataMap;
        private readonly ObservableCollection<StrategyData> _dataCollection;

        public int Count => _dataCollection.Count;
        public StrategyData this[string key] => _dataMap.TryGetValue(key, out StrategyData data) ? data : null;
        public StrategyData this[int index] => _dataCollection[index];
        public IReadOnlyList<StrategyData> DataCollection => _dataCollection;

        private readonly Dictionary<string, decimal> _marketPriceSnapshot;

        public string RecoverFile { get; private set; }

        public decimal ProfitTotal { get; private set; }
        public decimal ProfitTotalBest { get; private set; }
        public bool ProfitTotalStopWinTouched { get; private set; }
        public bool ProfitTotalStopWinClosed { get; private set; }

        public string Notice { get; private set; }

        public StrategyController(in AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(StrategyController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<StrategyData>();

            _dataMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridStrategyRule.SetColumns(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridStrategyRule.SetViewAndGetObservation<StrategyData>();

            _marketPriceSnapshot = new Dictionary<string, decimal>();

            RecoverFile = string.Empty;

            ProfitTotal = 0;
            ProfitTotalBest = 0;
            ProfitTotalStopWinTouched = false;
            ProfitTotalStopWinClosed = false;

            Notice = string.Empty;
        }

        private StrategyController() : this(null)
        { }

        public void SaveData(in IEnumerable<StrategyData> dataCollection, in DirectoryInfo dir, in string fileFormat)
        {
            DateTime start = _appCtrl.StartTrace($"{dir?.FullName}|{fileFormat}", UniqueName);

            try
            {
                string path = Path.Combine(dir.FullName, $"{start.ToString(fileFormat)}.csv");
                _appCtrl.LogTrace(start, path, UniqueName);

                using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.WriteLine(StrategyData.CSVColumnNames);

                    foreach (StrategyData data in dataCollection)
                    {
                        try
                        {
                            sw.WriteLine(data.ToCSVString());
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

        public void MarketCheck(in StrategyData data, in QuoteData quote)
        {
            if (quote.MarketGroupEnum == Market.EGroup.TSE || quote.MarketGroupEnum == Market.EGroup.OTC || quote.MarketGroupEnum == Market.EGroup.Emerging)
            {
                if (data.MarketType != Market.EType.Stock)
                {
                    throw new ArgumentException($"{nameof(QuoteData.MarketGroupEnum)}={quote.MarketGroupEnum}|{nameof(StrategyData.MarketType)}={data.MarketType}|{data.ToLog()}");
                }
            }
            else if (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option)
            {
                if (data.MarketType != Market.EType.Futures && data.MarketType != Market.EType.Option)
                {
                    throw new ArgumentException($"{nameof(QuoteData.MarketGroupEnum)}={quote.MarketGroupEnum}|{nameof(StrategyData.MarketType)}={data.MarketType}|{data.ToLog()}");
                }
                else if (quote.MarketGroupEnum == Market.EGroup.Futures)
                {
                    data.MarketType = Market.EType.Futures;
                }
                else if (quote.MarketGroupEnum == Market.EGroup.Option)
                {
                    data.MarketType = Market.EType.Option;
                }
            }
        }

        private void ParentCheck(in StrategyData data, decimal marketPrice, in bool readyToSend, in DateTime start)
        {
            data.Trim();

            if (string.IsNullOrWhiteSpace(data.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{data.ToLog()}");
            }
            else if (data.Parent != null)
            {
                throw new ArgumentException($"data.Parent != null|{data.Parent.ToLog()}");
            }
            else if (data.PositionEnum != OrderPosition.Enum.Open)
            {
                throw new ArgumentException($"data.PositionEnum != OrderPosition.Enum.Open|{data.ToLog()}");
            }
            else if (data.OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({data.OrderQty}) <= 0|{data.ToLog()}");
            }
            else if (_appCtrl.OrderDetail[data.PrimaryKey] != null)
            {
                throw new ArgumentException($"_appCtrl.OrderDetail[{data.PrimaryKey}] != null|{data.ToLog()}");
            }
            else if (data.WinCloseSeconds < 0)
            {
                throw new ArgumentException($"data.WinCloseSeconds({data.WinCloseSeconds}) < 0|{data.ToLog()}");
            }
            else if (data.LossCloseSeconds < 0)
            {
                throw new ArgumentException($"data.LossCloseSeconds({data.LossCloseSeconds}) < 0|{data.ToLog()}");
            }
            else if (data.Quote == null)
            {
                data.Quote = _appCtrl.CAPQuote[data.Symbol];
            }

            if (data.Quote == null)
            {
                bool isHoliday = _appCtrl.Config.IsHoliday(start);
                (int, SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(data.Symbol, start);

                if (product.Item1 != 0)
                {
                    if (!isHoliday)
                    {
                        throw new ArgumentException($"{nameof(StrategyData.Symbol)}={data.Symbol}|{data.ToLog()}");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(data.StopLossBefore) || !string.IsNullOrWhiteSpace(data.StopWin1Before) || !string.IsNullOrWhiteSpace(data.StopWin1Before))
                {
                    throw new ArgumentException($"商品 {data.Symbol} 無訂閱報價，無法進行策略監控|{data.ToLog()}");
                }

                if (product.Item1 == 0)
                {
                    MarketCheck(data, _appCtrl.CAPQuote.CreateOrUpdate(product.Item2));
                }

                return;
            }
            else if (data.Quote.Symbol != data.Symbol)
            {
                throw new ArgumentException($"策略關聯報價代碼錯誤|{data.Quote.Symbol} != {data.Symbol}|{data.ToLog()}");
            }

            MarketCheck(data, data.Quote);

            if (marketPrice == 0)
            {
                marketPrice = data.Quote.DealPrice;
            }

            (string, decimal) orderPriceAfter = OrderPrice.Parse(data.OrderPriceBefore, data.Quote, marketPrice);

            if (readyToSend)
            {
                data.OrderPriceAfter = orderPriceAfter.Item2;
                _appCtrl.LogTrace(start, $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }

            if (!string.IsNullOrWhiteSpace(data.StopLossBefore))
            {
                (string, decimal) stopLossPriceAfter = OrderPrice.Parse(data.StopLossBefore, data.Quote, marketPrice);

                if (data.BSEnum == OrderBS.Enum.Buy)
                {
                    if (stopLossPriceAfter.Item2 >= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{data.ToLog()}");
                    }
                }
                else if (stopLossPriceAfter.Item2 <= orderPriceAfter.Item2)
                {
                    throw new ArgumentException($"停損價({stopLossPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{data.ToLog()}");
                }

                if (readyToSend)
                {
                    data.StopLossAfterRaw = stopLossPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停損價計算前={data.StopLossBefore}|計算後={stopLossPriceAfter.Item1}", UniqueName);
                    Notice = $"停損價計算前={data.StopLossBefore}|計算後={stopLossPriceAfter.Item1}";
                }
            }

            if (!string.IsNullOrWhiteSpace(data.StopWinPriceABefore))
            {
                (string, decimal) stopWinPriceAfter = OrderPrice.Parse(data.StopWinPriceABefore, data.Quote, marketPrice);

                if (data.BSEnum == OrderBS.Enum.Buy)
                {
                    if (stopWinPriceAfter.Item2 <= orderPriceAfter.Item2)
                    {
                        throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) <= 委託價({orderPriceAfter.Item2})|{data.ToLog()}");
                    }
                }
                else if (stopWinPriceAfter.Item2 >= orderPriceAfter.Item2)
                {
                    throw new ArgumentException($"停利價({stopWinPriceAfter.Item2}) >= 委託價({orderPriceAfter.Item2})|{data.ToLog()}");
                }

                if (readyToSend)
                {
                    data.StopWinPriceAAfterRaw = stopWinPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停利價計算前={data.StopWinPriceABefore}|計算後={stopWinPriceAfter.Item1}", UniqueName);
                    Notice = $"停利價計算前={data.StopWinPriceABefore}|計算後={stopWinPriceAfter.Item1}";
                }
            }

            if (!string.IsNullOrWhiteSpace(data.StopWin1Before))
            {
                string stopWinBefore = data.StopWin1Before;

                if (stopWinBefore.Contains("(") || stopWinBefore.Contains(","))
                {
                    string[] cells = stopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    stopWinBefore = cells[0];
                    data.StopWin1Qty = int.Parse(cells[1]);
                }

                data.StopWin1Offset = decimal.Parse(stopWinBefore);
            }

            if (data.StopWin1Qty == 0)
            { } //滿足條件但不減倉
            else if (data.StopWin1Qty > 0)
            {
                throw new ArgumentException($"停利1減倉量({data.StopWin1Qty})應為負值或0|{data.ToLog()}");
            }
            else if (data.OrderQty + data.StopWin1Qty < 0)
            {
                throw new ArgumentException($"停利1減倉量({data.StopWin1Qty}) > 委託量({data.OrderQty})|{data.ToLog()}");
            }

            if (!string.IsNullOrWhiteSpace(data.StopWin2Before))
            {
                string stopWinBefore = data.StopWin2Before;

                if (stopWinBefore.Contains("(") || stopWinBefore.Contains(","))
                {
                    string[] cells = stopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    stopWinBefore = cells[0];
                    data.StopWin2Qty = int.Parse(cells[1]);
                }

                data.StopWin2Offset = decimal.Parse(stopWinBefore);
            }

            if (data.StopWin2Qty == 0)
            { } //滿足條件但不減倉
            else if (data.StopWin2Qty > 0)
            {
                throw new ArgumentException($"停利2減倉量({data.StopWin2Qty})應為負值或0|{data.ToLog()}");
            }
            else if (data.OrderQty + data.StopWin1Qty + data.StopWin2Qty < 0)
            {
                throw new ArgumentException($"停利2減倉量({data.StopWin2Qty}) + 停利1減倉量({data.StopWin1Qty}) > 委託量({data.OrderQty})|{data.ToLog()}");
            }

            DateTime closeTime = _appCtrl.CAPQuote.MarketCloseTime;

            if (closeTime < DateTime.Now)
            {
                closeTime = _appCtrl.CAPQuote.IsAMMarket ? _appCtrl.Settings.MarketClose[(int)Market.EDayNight.AM] : _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM];
            }
            if (data.WinCloseTime == DateTime.MinValue && data.WinCloseSeconds > 0)
            {
                data.WinCloseTime = closeTime.AddSeconds(-data.WinCloseSeconds);
            }
            if (data.LossCloseTime == DateTime.MinValue && data.LossCloseSeconds > 0)
            {
                data.LossCloseTime = closeTime.AddSeconds(-data.LossCloseSeconds);
            }

            foreach (string account in data.AccountsWinLossClose.SplitWithoutWhiteSpace(','))
            {
                OrderAccData acc = _appCtrl.CAPOrder[account];

                if (acc == null)
                {
                    throw new ArgumentException($"帳號判斷獲利或損失，查無帳號|{nameof(account)}={account}|{data.ToLog()}");
                }
            }
        }

        private bool Close(in StrategyData data, in int qty, in string comment, in DateTime start, [CallerMemberName] in string memberName = "")
        {
            try
            {
                _appCtrl.LogTrace(start, $"{nameof(qty)}={qty}|{comment}|{data.ToLog()}", UniqueName);

                if (data.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    _appCtrl.LogTrace(start, $"已經取消|{data.ToLog()}", UniqueName);

                    return true;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Waiting || data.StopLossData != null || data.MarketClosingData != null || data.UnclosedQty == 0)
                {
                    data.StatusEnum = StrategyStatus.Enum.Cancelled;
                    data.Comment = comment;
                    data.Updater = memberName;
                    data.UpdateTime = DateTime.Now;

                    return true;
                }

                StrategyData marketClosingOrder = data.CreateMarketClosingOrder(qty);

                data.StatusEnum = StrategyStatus.Enum.MarketClosingSent;
                data.Comment = comment;
                data.Updater = memberName;
                data.UpdateTime = DateTime.Now;

                //負值減倉正值留倉
                if (data.MarketClosingData == marketClosingOrder)
                {
                    _appCtrl.CAPOrder.Send(marketClosingOrder);
                }

                return true;
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

            return false;
        }

        public bool Close(in string primaryKey, in int qty, in string comment = "手動停止")
        {
            DateTime start = _appCtrl.StartTrace($"{nameof(primaryKey)}={primaryKey}|{nameof(qty)}={qty}|{comment}", UniqueName);

            try
            {
                StrategyData data = this[primaryKey];

                lock (data.SyncRoot)
                {
                    if (Close(data, qty, comment, start))
                    {
                        Task.Factory.StartNew(() => SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileSaveFormat));

                        return true;
                    }
                }
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

            return false;
        }

        public void CloseAll(in int qty, in string comment = "手動停止")
        {
            DateTime start = _appCtrl.StartTrace($"{nameof(qty)}={qty}|{comment}", UniqueName);

            for (int i = Count - 1; i >= 0; --i)
            {
                try
                {
                    StrategyData data = this[i];

                    if (_appCtrl.Settings.SendRealOrder && !data.SendRealOrder)
                    {
                        continue;
                    }

                    lock (data.SyncRoot)
                    {
                        Close(data, qty, comment, start);
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                    Notice = ex.Message;
                }
            }

            Task.Factory.StartNew(() => SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileSaveFormat));
        }

        private void OpenTrigger(in StrategyData data, in string primary, in DateTime start)
        {
            try
            {
                (LogLevel, string) result = _appCtrl.Trigger.Restart(primary);

                if (result.Item1 == LogLevel.Trace)
                {
                    _appCtrl.LogTrace(start, $"重啟觸價({primary})|{data.ToLog()}", UniqueName);
                }
                else
                {
                    _appCtrl.Log(result.Item1, $"{result.Item2}|{data.ToLog()}", UniqueName, DateTime.Now - start);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"重啟觸價({primary})失敗|{data.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void OpenStrategy(in StrategyData source, in string targetKey, in DateTime start)
        {
            try
            {
                if (targetKey == source.PrimaryKey)
                {
                    return;
                }

                _appCtrl.Strategy.StartNow(_appCtrl.Strategy[targetKey]);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"執行策略({targetKey})失敗|{source.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void AfterStopLoss(in StrategyData data, in DateTime start)
        {
            foreach (string primary in data.OpenTriggerAfterStopLoss.SplitWithoutWhiteSpace(','))
            {
                OpenTrigger(data, primary, start);
            }

            foreach (string primary in data.OpenStrategyAfterStopLoss.SplitWithoutWhiteSpace(','))
            {
                OpenStrategy(data, primary, start);
            }
        }

        private void AfterStopWin(in StrategyData data, in int number, in DateTime start)
        {
            if (number == StrategyData.StopWin1)
            {
                if (data.StopWin2Qty != 0)
                {
                    return;
                }
            }
            else if (data.StopWin2Qty == 0)
            {
                return;
            }

            foreach (string primary in data.CloseTriggerAfterStopWin.SplitWithoutWhiteSpace(','))
            {
                _appCtrl.Trigger.Cancel(primary, "策略取消");
            }

            foreach (string primary in data.CloseStrategyAfterStopWin.SplitWithoutWhiteSpace(','))
            {
                Close(primary, 0, "策略停止");
            }

            foreach (string primary in data.OpenTriggerAfterStopWin.SplitWithoutWhiteSpace(','))
            {
                OpenTrigger(data, primary, start);
            }

            foreach (string primary in data.OpenStrategyAfterStopWin.SplitWithoutWhiteSpace(','))
            {
                OpenStrategy(data, primary, start);
            }
        }

        private void SendStopWin(in StrategyData data, in int number, in DateTime start)
        {
            if (number == StrategyData.StopWin1)
            {
                StrategyData stopWinOrder = data.CreateStopWinOrder(number);

                data.StatusEnum = StrategyStatus.Enum.StopWinSent;

                if (data.StopWin1Qty == 0)
                { } //滿足條件但不減倉
                else
                {
                    _appCtrl.CAPOrder.Send(stopWinOrder);
                }

                AfterStopWin(data, number, start);

                return;
            }

            StrategyData moveStopWinOrder = data.CreateStopWinOrder(number);

            data.StatusEnum = StrategyStatus.Enum.StopWinOrderReport;
            data.StatusEnum = StrategyStatus.Enum.StopWinSent;

            if (data.StopWin2Qty == 0)
            { } //滿足條件但不減倉
            else
            {
                _appCtrl.CAPOrder.Send(moveStopWinOrder);
            }

            AfterStopWin(data, number, start);
        }

        private bool UpdateStatus(in StrategyData data, in decimal marketPrice, in DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            bool saveData = false;

            lock (data.SyncRoot)
            {
                if (_appCtrl.CAPQuote.Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                //else if (quote.Simulate != QuoteData.RealTrade)
                //{
                //    return saveData;
                //}
                //else if (marketPrice == 0)
                //{
                //    return saveData;
                //}
                else if (data.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    return saveData;
                }
                else if (data.MarketClosingData != null)
                {
                    return saveData;
                }
                else if (data.StartTimesMax < 0)
                {
                    data.StatusEnum = StrategyStatus.Enum.Cancelled;
                    data.Comment = "啟動次數限制";
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;

                    return saveData;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Monitoring)
                {
                    data.MarketPrice = marketPrice;
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;

                    if (data.OrderPriceBefore.Length >= 3 && ((data.OrderPriceBefore[1] == '+' && data.MarketPrice >= data.OrderPriceAfter) || (data.OrderPriceBefore[1] == '-' && data.MarketPrice <= data.OrderPriceAfter)))
                    { }
                    else if ((data.BSEnum == OrderBS.Enum.Buy && data.MarketPrice >= data.OrderPriceAfter) || (data.BSEnum == OrderBS.Enum.Sell && data.MarketPrice <= data.OrderPriceAfter))
                    {
                        if (data.OrderPriceBefore.Length >= 3 && ((data.OrderPriceBefore[1] == '+') || (data.OrderPriceBefore[1] == '-')))
                        {
                            return saveData;
                        }
                    }
                    else
                    {
                        return saveData;
                    }

                    if (data.OrderData == null)
                    {
                        StrategyData order = data.CreateOrder();

                        data.StatusEnum = StrategyStatus.Enum.OrderSent;
                        data.BestClosePrice = 0;
                        data.StopWinATouched = false;

                        order.OrderPriceBefore = OrderPrice.P;

                        _appCtrl.CAPOrder.Send(order);
                    }
                    else
                    {
                        _appCtrl.LogError(start, $"監控中的策略已經存在委託單|{data.ToLog()}|{data.OrderData.ToLog()}", UniqueName);

                        data.StatusEnum = StrategyStatus.Enum.OrderSent;
                    }

                    return saveData;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Waiting)
                {
                    data.MarketPrice = marketPrice;

                    return saveData;
                }
                else if (data.StopLossData != null || data.UnclosedQty == 0)
                {
                    if (data.StartTimesMax <= 0)
                    {
                        data.StatusEnum = StrategyStatus.Enum.Cancelled;
                        data.Comment = "啟動次數限制";
                        data.Updater = methodName;
                        data.UpdateTime = DateTime.Now;

                        return saveData;
                    }

                    string pk = $",{data.PrimaryKey},";

                    if ($",{data.OpenStrategyAfterStopLoss},".Contains(pk) || $",{data.OpenStrategyAfterStopWin},".Contains(pk))
                    {
                        data.MarketPrice = marketPrice;

                        if (data.OrderPriceBefore.Length >= 3 && ((data.OrderPriceBefore[1] == '+' && data.MarketPrice <= (data.OrderPriceAfter - 5)) || (data.OrderPriceBefore[1] == '-' && data.MarketPrice >= (data.OrderPriceAfter + 5))))
                        { }
                        else if ((data.BSEnum == OrderBS.Enum.Buy && data.MarketPrice <= (data.OrderPriceAfter - 5)) || (data.BSEnum == OrderBS.Enum.Sell && data.MarketPrice >= (data.OrderPriceAfter + 5)))
                        {
                            if (data.OrderPriceBefore.Length >= 3 && ((data.OrderPriceBefore[1] == '+') || (data.OrderPriceBefore[1] == '-')))
                            {
                                return saveData;
                            }
                        }
                        else
                        {
                            return saveData;
                        }

                        --data.StartTimesMax;

                        data.StatusEnum = StrategyStatus.Enum.Monitoring;
                        data.OrderData = null;
                        data.StopLossData = null;
                        data.StopWin1Data = null;
                        data.StopWin2Data = null;
                        data.ClosedProfit = 0;
                        data.UnclosedQty = 0;
                        data.MarketClosingData = null;
                        data.Comment = "自己重啟自己";

                        data.Updater = methodName;
                        data.UpdateTime = DateTime.Now;
                    }

                    return saveData;
                }

                data.MarketPrice = marketPrice;
                data.UnclosedProfit = (data.MarketPrice - data.DealPrice) * data.UnclosedQty * data.ProfitDirection;
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                if (data.BSEnum == OrderBS.Enum.Buy)
                {
                    if (data.BestClosePrice < data.MarketPrice)
                    {
                        data.BestClosePrice = data.MarketPrice;
                    }
                }
                else if ((data.BestClosePrice > data.MarketPrice || data.BestClosePrice == 0) && data.MarketPrice != 0)
                {
                    data.BestClosePrice = data.MarketPrice;
                }

                if (data.UnclosedQty > 0 && data.StatusEnum != StrategyStatus.Enum.MarketClosingSent && data.StatusEnum != StrategyStatus.Enum.MarketClosingOrderReport && data.StatusEnum != StrategyStatus.Enum.MarketClosingDealReport)
                {
                    if (data.WinCloseSeconds > 0 && DateTime.Now >= data.WinCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        if (!string.IsNullOrWhiteSpace(data.AccountsWinLossClose))
                        {
                            decimal? sum = _appCtrl.FuturesRights.SumProfit(data.AccountsWinLossClose, start);

                            if (sum.HasValue && sum.Value > 0)
                            {
                                Close(data, data.WinCloseQty, "收盤帳號獲利減倉", start);

                                saveData = true;
                                return saveData;
                            }
                        }
                        else if (data.UnclosedProfit > 0)
                        {
                            Close(data, data.WinCloseQty, "收盤策略獲利減倉", start);

                            saveData = true;
                            return saveData;
                        }
                    }
                    
                    if (data.LossCloseSeconds > 0 && DateTime.Now >= data.LossCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        if (!string.IsNullOrWhiteSpace(data.AccountsWinLossClose))
                        {
                            decimal? sum = _appCtrl.FuturesRights.SumProfit(data.AccountsWinLossClose, start);

                            if (sum.HasValue && sum.Value <= 0)
                            {
                                Close(data, data.LossCloseQty, "收盤帳號損失減倉", start);

                                saveData = true;
                                return saveData;
                            }
                        }
                        else if (data.UnclosedProfit <= 0)
                        {
                            Close(data, data.LossCloseQty, "收盤策略損失減倉", start);

                            saveData = true;
                            return saveData;
                        }
                    }
                }

                if (data.StatusEnum == StrategyStatus.Enum.OrderSent || data.StatusEnum == StrategyStatus.Enum.OrderReport || data.StatusEnum == StrategyStatus.Enum.DealReport || data.StatusEnum == StrategyStatus.Enum.OrderError)
                {
                    if (data.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (data.MarketPrice <= data.StopLossAfterRaw)
                        {
                            StrategyData stopLossOrder = data.CreateStopLossOrder();

                            data.StatusEnum = StrategyStatus.Enum.StopLossSent;

                            _appCtrl.CAPOrder.Send(stopLossOrder);

                            saveData = true;
                            AfterStopLoss(data, start);
                        }
                        else if (!data.StopWinATouched && data.MarketPrice >= data.StopWinPriceAAfterRaw)
                        {
                            data.StopWinATouched = true;
                        }
                        
                        if (data.StopWinATouched && data.StopWin1Qty <= 0)
                        {
                            if ((data.StopWin1Offset <= 0 && data.MarketPrice <= data.BestClosePrice + data.StopWin1Offset) ||
                                (data.StopWin1Offset > 0 && data.MarketPrice <= data.OrderPriceAfter + data.StopWin1Offset))
                            {
                                saveData = true;
                                SendStopWin(data, StrategyData.StopWin1, start);
                            }
                        }
                    }
                    else if (data.BSEnum == OrderBS.Enum.Sell)
                    {
                        if (data.MarketPrice >= data.StopLossAfterRaw)
                        {
                            StrategyData stopLossOrder = data.CreateStopLossOrder();

                            data.StatusEnum = StrategyStatus.Enum.StopLossSent;

                            _appCtrl.CAPOrder.Send(stopLossOrder);

                            saveData = true;
                            AfterStopLoss(data, start);
                        }
                        else if (!data.StopWinATouched && data.MarketPrice <= data.StopWinPriceAAfterRaw)
                        {
                            data.StopWinATouched = true;
                        }
                        
                        if (data.StopWinATouched && data.StopWin1Qty <= 0)
                        {
                            if ((data.StopWin1Offset >= 0 && data.MarketPrice >= data.BestClosePrice + data.StopWin1Offset) ||
                            (data.StopWin1Offset < 0 && data.MarketPrice >= data.OrderPriceAfter + data.StopWin1Offset))
                            {
                                saveData = true;
                                SendStopWin(data, StrategyData.StopWin1, start);
                            }
                        }
                    }
                }
                else if (data.StopWin2Qty <= 0 && data.StopWin2Data == null && data.StopWin1Data != null && data.StopWin1Data.OrderQty < data.OrderQty)
                {
                    if (data.BSEnum == OrderBS.Enum.Buy)
                    {
                        if ((data.StopWin2Offset <= 0 && data.MarketPrice <= data.BestClosePrice + data.StopWin2Offset) ||
                            (data.StopWin2Offset > 0 && data.MarketPrice <= data.OrderPriceAfter + data.StopWin2Offset))
                        {
                            saveData = true;
                            SendStopWin(data, StrategyData.StopWin2, start);
                        }
                    }
                    else if (data.BSEnum == OrderBS.Enum.Sell)
                    {
                        if ((data.StopWin2Offset >= 0 && data.MarketPrice >= data.BestClosePrice + data.StopWin2Offset) ||
                            (data.StopWin2Offset < 0 && data.MarketPrice >= data.OrderPriceAfter + data.StopWin2Offset))
                        {
                            saveData = true;
                            SendStopWin(data, StrategyData.StopWin2, start);
                        }
                    }
                }
            }

            return saveData;
        }

        /// <summary>
        /// Run in background.
        /// </summary>
        /// <param name="start"></param>
        public void UpdateStatus(DateTime start)
        {
            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out StrategyData data);

                StrategyData toRemove = null;

                if (!_dataMap.TryGetValue(data.PrimaryKey, out StrategyData _old))
                {
                    _appCtrl.LogTrace(start, $"新增設定|{data.ToLog()}", UniqueName);
                }
                else
                {
                    _appCtrl.LogWarn(start, $"重置設定|{data.ToLog()}", UniqueName);
                    _dataMap.Remove(data.PrimaryKey);
                    toRemove = _old;
                }

                _dataMap.Add(data.PrimaryKey, data);

                List<StrategyData> list = _dataMap.Values.ToList();
                int index = list.IndexOf(data);

                if (index + 1 < list.Count)
                {
                    StrategyData next = list[index + 1];
                    index = _dataCollection.IndexOf(next);
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    try
                    {
                        _dataCollection.Insert(index, data);

                        if (toRemove != null)
                        {
                            _dataCollection.Remove(toRemove);
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                });

                if (_waitToAdd.Count <= 0)
                {
                    SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileSaveFormat);
                }
            }

            if (!ProfitTotalStopWinClosed)
            {
                ProfitTotal = _appCtrl.Settings.SendRealOrder ? _dataMap.Values.Sum(x => x.SendRealOrder ? x.ClosedProfitTotalRaw + x.UnclosedProfit : 0) : _dataMap.Values.Sum(x => x.ClosedProfitTotalRaw + x.UnclosedProfit);

                if (ProfitTotal > 0 && ProfitTotalBest < ProfitTotal)
                {
                    _appCtrl.LogTrace(start, $"{nameof(ProfitTotal)}={ProfitTotal}|{nameof(ProfitTotalBest)}={ProfitTotalBest}", UniqueName);
                    ProfitTotalBest = ProfitTotal;
                }

                if (!ProfitTotalStopWinTouched && _appCtrl.Settings.StrategyStopWinProfit > 0 && ProfitTotalBest >= _appCtrl.Settings.StrategyStopWinProfit)
                {
                    ProfitTotalStopWinTouched = true;
                }

                if (ProfitTotalStopWinTouched && _appCtrl.Settings.StrategyStopWinProfit > 0)
                {
                    if (_appCtrl.Settings.StrategyStopWinOffset <= 0 && ProfitTotal <= ProfitTotalBest + _appCtrl.Settings.StrategyStopWinOffset)
                    {
                        ProfitTotalStopWinClosed = true;

                        CloseAll(0, "整體停利觸發");
                    }
                    else if (_appCtrl.Settings.StrategyStopWinOffset > 0 && ProfitTotal <= _appCtrl.Settings.StrategyStopWinOffset)
                    {
                        ProfitTotalStopWinClosed = true;

                        CloseAll(0, "整體停利觸發");
                    }
                }
            }

            bool saveData = false;

            _marketPriceSnapshot.Clear();

            for (int i = Count - 1; i >= 0; --i)
            {
                try
                {
                    StrategyData data = this[i];
                    decimal marketPrice = 0;

                    if (data.Quote.Simulate != QuoteData.RealTrade)
                    {
                        continue;
                    }
                    else if (!_marketPriceSnapshot.TryGetValue(data.Quote.Symbol, out marketPrice))
                    {
                        marketPrice = data.Quote.DealPrice;

                        _marketPriceSnapshot[data.Quote.Symbol] = marketPrice;
                    }

                    if (marketPrice == 0)
                    {
                        continue;
                    }
                    else if (UpdateStatus(data, marketPrice, start))
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
                SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileSaveFormat);
            }
        }

        private void AddOrUpdateRule(StrategyData data)
        {
            if (_appCtrl.Config.StrategyFolder == null)
            {
                throw new ArgumentNullException($"未設定策略資料夾(Settings.StrategyFolderPath)，無法建立策略|{data.ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(data.PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{data.ToLog()}");
            }
            //else if (_dataMap.ContainsKey(data.PrimaryKey))
            //{
            //    throw new ArgumentException($"_dataMap.ContainsKey({data.PrimaryKey})|{data.ToLog()}");
            //}
            //else if (data.StatusEnum != StrategyStatus.Enum.Waiting)
            //{
            //    throw new ArgumentException($"{data.StatusEnum} != StrategyStatus.Enum.Waiting|{data.ToLog()}");
            //}
            else if (_dataMap.TryGetValue(data.PrimaryKey, out StrategyData _old) && _old.StatusEnum != StrategyStatus.Enum.Waiting)
            {
                throw new ArgumentException($"舊設定已啟動或取消，無法更新|{_old.StatusEnum} != StrategyStatus.Enum.Waiting|{_old.ToLog()}");
            }

            string key1 = $"{data.FullAccount}_{data.Symbol}_{data.BSEnum}_{data.DayTradeEnum}_{data.OrderQty}_{data.SendRealOrder}";

            for (int i = Count - 1; i >= 0; --i)
            {
                StrategyData target = this[i];

                if (target == data)
                {
                    break;
                }
                else if (target.FullAccount != data.FullAccount)
                {
                    continue;
                }
                else if (_appCtrl.Settings.StrategyNotForOpenInterest.SplitWithoutWhiteSpace(',').FirstOrDefault(x => data.PrimaryKey.StartsWith(x)) != null)
                {
                    continue;
                }

                string key2 = $"{target.FullAccount}_{target.Symbol}_{target.BSEnum}_{target.DayTradeEnum}_{target.OrderQty}_{target.SendRealOrder}";

                if (key2 == key1)
                {
                    throw new NotSupportedException($"不支援重複相同委託量({target.OrderQty})的策略|{key2}|{target.ToLog()}");
                }
            }

            _waitToAdd.Enqueue(data);
        }

        public void AddRule(in StrategyData data)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}", UniqueName);

            try
            {
                ParentCheck(data, 0, false, start);
                AddOrUpdateRule(data);
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

        private void SerialReset(in StrategyData data, in bool toZero)
        {
            if (data == null)
            {
                return;
            }

            foreach (string primary in data.OpenStrategyAfterStopLoss.SplitWithoutWhiteSpace(','))
            {
                if (primary != data.PrimaryKey)
                {
                    SerialReset(this[primary], toZero);
                }
            }

            foreach (string primary in data.OpenStrategyAfterStopWin.SplitWithoutWhiteSpace(','))
            {
                if (primary != data.PrimaryKey)
                {
                    SerialReset(this[primary], toZero);
                }
            }

            if (toZero)
            {
                data.UnclosedQty = 0;
                data.UnclosedProfit = 0;
            }

            data.Reset();
        }

        public void ResetToZero(in string primaryKey)
        {
            SerialReset(this[primaryKey], true);
        }

        private void CancelAfterOrderSent(in StrategyData data, in DateTime start)
        {
            _appCtrl.OpenInterest.FilterFullAccount(data.FullAccount, start);

            try
            {
                string pk = $",{data.PrimaryKey},";

                _appCtrl.Trigger.CancelAfterOrderSent(data, start);

                foreach (StrategyData father in _dataMap.Values)
                {
                    if (father == data)
                    {
                        continue;
                    }
                    else if ($",{father.OpenStrategyAfterStopLoss},".Contains(pk) || $",{father.OpenStrategyAfterStopWin},".Contains(pk))
                    {
                        _appCtrl.Trigger.CancelAfterOrderSent(father, start);
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void StartNow(in StrategyData data, in decimal marketPrice = 0)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}", UniqueName);

            SerialReset(data, false);
            ParentCheck(data, marketPrice, true, start);

            if (data.StartTimesMax > 0)
            {
                --data.StartTimesMax;
            }
            else
            {
                throw new ArgumentException($"啟動次數限制({nameof(StrategyData.StartTimesMax)})={data.StartTimesMax}|{data.ToLog()}");
            }

            if (!decimal.TryParse(data.OrderPriceBefore, out _) && data.OrderPriceBefore.Length >= 3)
            {
                data.StatusEnum = StrategyStatus.Enum.Monitoring;

                CancelAfterOrderSent(data, start);

                return;
            }

            StrategyData order = data.CreateOrder();

            data.StatusEnum = StrategyStatus.Enum.OrderSent;

            _appCtrl.CAPOrder.Send(order);

            CancelAfterOrderSent(data, start);
        }

        public void StartNow(in string primaryKey)
        {
            StartNow(this[primaryKey.Replace(" ", string.Empty)]);
        }

        public void StartNow(in string keys, in TriggerData trigger, in DateTime start)
        {
            _marketPriceSnapshot.Clear();

            foreach (string primaryKey in keys.SplitWithoutWhiteSpace(','))
            {
                try
                {
                    StrategyData data = this[primaryKey];

                    if (!_marketPriceSnapshot.TryGetValue(data.Quote.Symbol, out decimal marketPrice))
                    {
                        marketPrice = data.Quote.DealPrice;

                        _marketPriceSnapshot[data.Quote.Symbol] = marketPrice;
                    }

                    _appCtrl.Strategy.StartNow(data, marketPrice);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                    _appCtrl.LogError(start, $"執行策略({primaryKey})失敗|{trigger.ToLog()}", UniqueName);
                }
                finally
                {
                    _appCtrl.EndTrace(start, UniqueName);
                }
            }
        }

        public void StartNow(in StrategyData data, in OpenInterestData openInterest)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}|{openInterest?.ToLog()}", UniqueName);

            QuoteData quoteBK = data.Quote;
            data.Quote = data.Quote.DeepClone();
            data.Quote.DealPrice = openInterest.AveragePrice;
            data.Quote.Simulate = QuoteData.SimulateTrade;

            bool sendRealOrder = data.SendRealOrder;
            data.SendRealOrder = false;

            try
            {
                if (!decimal.TryParse(data.OrderPriceBefore, out _) && data.OrderPriceBefore.Length >= 3)
                {
                    decimal offset = decimal.Parse(data.OrderPriceBefore.Substring(1));

                    data.Quote.DealPrice -= offset;
                }

                SerialReset(data, true);
                ParentCheck(data, 0, true, start);

                StrategyData order = data.CreateOrder();

                data.StatusEnum = StrategyStatus.Enum.OrderSent;
                order.OrderPriceBefore = OrderPrice.P;

                _appCtrl.CAPOrder.Send(order);

                CancelAfterOrderSent(data, start);

                data.DealPrice = openInterest.AveragePrice;

                order.DealPrice = openInterest.AveragePrice;
                order.SendRealOrder = sendRealOrder;

                if (string.IsNullOrWhiteSpace(openInterest.Strategy))
                {
                    openInterest.Strategy = data.PrimaryKey;
                }
                else
                {
                    openInterest.Strategy = $"{data.PrimaryKey},{openInterest.Strategy}";
                    openInterest.Strategy = openInterest.Strategy.JoinSortedSet(',');
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                data.Quote = quoteBK;
                data.SendRealOrder = sendRealOrder;

                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void RecoverSetting(FileInfo file = null, [CallerMemberName] in string memberName = "")
        {
            if (_dataMap.Count > 0)
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace($"{file?.FullName}", UniqueName);

            try
            {
                if (file == null)
                {
                    if (_appCtrl.Config.StrategyFolder == null)
                    {
                        return;
                    }
                    _appCtrl.Config.StrategyFolder.Refresh();

                    string loadFile = _appCtrl.Settings.StrategyFileLoadFormat;

                    if (loadFile.Contains(AppSettings.Keyword_Holiday))
                    {
                        loadFile = loadFile.Replace(AppSettings.Keyword_Holiday, _appCtrl.Config.IsHoliday(start.AddDays(1)).ToString());
                    }

                    if (loadFile.Contains(AppSettings.Keyword_DayNight))
                    {
                        loadFile = loadFile.Replace(AppSettings.Keyword_DayNight, _appCtrl.CAPQuote.IsAMMarket ? $"{Market.EDayNight.AM}" : $"{Market.EDayNight.PM}");
                    }

                    if (loadFile.Contains(AppSettings.Keyword_DayOfWeek))
                    {
                        int tradeDate = _appCtrl.CAPQuote.DataCollection.Max(x => x.TradeDateRaw);
                        DayOfWeek dow = DateTime.ParseExact(tradeDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).DayOfWeek;

                        loadFile = loadFile.Replace(AppSettings.Keyword_DayOfWeek, dow.ToString());
                    }

                    file = _appCtrl.Config.StrategyFolder.GetFiles(loadFile).LastOrDefault();

                    _appCtrl.StartTrace($"{file?.FullName}", UniqueName);
                }

                if (file == null)
                {
                    return;
                }

                RecoverFile = Path.Combine(file.Directory.Name, file.Name);

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (StrategyData data in StrategyData.ForeachQuoteFromCSVFile(file.FullName, columnNames))
                {
                    try
                    {
                        data.Trim(memberName);

                        if (string.IsNullOrWhiteSpace(data.PrimaryKey))
                        {
                            continue;
                        }

                        data.ClosedProfit = 0;
                        data.UnclosedQty = 0;
                        data.Reset(memberName);

                        data.MarketType = _appCtrl.CAPOrder[data.FullAccount].MarketType;
                        data.Quote = _appCtrl.CAPQuote[data.Symbol];

                        if (decimal.TryParse(data.PrimaryKey, out decimal _pk) && _pk > nextPK)
                        {
                            nextPK = _pk + 1;
                        }

                        AddRule(data);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                }

                SpinWait.SpinUntil(() => _waitToAdd.Count <= 0);
                Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                if (Count >= nextPK)
                {
                    nextPK = Count + 1;
                }

                if (!_dataMap.ContainsKey($"{nextPK}"))
                {
                    _appCtrl.MainForm.InvokeAsync(delegate { _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{nextPK}"; });
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
    }
}
