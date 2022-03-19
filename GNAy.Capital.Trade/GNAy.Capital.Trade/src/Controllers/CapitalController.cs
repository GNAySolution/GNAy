using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        public readonly DateTime CreatedTime;

        private SKCenterLib m_pSKCenter;
        private SKOrderLib m_pSKOrder;
        private SKReplyLib m_pSKReply;
        private SKQuoteLib m_SKQuoteLib;
        private SKOSQuoteLib m_pSKOSQuote;
        private SKOOQuoteLib m_pSKOOQuote;
        private SKReplyLib m_pSKReply2;
        private SKQuoteLib m_pSKQuote2;
        private SKOrderLib m_pSKOrder2;

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

        public (DateTime, string) AccountTimer { get; private set; }
        public (DateTime, string) QuoteTimer { get; private set; }

        private readonly Dictionary<int, QuoteData> QuoteIndexMap;
        private readonly ObservableCollection<QuoteData> QuoteCollection;

        public CapitalController()
        {
            CreatedTime = DateTime.Now;

            LoginAccountResult = -1;
            LoginQuoteStatus = -1;

            Account = String.Empty;
            DWP = String.Empty;

            AccountTimer = (DateTime.MinValue, string.Empty);
            QuoteTimer = (DateTime.MinValue, string.Empty);

            QuoteIndexMap = new Dictionary<int, QuoteData>();

            MainWindow.Instance.DataGridQuoteSubscribed.SetHeadersByBindings(QuoteData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1.ShortName));
            QuoteCollection = MainWindow.Instance.DataGridQuoteSubscribed.SetAndGetItemsSource<QuoteData>();
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
            //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
            //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
            //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
            else if (nCode < 2000 || nCode == 3021 || nCode == 3022 || nCode == 3033)
            {
                MainWindow.AppCtrl.LogError($"SKAPI|{msg}", lineNumber, memberName);
            }
            else if (nCode < 3000)
            {
                MainWindow.AppCtrl.LogWarn($"SKAPI|{msg}", lineNumber, memberName);
            }
            else
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|{msg}", lineNumber, memberName);
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

                    Account = account;
                    DWP = "********";
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

        public void PrintProductList()
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|Start");

            try
            {
                foreach(int market in MainWindow.AppCtrl.Settings.QuoteMarkets)
                {
                    //根據市場別編號，取得國內各市場代碼所包含的商品基本資料相關資訊
                    //結果會透過OnNotifyCommodityListWithTypeNo收到
                    int m_nCode = m_SKQuoteLib.SKQuoteLib_RequestStockList((short)market);
                    LogAPIMessage(m_nCode);
                }
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }
        }

        private QuoteData CreateQuote(SKSTOCKLONG raw)
        {
            QuoteData quote = new QuoteData()
            {
                Symbol = raw.bstrStockNo,
                Name = raw.bstrStockName,
                DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal),
                DealQty = raw.nTickQty,
                BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal),
                BestBuyQty = raw.nBc,
                BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal),
                BestSellQty = raw.nAc,
                OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal),
                HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal),
                LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal),
                Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal),
                Simulate = raw.nSimulate,
                TotalQty = raw.nTQty,
                TradeDateRaw = raw.nTradingDay,
                HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal),
                LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal),
                Index = raw.nStockIdx,
                Market = short.TryParse(raw.bstrMarketNo, out short x) ? x : (short)-1,
                DecimalPos = raw.sDecimal,
                TotalQtyBefore = raw.nYQty,
            };

            if (quote.DealPrice != 0 && quote.Reference != 0)
            {
                quote.UpDown = quote.DealPrice - quote.Reference;
                quote.UpDownPct = quote.UpDown / quote.Reference * 100;
            }

            quote.Creator = "GetStockByNoLONG";
            quote.CreatedTime = DateTime.Now;
            quote.Updater = quote.Creator;
            quote.UpdateTime = quote.CreatedTime;

            return quote;
        }

        private bool UpdateQuote(SKSTOCKLONG raw)
        {
            if (!QuoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote))
            {
                MainWindow.AppCtrl.LogError($"SKAPI|!QuoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote)|nStockIdx={raw.nStockIdx}");
                return false;
            }
            else if (quote.Symbol != raw.bstrStockNo)
            {
                MainWindow.AppCtrl.LogError($"SKAPI|quote.Symbol != raw.bstrStockNo|Symbol={quote.Symbol}|bstrStockNo={raw.bstrStockNo}");
                return false;
            }
            else if (quote.Name != raw.bstrStockName)
            {
                MainWindow.AppCtrl.LogError($"SKAPI|quote.Name != raw.bstrStockName|Name={quote.Name}|bstrStockName={raw.bstrStockName}");
                return false;
            }
            else if ($"{quote.Market}" != raw.bstrMarketNo)
            {
                MainWindow.AppCtrl.LogError($"SKAPI|quote.Market != raw.bstrMarketNo|Market={quote.Market}|bstrMarketNo={raw.bstrMarketNo}");
                return false;
            }

            //quote.Symbol = raw.bstrStockNo;
            //quote.Name = raw.bstrStockName;
            quote.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
            quote.DealQty = raw.nTickQty;
            quote.BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestBuyQty = raw.nBc;
            quote.BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestSellQty = raw.nAc;
            quote.OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal);
            quote.HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal);
            quote.LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Simulate = raw.nSimulate;
            quote.TotalQty = raw.nTQty;
            quote.TradeDateRaw = raw.nTradingDay;
            quote.HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal);
            quote.LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal);
            //quote.Index = raw.nStockIdx;
            //quote.Market = short.TryParse(raw.bstrMarketNo, out short x) ? x : (short)-1;
            quote.DecimalPos = raw.sDecimal;
            quote.TotalQtyBefore = raw.nYQty;

            if (quote.DealPrice != 0 && quote.Reference != 0)
            {
                quote.UpDown = quote.DealPrice - quote.Reference;
                quote.UpDownPct = quote.UpDown / quote.Reference * 100;
            }

            quote.Updater = "OnNotifyQuote";
            quote.UpdateTime = DateTime.Now;

            return true;
        }

        public void SubQuotes()
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|Start");

            DateTime now = DateTime.Now;

            try
            {
                if (LoginQuoteStatus != 3003) //3003 SK_SUBJECT_CONNECTION_STOCKS_READY 報價商品載入完成
                {
                    throw new ArgumentException($"SKAPI|LoginQuoteStatus != 3003|LoginQuoteStatusStr={LoginQuoteStatusStr}");
                }
                else if (QuoteCollection.Count > 0)
                {
                    throw new ArgumentException($"SKAPI|QuoteCollection.Count > 0|Count={QuoteCollection.Count}|Quotes are subscribed.");
                }

                bool isHoliday = MainWindow.AppCtrl.Config.IsHoliday(now);
                int nCode = -1;

                foreach (string product in MainWindow.AppCtrl.Settings.QuoteSubscribed)
                {
                    SKSTOCKLONG pSKStockLONG = new SKSTOCKLONG();
                    nCode = m_SKQuoteLib.SKQuoteLib_GetStockByNoLONG(product, ref pSKStockLONG); //根據商品代號，取回商品報價的相關資訊
                    if (nCode != 0)
                    {
                        LogAPIMessage(nCode);
                        continue;
                    }

                    QuoteData quote = CreateQuote(pSKStockLONG);
                    QuoteIndexMap.Add(quote.Index, quote);
                    QuoteCollection.Add(quote);

                    if (isHoliday) //假日不訂閱即時報價
                    {
                        continue;
                    }
                    else if (now.Hour >= 14 || now.Hour < 8)
                    {
                        //期貨選擇權夜盤，上市櫃已經收盤
                        if (quote.Market != 2 && quote.Market != 3)
                        {
                            continue;
                        }
                    }

                    short pageA = -1;
                    nCode = m_SKQuoteLib.SKQuoteLib_RequestLiveTick(ref pageA, quote.Symbol); //訂閱與要求傳送即時成交明細。(本功能不會訂閱最佳五檔，亦不包含歷史Ticks)
                    if (nCode != 0)
                    {
                        LogAPIMessage(nCode);
                        continue;
                    }
                    if (pageA < 0)
                    {
                        MainWindow.AppCtrl.LogError($"SKAPI|Sub quote failed.|Symbol={quote.Symbol}|pageA={pageA}");
                    }

                    quote.Page = pageA;
                }

                string products = string.Join(",", MainWindow.AppCtrl.Settings.QuoteSubscribed);
                short pageB = 1;

                nCode = m_SKQuoteLib.SKQuoteLib_RequestStocks(ref pageB, products); //訂閱指定商品即時報價，要求伺服器針對 bstrStockNos 內的商品代號訂閱商品報價通知動作
                if (nCode != 0)
                {
                    LogAPIMessage(nCode);
                }
                if (pageB < 0)
                {
                    MainWindow.AppCtrl.LogError($"SKAPI|Sub quote failed.|products={products}|pageB={pageB}");
                }
            }
            catch (Exception ex)
            {
                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                MainWindow.AppCtrl.LogTrace("SKAPI|End");
            }
        }

        public void SaveQuotesAsync()
        {
            if (QuoteCollection.Count <= 0)
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                MainWindow.AppCtrl.LogTrace($"SKAPI|Start");

                try
                {
                    string path = Path.Combine(MainWindow.AppCtrl.Config.QuoteFolder.FullName, $"{QuoteCollection.Max(x => x.TradeDateRaw)}.csv");

                    if (!File.Exists(path))
                    {
                        using (StreamWriter sw = new StreamWriter(path, false, TextEncoding.UTF8WithoutBOM))
                        {
                            sw.WriteLine(string.Join(",", QuoteData.ColumnGetters.Values.Select(x => x.Item1.Name)));
                        }
                    }

                    using (StreamWriter sw = new StreamWriter(path, true, TextEncoding.UTF8WithoutBOM))
                    {
                        foreach (QuoteData quote in QuoteCollection)
                        {
                            try
                            {
                                string line = string.Join("\",\"", QuoteData.ColumnGetters.Values.Select(x => x.Item2.PropertyValueToString(quote, x.Item1.StringFormat)));
                                line = $"\"{line}\"";

                                sw.WriteLine(line);
                            }
                            catch (Exception ex)
                            {
                                MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.AppCtrl.LogException(ex, ex.StackTrace);
                }
                finally
                {
                    MainWindow.AppCtrl.LogTrace("SKAPI|End");
                }
            });
        }
    }
}
