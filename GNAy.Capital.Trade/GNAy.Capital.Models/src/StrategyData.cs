﻿using GNAy.Tools.NET47;
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
    public class StrategyData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(StrategyData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(StrategyData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(StrategyData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.CSVName));

        public readonly object SyncRoot;

        public StrategyData Parent;

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

        private int _status;
        [Column("狀態索引")]
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

        [Column("狀態描述", "狀態", WPFDisplayIndex = 2)]
        public string StatusDes => StrategyStatus.Description[Status];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", WPFDisplayIndex = 3)]
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value); }
        }

        private Market.EType _marketType;
        [Column("市場", CSVIndex = -1)]
        public Market.EType MarketType
        {
            get { return _marketType; }
            set { OnPropertiesChanged(ref _marketType, value, nameof(MarketType), nameof(MarketName)); }
        }
        [Column("市場", CSVIndex = -1, WPFDisplayIndex = 4)]
        public string MarketName => Market.NameDescription[(int)MarketType];

        private string _branch;
        [Column("分公司")]
        public string Branch
        {
            get { return _branch; }
            set { OnPropertiesChanged(ref _branch, value, nameof(Branch), nameof(FullAccount)); }
        }
        private string _account;
        [Column("下單帳號")]
        public string Account
        {
            get { return _account; }
            set { OnPropertiesChanged(ref _account, value, nameof(Account), nameof(FullAccount)); }
        }
        [Column("下單帳號", CSVIndex = -1, WPFDisplayIndex = 5)]
        public string FullAccount => $"{Branch}{Account}";

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", WPFDisplayIndex = 6)]
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

        [Column("買賣描述", "買賣", WPFDisplayIndex = 7)]
        public string BSDes => OrderBS.Description[BS];

        private short _tradeType;
        [Column("掛單索引")]
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

        [Column("掛單描述", "掛單", WPFDisplayIndex = 8)]
        public string TradeTypeDes => OrderTradeType.Description[TradeType];

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

        [Column("當沖描述", "沖", WPFDisplayIndex = 9)]
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

        [Column("新倉平倉描述", "新平", WPFDisplayIndex = 10)]
        public string PositionDes => OrderPosition.Description[Position];

        private decimal _marketPrice;
        [Column("委託送出前的市場成交價", "市場價格", CSVStringFormat = "0.00", WPFDisplayIndex = 11, WPFStringFormat = "{0:0.00}")]
        public decimal MarketPrice
        {
            get { return _marketPrice; }
            set { OnPropertyChanged(ref _marketPrice, value); }
        }

        private string _orderPrice;
        [Column("委託價格", WPFDisplayIndex = 12)]
        public string OrderPrice
        {
            get { return _orderPrice; }
            set { OnPropertyChanged(ref _orderPrice, value); }
        }

        private int _orderQty;
        [Column("委託口數", "委量", WPFDisplayIndex = 13)]
        public int OrderQty
        {
            get { return _orderQty; }
            set { OnPropertyChanged(ref _orderQty, value); }
        }

        private string _stopLoss;
        [Column("停損設定", WPFDisplayIndex = 14)]
        public string StopLoss
        {
            get { return _stopLoss; }
            set { OnPropertyChanged(ref _stopLoss, value); }
        }

        private string _stopWinPrice;
        [Column("停利價格")]
        public string StopWinPrice
        {
            get { return _stopWinPrice; }
            set { OnPropertiesChanged(ref _stopWinPrice, value, nameof(StopWinPrice), nameof(StopWin)); }
        }
        private int _stopWinQty;
        [Column("停利減倉")]
        public int StopWinQty
        {
            get { return _stopWinQty; }
            set { OnPropertiesChanged(ref _stopWinQty, value, nameof(StopWinQty), nameof(StopWin)); }
        }
        [Column("停利設定", CSVIndex = -1, WPFDisplayIndex = 15)]
        public string StopWin => string.IsNullOrWhiteSpace(StopWinPrice) ? string.Empty : $"{StopWinPrice} ({StopWinQty})";

        private string _moveStopWinPrice;
        [Column("移動停利價格")]
        public string MoveStopWinPrice
        {
            get { return _moveStopWinPrice; }
            set { OnPropertiesChanged(ref _moveStopWinPrice, value, nameof(MoveStopWinPrice), nameof(MoveStopWin)); }
        }
        private int _moveStopWinQty;
        [Column("移動停利減倉")]
        public int MoveStopWinQty
        {
            get { return _moveStopWinQty; }
            set { OnPropertiesChanged(ref _moveStopWinQty, value, nameof(MoveStopWinQty), nameof(MoveStopWin)); }
        }
        [Column("移動停利設定", CSVIndex = -1, WPFDisplayIndex = 16)]
        public string MoveStopWin => string.IsNullOrWhiteSpace(MoveStopWinPrice) ? string.Empty : $"{MoveStopWinPrice} ({MoveStopWinQty})";

        private string _orderReport;
        [Column("13碼委託序號或錯誤訊息", "委託回報", WPFDisplayIndex = 17)]
        public string OrderReport
        {
            get { return _orderReport; }
            set { OnPropertyChanged(ref _orderReport, value); }
        }

        private decimal _dealPrice;
        [Column("成交價格", CSVStringFormat = "0.00", WPFDisplayIndex = 18, WPFStringFormat = "{0:0.00}")]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertiesChanged(ref _dealPrice, value, nameof(DealPrice), nameof(DealPct)); }
        }

        private int _dealQty;
        [Column("成交口數", WPFDisplayIndex = 19)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        [Column("成交價%", CSVStringFormat = "0.00", WPFDisplayIndex = 20, WPFStringFormat = "{0:0.00}%")]
        public decimal DealPct => (DealPrice != 0 && Quote != null && Quote.Reference != 0) ? (DealPrice - Quote.Reference) / Quote.Reference * 100 : 0;

        private string _dealReport;
        [Column("成交序號或錯誤訊息", "成交序號", WPFDisplayIndex = 21)]
        public string DealReport
        {
            get { return _dealReport; }
            set { OnPropertyChanged(ref _dealReport, value); }
        }

        //

        private string _comment;
        [Column("註解", WPFDisplayIndex = 22)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public StrategyData([CallerMemberName] string memberName = "")
        {
            SyncRoot = new object();
            Parent = null;
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusEnum = StrategyStatus.Enum.Waiting;
            PrimaryKey = string.Empty;
            MarketType = Market.EType.OverseaStock;
            Branch = string.Empty;
            Account = string.Empty;
            Quote = null;
            Symbol = string.Empty;
            BSEnum = OrderBS.Enum.Buy;
            TradeTypeEnum = OrderTradeType.Enum.ROD;
            DayTradeEnum = OrderDayTrade.Enum.No;
            PositionEnum = OrderPosition.Enum.Open;
            MarketPrice = 0;
            OrderPrice = Models.OrderPrice.P;
            OrderQty = -1;
            StopLoss = string.Empty;
            StopWinPrice = string.Empty;
            StopWinQty = 0;
            MoveStopWinPrice = string.Empty;
            MoveStopWinQty = 0;
            OrderReport = string.Empty;
            DealPrice = 0;
            DealQty = 0;
            DealReport = string.Empty;
            //
            Comment = string.Empty;
        }

        public StrategyData CreateOrder()
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (StatusDes != StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting])
            {
                throw new ArgumentException($"{StatusDes} != {StrategyStatus.Description[(int)StrategyStatus.Enum.Waiting]}|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Branch))
            {
                throw new ArgumentException($"未設定分公司|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Account))
            {
                throw new ArgumentException($"未設定下單帳號|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Symbol))
            {
                throw new ArgumentException($"未設定代碼|{ToLog()}");
            }
            else if (PositionDes == OrderPosition.Description[(int)OrderPosition.Enum.Close])
            {
                throw new ArgumentException($"PositionDes == {OrderPosition.Description[(int)OrderPosition.Enum.Close]}|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託口數({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{StrategyStatus.Enum.OrderSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Symbol = Symbol,
                BS = BS,
                TradeType = TradeType,
                DayTrade = DayTrade,
                Position = Position,
                OrderPrice = OrderPrice,
                OrderQty = OrderQty,
                Updater = nameof(CreateOrder),
                UpdateTime = DateTime.Now,
            };

            return order;
        }

        public StrategyData CreateStopLossOrder()
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (StatusDes != StrategyStatus.Description[(int)StrategyStatus.Enum.DealReport])
            {
                throw new ArgumentException($"{StatusDes} != {StrategyStatus.Description[(int)StrategyStatus.Enum.DealReport]}|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(PrimaryKey))
            {
                throw new ArgumentException($"未設定唯一鍵|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Branch))
            {
                throw new ArgumentException($"未設定分公司|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Account))
            {
                throw new ArgumentException($"未設定下單帳號|{ToLog()}");
            }
            else if (string.IsNullOrWhiteSpace(Symbol))
            {
                throw new ArgumentException($"未設定代碼|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{StrategyStatus.Enum.StopLossSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeType = TradeType,
                DayTrade = DayTrade,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPrice = Models.OrderPrice.P,
                //OrderQty = OrderQty,
                Updater = nameof(CreateStopLossOrder),
                UpdateTime = DateTime.Now,
            };

            return order;
        }

        public string ToLog()
        {//TODO
            return $"{StatusDes},{PrimaryKey},,{Comment}";
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
