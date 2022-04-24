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

        private readonly Dictionary<int, SKSTOCKLONG> _capitalProductRawMap;

        public QuoteData QuoteLastUpdated { get; private set; }
        private readonly Dictionary<int, QuoteData> _quoteIndexMap;
        private readonly ObservableCollection<QuoteData> _quoteCollection;

        public string OrderNotice { get; private set; }

        public int ReadCertResult { get; private set; }

        private readonly SortedDictionary<string, OrderAccData> _orderAccMap;
        private readonly ObservableCollection<OrderAccData> _orderAccCollection;

        public int OrderAccCount => _orderAccMap.Count;

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

            _capitalProductRawMap = new Dictionary<int, SKSTOCKLONG>();

            QuoteLastUpdated = new QuoteData();
            _quoteIndexMap = new Dictionary<int, QuoteData>();
            _appCtrl.MainForm.DataGridQuoteSubscribed.SetHeadersByBindings(QuoteData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _quoteCollection = _appCtrl.MainForm.DataGridQuoteSubscribed.SetAndGetItemsSource<QuoteData>();

            OrderNotice = string.Empty;

            ReadCertResult = -1;

            _orderAccMap = new SortedDictionary<string, OrderAccData>();
            _orderAccCollection = _appCtrl.MainForm.ComboBoxOrderAccs.SetAndGetItemsSource<OrderAccData>();

            _buySell = _appCtrl.MainForm.ComboBoxOrderBuySell.SetAndGetItemsSource(OrderBS.Description);
            _appCtrl.MainForm.ComboBoxOrderBuySell.SelectedIndex = (int)OrderBS.Enum.Buy;

            _tradeTypes = _appCtrl.MainForm.ComboBoxOrderTradeType.SetAndGetItemsSource(OrderTradeType.Description);
            _appCtrl.MainForm.ComboBoxOrderTradeType.SelectedIndex = (int)OrderTradeType.Enum.ROD;

            _dayTrade = _appCtrl.MainForm.ComboBoxOrderDayTrade.SetAndGetItemsSource(OrderDayTrade.Description);
            _appCtrl.MainForm.ComboBoxOrderDayTrade.SelectedIndex = (int)OrderDayTrade.Enum.No;

            _positionKinds = _appCtrl.MainForm.ComboBoxOrderPositionKind.SetAndGetItemsSource(OrderPosition.Description);
            _appCtrl.MainForm.ComboBoxOrderPositionKind.SelectedIndex = (int)OrderPosition.Enum.Open;

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

        public (LogLevel, string) LogAPIMessage(int nCode, string msg = "", TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            msg = string.Format("{0}{1}{2}", msg, string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", GetAPIMessage(nCode));

            if (nCode < 0)
            {
                _appCtrl.LogError(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }

            int _code = nCode % StatusCode.BaseTraceValue;

            if (_code == 0)
            {
                _appCtrl.LogTrace(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
            //2020 SK_WARNING_PRECHECK_RESULT_FAIL Precheck 失敗(EX:RCode)
            //2021 SK_WARNING_PRECHECK_RESULT_EMPTY Precheck結果回傳空值
            //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
            //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
            //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
            else if (_code < 2000 || _code == 2020 || _code == 2021 || _code == 3021 || _code == 3022 || _code == 3033)
            {
                _appCtrl.LogError(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }
            else if (_code < 3000)
            {
                _appCtrl.LogWarn(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Warn, msg);
            }
            else
            {
                _appCtrl.LogTrace(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
        }

        public (LogLevel, string) LogAPIMessage(DateTime start, int nCode, string msg = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            return LogAPIMessage(nCode, msg, DateTime.Now - start, lineNumber, memberName);
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

            DateTime start = _appCtrl.StartTrace($"userID={userID}|dwp=********", UniqueName);

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
                    _appCtrl.LogTrace(start, $"LoginUserResult={LoginUserResult}|雙因子登入成功", UniqueName);
                    UserID = userID;
                    DWP = "********";
                }
                else if (LoginUserResult >= 600 && LoginUserResult <= 699)
                {
                    _appCtrl.LogTrace(start, $"LoginUserResult={LoginUserResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效", UniqueName);
                    UserID = userID;
                    DWP = "********";
                }
                else
                {
                    //1097 SK_ERROR_TELNET_LOGINSERVER_FAIL Telnet登入主機失敗，請確認您的環境(Firewall及hosts…等)
                    //1098 SK_ERROR_TELNET_AGREEMENTSERVER_FAIL Telnet同意書查詢主機失敗，請確認您的環境(Firewall及hosts…等)
                    (LogLevel, string) apiMsg = LogAPIMessage(start, LoginUserResult);
                    m_pSKCenter = null;
                    _appCtrl.Exit(apiMsg.Item2, apiMsg.Item1);
                    return LoginUserResult;
                }

                //if (LoginUserResult == 0 || (LoginUserResult >= 600 && LoginUserResult <= 699))
                //{
                //    int nCode = m_pSKReply.SKReplyLib_ConnectByID(userID); //指定回報連線的使用者登入帳號
                //
                //    if (nCode != 0)
                //    {
                //        LogAPIMessage(start, nCode);
                //    }
                //}

                string version = m_pSKCenter.SKCenterLib_GetSKAPIVersionAndBit(userID); //取得目前註冊SKAPI 版本及位元
                _appCtrl.LogTrace(start, $"SKAPIVersionAndBit={version}", UniqueName);
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

        private void QuoteEnterMonitor(DateTime start)
        {
            QuoteStatus = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）

            //2020 SK_WARNING_PRECHECK_RESULT_FAIL Precheck 失敗(EX:RCode)
            //2021 SK_WARNING_PRECHECK_RESULT_EMPTY Precheck結果回傳空值
            //https://www.capital.com.tw/Service2/download/API_BBS.asp
            LogAPIMessage(start, QuoteStatus);
            //(LogLevel, string) apiMsg = LogAPIMessage(start, QuoteStatus);
            //if (QuoteStatus == 2020 || _appCtrl.Capital.QuoteStatus == 2021)
            //{
            //    m_pSKCenter = null;
            //    m_SKQuoteLib = null;
            //    _appCtrl.Exit(apiMsg.Item2, apiMsg.Item1);
            //}
        }

        public void LoginQuoteAsync(string dwp)
        {
            Task.Factory.StartNew(() =>
            {
                dwp = dwp.Trim();

                DateTime start = _appCtrl.StartTrace($"UserID={UserID}|dwp=********", UniqueName);

                try
                {
                    if (m_SKQuoteLib != null)
                    {
                        QuoteEnterMonitor(start);
                        return;
                    }

                    QuoteStatus = m_pSKCenter.SKCenterLib_LoginSetQuote(UserID, dwp, "Y"); //Y:啟用報價 N:停用報價

                    if (QuoteStatus == 0 || (QuoteStatus >= 600 && QuoteStatus <= 699))
                    {
                        _appCtrl.LogTrace(start, $"QuoteStatus={QuoteStatus}|登入成功", UniqueName);
                    }
                    else
                    {
                        LogAPIMessage(start, QuoteStatus);
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

                    QuoteEnterMonitor(start);
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
                LogAPIMessage(start, result);

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

        public (LogLevel, string) IsConnected()
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
                        return LogAPIMessage(start, isConnected);
                }

                int nCode = m_pSKReply.SKReplyLib_IsConnectedByID(UserID); //檢查輸入的帳號目前連線狀態 //正式環境： 0表示斷線。1表示連線中。2表示下載中

                if (nCode != 0 && nCode != 1 && nCode != 2)
                {
                    LogAPIMessage(start, nCode);
                }

                _appCtrl.LogTrace(start, result, UniqueName);

                return (LogLevel.Trace, result);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);

                return (LogLevel.Error, ex.Message);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
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

                    if (m_nCode != 0)
                    {
                        LogAPIMessage(start, m_nCode);
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

        private void ReadLastClosePrice(FileInfo quoteFile)
        {
            if (quoteFile == null)
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace(quoteFile.FullName, UniqueName);

            try
            {
                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);

                    if (quoteSub != null && quoteSub.LastClosePrice == 0)
                    {
                        if (quoteLast.MarketGroupEnum == Market.EGroup.Futures || quoteLast.MarketGroupEnum == Market.EGroup.Option)
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

        public (int, SKSTOCKLONG) GetProductInfo(string symbol, DateTime start)
        {
            SKSTOCKLONG pSKStockLONG = new SKSTOCKLONG();
            int nCode = m_SKQuoteLib.SKQuoteLib_GetStockByNoLONG(symbol, ref pSKStockLONG); //根據商品代號，取回商品報價的相關資訊

            if (nCode != 0)
            {
                LogAPIMessage(start, nCode);
            }

            return (nCode, pSKStockLONG);
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

                List<QuoteData> optionList = new List<QuoteData>();

                foreach (string symbol in _appCtrl.Config.QuoteSubscribed)
                {
                    (int, SKSTOCKLONG) product = GetProductInfo(symbol, start);

                    if (product.Item1 != 0)
                    {
                        continue;
                    }

                    _capitalProductRawMap.Add(product.Item2.nStockIdx, product.Item2);

                    QuoteData quote = CreateQuote(product.Item2);

                    _appCtrl.Trigger.Reset(quote);
                    _appCtrl.Strategy.Reset(quote);

                    try
                    {
                        _quoteIndexMap.Add(quote.Index, quote);

                        if (quote.MarketGroupEnum == Market.EGroup.Option)
                        {
                            optionList.Add(quote);
                        }
                        else
                        {
                            _quoteCollection.Add(quote);
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, $"{quote.ToCSVString()}{Environment.NewLine}{ex.StackTrace}");
                    }
                }

                foreach (QuoteData option in optionList)
                {
                    _quoteCollection.Add(option);
                }

                if (_quoteIndexMap.Count > 0)
                {
                    int tradeDate = _quoteIndexMap.Values.Max(x => x.TradeDateRaw);

                    if (start.Hour >= 14 && tradeDate <= int.Parse(start.ToString("yyyyMMdd")) && !IsAMMarket)
                    {
                        QuoteFileNameBase = $"{tradeDate}_{(int)Market.EDayNight.AM}";
                        _appCtrl.LogWarn(start, $"未訂閱或尚未收到夜盤商品基本資料|QuoteFileNameBase={QuoteFileNameBase}", UniqueName);
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
                start = _appCtrl.StartTrace(quoteFile.FullName, UniqueName);

                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = _quoteIndexMap.Values.FirstOrDefault(x => x.Symbol == quoteLast.Symbol);

                    if (quoteSub != null && (quoteLast.MarketGroupEnum == Market.EGroup.Futures || quoteLast.MarketGroupEnum == Market.EGroup.Option))
                    {
                        quoteSub.DealPrice = quoteLast.DealPrice;
                        quoteSub.DealQty = quoteLast.DealQty;
                        quoteSub.OpenPrice = quoteLast.OpenPrice;
                        quoteSub.HighPrice = quoteLast.HighPrice;
                        quoteSub.LowPrice = quoteLast.LowPrice;
                        quoteSub.Recovered = true;
                        _appCtrl.LogTrace(start, $"檔案回補開盤|{quoteSub.MarketGroupEnum}|{quoteSub.Symbol}|{quoteSub.Name}|DealPrice={quoteSub.DealPrice}|DealQty={quoteSub.DealQty}|OpenPrice={quoteSub.OpenPrice}|HighPrice={quoteSub.HighPrice}|LowPrice={quoteSub.LowPrice}|Simulate={quoteSub.Simulate}", UniqueName);
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
                        else if (!_appCtrl.Config.IsAMMarket(start) && quote.MarketGroupEnum != Market.EGroup.Futures && quote.MarketGroupEnum != Market.EGroup.Option) //期貨選擇權夜盤，上市櫃已經收盤
                        {
                            continue;
                        }

                        short qPage = -1;
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestLiveTick(ref qPage, quote.Symbol); //訂閱與要求傳送即時成交明細。(本功能不會訂閱最佳五檔，亦不包含歷史Ticks)

                        if (nCode != 0)
                        {
                            LogAPIMessage(start, nCode);
                            continue;
                        }

                        if (qPage < 0)
                        {
                            _appCtrl.LogError(start, $"Sub quote failed.|Symbol={quote.Symbol}|qPage={qPage}", UniqueName);
                        }

                        quote.Page = qPage;
                    }

                    if (_appCtrl.Config.QuoteSubscribed.Count > 0)
                    {
                        string requests = string.Join(",", _appCtrl.Config.QuoteSubscribed);
                        short qPage = 1;
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestStocks(ref qPage, requests); //訂閱指定商品即時報價，要求伺服器針對 bstrStockNos 內的商品代號訂閱商品報價通知動作

                        if (nCode != 0)
                        {
                            LogAPIMessage(start, nCode);

                            //nCode=3030|SK_SUBJECT_NO_QUOTE_SUBSCRIBE|即時行情連線數已達上限，行情訂閱功能受限
                            _appCtrl.LogWarn(start, $"QuoteStatus is changing.|before={QuoteStatus}|after={nCode + StatusCode.BaseWarnValue}", UniqueName);
                            QuoteStatus = nCode + StatusCode.BaseWarnValue;
                        }

                        if (qPage < 0)
                        {
                            _appCtrl.LogError(start, $"Sub quote failed.|requests={requests}|qPage={qPage}", UniqueName);
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
                DateTime start = _appCtrl.StartTrace($"products={products}", UniqueName);

                try
                {
                    foreach (string product in products.Split(','))
                    {
                        short qPage = -1;
                        int nCode = m_SKQuoteLib.SKQuoteLib_RequestTicks(ref qPage, product.Trim()); //訂閱要求傳送成交明細以及五檔

                        if (nCode != 0)
                        {
                            LogAPIMessage(start, nCode);
                            continue;
                        }

                        if (qPage < 0)
                        {
                            _appCtrl.LogError(start, $"Recover quote failed.|Symbol={product}|qPage={qPage}", UniqueName);
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
            DateTime start = _appCtrl.StartTrace($"product={product}", UniqueName);

            try
            {
                if (string.IsNullOrWhiteSpace(product))
                {
                    int nCode = m_SKQuoteLib.SKQuoteLib_RequestKLineAM(product, 0, 1, 0); //（僅提供歷史資料）向報價伺服器提出，取得單一商品技術分析資訊需求，可選AM盤或全盤

                    if (nCode != 0)
                    {
                        LogAPIMessage(start, nCode);
                    }
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

            DateTime start = _appCtrl.StartTrace($"folder={folder?.Name}|append={append}|prefix={prefix}|suffix={suffix}", UniqueName);

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
                LogAPIMessage(start, ReadCertResult);

                if (ReadCertResult != 0)
                {
                    m_pSKOrder = null;
                    return ReadCertResult;
                }

                //讀取憑證資訊。委託下單必須透過憑證，因此當元件初始化成功後即需要做讀取憑證的動作，如果使用群組的帳號做初始，則必須自行將所有的帳號依序做讀取憑證的動作。
                //如果送出委託前未經讀取憑證，送委託會得到 SK_ERROR_ORDER_SIGN_INVALID 的錯誤
                ReadCertResult = m_pSKOrder.ReadCertByID(UserID);
                LogAPIMessage(start, ReadCertResult);

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
                _orderAccMap.Clear();
                _orderAccCollection.Clear();

                int m_nCode = m_pSKOrder.GetUserAccount(); //取回目前可交易的所有帳號。資料由OnAccount事件回傳

                if (m_nCode != 0)
                {
                    LogAPIMessage(start, m_nCode);
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

        public void UnlockOrder(int marketType = -1)
        {
            DateTime start = _appCtrl.StartTrace($"marketType={marketType}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.UnlockOrder(marketType); //下單解鎖。下單函式上鎖後需經由此函式解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        LogAPIMessage(start, m_nCode);
                    }

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

            DateTime start = _appCtrl.StartTrace($"marketType={marketType}|maxQty={maxQty}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxQty(marketType, maxQty); //設定每秒委託「量」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        LogAPIMessage(start, m_nCode);
                    }

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

            DateTime start = _appCtrl.StartTrace($"marketType={marketType}|maxCount={maxCount}", UniqueName);

            try
            {
                if (marketType >= 0 && marketType < Market.CodeDescription.Count)
                {
                    int m_nCode = m_pSKOrder.SetMaxCount(marketType, maxCount); //設定每秒委託「筆數」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單

                    if (m_nCode != 0)
                    {
                        LogAPIMessage(start, m_nCode);
                    }

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
                DateTime start = _appCtrl.StartTrace($"orderAcc={orderAcc}|format={format}", UniqueName);

                try
                {
                    if (string.IsNullOrWhiteSpace(orderAcc))
                    {
                        foreach (OrderAccData acc in _orderAccCollection)
                        {
                            if (acc.MarketType != Market.EType.Futures)
                            {
                                continue;
                            }

                            //nCode=1019|SK_ERROR_QUERY_IN_PROCESSING|GetOpenInterest_Format::1
                            Thread.Sleep(1 * 1000);
                            GetOpenInterestAsync(acc.FullAccount, format);
                            Thread.Sleep(7 * 1000);
                        }
                    }
                    else
                    {
                        int m_nCode = m_pSKOrder.GetOpenInterestWithFormat(UserID, orderAcc, format); //查詢期貨未平倉－可指定回傳格式

                        if (m_nCode != 0)
                        {
                            LogAPIMessage(start, m_nCode);
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

        public void SendFutureOrder(StrategyData order)
        {
            DateTime start = _appCtrl.StartTrace($"{order?.ToLog()}", UniqueName);

            try
            {
                if (string.IsNullOrWhiteSpace(order.PrimaryKey))
                {
                    order.PrimaryKey = $"{order.CreatedTime:HH:mm:ss.fff}_{StrategyStatus.Enum.OrderSent}";

                    if (_appCtrl.Strategy[order.PrimaryKey] != null)
                    {
                        throw new ArgumentException($"_appCtrl.Strategy[{order.PrimaryKey}] != null|{order.ToLog()}");
                    }
                    else if (_appCtrl.Strategy.GetOrderDetail(order.PrimaryKey) != null)
                    {
                        throw new ArgumentException($"_appCtrl.Strategy.GetOrderDetail({order.PrimaryKey}) != null|{order.ToLog()}");
                    }
                }
                else if (_appCtrl.Strategy[order.PrimaryKey] == order)
                {
                    throw new ArgumentException($"_appCtrl.Strategy[{order.PrimaryKey}] == order|{order.ToLog()}");
                }
                else if (_appCtrl.Strategy.GetOrderDetail(order.PrimaryKey) != null)
                {
                    throw new ArgumentException($"_appCtrl.Strategy.GetOrderDetail({order.PrimaryKey}) != null|{order.ToLog()}");
                }

                _appCtrl.Strategy.AddOrder(order);
                _appCtrl.Strategy.OrderCheck(order, start);

                FUTUREORDER pFutureOrder = CreateCaptialFutureOrder(order);

                string orderMsg = $"_appCtrl.Settings.SendRealOrder={_appCtrl.Settings.SendRealOrder}"; //如果回傳值為 0表示委託成功，訊息內容則為13碼的委託序號
                int m_nCode = 0;
                (LogLevel, string) apiMsg = (LogLevel.Trace, orderMsg);

                order.StatusEnum = StrategyStatus.Enum.OrderSent;

                if (_appCtrl.Settings.SendRealOrder)
                {
                    lock (_syncOrderLock)
                    {
                        //送出期貨委託，無需倉位，預設為盤中，不可更改
                        //SKReplyLib.OnNewData，當有回報將主動呼叫函式，並通知委託的狀態。(新格式 包含預約單回報)
                        m_nCode = m_pSKOrder.SendFutureOrder(UserID, false, pFutureOrder, out orderMsg);
                        apiMsg = LogAPIMessage(start, m_nCode, orderMsg);

                        //Thread.Sleep(100);
                    }
                }
                else
                {
                    _appCtrl.Log(apiMsg.Item1, $"m_nCode={m_nCode}|{orderMsg}", UniqueName, DateTime.Now - start);
                }

                OrderNotice = orderMsg;

                order.StatusEnum = m_nCode == 0 ? StrategyStatus.Enum.OrderReport : StrategyStatus.Enum.OrderError;
                order.OrderReport = orderMsg;
                order.Updater = nameof(SendFutureOrder);
                order.UpdateTime = DateTime.Now;

                if (!_appCtrl.Settings.SendRealOrder)
                {
                    order.StatusEnum = StrategyStatus.Enum.DealReport;
                    order.DealPrice = decimal.TryParse(order.OrderPrice, out decimal pri) ? pri : order.MarketPrice;
                    order.DealQty = order.OrderQty;
                    order.DealReport = order.OrderReport;
                    order.Updater = nameof(SendFutureOrder);
                    order.UpdateTime = DateTime.Now;

                    if (order.Parent != null)
                    {
                        switch (order.Parent.StatusEnum)
                        {
                            case StrategyStatus.Enum.OrderSent:
                                order.Parent.StatusEnum = StrategyStatus.Enum.DealReport;
                                break;
                            case StrategyStatus.Enum.StopLossSent:
                                order.Parent.StatusEnum = StrategyStatus.Enum.StopLossDealReport;
                                break;
                            case StrategyStatus.Enum.StopWinSent:
                                order.Parent.StatusEnum = StrategyStatus.Enum.StopWinDealReport;
                                break;
                            case StrategyStatus.Enum.MoveStopWinSent:
                                order.Parent.StatusEnum = StrategyStatus.Enum.MoveStopWinDealReport;
                                break;
                        }

                        order.Parent.Updater = nameof(SendFutureOrder);
                        order.Parent.UpdateTime = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                OrderNotice = ex.Message;

                order.StatusEnum = StrategyStatus.Enum.OrderError;
                order.OrderReport = ex.Message;
                order.Updater = nameof(SendFutureOrder);
                order.UpdateTime = DateTime.Now;
            }
            finally
            {
                if (order.Parent != null)
                {
                    switch (order.Parent.StatusEnum)
                    {
                        case StrategyStatus.Enum.OrderSent:
                            order.Parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.OrderReport : StrategyStatus.Enum.OrderError;
                            break;
                        case StrategyStatus.Enum.StopLossSent:
                            order.Parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.StopLossOrderReport : StrategyStatus.Enum.StopLossError;
                            break;
                        case StrategyStatus.Enum.StopWinSent:
                            order.Parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.StopWinOrderReport : StrategyStatus.Enum.StopWinError;
                            break;
                        case StrategyStatus.Enum.MoveStopWinSent:
                            order.Parent.StatusEnum = order.StatusEnum == StrategyStatus.Enum.OrderReport ? StrategyStatus.Enum.MoveStopWinOrderReport : StrategyStatus.Enum.MoveStopWinError;
                            break;
                    }

                    order.Parent.Updater = nameof(SendFutureOrder);
                    order.Parent.UpdateTime = DateTime.Now;
                }

                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        public void SendFutureOrderAsync(StrategyData order)
        {
            Task.Factory.StartNew(() => SendFutureOrder(order));
        }

        public void CancelOrderBySeqNo(OrderAccData acc, string seqNo)
        {
            DateTime start = _appCtrl.StartTrace($"{acc?.FullAccount}|seqNo={seqNo}", UniqueName);

            try
            {
                //TODO

                string strMessage = "";
                int m_nCode = m_pSKOrder.CancelOrderBySeqNo(UserID, false, acc.FullAccount, seqNo, out strMessage); //國內委託删單(By委託序號)

                if (m_nCode != 0)
                {
                    LogAPIMessage(start, m_nCode);
                }

                _appCtrl.LogTrace(start, strMessage, UniqueName);
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
