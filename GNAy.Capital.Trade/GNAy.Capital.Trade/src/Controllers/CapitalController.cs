using NLog;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly DateTime CreatedTime;

        public int LoginAccountResult { get; private set; }

        public string LoginQuoteStatusStr { get; private set; }
        private int _loginQuoteStatus;
        public int LoginQuoteStatus
        {
            get
            {
                return _loginQuoteStatus;
            }
            private set
            {
                _loginQuoteStatus = value;
                LoginQuoteStatusStr = GetAPIMessage(value);
            }
        }

        public string Account { get; private set; }
        public string DWP { get; private set; }

        private SKCenterLib m_pSKCenter;
        private SKOrderLib m_pSKOrder;
        private SKReplyLib m_pSKReply;
        private SKQuoteLib m_SKQuoteLib;
        private SKOSQuoteLib m_pSKOSQuote;
        private SKOOQuoteLib m_pSKOOQuote;
        private SKReplyLib m_pSKReply2;
        private SKQuoteLib m_pSKQuote2;
        private SKOrderLib m_pSKOrder2;

        public CapitalController()
        {
            CreatedTime = DateTime.Now;

            LoginAccountResult = -1;
            LoginQuoteStatus = -1;
            Account = String.Empty;
            DWP = String.Empty;
        }

        public string GetAPIMessage(int nCode)
        {
            if (nCode <= 0)
            {
                return $"nCode={nCode}";
            }

            string lastLog = m_pSKCenter.SKCenterLib_GetLastLogInfo(); //取得最後一筆LOG內容
            string codeMessage = m_pSKCenter.SKCenterLib_GetReturnCodeMessage(nCode); //取得定義代碼訊息文字

            return $"nCode={nCode}|{codeMessage}|{lastLog}";
        }

        public string LogAPIMessage(int nCode, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string msg = GetAPIMessage(nCode);

            if (nCode <= 0)
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|{msg}", lineNumber, memberName);
            }
            else
            {
                MainWindow.AppCtrl.LogWarn($"SKAPI|{msg}", lineNumber, memberName);
            }

            return msg;
        }

        public int LoginAccount(string account, string dwp)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

            try
            {
                if (m_pSKCenter != null)
                {
                    return LoginAccountResult;
                }

                account = account.Trim().ToUpper();
                dwp = dwp.Trim();
                MainWindow.AppCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

                m_pSKReply = new SKReplyLib();
                m_pSKReply.OnReplyMessage += SKReply_OnAnnouncement;

                m_pSKCenter = new SKCenterLib();
                m_pSKCenter.SKCenterLib_SetAuthority(0); //SGX 專線屬性：關閉／開啟：0／1
                m_pSKCenter.OnTimer += SKCenter_OnTimer;

                LoginAccountResult = m_pSKCenter.SKCenterLib_Login(account, dwp); //元件初始登入。在使用此 Library 前必須先通過使用者的雙因子(憑證綁定)身份認證，方可使用

                if (LoginAccountResult == 0)
                {
                    MainWindow.AppCtrl.LogTrace($"SKAPI|LoginAccountResult={LoginAccountResult}|雙因子登入成功");
                    //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                    //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();

                    //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();

                    Account = account;
                    DWP = "********";
                }
                else if (LoginAccountResult >= 600 && LoginAccountResult <= 699)
                {
                    MainWindow.AppCtrl.LogTrace($"SKAPI|LoginAccountResult={LoginAccountResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效");
                    //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                    //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();

                    //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();

                }
                //else if (LoginAccountResult >= 500 && LoginAccountResult <= 599)
                //{
                //    WriteMessage(DateTime.Now.TimeOfDay.ToString() + "_" + LoginAccountResult.ToString() + ":未使用雙因子登入成功, 目前為強制雙因子登入,請確認憑證是否有效");
                //}
                else
                {
                    LogAPIMessage(LoginAccountResult);
                }

                string strSKAPIVersion = m_pSKCenter.SKCenterLib_GetSKAPIVersionAndBit(account); //取得目前註冊SKAPI 版本及位元
                MainWindow.AppCtrl.LogTrace($"SKAPI|Version={strSKAPIVersion}");
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }

            return LoginAccountResult;
        }

        public int LoginQuote(string dwp)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|account={Account}|dwp=********");

            try
            {
                if (m_SKQuoteLib != null)
                {
                    LoginQuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）
                    LogAPIMessage(LoginQuoteStatus);
                    return LoginQuoteStatus;
                }

                dwp = dwp.Trim();
                MainWindow.AppCtrl.LogTrace($"SKAPI|account={Account}|dwp=********");

                LoginQuoteStatus = m_pSKCenter.SKCenterLib_LoginSetQuote(Account, dwp, "Y"); //Y:啟用報價 N:停用報價
                if (LoginQuoteStatus == 0 || (LoginQuoteStatus >= 600 && LoginQuoteStatus <= 699))
                {
                    MainWindow.AppCtrl.LogTrace($"SKAPI|LoginQuoteResult={LoginQuoteStatus}|登入成功");
                    //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                    //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();

                    //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                }
                else
                {
                    LogAPIMessage(LoginQuoteStatus);
                }

                m_SKQuoteLib = new SKQuoteLib();
                m_SKQuoteLib.OnConnection += m_SKQuoteLib_OnConnection;
                m_SKQuoteLib.OnNotifyQuoteLONG += m_SKQuoteLib_OnNotifyQuote;
                m_SKQuoteLib.OnNotifyHistoryTicksLONG += m_SKQuoteLib_OnNotifyHistoryTicks;
                m_SKQuoteLib.OnNotifyTicksLONG += m_SKQuoteLib_OnNotifyTicks;
                m_SKQuoteLib.OnNotifyBest5LONG += m_SKQuoteLib_OnNotifyBest5;
                m_SKQuoteLib.OnNotifyKLineData += m_SKQuoteLib_OnNotifyKLineData;
                m_SKQuoteLib.OnNotifyServerTime += m_SKQuoteLib_OnNotifyServerTime;
                m_SKQuoteLib.OnNotifyMarketTot += m_SKQuoteLib_OnNotifyMarketTot;
                m_SKQuoteLib.OnNotifyMarketBuySell += m_SKQuoteLib_OnNotifyMarketBuySell;
                //m_SKQuoteLib.OnNotifyMarketHighLow += new _ISKQuoteLibEvents_OnNotifyMarketHighLowEventHandler(m_SKQuoteLib_OnNotifyMarketHighLow);
                m_SKQuoteLib.OnNotifyMACDLONG += m_SKQuoteLib_OnNotifyMACD;
                m_SKQuoteLib.OnNotifyBoolTunelLONG += m_SKQuoteLib_OnNotifyBoolTunel;
                m_SKQuoteLib.OnNotifyFutureTradeInfoLONG += m_SKQuoteLib_OnNotifyFutureTradeInfo;
                m_SKQuoteLib.OnNotifyStrikePrices += m_SKQuoteLib_OnNotifyStrikePrices;
                //m_SKQuoteLib.OnNotifyStockList += new _ISKQuoteLibEvents_OnNotifyStockListEventHandler(m_SKQuoteLib_OnNotifyStockList);

                m_SKQuoteLib.OnNotifyMarketHighLowNoWarrant += m_SKQuoteLib_OnNotifyMarketHighLowNoWarrant;

                m_SKQuoteLib.OnNotifyCommodityListWithTypeNo += m_SKQuoteLib_OnNotifyCommodityListWithTypeNo;
                m_SKQuoteLib.OnNotifyOddLotSpreadDeal += m_SKQuoteLib_OnNotifyOddLotSpreadDeal;

                LoginQuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）
                LogAPIMessage(LoginQuoteStatus);
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }

            return LoginQuoteStatus;
        }

        public int Disconnect()
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|Start");

            try
            {
                if (m_SKQuoteLib == null)
                {
                    return 0;
                }

                int result = m_SKQuoteLib.SKQuoteLib_LeaveMonitor(); //中斷所有Solace伺服器連線
                LogAPIMessage(result);
                return result;
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }

            return -1;
        }

        public string IsConnected()
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|Start");

            try
            {
                int isConnected = m_SKQuoteLib.SKQuoteLib_IsConnected(); //檢查目前報價的連線狀態 //0表示斷線。1表示連線中。2表示下載中
                string result = string.Empty;

                switch (isConnected)
                {
                    case 0:
                        result = $"isConnected={isConnected}|斷線";
                        break;
                    case 1:
                        result = $"isConnected={isConnected}|連線中";
                        break;
                    case 2:
                        result = $"isConnected={isConnected}|下載中";
                        break;
                    default:
                        return LogAPIMessage(isConnected);
                }

                MainWindow.AppCtrl.LogTrace($"SKAPI|{result}");
                return result;
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }

            return string.Empty;
        }
    }
}
