﻿using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalOrderController
    {
        /// <summary>
        /// 帳號資訊。透過呼叫GetUserAccount後，帳號資訊由該事件回傳
        /// </summary>
        /// <param name="bstrLogInID"></param>
        /// <param name="bstrAccountData"></param>
        private void m_OrderObj_OnAccount(string bstrLogInID, string bstrAccountData)
        {
            DateTime start = _appCtrl.StartTrace();

            _appCtrl.CAPCenter.AppendReply(bstrLogInID, bstrAccountData);

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

                if (Market.CodeMap.TryGetValue(cells[0], out Market.EType marketType))
                {
                    acc.MarketType = marketType;

                    _appCtrl.MainForm.InvokeSync(delegate
                    {
                        _dataCollection.Add(acc);
                        _appCtrl.MainForm.ComboBoxOrderAccs.SelectedIndex = 0;
                    });
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

        /// <summary>
        /// 非同步委託結果
        /// </summary>
        /// <param name="nThreaID"></param>
        /// <param name="nCode"></param>
        /// <param name="bstrMessage"></param>
        private void m_pSKOrder_OnAsyncOrder(int nThreaID, int nCode, string bstrMessage)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, $"{nameof(nThreaID)}={nThreaID}|{nameof(nCode)}={nCode}|{nameof(bstrMessage)}={bstrMessage}");
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
            _appCtrl.CAPCenter.AppendReply(string.Empty, $"{nameof(nThreaID)}={nThreaID}|{nameof(nCode)}={nCode}|{nameof(bstrMessage)}={bstrMessage}|{nameof(bstrOrderLinkedID)}={bstrOrderLinkedID}");
        }

        /// <summary>
        /// 查詢證券即時庫存內容
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRealBalanceReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 查詢期貨未平倉
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnOpenInterest(string bstrData)
        {
            _appCtrl.OpenInterest.AddOrUpdateAsync(bstrData);
        }

        /// <summary>
        /// 智慧單(包含停損單、二擇一、MIT)查詢。透過呼叫 GetStopLossReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnStopLossReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 國內期貨權益數。透過呼叫 GetFutureRights 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnFutureRights(string bstrData)
        {
            _appCtrl.FuturesRights.AddAsync(bstrData);
        }

        /// <summary>
        /// 證券即時損益試算。透過呼叫 GetRequestProfitReport後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnRequestProfitReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 資券配額查詢。透過呼叫 GetMarginPurchaseAmountLimit後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnMarginPurchaseAmountLimit(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 集保庫存查詢。透過呼叫 GetBalanceQuery後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnBalanceQueryReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 證券智慧單被動查詢結果。透過呼叫 GetTSSmartStrategyReport 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTSStrategyReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 證券新損益查詢結果。透過呼叫 GetProfitLossGWReport 後，資訊由該事件回傳。(現股當沖)
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTSProfitLossGWReport(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnOFOpenInterestGW(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }

        /// <summary>
        /// 透過呼叫 SKOrderLib_TelnetTest 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrData"></param>
        private void m_pSKOrder_OnTelnetTest(string bstrData)
        {
            _appCtrl.CAPCenter.AppendReply(string.Empty, bstrData);
        }
    }
}
