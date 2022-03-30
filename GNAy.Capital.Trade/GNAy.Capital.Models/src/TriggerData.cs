using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
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

        private bool _executing;
        [Column("執行中", 4)]
        public bool Executing
        {
            get { return _executing; }
            set { OnPropertyChanged(ref _executing, value); }
        }

        private string _orderAcc;
        [Column("下單帳號", 5)]
        public string OrderAcc
        {
            get { return _orderAcc; }
            set { OnPropertyChanged(ref _orderAcc, value); }
        }

        private string _symbol;
        [Column("代碼", 6)]
        public string Symbol
        {
            get { return _symbol; }
            set { OnPropertyChanged(ref _symbol, value); }
        }

        private string _columnName;
        [Column("欄位", 7)]
        public string ColumnName
        {
            get { return _columnName; }
            set { OnPropertyChanged(ref _columnName, value); }
        }

        private string _columnProperty;
        [Column("屬性", 8)]
        public string ColumnProperty
        {
            get { return _columnProperty; }
            set { OnPropertyChanged(ref _columnProperty, value); }
        }

        private string _rule;
        [Column("條件", 9)]
        public string Rule
        {
            get { return _rule; }
            set { OnPropertyChanged(ref _rule, value); }
        }

        private decimal _value;
        [Column("目標值", 10)]
        public decimal Value
        {
            get { return _value; }
            set { OnPropertyChanged(ref _value, value); }
        }

        private string _cancel;
        [Column("觸價後取消", 11)]
        public string Cancel
        {
            get { return _cancel; }
            set { OnPropertyChanged(ref _cancel, value); }
        }

        private string _strategy;
        [Column("觸價後執行", 12)]
        public string Strategy
        {
            get { return _strategy; }
            set { OnPropertyChanged(ref _strategy, value); }
        }

        public TriggerData()
        {
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Executing = false;
            OrderAcc = String.Empty;
            Symbol = String.Empty;
            ColumnName = String.Empty;
            ColumnProperty = String.Empty;
            Rule = String.Empty;
            Value = 0;
            Cancel = String.Empty;
            Strategy = String.Empty;
        }
    }
}
