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
        /// 例行性檢查
        /// </summary>
        public int TimerInterval1 { get; set; }
        /// <summary>
        /// 檢查行情報價斷線重連
        /// </summary>
        public int TimerInterval2 { get; set; }

        /// <summary>
        /// 排程啟動自動執行
        /// </summary>
        public bool AutoRun { get; set; }
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
        /// 定時儲存行情報價
        /// </summary>
        public string QuoteFolderPath { get; set; }

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
            Version = "0.22.322.1";
            Description = "測試用設定";

            Big5EncodingCodePage = 950; //"big5"

            HolidayFilePath = "holidaySchedule_{yyy}.csv";
            HolidayFileKeywords1 = new List<string>();
            //HolidayFileKeywords1 = new List<string>() { "月", "日" }; //
            HolidayFileKeywords2 = new List<string>();
            //HolidayFileKeywords2 = new List<string>() { "放假", "無交易", "補假" }; //

            DataGridAppLogRowsMax = 500;

            TimerInterval1 = 300;
            TimerInterval2 = 35 * 1000;

            AutoRun = true;
            TimeToExit = new List<DateTime>();
            //TimeToExit = new List<DateTime>() //
            //{
            //    DateTime.ParseExact("05:03", "HH:mm", CultureInfo.InvariantCulture),
            //    DateTime.ParseExact("13:48", "HH:mm", CultureInfo.InvariantCulture),
            //};

            QuoteMarkets = new List<int>();
            //QuoteMarkets = new List<int>() { 0, 1, 2, 4 };
            QuoteRequest = new List<string>();
            //QuoteRequest = new List<string>() { "TSEA", "OTCA", "0050", "00632R", "0056", "2330" }; //
            QuoteLive = new List<string>();
            //QuoteLive = new List<string>() { "UD06", "UN2206", "TX04", "TX05", "MTX04", "MTX05" }; //
            QuoteFolderPath = "QuoteData";

            SendOrder = false;
            OrderAndDeal = false;
        }
    }
}
