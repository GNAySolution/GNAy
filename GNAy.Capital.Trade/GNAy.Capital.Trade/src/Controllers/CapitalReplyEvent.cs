using GNAy.Capital.Models;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        /// <summary>
        /// 當有公告將主動呼叫函式，並通知公告類訊息
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="bstrMessage"></param>
        /// <param name="nConfirmCode"></param>
        private void SKReply_OnAnnouncement(string strUserID, string bstrMessage, out short nConfirmCode)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|bstrMessage={bstrMessage}");
            AppandReply(strUserID, bstrMessage);
            nConfirmCode = -1;
        }

        /// <summary>
        /// 定時Timer通知。每分鐘會由該函式得到一個時間
        /// </summary>
        /// <param name="nTime"></param>
        private void SKCenter_OnTimer(int nTime)
        {
            AccountTimer = (DateTime.Now, $"nTime={nTime}");
        }

        /// <summary>
        /// 當連線成功或失敗，會透過此事件函式告知連線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnConnect(string strUserID, int nErrorCode)
        {
            if (nErrorCode == 0)
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|nErrorCode={nErrorCode}");
                AppandReply(strUserID, $"{nErrorCode}");
                return;
            }

            string msg = LogAPIMessage(QuoteStatus);
            MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|{msg}");
            AppandReply(strUserID, msg);
        }

        /// <summary>
        /// 當連線中斷將會透過此事件函式告知連線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnDisconnect(string strUserID, int nErrorCode)
        {
            if (nErrorCode == 0)
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|nErrorCode={nErrorCode}");
                AppandReply(strUserID, $"{nErrorCode}");
                return;
            }

            string msg = LogAPIMessage(QuoteStatus);
            MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|{msg}");
            AppandReply(strUserID, msg);
        }

        /// <summary>
        /// 當solace連線，會透過此事件函式告知
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nCode"></param>
        private void OnSolaceReplyConnection(string strUserID, int nCode)
        {
            if (nCode == 0)
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|nCode={nCode}");
                AppandReply(strUserID, $"{nCode}");
                return;
            }

            string msg = LogAPIMessage(QuoteStatus);
            MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|{msg}");
            AppandReply(strUserID, msg);
        }

        /// <summary>
        /// 當中斷solace連線，會透過此事件函式告知斷線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnSolaceReplyDisconnect(string strUserID, int nErrorCode)
        {
            if (nErrorCode == 0)
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|nErrorCode={nErrorCode}");
                AppandReply(strUserID, $"{nErrorCode}");
                return;
            }

            string msg = LogAPIMessage(QuoteStatus);
            MainWindow.AppCtrl.LogTrace($"SKAPI|strUserID={strUserID}|{msg}");
            AppandReply(strUserID, msg);
        }
    }
}
