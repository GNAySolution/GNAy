using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
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
        public readonly string UniqueName;
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

        public int LoginUserResult { get; private set; }
        public string UserID { get; private set; }
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

        public (DateTime, string) UserIDTimer { get; private set; }
        public string QuoteTimer { get; private set; }

        public readonly bool IsAMMarket;
        public string QuoteFileNameBase { get; private set; }

        private readonly Dictionary<int, APIReplyData> _apiReplyMap;
        private readonly ObservableCollection<APIReplyData> _apiReplyCollection;

        public QuoteData QuoteLastUpdated { get; private set; }
        private readonly Dictionary<int, QuoteData> _quoteIndexMap;
        private readonly ObservableCollection<QuoteData> _quoteCollection;

        public int ReadCertResult { get; private set; }

        private readonly ObservableCollection<OrderAccData> _stockAccCollection;
        public int StockAccCount => _stockAccCollection.Count;

        private readonly ObservableCollection<OrderAccData> _futuresAccCollection;
        public int FuturesAccCount => _futuresAccCollection.Count;

        private readonly ObservableCollection<string> _buySell;
        private readonly ObservableCollection<string> _tradeTypes;
        private readonly ObservableCollection<string> _dayTrade;
        private readonly ObservableCollection<string> _positionKinds;

        private readonly object _syncOrderLock;

        public CapitalController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            LoginUserResult = -1;
            UserID = string.Empty;
            DWP = string.Empty;

            QuoteStatus = -1;

            UserIDTimer = (DateTime.MinValue, string.Empty);
            QuoteTimer = string.Empty;

            IsAMMarket = _appCtrl.Config.IsAMMarket(CreatedTime);
            QuoteFileNameBase = string.Empty;

            _apiReplyMap = new Dictionary<int, APIReplyData>();
            _appCtrl.MainForm.DataGridAPIReply.SetHeadersByBindings(APIReplyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _apiReplyCollection = _appCtrl.MainForm.DataGridAPIReply.SetAndGetItemsSource<APIReplyData>();

            QuoteLastUpdated = new QuoteData();
            _quoteIndexMap = new Dictionary<int, QuoteData>();
            _appCtrl.MainForm.DataGridQuoteSubscribed.SetHeadersByBindings(QuoteData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _quoteCollection = _appCtrl.MainForm.DataGridQuoteSubscribed.SetAndGetItemsSource<QuoteData>();

            ReadCertResult = -1;

            _stockAccCollection = _appCtrl.MainForm.ComboBoxStockAccs.SetAndGetItemsSource<OrderAccData>();
            _futuresAccCollection = _appCtrl.MainForm.ComboBoxFuturesAccs.SetAndGetItemsSource<OrderAccData>();

            _buySell = _appCtrl.MainForm.ComboBoxOrderBuySell.SetAndGetItemsSource(BS.Description);
            _appCtrl.MainForm.ComboBoxOrderBuySell.SelectedIndex = (int)BS.Enum.Buy;

            _tradeTypes = _appCtrl.MainForm.ComboBoxOrderTradeType.SetAndGetItemsSource(TradeType.Description);
            _appCtrl.MainForm.ComboBoxOrderTradeType.SelectedIndex = (int)TradeType.Enum.ROD;

            _dayTrade = _appCtrl.MainForm.ComboBoxOrderDayTrade.SetAndGetItemsSource(DayTrade.Description);
            _appCtrl.MainForm.ComboBoxOrderDayTrade.SelectedIndex = (int)DayTrade.Enum.No;

            _positionKinds = _appCtrl.MainForm.ComboBoxOrderPositionKind.SetAndGetItemsSource(PositionKind.Description);
            _appCtrl.MainForm.ComboBoxOrderPositionKind.SelectedIndex = (int)PositionKind.Enum.Open;

            _syncOrderLock = new object();
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

        public (LogLevel, string) LogAPIMessage(int nCode, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string msg = GetAPIMessage(nCode);

            if (nCode < 0)
            {
                _appCtrl.LogError(msg, UniqueName, null, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }

            int _code = nCode % StatusCode.BaseTraceValue;

            if (_code == 0)
            {
                _appCtrl.LogTrace(msg, UniqueName, null, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
            //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
            //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
            //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
            else if (_code < 2000 || _code == 3021 || _code == 3022 || _code == 3033)
            {
                _appCtrl.LogError(msg, UniqueName, null, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }
            else if (_code < 3000)
            {
                _appCtrl.LogWarn(msg, UniqueName, null, lineNumber, memberName);
                return (LogLevel.Warn, msg);
            }
            else
            {
                _appCtrl.LogTrace(msg, UniqueName, null, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
        }

        public void AppendReply(string userID, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            APIReplyData replay = new APIReplyData()
            {
                ThreadID = Thread.CurrentThread.ManagedThreadId,
                UserID = userID,
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
                        _appCtrl.MainForm.DataGridAPIReply.ScrollToBorderEnd();
                    }
                }
                catch
                { }
            });
        }

        public int LoginUser(string userID, string dwp)
        {
            userID = userID.Trim().ToUpper();
            dwp = dwp.Trim();

            DateTime start = _appCtrl.StartTrace(UniqueName, $"userID={userID}|dwp=********");

            try
            {
                if (m_pSKCenter != null)
                {
                    return LoginUserResult;
                }

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

                LoginUserResult = m_pSKCenter.SKCenterLib_Login(userID, dwp); //元件初始登入。在使用此 Library 前必須先通過使用者的雙因子(憑證綁定)身份認證，方可使用

                if (LoginUserResult == 0)
                {
                    _appCtrl.LogTrace($"LoginUserResult={LoginUserResult}|雙因子登入成功", UniqueName);
                    UserID = userID;
                    DWP = "********";
                }
                else if (LoginUserResult >= 600 && LoginUserResult <= 699)
                {
                    _appCtrl.LogTrace($"LoginUserResult={LoginUserResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效", UniqueName);
                    UserID = userID;
                    DWP = "********";
                }
                else
                {
                    LogAPIMessage(LoginUserResult);
                    m_pSKCenter = null;
                    return LoginUserResult;
                }

                //if (LoginUserResult == 0 || (LoginUserResult >= 600 && LoginUserResult <= 699))
                //{
                //    int nCode = m_pSKReply.SKReplyLib_ConnectByID(userID); //指定回報連線的使用者登入帳號
                //    if (nCode != 0)
                //    {
                //        LogAPIMessage(nCode);
                //    }
                //}

                string version = m_pSKCenter.SKCenterLib_GetSKAPIVersionAndBit(userID); //取得目前註冊SKAPI 版本及位元
                _appCtrl.LogTrace($"SKAPIVersionAndBit={version}", UniqueName);
                _appCtrl.MainForm.StatusBarItemBA2.Text = $"SKAPIVersionAndBit={version}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return LoginUserResult;
        }

        public void LoginQuoteAsync(string dwp)
        {
            Task.Factory.StartNew(() =>
            {
                dwp = dwp.Trim();

                DateTime start = _appCtrl.StartTrace(UniqueName, $"userID={UserID}|dwp=********");

                try
                {
                    if (m_SKQuoteLib != null)
                    {
                        QuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）
                        LogAPIMessage(QuoteStatus);
                        return;
                    }

                    QuoteStatus = m_pSKCenter.SKCenterLib_LoginSetQuote(UserID, dwp, "Y"); //Y:啟用報價 N:停用報價

                    if (QuoteStatus == 0 || (QuoteStatus >= 600 && QuoteStatus <= 699))
                    {
                        _appCtrl.LogTrace($"QuoteStatus={QuoteStatus}|登入成功", UniqueName);
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
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
                finally
                {
                    _appCtrl.EndTrace(start, UniqueName);
                }
            });
        }

        public int Disconnect()
        {
            DateTime start = _appCtrl.StartTrace();

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
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return -1;
        }

        public string IsConnected()
        {
            DateTime start = _appCtrl.StartTrace();

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
                        return LogAPIMessage(isConnected).Item2;
                }

                int nCode = m_pSKReply.SKReplyLib_IsConnectedByID(UserID);

                if (nCode != 0 && nCode != 1 && nCode != 2)
                {
                    LogAPIMessage(nCode);
                }

                _appCtrl.LogTrace(result, UniqueName);
                return result;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return string.Empty;
        }

        public void PrintProductList()
        {
            DateTime start = _appCtrl.StartTrace();

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
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ReadLastClosePrice(FileInfo quoteFile)
        {
            if (quoteFile == null)
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace(UniqueName, quoteFile.FullName);

            try
            {
                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);

                    if (quoteSub != null && quoteSub.LastClosePrice == 0)
                    {
                        if (quoteLast.MarketGroupEnum == Market.EGroup.Futures || quoteLast.MarketGroupEnum == Market.EGroup.Options)
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
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
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
                        if (files.Length > 0 && !files[files.Length - 1].Name.Contains(QuoteFileNameBase))
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
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    throw new ArgumentException($"QuoteStatus != {StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY}|QuoteStatusStr={QuoteStatusStr}");
                }
                else if (_quoteIndexMap.Count > 0)
                {
                    throw new ArgumentException($"_quoteCollection.Count > 0|Count={_quoteIndexMap.Count}|Quotes are subscribed.");
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
                    int tradeDate = _quoteIndexMap.Values.Max(x => x.TradeDateRaw);

                    if (start.Hour >= 14 && tradeDate <= int.Parse(start.ToString("yyyyMMdd")) && !IsAMMarket)
                    {
                        QuoteFileNameBase = $"{tradeDate}_{(int)Market.EDayNight.AM}";
                        _appCtrl.LogTrace($"未訂閱或尚未收到夜盤商品基本資料|QuoteFileNameBase={QuoteFileNameBase}", UniqueName);
                    }
                    else if (IsAMMarket)
                    {
                        QuoteFileNameBase = $"{tradeDate}_{(int)Market.EDayNight.AM}";
                    }
                    else
                    {
                        QuoteFileNameBase = $"{tradeDate}_{(int)Market.EDayNight.PM}";
                    }
                }

                ReadLastClosePriceAsync();
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

        private void RecoverOpenQuotesFromFile()
        {
            if (!IsAMMarket || string.IsNullOrWhiteSpace(QuoteFileNameBase) || string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileClosePrefix))
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Config.QuoteFolder.Refresh();
                FileInfo[] files = _appCtrl.Config.QuoteFolder.GetFiles($"{_appCtrl.Settings.QuoteFileClosePrefix}*.csv");
                FileInfo quoteFile = files.LastOrDefault(x => x.Name.Contains(QuoteFileNameBase));

                if (quoteFile == null)
                {
                    return;
                }
                start = _appCtrl.StartTrace(UniqueName, quoteFile.FullName);

                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);

                    if (quoteSub != null && (quoteLast.MarketGroupEnum == Market.EGroup.Futures || quoteLast.MarketGroupEnum == Market.EGroup.Options))
                    {
                        quoteSub.DealPrice = quoteLast.DealPrice;
                        quoteSub.DealQty = quoteLast.DealQty;
                        quoteSub.OpenPrice = quoteLast.OpenPrice;
                        quoteSub.HighPrice = quoteLast.HighPrice;
                        quoteSub.LowPrice = quoteLast.LowPrice;
                        quoteSub.Recovered = true;
                        _appCtrl.LogTrace($"檔案回補開盤|{quoteSub.MarketGroupEnum}|{quoteSub.Symbol}|{quoteSub.Name}|DealPrice={quoteSub.DealPrice}|DealQty={quoteSub.DealQty}|OpenPrice={quoteSub.OpenPrice}|HighPrice={quoteSub.HighPrice}|LowPrice={quoteSub.LowPrice}|Simulate={quoteSub.Simulate}", UniqueName);
                    }
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

        public void SubQuotesAsync()
        {
            if (_quoteIndexMap.Count <= 0)
            {
                return;
            }

            RecoverOpenQuotesFromFile();

            Task.Factory.StartNew(() =>
            {
                DateTime start = _appCtrl.StartTrace();

                try
                {
                    bool isHoliday = _appCtrl.Config.IsHoliday(start);

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
                        else if (!_appCtrl.Config.IsAMMarket(start) && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Options) //期貨選擇權夜盤，上市櫃已經收盤
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
                            _appCtrl.LogError($"Sub quote failed.|Symbol={quote.Symbol}|pageA={pageA}", UniqueName);
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
                            _appCtrl.LogWarn($"QuoteStatus is changing.|before={QuoteStatus}|after={nCode + StatusCode.BaseWarnValue}", UniqueName);
                            QuoteStatus = nCode + StatusCode.BaseWarnValue;
                        }

                        if (pageB < 0)
                        {
                            _appCtrl.LogError($"Sub quote failed.|requests={requests}|pageB={pageB}", UniqueName);
                        }
                    }

                    _appCtrl.MainForm.InvokeRequired(delegate
                    {
                        _appCtrl.MainForm.ComboBoxOrderProduct.SetAndGetItemsSource(_quoteIndexMap.Values.Select(x => x.Symbol));
                        _appCtrl.MainForm.ComboBoxOrderProduct.SelectedIndex = _appCtrl.MainForm.ComboBoxOrderProduct.Items.Count - 1;
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
            });
        }

        public void RecoverQuotesAsync(string products)
        {
            Task.Factory.StartNew(() =>
            {
                DateTime start = _appCtrl.StartTrace(UniqueName, $"products={products}");

                try
                {
                    foreach (string product in products.SplitToCSV())
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
                            _appCtrl.LogError($"Recover quote failed.|Symbol={product}|pageA={pageA}", UniqueName);
                        }
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
            });
        }

        public QuoteData GetQuote(string symbol)
        {
            return _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == symbol);
        }

        public void RequestKLine(string product = "")
        {
            DateTime start = _appCtrl.StartTrace(UniqueName, $"product={product}");

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
                        RequestKLine(quote.Symbol);
                    }
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

        public void SaveQuotes(DirectoryInfo folder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (_quoteIndexMap.Count <= 0 || string.IsNullOrWhiteSpace(QuoteFileNameBase))
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace(UniqueName, $"folder={folder?.Name}|append={append}|prefix={prefix}|suffix={suffix}");

            try
            {
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
                                _appCtrl.LogException(start, ex, ex.StackTrace);
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
                            _appCtrl.LogException(start, ex, ex.StackTrace);
                        }
                    }
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

        public Task SaveQuotesAsync(DirectoryInfo quoteFolder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (_quoteIndexMap.Count <= 0 || string.IsNullOrWhiteSpace(QuoteFileNameBase))
            {
                return null;
            }

            return Task.Factory.StartNew(() => SaveQuotes(quoteFolder, append, prefix, suffix, quote));
        }

        public int ReadCertification()
        {
            DateTime start = _appCtrl.StartTrace();

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
                ReadCertResult = m_pSKOrder.ReadCertByID(UserID);
                LogAPIMessage(ReadCertResult);

                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
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

            return ReadCertResult;
        }

        public void GetGetOrderAccs()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _stockAccCollection.Clear();
                _futuresAccCollection.Clear();

                int m_nCode = m_pSKOrder.GetUserAccount(); //取回目前可交易的所有帳號。資料由OnAccount事件回傳
                LogAPIMessage(m_nCode);
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

        public void UnlockOrder(int marketType = -1)
        {
            DateTime start = _appCtrl.StartTrace(UniqueName, $"marketType={marketType}");

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.UnlockOrder(marketType); //下單解鎖。下單函式上鎖後需經由此函式解鎖才可繼續下單
                    LogAPIMessage(m_nCode);
                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
                {
                    UnlockOrder(i);
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

        public void SetOrderMaxQty(int marketType = -1, int maxQty = -1)
        {
            if (maxQty <= 0)
            {
                maxQty = _appCtrl.Settings.OrderMaxQty;
            }

            DateTime start = _appCtrl.StartTrace(UniqueName, $"marketType={marketType}|maxQty={maxQty}");

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxQty(marketType, maxQty); //設定每秒委託「量」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單
                    LogAPIMessage(m_nCode);
                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
                {
                    SetOrderMaxQty(i, maxQty);
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

        public void SetOrderMaxCount(int marketType = -1, int maxCount = -1)
        {
            if (maxCount <= 0)
            {
                maxCount = _appCtrl.Settings.OrderMaxCount;
            }

            DateTime start = _appCtrl.StartTrace(UniqueName, $"marketType={marketType}|maxCount={maxCount}");

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxCount(marketType, maxCount); //設定每秒委託「筆數」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單
                    LogAPIMessage(m_nCode);
                    return;
                }
                else if (marketType >= Market.CodeDescription.Count)
                {
                    throw new ArgumentException($"marketType({marketType}) >= Market.CodeDescription.Count({Market.CodeDescription.Count})");
                }

                for (int i = 0; i < Market.CodeDescription.Count; ++i)
                {
                    SetOrderMaxCount(i, maxCount);
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

        public void GetOpenInterestAsync(string orderAcc = "", int format = 1)
        {
            Task.Factory.StartNew(() =>
            {
                DateTime start = _appCtrl.StartTrace(UniqueName, $"orderAcc={orderAcc}|format={format}");

                try
                {
                    if (string.IsNullOrWhiteSpace(orderAcc))
                    {
                        foreach (OrderAccData acc in _futuresAccCollection)
                        {
                            //nCode=1019|SK_ERROR_QUERY_IN_PROCESSING|GetOpenInterest_Format::1
                            Thread.Sleep(1 * 1000);
                            GetOpenInterestAsync(acc.FullAccount, format);
                            Thread.Sleep(7 * 1000);
                        }
                    }
                    else
                    {
                        int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(UserID, orderAcc, format); //查詢期貨未平倉－可指定回傳格式
                        LogAPIMessage(m_nCode);
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
            });
        }

        public void SendFutureOrder()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                FUTUREORDER pFutureOrder = new FUTUREORDER();

                pFutureOrder.bstrFullAccount = ((OrderAccData)_appCtrl.MainForm.ComboBoxFuturesAccs.SelectedItem).FullAccount;
                pFutureOrder.bstrPrice = _appCtrl.MainForm.TextBoxOrderPrice.Text;
                pFutureOrder.bstrStockNo = _appCtrl.MainForm.ComboBoxOrderProduct.Text;
                pFutureOrder.nQty = int.Parse(_appCtrl.MainForm.TextBoxOrderQuantity.Text);
                pFutureOrder.sBuySell = (short)_appCtrl.MainForm.ComboBoxOrderBuySell.SelectedIndex;
                pFutureOrder.sDayTrade = (short)_appCtrl.MainForm.ComboBoxOrderDayTrade.SelectedIndex;
                pFutureOrder.sTradeType = (short)_appCtrl.MainForm.ComboBoxOrderTradeType.SelectedIndex;
                pFutureOrder.sNewClose = (short)_appCtrl.MainForm.ComboBoxOrderPositionKind.SelectedIndex;

                string strMessage = nameof(_appCtrl.Settings.SendRealOrder); //如果回傳值為 0表示委託成功，訊息內容則為13碼的委託序號
                int m_nCode = 0;
                (LogLevel, string) apiReturn = (LogLevel.Trace, _appCtrl.Settings.SendRealOrder.ToString());

                if (_appCtrl.Settings.SendRealOrder)
                {
                    lock (_syncOrderLock)
                    {
                        m_nCode = m_pSKOrder.SendFutureOrder(UserID, false, pFutureOrder, out strMessage); //送出期貨委託，無需倉位，預設為盤中，不可更改
                        apiReturn = LogAPIMessage(m_nCode);

                        Thread.Sleep(100);
                    }
                }

                _appCtrl.Log(apiReturn.Item1, $"m_nCode={m_nCode}|strMessage={strMessage}", UniqueName);
                _appCtrl.Log(apiReturn.Item1, apiReturn.Item2, UniqueName);
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
    }
}
