using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class StatusCode
    {
        /// <summary>
        /// 報價商品載入完成
        /// </summary>
        public const int SK_SUBJECT_CONNECTION_STOCKS_READY = 3003;

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/windows/win32/debug/system-error-codes
        /// </summary>
        public const int WinError = 16000;

        public const int BaseTraceValue = 100000;
        public const int BaseDebugValue = 200000;
        public const int BaseInfoValue = 300000;
        public const int BaseWarnValue = 400000;
        public const int BaseErrorValue = 500000;
    }
}
