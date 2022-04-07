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
        public string Version { get; set; }
        public string Description { get; set; }

        public int Big5EncodingCodePage { get; set; }

        public string HolidayFilePath { get; set; }
        public List<string> HolidayFileKeywords1 { get; set; }
        public List<string> HolidayFileKeywords2 { get; set; }

        public int DataGridAppLogRowsMax { get; set; }

        /// <summary>
        /// 與UI無關，背景執行的Timer
        /// </summary>
        public int TimerIntervalBackground { get; set; }
        /// <summary>
        /// 檢查Trigger，對效能要求較高，另開Timer獨立執行
        /// </summary>
        public int TimerIntervalTrigger { get; set; }
        /// <summary>
        /// 監控UI
        /// </summary>
        public int TimerIntervalUI1 { get; set; }
        /// <summary>
        /// 檢查行情報價斷線重連
        /// </summary>
        public int TimerIntervalUI2 { get; set; }

        /// <summary>
        /// 排程啟動自動執行
        /// </summary>
        public bool AutoRun { get; set; }
        /// <summary>
        /// 在台指期日盤夜盤開盤前啟動程式
        /// </summary>
        public List<DateTime> TimeToStart { get; set; }
        /// <summary>
        /// 在台指期日盤夜盤收盤後關閉程式
        /// </summary>
        public List<DateTime> TimeToExit { get; set; }

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
        public string TriggerFileFormat { get; set; }

        /// <summary>
        /// false=測試或跑回測時，不實際下單
        /// </summary>
        public bool SendOrder { get; set; }
        /// <summary>
        /// false=下單測試委託回報，不成交
        /// </summary>
        public bool OrderAndDeal { get; set; }

        public AppSettings()
        {
            Version = "0.22.407.3";
            Description = "測試用設定";

            Big5EncodingCodePage = 950; //"big5"

            HolidayFilePath = "holidaySchedule_{yyy}.csv";
            HolidayFileKeywords1 = new List<string>();
            //HolidayFileKeywords1 = new List<string>() { "月", "日" }; //
            HolidayFileKeywords2 = new List<string>();
            //HolidayFileKeywords2 = new List<string>() { "放假", "無交易", "補假" }; //

            DataGridAppLogRowsMax = 500;

            TimerIntervalBackground = 900;
            TimerIntervalTrigger = 30;
            TimerIntervalUI1 = 200;
            TimerIntervalUI2 = 35 * 1000;

            AutoRun = true;
            TimeToStart = new List<DateTime>();
            //TimeToStart = new List<DateTime>() //
            //{
            //    DateTime.ParseExact("08:43", "HH:mm", CultureInfo.InvariantCulture),
            //    DateTime.ParseExact("14:58", "HH:mm", CultureInfo.InvariantCulture),
            //};
            TimeToExit = new List<DateTime>();
            //TimeToExit = new List<DateTime>() //
            //{
            //    DateTime.ParseExact("05:03", "HH:mm", CultureInfo.InvariantCulture),
            //    DateTime.ParseExact("13:48", "HH:mm", CultureInfo.InvariantCulture),
            //};

            QuoteMarkets = new List<int>();
            //QuoteMarkets = new List<int>() { Definition.MarketTSE, Definition.MarketOTC, Definition.MarketFutures, Definition.MarketEmerging };
            QuoteRequest = new List<string>();
            //QuoteRequest = new List<string>() { "TSEA", "0050", "00632R", "TX06", "MTX06" }; //
            QuoteLive = new List<string>();
            //QuoteLive = new List<string>() { "2330", "UD06", "UN2206", "TX05", "MTX05" }; //

            QuoteFolderPath = "QuoteData";
            QuoteFileClosePrefix = "Last_";
            QuoteFileRecoverPrefix = "Recover_";
            QuoteSaveInterval = 45;

            OrderMaxQty = 50;
            OrderMaxCount = 10;

            TriggerFolderPath = "TriggerData";
            TriggerFileFormat = "MMdd_HHmm";

            SendOrder = false;
            OrderAndDeal = false;
        }
    }
}
