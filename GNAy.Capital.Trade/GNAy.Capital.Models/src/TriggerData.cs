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

        private TriggerStatus.Enum _statusEnum;
        [Column("狀態索引")]
        public TriggerStatus.Enum StatusEnum
        {
            get { return _statusEnum; }
            set { OnPropertiesChanged(ref _statusEnum, value, nameof(StatusEnum), nameof(StatusDes)); }
        }
        [Column("狀態描述", "狀態", WPFDisplayIndex = 2, WPFForeground = "MediumBlue")]
        public string StatusDes => TriggerStatus.Description[(int)StatusEnum];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", WPFDisplayIndex = 3, WPFHorizontalAlignment = WPFHorizontalAlignment.Center)]
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value); }
        }

        public QuoteData Quote1;

        private string _symbol1;
        [Column("代碼1", WPFDisplayIndex = 4, WPFForeground = "MediumBlue")]
        public string Symbol1
        {
            get { return _symbol1; }
            set { OnPropertyChanged(ref _symbol1, value); }
        }

        public TradeColumnTrigger Column { get; private set; }

        [Column("欄位", WPFDisplayIndex = 5)]
        public string ColumnName => Column.Attribute.CSVName;

        [Column("屬性", WPFDisplayIndex = 6)]
        public string ColumnProperty => Column.Property.Name;

        private decimal _columnValue;
        [Column("欄位值", CSVStringFormat = "0.00####", WPFDisplayIndex = 7, WPFStringFormat = "{0:0.00####}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal ColumnValue
        {
            get { return _columnValue; }
            set { OnPropertyChanged(ref _columnValue, value); }
        }

        private string _rule;
        [Column("條件", WPFDisplayIndex = 8, WPFHorizontalAlignment = WPFHorizontalAlignment.Center)]
        public string Rule
        {
            get { return _rule; }
            set { OnPropertyChanged(ref _rule, value); }
        }

        private decimal _targetValue;
        [Column("目標值", CSVStringFormat = "0.00####", WPFDisplayIndex = 9, WPFStringFormat = "{0:0.00####}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right, WPFForeground = "MediumBlue")]
        public decimal TargetValue
        {
            get { return _targetValue; }
            set { OnPropertyChanged(ref _targetValue, value); }
        }

        public decimal Symbol2Offset;
        private string _symbol2Setting;
        [Column("代碼2設定", WPFDisplayIndex = 10, WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public string Symbol2Setting
        {
            get { return _symbol2Setting; }
            set
            {
                if (OnPropertyChanged(ref _symbol2Setting, value))
                {
                    Symbol2Offset = string.IsNullOrWhiteSpace(value) ? 0 : decimal.Parse(value);
                }
            }
        }

        private string _symbol2;
        [Column("代碼2", WPFDisplayIndex = 11, WPFForeground = "MediumBlue")]
        public string Symbol2
        {
            get { return _symbol2; }
            set { OnPropertyChanged(ref _symbol2, value); }
        }

        public QuoteData Quote2;

        private string _cancel;
        [Column("觸價後取消其他觸價監控", "後取消觸價", WPFDisplayIndex = 12)]
        public string Cancel
        {
            get { return _cancel; }
            set { OnPropertyChanged(ref _cancel, value); }
        }

        private string _start;
        [Column("觸價後啟動其他觸價監控", "後啟動觸價", WPFDisplayIndex = 13)]
        public string Start
        {
            get { return _start; }
            set { OnPropertyChanged(ref _start, value); }
        }

        private string _strategyOpenOR;
        [Column("滿足單一條件即執行策略新倉", "策略新倉OR", WPFDisplayIndex = 14, WPFForeground = "MediumBlue")]
        public string StrategyOpenOR
        {
            get { return _strategyOpenOR; }
            set { OnPropertyChanged(ref _strategyOpenOR, value); }
        }

        private string _strategyOpenAND;
        [Column("滿足全部條件再執行策略新倉", "策略新倉AND", WPFDisplayIndex = 15)]
        public string StrategyOpenAND
        {
            get { return _strategyOpenAND; }
            set { OnPropertyChanged(ref _strategyOpenAND, value); }
        }

        private string _strategyCloseOR;
        [Column("滿足單一條件即執行策略平倉", "策略平倉OR", WPFDisplayIndex = 16, WPFForeground = "MediumBlue")]
        public string StrategyCloseOR
        {
            get { return _strategyCloseOR; }
            set { OnPropertyChanged(ref _strategyCloseOR, value); }
        }

        private string _strategyCloseAND;
        [Column("滿足全部條件再執行策略平倉", "策略平倉AND", WPFDisplayIndex = 17)]
        public string StrategyCloseAND
        {
            get { return _strategyCloseAND; }
            set { OnPropertyChanged(ref _strategyCloseAND, value); }
        }

        private DateTime? _startTime;
        [Column("監控開始", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 18, WPFStringFormat = "{0:MM/dd HH:mm:ss}", WPFForeground = "MediumBlue")]
        public DateTime? StartTime
        {
            get { return _startTime; }
            set { OnPropertyChanged(ref _startTime, value); }
        }

        private DateTime? _endTime;
        [Column("監控結束", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 19, WPFStringFormat = "{0:MM/dd HH:mm:ss}")]
        public DateTime? EndTime
        {
            get { return _endTime; }
            set { OnPropertyChanged(ref _endTime, value); }
        }

        private string _comment;
        [Column("註解", WPFDisplayIndex = 20)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public TriggerData(in TradeColumnTrigger column, [CallerMemberName] in string memberName = "")
        {
            SyncRoot = new object();
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusEnum = TriggerStatus.Enum.Waiting;
            PrimaryKey = string.Empty;
            Quote1 = null;
            Symbol1 = string.Empty;
            Column = column;
            ColumnValue = 0;
            Rule = string.Empty;
            TargetValue = 0;
            Symbol2Offset = 0;
            Symbol2Setting = string.Empty;
            Symbol2 = string.Empty;
            Quote2 = null;
            Cancel = string.Empty;
            Start = string.Empty;
            StrategyOpenOR = string.Empty;
            StrategyOpenAND = string.Empty;
            StrategyCloseOR = string.Empty;
            StrategyCloseAND = string.Empty;
            StartTime = null;
            EndTime = null;
            Comment = string.Empty;
        }

        private TriggerData() : this(null, null)
        { }

        public void Trim([CallerMemberName] in string memberName = "")
        {
            PrimaryKey = PrimaryKey.Replace(" ", string.Empty);
            Symbol1 = Symbol1.Replace(" ", string.Empty);
            Rule = Rule.Replace(" ", string.Empty);
            Symbol2Setting = Symbol2Setting.Replace(" ", string.Empty);
            Symbol2 = Symbol2.Replace(" ", string.Empty);
            Cancel = Cancel.Replace(" ", string.Empty).JoinSortedSet(',');
            Start = Start.Replace(" ", string.Empty).JoinSortedSet(',');
            StrategyOpenOR = StrategyOpenOR.Replace(" ", string.Empty).JoinSortedSet(',');
            StrategyOpenAND = StrategyOpenAND.Replace(" ", string.Empty).JoinSortedSet(',');
            StrategyCloseOR = StrategyCloseOR.Replace(" ", string.Empty).JoinSortedSet(',');
            StrategyCloseAND = StrategyCloseAND.Replace(" ", string.Empty).JoinSortedSet(',');
            Comment = Comment.Replace(" ", string.Empty);

            Updater = memberName;
            UpdateTime = DateTime.Now;
        }

        public decimal GetColumnValue(in QuoteData quote)
        {
            object valueObj = Column.Property.GetValue(quote);

            if (Column.Property.PropertyType == typeof(DateTime))
            {
                return decimal.Parse(((DateTime)valueObj).ToString(Column.Attribute.TriggerFormat));
            }
            else if (Column.Property.PropertyType == typeof(string))
            {
                return decimal.Parse((string)valueObj);
            }

            return (decimal)valueObj;
        }

        public decimal GetTargetValue()
        {
            return (Quote2 == null) ? TargetValue : (GetColumnValue(Quote2) + Symbol2Offset);
        }

        public bool? IsMatchedRule(in decimal columnValue, in decimal targetValue)
        {
            if (columnValue == 0 || targetValue == 0)
            {
                return false;
            }
            else if (Rule == IsGreaterThanOrEqualTo)
            {
                return columnValue >= targetValue;
            }
            else if (Rule == IsLessThanOrEqualTo)
            {
                return columnValue <= targetValue;
            }
            else if (Rule == IsEqualTo)
            {
                return columnValue == targetValue;
            }

            return null;
        }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},{Symbol1},{Symbol2},{ColumnProperty}({ColumnName}),{Cancel},{Start},{Comment}";
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
                    value.Item2.SetValueFromString(this, cells.Count > i ? cells[i] : null, value.Item1.CSVStringFormat);
                }
            }
        }

        public static TriggerData Create(in IList<string> columnNames, in string lineCSV, in int propertyIndex)
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
