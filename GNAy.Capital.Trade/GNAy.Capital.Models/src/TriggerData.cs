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
                    OnPropertyChanged(nameof(StatusStr));
                }
            }
        }

        [Column("狀態描述", "狀態", 5)]
        public string StatusStr => Definition.TriggerStatusKinds[StatusIndex];

        private OrderAccData _orderAccData;
        [Column("下單帳號", 6)]
        public string OrderAcc => _orderAccData.Account;

        public QuoteData Quote;

        private string _symbol;
        [Column("代碼", 7)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private TradeColumnTrigger _column;

        [Column("欄位", 8)]
        public string ColumnName => _column.Name;

        [Column("屬性", 9)]
        public string ColumnProperty => _column.Property;

        private string _rule;
        [Column("條件", 10)]
        public string Rule
        {
            get { return _rule; }
            set { OnPropertyChanged(ref _rule, value); }
        }

        private decimal _value;
        [Column("目標值", 11)]
        public decimal Value
        {
            get { return _value; }
            set { OnPropertyChanged(ref _value, value); }
        }

        private int _cancelIndex;
        [Column("觸價取消索引", 12)]
        public int CancelIndex
        {
            get { return _cancelIndex; }
            set
            {
                if (OnPropertyChanged(ref _cancelIndex, value))
                {
                    OnPropertyChanged(nameof(CancelStr));
                }
            }
        }

        [Column("觸價取消描述", "觸價後取消", 13)]
        public string CancelStr => Definition.TriggerCancelKinds[CancelIndex];

        private string _strategy;
        [Column("觸價後執行", 14)]
        public string Strategy
        {
            get { return _strategy; }
            set { OnPropertyChanged(ref _strategy, value); }
        }

        private DateTime? _startTime;
        [Column("監控開始", 15, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime? StartTime
        {
            get { return _startTime; }
            set { OnPropertyChanged(ref _startTime, value); }
        }

        private DateTime? _endTime;
        [Column("監控結束", 16, StringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime? EndTime
        {
            get { return _endTime; }
            set { OnPropertyChanged(ref _endTime, value); }
        }

        public TriggerData(OrderAccData orderAcc, TradeColumnTrigger column)
        {
            SyncRoot = new object();
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Updater = String.Empty;
            UpdateTime = DateTime.MaxValue;
            StatusIndex = Definition.TriggerStatus0.Item1;
            _orderAccData = orderAcc;
            Quote = null;
            Symbol = String.Empty;
            _column = column;
            Rule = String.Empty;
            Value = 0;
            CancelIndex = Definition.TriggerCancel0.Item1;
            Strategy = String.Empty;
            StartTime = null;
            EndTime = null;
        }

        public TriggerData() : this(null, null)
        { }

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

        public static TriggerData Create(IList<string> columnNames, string lineCSV)
        {
            TriggerData data = new TriggerData();
            data.SetValues(columnNames, lineCSV.Split(Separator.CSV, StringSplitOptions.RemoveEmptyEntries));
            return data;
        }

        public static IEnumerable<TriggerData> ForeachQuoteFromCSVFile(string quotePath, List<string> columnNames)
        {
            foreach (string line in File.ReadLines(quotePath, TextEncoding.UTF8WithoutBOM))
            {
                if (columnNames.Count <= 0)
                {
                    columnNames.AddRange(line.Split(Separator.CSV));
                    continue;
                }

                TriggerData data = Create(columnNames, line);

                yield return data;
            }
        }
    }
}
