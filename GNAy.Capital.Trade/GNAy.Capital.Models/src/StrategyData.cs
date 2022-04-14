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
        public StrategyStatus.Enum StatusEnum
        {
            get { return (StrategyStatus.Enum)StatusIndex; }
            set { StatusIndex = (int)value; }
        }

        [Column("狀態描述", "狀態", 5)]
        public string StatusDes => StrategyStatus.Description[StatusIndex];

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
            set
            {
                if (OnPropertyChanged(ref _branch, value))
                {
                    OnPropertyChanged(nameof(FullAccount));
                }
            }
        }
        private string _account;
        [Column("下單帳號", 8)]
        public string Account
        {
            get { return _account; }
            set
            {
                if (OnPropertyChanged(ref _account, value))
                {
                    OnPropertyChanged(nameof(FullAccount));
                }
            }
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

        private int _bsIndex;
        [Column("買賣索引", 10)]
        public int BSIndex
        {
            get { return _bsIndex; }
            set
            {
                if (OnPropertyChanged(ref _bsIndex, value))
                {
                    OnPropertyChanged(nameof(BSDes));
                }
            }
        }
        public BS.Enum BSEnum
        {
            get { return (BS.Enum)BSIndex; }
            set { BSIndex = (int)value; }
        }

        [Column("買賣描述", "買賣", 11)]
        public string BSDes => BS.Description[BSIndex];

        private int _tradeTypeIndex;
        [Column("掛單索引", 12)]
        public int TradeTypeIndex
        {
            get { return _tradeTypeIndex; }
            set
            {
                if (OnPropertyChanged(ref _tradeTypeIndex, value))
                {
                    OnPropertyChanged(nameof(TradeTypeDes));
                }
            }
        }
        public TradeType.Enum TradeTypeEnum
        {
            get { return (TradeType.Enum)TradeTypeIndex; }
            set { TradeTypeIndex = (int)value; }
        }

        [Column("掛單描述", "掛單", 13)]
        public string TradeTypeDes => TradeType.Description[TradeTypeIndex];

        private int _dayTradeIndex;
        [Column("當沖索引", 14)]
        public int DayTradeIndex
        {
            get { return _dayTradeIndex; }
            set
            {
                if (OnPropertyChanged(ref _dayTradeIndex, value))
                {
                    OnPropertyChanged(nameof(DayTradeDes));
                }
            }
        }
        public DayTrade.Enum DayTradeEnum
        {
            get { return (DayTrade.Enum)DayTradeIndex; }
            set { DayTradeIndex = (int)value; }
        }

        [Column("當沖描述", "沖", 15)]
        public string DayTradeDes => DayTrade.Description[DayTradeIndex];

        private int _positionKindIndex;
        [Column("新倉平倉索引", 16)]
        public int PositionKindIndex
        {
            get { return _positionKindIndex; }
            set
            {
                if (OnPropertyChanged(ref _positionKindIndex, value))
                {
                    OnPropertyChanged(nameof(PositionKindDes));
                }
            }
        }
        public PositionKind.Enum PositionKindEnum
        {
            get { return (PositionKind.Enum)PositionKindIndex; }
            set { PositionKindIndex = (int)value; }
        }

        [Column("新倉平倉描述", "新平倉", 17)]
        public string PositionKindDes => PositionKind.Description[PositionKindIndex];

        private string _price;
        [Column("委託價格", "委託價", 18)]
        public string Price
        {
            get { return _price; }
            set { OnPropertyChanged(ref _price, value); }
        }

        private string _quantity;
        [Column("委託口數", "委託量", 19)]
        public string Quantity
        {
            get { return _quantity; }
            set { OnPropertyChanged(ref _quantity, value); }
        }

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
            //
            Quote = null;
            Symbol = string.Empty;
            //
            Comment = string.Empty;
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
    }
}
