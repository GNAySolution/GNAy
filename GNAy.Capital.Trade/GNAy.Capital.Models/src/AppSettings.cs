using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    /// <summary>
    /// 設定檔
    /// </summary>
    [Serializable]
    public class AppSettings
    {
        public const string Keyword_Holiday = "{Holiday}";
        public const string Keyword_DayNight = "{DayNight}";

        public string Version { get; set; }
        public string Description { get; set; }

        public string ProcessPriority { get; set; }
        public int Big5EncodingCodePage { get; set; }

        public string ScreenshotFolderPath { get; set; }
        public bool ShowDataGrid { get; set; }
        public int DataGridAppLogRowsMax { get; set; }

        /// <summary>
        /// 與UI無關，背景執行的Timer
        /// </summary>
        public int TimerIntervalBackground { get; set; }
        /// <summary>
        /// 監控UI
        /// </summary>
        public int TimerIntervalUI1 { get; set; }
        /// <summary>
        /// 檢查行情報價斷線重連
        /// </summary>
        public int TimerIntervalUI2 { get; set; }

        public string HolidayFilePath { get; set; }
        public List<string> HolidayFileKeywords1 { get; set; }
        public List<string> HolidayFileKeywords2 { get; set; }

        /// <summary>
        /// 排程啟動自動執行
        /// </summary>
        public bool AutoRunInTradeDay { get; set; }
        /// <summary>
        /// 排程啟動自動執行
        /// </summary>
        public bool AutoRunInHoliday { get; set; }

        /// <summary>
        /// 在台指期日盤夜盤開盤前啟動程式
        /// </summary>
        public List<DateTime> MarketStart { get; set; }
        /// <summary>
        /// 台指期日盤夜盤收盤
        /// </summary>
        public List<DateTime> MarketClose { get; set; }

        /// <summary>
        /// 期貨近月到期第幾星期
        /// </summary>
        public int FuturesLastTradeWeek { get; set; }
        /// <summary>
        /// 期貨近月到期第幾天
        /// </summary>
        public string FuturesLastTradeDay { get; set; }
        /// <summary>
        /// 期貨近月到期前幾天換月
        /// </summary>
        public int DayToChangeFutures { get; set; }

        /// <summary>
        /// 上市 0、上櫃 1、期貨 2、選擇權 3、興櫃 4、盤中零股-上市5、盤中零股-上櫃6
        /// </summary>
        public List<int> QuoteMarkets { get; set; }
        /// <summary>
        /// <para>訂閱行情報價</para>
        /// <para>TSEA,6005,TX00,TX03,TX04,MTX00,MTX03,MTX04</para>
        /// <para>上市加權指數,群益證,台指期近月,台指期3月,台指期4月,小台期近月,小台期3月,小台期4月</para>
        /// <para>SKQuoteLib_RequestStocks</para>
        /// </summary>
        public List<string> QuoteRequest { get; set; }
        /// <summary>
        /// SKQuoteLib_RequestLiveTick
        /// </summary>
        public List<string> QuoteLive { get; set; }

        /// <summary>
        /// 儲存報價到資料夾
        /// </summary>
        public string QuoteFolderPath { get; set; }
        /// <summary>
        /// 開盤報價檔名前綴
        /// </summary>
        public string QuoteFileOpenPrefix { get; set; }
        /// <summary>
        /// 收盤報價檔名前綴
        /// </summary>
        public string QuoteFileClosePrefix { get; set; }
        /// <summary>
        /// 回補報價檔名前綴
        /// </summary>
        public string QuoteFileRecoverPrefix { get; set; }
        /// <summary>
        /// 間隔幾秒備份報價資料
        /// </summary>
        public int QuoteSaveInterval { get; set; }

        /// <summary>
        /// 查詢期貨未平倉的間隔秒
        /// </summary>
        public int OpenInterestInterval { get; set; }
        /// <summary>
        /// 查詢期貨權益數的間隔秒
        /// </summary>
        public int FuturesRightsInterval { get; set; }

        /// <summary>
        /// 委託單的間隔毫秒
        /// </summary>
        public int OrderTimeInterval { get; set; }
        /// <summary>
        /// 設定每秒委託「量」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單
        /// </summary>
        public int OrderMaxQty { get; set; }
        /// <summary>
        /// 設定每秒委託「筆數」限制。一秒內下單超過設定值時下該類型下單將被鎖定，需進行解鎖才可繼續下單
        /// </summary>
        public int OrderMaxCount { get; set; }

        /// <summary>
        /// 觸價資料夾
        /// </summary>
        public string TriggerFolderPath { get; set; }
        public string TriggerFileLoadFormat { get; set; }
        public string TriggerFileSaveFormat { get; set; }

        /// <summary>
        /// 策略資料夾
        /// </summary>
        public string StrategyFolderPath { get; set; }
        public string StrategyFileLoadFormat { get; set; }
        public string StrategyFileSaveFormat { get; set; }

        /// <summary>
        /// 送出的委託單，不論是否有收到委回成回
        /// </summary>
        public string SentOrderFolderPath { get; set; }
        public string SentOrderFileFormat { get; set; }

        /// <summary>
        /// false=測試或跑回測，不實際下單
        /// </summary>
        public bool SendRealOrder { get; set; }

        /// <summary>
        /// 直播模式，隱藏隱私資料，只呈現損益，方便使用者實況(或錄影)播放自己的交易過程
        /// </summary>
        public bool LiveMode { get; set; }

        /// <summary>
        /// 程式啟動後，利用庫存判斷要啟動的策略
        /// </summary>
        public bool StartFromOpenInterest { get; set; }

        public AppSettings()
        {
            Version = "1.22.617.1";
            Description = "測試用設定";

            //ProcessPriority = 0x80; //ProcessPriorityClass.High
            ProcessPriority = "Normal"; //0x20; //ProcessPriorityClass.Normal
            Big5EncodingCodePage = 950; //"big5"

            ScreenshotFolderPath = "Screenshot";
            ShowDataGrid = true;
            DataGridAppLogRowsMax = 500;

            TimerIntervalBackground = 50;
            TimerIntervalUI1 = 300;
            TimerIntervalUI2 = 35 * 1000;

            HolidayFilePath = "holidaySchedule_{yyy}.csv";
            HolidayFileKeywords1 = new List<string>();
            //HolidayFileKeywords1 = new List<string>() { "月", "日" }; //
            HolidayFileKeywords2 = new List<string>();
            //HolidayFileKeywords2 = new List<string>() { "放假", "無交易", "補假" }; //

            AutoRunInTradeDay = true;
            AutoRunInHoliday = false;

            MarketStart = new List<DateTime>();
            //MarketStart = new List<DateTime>() //
            //{
            //    DateTime.ParseExact("15:00", "HH:mm", CultureInfo.InvariantCulture),
            //    DateTime.ParseExact("08:45", "HH:mm", CultureInfo.InvariantCulture),
            //};
            MarketClose = new List<DateTime>();
            //MarketClose = new List<DateTime>() //
            //{
            //    DateTime.ParseExact("05:00:00", "HH:mm:ss", CultureInfo.InvariantCulture),
            //    DateTime.ParseExact("13:45:00", "HH:mm:ss", CultureInfo.InvariantCulture),
            //};

            FuturesLastTradeWeek = 3;
            FuturesLastTradeDay = "Wednesday"; //"wed" //"3"
            DayToChangeFutures = -1;

            QuoteMarkets = new List<int>();
            //QuoteMarkets = new List<int>() { (int)Market.EGroup.TSE, (int)Market.EGroup.OTC, (int)Market.EGroup.Futures, (int)Market.EGroup.Emerging }; //
            QuoteRequest = new List<string>();
            //QuoteRequest = new List<string>() { "TSEA", "0050", "00632R", "UN2206", "TX06", "MTX06", "TXO16500Q2", "TXO16500E2", "TXO16600Q2", "TXO16600E2", "TXO16700Q2", "TXO16700E2", "TXO16800Q2", "TXO16800E2", "TXO16900Q2", "TXO16900E2" }; //
            QuoteLive = new List<string>();
            //QuoteLive = new List<string>() { "2330", "UD06", "TX05", "MTX05" }; //

            QuoteFolderPath = "QuoteData";
            QuoteFileOpenPrefix = "Open_";
            QuoteFileClosePrefix = "Last_";
            QuoteFileRecoverPrefix = "Recover_";
            QuoteSaveInterval = 45;

            OpenInterestInterval = 8;
            FuturesRightsInterval = 28;

            OrderTimeInterval = 100;
            OrderMaxQty = 50;
            OrderMaxCount = 50;

            TriggerFolderPath = "TriggerData";
            TriggerFileLoadFormat = $"T*{Keyword_DayNight}.csv";
            TriggerFileSaveFormat = "MMdd_HHmm";

            StrategyFolderPath = "StrategyData";
            StrategyFileLoadFormat = $"T*{Keyword_Holiday}_{Keyword_DayNight}.csv";
            StrategyFileSaveFormat = "MMdd_HHmm";

            SentOrderFolderPath = "SentOrder";
            SentOrderFileFormat = "yyMMdd_HHmmss_ffffff";

            SendRealOrder = false;
            LiveMode = false;

            StartFromOpenInterest = false;
        }
    }
}
