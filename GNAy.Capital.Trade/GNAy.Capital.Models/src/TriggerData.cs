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
    public class TriggerData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, TradeColumnTrigger> QuoteColumnTriggerMap = QuoteData.PropertyMap.Values.Where(x => x.Item1.IsTrigger).ToDictionary(x => x.Item2.Name, x => new TradeColumnTrigger(x.Item1, x.Item2));
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(TriggerData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        public static readonly SortedDictionary<int, (ColumnAttribute, PropertyInfo)> ColumnGetters = typeof(TriggerData).GetColumnAttrMapByIndex<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> ColumnSetters = typeof(TriggerData).GetColumnAttrMapByName<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
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
            set
            {
                if (OnPropertyChanged(ref _createdTime, value))
                {
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
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
            set
            {
                if (OnPropertyChanged(ref _updateTime, value))
                {
                    OnPropertyChanged(nameof(UpdateDate));
                }
            }
        }

        private int _statusIndex;
        [Column("狀態索引", 4)]
        public int StatusIndex
        {
            get { return _statusIndex; }
            set
            {
                if (OnPropertyChanged(ref _statusIndex, value))
                {
                    OnPropertyChanged(nameof(StatusDes));
                }
            }
        }
        public TriggerStatus.Enum StatusEnum
        {
            get { return (TriggerStatus.Enum)StatusIndex; }
            set { StatusIndex = (int)value; }
        }

        [Column("狀態描述", "狀態", 5)]
        public string StatusDes => TriggerStatus.Description[StatusIndex];

        private string _primaryKey;
        [Column("自定義唯一鍵", "唯一鍵", 6)]
        public string PrimaryKey
        {
            get { return _primaryKey; }
            set { OnPropertyChanged(ref _primaryKey, value); }
        }

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", 7)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        public TradeColumnTrigger Column { get; private set; }

        [Column("欄位", 8)]
        public string ColumnName => Column.Attribute.Name;

        [Column("屬性", 9)]
        public string ColumnProperty => Column.Property.Name;

        private decimal _columnValue;
        [Column("欄位值", 10)]
        public decimal ColumnValue
        {
            get { return _columnValue; }
            set { OnPropertyChanged(ref _columnValue, value); }
        }

        private string _rule;
        [Column("條件", 11)]
        public string Rule
        {
            get { return _rule; }
            set { OnPropertyChanged(ref _rule, value); }
        }

        private decimal _targetValue;
        [Column("目標值", 12)]
        public decimal TargetValue
        {
            get { return _targetValue; }
            set { OnPropertyChanged(ref _targetValue, value); }
        }

        private int _cancelIndex;
        [Column("觸價取消索引", 13)]
        public int CancelIndex
        {
            get { return _cancelIndex; }
            set
            {
                if (OnPropertyChanged(ref _cancelIndex, value))
                {
                    OnPropertyChanged(nameof(CancelDes));
                }
            }
        }
        public TriggerCancel.Enum CancelEnum
        {
            get { return (TriggerCancel.Enum)CancelIndex; }
            set { CancelIndex = (int)value; }
        }

        [Column("觸價取消描述", "觸價後取消", 14)]
        public string CancelDes => TriggerCancel.Description[CancelIndex];

        private string _strategy;
        [Column("觸價後執行", 15)]
        public string Strategy
        {
            get { return _strategy; }
            set { OnPropertyChanged(ref _strategy, value); }
        }

        private DateTime? _startTime;
        [Column("監控開始", 16, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime? StartTime
        {
            get { return _startTime; }
            set { OnPropertyChanged(ref _startTime, value); }
        }

        private DateTime? _endTime;
        [Column("監控結束", 17, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime? EndTime
        {
            get { return _endTime; }
            set { OnPropertyChanged(ref _endTime, value); }
        }

        private string _comment;
        [Column("註解", 18)]
        public string Comment
        {
            get { return _comment; }
            set { OnPropertyChanged(ref _comment, value); }
        }

        public TriggerData(QuoteData quote, TradeColumnTrigger column)
        {
            SyncRoot = new object();
            Creator = string.Empty;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusEnum = TriggerStatus.Enum.Waiting;
            PrimaryKey = string.Empty;
            Quote = quote;
            Symbol = quote.Symbol;
            Column = column;
            ColumnValue = 0;
            Rule = string.Empty;
            TargetValue = 0;
            CancelEnum = TriggerCancel.Enum.SameSymbolSameColumn;
            Strategy = string.Empty;
            StartTime = null;
            EndTime = null;
            Comment = string.Empty;
        }

        private TriggerData() : this(null, null)
        { }

        public string ToLog()
        {
            return $"{StatusDes},{PrimaryKey},{ColumnProperty}({ColumnName}),{Comment}";
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
                    value.Item2.SetValueFromString(this, cells.Count > i ? cells[i] : null, value.Item1.StringFormat);
                }
            }
        }

        public static TriggerData Create(IList<string> columnNames, string lineCSV, int propertyIndex)
        {
            string[] cells = lineCSV.SplitToCSV();
            string propertyName = cells[propertyIndex];

            TriggerData data = new TriggerData(new QuoteData(), QuoteColumnTriggerMap[propertyName]);
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
                    propertyIndex = columnNames.FindIndex(x => x == PropertyMap[nameof(ColumnProperty)].Item1.Name);
                    continue;
                }

                TriggerData data = Create(columnNames, line, propertyIndex);

                yield return data;
            }
        }
    }
}
