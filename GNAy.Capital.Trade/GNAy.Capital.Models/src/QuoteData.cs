using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class QuoteData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (TradeColumnAttribute, PropertyInfo)> PropertyMap = typeof(QuoteData).GetColumnAttrMapByProperty<TradeColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (TradeColumnAttribute, PropertyInfo)> ColumnGetters = typeof(QuoteData).GetColumnAttrMapByIndex<TradeColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (TradeColumnAttribute, PropertyInfo)> ColumnSetters = typeof(QuoteData).GetColumnAttrMapByName<TradeColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.Name));

        private string _creator;
        [TradeColumn("建立者", 0)]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [TradeColumn("日期", -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [TradeColumn("時間", 1, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set
            {
                if (OnPropertyChanged(ref _createdTime, value))
                {
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
        }

        private string _updater;
        [TradeColumn("更新者", 2)]
        public string Updater
        {
            get { return _updater; }
            set { OnPropertyChanged(ref _updater, value); }
        }

        [TradeColumn("更新日", -1)]
        public DateTime UpdateDate => UpdateTime.Date;

        private DateTime _updateTime;
        [TradeColumn("更新時", 3, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", IsTrigger = true, ValueFormat = "HHmmss.ffffff")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set
            {
                if (OnPropertyChanged(ref _updateTime, value))
                {
                    OnPropertyChanged(nameof(UpdateDate));
                }
            }
        }

        private string _symbol;
        [TradeColumn("代碼", 4)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private string _name;
        [TradeColumn("名稱", 5)]
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged(ref _name, value); }
        }

        public string SymbolAndName => $"{Symbol},{Name}";

        private int _matchedTimeHHmmss;
        [TradeColumn("成交時分秒", "成時分秒", 6, StringFormat = "000000", IsTrigger = true)]
        public int MatchedTimeHHmmss
        {
            get { return _matchedTimeHHmmss; }
            set { OnPropertyChanged(ref _matchedTimeHHmmss, value); }
        }

        private int _matchedTimefff;
        [TradeColumn("成交微秒", "成微秒", 7, StringFormat = "000000")]
        public int MatchedTimefff
        {
            get { return _matchedTimefff; }
            set { OnPropertyChanged(ref _matchedTimefff, value); }
        }

        [TradeColumn("成交時間", 8, StringFormat = "HHmmss.ffffff")]
        public DateTime MatchedTime => DateTime.ParseExact(String.Format("{0}.{1}", MatchedTimeHHmmss.ToString().PadLeft(6, '0'), MatchedTimefff.ToString().PadLeft(6, '0')), "HHmmss.ffffff", CultureInfo.InvariantCulture);

        private decimal _dealPrice;
        [TradeColumn("成交價", "成價", 9, StringFormat = "0.00", IsTrigger = true)]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set
            {
                if (OnPropertyChanged(ref _dealPrice, value))
                {
                    OnPropertyChanged(nameof(UpDown));
                    OnPropertyChanged(nameof(UpDownPct));
                    OnPropertyChanged(nameof(DealHigh));
                    OnPropertyChanged(nameof(DealLow));
                    OnPropertyChanged(nameof(RowBackground));
                }
            }
        }

        private int _dealQty;
        [TradeColumn("成交量", "成量", 10, IsTrigger = true)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        [TradeColumn("漲跌", 11, StringFormat = "0.00", IsTrigger = true)]
        public decimal UpDown => (DealPrice != 0 && Reference != 0) ? DealPrice - Reference : 0;

        [TradeColumn("漲跌幅", 12, StringFormat = "0.00", IsTrigger = true)]
        public decimal UpDownPct => (DealPrice != 0 && Reference != 0) ? (DealPrice - Reference) / Reference * 100 : 0;

        private decimal _bestBuyPrice;
        [TradeColumn("買價", 13, StringFormat = "0.00", IsTrigger = true)]
        public decimal BestBuyPrice
        {
            get { return _bestBuyPrice; }
            set { OnPropertyChanged(ref _bestBuyPrice, value); }
        }

        private int _bestBuyQty;
        [TradeColumn("買量", 14, IsTrigger = true)]
        public int BestBuyQty
        {
            get { return _bestBuyQty; }
            set { OnPropertyChanged(ref _bestBuyQty, value); }
        }

        private decimal _bestSellPrice;
        [TradeColumn("賣價", 15, StringFormat = "0.00", IsTrigger = true)]
        public decimal BestSellPrice
        {
            get { return _bestSellPrice; }
            set { OnPropertyChanged(ref _bestSellPrice, value); }
        }

        private int _bestSellyQty;
        [TradeColumn("賣量", 16, IsTrigger = true)]
        public int BestSellQty
        {
            get { return _bestSellyQty; }
            set { OnPropertyChanged(ref _bestSellyQty, value); }
        }

        private decimal _openPrice;
        [TradeColumn("開盤價", 17, StringFormat = "0.00", IsTrigger = true)]
        public decimal OpenPrice
        {
            get { return _openPrice; }
            set
            {
                if (OnPropertyChanged(ref _openPrice, value))
                {
                    OnPropertyChanged(nameof(OpenUpDown));
                    OnPropertyChanged(nameof(OpenLastCloseUpDown));
                }
            }
        }

        [TradeColumn("開盤漲跌", "開盤漲", 18, StringFormat = "0.00", IsTrigger = true)]
        public decimal OpenUpDown => (OpenPrice != 0 && Reference != 0) ? OpenPrice - Reference : 0;

        private decimal _highPrice;
        [TradeColumn("最高價", 19, StringFormat = "0.00", IsTrigger = true)]
        public decimal HighPrice
        {
            get { return _highPrice; }
            set
            {
                OnPropertyChanged(ref _highPrice, value);
                OnPropertyChanged(nameof(DealHigh));
                OnPropertyChanged(nameof(HighLow));
            }
        }

        [TradeColumn("成交最高價差", "成高差", 20, StringFormat = "0.00", IsTrigger = true)]
        public decimal DealHigh => (HighPrice != 0) ? DealPrice - HighPrice : 0;

        private decimal _lowPrice;
        [TradeColumn("最低價", 21, StringFormat = "0.00", IsTrigger = true)]
        public decimal LowPrice
        {
            get { return _lowPrice; }
            set
            {
                OnPropertyChanged(ref _lowPrice, value);
                OnPropertyChanged(nameof(DealLow));
                OnPropertyChanged(nameof(HighLow));
            }
        }

        [TradeColumn("成交最低價差", "成低差", 22, StringFormat = "0.00", IsTrigger = true)]
        public decimal DealLow => (LowPrice != 0) ? DealPrice - LowPrice : 0;

        [TradeColumn("最高最低價差", "高低差", 23, StringFormat = "0.00", IsTrigger = true)]
        public decimal HighLow => HighPrice - LowPrice;

        private decimal _reference;
        [TradeColumn("參考價", 24, StringFormat = "0.00")]
        public decimal Reference
        {
            get { return _reference; }
            set
            {
                if (OnPropertyChanged(ref _reference, value))
                {
                    OnPropertyChanged(nameof(UpDown));
                    OnPropertyChanged(nameof(UpDownPct));
                    OnPropertyChanged(nameof(RowBackground));
                    OnPropertyChanged(nameof(OpenUpDown));
                }
            }
        }

        private decimal _lastClosePrice;
        [TradeColumn("前盤收盤價格", "前盤收價", 25, StringFormat = "0.00")]
        public decimal LastClosePrice
        {
            get { return _lastClosePrice; }
            set
            {
                if (OnPropertyChanged(ref _lastClosePrice, value))
                {
                    OnPropertyChanged(nameof(OpenLastCloseUpDown));
                }
            }
        }

        [TradeColumn("開盤與前盤收盤價差", "開前價差", 26, StringFormat = "0.00", IsTrigger = true)]
        public decimal OpenLastCloseUpDown => (OpenPrice != 0 && LastClosePrice != 0) ? OpenPrice - LastClosePrice : 0;

        private int _simulate;
        [TradeColumn("試撮", "試", 27)]
        public int Simulate
        {
            get { return _simulate; }
            set
            {
                if (OnPropertyChanged(ref _simulate, value))
                {
                    OnPropertyChanged(nameof(RowBackground));
                }
            }
        }

        private int _totalQty;
        [TradeColumn("總量", 28, IsTrigger = true)]
        public int TotalQty
        {
            get { return _totalQty; }
            set { OnPropertyChanged(ref _totalQty, value); }
        }

        private int _tradeDateRaw;
        [TradeColumn("交易日", 29, IsTrigger = true, ValueFormat = "yyyyMMdd")]
        public int TradeDateRaw
        {
            get { return _tradeDateRaw; }
            set
            {
                if (OnPropertyChanged(ref _tradeDateRaw, value))
                {
                    OnPropertyChanged(nameof(TradeDate));
                }
            }
        }

        [TradeColumn("交易日", -1)]
        public DateTime TradeDate => (TradeDateRaw <= 0) ? DateTime.MaxValue.Date : DateTime.ParseExact(TradeDateRaw.ToString().PadLeft(8, '0'), "yyyyMMdd", CultureInfo.InvariantCulture);

        private decimal _highPriceLimit;
        [TradeColumn("漲停", 30, StringFormat = "0.00")]
        public decimal HighPriceLimit
        {
            get { return _highPriceLimit; }
            set { OnPropertyChanged(ref _highPriceLimit, value); }
        }

        private decimal _lowPriceLimit;
        [TradeColumn("跌停", 31, StringFormat = "0.00")]
        public decimal LowPriceLimit
        {
            get { return _lowPriceLimit; }
            set { OnPropertyChanged(ref _lowPriceLimit, value); }
        }

        private int _count;
        [TradeColumn("筆數", -1)]
        public int Count
        {
            get { return _count; }
            set { OnPropertyChanged(ref _count, value); }
        }

        private int _index;
        [TradeColumn("索引", 32)]
        public int Index
        {
            get { return _index; }
            set { OnPropertyChanged(ref _index, value); }
        }

        private short _page;
        [TradeColumn("Page", "P", 33)]
        public short Page
        {
            get { return _page; }
            set { OnPropertyChanged(ref _page, value); }
        }

        private short _market;
        [TradeColumn("市場", "市", 34)]
        public short Market
        {
            get { return _market; }
            set { OnPropertyChanged(ref _market, value); }
        }

        private short _decimalPos;
        [TradeColumn("小數位數", "D", 35)]
        public short DecimalPos
        {
            get { return _decimalPos; }
            set { OnPropertyChanged(ref _decimalPos, value); }
        }

        private int _totalQtyBefore;
        [TradeColumn("昨量", 36)]
        public int TotalQtyBefore
        {
            get { return _totalQtyBefore; }
            set { OnPropertyChanged(ref _totalQtyBefore, value); }
        }

        private bool _recovered;
        [TradeColumn("從檔案回補報價資料", "回補", -1)]
        public bool Recovered
        {
            get { return _recovered; }
            set { OnPropertyChanged(ref _recovered, value); }
        }

        public int RowBackground
        {
            get
            {
                if (Simulate.IsRealTrading())
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
            MatchedTimeHHmmss = 0;
            MatchedTimefff = 0;
            DealPrice = 0;
            DealQty = 0;
            BestBuyPrice = 0;
            BestBuyQty = 0;
            BestSellPrice = 0;
            BestSellQty = 0;
            OpenPrice = 0;
            HighPrice = 0;
            LowPrice = 0;
            Reference = 0;
            LastClosePrice = 0;
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
            Recovered = false;
        }

        public string ToCSVString()
        {
            string result = string.Join("\",\"", ColumnGetters.Values.Select(x => x.Item2.ValueToString(this, x.Item1.StringFormat)));
            return $"\"{result}\"";
        }

        public void ToCSVFile(string path, bool append = true)
        {
            bool exists = File.Exists(path);

            using (StreamWriter sw = new StreamWriter(path, append, TextEncoding.UTF8WithoutBOM))
            {
                if (!append || !exists)
                {
                    sw.WriteLine(CSVColumnNames);
                }

                sw.WriteLine(ToCSVString());
            }
        }

        public void SetValues(IList<string> columnNames, IList<string> cells)
        {
            for (int i = 0; i < columnNames.Count; ++i)
            {
                if (ColumnSetters.TryGetValue(columnNames[i], out (TradeColumnAttribute, PropertyInfo) value))
                {
                    value.Item2.SetValueFromString(this, cells[i], value.Item1.StringFormat);
                }
            }
        }

        public static QuoteData Create(IList<string> columnNames, string lineCSV)
        {
            QuoteData data = new QuoteData();
            data.SetValues(columnNames, lineCSV.Split(Separator.CSV, StringSplitOptions.RemoveEmptyEntries));
            return data;
        }

        public static IEnumerable<QuoteData> ForeachQuoteFromCSVFile(string quotePath, List<string> columnNames)
        {
            foreach (string line in File.ReadLines(quotePath, TextEncoding.UTF8WithoutBOM))
            {
                if (columnNames.Count <= 0)
                {
                    columnNames.AddRange(line.Split(Separator.CSV));
                    continue;
                }

                QuoteData data = Create(columnNames, line);
                if (!data.Simulate.IsRealTrading())
                {
                    continue;
                }

                yield return data;
            }
        }
    }
}
