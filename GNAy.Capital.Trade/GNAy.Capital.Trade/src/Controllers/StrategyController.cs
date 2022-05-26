using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        public IReadOnlyList<StrategyData> DataCollection => _dataCollection;

        public string Notice { get; private set; }

        public StrategyController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(StrategyController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<StrategyData>();

            _dataMap = new SortedDictionary<string, StrategyData>();
            _appCtrl.MainForm.DataGridStrategyRule.SetHeadersByBindings(StrategyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridStrategyRule.SetAndGetItemsSource<StrategyData>();

            Notice = string.Empty;
        }

        private StrategyController() : this(null)
        { }

        public void SaveData(IEnumerable<StrategyData> dataCollection, DirectoryInfo dir, string fileFormat)
        {
            DateTime start = _appCtrl.StartTrace($"{dir?.FullName}|{fileFormat}", UniqueName);

            try
            {
                string path = Path.Combine(dir.FullName, string.Format("{0}.csv", DateTime.Now.ToString(fileFormat)));
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

        public void MarketCheck(StrategyData data, QuoteData quote)
        {
            if (quote.MarketGroupEnum == Market.EGroup.TSE || quote.MarketGroupEnum == Market.EGroup.OTC || quote.MarketGroupEnum == Market.EGroup.Emerging)
            {
                if (data.MarketType != Market.EType.Stock)
                {
                    throw new ArgumentException($"MarketGroupEnum={quote.MarketGroupEnum}|MarketType={data.MarketType}|{data.ToLog()}");
                }
            }
            else if (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option)
            {
                if (data.MarketType != Market.EType.Futures && data.MarketType != Market.EType.Option)
                {
                    throw new ArgumentException($"MarketGroupEnum={quote.MarketGroupEnum}|MarketType={data.MarketType}|{data.ToLog()}");
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

        private void ParentCheck(StrategyData data, bool readyToSend, DateTime start)
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
            else if (data.Quote != null && data.Quote.Symbol != data.Symbol)
            {
                throw new ArgumentException($"策略關聯報價代碼錯誤|{data.Quote.Symbol} != {data.Symbol}|{data.ToLog()}");
            }
            else if (data.Quote == null)
            {
                data.Quote = _appCtrl.CAPQuote[data.Symbol];
            }

            if (data.Quote == null)
            {
                (int, SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(data.Symbol, start);

                if (product.Item1 != 0)
                {
                    throw new ArgumentException($"data.Symbol={data.Symbol}|{data.ToLog()}");
                }
                else if (!string.IsNullOrWhiteSpace(data.StopLossBefore) || !string.IsNullOrWhiteSpace(data.StopWinBefore) || !string.IsNullOrWhiteSpace(data.MoveStopWinBefore))
                {
                    throw new ArgumentException($"商品 {data.Symbol} 無訂閱報價，無法進行策略監控|{data.ToLog()}");
                }

                MarketCheck(data, _appCtrl.CAPQuote.CreateOrUpdate(product.Item2));

                return;
            }

            MarketCheck(data, data.Quote);

            if (_appCtrl.CAPQuote.MarketCloseTime != DateTime.MinValue)
            {
                if (data.WinCloseTime == DateTime.MinValue && data.WinCloseSeconds > 0)
                {
                    data.WinCloseTime = _appCtrl.CAPQuote.MarketCloseTime.AddSeconds(-data.WinCloseSeconds);
                }

                if (data.LossCloseTime == DateTime.MinValue && data.LossCloseSeconds > 0)
                {
                    data.LossCloseTime = _appCtrl.CAPQuote.MarketCloseTime.AddSeconds(-data.LossCloseSeconds);
                }
            }

            (string, decimal) orderPriceAfter = OrderPrice.Parse(data.OrderPriceBefore, data.Quote);

            if (readyToSend)
            {
                data.OrderPriceAfter = orderPriceAfter.Item2;
                _appCtrl.LogTrace(start, $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}", UniqueName);
                Notice = $"委託價計算前={data.OrderPriceBefore}|計算後={orderPriceAfter.Item1}";
            }

            if (!string.IsNullOrWhiteSpace(data.StopLossBefore))
            {
                (string, decimal) stopLossPriceAfter = OrderPrice.Parse(data.StopLossBefore, data.Quote);

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
                    data.StopLossAfter = stopLossPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停損價計算前={data.StopLossBefore}|計算後={stopLossPriceAfter.Item1}", UniqueName);
                    Notice = $"停損價計算前={data.StopLossBefore}|計算後={stopLossPriceAfter.Item1}";
                }
            }

            if (!string.IsNullOrWhiteSpace(data.StopWinBefore))
            {
                string stopWinBefore = data.StopWinBefore;

                if (stopWinBefore.Contains("(") || stopWinBefore.Contains(","))
                {
                    string[] cells = stopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    stopWinBefore = cells[0];
                    data.StopWinQty = int.Parse(cells[1]);
                }

                (string, decimal) stopWinPriceAfter = OrderPrice.Parse(stopWinBefore, data.Quote);

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
                    data.StopWinPrice = stopWinPriceAfter.Item2;
                    _appCtrl.LogTrace(start, $"停利價計算前={stopWinBefore}|計算後={stopWinPriceAfter.Item1}", UniqueName);
                    Notice = $"停利價計算前={stopWinBefore}|計算後={stopWinPriceAfter.Item1}";
                }
            }

            if (data.StopWinQty == 0)
            { } //滿足條件但不減倉
            else if (data.StopWinQty > 0)
            {
                throw new ArgumentException($"停利減倉量({data.StopWinQty})應為負值或0|{data.ToLog()}");
            }
            else if (data.OrderQty + data.StopWinQty < 0)
            {
                throw new ArgumentException($"停利減倉量({data.StopWinQty}) > 委託量({data.OrderQty})|{data.ToLog()}");
            }

            if (!string.IsNullOrWhiteSpace(data.MoveStopWinBefore))
            {
                string moveStopWinBefore = data.MoveStopWinBefore;

                if (moveStopWinBefore.Contains("(") || moveStopWinBefore.Contains(","))
                {
                    string[] cells = moveStopWinBefore.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    moveStopWinBefore = cells[0];
                    data.MoveStopWinQty = int.Parse(cells[1]);
                }

                data.MoveStopWinOffset = decimal.Parse(moveStopWinBefore);

                if (data.BSEnum == OrderBS.Enum.Buy)
                {
                    if (data.MoveStopWinOffset >= 0)
                    {
                        throw new ArgumentException($"移動停利位移({data.MoveStopWinOffset}) >= 0|{data.ToLog()}");
                    }
                }
                else if (data.MoveStopWinOffset <= 0)
                {
                    throw new ArgumentException($"移動停利位移({data.MoveStopWinOffset}) <= 0|{data.ToLog()}");
                }
            }

            if (data.MoveStopWinQty == 0)
            { } //滿足條件但不減倉
            else if (data.MoveStopWinQty > 0)
            {
                throw new ArgumentException($"移動停利減倉量({data.MoveStopWinQty})應為負值或0|{data.ToLog()}");
            }
            else if (data.OrderQty + data.StopWinQty + data.MoveStopWinQty < 0)
            {
                throw new ArgumentException($"移動停利減倉量({data.MoveStopWinQty}) + 停利減倉量({data.StopWinQty}) > 委託量({data.OrderQty})|{data.ToLog()}");
            }
        }

        private bool Close(StrategyData data, int qty, string comment, DateTime start)
        {
            const string methodName = nameof(Close);

            try
            {
                if (data.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    _appCtrl.LogTrace(start, $"已經取消|{data.ToLog()}", UniqueName);

                    return true;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Waiting || data.StopLossData != null || data.MarketClosingData != null || data.UnclosedQty <= 0)
                {
                    data.StatusEnum = StrategyStatus.Enum.Cancelled;
                    data.Comment = comment;
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;

                    return true;
                }

                StrategyData marketClosingOrder = data.CreateMarketClosingOrder(qty);

                data.StatusEnum = StrategyStatus.Enum.MarketClosingSent;
                data.Comment = comment;
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                _appCtrl.CAPOrder.Send(marketClosingOrder);

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

        public bool Close(string primaryKey, int qty = 0, string comment = "手動停止")
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}|qty={qty}", UniqueName);

            try
            {
                if (!_dataMap.TryGetValue(primaryKey.Replace(" ", string.Empty), out StrategyData data))
                {
                    throw new ArgumentNullException($"查無此唯一鍵|{primaryKey}");
                }

                lock (data.SyncRoot)
                {
                    if (Close(data, qty, comment, start))
                    {
                        Task.Factory.StartNew(() => SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat));

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

        private void OpenTrigger(StrategyData data, string primary, DateTime start)
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

        private void OpenStrategy(StrategyData data, string primary, DateTime start)
        {
            try
            {
                StrategyData target = _appCtrl.Strategy[primary];

                _appCtrl.Strategy.StartNow(target.PrimaryKey);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _appCtrl.LogError(start, $"執行策略({primary})失敗|{data.ToLog()}", UniqueName);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void AfterStopLoss(StrategyData data, DateTime start)
        {
            if (!string.IsNullOrWhiteSpace(data.OpenTriggerAfterStopLoss))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenTriggerAfterStopLoss.Split(','));

                foreach (string primary in targets)
                {
                    OpenTrigger(data, primary, start);
                }
            }

            if (!string.IsNullOrWhiteSpace(data.OpenStrategyAfterStopLoss))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenStrategyAfterStopLoss.Split(','));

                foreach (string primary in targets)
                {
                    OpenStrategy(data, primary, start);
                }
            }
        }

        private void AfterStopWin(StrategyData data, bool isMoveStopWin, DateTime start)
        {
            if (isMoveStopWin)
            {
                if (data.MoveStopWinQty == 0)
                {
                    return;
                }
            }
            else if (data.MoveStopWinQty != 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.CloseTriggerAfterStopWin))
            {
                HashSet<string> targets = new HashSet<string>(data.CloseTriggerAfterStopWin.Split(','));

                foreach (string primary in targets)
                {
                    _appCtrl.Trigger.Cancel(primary, "策略取消");
                }
            }

            if (!string.IsNullOrWhiteSpace(data.CloseStrategyAfterStopWin))
            {
                HashSet<string> targets = new HashSet<string>(data.CloseStrategyAfterStopWin.Split(','));

                foreach (string primary in targets)
                {
                    Close(primary, 0, "策略停止");
                }
            }

            if (!string.IsNullOrWhiteSpace(data.OpenTriggerAfterStopWin))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenTriggerAfterStopWin.Split(','));

                foreach (string primary in targets)
                {
                    OpenTrigger(data, primary, start);
                }
            }

            if (!string.IsNullOrWhiteSpace(data.OpenStrategyAfterStopWin))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenStrategyAfterStopWin.Split(','));

                foreach (string primary in targets)
                {
                    OpenStrategy(data, primary, start);
                }
            }
        }

        private bool UpdateStatus(StrategyData data, QuoteData quote, DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            bool saveData = false;

            lock (data.SyncRoot)
            {
                if (_appCtrl.CAPQuote.Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    return saveData;
                }
                else if (quote.Simulate != QuoteData.RealTrade)
                {
                    return saveData;
                }
                else if (quote.DealPrice == 0)
                {
                    return saveData;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Cancelled)
                {
                    return saveData;
                }
                else if (data.StopLossData != null || data.MarketClosingData != null)
                {
                    return saveData;
                }
                else if (data.UnclosedQty <= 0)
                {
                    return saveData;
                }
                else if (data.StatusEnum == StrategyStatus.Enum.Waiting)
                {
                    data.MarketPrice = quote.DealPrice;
                    return saveData;
                }

                data.MarketPrice = quote.DealPrice;
                data.UnclosedProfit = (data.MarketPrice - data.DealPrice) * data.UnclosedQty * (data.BSEnum == OrderBS.Enum.Buy ? 1 : -1);
                data.Updater = methodName;
                data.UpdateTime = DateTime.Now;

                if (data.BSEnum == OrderBS.Enum.Buy)
                {
                    if (data.MoveStopWinPrice < data.MarketPrice)
                    {
                        data.MoveStopWinPrice = data.MarketPrice;
                    }
                }
                else if ((data.MoveStopWinPrice > data.MarketPrice || data.MoveStopWinPrice == 0) && data.MarketPrice != 0)
                {
                    data.MoveStopWinPrice = data.MarketPrice;
                }

                if (data.UnclosedQty > 0)
                {
                    if (data.UnclosedProfit > 0 && data.WinCloseSeconds > 0 && DateTime.Now >= data.WinCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        Close(data, data.WinCloseQty, "收盤獲利減倉", start);

                        saveData = true;
                        return saveData;
                    }
                    else if (data.UnclosedProfit <= 0 && data.LossCloseSeconds > 0 && DateTime.Now >= data.LossCloseTime && DateTime.Now < _appCtrl.CAPQuote.MarketCloseTime)
                    {
                        Close(data, data.LossCloseQty, "收盤損失減倉", start);

                        saveData = true;
                        return saveData;
                    }
                }

                if (data.StatusEnum == StrategyStatus.Enum.OrderSent || data.StatusEnum == StrategyStatus.Enum.OrderReport || data.StatusEnum == StrategyStatus.Enum.DealReport || data.StatusEnum == StrategyStatus.Enum.OrderError)
                {
                    if (data.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (data.MarketPrice <= data.StopLossAfter)
                        {
                            StrategyData stopLossOrder = data.CreateStopLossOrder();

                            data.StatusEnum = StrategyStatus.Enum.StopLossSent;

                            _appCtrl.CAPOrder.Send(stopLossOrder);

                            saveData = true;
                            AfterStopLoss(data, start);
                        }
                        else if (data.MarketPrice >= data.StopWinPrice && data.StopWinQty <= 0)
                        {
                            StrategyData stopWinOrder = data.CreateStopWinOrder();

                            data.StatusEnum = StrategyStatus.Enum.StopWinSent;

                            if (data.StopWinQty == 0)
                            { } //滿足條件但不減倉
                            else
                            {
                                _appCtrl.CAPOrder.Send(stopWinOrder);
                            }

                            saveData = true;
                            AfterStopWin(data, false, start);
                        }
                    }
                    else if (data.MarketPrice >= data.StopLossAfter) //data.BSEnum == OrderBS.Enum.Sell
                    {
                        StrategyData stopLossOrder = data.CreateStopLossOrder();

                        data.StatusEnum = StrategyStatus.Enum.StopLossSent;

                        _appCtrl.CAPOrder.Send(stopLossOrder);

                        saveData = true;
                        AfterStopLoss(data, start);
                    }
                    else if (data.MarketPrice <= data.StopWinPrice && data.StopWinQty <= 0)
                    {
                        StrategyData stopWinOrder = data.CreateStopWinOrder();

                        data.StatusEnum = StrategyStatus.Enum.StopWinSent;

                        if (data.StopWinQty == 0)
                        { } //滿足條件但不減倉
                        else
                        {
                            _appCtrl.CAPOrder.Send(stopWinOrder);
                        }

                        saveData = true;
                        AfterStopWin(data, false, start);
                    }
                }
                else if (data.MoveStopWinQty <= 0 && data.MoveStopWinData == null && data.StopWinData != null && data.StopWinData.OrderQty < data.OrderQty)
                {
                    if (data.BSEnum == OrderBS.Enum.Buy)
                    {
                        if (data.MarketPrice <= data.MoveStopWinPrice + data.MoveStopWinOffset)
                        {
                            StrategyData moveStopWinOrder = data.CreateMoveStopWinOrder();

                            data.StatusEnum = StrategyStatus.Enum.MoveStopWinSent;

                            if (data.MoveStopWinQty == 0)
                            { } //滿足條件但不減倉
                            else
                            {
                                _appCtrl.CAPOrder.Send(moveStopWinOrder);
                            }

                            saveData = true;
                            AfterStopWin(data, true, start);
                        }
                    }
                    else if (data.MarketPrice >= data.MoveStopWinPrice + data.MoveStopWinOffset)
                    {
                        StrategyData moveStopWinOrder = data.CreateMoveStopWinOrder();

                        data.StatusEnum = StrategyStatus.Enum.MoveStopWinSent;

                        if (data.MoveStopWinQty == 0)
                        { } //滿足條件但不減倉
                        else
                        {
                            _appCtrl.CAPOrder.Send(moveStopWinOrder);
                        }

                        saveData = true;
                        AfterStopWin(data, true, start);
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
                    SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat);
                }
            }

            bool saveData = false;

            for (int i = _dataCollection.Count - 1; i >= 0; --i)
            {
                try
                {
                    StrategyData data = _dataCollection[i];

                    if (UpdateStatus(data, data.Quote, start))
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
                SaveData(_dataMap.Values, _appCtrl.Config.StrategyFolder, _appCtrl.Settings.StrategyFileFormat);
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

            _waitToAdd.Enqueue(data);
        }

        public void AddRule(StrategyData data)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}", UniqueName);

            try
            {
                ParentCheck(data, false, start);
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

        private void SerialReset(StrategyData data)
        {
            if (data == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.OpenStrategyAfterStopLoss))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenStrategyAfterStopLoss.Split(','));

                foreach (string primary in targets)
                {
                    SerialReset(this[primary]);
                }
            }

            if (!string.IsNullOrWhiteSpace(data.OpenStrategyAfterStopWin))
            {
                HashSet<string> targets = new HashSet<string>(data.OpenStrategyAfterStopWin.Split(','));

                foreach (string primary in targets)
                {
                    SerialReset(this[primary]);
                }
            }

            data.Reset();
        }

        public void StartNow(string primaryKey)
        {
            DateTime start = _appCtrl.StartTrace($"primaryKey={primaryKey}", UniqueName);

            StrategyData data = this[primaryKey.Replace(" ", string.Empty)];
            SerialReset(data);
            ParentCheck(data, true, start);

            StrategyData order = data.CreateOrder();

            data.StatusEnum = StrategyStatus.Enum.OrderSent;

            _appCtrl.CAPOrder.Send(order);
        }

        public void CreateAndAddOrder(StrategyData data, OpenInterestData openInterest)
        {
            DateTime start = _appCtrl.StartTrace($"{data?.ToLog()}|{openInterest?.ToLog()}", UniqueName);

            try
            {
                QuoteData quoteBK = data.Quote;
                data.Quote = quoteBK.DeepClone();
                data.Quote.DealPrice = openInterest.AveragePrice;
                data.Quote.Simulate = 1;

                bool sendRealOrder = data.SendRealOrder;
                data.SendRealOrder = false;

                SerialReset(data);
                ParentCheck(data, true, start);

                StrategyData order = data.CreateOrder();

                data.StatusEnum = StrategyStatus.Enum.OrderSent;

                _appCtrl.CAPOrder.Send(order);

                data.Quote = quoteBK;

                data.DealPrice = openInterest.AveragePrice;
                order.DealPrice = openInterest.AveragePrice;

                data.SendRealOrder = sendRealOrder;
                order.SendRealOrder = sendRealOrder;
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

        public void RecoverSetting(FileInfo fileStrategy = null, FileInfo fileSentOrder = null)
        {
            const string methodName = nameof(RecoverSetting);

            DateTime start = _appCtrl.StartTrace($"{fileStrategy?.FullName}", UniqueName);

            try
            {
                if (_dataMap.Count > 0)
                {
                    return;
                }

                if (fileStrategy == null)
                {
                    if (_appCtrl.Config.StrategyFolder == null)
                    {
                        return;
                    }

                    _appCtrl.Config.StrategyFolder.Refresh();
                    fileStrategy = _appCtrl.Config.StrategyFolder.GetFiles("*.csv").LastOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).Length == _appCtrl.Settings.StrategyFileFormat.Length);
                }

                if (fileStrategy == null)
                {
                    return;
                }

                List<string> columnNames = new List<string>();
                decimal nextPK = -1;

                foreach (StrategyData data in StrategyData.ForeachQuoteFromCSVFile(fileStrategy.FullName, columnNames))
                {
                    try
                    {
                        data.Trim();

                        if (string.IsNullOrWhiteSpace(data.PrimaryKey))
                        {
                            continue;
                        }

                        data.UnclosedQty = 0;
                        data.Reset();

                        data.MarketType = _appCtrl.CAPOrder[data.FullAccount].MarketType;
                        data.Quote = _appCtrl.CAPQuote[data.Symbol];
                        data.Updater = methodName;
                        data.UpdateTime = DateTime.Now;

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

                if (_dataCollection.Count >= nextPK)
                {
                    nextPK = _dataCollection.Count + 1;
                }

                _appCtrl.MainForm.InvokeAsync(delegate { _appCtrl.MainForm.TextBoxStrategyPrimaryKey.Text = $"{nextPK}"; });
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
