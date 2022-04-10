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
    public class TriggerCancel
    {
        public enum Enum
        {
            [Description("同代碼同欄位")]
            SameSymbolSameColumn, //0

            [Description("同代碼全欄位")]
            SameSymbolAllColumns, //1

            [Description("全代碼同欄位")]
            AllSymbolsSameColumn, //2

            [Description("全代碼全欄位")]
            AllSymbolsAllColumns, //3
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.SameSymbolSameColumn.GetDescription(),
            Enum.SameSymbolAllColumns.GetDescription(),
            Enum.AllSymbolsSameColumn.GetDescription(),
            Enum.AllSymbolsAllColumns.GetDescription(),
        }.AsReadOnly();
    }
}
