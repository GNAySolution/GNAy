using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class QuoteData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(QuoteData).GetColumnAttrMapByProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(QuoteData).GetColumnAttrMapByIndex(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(QuoteData).GetColumnAttrMapByIndex(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

        private string _creator;
        [Column("建立者", 0)]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Column("日期", -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", 1, "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set
            {
                OnPropertyChanged(ref _createdTime, value);
                OnPropertyChanged("CreatedDate");
            }
        }

        private string _updater;
        [Column("更新者", 2)]
        public string Updater
        {
            get { return _updater; }
            set { OnPropertyChanged(ref _updater, value); }
        }

        [Column("更新日", -1)]
        public DateTime UpdateDate => UpdateTime.Date;

        private DateTime _updateTime;
        [Column("更新時", 3, "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set
            {
                OnPropertyChanged(ref _updateTime, value);
                OnPropertyChanged("UpdateDate");
                OnPropertyChanged("Elapsed");
            }
        }

        [Column("經過", 4)]
        public string Elapsed => ((UpdateTime == DateTime.MaxValue) ? TimeSpan.MaxValue : (DateTime.Now - UpdateTime)).ToString(@"hh\:mm\:ss");

        private string _symbol;
        [Column("代碼", 5)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private string _name;
        [Column("名稱", 6)]
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value); }
        }

        private string _matchedTimeRaw;
        [Column("成交時間", 7)]
        public string MatchedTimeRaw
        {
            get { return _matchedTimeRaw; }
            set
            {
                OnPropertyChanged(ref _matchedTimeRaw, value);
                OnPropertyChanged("MatchedTime");
            }
        }

        [Column("成交時間", -1)]
        public DateTime MatchedTime => DateTime.ParseExact(MatchedTimeRaw.ToString(), "HHmmss.ffffff", CultureInfo.InvariantCulture);

        private decimal _dealPrice;
        [Column("成交價", "成價", 8, "0.00")]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertyChanged(ref _dealPrice, value); }
        }

        private int _dealQty;
        [Column("成交量", "成量", 9)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        private decimal _upDown;
        [Column("漲跌", 10, "0.00")]
        public decimal UpDown
        {
            get { return _upDown; }
            set
            {
                OnPropertyChanged(ref _upDown, value);
                OnPropertyChanged("RowBackground");
            }
        }

        private decimal _upDownPct;
        [Column("漲跌幅", 11, "0.00")]
        public decimal UpDownPct
        {
            get { return _upDownPct; }
            set { OnPropertyChanged(ref _upDownPct, value); }
        }

        private decimal _bestBuyPrice;
        [Column("買價", 12, "0.00")]
        public decimal BestBuyPrice
        {
            get { return _bestBuyPrice; }
            set { OnPropertyChanged(ref _bestBuyPrice, value); }
        }

        private int _bestBuyQty;
        [Column("買量", 13)]
        public int BestBuyQty
        {
            get { return _bestBuyQty; }
            set { OnPropertyChanged(ref _bestBuyQty, value); }
        }

        private decimal _bestSellPrice;
        [Column("賣價", 14, "0.00")]
        public decimal BestSellPrice
        {
            get { return _bestSellPrice; }
            set { OnPropertyChanged(ref _bestSellPrice, value); }
        }

        private int _bestSellyQty;
        [Column("賣量", 15)]
        public int BestSellQty
        {
            get { return _bestSellyQty; }
            set { OnPropertyChanged(ref _bestSellyQty, value); }
        }

        private decimal _openPrice;
        [Column("開盤價", 16, "0.00")]
        public decimal OpenPrice
        {
            get { return _openPrice; }
            set { OnPropertyChanged(ref _openPrice, value); }
        }

        private decimal _highPrice;
        [Column("最高價", 17, "0.00")]
        public decimal HighPrice
        {
            get { return _highPrice; }
            set { OnPropertyChanged(ref _highPrice, value); }
        }

        private decimal _lowPrice;
        [Column("最低價", 18, "0.00")]
        public decimal LowPrice
        {
            get { return _lowPrice; }
            set { OnPropertyChanged(ref _lowPrice, value); }
        }

        private decimal _reference;
        [Column("參考價", 19, "0.00")]
        public decimal Reference
        {
            get { return _reference; }
            set { OnPropertyChanged(ref _reference, value); }
        }

        private int _simulate;
        [Column("試撮", "試", 20)]
        public int Simulate
        {
            get { return _simulate; }
            set
            {
                OnPropertyChanged(ref _simulate, value);
                OnPropertyChanged("RowBackground");
            }
        }

        private int _totalQty;
        [Column("總量", 21)]
        public int TotalQty
        {
            get { return _totalQty; }
            set { OnPropertyChanged(ref _totalQty, value); }
        }

        private int _tradeDateRaw;
        [Column("交易日", 22)]
        public int TradeDateRaw
        {
            get { return _tradeDateRaw; }
            set
            {
                OnPropertyChanged(ref _tradeDateRaw, value);
                OnPropertyChanged("TradeDate");
            }
        }

        [Column("交易日", -1)]
        public DateTime TradeDate => (TradeDateRaw <= 0) ? DateTime.MaxValue.Date : DateTime.ParseExact(TradeDateRaw.ToString().PadLeft(8, '0'), "yyyyMMdd", CultureInfo.InvariantCulture);

        private decimal _highPriceLimit;
        [Column("漲停", 23, "0.00")]
        public decimal HighPriceLimit
        {
            get { return _highPriceLimit; }
            set { OnPropertyChanged(ref _highPriceLimit, value); }
        }

        private decimal _lowPriceLimit;
        [Column("跌停", 24, "0.00")]
        public decimal LowPriceLimit
        {
            get { return _lowPriceLimit; }
            set { OnPropertyChanged(ref _lowPriceLimit, value); }
        }

        private int _count;
        [Column("筆數", -1)]
        public int Count
        {
            get { return _count; }
            set { OnPropertyChanged(ref _count, value); }
        }

        private int _index;
        [Column("索引", 25)]
        public int Index
        {
            get { return _index; }
            set { OnPropertyChanged(ref _index, value); }
        }

        private short _page;
        [Column("Page", "P", 26)]
        public short Page
        {
            get { return _page; }
            set { OnPropertyChanged(ref _page, value); }
        }

        private short _market;
        [Column("市場", "市", 27)]
        public short Market
        {
            get { return _market; }
            set { OnPropertyChanged(ref _market, value); }
        }

        private short _decimalPos;
        [Column("小數位數", "D", 28)]
        public short DecimalPos
        {
            get { return _decimalPos; }
            set { OnPropertyChanged(ref _decimalPos, value); }
        }

        private int _totalQtyBefore;
        [Column("昨量", 29)]
        public int TotalQtyBefore
        {
            get { return _totalQtyBefore; }
            set { OnPropertyChanged(ref _totalQtyBefore, value); }
        }

        public int RowBackground
        {
            get
            {
                if (Simulate == 0)
                {
                    return UpDown > 0 ? 1 : 0;
                }

                return UpDown > 0 ? 3 : 2;
            }
        }

        public QuoteData()
        {
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Updater = String.Empty;
            UpdateTime = DateTime.MaxValue;
            Symbol = String.Empty;
            Name = String.Empty;
            MatchedTimeRaw = $"{DateTime.MinValue:HHmmss.ffffff}";
            DealPrice = 0;
            DealQty = 0;
            UpDown = 0;
            UpDownPct = 0;
            BestBuyPrice = 0;
            BestBuyQty = 0;
            BestSellPrice = 0;
            BestSellQty = 0;
            OpenPrice = 0;
            HighPrice = 0;
            LowPrice = 0;
            Reference = 0;
            Simulate = -1;
            TotalQty = 0;
            TradeDateRaw = 0;
            HighPriceLimit = 0;
            LowPriceLimit = 0;
            Count = 0;
            Index = -1;
            Page = -1;
            Market = -1;
            DecimalPos = 0;
            TotalQtyBefore = 0;
        }

        public string ToCSVString()
        {
            //string result = string.Join(",",
            //    Creator,
            //    $"{CreatedTime:yyyy/MM/dd HH:mm:ss.ffffff}",
            //    Updater,
            //    $"{UpdateTime:yyyy/MM/dd HH:mm:ss.ffffff}",
            //    Elapsed,
            //    Symbol,
            //    Name,
            //    MatchedTimeRaw,
            //    $"{DealPrice:0.00}",
            //    $"{DealQty}",
            //    $"{UpDown:0.00}",
            //    $"{UpDownPct:0.00}",
            //    $"{BestBuyPrice:0.00}",
            //    $"{BestBuyQty}",
            //    $"{BestSellPrice:0.00}",
            //    $"{BestSellQty}",
            //    $"{OpenPrice:0.00}",
            //    $"{HighPrice:0.00}",
            //    $"{LowPrice:0.00}",
            //    $"{Reference:0.00}",
            //    $"{Simulate}",
            //    $"{TotalQty}",
            //    $"{TradeDateRaw}",
            //    $"{HighPriceLimit:0.00}",
            //    $"{LowPriceLimit:0.00}",
            //    $"{Index}",
            //    $"{Page}",
            //    $"{Market}",
            //    $"{DecimalPos}",
            //    $"{TotalQtyBefore}"
            //    );

            string result = string.Join("\",\"", ColumnGetters.Values.Select(x => x.Item2.PropertyValueToString(this, x.Item1.StringFormat)));
            return $"\"{result}\"";
        }
    }
}
