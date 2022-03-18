using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly Dictionary<string, string> PropertyDescriptionMap = typeof(QuoteData).GetPropertyDescriptionMap(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        private string _creator;
        [Description("建立者")]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Description("日期")]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Description("時間")]
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
        [Description("更新者")]
        public string Updater
        {
            get { return _updater; }
            set { OnPropertyChanged(ref _updater, value); }
        }

        [Description("更新日")]
        public DateTime UpdateDate => UpdateTime.Date;

        private DateTime _updateTime;
        [Description("更新時")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set
            {
                OnPropertyChanged(ref _updateTime, value);
                OnPropertyChanged("UpdateDate");
            }
        }

        private string _symbol;
        [Description("代碼")]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private string _name;
        [Description("名稱")]
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value); }
        }

        private string _packetTimeRaw;
        [Description("封包時間raw")]
        public string PacketTimeRaw
        {
            get { return _packetTimeRaw; }
            set
            {
                OnPropertyChanged(ref _packetTimeRaw, value);
                OnPropertyChanged("PacketTime");
            }
        }

        [Description("封包時間")]
        public DateTime PacketTime
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PacketTimeRaw))
                {
                    return DateTime.MinValue;
                }

                try
                {
                    return DateTime.ParseExact(PacketTimeRaw.ToString(), "HHmmss.ffffff", CultureInfo.InvariantCulture);
                }
                catch
                { }

                return DateTime.MinValue;
            }
        }

        private decimal _dealPrice;
        /// <summary>
        /// 成交價
        /// </summary>
        [Description("成價")]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertyChanged(ref _dealPrice, value); }
        }

        private int _dealQty;
        /// <summary>
        /// 成交量
        /// </summary>
        [Description("成量")]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        private decimal _upDown;
        [Description("漲跌")]
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
        [Description("漲跌幅")]
        public decimal UpDownPct
        {
            get { return _upDownPct; }
            set { OnPropertyChanged(ref _upDownPct, value); }
        }

        private decimal _bestBuyPrice;
        [Description("買價")]
        public decimal BestBuyPrice
        {
            get { return _bestBuyPrice; }
            set { OnPropertyChanged(ref _bestBuyPrice, value); }
        }

        private int _bestBuyQty;
        [Description("買量")]
        public int BestBuyQty
        {
            get { return _bestBuyQty; }
            set { OnPropertyChanged(ref _bestBuyQty, value); }
        }

        private decimal _bestSellPrice;
        [Description("賣價")]
        public decimal BestSellPrice
        {
            get { return _bestSellPrice; }
            set { OnPropertyChanged(ref _bestSellPrice, value); }
        }

        private int _bestSellyQty;
        [Description("賣量")]
        public int BestSellQty
        {
            get { return _bestSellyQty; }
            set { OnPropertyChanged(ref _bestSellyQty, value); }
        }

        private decimal _openPrice;
        [Description("開盤價")]
        public decimal OpenPrice
        {
            get { return _openPrice; }
            set { OnPropertyChanged(ref _openPrice, value); }
        }

        private decimal _highPrice;
        [Description("最高價")]
        public decimal HighPrice
        {
            get { return _highPrice; }
            set { OnPropertyChanged(ref _highPrice, value); }
        }

        private decimal _lowPrice;
        [Description("最低價")]
        public decimal LowPrice
        {
            get { return _lowPrice; }
            set { OnPropertyChanged(ref _lowPrice, value); }
        }

        private decimal _reference;
        [Description("參考價")]
        public decimal Reference
        {
            get { return _reference; }
            set { OnPropertyChanged(ref _reference, value); }
        }

        private int _simulate;
        /// <summary>
        /// 試撮
        /// </summary>
        [Description("試")]
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
        [Description("總量")]
        public int TotalQty
        {
            get { return _totalQty; }
            set { OnPropertyChanged(ref _totalQty, value); }
        }

        private int _tradeDateRaw;
        [Description("交易日raw")]
        public int TradeDateRaw
        {
            get { return _tradeDateRaw; }
            set
            {
                OnPropertyChanged(ref _tradeDateRaw, value);
                OnPropertyChanged("TradeDate");
            }
        }

        [Description("交易日")]
        public DateTime TradeDate
        {
            get
            {
                if (TradeDateRaw <= 0)
                {
                    return DateTime.MinValue;
                }

                try
                {
                    return DateTime.ParseExact(TradeDateRaw.ToString().PadLeft(8, '0'), "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                catch
                { }

                return DateTime.MinValue;
            }
        }

        private decimal _highPriceLimit;
        [Description("漲停")]
        public decimal HighPriceLimit
        {
            get { return _highPriceLimit; }
            set { OnPropertyChanged(ref _highPriceLimit, value); }
        }

        private decimal _lowPriceLimit;
        [Description("跌停")]
        public decimal LowPriceLimit
        {
            get { return _lowPriceLimit; }
            set { OnPropertyChanged(ref _lowPriceLimit, value); }
        }

        private int _count;
        [Description("筆數")]
        public int Count
        {
            get { return _count; }
            set { OnPropertyChanged(ref _count, value); }
        }

        private int _index;
        [Description("索引")]
        public int Index
        {
            get { return _index; }
            set { OnPropertyChanged(ref _index, value); }
        }

        private short _page;
        /// <summary>
        /// Page
        /// </summary>
        [Description("P")]
        public short Page
        {
            get { return _page; }
            set { OnPropertyChanged(ref _page, value); }
        }

        private short _market;
        /// <summary>
        /// 市場
        /// </summary>
        [Description("市")]
        public short Market
        {
            get { return _market; }
            set { OnPropertyChanged(ref _market, value); }
        }

        private short _decimalPos;
        /// <summary>
        /// 小數
        /// </summary>
        [Description("D")]
        public short DecimalPos
        {
            get { return _decimalPos; }
            set { OnPropertyChanged(ref _decimalPos, value); }
        }

        private int _totalQtyBefore;
        [Description("昨量")]
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
            UpdateTime = DateTime.MinValue;
            Symbol = String.Empty;
            Name = String.Empty;
            PacketTimeRaw = String.Empty;
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
    }
}
