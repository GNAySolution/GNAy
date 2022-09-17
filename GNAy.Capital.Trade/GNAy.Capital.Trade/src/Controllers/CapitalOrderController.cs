using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalOrderController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly object _syncOrderLock;

        private SKOrderLib m_pSKOrder;

        public int ReadCertResult { get; private set; }

        private readonly ObservableCollection<OrderAccData> _dataCollection;

        public int Count => _dataCollection.Count;
        public OrderAccData this[string account] => _dataCollection.FirstOrDefault(x => x.FullAccount == account || x.Account == account);
        public OrderAccData this[int index] => _dataCollection[index];

        private readonly ObservableCollection<string> _buySell;
        private readonly ObservableCollection<string> _tradeTypes;
        private readonly ObservableCollection<string> _dayTrade;
        private readonly ObservableCollection<string> _positionKinds;

        public string Notice { get; private set; }

        public CapitalOrderController(in AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(CapitalOrderController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _syncOrderLock = new object();

            ReadCertResult = -1;

            _dataCollection = _appCtrl.MainForm.ComboBoxOrderAccs.SetViewAndGetObservation<OrderAccData>();

            _buySell = _appCtrl.MainForm.ComboBoxOrderBuySell.SetViewAndGetObservation(OrderBS.Description);
            _appCtrl.MainForm.ComboBoxOrderBuySell.SelectedIndex = (int)OrderBS.Enum.Buy;

            _tradeTypes = _appCtrl.MainForm.ComboBoxOrderTradeType.SetViewAndGetObservation(OrderTradeType.Description);
            _appCtrl.MainForm.ComboBoxOrderTradeType.SelectedIndex = (int)OrderTradeType.Enum.ROD;

            _dayTrade = _appCtrl.MainForm.ComboBoxOrderDayTrade.SetViewAndGetObservation(OrderDayTrade.Description);
            _appCtrl.MainForm.ComboBoxOrderDayTrade.SelectedIndex = (int)OrderDayTrade.Enum.No;

            _positionKinds = _appCtrl.MainForm.ComboBoxOrderPositionKind.SetViewAndGetObservation(OrderPosition.Description);
            _appCtrl.MainForm.ComboBoxOrderPositionKind.SelectedIndex = (int)OrderPosition.Enum.Open;

            Notice = string.Empty;
        }

        private CapitalOrderController() : this(null)
        { }

        public int ReadCertification()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (m_pSKOrder != null)
                {
                    return ReadCertResult;
                }

                m_pSKOrder = new SKOrderLib();
                m_pSKOrder.OnAccount += m_OrderObj_OnAccount;
                m_pSKOrder.OnAsyncOrder += m_pSKOrder_OnAsyncOrder;
                m_pSKOrder.OnAsyncOrderOLID += m_pSKOrder_OnAsyncOrderOLID;
                m_pSKOrder.OnRealBalanceReport += m_pSKOrder_OnRealBalanceReport;
                m_pSKOrder.OnOpenInterest += m_pSKOrder_OnOpenInterest;
                m_pSKOrder.OnStopLossReport += m_pSKOrder_OnStopLossReport;
                m_pSKOrder.OnFutureRights += m_pSKOrder_OnFutureRights;
                m_pSKOrder.OnRequestProfitReport += m_pSKOrder_OnRequestProfitReport;
                m_pSKOrder.OnMarginPurchaseAmountLimit += m_pSKOrder_OnMarginPurchaseAmountLimit;
                m_pSKOrder.OnBalanceQuery += m_pSKOrder_OnBalanceQueryReport;
                m_pSKOrder.OnTSSmartStrategyReport += m_pSKOrder_OnTSStrategyReport;
                m_pSKOrder.OnProfitLossGWReport += m_pSKOrder_OnTSProfitLossGWReport;
                m_pSKOrder.OnOFOpenInterestGWReport += m_pSKOrder_OnOFOpenInterestGW;
                m_pSKOrder.OnTelnetTest += m_pSKOrder_OnTelnetTest;

                ReadCertResult = m_pSKOrder.SKOrderLib_Initialize(); //下單物件初始化。産生下單物件後需先執行初始動作
                _appCtrl.CAPCenter.LogAPIMessage(start, ReadCertResult);

                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
                }

                //讀取憑證資訊。委託下單必須透過憑證，因此當元件初始化成功後即需要做讀取憑證的動作，如果使用群組的帳號做初始，則必須自行將所有的帳號依序做讀取憑證的動作。
                //如果送出委託前未經讀取憑證，送委託會得到 SK_ERROR_ORDER_SIGN_INVALID 的錯誤
                ReadCertResult = m_pSKOrder.ReadCertByID(_appCtrl.CAPCenter.UserID);
                _appCtrl.CAPCenter.LogAPIMessage(start, ReadCertResult);

                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
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

            return ReadCertResult;
        }

        public void GetAccounts()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _dataCollection.Clear();

                int m_nCode = m_pSKOrder.GetUserAccount(); //取回目前可交易的所有帳號。資料由OnAccount事件回傳

                if (m_nCode != 0)
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
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

        public void Unlock(in int marketType = -1)
        {
            DateTime start = _appCtrl.StartTrace($"{nameof(marketType)}={marketType}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Length)
                {
                    int m_nCode = m_pSKOrder.UnlockOrder(marketType); //下單解鎖。下單函式上鎖後需經由此函式解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Length)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Length({Market.CodeDescription.Length})");
                }

                for (int i = 0; i < Market.CodeDescription.Length; ++i)
                {
                    Unlock(i);
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

        public void SetMaxQty(in int marketType = -1, int maxQty = -1)
        {
            if (maxQty < 0)
            {
                maxQty = _appCtrl.Settings.OrderMaxQty;
            }

            DateTime start = _appCtrl.StartTrace($"{nameof(marketType)}={marketType}|{nameof(maxQty)}={maxQty}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Length)
                {
                    int m_nCode = m_pSKOrder.SetMaxQty(marketType, maxQty); //設定每秒委託「量」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Length)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Length({Market.CodeDescription.Length})");
                }

                for (int i = 0; i < Market.CodeDescription.Length; ++i)
                {
                    SetMaxQty(i, maxQty);
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

        public void SetMaxCount(in int marketType = -1, int maxCount = -1)
        {
            if (maxCount < 0)
            {
                maxCount = _appCtrl.Settings.OrderMaxCount;
            }

            DateTime start = _appCtrl.StartTrace($"{nameof(marketType)}={marketType}|{nameof(maxCount)}={maxCount}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Length)
                {
                    int m_nCode = m_pSKOrder.SetMaxCount(marketType, maxCount); //設定每秒委託「筆數」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Length)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Length({Market.CodeDescription.Length})");
                }

                for (int i = 0; i < Market.CodeDescription.Length; ++i)
                {
                    SetMaxCount(i, maxCount);
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

        public int GetOpenInterest(in string orderAcc, in int format = 1)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(_appCtrl.CAPCenter.UserID, orderAcc, format); //查詢期貨未平倉－可指定回傳格式

                if (m_nCode != 0)
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, $"{nameof(orderAcc)}={orderAcc}|{nameof(format)}={format}");
                }

                return m_nCode;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, $"{nameof(orderAcc)}={orderAcc}|{nameof(format)}={format}|{ex.StackTrace}");
            }

            return -1;
        }

        public int GetFuturesRights(in string orderAcc, in short coinType = 1)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                int m_nCode = m_pSKOrder.GetFutureRights(_appCtrl.CAPCenter.UserID, orderAcc, coinType); //查詢國內權益數 //0:全幣別，1:基幣(台幣TWD)，2:人民幣RMB

                if (m_nCode != 0)
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, $"{nameof(orderAcc)}={orderAcc}|{nameof(coinType)}={coinType}");
                }

                return m_nCode;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, $"{nameof(orderAcc)}={orderAcc}|{nameof(coinType)}={coinType}|{ex.StackTrace}");
            }

            return -1;
        }

        private FUTUREORDER CreateCaptialFutures(in StrategyData order)
        {
            FUTUREORDER pFutureOrder = new FUTUREORDER()
            {
                bstrFullAccount = order.FullAccount,
                bstrStockNo = order.Symbol,
                sBuySell = (short)order.BSEnum,
                sTradeType = (short)order.TradeTypeEnum,
                sDayTrade = (short)order.DayTradeEnum,
                sNewClose = (short)order.PositionEnum,
                bstrPrice = (order.OrderPriceBefore == OrderPrice.M || order.OrderPriceBefore == OrderPrice.P) ? order.OrderPriceBefore : order.OrderPriceAfter.ToString("0.00"),
                nQty = order.OrderQty,
            };

            return pFutureOrder;
        }

        private (LogLevel, string) SendFutures(in StrategyData order, in DateTime start)
        {
            string orderResult = string.Empty;

            if (order.TradeTypeEnum == OrderTradeType.Enum.ROD)
            {
                FUTUREORDER capOrder = CreateCaptialFutures(order);

                //送出期貨委託，無需倉位，預設為盤中，不可更改
                //SKReplyLib.OnNewData，當有回報將主動呼叫函式，並通知委託的狀態。(新格式 包含預約單回報)
                int m_nCode = m_pSKOrder.SendFutureOrder(_appCtrl.CAPCenter.UserID, false, capOrder, out string orderMsg);
                orderResult = orderMsg;

                Thread.Sleep(_appCtrl.Settings.OrderTimeInterval);

                return m_nCode == 0 ? (LogLevel.Trace, orderResult) : _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, orderResult);
            }

            int succeededCnt = 0;
            (LogLevel, string) output = (LogLevel.Error, orderResult);

            //for (int i = 0; i < order.OrderQty * 8; ++i)
            for (int i = 0; i < order.OrderQty * 1; ++i)
            {
                FUTUREORDER capOrder = CreateCaptialFutures(order);
                capOrder.nQty = 1;

                int m_nCode = m_pSKOrder.SendFutureOrder(_appCtrl.CAPCenter.UserID, false, capOrder, out string orderMsg);
                orderResult = orderMsg;

                Thread.Sleep(_appCtrl.Settings.OrderTimeInterval);

                if (m_nCode == 0)
                {
                    ++succeededCnt;

                    if (succeededCnt >= order.OrderQty)
                    {
                        return (LogLevel.Trace, orderResult);
                    }
                }
                else
                {
                    output = _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, orderResult);

                    if (i == order.OrderQty * 8 - 1)
                    {
                        _appCtrl.LogError(start, $"委託部份失敗|{nameof(succeededCnt)}={succeededCnt}|failed={order.OrderQty - succeededCnt}|{order.ToLog()}", UniqueName);

                        return output;
                    }
                }
            }

            return output;
        }

        private void SendFuturesLimitStopWin(in StrategyData order, in DateTime start)
        {
            StrategyData parent = order.Parent;
            StrategyData stopWinOrder = null;

            if (parent == null || order != parent.OrderData)
            {
                return;
            }
            else if (parent.StopWin1Data == null && parent.StopWin1Offset == 0 && parent.StopWin1Qty < 0)
            {
                stopWinOrder = parent.CreateStopWinOrder(StrategyData.StopWin1);
                parent.StopWin1Data = null;
            }
            else if (parent.StopWin2Data == null && parent.StopWin2Offset == 0 && parent.StopWin2Qty < 0)
            {
                stopWinOrder = parent.CreateStopWinOrder(StrategyData.StopWin2);
                parent.StopWin2Data = null;
            }
            else
            {
                return;
            }

            stopWinOrder.TradeTypeEnum = OrderTradeType.Enum.ROD;
            stopWinOrder.OrderPriceBefore = parent.StopWinPriceAAfterRaw.ToString("0.00");
            stopWinOrder.OrderPriceAfter = parent.StopWinPriceAAfterRaw;

            int succeededCnt = 0;

            //for (int i = 0; i < stopWinOrder.OrderQty * 8; ++i)
            for (int i = 0; i < stopWinOrder.OrderQty * 1; ++i)
            {
                FUTUREORDER capOrder = CreateCaptialFutures(stopWinOrder);
                capOrder.nQty = 1;

                int m_nCode = m_pSKOrder.SendFutureOrder(_appCtrl.CAPCenter.UserID, false, capOrder, out string orderMsg);

                Thread.Sleep(_appCtrl.Settings.OrderTimeInterval);

                if (m_nCode == 0 && !string.IsNullOrWhiteSpace(orderMsg))
                {
                    ++succeededCnt;

                    _appCtrl.LogTrace(start, $"限價停利委託成功|{nameof(succeededCnt)}={succeededCnt}|{nameof(orderMsg)}={orderMsg}|{stopWinOrder.ToLog()}", UniqueName);

                    parent.OrdersSeqNoQueue.Enqueue(orderMsg);

                    if (succeededCnt >= stopWinOrder.OrderQty)
                    {
                        parent.OrdersSeqNos = string.Join(",", parent.OrdersSeqNoQueue);

                        return;
                    }
                }
                else
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);

                    if (i == stopWinOrder.OrderQty * 8 - 1)
                    {
                        _appCtrl.LogError(start, $"委託部份失敗|{nameof(succeededCnt)}={succeededCnt}|{nameof(orderMsg)}={orderMsg}|failed={stopWinOrder.OrderQty - succeededCnt}|{stopWinOrder.ToLog()}", UniqueName);

                        parent.OrdersSeqNos = string.Join(",", parent.OrdersSeqNoQueue);

                        return;
                    }
                }
            }
        }

        private (LogLevel, string) SendOption(in StrategyData order, in DateTime start)
        {
            FUTUREORDER capOrder = CreateCaptialFutures(order);
            string orderMsg = string.Empty;
            int m_nCode = m_pSKOrder.SendOptionOrder(_appCtrl.CAPCenter.UserID, false, capOrder, out orderMsg);

            Thread.Sleep(_appCtrl.Settings.OrderTimeInterval);

            return m_nCode == 0 ? (LogLevel.Trace, orderMsg) : _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, orderMsg);
        }

        private void SendAsync(in StrategyData order, in DateTime start, [CallerMemberName] in string memberName = "")
        {
            try
            {
                (LogLevel, string) orderResult = (LogLevel.Trace, $"{nameof(AppSettings.SendRealOrder)}={_appCtrl.Settings.SendRealOrder && order.SendRealOrder}"); //如果回傳值為 0表示委託成功，訊息內容則為13碼的委託序號

                if (_appCtrl.Settings.SendRealOrder && order.SendRealOrder)
                {
                    lock (_syncOrderLock)
                    {
                        switch (order.MarketType)
                        {
                            case Market.EType.Futures:
                                orderResult = SendFutures(order, start);
                                SendFuturesLimitStopWin(order, start);
                                break;

                            case Market.EType.Option:
                                orderResult = SendOption(order, start);
                                break;

                            default:
                                throw new NotSupportedException(order.ToLog());
                        }
                    }
                }

                Notice = orderResult.Item2;

                order.StatusEnum = orderResult.Item1 == LogLevel.Trace ? StrategyStatus.Enum.OrderReport : StrategyStatus.Enum.OrderError;
                order.OrderReport = orderResult.Item2;
                order.Updater = memberName;
                order.UpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;

                order.StatusEnum = StrategyStatus.Enum.OrderError;
                order.OrderReport = ex.Message;
                order.Updater = memberName;
                order.UpdateTime = DateTime.Now;
            }
            finally
            {
                StrategyData parent = order.Parent;

                if (parent != null)
                {
                    switch (parent.StatusEnum)
                    {
                        case StrategyStatus.Enum.OrderSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.OrderReport : StrategyStatus.Enum.OrderError;
                            break;
                        case StrategyStatus.Enum.StopLossSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.StopLossOrderReport : StrategyStatus.Enum.StopLossError;
                            break;
                        case StrategyStatus.Enum.StopWinSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.StopWinOrderReport : StrategyStatus.Enum.StopWinError;
                            break;
                        case StrategyStatus.Enum.MarketClosingSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.MarketClosingOrderReport : StrategyStatus.Enum.MarketClosingError;
                            break;
                    }

                    parent.Updater = memberName;
                    parent.UpdateTime = DateTime.Now;
                }

                _appCtrl.Strategy.SaveData(_appCtrl.OrderDetail.DataCollection, _appCtrl.Config.SentOrderFolder, _appCtrl.Settings.SentOrderFileFormat);

                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void Send(StrategyData order, [CallerMemberName] string memberName = "")
        {
            DateTime start = _appCtrl.StartTrace($"{order?.ToLog()}", UniqueName);

            try
            {
                if (string.IsNullOrWhiteSpace(order.PrimaryKey))
                {
                    order.PrimaryKey = $"{order.CreatedTime:HHmmss}_{StrategyStatus.Enum.OrderSent}";

                    if (_appCtrl.Strategy[order.PrimaryKey] != null)
                    {
                        throw new ArgumentException($"_appCtrl.Strategy[{order.PrimaryKey}] != null|{order.ToLog()}");
                    }
                }
                else if (_appCtrl.Strategy[order.PrimaryKey] == order)
                {
                    throw new ArgumentException($"_appCtrl.Strategy[{order.PrimaryKey}] == order|{order.ToLog()}");
                }

                _appCtrl.OrderDetail.Add(order);
                _appCtrl.OrderDetail.Check(order, start);

                order.StatusEnum = StrategyStatus.Enum.OrderSent;

                if (_appCtrl.Settings.SendRealOrder && order.SendRealOrder && order.OrderQty > 0)
                {
                    Task.Factory.StartNew(() => SendAsync(order, start, memberName));
                }
                else
                {
                    SendAsync(order, start, memberName);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;

                order.StatusEnum = StrategyStatus.Enum.OrderError;
                order.OrderReport = ex.Message;
                order.Updater = memberName;
                order.UpdateTime = DateTime.Now;

                StrategyData parent = order.Parent;

                if (parent != null)
                {
                    switch (parent.StatusEnum)
                    {
                        case StrategyStatus.Enum.OrderSent:
                            parent.StatusEnum = StrategyStatus.Enum.OrderError;
                            break;
                        case StrategyStatus.Enum.StopLossSent:
                            parent.StatusEnum = StrategyStatus.Enum.StopLossError;
                            break;
                        case StrategyStatus.Enum.StopWinSent:
                            parent.StatusEnum = StrategyStatus.Enum.StopWinError;
                            break;
                        case StrategyStatus.Enum.MarketClosingSent:
                            parent.StatusEnum = StrategyStatus.Enum.MarketClosingError;
                            break;
                    }

                    parent.Updater = memberName;
                    parent.UpdateTime = DateTime.Now;
                }
            }
        }

        public int CancelBySeqNo(in string fullAccount, in string seqNo, in DateTime start)
        {
            int m_nCode = 0;

            try
            {
                m_nCode = m_pSKOrder.CancelOrderBySeqNo(_appCtrl.CAPCenter.UserID, false, fullAccount, seqNo, out string strMessage); //國內委託删單(By委託序號)
                _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, strMessage);

                //取消送出[cancel send], 取消結果請查詢委託回報
                //測試
                if (strMessage.Contains("取消送出"))
                {
                    Thread.Sleep(_appCtrl.Settings.OrderTimeInterval);

                    int _Code = m_pSKOrder.CancelOrderBySeqNo(_appCtrl.CAPCenter.UserID, false, fullAccount, seqNo, out string msg);
                    _appCtrl.CAPCenter.LogAPIMessage(start, _Code, msg);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            return m_nCode;
        }

        public void CancelBySeqNo(in OrderAccData acc, in string seqNo)
        {
            DateTime start = _appCtrl.StartTrace($"{acc?.FullAccount}|{nameof(seqNo)}={seqNo}", UniqueName);

            try
            {
                CancelBySeqNo(acc.FullAccount, seqNo, start);
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
