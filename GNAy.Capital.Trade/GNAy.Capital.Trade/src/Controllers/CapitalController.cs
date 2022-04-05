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
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        public readonly DateTime CreatedTime;
        private readonly AppController _appCtrl;

        private SKCenterLib m_pSKCenter { get; set; }
        private SKOrderLib m_pSKOrder { get; set; }
        private SKReplyLib m_pSKReply { get; set; }
        private SKQuoteLib m_SKQuoteLib { get; set; }
        private SKOSQuoteLib m_pSKOSQuote { get; set; }
        private SKOOQuoteLib m_pSKOOQuote { get; set; }
        private SKReplyLib m_pSKReply2 { get; set; }
        private SKQuoteLib m_pSKQuote2 { get; set; }
        private SKOrderLib m_pSKOrder2 { get; set; }

        public int LoginAccountResult { get; private set; }
        public string Account { get; private set; }
        public string DWP { get; private set; }

        public string QuoteStatusStr { get; private set; }
        private int _quoteStatus;
        public int QuoteStatus
        {
            get
            {
                return _quoteStatus;
            }
            private set
            {
                _quoteStatus = value;
                QuoteStatusStr = GetAPIMessage(value);
            }
        }

        public (DateTime, string) AccountTimer { get; private set; }
        public (DateTime, string, string) QuoteTimer { get; private set; }

        public readonly bool IsAMMarket;
        public string QuoteFileNameBase { get; private set; }

        private readonly Dictionary<int, APIReplyData> _apiReplyMap;
        private readonly ObservableCollection<APIReplyData> _apiReplyCollection;

        private readonly Dictionary<int, QuoteData> _quoteIndexMap;
        private readonly ObservableCollection<QuoteData> _quoteCollection;

        public int ReadCertResult { get; private set; }

        private readonly ObservableCollection<OrderAccData> _stockAccCollection;
        private readonly ObservableCollection<OrderAccData> _futuresAccCollection;

        public CapitalController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            _appCtrl = appCtrl;

            LoginAccountResult = -1;
            Account = String.Empty;
            DWP = String.Empty;

            QuoteStatus = -1;

            AccountTimer = (DateTime.MinValue, string.Empty);
            QuoteTimer = (DateTime.MinValue, string.Empty, string.Empty);

            IsAMMarket = _appCtrl.Config.IsAMMarket(CreatedTime);
            QuoteFileNameBase = string.Empty;

            _apiReplyMap = new Dictionary<int, APIReplyData>();
            _appCtrl.MainForm.DataGridAPIReply.SetHeadersByBindings(APIReplyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _apiReplyCollection = _appCtrl.MainForm.DataGridAPIReply.SetAndGetItemsSource<APIReplyData>();

            _quoteIndexMap = new Dictionary<int, QuoteData>();
            _appCtrl.MainForm.DataGridQuoteSubscribed.SetHeadersByBindings(QuoteData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _quoteCollection = _appCtrl.MainForm.DataGridQuoteSubscribed.SetAndGetItemsSource<QuoteData>();

            ReadCertResult = -1;

            _stockAccCollection = _appCtrl.MainForm.ComboBoxStockAccs.SetAndGetItemsSource<OrderAccData>();
            _futuresAccCollection = _appCtrl.MainForm.ComboBoxFuturesAccs.SetAndGetItemsSource<OrderAccData>();
        }

        private CapitalController() : this(null)
        { }

        public string GetAPIMessage(int nCode)
        {
            if (nCode <= 0)
            {
                return $"nCode={nCode}";
            }

            string lastLog = m_pSKCenter.SKCenterLib_GetLastLogInfo(); //取得最後一筆LOG內容
            string codeMessage = m_pSKCenter.SKCenterLib_GetReturnCodeMessage(nCode % StatusCode.BaseTraceValue); //取得定義代碼訊息文字

            return $"nCode={nCode}|{codeMessage}|{lastLog}";
        }

        public string LogAPIMessage(int nCode, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string msg = GetAPIMessage(nCode);

            if (nCode < 0)
            {
                _appCtrl.LogError($"SKAPI|{msg}", lineNumber, memberName);
                return msg;
            }

            int _code = nCode % StatusCode.BaseTraceValue;

            if (_code == 0)
            {
                _appCtrl.LogTrace($"SKAPI|{msg}", lineNumber, memberName);
            }
            //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
            //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
            //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
            else if (_code < 2000 || _code == 3021 || _code == 3022 || _code == 3033)
            {
                _appCtrl.LogError($"SKAPI|{msg}", lineNumber, memberName);
            }
            else if (_code < 3000)
            {
                _appCtrl.LogWarn($"SKAPI|{msg}", lineNumber, memberName);
            }
            else
            {
                _appCtrl.LogTrace($"SKAPI|{msg}", lineNumber, memberName);
            }

            return msg;
        }

        public void AppendReply(string account, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            APIReplyData replay = new APIReplyData()
            {
                ThreadID = Thread.CurrentThread.ManagedThreadId,
                Account = account,
                Message = msg,
                CallerLineNumber = lineNumber,
                CallerMemberName = memberName,
            };

            _appCtrl.MainForm.InvokeRequired(delegate
            {
                try
                {
                    _appCtrl.MainForm.TabControlBA.SelectedIndex = 0;

                    _apiReplyCollection.Add(replay);

                    //while (_apiReplyCollection.Count > _appCtrl.Settings.DataGridAppLogRowsMax * 2)
                    //{
                    //    _apiReplyCollection.RemoveAt(0);
                    //}

                    if (!_appCtrl.MainForm.DataGridAPIReply.IsMouseOver)
                    {
                        _appCtrl.MainForm.DataGridAPIReply.ScrollToBorder();
                    }
                }
                catch
                { }
            });
        }

        public int LoginAccount(string account, string dwp)
        {
            _appCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

            try
            {
                if (m_pSKCenter != null)
                {
                    return LoginAccountResult;
                }

                account = account.Trim().ToUpper();
                dwp = dwp.Trim();
                _appCtrl.LogTrace($"SKAPI|account={account}|dwp=********");

                m_pSKReply = new SKReplyLib();
                m_pSKReply.OnReplyMessage += SKReply_OnAnnouncement;
                m_pSKReply.OnConnect += OnConnect;
                m_pSKReply.OnDisconnect += OnDisconnect;
                m_pSKReply.OnSolaceReplyConnection += OnSolaceReplyConnection;
                m_pSKReply.OnSolaceReplyDisconnect += OnSolaceReplyDisconnect;
                m_pSKReply.OnComplete += OnComplete;
                m_pSKReply.OnNewData += OnNewData;
                m_pSKReply.OnReportCount += m_SKReplyLib_OnReportCount;
                m_pSKReply.OnReplyClear += OnClear;
                m_pSKReply.OnReplyClearMessage += OnClearMessage;

                m_pSKCenter = new SKCenterLib();
                m_pSKCenter.SKCenterLib_SetAuthority(0); //SGX 專線屬性：關閉／開啟：0／1
                m_pSKCenter.OnTimer += SKCenter_OnTimer;

                LoginAccountResult = m_pSKCenter.SKCenterLib_Login(account, dwp); //元件初始登入。在使用此 Library 前必須先通過使用者的雙因子(憑證綁定)身份認證，方可使用

                if (LoginAccountResult == 0)
                {
                    _appCtrl.LogTrace($"SKAPI|LoginAccountResult={LoginAccountResult}|雙因子登入成功");
                    Account = account;
                    DWP = "********";
                }
                else if (LoginAccountResult >= 600 && LoginAccountResult <= 699)
                {
                    _appCtrl.LogTrace($"SKAPI|LoginAccountResult={LoginAccountResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效");
                    Account = account;
                    DWP = "********";
                }
                else
                {
                    LogAPIMessage(LoginAccountResult);
                    m_pSKCenter = null;
                    return LoginAccountResult;
                }

                //if (LoginAccountResult == 0 || (LoginAccountResult >= 600 && LoginAccountResult <= 699))
                //{
                //    int nCode = m_pSKReply.SKReplyLib_ConnectByID(account); //指定回報連線的使用者登入帳號
                //    if (nCode != 0)
                //    {
                //        LogAPIMessage(nCode);
                //    }
                //}

                string strSKAPIVersion = m_pSKCenter.SKCenterLib_GetSKAPIVersionAndBit(account); //取得目前註冊SKAPI 版本及位元
                _appCtrl.LogTrace($"SKAPI|Version={strSKAPIVersion}");
                _appCtrl.MainForm.StatusBarItemAA4.Text = $"SKAPIVersionAndBit={strSKAPIVersion}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }

            return LoginAccountResult;
        }

        public void LoginQuoteAsync(string dwp)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _appCtrl.LogTrace($"SKAPI|account={Account}|dwp=********");

                    if (m_SKQuoteLib != null)
                    {
                        QuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）
                        LogAPIMessage(QuoteStatus);
                        return;
                    }

                    dwp = dwp.Trim();
                    _appCtrl.LogTrace($"SKAPI|account={Account}|dwp=********");

                    QuoteStatus = m_pSKCenter.SKCenterLib_LoginSetQuote(Account, dwp, "Y"); //Y:啟用報價 N:停用報價
                    if (QuoteStatus == 0 || (QuoteStatus >= 600 && QuoteStatus <= 699))
                    {
                        _appCtrl.LogTrace($"SKAPI|QuoteStatus={QuoteStatus}|登入成功");
                        //skOrder1.LoginID = txtAccount.Text.Trim().ToUpper();
                        //skOrder1.LoginID2 = txtAccount2.Text.Trim().ToUpper();

                        //skReply1.LoginID = txtAccount.Text.Trim().ToUpper();

                        //skQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                        //skosQuote1.LoginID = txtAccount.Text.Trim().ToUpper();
                    }
                    else
                    {
                        LogAPIMessage(QuoteStatus);
                    }

                    m_SKQuoteLib = new SKQuoteLib();
                    m_SKQuoteLib.OnConnection += m_SKQuoteLib_OnConnection;
                    m_SKQuoteLib.OnNotifyQuoteLONG += m_SKQuoteLib_OnNotifyQuote;
                    m_SKQuoteLib.OnNotifyHistoryTicksLONG += OnNotifyHistoryTicks;
                    m_SKQuoteLib.OnNotifyTicksLONG += OnNotifyTicks;
                    m_SKQuoteLib.OnNotifyBest5LONG += m_SKQuoteLib_OnNotifyBest5;
                    m_SKQuoteLib.OnNotifyKLineData += m_SKQuoteLib_OnNotifyKLineData;
                    m_SKQuoteLib.OnNotifyServerTime += OnNotifyServerTime;
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

                    QuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）
                    LogAPIMessage(QuoteStatus);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
                finally
                {
                    _appCtrl.LogTrace("SKAPI|End");
                }
            });
        }

        public int Disconnect()
        {
            _appCtrl.LogTrace($"SKAPI|Start");

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
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }

            return -1;
        }

        public string IsConnected()
        {
            _appCtrl.LogTrace($"SKAPI|Start");

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

                int nCode = m_pSKReply.SKReplyLib_IsConnectedByID(Account);
                if (nCode != 0 && nCode != 1 && nCode != 2)
                {
                    LogAPIMessage(nCode);
                }

                _appCtrl.LogTrace($"SKAPI|{result}");
                return result;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }

            return string.Empty;
        }

        public void PrintProductList()
        {
            _appCtrl.LogTrace($"SKAPI|Start");

            try
            {
                foreach (int market in _appCtrl.Settings.QuoteMarkets)
                {
                    //根據市場別編號，取得國內各市場代碼所包含的商品基本資料相關資訊
                    //結果會透過OnNotifyCommodityListWithTypeNo收到
                    int m_nCode = m_SKQuoteLib.SKQuoteLib_RequestStockList((short)market);
                    LogAPIMessage(m_nCode);
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        private QuoteData CreateQuote(SKSTOCKLONG raw)
        {
            QuoteData quote = new QuoteData()
            {
                Creator = nameof(CreateQuote),
                CreatedTime = DateTime.Now,
                Updater = nameof(CreateQuote),
                UpdateTime = DateTime.Now,
                Symbol = raw.bstrStockNo,
                Name = raw.bstrStockName,
                DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal),
                DealQty = raw.nTickQty,
                BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal),
                BestBuyQty = raw.nBc,
                BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal),
                BestSellQty = raw.nAc,
                //OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal),
                //HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal),
                //LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal),
                Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal),
                Simulate = raw.nSimulate,
                TotalQty = raw.nTQty,
                TradeDateRaw = raw.nTradingDay,
                HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal),
                LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal),
                Index = raw.nStockIdx,
                Market = short.Parse(raw.bstrMarketNo),
                DecimalPos = raw.sDecimal,
                TotalQtyBefore = raw.nYQty,
            };

            return quote;
        }

        private bool UpdateQuote(SKSTOCKLONG raw)
        {
            if (!_quoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote))
            {
                _appCtrl.LogError($"SKAPI|!_quoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote)|nStockIdx={raw.nStockIdx}");
                return false;
            }
            else if (quote.Symbol != raw.bstrStockNo)
            {
                _appCtrl.LogError($"SKAPI|quote.Symbol != raw.bstrStockNo|Symbol={quote.Symbol}|bstrStockNo={raw.bstrStockNo}");
                return false;
            }
            else if (quote.Name != raw.bstrStockName)
            {
                _appCtrl.LogError($"SKAPI|quote.Name != raw.bstrStockName|Name={quote.Name}|bstrStockName={raw.bstrStockName}");
                return false;
            }
            else if ($"{quote.Market}" != raw.bstrMarketNo)
            {
                _appCtrl.LogError($"SKAPI|quote.Market != raw.bstrMarketNo|Market={quote.Market}|bstrMarketNo={raw.bstrMarketNo}");
                return false;
            }
            //開盤成交分別收到
            //else if (raw.nOpen != 0 && (raw.nClose == 0 || raw.nTickQty == 0))
            //else if (!IsAMMarket && quote.Page < 0 && (quote.Market == Definition.MarketTSE || quote.Market == Definition.MarketOTC))
            //{
            //    QuoteData quoteBK = new QuoteData()
            //    {
            //        Creator = nameof(UpdateQuote),
            //        CreatedTime = DateTime.Now,
            //        Updater = nameof(UpdateQuote),
            //        UpdateTime = DateTime.Now,
            //        Symbol = raw.bstrStockNo,
            //        Name = raw.bstrStockName,
            //        DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal),
            //        DealQty = raw.nTickQty,
            //        BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal),
            //        BestBuyQty = raw.nBc,
            //        BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal),
            //        BestSellQty = raw.nAc,
            //        OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal),
            //        HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal),
            //        LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal),
            //        Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal),
            //        Simulate = raw.nSimulate,
            //        TotalQty = raw.nTQty,
            //        TradeDateRaw = raw.nTradingDay,
            //        HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal),
            //        LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal),
            //        Index = raw.nStockIdx,
            //        Market = short.Parse(raw.bstrMarketNo),
            //        DecimalPos = raw.sDecimal,
            //        TotalQtyBefore = raw.nYQty,
            //    };

            //    SaveQuotes(_appCtrl.Config.QuoteFolder, true, $"Unknown_", string.Empty, quoteBK);
            //    return false;
            //}

            bool firstTick = false;

            //quote.Symbol = raw.bstrStockNo;
            //quote.Name = raw.bstrStockName;
            //quote.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
            //quote.DealQty = raw.nTickQty;
            quote.BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestBuyQty = raw.nBc;
            quote.BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestSellQty = raw.nAc;
            if (quote.DealQty > 0)
            {
                if (IsAMMarket && (quote.Market == Definition.MarketFutures || quote.Market == Definition.MarketOptions) && (_appCtrl.Config.StartOnTime || quote.Recovered))
                {
                    if (raw.nSimulate.IsRealTrading() && quote.OpenPrice == 0) //開盤第一筆成交
                    {
                        quote.OpenPrice = quote.DealPrice;
                        firstTick = true;
                    }
                }
            }
            if (raw.nSimulate.IsRealTrading() && !quote.Recovered)
            {
                quote.OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal);
                quote.HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal);
                quote.LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal);
            }
            quote.Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Simulate = raw.nSimulate;
            quote.TotalQty = raw.nTQty;
            //quote.TradeDateRaw = raw.nTradingDay;
            quote.HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal);
            quote.LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal);
            //quote.Index = raw.nStockIdx;
            //quote.Market = short.TryParse(raw.bstrMarketNo, out short x) ? x : (short)-1;
            quote.DecimalPos = raw.sDecimal;
            quote.TotalQtyBefore = raw.nYQty;

            if (quote.Page < 0 || quote.DealQty == 0) //沒有訂閱SKQuoteLib_RequestLiveTick
            {
                quote.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
                quote.DealQty = raw.nTickQty;
                if (raw.nTradingDay > quote.TradeDateRaw)
                {
                    quote.TradeDateRaw = raw.nTradingDay;
                }
            }

            quote.Updater = nameof(UpdateQuote);
            quote.UpdateTime = DateTime.Now;

            QuoteTimer = (quote.UpdateTime, QuoteTimer.Item2, quote.Updater);

            if (IsAMMarket && (quote.Market == Definition.MarketFutures || quote.Market == Definition.MarketOptions) && (_appCtrl.Config.StartOnTime || quote.Recovered))
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if (quote.LowPrice > quote.DealPrice || quote.LowPrice == 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            if (firstTick)
            {
                _appCtrl.LogTrace($"SKAPI|開盤|{quote.Market}|{quote.Symbol}|{quote.Name}|DealPrice={quote.DealPrice}|DealQty={quote.DealQty}|OpenPrice={quote.OpenPrice}|Simulate={quote.Simulate}");
            }

            return true;
        }

        private void ReadLastClosePrice(FileInfo quoteFile)
        {
            if (quoteFile == null)
            {
                return;
            }

            try
            {
                _appCtrl.LogTrace($"SKAPI|Start|{quoteFile.FullName}");

                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);
                    if (quoteSub != null && quoteSub.LastClosePrice == 0)
                    {
                        if (quoteLast.Market == Definition.MarketFutures || quoteLast.Market == Definition.MarketOptions)
                        {
                            quoteSub.LastClosePrice = quoteLast.DealPrice;
                        }
                        else if (quoteSub.TradeDate > quoteLast.TradeDate)
                        {
                            quoteSub.LastClosePrice = quoteLast.DealPrice;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        private void ReadLastClosePriceAsync()
        {
            if (string.IsNullOrWhiteSpace(QuoteFileNameBase) && string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileClosePrefix))
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _appCtrl.Config.QuoteFolder.Refresh();
                    FileInfo[] files = _appCtrl.Config.QuoteFolder.GetFiles($"{_appCtrl.Settings.QuoteFileClosePrefix}*.csv");
                    FileInfo lastQuote1 = null;
                    FileInfo lastQuote2 = null;

                    for (int i = files.Length - 1; i >= 0; --i)
                    {
                        if (files[i].Name.Contains(QuoteFileNameBase) && i > 0 && !files[i - 1].Name.Contains(QuoteFileNameBase))
                        {
                            lastQuote1 = files[i - 1];

                            if (i > 1)
                            {
                                lastQuote2 = files[i - 2];
                            }

                            break;
                        }
                    }

                    if (lastQuote1 == null)
                    {
                        if (files.Length > 0)
                        {
                            lastQuote1 = files[files.Length - 1];

                            if (files.Length > 1)
                            {
                                lastQuote2 = files[files.Length - 2];
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    ReadLastClosePrice(lastQuote1);
                    ReadLastClosePrice(lastQuote2);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
            });
        }

        public void GetProductInfo()
        {
            _appCtrl.LogTrace($"SKAPI|Start");

            try
            {
                if (QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    throw new ArgumentException($"SKAPI|QuoteStatus != {StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY}|QuoteStatusStr={QuoteStatusStr}");
                }
                else if (_quoteIndexMap.Count > 0)
                {
                    throw new ArgumentException($"SKAPI|_quoteCollection.Count > 0|Count={_quoteIndexMap.Count}|Quotes are subscribed.");
                }

                QuoteFileNameBase = string.Empty;

                foreach (string product in _appCtrl.Config.QuoteSubscribed)
                {
                    SKSTOCKLONG pSKStockLONG = new SKSTOCKLONG();
                    int nCode = m_SKQuoteLib.SKQuoteLib_GetStockByNoLONG(product, ref pSKStockLONG); //根據商品代號，取回商品報價的相關資訊
                    if (nCode != 0)
                    {
                        LogAPIMessage(nCode);
                        continue;
                    }

                    QuoteData quote = CreateQuote(pSKStockLONG);
                    _appCtrl.Trigger.Reset(quote);
                    _quoteIndexMap.Add(quote.Index, quote);
                    _quoteCollection.Add(quote);
                }

                if (_quoteIndexMap.Count > 0)
                {
                    DateTime now = DateTime.Now;
                    int tradeDate = _quoteIndexMap.Values.Max(x => x.TradeDateRaw);

                    if (now.Hour >= 14 && tradeDate <= int.Parse(now.ToString("yyyyMMdd")) && !IsAMMarket)
                    {
                        QuoteFileNameBase = $"{tradeDate}_1";
                        _appCtrl.LogTrace($"SKAPI|未訂閱或尚未收到夜盤商品基本資料|QuoteFileNameBase={QuoteFileNameBase}");
                    }
                    else if (IsAMMarket)
                    {
                        QuoteFileNameBase = $"{tradeDate}_1";
                    }
                    else
                    {
                        QuoteFileNameBase = $"{tradeDate}_0";
                    }
                }

                ReadLastClosePriceAsync();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        private void RecoverOpenQuotesFromFile()
        {
            if (!IsAMMarket || string.IsNullOrWhiteSpace(QuoteFileNameBase) || string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileClosePrefix))
            {
                return;
            }

            try
            {
                _appCtrl.Config.QuoteFolder.Refresh();
                FileInfo[] files = _appCtrl.Config.QuoteFolder.GetFiles($"{_appCtrl.Settings.QuoteFileClosePrefix}*.csv");
                FileInfo quoteFile = files.LastOrDefault(x => x.Name.Contains(QuoteFileNameBase));

                if (quoteFile == null)
                {
                    return;
                }
                _appCtrl.LogTrace($"SKAPI|{quoteFile.FullName}");

                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);
                    if (quoteSub != null && (quoteLast.Market == Definition.MarketFutures || quoteLast.Market == Definition.MarketOptions))
                    {
                        quoteSub.DealPrice = quoteLast.DealPrice;
                        quoteSub.DealQty = quoteLast.DealQty;
                        quoteSub.OpenPrice = quoteLast.OpenPrice;
                        quoteSub.HighPrice = quoteLast.HighPrice;
                        quoteSub.LowPrice = quoteLast.LowPrice;
                        quoteSub.Recovered = true;
                        _appCtrl.LogTrace($"SKAPI|檔案回補開盤|{quoteSub.Market}|{quoteSub.Symbol}|{quoteSub.Name}|DealPrice={quoteSub.DealPrice}|DealQty={quoteSub.DealQty}|OpenPrice={quoteSub.OpenPrice}|HighPrice={quoteSub.HighPrice}|LowPrice={quoteSub.LowPrice}|Simulate={quoteSub.Simulate}");
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
        }

        public void SubQuotesAsync()
        {
            if (_quoteIndexMap.Count <= 0)
            {
                return;
            }

            _appCtrl.LogTrace($"SKAPI|Start");

            RecoverOpenQuotesFromFile();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    DateTime now = DateTime.Now;
                    bool isHoliday = _appCtrl.Config.IsHoliday(now);

                    foreach (QuoteData quote in _quoteIndexMap.Values)
                    {
                        if (isHoliday) //假日不訂閱即時報價
                        {
                            continue;
                        }
                        else if (!_appCtrl.Settings.QuoteLive.Contains(quote.Symbol))
                        {
                            continue;
                        }
                        else if (!_appCtrl.Config.IsAMMarket(now) && quote.Market != Definition.MarketFutures && quote.Market != Definition.MarketOptions) //期貨選擇權夜盤，上市櫃已經收盤
                        {
                            continue;
                        }

                        short pageA = -1;
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestLiveTick(ref pageA, quote.Symbol); //訂閱與要求傳送即時成交明細。(本功能不會訂閱最佳五檔，亦不包含歷史Ticks)
                        if (nCode != 0)
                        {
                            LogAPIMessage(nCode);
                            continue;
                        }
                        if (pageA < 0)
                        {
                            _appCtrl.LogError($"SKAPI|Sub quote failed.|Symbol={quote.Symbol}|pageA={pageA}");
                        }

                        quote.Page = pageA;
                    }

                    if (_appCtrl.Config.QuoteSubscribed.Count > 0)
                    {
                        string requests = string.Join(",", _appCtrl.Config.QuoteSubscribed);
                        short pageB = 1;

                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestStocks(ref pageB, requests); //訂閱指定商品即時報價，要求伺服器針對 bstrStockNos 內的商品代號訂閱商品報價通知動作
                        if (nCode != 0)
                        {
                            LogAPIMessage(nCode);

                            //nCode=3030|SK_SUBJECT_NO_QUOTE_SUBSCRIBE|即時行情連線數已達上限，行情訂閱功能受限
                            _appCtrl.LogWarn($"SKAPI|QuoteStatus is changing.|before={QuoteStatus}|after={nCode + StatusCode.BaseWarnValue}");
                            QuoteStatus = nCode + StatusCode.BaseWarnValue;
                        }
                        if (pageB < 0)
                        {
                            _appCtrl.LogError($"SKAPI|Sub quote failed.|requests={requests}|pageB={pageB}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
            });
        }

        public void RecoverQuotesAsync(string products)
        {
            Task.Factory.StartNew(() =>
            {
                _appCtrl.LogTrace($"SKAPI|Start|products={products}");

                try
                {
                    foreach (string product in products.Split(Separator.CSV, StringSplitOptions.RemoveEmptyEntries))
                    {
                        short pageA = -1;
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestTicks(ref pageA, product.Trim()); //訂閱要求傳送成交明細以及五檔
                        if (nCode != 0)
                        {
                            LogAPIMessage(nCode);
                            continue;
                        }
                        if (pageA < 0)
                        {
                            _appCtrl.LogError($"SKAPI|Recover quote failed.|Symbol={product}|pageA={pageA}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(ex, ex.StackTrace);
                }
                finally
                {
                    _appCtrl.LogTrace("SKAPI|End");
                }
            });
        }

        public void RequestKLine(string product = "")
        {
            _appCtrl.LogTrace($"SKAPI|Start|product={product}");

            try
            {
                if (string.IsNullOrWhiteSpace(product))
                {
                    int nCode = m_SKQuoteLib.SKQuoteLib_RequestKLineAM(product, 0, 1, 0); //（僅提供歷史資料）向報價伺服器提出，取得單一商品技術分析資訊需求，可選AM盤或全盤
                    LogAPIMessage(nCode);
                }
                else
                {
                    foreach (QuoteData quote in _quoteIndexMap.Values)
                    {
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestKLineAM(quote.Symbol, 0, 1, 0);
                        LogAPIMessage(nCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        public void SaveQuotes(DirectoryInfo folder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY || _quoteIndexMap.Count <= 0 || string.IsNullOrWhiteSpace(QuoteFileNameBase))
            {
                return;
            }

            try
            {
                _appCtrl.LogTrace($"SKAPI|Start|folder={folder?.Name}|append={append}|prefix={prefix}|suffix={suffix}");

                QuoteData[] quotes = _quoteIndexMap.Values.ToArray();
                string path = Path.Combine(folder.FullName, $"{prefix}{QuoteFileNameBase}{suffix}.csv");
                bool exists = File.Exists(path);

                using (StreamWriter sw = new StreamWriter(path, append, TextEncoding.UTF8WithoutBOM))
                {
                    if (!append || !exists)
                    {
                        sw.WriteLine(QuoteData.CSVColumnNames);
                    }

                    if (quote == null)
                    {
                        foreach (QuoteData q in quotes)
                        {
                            try
                            {
                                sw.WriteLine(q.ToCSVString());
                            }
                            catch (Exception ex)
                            {
                                _appCtrl.LogException(ex, ex.StackTrace);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            sw.WriteLine(quote.ToCSVString());
                        }
                        catch (Exception ex)
                        {
                            _appCtrl.LogException(ex, ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        public Task SaveQuotesAsync(DirectoryInfo quoteFolder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY || _quoteIndexMap.Count <= 0 || string.IsNullOrWhiteSpace(QuoteFileNameBase))
            {
                return null;
            }

            return Task.Factory.StartNew(() => SaveQuotes(quoteFolder, append, prefix, suffix, quote));
        }

        public int ReadCertification()
        {
            _appCtrl.LogTrace("SKAPI|Start");

            try
            {
                if (m_pSKCenter != null && m_pSKOrder != null)
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
                LogAPIMessage(ReadCertResult);
                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
                }

                //讀取憑證資訊。委託下單必須透過憑證，因此當元件初始化成功後即需要做讀取憑證的動作，如果使用群組的帳號做初始，則必須自行將所有的帳號依序做讀取憑證的動作。
                //如果送出委託前未經讀取憑證，送委託會得到 SK_ERROR_ORDER_SIGN_INVALID 的錯誤
                ReadCertResult = m_pSKOrder.ReadCertByID(Account);
                LogAPIMessage(ReadCertResult);
                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }

            return ReadCertResult;
        }

        public void GetGetOrderAccs()
        {
            _appCtrl.LogTrace("SKAPI|Start");

            try
            {
                _stockAccCollection.Clear();
                _futuresAccCollection.Clear();

                int m_nCode = m_pSKOrder.GetUserAccount(); //取回目前可交易的所有帳號。資料由OnAccount事件回傳
                LogAPIMessage(m_nCode);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        /// <summary>
        /// <para>下單解鎖。下單函式上鎖後需經由此函式解鎖才可繼續下單</para>
        /// <para>0：TS(證券)</para>
        /// <para>1：TF(期貨)</para>
        /// <para>2：TO(選擇權)</para>
        /// <para>3：OS(複委託)</para>
        /// <para>4：OF(海外期貨)</para>
        /// <para>5：OO(海外選擇權)</para>
        /// </summary>
        /// <param name="marketKind"></param>
        public void UnlockOrder(int marketKind)
        {
            _appCtrl.LogTrace($"SKAPI|Start|marketKind={marketKind}");

            try
            {
                int m_nCode = m_pSKOrder.UnlockOrder(marketKind);
                LogAPIMessage(m_nCode);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }

        public void GetOpenInterest(string orderAcc, int format = 1)
        {
            _appCtrl.LogTrace($"SKAPI|Start|orderAcc={orderAcc}|format={format}");

            try
            {
                int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(Account, orderAcc, format); //查詢期貨未平倉－可指定回傳格式
                LogAPIMessage(m_nCode);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("SKAPI|End");
            }
        }
    }
}
