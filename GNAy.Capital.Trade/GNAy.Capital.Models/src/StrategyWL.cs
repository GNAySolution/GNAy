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
    public class StrategyWL
    {
        public enum Enum
        {
            [Description("Win")]
            Win, //0

            [Description("Loss")]
            Loss, //1
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Win.GetDescription(),
            Enum.Loss.GetDescription(),
        }.AsReadOnly();
    }
}
