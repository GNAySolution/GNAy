using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class StrategyData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(StrategyData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(StrategyData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(StrategyData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.Name));

        public readonly object SyncRoot;

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
        [Column("時間", 1, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
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
        [Column("更新時", 3, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { OnPropertiesChanged(ref _updateTime, value, nameof(UpdateTime), nameof(UpdateDate)); }
        }

        private int _status;
        [Column("狀態索引", 4)]
        public int Status
        {
            get { return _status; }
            set { OnPropertiesChanged(ref _status, value, nameof(Status), nameof(StatusDes)); }
        }
        public StrategyStatus.Enum StatusEnum
        {
            get { return (StrategyStatus.Enum)Status; }
            set { Status = (int)value; }
        }

        [Column("狀態描述", "狀態", 5)]
        public string StatusDes => StrategyStatus.Description[Status];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", 6)]
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value); }
        }

        public OrderAccData OrderAcc;

        private string _branch;
        [Column("分公司", 7)]
        public string Branch
        {
            get { return _branch; }
            set { OnPropertiesChanged(ref _branch, value, nameof(Branch), nameof(FullAccount)); }
        }
        private string _account;
        [Column("下單帳號", 8)]
        public string Account
        {
            get { return _account; }
            set { OnPropertiesChanged(ref _account, value, nameof(Account), nameof(FullAccount)); }
        }
        [Column("下單帳號", -1)]
        public string FullAccount => $"{Branch}{Account}";

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", 9)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private short _bs;
        [Column("買賣索引", 10)]
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

        [Column("買賣描述", "買賣", 11)]
        public string BSDes => OrderBS.Description[BS];

        private short _tradeType;
        [Column("掛單索引", 12)]
        public short TradeType
        {
            get { return _tradeType; }
            set { OnPropertiesChanged(ref _tradeType, value, nameof(TradeType), nameof(TradeTypeDes)); }
        }
        public OrderTradeType.Enum TradeTypeEnum
        {
            get { return (OrderTradeType.Enum)TradeType; }
            set { TradeType = (short)value; }
        }

        [Column("掛單描述", "掛單", 13)]
        public string TradeTypeDes => OrderTradeType.Description[TradeType];

        private short _dayTrade;
        [Column("當沖索引", 14)]
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

        [Column("當沖描述", "沖", 15)]
        public string DayTradeDes => OrderDayTrade.Description[DayTrade];

        private short _position;
        [Column("新倉平倉索引", 16)]
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

        [Column("新倉平倉描述", "新平", 17)]
        public string PositionDes => OrderPosition.Description[Position];

        private string _price;
        [Column("委託價格", "委價", 18)]
        public string Price
        {
            get { return _price; }
            set { OnPropertyChanged(ref _price, value); }
        }

        private int _quantity;
        [Column("委託口數", "委量", 19)]
        public int Quantity
        {
            get { return _quantity; }
            set { OnPropertyChanged(ref _quantity, value); }
        }

        private string _stopLossPrice;
        [Column("停損價格設定", 20)]
        public string StopLossPrice
        {
            get { return _stopLossPrice; }
            set { OnPropertiesChanged(ref _stopLossPrice, value, nameof(StopLossPrice), nameof(StopLoss)); }
        }
        private decimal _stopLossPct;
        [Column("停損價%設定", 21)]
        public decimal StopLossPct
        {
            get { return _stopLossPct; }
            set { OnPropertiesChanged(ref _stopLossPct, value, nameof(StopLossPct), nameof(StopLoss)); }
        }
        [Column("停損設定", -1)]
        public string StopLoss => $"{StopLossPrice} ({StopLossPct:0.00}%)";

        private string _afterStopLossPrice;
        [Column("停損價格觸發", 22)]
        public string AfterStopLossPrice
        {
            get { return _afterStopLossPrice; }
            set { OnPropertiesChanged(ref _afterStopLossPrice, value, nameof(AfterStopLossPrice), nameof(AfterStopLoss)); }
        }
        private decimal _afterStopLossPct;
        [Column("停損價%觸發", 23)]
        public decimal AfterStopLossPct
        {
            get { return _afterStopLossPct; }
            set { OnPropertiesChanged(ref _afterStopLossPct, value, nameof(AfterStopLossPct), nameof(AfterStopLoss)); }
        }
        [Column("停損觸發", -1)]
        public string AfterStopLoss => $"{AfterStopLossPrice} ({AfterStopLossPct:0.00}%)";

        //

        private string _comment;
        [Column("註解", 99)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public StrategyData()
        {
            SyncRoot = new object();
            Creator = string.Empty;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusEnum = StrategyStatus.Enum.Waiting;
            PrimaryKey = string.Empty;
            OrderAcc = null;
            Branch = string.Empty;
            Account = string.Empty;
            Quote = null;
            Symbol = string.Empty;
            BSEnum = OrderBS.Enum.Buy;
            TradeTypeEnum = OrderTradeType.Enum.ROD;
            DayTradeEnum = OrderDayTrade.Enum.No;
            PositionEnum = OrderPosition.Enum.Open;
            Price = OrderPrice.P;
            Quantity = 1;
            StopLossPrice = string.Empty;
            StopLossPct = 0;
            AfterStopLossPrice = string.Empty;
            AfterStopLossPct = 0;
            //
            Comment = string.Empty;
        }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},,{Comment}";
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
                if (ColumnSetters.TryGetValue(columnNames[i], out (ColumnAttribute, PropertyInfo) value))
                {
                    value.Item2.SetValueFromString(this, cells[i], value.Item1.StringFormat);
                }
            }
        }

        public static StrategyData Create(IList<string> columnNames, string lineCSV)
        {
            StrategyData data = new StrategyData();
            data.SetValues(columnNames, lineCSV.SplitToCSV());
            return data;
        }

        public static IEnumerable<StrategyData> ForeachQuoteFromCSVFile(string quotePath, List<string> columnNames)
        {
            foreach (string line in File.ReadLines(quotePath, TextEncoding.UTF8WithoutBOM))
            {
                if (columnNames.Count <= 0)
                {
                    columnNames.AddRange(line.Split(','));
                    continue;
                }

                StrategyData data = Create(columnNames, line);
                yield return data;
            }
        }
    }
}
