using NLog;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class CapitalController
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly DateTime CreatedTime;

        public int LoginResult { get; private set; }
        public string Account { get; private set; }
        public string DWP { get; private set; }

        private SKCenterLib m_pSKCenter;
        private SKOrderLib m_pSKOrder;
        private SKReplyLib m_pSKReply;
        private SKQuoteLib m_pSKQuote;
        private SKOSQuoteLib m_pSKOSQuote;
        private SKOOQuoteLib m_pSKOOQuote;
        private SKReplyLib m_pSKReply2;
        private SKQuoteLib m_pSKQuote2;
        private SKOrderLib m_pSKOrder2;

        public CapitalController()
        {
            CreatedTime = DateTime.Now;

            LoginResult = -1;
            Account = String.Empty;
            DWP = String.Empty;
        }

        public int LoginAccount(string account, string dwp)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

            try
            {
                if (m_pSKCenter != null)
                {
                    return LoginResult;
                }

                account = account.Trim().ToUpper();
                dwp = dwp.Trim();
                MainWindow.AppCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

                m_pSKReply = new SKReplyLib();
                m_pSKReply.OnReplyMessage += SKReply_OnAnnouncement;

                m_pSKCenter = new SKCenterLib();
                m_pSKCenter.SKCenterLib_SetAuthority(0); //SGX 專線屬性：關閉／開啟：0／1
                m_pSKCenter.OnTimer += SKCenter_OnTimer;

                LoginResult = m_pSKCenter.SKCenterLib_Login(account, dwp); //元件初始登入。在使用此 Library 前必須先通過使用者的雙因子(憑證綁定)身份認證，方可使用

                if (LoginResult == 0)
                {
                    MainWindow.AppCtrl.LogTrace($"SKAPI|LoginResult={LoginResult}|雙因子登入成功");
                    //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                    //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();


                    //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();

                    Account = account;
                    DWP = "********";
                }
                else if (LoginResult >= 600 && LoginResult <= 699)
                {
                    MainWindow.AppCtrl.LogTrace($"SKAPI|LoginResult={LoginResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效");
                    //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                    //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();

                    //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();

                }
                //else if (LoginResult >= 500 && LoginResult <= 599)
                //{
                //    WriteMessage(DateTime.Now.TimeOfDay.ToString() + "_" + LoginResult.ToString() + ":未使用雙因子登入成功, 目前為強制雙因子登入,請確認憑證是否有效");
                //}
                else
                {
                    MainWindow.AppCtrl.LogError($"SKAPI|LoginResult={LoginResult}|{m_pSKCenter.SKCenterLib_GetReturnCodeMessage(LoginResult)}"); //取得定義代碼訊息文字
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

            return LoginResult;
        }

        /// <summary>
        /// 當有公告將主動呼叫函式，並通知公告類訊息
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="bstrMessage"></param>
        /// <param name="nConfirmCode"></param>
        private void SKReply_OnAnnouncement(string strUserID, string bstrMessage, out short nConfirmCode)
        {
            try
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|bstrMessage={bstrMessage}");
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                nConfirmCode = -1;
            }
        }

        /// <summary>
        /// 定時Timer通知。每分鐘會由該函式得到一個時間
        /// </summary>
        /// <param name="nTime"></param>
        private void SKCenter_OnTimer(int nTime)
        {
            try
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|nTime={nTime}");
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            { }
        }
    }
}
