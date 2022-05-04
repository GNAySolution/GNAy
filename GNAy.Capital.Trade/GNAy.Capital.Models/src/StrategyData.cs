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

        private string _orderPriceBefore;
        [Column("委託價設定", "委價設定", WPFDisplayIndex = 12)]
        public string OrderPriceBefore
        {
            get { return _orderPriceBefore; }
            set { OnPropertyChanged(ref _orderPriceBefore, value); }
        }
        private decimal _orderPriceAfter;
        [Column("委託價觸發", "委價觸發", CSVStringFormat = "0.00", WPFDisplayIndex = 13, WPFStringFormat = "{0:0.00}")]
        public decimal OrderPriceAfter
        {
            get { return _orderPriceAfter; }
            set { OnPropertyChanged(ref _orderPriceAfter, value); }
        }

        private int _orderQty;
        [Column("委託口數", "委量", WPFDisplayIndex = 14)]
        public int OrderQty
        {
            get { return _orderQty; }
            set { OnPropertyChanged(ref _orderQty, value); }
        }

        public StrategyData OrderData;

        private string _stopLossBefore;
        [Column("停損設定", WPFDisplayIndex = 15)]
        public string StopLossBefore
        {
            get { return _stopLossBefore; }
            set { OnPropertyChanged(ref _stopLossBefore, value); }
        }
        private decimal _stopLossAfter;
        [Column("停損觸發", CSVStringFormat = "0.00", WPFDisplayIndex = 16, WPFStringFormat = "{0:0.00}")]
        public decimal StopLossAfter
        {
            get { return _stopLossAfter; }
            set { OnPropertyChanged(ref _stopLossAfter, value); }
        }

        public StrategyData StopLossData;

        private string _stopWinBefore;
        [Column("停利設定", WPFDisplayIndex = 17)]
        public string StopWinBefore
        {
            get { return _stopWinBefore; }
            set { OnPropertyChanged(ref _stopWinBefore, value); }
        }
        private decimal _stopWinPrice;
        [Column("停利價格", CSVStringFormat = "0.00")]
        public decimal StopWinPrice
        {
            get { return _stopWinPrice; }
            set { OnPropertiesChanged(ref _stopWinPrice, value, nameof(StopWinPrice), nameof(StopWinAfter)); }
        }
        private int _stopWinQty;
        [Column("停利減倉")]
        public int StopWinQty
        {
            get { return _stopWinQty; }
            set { OnPropertiesChanged(ref _stopWinQty, value, nameof(StopWinQty), nameof(StopWinAfter)); }
        }
        [Column("停利觸發", CSVIndex = -1, WPFDisplayIndex = 18)]
        public string StopWinAfter => StopWinPrice == 0 ? string.Empty : $"{StopWinPrice} ({StopWinQty})";

        public StrategyData StopWinData;

        private string _moveStopWinBefore;
        [Column("移動停利設定", "移利設定", WPFDisplayIndex = 19)]
        public string MoveStopWinBefore
        {
            get { return _moveStopWinBefore; }
            set { OnPropertyChanged(ref _moveStopWinBefore, value); }
        }
        private decimal _moveStopWinPrice;
        [Column("移動停利價格", CSVStringFormat = "0.00")]
        public decimal MoveStopWinPrice
        {
            get { return _moveStopWinPrice; }
            set { OnPropertiesChanged(ref _moveStopWinPrice, value, nameof(MoveStopWinPrice), nameof(MoveStopWinAfter)); }
        }
        private int _moveStopWinQty;
        [Column("移動停利減倉")]
        public int MoveStopWinQty
        {
            get { return _moveStopWinQty; }
            set { OnPropertiesChanged(ref _moveStopWinQty, value, nameof(MoveStopWinQty), nameof(MoveStopWinAfter)); }
        }
        [Column("移動停利觸發", "移利觸發", CSVIndex = -1, WPFDisplayIndex = 20)]
        public string MoveStopWinAfter => MoveStopWinPrice == 0 ? string.Empty : $"{MoveStopWinPrice} ({MoveStopWinQty})";

        public StrategyData MoveStopWinData;

        private string _orderReport;
        [Column("13碼委託序號或錯誤訊息", "委託回報", WPFDisplayIndex = 21)]
        public string OrderReport
        {
            get { return _orderReport; }
            set { OnPropertyChanged(ref _orderReport, value); }
        }

        private decimal _dealPrice;
        [Column("成交價格", "成價", CSVStringFormat = "0.00", WPFDisplayIndex = 22, WPFStringFormat = "{0:0.00}")]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertyChanged(ref _dealPrice, value); }
        }

        private int _dealQty;
        [Column("成交口數", "成量", WPFDisplayIndex = 23)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        private string _dealReport;
        [Column("成交序號或錯誤訊息", "成交序號", WPFDisplayIndex = 24)]
        public string DealReport
        {
            get { return _dealReport; }
            set { OnPropertyChanged(ref _dealReport, value); }
        }

        private decimal _closedProfit;
        [Column("已實現損益", "已損益", CSVStringFormat = "0.00", WPFDisplayIndex = 25, WPFStringFormat = "{0:0.00}")]
        public decimal ClosedProfit
        {
            get { return _closedProfit; }
            set { OnPropertyChanged(ref _closedProfit, value); }
        }

        private int _unclosedQty;
        [Column("未平倉口數", "未平量", WPFDisplayIndex = 26)]
        public int UnclosedQty
        {
            get { return _unclosedQty; }
            set
            {
                if (OnPropertyChanged(ref _unclosedQty, value) && value == 0)
                {
                    UnclosedProfit = 0;
                }
            }
        }

        private decimal _unclosedProfit;
        [Column("未實現損益", "未損益", CSVStringFormat = "0.00", WPFDisplayIndex = 27, WPFStringFormat = "{0:0.00}")]
        public decimal UnclosedProfit
        {
            get { return _unclosedProfit; }
            set { OnPropertyChanged(ref _unclosedProfit, value); }
        }

        private string _triggerAfterStopLoss;
        [Column("停損後接續執行觸價", "停損後觸價", WPFDisplayIndex = 28)]
        public string TriggerAfterStopLoss
        {
            get { return _triggerAfterStopLoss; }
            set { OnPropertyChanged(ref _triggerAfterStopLoss, value); }
        }

        private string _strategyAfterStopLoss;
        [Column("停損後接續執行策略", "停損後策略", WPFDisplayIndex = 29)]
        public string StrategyAfterStopLoss
        {
            get { return _strategyAfterStopLoss; }
            set { OnPropertyChanged(ref _strategyAfterStopLoss, value); }
        }

        private string _triggerAfterStopWin;
        [Column("停利後接續執行觸價", "停利後觸價", WPFDisplayIndex = 30)]
        public string TriggerAfterStopWin
        {
            get { return _triggerAfterStopWin; }
            set { OnPropertyChanged(ref _triggerAfterStopWin, value); }
        }

        private string _strategyAfterStopWin;
        [Column("停利後接續執行策略", "停利後策略", WPFDisplayIndex = 31)]
        public string StrategyAfterStopWin
        {
            get { return _strategyAfterStopWin; }
            set { OnPropertyChanged(ref _strategyAfterStopWin, value); }
        }

        private int _winCloseQty;
        [Column("收盤獲利減倉口數", "收獲量", WPFDisplayIndex = 32)]
        public int WinCloseQty
        {
            get { return _winCloseQty; }
            set { OnPropertyChanged(ref _winCloseQty, value); }
        }

        private int _winCloseSeconds;
        [Column("收盤前幾秒獲利減倉", "收獲秒", WPFDisplayIndex = 33)]
        public int WinCloseSeconds
        {
            get { return _winCloseSeconds; }
            set { OnPropertyChanged(ref _winCloseSeconds, value); }
        }

        private int _lossCloseQty;
        [Column("收盤損失減倉口數", "收損量", WPFDisplayIndex = 34)]
        public int LossCloseQty
        {
            get { return _lossCloseQty; }
            set { OnPropertyChanged(ref _lossCloseQty, value); }
        }

        private int _lossCloseSeconds;
        [Column("收盤前幾秒損失減倉", "收損秒", WPFDisplayIndex = 35)]
        public int LossCloseSeconds
        {
            get { return _lossCloseSeconds; }
            set { OnPropertyChanged(ref _lossCloseSeconds, value); }
        }

        //

        private string _comment;
        [Column("註解", WPFDisplayIndex = 36)]
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
            OrderPriceBefore = OrderPrice.P;
            OrderPriceAfter = 0;
            OrderQty = -1;
            OrderData = null;
            StopLossBefore = string.Empty;
            StopLossAfter = 0;
            StopLossData = null;
            StopWinBefore = string.Empty;
            StopWinPrice = 0;
            StopWinQty = 0;
            StopWinData = null;
            MoveStopWinBefore = string.Empty;
            MoveStopWinPrice = 0;
            MoveStopWinQty = 0;
            MoveStopWinData = null;
            OrderReport = string.Empty;
            DealPrice = 0;
            DealQty = 0;
            DealReport = string.Empty;
            ClosedProfit = 0;
            UnclosedQty = 0;
            UnclosedProfit = 0;
            TriggerAfterStopLoss = string.Empty;
            StrategyAfterStopLoss = string.Empty;
            TriggerAfterStopWin = string.Empty;
            StrategyAfterStopWin = string.Empty;
            WinCloseQty = 0;
            WinCloseSeconds = 0;
            LossCloseQty = 0;
            LossCloseSeconds = 0;
            //
            Comment = string.Empty;
        }

        public StrategyData Trim()
        {
            PrimaryKey = PrimaryKey.Replace(" ", string.Empty);
            Branch = Branch.Replace(" ", string.Empty);
            Account = Account.Replace(" ", string.Empty);
            Symbol = Symbol.Replace(" ", string.Empty);
            OrderPriceBefore = OrderPriceBefore.Replace(" ", string.Empty);
            StopLossBefore = StopLossBefore.Replace(" ", string.Empty);
            StopWinBefore = StopWinBefore.Replace(" ", string.Empty);
            MoveStopWinBefore = MoveStopWinBefore.Replace(" ", string.Empty);
            OrderReport = OrderReport.Replace(" ", string.Empty);
            DealReport = DealReport.Replace(" ", string.Empty);
            TriggerAfterStopLoss = TriggerAfterStopLoss.Replace(" ", string.Empty);
            StrategyAfterStopLoss = StrategyAfterStopLoss.Replace(" ", string.Empty);
            TriggerAfterStopWin = TriggerAfterStopWin.Replace(" ", string.Empty);
            StrategyAfterStopWin = StrategyAfterStopWin.Replace(" ", string.Empty);
            Comment = Comment.Replace(" ", string.Empty);

            return this;
        }

        public StrategyData CreateOrder()
        {
            const string methodName = nameof(CreateOrder);

            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            //else if (StatusEnum != StrategyStatus.Enum.Waiting)
            //{
            //    throw new ArgumentException($"{StatusEnum} != StrategyStatus.Enum.Waiting|{ToLog()}");
            //}
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
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
                Quote = Quote,
                Symbol = Symbol,
                BS = BS,
                TradeType = TradeType,
                DayTrade = DayTrade,
                Position = Position,
                OrderPriceBefore = OrderPriceBefore,
                OrderPriceAfter = OrderPriceAfter,
                OrderQty = OrderQty,
                Updater = methodName,
                UpdateTime = DateTime.Now,
            };

            OrderData = order;

            return order;
        }

        public StrategyData CreateStopLossOrder()
        {
            const string methodName = nameof(CreateStopLossOrder);

            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            //else if (StatusEnum != StrategyStatus.Enum.DealReport)
            //{
            //    throw new ArgumentException($"{StatusEnum} != StrategyStatus.Enum.DealReport|{ToLog()}");
            //}
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託口數({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{StrategyStatus.Enum.StopLossSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeType = TradeType,
                DayTrade = DayTrade,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPriceBefore = OrderPrice.P,
                OrderPriceAfter = 0,
                OrderQty = OrderQty,
                Updater = methodName,
                UpdateTime = DateTime.Now,
            };

            StopLossData = order;

            return order;
        }

        public StrategyData CreateStopWinOrder()
        {
            const string methodName = nameof(CreateStopWinOrder);

            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            //else if (StatusEnum != StrategyStatus.Enum.DealReport)
            //{
            //    throw new ArgumentException($"{StatusEnum} != StrategyStatus.Enum.DealReport|{ToLog()}");
            //}
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託口數({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{StrategyStatus.Enum.StopWinSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeType = TradeType,
                DayTrade = DayTrade,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPriceBefore = OrderPrice.P,
                OrderPriceAfter = 0,
                OrderQty = Math.Abs(StopWinQty),
                Updater = methodName,
                UpdateTime = DateTime.Now,
            };

            StopWinData = order;

            return order;
        }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},{MarketType},{Account},{Symbol},{BSEnum},{PositionEnum},{OrderPriceBefore},{OrderPriceAfter:0.00},{OrderQty},{Comment}";
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
