using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    /// <summary>
    /// 觸價狀態
    /// </summary>
    public static class TriggerStatus
    {
        public enum Enum
        {
            [Description("等待中")]
            Waiting, //0

            [Description("已取消")]
            Cancelled, //1

            [Description("監控中")]
            Monitoring, //2

            [Description("已觸發")]
            Executed, //3,
        }

        /// <summary>
        /// https://stackoverflow.com/questions/4680035/read-only-list-in-c-sharp
        /// </summary>
        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            Enum.Monitoring.GetDescription(),
            Enum.Executed.GetDescription(),
        }.AsReadOnly();
    }
}
