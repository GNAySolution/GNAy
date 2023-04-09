using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48.Models
{
    [Serializable]
    public class AppLog : NotifyPropertyChanged
    {
        [Column("建立者")]
        public virtual string Creator { get; set; }

        [Column("日期", CSVIndex = -1)]
        public virtual DateTime CreatedDate => CreatedTime.Date;

        protected DateTime _createdTime;
        [Column("時間")]
        public virtual DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        [Column("專案")]
        public virtual string Project { get; set; }

        [Column("等級")]
        public virtual string Level { get; set; }

        [Column("訊息")]
        public virtual string Message { get; set; }

        public AppLog([CallerMemberName] in string memberName = "")
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Project = string.Empty;
            Level = string.Empty;
            Message = string.Empty;
        }
    }
}
