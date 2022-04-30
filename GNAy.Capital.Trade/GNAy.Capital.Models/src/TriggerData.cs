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
    public class TriggerData : NotifyPropertyChanged
    {
        /// <summary>
        /// 大於或等於
        /// </summary>
        public const string IsGreaterThanOrEqualTo = ">=";

        /// <summary>
        /// 大於
        /// </summary>
        public const string IsGreaterThan = ">";

        /// <summary>
        /// 等於
        /// </summary>
        public const string IsEqualTo = "=";

        /// <summary>
        /// 小於或等於
        /// </summary>
        public const string IsLessThanOrEqualTo = "<=";

        /// <summary>
        /// 小於
        /// </summary>
        public const string IsLessThan = "<";

        public static readonly Dictionary<string, TradeColumnTrigger> QuoteColumnTriggerMap = QuoteData.PropertyMap.Values.Where(x => x.Item1.IsTrigger).ToDictionary(x => x.Item2.Name, x => new TradeColumnTrigger(x.Item1, x.Item2));
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(TriggerData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(TriggerData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(TriggerData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        public static readonly string CSVColumnNames = string.Join(",", ColumnGetters.Values.Select(x => x.Item1.CSVName));

        public readonly object SyncRoot;

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
        public TriggerStatus.Enum StatusEnum
        {
            get { return (TriggerStatus.Enum)Status; }
            set { Status = (int)value; }
        }

        [Column("狀態描述", "狀態", WPFDisplayIndex = 2)]
        public string StatusDes => TriggerStatus.Description[Status];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", WPFDisplayIndex = 3)]
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value); }
        }

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", WPFDisplayIndex = 4)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        public TradeColumnTrigger Column { get; private set; }

        [Column("欄位", WPFDisplayIndex = 5)]
        public string ColumnName => Column.Attribute.CSVName;

        [Column("屬性", WPFDisplayIndex = 6)]
        public string ColumnProperty => Column.Property.Name;

        private decimal _columnValue;
        [Column("欄位值", CSVStringFormat = "0.00####", WPFDisplayIndex = 7, WPFStringFormat = "{0:0.00####}")]
        public decimal ColumnValue
        {
            get { return _columnValue; }
            set { OnPropertyChanged(ref _columnValue, value); }
        }

        private string _rule;
        [Column("條件", WPFDisplayIndex = 8)]
        public string Rule
        {
            get { return _rule; }
            set { OnPropertyChanged(ref _rule, value); }
        }

        private decimal _targetValue;
        [Column("目標值", CSVStringFormat = "0.00####", WPFDisplayIndex = 9, WPFStringFormat = "{0:0.00####}")]
        public decimal TargetValue
        {
            get { return _targetValue; }
            set { OnPropertyChanged(ref _targetValue, value); }
        }

        private int _cancel;
        [Column("觸價取消索引")]
        public int Cancel
        {
            get { return _cancel; }
            set { OnPropertiesChanged(ref _cancel, value, nameof(Cancel), nameof(CancelDes)); }
        }
        public TriggerCancel.Enum CancelEnum
        {
            get { return (TriggerCancel.Enum)Cancel; }
            set { Cancel = (int)value; }
        }

        [Column("觸價取消描述", "觸價後取消", WPFDisplayIndex = 10)]
        public string CancelDes => TriggerCancel.Description[Cancel];

        private string _strategyOR;
        [Column("滿足單一條件即執行策略", "執行策略OR", WPFDisplayIndex = 11)]
        public string StrategyOR
        {
            get { return _strategyOR; }
            set { OnPropertyChanged(ref _strategyOR, value); }
        }

        private string _strategyAND;
        [Column("滿足全部條件再執行策略", "執行策略AND", WPFDisplayIndex = 12)]
        public string StrategyAND
        {
            get { return _strategyAND; }
            set { OnPropertyChanged(ref _strategyAND, value); }
        }

        private DateTime? _startTime;
        [Column("監控開始", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 13, WPFStringFormat = "{0:MM/dd HH:mm:ss}")]
        public DateTime? StartTime
        {
            get { return _startTime; }
            set { OnPropertyChanged(ref _startTime, value); }
        }

        private DateTime? _endTime;
        [Column("監控結束", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 14, WPFStringFormat = "{0:MM/dd HH:mm:ss}")]
        public DateTime? EndTime
        {
            get { return _endTime; }
            set { OnPropertyChanged(ref _endTime, value); }
        }

        private string _comment;
        [Column("註解", WPFDisplayIndex = 15)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public TriggerData(TradeColumnTrigger column, [CallerMemberName] string memberName = "")
        {
            SyncRoot = new object();
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusEnum = TriggerStatus.Enum.Waiting;
            PrimaryKey = string.Empty;
            Quote = null;
            Symbol = string.Empty;
            Column = column;
            ColumnValue = 0;
            Rule = string.Empty;
            TargetValue = 0;
            CancelEnum = TriggerCancel.Enum.SameSymbolSameColumn;
            StrategyOR = string.Empty;
            StrategyAND = string.Empty;
            StartTime = null;
            EndTime = null;
            Comment = string.Empty;
        }

        private TriggerData() : this(null, null)
        { }

        public TriggerData Trim()
        {
            PrimaryKey = PrimaryKey.Replace(" ", string.Empty);
            Symbol = Symbol.Replace(" ", string.Empty);
            Rule = Rule.Replace(" ", string.Empty);
            StrategyOR = StrategyOR.Replace(" ", string.Empty);
            StrategyAND = StrategyAND.Replace(" ", string.Empty);
            Comment = Comment.Replace(" ", string.Empty);

            return this;
        }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},{Symbol},{ColumnProperty}({ColumnName}),{CancelDes},{Comment}";
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
                    value.Item2.SetValueFromString(this, cells.Count > i ? cells[i] : null, value.Item1.CSVStringFormat);
                }
            }
        }

        public static TriggerData Create(IList<string> columnNames, string lineCSV, int propertyIndex)
        {
            string[] cells = lineCSV.SplitToCSV();
            string propertyName = cells[propertyIndex];

            TriggerData data = new TriggerData(QuoteColumnTriggerMap[propertyName]);
            data.SetValues(columnNames, cells);

            return data;
        }

        public static IEnumerable<TriggerData> ForeachQuoteFromCSVFile(string quotePath, List<string> columnNames)
        {
            int propertyIndex = -1;

            foreach (string line in File.ReadLines(quotePath, TextEncoding.UTF8WithoutBOM))
            {
                if (columnNames.Count <= 0)
                {
                    columnNames.AddRange(line.Split(','));
                    propertyIndex = columnNames.FindIndex(x => x == PropertyMap[nameof(ColumnProperty)].Item1.CSVName);
                    continue;
                }

                TriggerData data = Create(columnNames, line, propertyIndex);
                yield return data;
            }
        }
    }
}
