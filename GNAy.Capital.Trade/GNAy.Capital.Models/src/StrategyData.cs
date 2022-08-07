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
        public const int StopWin1 = 1;
        public const int StopWin2 = 2;
        public const int StopWin3 = 3;

        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(StrategyData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(StrategyData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(StrategyData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.CSVName));

        public readonly object SyncRoot;

        public StrategyData Parent;

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

        private StrategyStatus.Enum _statusEnum;
        [Column("狀態索引")]
        public StrategyStatus.Enum StatusEnum
        {
            get { return _statusEnum; }
            set { OnPropertiesChanged(ref _statusEnum, value, nameof(StatusEnum), nameof(StatusDes), nameof(StopLossAfter), nameof(StopWinPriceAAfter), nameof(StopWin1After), nameof(StopWin2After)); }
        }
        [Column("狀態描述", "狀態", WPFDisplayIndex = 2, WPFForeground = "MediumBlue")]
        public string StatusDes => StrategyStatus.Description[(int)StatusEnum];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", WPFDisplayIndex = 3, WPFHorizontalAlignment = WPFHorizontalAlignment.Center)]
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
        [Column("代碼", WPFDisplayIndex = 6, WPFForeground = "MediumBlue")]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private OrderBS.Enum _bsEnum;
        [Column("買賣", WPFDisplayIndex = 7)]
        public OrderBS.Enum BSEnum
        {
            get { return _bsEnum; }
            set
            {
                if (OnPropertyChanged(ref _bsEnum, value))
                {
                    ProfitDirection = value == OrderBS.Enum.Buy ? 1 : -1;
                }
            }
        }
        public int ProfitDirection;

        private OrderTradeType.Enum _tradeTypeEnum;
        [Column("掛單", WPFDisplayIndex = 8)]
        public OrderTradeType.Enum TradeTypeEnum
        {
            get { return _tradeTypeEnum; }
            set { OnPropertyChanged(ref _tradeTypeEnum, value); }
        }

        private OrderDayTrade.Enum _dayTradeEnum;
        [Column("當沖", WPFDisplayIndex = 9)]
        public OrderDayTrade.Enum DayTradeEnum
        {
            get { return _dayTradeEnum; }
            set { OnPropertyChanged(ref _dayTradeEnum, value); }
        }

        private OrderPosition.Enum _positionEnum;
        [Column("新倉平倉", "新平", WPFDisplayIndex = 10)]
        public OrderPosition.Enum PositionEnum
        {
            get { return _positionEnum; }
            set { OnPropertyChanged(ref _positionEnum, value); }
        }

        private decimal _marketPrice;
        [Column("委託送出前的市場成交價", "市場價格", CSVStringFormat = "0.00", WPFDisplayIndex = 11, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal MarketPrice
        {
            get { return _marketPrice; }
            set { OnPropertyChanged(ref _marketPrice, value); }
        }

        private string _orderPriceBefore;
        [Column("委託價設定", "委價設定", WPFDisplayIndex = 12, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string OrderPriceBefore
        {
            get { return _orderPriceBefore; }
            set { OnPropertyChanged(ref _orderPriceBefore, value); }
        }
        private decimal _orderPriceAfter;
        [Column("委託價觸發", "委價觸發", CSVStringFormat = "0.00", WPFDisplayIndex = 13, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal OrderPriceAfter
        {
            get { return _orderPriceAfter; }
            set { OnPropertyChanged(ref _orderPriceAfter, value); }
        }

        private int _orderQty;
        [Column("委託量", "委量", WPFDisplayIndex = 14, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int OrderQty
        {
            get { return _orderQty; }
            set { OnPropertyChanged(ref _orderQty, value); }
        }

        private decimal _bestClosePrice;
        [Column("最佳平倉價格", "最平價", CSVStringFormat = "0.00", WPFDisplayIndex = 15, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal BestClosePrice
        {
            get { return _bestClosePrice; }
            set { OnPropertiesChanged(ref _bestClosePrice, value, nameof(BestClosePrice), nameof(StopWin1After), nameof(StopWin2After)); }
        }

        public StrategyData OrderData;

        private string _stopLossBefore;
        [Column("停損設定", WPFDisplayIndex = 16, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string StopLossBefore
        {
            get { return _stopLossBefore; }
            set { OnPropertyChanged(ref _stopLossBefore, value); }
        }
        private decimal _stopLossAfter;
        [Column("停損觸發", CSVStringFormat = "0.00")]
        public decimal StopLossAfterRaw
        {
            get { return _stopLossAfter; }
            set { OnPropertiesChanged(ref _stopLossAfter, value, nameof(StopLossAfterRaw), nameof(StopLossAfter)); }
        }
        [Column("停損觸發", CSVIndex = -1, WPFDisplayIndex = 17, WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public string StopLossAfter => StopLossAfterRaw == 0 ? string.Empty : StopLossData == null ? $"*{StopLossAfterRaw:0.00}" : $"{StopLossAfterRaw:0.00}";

        public StrategyData StopLossData;

        private string _stopWinPriceABefore;
        [Column("停利價A設定", WPFDisplayIndex = 18, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string StopWinPriceABefore
        {
            get { return _stopWinPriceABefore; }
            set { OnPropertyChanged(ref _stopWinPriceABefore, value); }
        }
        private bool _stopWinATouched;
        [Column("停利A觸發", CSVIndex = -1)]
        public bool StopWinATouched
        {
            get { return _stopWinATouched; }
            set { OnPropertiesChanged(ref _stopWinATouched, value, nameof(StopWinATouched), nameof(StopWinPriceAAfter)); }
        }
        private decimal _stopWinPriceAAfterRaw;
        [Column("停利價A觸發", CSVStringFormat = "0.00")]
        public decimal StopWinPriceAAfterRaw
        {
            get { return _stopWinPriceAAfterRaw; }
            set { OnPropertiesChanged(ref _stopWinPriceAAfterRaw, value, nameof(StopWinPriceAAfterRaw), nameof(StopWinPriceAAfter)); }
        }
        [Column("停利價A觸發", CSVIndex = -1, WPFDisplayIndex = 19, WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public string StopWinPriceAAfter => StopWinPriceAAfterRaw == 0 ? string.Empty : StopWinATouched ? $"{StopWinPriceAAfterRaw:0.00}" : $"*{StopWinPriceAAfterRaw:0.00}";

        private string _stopWin1Before;
        [Column("停利1設定", WPFDisplayIndex = 20, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string StopWin1Before
        {
            get { return _stopWin1Before; }
            set { OnPropertyChanged(ref _stopWin1Before, value); }
        }
        private decimal _stopWin1Offset;
        [Column("停利1位移", CSVStringFormat = "0.00")]
        public decimal StopWin1Offset
        {
            get { return _stopWin1Offset; }
            set { OnPropertiesChanged(ref _stopWin1Offset, value, nameof(StopWin1Offset), nameof(StopWin1After)); }
        }
        private int _stopWin1Qty;
        [Column("停利1減倉")]
        public int StopWin1Qty
        {
            get { return _stopWin1Qty; }
            set { OnPropertiesChanged(ref _stopWin1Qty, value, nameof(StopWin1Qty), nameof(StopWin1After)); }
        }
        private string StopWin1AfterRaw =>
            BestClosePrice == 0 || StopWin1Offset == 0 ? string.Empty :
            BSEnum == OrderBS.Enum.Buy && StopWin1Offset <= 0 ? $"{BestClosePrice + StopWin1Offset:0.00} ({StopWin1Qty})" :
            BSEnum == OrderBS.Enum.Buy && StopWin1Offset > 0 ? $"{OrderPriceAfter + StopWin1Offset:0.00} ({StopWin1Qty})" :
            BSEnum == OrderBS.Enum.Sell && StopWin1Offset >= 0 ? $"{BestClosePrice + StopWin1Offset:0.00} ({StopWin1Qty})" :
            $"{OrderPriceAfter + StopWin1Offset:0.00} ({StopWin1Qty})";
        [Column("停利1觸發", CSVIndex = -1, WPFDisplayIndex = 21, WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public string StopWin1After => BestClosePrice == 0 || StopWin1Offset == 0 ? string.Empty : StopWin1Data == null ? $"*{StopWin1AfterRaw}" : $"{StopWin1AfterRaw}";

        public StrategyData StopWin1Data;

        private string _stopWin2Before;
        [Column("停利2設定", WPFDisplayIndex = 22, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string StopWin2Before
        {
            get { return _stopWin2Before; }
            set { OnPropertyChanged(ref _stopWin2Before, value); }
        }
        private decimal _stopWin2Offset;
        [Column("停利2位移", CSVStringFormat = "0.00")]
        public decimal StopWin2Offset
        {
            get { return _stopWin2Offset; }
            set { OnPropertiesChanged(ref _stopWin2Offset, value, nameof(StopWin2Offset), nameof(StopWin2After)); }
        }
        private int _stopWin2Qty;
        [Column("停利2減倉")]
        public int StopWin2Qty
        {
            get { return _stopWin2Qty; }
            set { OnPropertiesChanged(ref _stopWin2Qty, value, nameof(StopWin2Qty), nameof(StopWin2After)); }
        }
        private string StopWin2AfterRaw =>
            BestClosePrice == 0 || StopWin2Offset == 0 ? string.Empty :
            BSEnum == OrderBS.Enum.Buy && StopWin2Offset <= 0 ? $"{BestClosePrice + StopWin2Offset:0.00} ({StopWin2Qty})" :
            BSEnum == OrderBS.Enum.Buy && StopWin2Offset > 0 ? $"{OrderPriceAfter + StopWin2Offset:0.00} ({StopWin2Qty})" :
            BSEnum == OrderBS.Enum.Sell && StopWin2Offset >= 0 ? $"{BestClosePrice + StopWin2Offset:0.00} ({StopWin2Qty})" :
            $"{OrderPriceAfter + StopWin2Offset:0.00} ({StopWin2Qty})";
        [Column("停利2觸發", CSVIndex = -1, WPFDisplayIndex = 23, WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public string StopWin2After => BestClosePrice == 0 || StopWin2Offset == 0 ? string.Empty : StopWin2Data == null ? $"*{StopWin2AfterRaw}" : $"{StopWin2AfterRaw}";

        public StrategyData StopWin2Data;

        private string _orderReport;
        [Column("13碼委託序號或錯誤訊息", "委託回報", WPFDisplayIndex = 24)]
        public string OrderReport
        {
            get { return _orderReport; }
            set { OnPropertyChanged(ref _orderReport, value); }
        }

        private decimal _dealPrice;
        [Column("成交價格", "成價", CSVStringFormat = "0.00", WPFDisplayIndex = 25, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal DealPrice
        {
            get { return _dealPrice; }
            set { OnPropertyChanged(ref _dealPrice, value); }
        }

        private int _dealQty;
        [Column("成交量", "成量", WPFDisplayIndex = 26, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int DealQty
        {
            get { return _dealQty; }
            set { OnPropertyChanged(ref _dealQty, value); }
        }

        private string _dealReport;
        [Column("成交序號或錯誤訊息", "成交序號", WPFDisplayIndex = 27)]
        public string DealReport
        {
            get { return _dealReport; }
            set { OnPropertyChanged(ref _dealReport, value); }
        }

        public List<decimal> ClosedProfitList { get; set; }
        public decimal ClosedProfitTotalRaw;

        private string _closedProfitTotal;
        [Column("累計已實現損益估計", "累損益", CSVStringFormat = "0.00", WPFDisplayIndex = 28, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public string ClosedProfitTotal
        {
            get { return _closedProfitTotal; }
            set { OnPropertyChanged(ref _closedProfitTotal, value); }
        }

        private decimal _closedProfit;
        [Column("已實現損益估計", "已損益", CSVStringFormat = "0.00", WPFDisplayIndex = 29, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal ClosedProfit
        {
            get { return _closedProfit; }
            set { OnPropertyChanged(ref _closedProfit, value); }
        }

        private int _unclosedQty;
        [Column("未平倉量", "未平量", WPFDisplayIndex = 30, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
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
        [Column("未實現損益估計", "未損益", CSVStringFormat = "0.00", WPFDisplayIndex = 31, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal UnclosedProfit
        {
            get { return _unclosedProfit; }
            set { OnPropertyChanged(ref _unclosedProfit, value); }
        }

        private string _openTriggerAfterStopLoss;
        [Column("停損後接續執行觸價", "停損後觸價", WPFDisplayIndex = 32)]
        public string OpenTriggerAfterStopLoss
        {
            get { return _openTriggerAfterStopLoss; }
            set { OnPropertyChanged(ref _openTriggerAfterStopLoss, value); }
        }

        private string _openStrategyAfterStopLoss;
        [Column("停損後接續執行策略", "停損後策略", WPFDisplayIndex = 33)]
        public string OpenStrategyAfterStopLoss
        {
            get { return _openStrategyAfterStopLoss; }
            set { OnPropertyChanged(ref _openStrategyAfterStopLoss, value); }
        }

        private string _openTriggerAfterStopWin;
        [Column("停利後接續執行觸價", "停利後觸價", WPFDisplayIndex = 34)]
        public string OpenTriggerAfterStopWin
        {
            get { return _openTriggerAfterStopWin; }
            set { OnPropertyChanged(ref _openTriggerAfterStopWin, value); }
        }

        private string _openStrategyAfterStopWin;
        [Column("停利後接續執行策略", "停利後策略", WPFDisplayIndex = 35)]
        public string OpenStrategyAfterStopWin
        {
            get { return _openStrategyAfterStopWin; }
            set { OnPropertyChanged(ref _openStrategyAfterStopWin, value); }
        }

        private string _closeTriggerAfterStopWin;
        [Column("停利後停止觸價", "停止觸價", WPFDisplayIndex = 36)]
        public string CloseTriggerAfterStopWin
        {
            get { return _closeTriggerAfterStopWin; }
            set { OnPropertyChanged(ref _closeTriggerAfterStopWin, value); }
        }

        private string _closeStrategyAfterStopWin;
        [Column("停利後停止策略", "停止策略", WPFDisplayIndex = 37)]
        public string CloseStrategyAfterStopWin
        {
            get { return _closeStrategyAfterStopWin; }
            set { OnPropertyChanged(ref _closeStrategyAfterStopWin, value); }
        }

        private int _winCloseQty;
        [Column("收盤獲利減倉量", "收獲減倉", WPFDisplayIndex = 38, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int WinCloseQty
        {
            get { return _winCloseQty; }
            set { OnPropertyChanged(ref _winCloseQty, value); }
        }

        private int _winCloseSeconds;
        [Column("收盤前幾秒獲利減倉")]
        public int WinCloseSeconds
        {
            get { return _winCloseSeconds; }
            set { OnPropertyChanged(ref _winCloseSeconds, Math.Abs(value)); }
        }
        private DateTime _winCloseTime;
        [Column("收盤獲利減倉時間", "收獲時間", CSVIndex = -1, WPFDisplayIndex = 39, WPFStringFormat = "{0:MM/dd HH:mm:ss}")]
        public DateTime WinCloseTime
        {
            get { return _winCloseTime; }
            set { OnPropertyChanged(ref _winCloseTime, value); }
        }

        private int _lossCloseQty;
        [Column("收盤損失減倉量", "收損減倉", WPFDisplayIndex = 40, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int LossCloseQty
        {
            get { return _lossCloseQty; }
            set { OnPropertyChanged(ref _lossCloseQty, value); }
        }

        private int _lossCloseSeconds;
        [Column("收盤前幾秒損失減倉")]
        public int LossCloseSeconds
        {
            get { return _lossCloseSeconds; }
            set { OnPropertyChanged(ref _lossCloseSeconds, Math.Abs(value)); }
        }
        private DateTime _lossCloseTime;
        [Column("收盤損失減倉時間", "收損時間", CSVIndex = -1, WPFDisplayIndex = 41, WPFStringFormat = "{0:MM/dd HH:mm:ss}")]
        public DateTime LossCloseTime
        {
            get { return _lossCloseTime; }
            set { OnPropertyChanged(ref _lossCloseTime, value); }
        }

        private string _accountsWinLossClose;
        [Column("帳號判斷獲利或損失", "帳獲損", WPFDisplayIndex = 42)]
        public string AccountsWinLossClose
        {
            get { return _accountsWinLossClose; }
            set { OnPropertyChanged(ref _accountsWinLossClose, value); }
        }

        public StrategyData MarketClosingData;

        private int _startTimesMax;
        [Column("啟動次數限制", "限", WPFDisplayIndex = 43, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public int StartTimesMax
        {
            get { return _startTimesMax; }
            set { OnPropertyChanged(ref _startTimesMax, value); }
        }

        private bool _sendRealOrder;
        [Column("真實下單", "實單", WPFDisplayIndex = 44, WPFForeground = "MediumBlue")]
        public bool SendRealOrder
        {
            get { return _sendRealOrder; }
            set { OnPropertyChanged(ref _sendRealOrder, value); }
        }

        private string _comment;
        [Column("註解", WPFDisplayIndex = 45)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public StrategyData([CallerMemberName] in string memberName = "")
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
            ProfitDirection = BSEnum == OrderBS.Enum.Buy ? 1 : -1;
            TradeTypeEnum = OrderTradeType.Enum.ROD;
            DayTradeEnum = OrderDayTrade.Enum.No;
            PositionEnum = OrderPosition.Enum.Open;
            MarketPrice = 0;
            OrderPriceBefore = OrderPrice.P;
            OrderPriceAfter = 0;
            OrderQty = -1;
            OrderData = null;
            BestClosePrice = 0;
            StopLossBefore = string.Empty;
            StopLossAfterRaw = 0;
            StopLossData = null;
            StopWinPriceABefore = string.Empty;
            StopWinATouched = false;
            StopWinPriceAAfterRaw = 0;
            StopWin1Before = string.Empty;
            StopWin1Offset = 0;
            StopWin1Qty = 0;
            StopWin1Data = null;
            StopWin2Before = string.Empty;
            StopWin2Offset = 0;
            StopWin2Qty = 0;
            StopWin2Data = null;
            OrderReport = string.Empty;
            DealPrice = 0;
            DealQty = 0;
            DealReport = string.Empty;
            ClosedProfitList = new List<decimal>();
            ClosedProfitTotalRaw = 0;
            ClosedProfitTotal = string.Empty;
            ClosedProfit = 0;
            UnclosedQty = 0;
            UnclosedProfit = 0;
            OpenTriggerAfterStopLoss = string.Empty;
            OpenStrategyAfterStopLoss = string.Empty;
            OpenTriggerAfterStopWin = string.Empty;
            OpenStrategyAfterStopWin = string.Empty;
            CloseTriggerAfterStopWin = string.Empty;
            CloseStrategyAfterStopWin = string.Empty;
            WinCloseQty = 0;
            WinCloseSeconds = 0;
            WinCloseTime = DateTime.MinValue;
            LossCloseQty = 0;
            LossCloseSeconds = 0;
            LossCloseTime = DateTime.MinValue;
            AccountsWinLossClose = string.Empty;
            MarketClosingData = null;
            StartTimesMax = 9;
            SendRealOrder = false;
            Comment = string.Empty;
        }

        public void Trim([CallerMemberName] in string memberName = "")
        {
            PrimaryKey = PrimaryKey.Replace(" ", string.Empty);
            Branch = Branch.Replace(" ", string.Empty);
            Account = Account.Replace(" ", string.Empty);
            Symbol = Symbol.Replace(" ", string.Empty);
            OrderPriceBefore = OrderPriceBefore.Replace(" ", string.Empty);
            StopLossBefore = StopLossBefore.Replace(" ", string.Empty);
            StopWinPriceABefore = StopWinPriceABefore.Replace(" ", string.Empty);
            StopWin1Before = StopWin1Before.Replace(" ", string.Empty);
            StopWin2Before = StopWin2Before.Replace(" ", string.Empty);
            OrderReport = OrderReport.Replace(" ", string.Empty);
            DealReport = DealReport.Replace(" ", string.Empty);
            OpenTriggerAfterStopLoss = OpenTriggerAfterStopLoss.Replace(" ", string.Empty).JoinSortedSet(',');
            OpenStrategyAfterStopLoss = OpenStrategyAfterStopLoss.Replace(" ", string.Empty).JoinSortedSet(',');
            OpenTriggerAfterStopWin = OpenTriggerAfterStopWin.Replace(" ", string.Empty).JoinSortedSet(',');
            OpenStrategyAfterStopWin = OpenStrategyAfterStopWin.Replace(" ", string.Empty).JoinSortedSet(',');
            CloseTriggerAfterStopWin = CloseTriggerAfterStopWin.Replace(" ", string.Empty).JoinSortedSet(',');
            CloseStrategyAfterStopWin = CloseStrategyAfterStopWin.Replace(" ", string.Empty).JoinSortedSet(',');
            AccountsWinLossClose = AccountsWinLossClose.Replace(" ", string.Empty).JoinSortedSet(',');
            Comment = Comment.Replace(" ", string.Empty);

            Updater = memberName;
            UpdateTime = DateTime.Now;
        }

        public void Reset([CallerMemberName] in string memberName = "")
        {
            if (UnclosedQty > 0)
            {
                throw new ArgumentException(ToLog());
            }

            StatusEnum = StrategyStatus.Enum.Waiting;
            MarketPrice = 0;
            OrderPriceAfter = 0;
            OrderData = null;
            BestClosePrice = 0;
            StopLossAfterRaw = 0;
            StopLossData = null;
            StopWinATouched = false;
            StopWinPriceAAfterRaw = 0;
            StopWin1Offset = 0;
            StopWin1Data = null;
            StopWin2Offset = 0;
            StopWin2Data = null;
            ClosedProfit = 0;
            UnclosedQty = 0;
            MarketClosingData = null;
            Comment = string.Empty;

            Updater = memberName;
            UpdateTime = DateTime.Now;
        }

        public StrategyData CreateOrder([CallerMemberName] in string memberName = "")
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (OrderData != null)
            {
                throw new ArgumentException($"OrderData != null|{OrderData.ToLog()}");
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
            else if (PositionEnum != OrderPosition.Enum.Open)
            {
                throw new ArgumentException($"PositionEnum != OrderPosition.Enum.Open|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{DateTime.Now:HHmmss}_{StrategyStatus.Enum.OrderSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum,
                TradeTypeEnum = TradeTypeEnum,
                DayTradeEnum = DayTradeEnum,
                PositionEnum = PositionEnum,
                OrderPriceBefore = OrderPriceBefore,
                OrderPriceAfter = OrderPriceAfter,
                OrderQty = OrderQty,
                SendRealOrder = SendRealOrder,
                Updater = memberName,
                UpdateTime = DateTime.Now,
            };

            OrderData = order;

            return order;
        }

        public StrategyData CreateStopLossOrder([CallerMemberName] in string memberName = "")
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (StopLossData != null)
            {
                throw new ArgumentException($"StopLossData != null|{StopLossData.ToLog()}");
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{DateTime.Now:HHmmss}_{StrategyStatus.Enum.StopLossSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeTypeEnum = OrderTradeType.Enum.IOC,
                DayTradeEnum = DayTradeEnum,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPriceBefore = OrderPrice.P,
                OrderPriceAfter = 0,
                OrderQty = OrderQty,
                SendRealOrder = SendRealOrder,
                Updater = memberName,
                UpdateTime = DateTime.Now,
            };

            StopLossData = order;

            return order;
        }

        public StrategyData CreateStopWinOrder(in int number, [CallerMemberName] in string memberName = "")
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (number == StopWin1 && StopWin1Data != null)
            {
                throw new ArgumentException($"StopWin{number}Data != null|{StopWin1Data.ToLog()}");
            }
            else if (number != StopWin1 && StopWin2Data != null)
            {
                throw new ArgumentException($"StopWin{number}Data != null|{StopWin2Data.ToLog()}");
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{DateTime.Now:HHmmss}_{StrategyStatus.Enum.StopWinSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeTypeEnum = OrderTradeType.Enum.IOC,
                DayTradeEnum = DayTradeEnum,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPriceBefore = OrderPrice.P,
                OrderPriceAfter = 0,
                OrderQty = Math.Abs(number == StopWin1 ? StopWin1Qty : StopWin2Qty),
                SendRealOrder = SendRealOrder,
                Updater = memberName,
                UpdateTime = DateTime.Now,
            };

            if (number == StopWin1)
            {
                StopWin1Data = order;
            }
            else
            {
                StopWin2Data = order;
            }

            return order;
        }

        public StrategyData CreateMarketClosingOrder(in int qty, [CallerMemberName] in string memberName = "")
        {
            if (Parent != null)
            {
                throw new ArgumentException($"Parent != null|{Parent.ToLog()}");
            }
            else if (MarketClosingData != null)
            {
                throw new ArgumentException($"MarketClosingData != null|{MarketClosingData.ToLog()}");
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
            else if (PositionEnum == OrderPosition.Enum.Close)
            {
                throw new ArgumentException($"PositionEnum == OrderPosition.Enum.Close|{ToLog()}");
            }
            else if (OrderQty <= 0)
            {
                throw new ArgumentException($"委託量({OrderQty}) <= 0|{ToLog()}");
            }

            StrategyData order = new StrategyData()
            {
                Parent = this,
                PrimaryKey = $"{PrimaryKey}_{DateTime.Now:HHmmss}_{StrategyStatus.Enum.MarketClosingSent}",
                MarketType = MarketType,
                Branch = Branch,
                Account = Account,
                Quote = Quote,
                Symbol = Symbol,
                BSEnum = BSEnum == OrderBS.Enum.Buy ? OrderBS.Enum.Sell : OrderBS.Enum.Buy,
                TradeTypeEnum = OrderTradeType.Enum.IOC,
                DayTradeEnum = DayTradeEnum,
                PositionEnum = OrderPosition.Enum.Close,
                OrderPriceBefore = OrderPrice.P,
                OrderPriceAfter = 0,
                OrderQty = 0,
                SendRealOrder = SendRealOrder,
                Updater = memberName,
                UpdateTime = DateTime.Now,
            };

            //負值減倉
            order.OrderQty = (qty < 0) ? qty * -1 : UnclosedQty - qty;

            if (order.OrderQty <= 0)
            {
                //正值留倉
                order.OrderQty = 0;
            }
            else if (order.OrderQty > UnclosedQty)
            {
                order.OrderQty = UnclosedQty;
                MarketClosingData = order;
            }
            else
            {
                MarketClosingData = order;
            }

            return order;
        }

        public void SumClosedProfit(decimal addValue)
        {
            ClosedProfitList.Add(addValue);
            ClosedProfitTotalRaw += addValue;

            ClosedProfitTotal = $"{ClosedProfitTotalRaw:0.00}=";

            foreach (decimal profit in ClosedProfitList)
            {
                ClosedProfitTotal = profit > 0 ? $"{ClosedProfitTotal}+{profit:0.00}" : $"{ClosedProfitTotal}{profit:0.00}";
            }
        }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},{MarketType},{Account},{Symbol},{BSEnum},{ProfitDirection},{PositionEnum},{OrderPriceBefore},{OrderPriceAfter:0.00},{OrderQty},{StartTimesMax},{SendRealOrder},{Comment}";
        }

        public string ToCSVString()
        {
            string result = string.Join("\",\"", ColumnGetters.Values.Select(x => x.Item2.ValueToString(this, x.Item1.CSVStringFormat)));
            return $"\"{result}\"";
        }

        public void ToCSVFile(in string path, in bool append = true)
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

        public void SetValues(in IList<string> columnNames, in IList<string> cells)
        {
            for (int i = 0; i < columnNames.Count; ++i)
            {
                if (ColumnSetters.TryGetValue(columnNames[i], out (ColumnAttribute, PropertyInfo) value))
                {
                    value.Item2.SetValueFromString(this, cells[i], value.Item1.CSVStringFormat);
                }
            }
        }

        public static StrategyData Create(in IList<string> columnNames, in string lineCSV)
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
