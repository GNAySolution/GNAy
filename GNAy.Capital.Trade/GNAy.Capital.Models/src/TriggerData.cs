using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class TriggerData : NotifyPropertyChanged
    {
        public string Creator { get; set; }

        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
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

        public string Symbol { get; set; }

        public string ColumnProperty { get; set; }

        public string ColumnName { get; set; }

        public string Rule { get; set; }

        public decimal Value { get; set; }

        public string Strategy { get; set; }

        public TriggerData()
        {
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Symbol = String.Empty;
            ColumnProperty = String.Empty;
            ColumnName = String.Empty;
            Rule = String.Empty;
            Value = 0;
            Strategy = String.Empty;
        }
    }
}
