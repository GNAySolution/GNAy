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
    /// 策略狀態
    /// </summary>
    public class StrategyStatus
    {
        public enum Enum
        {
            [Description("等待中")]
            Waiting, //0

            [Description("已取消")]
            Cancelled, //1

            [Description("執行中")]
            Running, //2

            [Description("已停損")]
            StoppedLoss, //3,

            [Description("已停利")]
            StoppedWin, //4,

            [Description("已移動停利")]
            MoveStoppedWin, //5,
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            Enum.Running.GetDescription(),
            Enum.StoppedLoss.GetDescription(),
            Enum.StoppedWin.GetDescription(),
            Enum.MoveStoppedWin.GetDescription(),
        }.AsReadOnly();
    }
}
