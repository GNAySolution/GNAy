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
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalQuoteController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private SKQuoteLib m_SKQuoteLib;

        public string StatusStr { get; private set; }
        private int _status;
        public int Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                StatusStr = _appCtrl.CAPCenter.GetAPIMessage(value);
            }
        }

        public bool IsAMMarket { get; private set; }

        /// <summary>
        /// 在開盤前執行登入動作(未考慮登入失敗或其他異常情況)
        /// </summary>
        public bool LoadedOnTime { get; private set; }

        public DateTime MarketStartTime { get; private set; }
        public DateTime MarketCloseTime { get; private set; }

        public string Timer { get; private set; }

        public string FileNameBase { get; private set; }

        private readonly Dictionary<int, SKSTOCKLONG> _dataRawMap;

        public QuoteData LastData { get; private set; }
        private readonly Dictionary<int, QuoteData> _dataIndexMap;
        private readonly Dictionary<string, QuoteData> _dataSymbolMap;
        private readonly ObservableCollection<QuoteData> _dataCollection;
        public int Count => _dataCollection.Count;
        public QuoteData this[string symbol] => _dataSymbolMap.TryGetValue(symbol, out QuoteData data) ? data : _dataCollection.FirstOrDefault(x => x.Symbol == symbol);
        public IReadOnlyList<QuoteData> DataCollection => _dataCollection;

        public CapitalQuoteController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(CapitalQuoteController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            Status = -1;

            IsAMMarket = false;
            LoadedOnTime = false;
            MarketStartTime = DateTime.MinValue;
            MarketCloseTime = DateTime.MinValue;

            Timer = string.Empty;

            FileNameBase = string.Empty;

            _dataRawMap = new Dictionary<int, SKSTOCKLONG>();

            LastData = new QuoteData();
            _dataIndexMap = new Dictionary<int, QuoteData>();
            _dataSymbolMap = new Dictionary<string, QuoteData>();

            _appCtrl.MainForm.DataGridQuoteSubscribed.SetHeadersByBindings(QuoteData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridQuoteSubscribed.SetAndGetItemsSource<QuoteData>();
        }

        private CapitalQuoteController() : this(null)
        { }

        private void EnterMonitor(DateTime start)
        {
            Status = m_SKQuoteLib.SKQuoteLib_EnterMonitorLONG(); //與報價伺服器建立連線。（含盤中零股市場商品）

            //https://www.capital.com.tw/Service2/download/API_BBS.asp
            _appCtrl.CAPCenter.LogAPIMessage(start, Status);
            //(LogLevel, string) apiMsg = LogAPIMessage(start, Status);
            //if (Status == StatusCode.SK_WARNING_PRECHECK_RESULT_FAIL || _appCtrl.CAPQuote.Status == StatusCode.SK_WARNING_PRECHECK_RESULT_EMPTY)
            //{
            //    m_pSKCenter = null;
            //    m_SKQuoteLib = null;
            //    _appCtrl.Exit(apiMsg.Item2, apiMsg.Item1);
            //}
        }

        public void LoginAsync(string dwp)
        {
            Task.Factory.StartNew(() =>
            {
                DateTime start = _appCtrl.StartTrace();

                try
                {
                    if (m_SKQuoteLib != null)
                    {
                        EnterMonitor(start);
                        return;
                    }

                    Status = _appCtrl.CAPCenter.LoginQuote(dwp);

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

                    EnterMonitor(start);

                    IsAMMarket = _appCtrl.Config.IsAMMarket(CreatedTime);

                    bool startDelayed = false; //因為一些異常情況，程式沒有在正常時間啟動
                    if (IsAMMarket)
                    {
                        if (CreatedTime > _appCtrl.Settings.MarketStart[(int)Market.EDayNight.AM].AddMinutes(-2))
                        {
                            startDelayed = true;
                        }
                    }
                    else if (CreatedTime > _appCtrl.Settings.MarketStart[(int)Market.EDayNight.PM].AddMinutes(-2) || _appCtrl.Config.IsHoliday(CreatedTime) || CreatedTime.Hour < _appCtrl.Settings.MarketStart[(int)Market.EDayNight.AM].Hour)
                    {
                        startDelayed = true;
                    }
                    LoadedOnTime = !startDelayed;

                    if (IsAMMarket)
                    {
                        MarketStartTime = _appCtrl.Settings.MarketStart[(int)Market.EDayNight.AM];
                        MarketCloseTime = _appCtrl.Settings.MarketClose[(int)Market.EDayNight.AM];
                    }
                    else if (!_appCtrl.Config.IsHoliday(start))
                    {
                        if (CreatedTime.Hour < _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].Hour)
                        {
                            MarketStartTime = _appCtrl.Settings.MarketStart[(int)Market.EDayNight.PM].AddDays(-1);
                            MarketCloseTime = _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM];
                        }
                        else
                        {
                            MarketStartTime = _appCtrl.Settings.MarketStart[(int)Market.EDayNight.PM];
                            MarketCloseTime = _appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].AddDays(1);
                        }
                    }

                    if (!LoadedOnTime)
                    {
                        _appCtrl.LogWarn(start, $"沒有在開盤前執行登入動作", UniqueName);
                    }

                    _appCtrl.LogTrace(start, $"MarketStartTime={MarketStartTime:MM/dd HH:mm}|MarketCloseTime={MarketCloseTime:MM/dd HH:mm}", UniqueName);
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
                _appCtrl.CAPCenter.LogAPIMessage(start, result);

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
                        return _appCtrl.CAPCenter.LogAPIMessage(start, isConnected);
                }

                _appCtrl.LogTrace(start, result, UniqueName);

                _appCtrl.CAPCenter.IsConnected();

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
                        _appCtrl.CAPCenter.LogAPIMessage(start, m_nCode);
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
                    QuoteData quoteSub = this[quoteLast.Symbol];

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
            if (string.IsNullOrWhiteSpace(FileNameBase) && string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileClosePrefix))
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
                        if (files[i].Name.Contains(FileNameBase) && i > 0 && !files[i - 1].Name.Contains(FileNameBase))
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
                        if (files.Length > 0 && !files[files.Length - 1].Name.Contains(FileNameBase))
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
                _appCtrl.CAPCenter.LogAPIMessage(start, nCode);
            }

            return (nCode, pSKStockLONG);
        }

        public void GetProductInfo()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY)
                {
                    throw new ArgumentException($"Status != {StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY}|StatusStr={StatusStr}");
                }
                else if (_dataIndexMap.Count > 0)
                {
                    throw new ArgumentException($"_dataIndexMap.Count > 0|Count={_dataIndexMap.Count}|Quotes are subscribed.");
                }

                FileNameBase = string.Empty;

                List<QuoteData> optionList = new List<QuoteData>();

                foreach (string symbol in _appCtrl.Config.QuoteSubscribed)
                {
                    (int, SKSTOCKLONG) product = GetProductInfo(symbol, start);

                    if (product.Item1 != 0)
                    {
                        continue;
                    }

                    QuoteData quote = this[symbol];
                    quote = CreateOrUpdate(product.Item2, quote);

                    try
                    {
                        _dataRawMap.Add(quote.PrimaryKey, product.Item2);
                        _dataIndexMap.Add(quote.PrimaryKey, quote);
                        _dataSymbolMap.Add(symbol, quote);

                        if (quote.MarketGroupEnum == Market.EGroup.Option)
                        {
                            optionList.Add(quote);
                        }
                        else if (_dataCollection.FirstOrDefault(x => x.Symbol == quote.Symbol) == null)
                        {
                            _dataCollection.Add(quote);
                        }
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, $"{quote.ToCSVString()}{Environment.NewLine}{ex.StackTrace}");
                    }
                }

                foreach (QuoteData option in optionList)
                {
                    if (_dataCollection.FirstOrDefault(x => x.Symbol == option.Symbol) == null)
                    {
                        _dataCollection.Add(option);
                    }
                }

                if (Count > 0)
                {
                    int tradeDate = _dataCollection.Max(x => x.TradeDateRaw);

                    if (start.Hour >= 14 && tradeDate <= int.Parse(start.ToString("yyyyMMdd")) && !IsAMMarket)
                    {
                        FileNameBase = $"{tradeDate}_{(int)Market.EDayNight.AM}";
                        _appCtrl.LogWarn(start, $"未訂閱或尚未收到夜盤商品基本資料|FileNameBase={FileNameBase}", UniqueName);
                    }
                    else if (IsAMMarket)
                    {
                        FileNameBase = $"{tradeDate}_{(int)Market.EDayNight.AM}";
                    }
                    else
                    {
                        FileNameBase = $"{tradeDate}_{(int)Market.EDayNight.PM}";
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

        private void RecoverOpeningDataFromFile()
        {
            if (!IsAMMarket || string.IsNullOrWhiteSpace(FileNameBase) || string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileClosePrefix))
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Config.QuoteFolder.Refresh();
                FileInfo[] files = _appCtrl.Config.QuoteFolder.GetFiles($"{_appCtrl.Settings.QuoteFileClosePrefix}*.csv");
                FileInfo quoteFile = files.LastOrDefault(x => x.Name.Contains(FileNameBase));

                if (quoteFile == null)
                {
                    return;
                }

                _appCtrl.StartTrace(quoteFile.FullName, UniqueName);

                List<string> columnNames = new List<string>();

                foreach (QuoteData quoteLast in QuoteData.ForeachQuoteFromCSVFile(quoteFile.FullName, columnNames))
                {
                    QuoteData quoteSub = this[quoteLast.Symbol];

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

        public void SubscribeAsync()
        {
            Task.Factory.StartNew(() =>
            {
                DateTime start = _appCtrl.StartTrace();

                try
                {
                    bool isHoliday = _appCtrl.Config.IsHoliday(start);

                    RecoverOpeningDataFromFile();

                    foreach (QuoteData quote in _dataCollection)
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
                            _appCtrl.CAPCenter.LogAPIMessage(start, nCode);

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
                            _appCtrl.CAPCenter.LogAPIMessage(start, nCode);

                            //nCode=3030|SK_SUBJECT_NO_QUOTE_SUBSCRIBE|即時行情連線數已達上限，行情訂閱功能受限
                            _appCtrl.LogWarn(start, $"Status is changing.|before={Status}|after={nCode + StatusCode.BaseWarnValue}", UniqueName);
                            Status = nCode + StatusCode.BaseWarnValue;
                        }

                        if (qPage < 0)
                        {
                            _appCtrl.LogError(start, $"Sub quote failed.|requests={requests}|qPage={qPage}", UniqueName);
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

        public void RecoverDataAsync(string products)
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
                            _appCtrl.CAPCenter.LogAPIMessage(start, nCode);

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
                        _appCtrl.CAPCenter.LogAPIMessage(start, nCode);
                    }
                }
                else
                {
                    foreach (QuoteData quote in _dataCollection)
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

        public void SaveData(DirectoryInfo folder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (Count <= 0 || string.IsNullOrWhiteSpace(FileNameBase))
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace($"folder={folder?.Name}|append={append}|prefix={prefix}|suffix={suffix}", UniqueName);

            try
            {
                string path = Path.Combine(folder.FullName, $"{prefix}{FileNameBase}{suffix}.csv");
                bool exists = File.Exists(path);

                using (StreamWriter sw = new StreamWriter(path, append, TextEncoding.UTF8WithoutBOM))
                {
                    if (!append || !exists)
                    {
                        sw.WriteLine(QuoteData.CSVColumnNames);
                    }

                    if (quote == null)
                    {
                        foreach (QuoteData q in _dataCollection)
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

        public Task SaveDataAsync(DirectoryInfo quoteFolder, bool append = true, string prefix = "", string suffix = "", QuoteData quote = null)
        {
            if (Count <= 0 || string.IsNullOrWhiteSpace(FileNameBase))
            {
                return null;
            }

            return Task.Factory.StartNew(() => SaveData(quoteFolder, append, prefix, suffix, quote));
        }
    }
}
