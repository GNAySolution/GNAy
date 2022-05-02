using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class AppConfig
    {
        public readonly DateTime CreatedTime;

        public readonly AppSettings Settings;
        public readonly Version Version;

        public readonly Encoding Big5Encoding;

        public readonly SortedDictionary<DateTime, string> Holidays;

        public readonly bool AutoRun;

        public readonly HashSet<string> QuoteSubscribed;
        public readonly DirectoryInfo QuoteFolder;

        public readonly DirectoryInfo TriggerFolder;
        public readonly DirectoryInfo StrategyFolder;
        public readonly DirectoryInfo SentOrderFolder;

        public readonly FileInfo Archive;

        public AppConfig(AppSettings settings, FileInfo archive)
        {
            CreatedTime = DateTime.Now;

            if (archive == null && settings == null)
            {
                settings = new AppSettings();
            }

            Settings = settings;
            Version = new Version(settings.Version);

            Big5Encoding = Encoding.GetEncoding(settings.Big5EncodingCodePage);

            Holidays = new SortedDictionary<DateTime, string>();
            DateTime today = DateTime.Today;
            string holidayPathThisYear = settings.HolidayFilePath.ToROCYear(today);
            Holidays.LoadHolidays(holidayPathThisYear, Big5Encoding, today.Year, settings.HolidayFileKeywords1, settings.HolidayFileKeywords2);
            string holidayPathLastYear = settings.HolidayFilePath.ToROCYear(today.AddYears(-1));
            Holidays.LoadHolidays(holidayPathLastYear, Big5Encoding, today.AddYears(-1).Year, settings.HolidayFileKeywords1, settings.HolidayFileKeywords2);

            AutoRun = IsHoliday(CreatedTime) ? settings.AutoRunInHoliday : settings.AutoRunInTradeDay;

            QuoteSubscribed = new HashSet<string>();
            foreach (string product in settings.QuoteRequest)
            {
                QuoteSubscribed.Add(product.Trim());
            }
            foreach (string product in settings.QuoteLive)
            {
                QuoteSubscribed.Add(product.Trim());
            }

            QuoteFolder = null;
            if (!string.IsNullOrWhiteSpace(settings.QuoteFolderPath))
            {
                QuoteFolder = new DirectoryInfo(settings.QuoteFolderPath);
                QuoteFolder.Create();
                QuoteFolder.Refresh();
            }

            TriggerFolder = null;
            if (!string.IsNullOrWhiteSpace(settings.TriggerFolderPath))
            {
                TriggerFolder = new DirectoryInfo(settings.TriggerFolderPath);
                TriggerFolder.Create();
                TriggerFolder.Refresh();
            }

            StrategyFolder = null;
            if (!string.IsNullOrWhiteSpace(settings.StrategyFolderPath))
            {
                StrategyFolder = new DirectoryInfo(settings.StrategyFolderPath);
                StrategyFolder.Create();
                StrategyFolder.Refresh();
            }

            SentOrderFolder = null;
            if (!string.IsNullOrWhiteSpace(settings.SentOrderFolderPath))
            {
                SentOrderFolder = new DirectoryInfo(settings.SentOrderFolderPath);
                SentOrderFolder.Create();
                SentOrderFolder.Refresh();
            }

            Archive = archive;
        }

        public AppConfig() : this(null, null)
        {
            //
        }

        /// <summary>
        /// 考慮期貨夜盤跨日
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsHoliday(DateTime time)
        {
            if (time.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            DateTime date = time.Date;

            if (time.DayOfWeek == DayOfWeek.Saturday)
            { }
            else if (!Holidays.ContainsKey(date))
            {
                return false;
            }

            return Holidays.ContainsKey(date.AddDays(-1)) || time.Hour >= 5; //夜盤跨日到早上5點

            //DateTime beforeDate = date.AddDays(-1);

            //if (Holidays.ContainsKey(beforeDate))
            //{
            //    return true;
            //}

            //return time.Hour >= 5;
        }

        /// <summary>
        /// 考慮期貨日夜盤
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsAMMarket(DateTime time)
        {
            return !IsHoliday(time) && time.Hour >= 8 && time.Hour < 14;

            //bool result = false;

            //if (!IsHoliday(time) && time.Hour >= 8 && time.Hour < 14)
            //{
            //    result = true;
            //}

            //return result;
        }
    }
}
