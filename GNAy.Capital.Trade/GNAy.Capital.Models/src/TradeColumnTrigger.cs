using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class TradeColumnTrigger : NotifyPropertyChanged
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

        public string Property { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public int Index { get; private set; }
        public bool Trigger { get; private set; }

        public string DisplayName => $"{Name},{Property}";
        public string ToolTip => ToString();

        public TradeColumnTrigger(string propertyName, TradeColumnAttribute attr)
        {
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Property = propertyName;
            Name = attr.Name;
            ShortName = attr.ShortName;
            Index = attr.Index;
            Trigger = attr.Trigger;
        }

        private TradeColumnTrigger() : this(string.Empty, null)
        { }

        public override string ToString()
        {
            return string.Join(",", Property, Name, ShortName, Index, Trigger);
        }
    }
}
