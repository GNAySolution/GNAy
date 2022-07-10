using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class OpenInterestData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(OpenInterestData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        private string _creator;
        [Column("建立者", CSVIndex = -1)]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Column("日期", CSVIndex = -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", CSVIndex = -1)]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        private string _updater;
        [Column("更新者", WPFDisplayIndex = 0)]
        public string Updater
        {
            get { return _updater; }
            set { OnPropertyChanged(ref _updater, value); }
        }

        [Column("更新日", CSVIndex = -1)]
        public DateTime UpdateDate => UpdateTime.Date;

        private DateTime _updateTime;
        [Column("更新時", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 1, WPFStringFormat = "{0:HH:mm:ss.fff}")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { OnPropertiesChanged(ref _updateTime, value, nameof(UpdateTime), nameof(UpdateDate)); }
        }

        private string _strategy;
        [Column("策略唯一鍵", "策略", WPFDisplayIndex = 2, WPFForeground = "MediumBlue")]
        public string Strategy
        {
            get { return _strategy; }
            set { OnPropertyChanged(ref _strategy, value); }
        }

        private Market.EType _marketType;
        [Column("市場", CSVIndex = -1)]
        public Market.EType MarketType
        {
            get { return _marketType; }
            set { OnPropertiesChanged(ref _marketType, value, nameof(MarketType), nameof(MarketName)); }
        }
        [Column("市場", CSVIndex = -1, WPFDisplayIndex = 3)]
        public string MarketName => Market.NameDescription[(int)MarketType];

        private string _account;
        [Column("下單帳號", WPFDisplayIndex = 4)]
        public string Account
        {
            get { return _account; }
            set { OnPropertyChanged(ref _account, value); }
        }

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", WPFDisplayIndex = 5, WPFForeground = "MediumBlue")]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private OrderBS.Enum _bsEnum;
        [Column("買賣", WPFDisplayIndex = 6)]
        public OrderBS.Enum BSEnum
        {
            get { return _bsEnum; }
            set { OnPropertyChanged(ref _bsEnum, value); }
        }

        private OrderDayTrade.Enum _dayTradeEnum;
        [Column("當沖", WPFDisplayIndex = 7)]
        public OrderDayTrade.Enum DayTradeEnum
        {
            get { return _dayTradeEnum; }
            set { OnPropertyChanged(ref _dayTradeEnum, value); }
        }

        public string PrimaryKey => $"{Account}_{Symbol}_{BSEnum}_{DayTradeEnum}";

        private OrderPosition.Enum _positionEnum;
        [Column("新平", WPFDisplayIndex = 8)]
        public OrderPosition.Enum PositionEnum
        {
            get { return _positionEnum; }
            set { OnPropertyChanged(ref _positionEnum, value); }
        }

        private decimal _marketPrice;
        [Column("平倉前的市場價格", "市場價格", CSVStringFormat = "0.00", WPFDisplayIndex = 9, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal MarketPrice
        {
            get { return _marketPrice; }
            set { OnPropertyChanged(ref _marketPrice, value); }
        }

        private decimal _averagePrice;
        [Column("成交均價", CSVStringFormat = "0.00", WPFDisplayIndex = 10, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal AveragePrice
        {
            get { return _averagePrice; }
            set { OnPropertyChanged(ref _averagePrice, value); }
        }

        private int _quantity;
        [Column("成交量", "成量", WPFDisplayIndex = 11, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int Quantity
        {
            get { return _quantity; }
            set { OnPropertyChanged(ref _quantity, value); }
        }

        private decimal _unclosedProfit;
        [Column("未實現損益估計", "未損益", CSVStringFormat = "0.00", WPFDisplayIndex = 12, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal UnclosedProfit
        {
            get { return _unclosedProfit; }
            set { OnPropertyChanged(ref _unclosedProfit, value); }
        }

        public OpenInterestData([CallerMemberName] string memberName = "")
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            Strategy = string.Empty;
            MarketType = Market.EType.OverseaStock;
            Account = string.Empty;
            Quote = null;
            Symbol = string.Empty;
            BSEnum = OrderBS.Enum.Buy;
            DayTradeEnum = OrderDayTrade.Enum.No;
            PositionEnum = OrderPosition.Enum.Open;
            MarketPrice = 0;
            AveragePrice = 0;
            Quantity = 0;
            UnclosedProfit = 0;
        }

        public string ToLog()
        {
            return $"{Strategy},{MarketType},{Account},{Symbol},{BSEnum},{DayTradeEnum},{PositionEnum},{AveragePrice:0.00},{Quantity}";
        }
    }
}
