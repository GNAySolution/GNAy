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
    public class StrategyStartPrinciple
    {
        public enum Enum
        {
            [Description("OR")]
            OR, //0

            [Description("AND")]
            AND, //1
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.OR.GetDescription(),
            Enum.AND.GetDescription(),
        }.AsReadOnly();
    }
}
