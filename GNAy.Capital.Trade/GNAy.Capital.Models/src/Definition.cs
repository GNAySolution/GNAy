using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public static class Definition
    {
        //public const int SimulateTrade = 1;
        public const int RealTrade = 0;

        /// <summary>
        /// 試撮
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSimulating(this int obj)
        {
            return obj > RealTrade;
        }

        public static bool IsRealTrading(this int obj)
        {
            return obj == RealTrade;
        }

        /// <summary>
        /// 大於或等於
        /// </summary>
        public const string IsGreaterThanOrEqualTo = ">=";

        /// <summary>
        /// 大於
        /// </summary>
        public const string IsGreaterThan = ">";

        /// <summary>
        /// 等於
        /// </summary>
        public const string IsEqualTo = "=";

        /// <summary>
        /// 小於或等於
        /// </summary>
        public const string IsLessThanOrEqualTo = "<=";

        /// <summary>
        /// 小於
        /// </summary>
        public const string IsLessThan = "<";
    }
}
