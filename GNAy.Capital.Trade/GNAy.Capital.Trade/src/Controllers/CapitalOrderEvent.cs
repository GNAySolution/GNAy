using GNAy.Capital.Models;
using GNAy.Tools.WPF;
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
        /// 帳號資訊。透過呼叫GetUserAccount後，帳號資訊由該事件回傳
        /// </summary>
        /// <param name="bstrLogInID"></param>
        /// <param name="bstrAccountData"></param>
        private void m_OrderObj_OnAccount(string bstrLogInID, string bstrAccountData)
        {
            DateTime start = _appCtrl.StartTrace($"bstrLogInID={bstrLogInID}|bstrAccountData={bstrAccountData}", UniqueName);

            AppendReply(bstrLogInID, bstrAccountData);

            try
            {
                string[] cells = bstrAccountData.Split(',');

                //市場,分公司,分公司代號,帳號,身份證字號,姓名
                OrderAccData acc = new OrderAccData()
                {
                    Branch = cells[1],
                    BranchCode = cells[2],
                    Account = cells[3],
                    Identity = cells[4],
                    MemberName = cells[5],
                };

                _appCtrl.MainForm.InvokeRequired(delegate
                {
                    if (Market.CodeMap.TryGetValue(cells[0], out Market.EType marketType))
                    {
                        acc.MarketType = marketType;
                        _orderAccCollection.Add(acc);
                        _appCtrl.MainForm.ComboBoxOrderAccs.SelectedIndex = 0;
                    }
                });
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

        /// <summary>
        /// 非同步委託結果
        /// </summary>
        /// <param name="nThreaID"></param>
        /// <param name="nCode"></param>
        /// <param name="bstrMessage"></param>
        private void m_pSKOrder_OnAsyncOrder(int nThreaID, int nCode, string bstrMessage)
        {
            _appCtrl.LogTrace($"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}", UniqueName);
            AppendReply(string.Empty, $"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}");
        }

        /// <summary>
        /// 非同步委託結果。(含單獨自訂資料欄)
        /// </summary>
        /// <param name="nThreaID"></param>
        /// <param name="nCode"></param>
        /// <param name="bstrMessage"></param>
        /// <param name="bstrOrderLinkedID"></param>
        private void m_pSKOrder_OnAsyncOrderOLID(int nThreaID, int nCode, string bstrMessage, string bstrOrderLinkedID)
        {
            _appCtrl.LogTrace($"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}|bstrOrderLinkedID={bstrOrderLinkedID}", UniqueName);
            AppendReply(string.Empty, $"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}|bstrOrderLinkedID={bstrOrderLinkedID}");
        }

        /// <summary>
        /// 查詢證券即時庫存內容
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRealBalanceReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 查詢期貨未平倉
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnOpenInterest(string bstrData)
        {
            //完整： (含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,買方成交均價(二位小數),賣方未平倉,賣方當沖未平倉,賣方成交均價(二位小數), LOGIN_ID(V2.13.30新增)
            //格式1：(含複式單，市場別：TM)市場別, 帳號, 商品, 買方未平倉,買方當沖未平倉,賣方未平倉,賣方當沖未平倉, LOGIN_ID(V2.13.30新增)
            //格式2：(不含複式單，市場別：TM，可自行計算損益)市場別, 帳號, 商品, 買賣別, 未平倉部位, 當沖未平倉部位, 平均成本(三位小數), 一點價值, 單口手續費, 交易稅(萬分之X), LOGIN_ID(V2.13.30新增)
            //TF,OrderAccount,MTX05,1,0,1652500,0,0,0,UserID

            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 智慧單(包含停損單、二擇一、MIT)查詢。透過呼叫 GetStopLossReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnStopLossReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 國內期貨權益數。透過呼叫 GetFutureRights 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnFutureRights(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 證券即時損益試算。透過呼叫 GetRequestProfitReport後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRequestProfitReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 資券配額查詢。透過呼叫 GetMarginPurchaseAmountLimit後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnMarginPurchaseAmountLimit(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 集保庫存查詢。透過呼叫 GetBalanceQuery後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnBalanceQueryReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 證券智慧單被動查詢結果。透過呼叫 GetTSSmartStrategyReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTSStrategyReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 證券新損益查詢結果。透過呼叫 GetProfitLossGWReport 後，資訊由該事件回傳。(現股當沖)
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTSProfitLossGWReport(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnOFOpenInterestGW(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 透過呼叫 SKOrderLib_TelnetTest 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTelnetTest(string bstrData)
        {
            _appCtrl.LogTrace($"bstrData={bstrData}", UniqueName);
            AppendReply(string.Empty, bstrData);
        }
    }
}
