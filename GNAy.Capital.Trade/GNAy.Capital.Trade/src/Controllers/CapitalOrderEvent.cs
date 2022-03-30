﻿using GNAy.Capital.Models;
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
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrLogInID={bstrLogInID}|bstrAccountData={bstrAccountData}");
            AppendReply(bstrLogInID, bstrAccountData);

            try
            {
                //市場,分公司,分公司代號,帳號,身份證字號,姓名
                string[] cells = bstrAccountData.Split(',');
                OrderAccData acc = new OrderAccData()
                {
                    MarketKind = cells[0],
                    Branch = cells[1],
                    BranchCode = cells[2],
                    Account = cells[3],
                    Identity = cells[4],
                    MemberName = cells[5],
                };

                if (cells[0] == "TS")
                {
                    StockAccCollection.Add(acc);
                    MainWindow.Instance.ComboBoxStockAccs.SelectedIndex = 0;
                }
                else if (cells[0] == "TF") //cells[0] == "OF"
                {
                    FuturesAccCollection.Add(acc);
                    MainWindow.Instance.ComboBoxFuturesAccs.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
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
            MainWindow.AppCtrl.LogTrace($"SKAPI|nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}");
            AppendReply(String.Empty, $"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}");
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
            MainWindow.AppCtrl.LogTrace($"SKAPI|nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}|bstrOrderLinkedID={bstrOrderLinkedID}");
            AppendReply(String.Empty, $"nThreaID={nThreaID}|nCode={nCode}|bstrMessage={bstrMessage}|bstrOrderLinkedID={bstrOrderLinkedID}");
        }

        /// <summary>
        /// 查詢證券即時庫存內容
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRealBalanceReport(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 查詢期貨未平倉
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnOpenInterest(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 智慧單(包含停損單、二擇一、MIT)查詢。透過呼叫 GetStopLossReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnStopLossReport(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 國內期貨權益數。透過呼叫 GetFutureRights 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnFutureRights(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 證券即時損益試算。透過呼叫 GetRequestProfitReport後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRequestProfitReport(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 資券配額查詢。透過呼叫 GetMarginPurchaseAmountLimit後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnMarginPurchaseAmountLimit(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 集保庫存查詢。透過呼叫 GetBalanceQuery後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnBalanceQueryReport(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }

        /// <summary>
        /// 證券智慧單被動查詢結果。透過呼叫 GetTSSmartStrategyReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTSStrategyReport(string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrData={bstrData}");
            AppendReply(String.Empty, bstrData);
        }
    }
}
