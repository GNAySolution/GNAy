using GNAy.Capital.Models;
using GNAy.Tools.WPF;
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
        /// 接收連線狀態
        /// </summary>
        /// <param name="nKind"></param>
        /// <param name="nCode"></param>
        private void m_SKQuoteLib_OnConnection(int nKind, int nCode)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|nKind={nKind}|nCode={nCode}");

            //3001 SK_SUBJECT_CONNECTION_CONNECTED 連線
            //3002 SK_SUBJECT_CONNECTION_DISCONNECT 斷線
            //3003 SK_SUBJECT_CONNECTION_STOCKS_READY 報價商品載入完成
            //3004 SK_SUBJECT_CONNECTION_CLEAR
            //3005 SK_SUBJECT_CONNECTION_RECONNECT
            //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
            //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
            //3026 SK_SUBJECT_CONNECTION_SGX_API_READY SGX API專線建立完成
            //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
            QuoteStatus = nKind;

            LogAPIMessage(nKind);
            LogAPIMessage(nCode);

            if (nKind == StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
            {
                MainWindow.Instance.InvokeRequired(delegate
                {
                    try
                    {
                        QuoteIndexMap.Clear();
                        QuoteCollection.Clear();
                    }
                    catch (Exception ex)
                    {
                        MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
                    }
                });
            }
        }

        /// <summary>
        /// 當有索取的個股報價異動時，將透過此事件通知應用程式處理
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        private void m_SKQuoteLib_OnNotifyQuote(short sMarketNo, int nStockIdx)
        {
            //TMainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}");

            try
            {
                SKSTOCKLONG pSKStockLONG = new SKSTOCKLONG();

                //請先訂閱即時報價(SKQuoteLib_ReqeustStocks),方可取得商品報價
                //未訂閱即時報價,僅可取得商品基本資料
                //根據市場別編號與系統所編的索引代碼，取回商品報價的及商品相關資訊
                m_SKQuoteLib.SKQuoteLib_GetStockByIndexLONG(sMarketNo, nStockIdx, ref pSKStockLONG);

                UpdateQuote(pSKStockLONG);
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        /// <summary>
        /// 當首次索取個股成交明細，此事件會回補當天Tick
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="nPtr"></param>
        /// <param name="nDate"></param>
        /// <param name="lTimehms"></param>
        /// <param name="lTimemillismicros"></param>
        /// <param name="nBid"></param>
        /// <param name="nAsk"></param>
        /// <param name="nClose"></param>
        /// <param name="nQty"></param>
        /// <param name="nSimulate"></param>
        private void m_SKQuoteLib_OnNotifyHistoryTicks(short sMarketNo, int nStockIdx, int nPtr, int nDate, int lTimehms, int lTimemillismicros, int nBid, int nAsk, int nClose, int nQty, int nSimulate)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|nPtr={nPtr}|nDate={nDate}|lTimehms={lTimehms}|lTimemillismicros={lTimemillismicros}|nBid={nBid}|nAsk={nAsk}|nClose={nClose}|nQty={nQty}|nSimulate={nSimulate}");

            //[-0219-add Qty-]
            string strData = "";

            //if (chkbox_msms.Checked == true)
            //    strData = nStockIdx.ToString() + "," + nPtr.ToString() + "," + nDate.ToString() + " " + lTimehms.ToString() + "," + nBid.ToString() + "," + nAsk.ToString() + "," + nClose.ToString() + "," + nQty.ToString();
            //else if (chkbox_msms.Checked == false)
                strData = nStockIdx.ToString() + "," + nPtr.ToString() + "," + nDate.ToString() + " " + lTimehms.ToString() + " " + lTimemillismicros.ToString() + "," + nBid.ToString() + "," + nAsk.ToString() + "," + nClose.ToString() + "," + nQty.ToString();

            //if (Box_M.Checked == true) //含市價揭示轉換
            //{
            //    if (nBid == kMarketPrice)
            //        strData = nStockIdx.ToString() + "," + nPtr.ToString() + "," + nDate.ToString() + " " + lTimehms.ToString() + "," + "M" + "," + nAsk.ToString() + "," + nClose.ToString() + "," + nQty.ToString();
            //    else if (nAsk == kMarketPrice)
            //        strData = nStockIdx.ToString() + "," + nPtr.ToString() + "," + nDate.ToString() + " " + lTimehms.ToString() + "," + nBid.ToString() + "," + "M" + "," + nClose.ToString() + "," + nQty.ToString();
            //    else
            //        strData = nStockIdx.ToString() + "," + nPtr.ToString() + "," + nDate.ToString() + " " + lTimehms.ToString() + "," + nBid.ToString() + "," + nAsk.ToString() + "," + nClose.ToString() + "," + nQty.ToString();
            //}


            //[揭示]//0:一般;1:試算揭示// 0331-會無歷史tick
            //if (strData != "")// &&( (chkBoxSimulate.Checked) || (!chkBoxSimulate.Checked && nSimulate == 0)))
            //    listTicks.Items.Add("[OnNotifyHistoryTickLONG]" + strData);

            //if (listTicks.Items.Count < 200)
            //    listTicks.SelectedIndex = listTicks.Items.Count - 1;
            //else
            //    listTicks.Items.Clear();
        }

        /// <summary>
        /// 當有索取的個股成交明細有所異動，即透過向此註冊事件回傳所異動的個股成交明細
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="nPtr"></param>
        /// <param name="nDate"></param>
        /// <param name="lTimehms"></param>
        /// <param name="lTimemillismicros"></param>
        /// <param name="nBid"></param>
        /// <param name="nAsk"></param>
        /// <param name="nClose"></param>
        /// <param name="nQty"></param>
        /// <param name="nSimulate"></param>
        private void m_SKQuoteLib_OnNotifyTicks(short sMarketNo, int nStockIdx, int nPtr, int nDate, int lTimehms, int lTimemillismicros, int nBid, int nAsk, int nClose, int nQty, int nSimulate)
        {
            //MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|nPtr={nPtr}|nDate={nDate}|lTimehms={lTimehms}|lTimemillismicros={lTimemillismicros}|nBid={nBid}|nAsk={nAsk}|nClose={nClose}|nQty={nQty}|nSimulate={nSimulate}");

            try
            {
                if (!QuoteIndexMap.TryGetValue(nStockIdx, out QuoteData quote))
                {
                    MainWindow.AppCtrl.LogError($"SKAPI|!QuoteIndexMap.TryGetValue(nStockIdx, out QuoteData quote)|nStockIdx={nStockIdx}");
                    return;
                }
                else if (quote.Market != sMarketNo)
                {
                    MainWindow.AppCtrl.LogError($"SKAPI|quote.Market != raw.bstrMarketNo|Market={quote.Market}|sMarketNo={sMarketNo}");
                    return;
                }

                quote.Count = nPtr;
                if (nDate > quote.TradeDateRaw)
                {
                    quote.TradeDateRaw = nDate;
                }
                quote.MatchedTimeRaw = String.Format("{0}.{1}", lTimehms.ToString().PadLeft(6, '0'), lTimemillismicros.ToString().PadLeft(6, '0'));
                quote.BestBuyPrice = nBid / (decimal)Math.Pow(10, quote.DecimalPos);
                quote.BestSellPrice = nAsk / (decimal)Math.Pow(10, quote.DecimalPos);
                quote.DealPrice = nClose / (decimal)Math.Pow(10, quote.DecimalPos);
                quote.DealQty = nQty;
                quote.Simulate = nSimulate;

                quote.Updater = "OnNotifyTicks";
                quote.UpdateTime = DateTime.Now;

                QuoteTimer = (quote.UpdateTime, QuoteTimer.Item2, quote.Updater);
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        /// <summary>
        /// 當有索取的個股五檔價格有所異動，即透過向此註冊的事件進行處理。此函式會回傳所異動的個股五檔價格
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="nBestBid1"></param>
        /// <param name="nBestBidQty1"></param>
        /// <param name="nBestBid2"></param>
        /// <param name="nBestBidQty2"></param>
        /// <param name="nBestBid3"></param>
        /// <param name="nBestBidQty3"></param>
        /// <param name="nBestBid4"></param>
        /// <param name="nBestBidQty4"></param>
        /// <param name="nBestBid5"></param>
        /// <param name="nBestBidQty5"></param>
        /// <param name="nExtendBid"></param>
        /// <param name="nExtendBidQty"></param>
        /// <param name="nBestAsk1"></param>
        /// <param name="nBestAskQty1"></param>
        /// <param name="nBestAsk2"></param>
        /// <param name="nBestAskQty2"></param>
        /// <param name="nBestAsk3"></param>
        /// <param name="nBestAskQty3"></param>
        /// <param name="nBestAsk4"></param>
        /// <param name="nBestAskQty4"></param>
        /// <param name="nBestAsk5"></param>
        /// <param name="nBestAskQty5"></param>
        /// <param name="nExtendAsk"></param>
        /// <param name="nExtendAskQty"></param>
        /// <param name="nSimulate"></param>
        private void m_SKQuoteLib_OnNotifyBest5(short sMarketNo, int nStockIdx, int nBestBid1, int nBestBidQty1, int nBestBid2, int nBestBidQty2, int nBestBid3, int nBestBidQty3, int nBestBid4, int nBestBidQty4, int nBestBid5, int nBestBidQty5, int nExtendBid, int nExtendBidQty, int nBestAsk1, int nBestAskQty1, int nBestAsk2, int nBestAskQty2, int nBestAsk3, int nBestAskQty3, int nBestAsk4, int nBestAskQty4, int nBestAsk5, int nBestAskQty5, int nExtendAsk, int nExtendAskQty, int nSimulate)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|nBestBid1={nBestBid1}|nBestBidQty1={nBestBidQty1}|nBestBid2={nBestBid2}|nBestBidQty2={nBestBidQty2}|nBestBid3={nBestBid3}|nBestBidQty3={nBestBidQty3}|nBestBid4={nBestBid4}|nBestBidQty4={nBestBidQty4}|nBestBid5={nBestBid5}|nBestBidQty5={nBestBidQty5}|nExtendBid={nExtendBid}|nExtendBidQty={nExtendBidQty}|nBestAsk1={nBestAsk1}|nBestAskQty1={nBestAskQty1}|nBestAsk2={nBestAsk2}|nBestAskQty2={nBestAskQty2}|nBestAsk3={nBestAsk3}|nBestAskQty3={nBestAskQty3}|nBestAsk4={nBestAsk4}|nBestAskQty4={nBestAskQty4}|nBestAsk5={nBestAsk5}|nBestAskQty5={nBestAskQty5}|nExtendAsk={nExtendAsk}|nExtendAskQty={nExtendAskQty}|nSimulate={nSimulate}");

            //0:一般;1:試算揭示
            if (nSimulate == 0)
            {
                //GridBest5Ask.ForeColor = Color.Black;
                //GridBest5Bid.ForeColor = Color.Black;
            }
            else
            {
                //GridBest5Ask.ForeColor = Color.Gray;
                //GridBest5Bid.ForeColor = Color.Gray;
            }

            SKSTOCKLONG pSKStockLONG = new SKSTOCKLONG();
            double dDigitNum = 0.000;
            string strStockNoTick = "6005"; //txtTick.Text.Trim(); //股票代碼

            int nCode = 0;
            //[--]//
            if (sMarketNo == 6 || sMarketNo == 5)
                nCode = m_SKQuoteLib.SKQuoteLib_GetStockByMarketAndNo(sMarketNo, strStockNoTick, ref pSKStockLONG); //根據市場別編號與商品代號，取回商品報價的相關資訊
            else
                nCode = m_SKQuoteLib.SKQuoteLib_GetStockByNoLONG(strStockNoTick, ref pSKStockLONG); //根據商品代號，取回商品報價的相關資訊
            //[-1022-a-]
            if (nCode == 0)
                dDigitNum = (Math.Pow(10, pSKStockLONG.sDecimal));
            else
                dDigitNum = 100.00;//default value

            //if (m_dtBest5Ask.Rows.Count == 0 && m_dtBest5Bid.Rows.Count == 0)
            //{
            //    DataRow myDataRow;

            //    myDataRow = m_dtBest5Ask.NewRow();
            //    myDataRow["m_nAskQty"] = nBestAskQty1;
            //    if (nBestAsk1 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestAsk1 / dDigitNum).ToString();///100.00;
            //    m_dtBest5Ask.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Ask.NewRow();
            //    myDataRow["m_nAskQty"] = nBestAskQty2;

            //    if (nBestAsk2 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestAsk2 / dDigitNum).ToString();//100.00;
            //    m_dtBest5Ask.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Ask.NewRow();
            //    myDataRow["m_nAskQty"] = nBestAskQty3;

            //    if (nBestAsk3 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestAsk3 / dDigitNum).ToString();//100.00;
            //    m_dtBest5Ask.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Ask.NewRow();
            //    myDataRow["m_nAskQty"] = nBestAskQty4;

            //    if (nBestAsk4 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestAsk4 / dDigitNum).ToString();// 100.00;
            //    m_dtBest5Ask.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Ask.NewRow();
            //    myDataRow["m_nAskQty"] = nBestAskQty5;

            //    if (nBestAsk5 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestAsk5 / dDigitNum).ToString();// 100.00;
            //    m_dtBest5Ask.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Bid.NewRow();
            //    myDataRow["m_nAskQty"] = nBestBidQty1;

            //    if (nBestBid1 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else myDataRow["m_nAsk"] = (nBestBid1 / dDigitNum).ToString();
            //    m_dtBest5Bid.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Bid.NewRow();
            //    myDataRow["m_nAskQty"] = nBestBidQty2;
            //    if (nBestBid2 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else myDataRow["m_nAsk"] = (nBestBid2 / dDigitNum).ToString();
            //    m_dtBest5Bid.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Bid.NewRow();
            //    myDataRow["m_nAskQty"] = nBestBidQty3;
            //    if (nBestBid3 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestBid3 / dDigitNum).ToString();
            //    m_dtBest5Bid.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Bid.NewRow();
            //    myDataRow["m_nAskQty"] = nBestBidQty4;
            //    if (nBestBid4 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestBid4 / dDigitNum).ToString();
            //    m_dtBest5Bid.Rows.Add(myDataRow);

            //    myDataRow = m_dtBest5Bid.NewRow();
            //    myDataRow["m_nAskQty"] = nBestBidQty5;
            //    if (nBestBid5 == kMarketPrice)
            //        myDataRow["m_nAsk"] = "M";
            //    else
            //        myDataRow["m_nAsk"] = (nBestBid5 / dDigitNum).ToString();
            //    m_dtBest5Bid.Rows.Add(myDataRow);
            //}
            //else
            //{
            //    m_dtBest5Ask.Rows[0]["m_nAskQty"] = nBestAskQty1;
            //    if (nBestAsk1 == kMarketPrice) m_dtBest5Ask.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Ask.Rows[0]["m_nAsk"] = (nBestAsk1 / dDigitNum).ToString();

            //    m_dtBest5Ask.Rows[1]["m_nAskQty"] = nBestAskQty2;
            //    if (nBestAsk2 == kMarketPrice) m_dtBest5Ask.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Ask.Rows[1]["m_nAsk"] = (nBestAsk2 / dDigitNum).ToString();

            //    m_dtBest5Ask.Rows[2]["m_nAskQty"] = nBestAskQty3;
            //    if (nBestAsk3 == kMarketPrice) m_dtBest5Ask.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Ask.Rows[2]["m_nAsk"] = (nBestAsk3 / dDigitNum).ToString();

            //    m_dtBest5Ask.Rows[3]["m_nAskQty"] = nBestAskQty4;
            //    if (nBestAsk4 == kMarketPrice) m_dtBest5Ask.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Ask.Rows[3]["m_nAsk"] = (nBestAsk4 / dDigitNum).ToString();

            //    m_dtBest5Ask.Rows[4]["m_nAskQty"] = nBestAskQty5;
            //    if (nBestAsk5 == kMarketPrice) m_dtBest5Ask.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Ask.Rows[4]["m_nAsk"] = (nBestAsk5 / dDigitNum).ToString();

            //    m_dtBest5Bid.Rows[0]["m_nAskQty"] = nBestBidQty1;
            //    if (nBestBid1 == kMarketPrice) m_dtBest5Bid.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Bid.Rows[0]["m_nAsk"] = (nBestBid1 / dDigitNum).ToString();

            //    m_dtBest5Bid.Rows[1]["m_nAskQty"] = nBestBidQty2;
            //    if (nBestBid2 == kMarketPrice) m_dtBest5Bid.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Bid.Rows[1]["m_nAsk"] = (nBestBid2 / dDigitNum).ToString();

            //    m_dtBest5Bid.Rows[2]["m_nAskQty"] = nBestBidQty3;
            //    if (nBestBid3 == kMarketPrice) m_dtBest5Bid.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Bid.Rows[2]["m_nAsk"] = (nBestBid3 / dDigitNum).ToString();

            //    m_dtBest5Bid.Rows[3]["m_nAskQty"] = nBestBidQty4;
            //    if (nBestBid4 == kMarketPrice) m_dtBest5Bid.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Bid.Rows[3]["m_nAsk"] = (nBestBid4 / dDigitNum).ToString();

            //    m_dtBest5Bid.Rows[4]["m_nAskQty"] = nBestBidQty5;
            //    if (nBestBid5 == kMarketPrice) m_dtBest5Bid.Rows[0]["m_nAsk"] = "M";
            //    else m_dtBest5Bid.Rows[4]["m_nAsk"] = (nBestBid5 / dDigitNum).ToString();
            //}
        }

        /// <summary>
        /// 事件回傳技術分析資訊
        /// </summary>
        /// <param name="bstrStockNo"></param>
        /// <param name="bstrData"></param>
        private void m_SKQuoteLib_OnNotifyKLineData(string bstrStockNo, string bstrData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrStockNo={bstrStockNo}|bstrData={bstrData}");

            //listKLine.Items.Add("[OnNotifyKLineData]" + bstrData);
        }

        /// <summary>
        /// 事件回傳查詢主機時間的結果
        /// </summary>
        /// <param name="sHour"></param>
        /// <param name="sMinute"></param>
        /// <param name="sSecond"></param>
        /// <param name="nTotal"></param>
        private void m_SKQuoteLib_OnNotifyServerTime(short sHour, short sMinute, short sSecond, int nTotal)
        {
            QuoteTimer = (DateTime.Now, $"{sHour}:{sMinute}:{sSecond} ({nTotal})", "OnNotifyServerTime");

            try
            {
                int sec = sSecond % 10;

                if (QuoteStatus == StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY && sec >= 0 && sec < 5)
                {
                    //要求報價主機傳送目前時間。
                    //注意：為避免收盤後無報價資料傳送，導致連線被防火牆切斷，目前solace固定每五秒會自動更新時間，請固定每十五秒呼叫此函式，確保連線正常
                    int m_nCode = m_SKQuoteLib.SKQuoteLib_RequestServerTime();
                    if (m_nCode != 0)
                    {
                        LogAPIMessage(m_nCode);
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        /// <summary>
        /// 透過呼叫 SKQuoteLib_GetMarketBuySellUpDown 後，事件回傳大盤成交張筆資料
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="sPtr"></param>
        /// <param name="nTime"></param>
        /// <param name="nTotv"></param>
        /// <param name="nTots"></param>
        /// <param name="nTotc"></param>
        private void m_SKQuoteLib_OnNotifyMarketTot(short sMarketNo, short sPtr, int nTime, int nTotv, int nTots, int nTotc)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|sPtr={sPtr}|nTime={nTime}|nTotv={nTotv}|nTots={nTots}|nTotc={nTotc}");

            double dTotv = nTotv / 100.0;

            //if (sMarketNo == 0)
            //{
            //    lblTotv.Text = dTotv.ToString() + "(億)";
            //    lblTots.Text = nTots.ToString() + "(張)";
            //    lblTotc.Text = nTotc.ToString() + "(筆)";
            //}
            //else
            //{
            //    lblTotv2.Text = dTotv.ToString() + "(億)";
            //    lblTots2.Text = nTots.ToString() + "(張)";
            //    lblTotc2.Text = nTotc.ToString() + "(筆)";
            //}
        }

        /// <summary>
        /// 透過呼叫 SKQuoteLib_GetMarketBuySellUpDown 後，事件回傳大盤成交買賣張筆數資料
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="sPtr"></param>
        /// <param name="nTime"></param>
        /// <param name="nBc"></param>
        /// <param name="nSc"></param>
        /// <param name="nBs"></param>
        /// <param name="nSs"></param>
        private void m_SKQuoteLib_OnNotifyMarketBuySell(short sMarketNo, short sPtr, int nTime, int nBc, int nSc, int nBs, int nSs)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|sPtr={sPtr}|nTime={nTime}|nBc={nBc}|nSc={nSc}|nBs={nBs}|nSs={nSs}");

            //if (sMarketNo == 0)
            //{
            //    lbllBc.Text = nBc.ToString() + "(筆)";
            //    lbllBs.Text = nBs.ToString() + "(張)";
            //    lbllSc.Text = nSc.ToString() + "(筆)";
            //    lbllSs.Text = nSs.ToString() + "(張)";
            //}
            //else
            //{
            //    lbllBc2.Text = nBc.ToString() + "(筆)";
            //    lbllBs2.Text = nBs.ToString() + "(張)";
            //    lbllSc2.Text = nSc.ToString() + "(筆)";
            //    lbllSs2.Text = nSs.ToString() + "(張)";
            //}
        }

        /// <summary>
        /// 事件回傳證券市場－技術分析平滑異同平均線MACD數值。（日線－完整）
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="bstrMACD"></param>
        /// <param name="bstrDIF"></param>
        /// <param name="bstrOSC"></param>
        private void m_SKQuoteLib_OnNotifyMACD(short sMarketNo, int nStockIdx, string bstrMACD, string bstrDIF, string bstrOSC)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|bstrMACD={bstrMACD}|bstrDIF={bstrDIF}|bstrOSC={bstrOSC}");

            //lblMACD.Text = bstrMACD;

            //lblDIF.Text = bstrDIF;
            //lblOSC.Text = bstrOSC;
        }

        /// <summary>
        /// 事件回傳技術分析－布林通道。（日線－完整）
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="bstrAVG"></param>
        /// <param name="bstrUBT"></param>
        /// <param name="bstrLBT"></param>
        private void m_SKQuoteLib_OnNotifyBoolTunel(short sMarketNo, int nStockIdx, string bstrAVG, string bstrUBT, string bstrLBT)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|bstrAVG={bstrAVG}|bstrUBT={bstrUBT}|bstrLBT={bstrLBT}");

            //lblAVG.Text = bstrAVG;
            //lblUBT.Text = bstrUBT;
            //lblLBT.Text = bstrLBT;
        }

        /// <summary>
        /// 事件回傳接收期貨商品的交易資訊
        /// </summary>
        /// <param name="bstrStockNo"></param>
        /// <param name="sMarketNo"></param>
        /// <param name="nStockIdx"></param>
        /// <param name="nBuyTotalCount"></param>
        /// <param name="nSellTotalCount"></param>
        /// <param name="nBuyTotalQty"></param>
        /// <param name="nSellTotalQty"></param>
        /// <param name="nBuyDealTotalCount"></param>
        /// <param name="nSellDealTotalCount"></param>
        private void m_SKQuoteLib_OnNotifyFutureTradeInfo(string bstrStockNo, short sMarketNo, int nStockIdx, int nBuyTotalCount, int nSellTotalCount, int nBuyTotalQty, int nSellTotalQty, int nBuyDealTotalCount, int nSellDealTotalCount)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrStockNo={bstrStockNo}|sMarketNo={sMarketNo}|nStockIdx={nStockIdx}|nBuyTotalCount={nBuyTotalCount}|nSellTotalCount={nSellTotalCount}|nBuyTotalQty={nBuyTotalQty}|nSellTotalQty={nSellTotalQty}|nBuyDealTotalCount={nBuyDealTotalCount}|nSellDealTotalCount={nSellDealTotalCount}");

            //lblMarketNo.Text = "MarketNo";
            //lblStockIdx.Text = "StockIndex";
            //lblFTIBc.Text = "TotalBc";
            //lblFTISc.Text = "TotalSc";
            //lblFTIBq.Text = "TotalBq";
            //lblFTISq.Text = "TotalSq";
            //lblFTIBDC.Text = "TotalBDC";
            //lblFTISDC.Text = "TotalSDC";

            //lblMarketNo.Text = sMarketNo.ToString();
            //lblStockIdx.Text = nStockIdx.ToString();
            //lblFTIBc.Text = nBuyTotalCount.ToString();
            //lblFTISc.Text = nSellTotalCount.ToString();
            //lblFTIBq.Text = nBuyTotalQty.ToString();
            //lblFTISq.Text = nSellTotalQty.ToString();
            //lblFTIBDC.Text = nBuyDealTotalCount.ToString();
            //lblFTISDC.Text = nSellDealTotalCount.ToString();
        }

        /// <summary>
        /// 選擇權資訊。透過呼叫 GetStrikePrices 後，資訊由該事件回傳
        /// </summary>
        /// <param name="bstrOptionData"></param>
        private void m_SKQuoteLib_OnNotifyStrikePrices(string bstrOptionData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrOptionData={bstrOptionData}");

            //[-0119-]
            string strData = "";
            strData = "[OnNotifyStrikePrices]" + bstrOptionData;

            //listStrikePrices.Items.Add(strData);
            //m_nCount++;
            //listStrikePrices.SelectedIndex = listStrikePrices.Items.Count - 1;

            //if (bstrOptionData.Substring(0, 2) != "##")   //開頭##表結束，不計商品數量
            //    txt_StrikePriceCount.Text = m_nCount.ToString();
        }

        /// <summary>
        /// 透過呼叫 SKQuoteLib_GetMarketBuySellUpDown 後，事件回傳大盤成交上漲下跌家數資料(包含『含權證家數』、 『不含權證家數』)
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="sPtr"></param>
        /// <param name="nTime"></param>
        /// <param name="sUp"></param>
        /// <param name="sDown"></param>
        /// <param name="sHigh"></param>
        /// <param name="sLow"></param>
        /// <param name="sNoChange"></param>
        /// <param name="sUpNoW"></param>
        /// <param name="sDownNoW"></param>
        /// <param name="sHighNoW"></param>
        /// <param name="sLowNoW"></param>
        /// <param name="sNoChangeNoW"></param>
        private void m_SKQuoteLib_OnNotifyMarketHighLowNoWarrant(short sMarketNo, int sPtr, int nTime, int sUp, int sDown, int sHigh, int sLow, int sNoChange, int sUpNoW, int sDownNoW, int sHighNoW, int sLowNoW, int sNoChangeNoW)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|sPtr={sPtr}|nTime={nTime}|sUp={sUp}|sDown={sDown}|sHigh={sHigh}|sLow={sLow}|sNoChange={sNoChange}|sUpNoW={sUpNoW}|sDownNoW={sDownNoW}|sHighNoW={sHighNoW}|sLowNoW={sLowNoW}|sNoChangeNoW={sNoChangeNoW}");

            //if (sMarketNo == 0)
            //{
            //    lblsUp.Text = sUp.ToString();
            //    lblsDown.Text = sDown.ToString();
            //    lblsHigh.Text = sHigh.ToString();
            //    lblsLow.Text = sLow.ToString();
            //    lblsNoChange.Text = sNoChange.ToString();
            //    lblUpNoW.Text = sUpNoW.ToString();
            //    lblDownNoW.Text = sDownNoW.ToString();
            //    lblHighNoW.Text = sHighNoW.ToString();
            //    lblLowNoW.Text = sLowNoW.ToString();
            //    lblNoChangeNoW.Text = sNoChangeNoW.ToString();
            //}
            //else
            //{
            //    lblsUp2.Text = sUp.ToString();
            //    lblsDown2.Text = sDown.ToString();
            //    lblsHigh2.Text = sHigh.ToString();
            //    lblsLow2.Text = sLow.ToString();
            //    lblsNoChange2.Text = sNoChange.ToString();

            //    lblUp2NoW.Text = sUpNoW.ToString();
            //    lblDown2NoW.Text = sDownNoW.ToString();
            //    lblHigh2NoW.Text = sHighNoW.ToString();
            //    lblLow2NoW.Text = sLowNoW.ToString();
            //    lblNoChange2NoW.Text = sNoChangeNoW.ToString();
            //}
        }

        /// <summary>
        /// 事件回傳指定國內市場－含類別代碼及類別中文名稱之商品清單
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="bstrStockListData"></param>
        private void m_SKQuoteLib_OnNotifyCommodityListWithTypeNo(short sMarketNo, string bstrStockListData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|bstrStockListData={bstrStockListData}");

            //string strData = "";
            //strData = "[OnNotifyCommodityList]" + bstrStockListData;

            //StockList.Items.Add(strData);
            //m_nCount++;
            //if (StockList.Items.Count < 200)
            //    StockList.SelectedIndex = listStrikePrices.Items.Count - 1;
            //else
            //    StockList.Items.Clear();

            //Size size = TextRenderer.MeasureText(bstrStockListData, StockList.Font);
            //if (StockList.HorizontalExtent < size.Width)
            //    StockList.HorizontalExtent = size.Width + 200;
        }

        /// <summary>
        /// 事件回傳證券市場－整零價差即時行情
        /// </summary>
        /// <param name="sMarketNo"></param>
        /// <param name="bstrStockNo"></param>
        /// <param name="nDealPrice"></param>
        /// <param name="sDigit"></param>
        private void m_SKQuoteLib_OnNotifyOddLotSpreadDeal(short sMarketNo, string bstrStockNo, int nDealPrice, short sDigit)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|sMarketNo={sMarketNo}|bstrStockNo={bstrStockNo}|nDealPrice={nDealPrice}|sDigit={sDigit}");

            if (sMarketNo == 5 || sMarketNo == 6)
            {
                //m_SKQuoteLib.SKQuoteLib_GetStockByIndexLONG(sMarketNo, nStockIdx, ref pSKStockLONG);

                //OnUpDateDataRow2(sMarketNo, bstrStockNo, nDealPrice, sDigit);
            }
        }
    }
}
