﻿using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public IReadOnlyList<OrderAccData> DataCollection => _dataCollection;

        private readonly ObservableCollection<string> _buySell;
        private readonly ObservableCollection<string> _tradeTypes;
        private readonly ObservableCollection<string> _dayTrade;
        private readonly ObservableCollection<string> _positionKinds;

        public string Notice { get; private set; }

        public CapitalOrderController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(CapitalOrderController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _syncOrderLock = new object();

            ReadCertResult = -1;

            _dataCollection = _appCtrl.MainForm.ComboBoxOrderAccs.SetAndGetItemsSource<OrderAccData>();

            _buySell = _appCtrl.MainForm.ComboBoxOrderBuySell.SetAndGetItemsSource(OrderBS.Description);
            _appCtrl.MainForm.ComboBoxOrderBuySell.SelectedIndex = (int)OrderBS.Enum.Buy;

            _tradeTypes = _appCtrl.MainForm.ComboBoxOrderTradeType.SetAndGetItemsSource(OrderTradeType.Description);
            _appCtrl.MainForm.ComboBoxOrderTradeType.SelectedIndex = (int)OrderTradeType.Enum.ROD;

            _dayTrade = _appCtrl.MainForm.ComboBoxOrderDayTrade.SetAndGetItemsSource(OrderDayTrade.Description);
            _appCtrl.MainForm.ComboBoxOrderDayTrade.SelectedIndex = (int)OrderDayTrade.Enum.No;

            _positionKinds = _appCtrl.MainForm.ComboBoxOrderPositionKind.SetAndGetItemsSource(OrderPosition.Description);
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

        public void Unlock(int marketType = -1)
        {
            DateTime start = _appCtrl.StartTrace($"marketType={marketType}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.UnlockOrder(marketType); //下單解鎖。下單函式上鎖後需經由此函式解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
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

        public void SetMaxQty(int marketType = -1, int maxQty = -1)
        {
            if (maxQty <= 0)
            {
                maxQty = _appCtrl.Settings.OrderMaxQty;
            }

            DateTime start = _appCtrl.StartTrace($"marketType={marketType}|maxQty={maxQty}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxQty(marketType, maxQty); //設定每秒委託「量」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
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

        public void SetMaxCount(int marketType = -1, int maxCount = -1)
        {
            if (maxCount <= 0)
            {
                maxCount = _appCtrl.Settings.OrderMaxCount;
            }

            DateTime start = _appCtrl.StartTrace($"marketType={marketType}|maxCount={maxCount}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxCount(marketType, maxCount); //設定每秒委託「筆數」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                    }

                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
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

        public int GetOpenInterest(string orderAcc = "", int format = 1)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(_appCtrl.CAPCenter.UserID, orderAcc, format); //查詢期貨未平倉－可指定回傳格式

                if (m_nCode != 0)
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, $"orderAcc={orderAcc}|format={format}");
                }

                return m_nCode;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, $"orderAcc={orderAcc}|format={format}|{ex.StackTrace}");
            }

            return -1;
        }

        private FUTUREORDER CreateCaptialFutures(StrategyData order)
        {
            FUTUREORDER pFutureOrder = new FUTUREORDER()
            {
                bstrFullAccount = order.FullAccount,
                bstrStockNo = order.Symbol,
                sBuySell = order.BS,
                sTradeType = order.TradeType,
                sDayTrade = order.DayTrade,
                sNewClose = order.Position,
                bstrPrice = (order.OrderPriceBefore == OrderPrice.M || order.OrderPriceBefore == OrderPrice.P) ? order.OrderPriceBefore : order.OrderPriceAfter.ToString("0.00"),
                nQty = order.OrderQty,
            };

            return pFutureOrder;
        }

        public void SendTW(StrategyData order)
        {
            const string methodName = nameof(SendTW);

            DateTime start = _appCtrl.StartTrace($"{order?.ToLog()}", UniqueName);

            StrategyData parent = order.Parent;

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

                FUTUREORDER pFutureOrder = CreateCaptialFutures(order);

                string orderMsg = $"_appCtrl.Settings.SendRealOrder={_appCtrl.Settings.SendRealOrder}"; //如果回傳值為 0表示委託成功，訊息內容則為13碼的委託序號
                int m_nCode = 0;
                (LogLevel, string) apiMsg = (LogLevel.Trace, orderMsg);

                order.StatusEnum = StrategyStatus.Enum.OrderSent;
                _appCtrl.Strategy.SaveData(_appCtrl.OrderDetail.DataCollection, _appCtrl.Config.SentOrderFolder, _appCtrl.Settings.SentOrderFileFormat);

                if (_appCtrl.Settings.SendRealOrder)
                {
                    lock (_syncOrderLock)
                    {
                        if (order.MarketType == Market.EType.Futures)
                        {
                            //送出期貨委託，無需倉位，預設為盤中，不可更改
                            //SKReplyLib.OnNewData，當有回報將主動呼叫函式，並通知委託的狀態。(新格式 包含預約單回報)
                            m_nCode = m_pSKOrder.SendFutureOrder(_appCtrl.CAPCenter.UserID, false, pFutureOrder, out orderMsg);
                        }
                        else if (order.MarketType == Market.EType.Option)
                        {
                            m_nCode = m_pSKOrder.SendOptionOrder(_appCtrl.CAPCenter.UserID, false, pFutureOrder, out orderMsg);
                        }

                        apiMsg = _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode, orderMsg);

                        Thread.Sleep(100);
                    }
                }
                else
                {
                    _appCtrl.Log(apiMsg.Item1, $"m_nCode={m_nCode}|{orderMsg}", UniqueName, DateTime.Now - start);
                }

                Notice = orderMsg;

                order.StatusEnum = m_nCode == 0 ? StrategyStatus.Enum.OrderReport : StrategyStatus.Enum.OrderError;
                order.OrderReport = orderMsg;
                order.Updater = methodName;
                order.UpdateTime = DateTime.Now;

                if (!_appCtrl.Settings.SendRealOrder)
                {
                    order.StatusEnum = StrategyStatus.Enum.DealReport;
                    order.DealPrice = order.OrderPriceAfter;
                    order.DealQty = order.OrderQty;
                    order.DealReport = order.OrderReport;
                    order.Updater = methodName;
                    order.UpdateTime = DateTime.Now;

                    if (parent == null)
                    {
                        if (order.PositionEnum == OrderPosition.Enum.Open)
                        {
                            order.UnclosedQty = order.DealQty;
                        }
                    }
                    else
                    {
                        switch (parent.StatusEnum)
                        {
                            case StrategyStatus.Enum.OrderSent:
                                parent.StatusEnum = StrategyStatus.Enum.DealReport;
                                break;
                            case StrategyStatus.Enum.StopLossSent:
                                parent.StatusEnum = StrategyStatus.Enum.StopLossDealReport;
                                break;
                            case StrategyStatus.Enum.StopWinSent:
                                parent.StatusEnum = StrategyStatus.Enum.StopWinDealReport;
                                break;
                            case StrategyStatus.Enum.MoveStopWinSent:
                                parent.StatusEnum = StrategyStatus.Enum.MoveStopWinDealReport;
                                break;
                            case StrategyStatus.Enum.MarketClosingSent:
                                parent.StatusEnum = StrategyStatus.Enum.MarketClosingDealReport;
                                break;
                        }

                        if (order == parent.OrderData)
                        {
                            order.ClosedProfit = 0;
                            order.UnclosedQty = order.DealQty;
                            order.UnclosedProfit = 0;

                            parent.ClosedProfit += order.ClosedProfit;
                            parent.UnclosedQty = order.UnclosedQty;
                        }
                        else if (order == parent.StopLossData || order == parent.StopWinData || order == parent.MoveStopWinData || order == parent.MarketClosingData)
                        {
                            order.ClosedProfit = (order.DealPrice - parent.OrderData.DealPrice) * order.DealQty * (parent.OrderData.BSEnum == OrderBS.Enum.Buy ? 1 : -1);
                            order.UnclosedQty = parent.UnclosedQty - order.DealQty;
                            order.UnclosedProfit = (order.DealPrice - parent.OrderData.DealPrice) * order.UnclosedQty * (parent.OrderData.BSEnum == OrderBS.Enum.Buy ? 1 : -1);

                            parent.ClosedProfit += order.ClosedProfit;
                            parent.UnclosedQty = order.UnclosedQty;
                        }

                        parent.Updater = methodName;
                        parent.UpdateTime = DateTime.Now;
                    }
                }

                _appCtrl.Strategy.SaveData(_appCtrl.OrderDetail.DataCollection, _appCtrl.Config.SentOrderFolder, _appCtrl.Settings.SentOrderFileFormat);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                Notice = ex.Message;

                order.StatusEnum = StrategyStatus.Enum.OrderError;
                order.OrderReport = ex.Message;
                order.Updater = methodName;
                order.UpdateTime = DateTime.Now;
            }
            finally
            {
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
                        case StrategyStatus.Enum.MoveStopWinSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.MoveStopWinOrderReport : StrategyStatus.Enum.MoveStopWinError;
                            break;
                        case StrategyStatus.Enum.MarketClosingSent:
                            parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.MarketClosingOrderReport : StrategyStatus.Enum.MarketClosingError;
                            break;
                    }

                    parent.Updater = methodName;
                    parent.UpdateTime = DateTime.Now;
                }

                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void SendTWAsync(StrategyData order)
        {
            Task.Factory.StartNew(() => SendTW(order));
        }

        public void CancelBySeqNo(OrderAccData acc, string seqNo)
        {
            DateTime start = _appCtrl.StartTrace($"{acc?.FullAccount}|seqNo={seqNo}", UniqueName);

            try
            {
                //TODO

                string strMessage = "";
                int m_nCode = m_pSKOrder.CancelOrderBySeqNo(_appCtrl.CAPCenter.UserID, false, acc.FullAccount, seqNo, out strMessage); //國內委託删單(By委託序號)

                if (m_nCode != 0)
                {
                    _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
                }

                _appCtrl.LogTrace(start, strMessage, UniqueName);
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
