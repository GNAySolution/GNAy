using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(OpenInterestData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(OpenInterestData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.CSVName));

        public readonly object SyncRoot;

        public OpenInterestData Parent;

        private string _creator;
        [Column("建立者")]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Column("日期", CSVIndex = -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
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

        private Market.EType _marketType;
        [Column("市場", CSVIndex = -1)]
        public Market.EType MarketType
        {
            get { return _marketType; }
            set { OnPropertiesChanged(ref _marketType, value, nameof(MarketType), nameof(MarketName)); }
        }
        [Column("市場", CSVIndex = -1, WPFDisplayIndex = 2)]
        public string MarketName => Market.NameDescription[(int)MarketType];

        private string _account;
        [Column("下單帳號")]
        public string Account
        {
            get { return _account; }
            set { OnPropertyChanged(ref _account, value); }
        }

        private string _symbol;
        [Column("代碼", WPFDisplayIndex = 3)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private short _bs;
        [Column("買賣索引")]
        public short BS
        {
            get { return _bs; }
            set { OnPropertiesChanged(ref _bs, value, nameof(BS), nameof(BSDes)); }
        }
        public OrderBS.Enum BSEnum
        {
            get { return (OrderBS.Enum)BS; }
            set { BS = (short)value; }
        }

        [Column("買賣描述", "買賣", WPFDisplayIndex = 4)]
        public string BSDes => OrderBS.Description[BS];

        private short _dayTrade;
        [Column("當沖索引")]
        public short DayTrade
        {
            get { return _dayTrade; }
            set { OnPropertiesChanged(ref _dayTrade, value, nameof(DayTrade), nameof(DayTradeDes)); }
        }
        public OrderDayTrade.Enum DayTradeEnum
        {
            get { return (OrderDayTrade.Enum)DayTrade; }
            set { DayTrade = (short)value; }
        }

        [Column("當沖描述", "沖", WPFDisplayIndex = 5)]
        public string DayTradeDes => OrderDayTrade.Description[DayTrade];

        private short _position;
        [Column("新倉平倉索引")]
        public short Position
        {
            get { return _position; }
            set { OnPropertiesChanged(ref _position, value, nameof(Position), nameof(PositionDes)); }
        }
        public OrderPosition.Enum PositionEnum
        {
            get { return (OrderPosition.Enum)Position; }
            set { Position = (short)value; }
        }

        [Column("新倉平倉描述", "新平", WPFDisplayIndex = 6)]
        public string PositionDes => OrderPosition.Description[Position];

        private decimal _dealPrice;
        [Column("成交均價", "成價", CSVStringFormat = "0.00", WPFDisplayIndex = 7, WPFStringFormat = "{0:0.00}")]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertyChanged(ref _dealPrice, value); }
        }

        private int _dealQty;
        [Column("成交口數", "成量", WPFDisplayIndex = 8)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        public OpenInterestData([CallerMemberName] string memberName = "")
        {
            SyncRoot = new object();
            Parent = null;
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            MarketType = Market.EType.OverseaStock;
            Account = string.Empty;
            Symbol = string.Empty;
            BSEnum = OrderBS.Enum.Buy;
            DayTradeEnum = OrderDayTrade.Enum.No;
            PositionEnum = OrderPosition.Enum.Open;
            DealPrice = 0;
            DealQty = 0;
        }

        public string ToLog()
        {
            return $"{MarketType},{Account},{Symbol},{BSEnum},{PositionEnum}";
        }

        public string ToCSVString()
        {
            string result = string.Join("\",\"", ColumnGetters.Values.Select(x => x.Item2.ValueToString(this, x.Item1.CSVStringFormat)));
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
                if (ColumnSetters.TryGetValue(columnNames[i], out (ColumnAttribute, PropertyInfo) value))
                {
                    value.Item2.SetValueFromString(this, cells[i], value.Item1.CSVStringFormat);
                }
            }
        }

        public static OpenInterestData Create(IList<string> columnNames, string lineCSV)
        {
            OpenInterestData data = new OpenInterestData();
            data.SetValues(columnNames, lineCSV.SplitToCSV());
            return data;
        }

        public static IEnumerable<OpenInterestData> ForeachQuoteFromCSVFile(string quotePath, List<string> columnNames)
        {
            foreach (string line in File.ReadLines(quotePath, TextEncoding.UTF8WithoutBOM))
            {
                if (columnNames.Count <= 0)
                {
                    columnNames.AddRange(line.Split(','));
                    continue;
                }

                OpenInterestData data = Create(columnNames, line);
                yield return data;
            }
        }
    }
}
