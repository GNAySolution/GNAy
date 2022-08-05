using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    /// <summary>
    /// 觸價狀態
    /// </summary>
    public class TriggerStatus
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

        public static readonly string[] Description =
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            Enum.Monitoring.GetDescription(),
            Enum.Executed.GetDescription(),
        };
    }
}
