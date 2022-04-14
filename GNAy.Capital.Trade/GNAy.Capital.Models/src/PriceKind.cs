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
    public class PriceKind
    {
        public enum Enum : short
        {
            [Description("限價")]
            L, //0

            [Description("市價")]
            M, //1

            [Description("範圍市價")]
            P, //2
        }

        public static readonly string L = string.Empty;
        public static readonly string M = Enum.M.ToString();
        public static readonly string P = Enum.P.ToString();

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.L.GetDescription(),
            Enum.M.GetDescription(),
            Enum.P.GetDescription(),
        }.AsReadOnly();
    }
}
