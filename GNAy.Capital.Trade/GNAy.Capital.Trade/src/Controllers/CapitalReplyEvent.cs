using NLog;
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
            //_appCtrl.LogTrace($"strUserID={strUserID}|bstrMessage={bstrMessage}", UniqueName);
            AppendReply(strUserID, bstrMessage);
            nConfirmCode = -1;
        }

        /// <summary>
        /// 定時Timer通知。每分鐘會由該函式得到一個時間
        /// </summary>
        /// <param name="nTime"></param>
        private void SKCenter_OnTimer(int nTime)
        {
            UserIDTimer = (DateTime.Now, $"nTime={nTime}");
        }

        /// <summary>
        /// 當連線成功或失敗，會透過此事件函式告知連線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnConnect(string strUserID, int nErrorCode)
        {
            (LogLevel, string) apiMsg = LogAPIMessage(QuoteStatus, $"strUserID={strUserID}");
            AppendReply(strUserID, apiMsg.Item2);
        }

        /// <summary>
        /// 當連線中斷將會透過此事件函式告知連線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnDisconnect(string strUserID, int nErrorCode)
        {
            (LogLevel, string) apiMsg = LogAPIMessage(QuoteStatus, $"strUserID={strUserID}");
            AppendReply(strUserID, apiMsg.Item2);
        }

        /// <summary>
        /// 當solace連線，會透過此事件函式告知
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nCode"></param>
        private void OnSolaceReplyConnection(string strUserID, int nCode)
        {
            (LogLevel, string) apiMsg = LogAPIMessage(QuoteStatus, $"strUserID={strUserID}");
            AppendReply(strUserID, apiMsg.Item2);
        }

        /// <summary>
        /// 當中斷solace連線，會透過此事件函式告知斷線結果
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="nErrorCode"></param>
        private void OnSolaceReplyDisconnect(string strUserID, int nErrorCode)
        {
            (LogLevel, string) apiMsg = LogAPIMessage(QuoteStatus, $"strUserID={strUserID}");
            AppendReply(strUserID, apiMsg.Item2);
        }

        /// <summary>
        /// 回報連線後會進行回報回補，等收到此事件通知後表示回補完成
        /// </summary>
        /// <param name="strUserID"></param>
        private void OnComplete(string strUserID)
        {
            //_appCtrl.LogTrace($"strUserID={strUserID}", UniqueName);
            AppendReply(strUserID, string.Empty);
        }

        /// <summary>
        /// 當有回報將主動呼叫函式，並通知委託的狀態。(新格式 包含預約單回報)
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="strData"></param>
        private void OnNewData(string strUserID, string strData)
        {
            //_appCtrl.LogTrace($"strUserID={strUserID}|strData={strData}", UniqueName);
            AppendReply(strUserID, strData);
        }

        private void m_SKReplyLib_OnReportCount(string strUserID, int nCount)
        {
            //_appCtrl.LogTrace($"strUserID={strUserID}|nCount={nCount}", UniqueName);
            AppendReply(strUserID, $"{nCount}");
        }

        /// <summary>
        /// <para>當有回報開始清除前日資料時，會發出的通知，表示清除前日回報</para>
        /// <para>R1 證券</para>
        /// <para>R2 國內期選</para>
        /// <para>R3 海外股市</para>
        /// <para>R4 海外期選</para>
        /// <para>R11 盤中零股</para>
        /// <para>R20 ~R23 智慧單</para>
        /// </summary>
        /// <param name="bstrMarket"></param>
        private void OnClear(string bstrMarket)
        {
            //_appCtrl.LogTrace($"bstrMarket={bstrMarket}", UniqueName);
            AppendReply(string.Empty, bstrMarket);
        }

        /// <summary>
        /// 當公告開始清除前日資料時，會發出的通知
        /// </summary>
        /// <param name="strUserID"></param>
        private void OnClearMessage(string strUserID)
        {
            //_appCtrl.LogTrace($"strUserID={strUserID}", UniqueName);
            AppendReply(strUserID, string.Empty);
        }
    }
}
