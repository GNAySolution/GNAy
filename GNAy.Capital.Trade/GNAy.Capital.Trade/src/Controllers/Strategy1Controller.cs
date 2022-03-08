using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class Strategy1Controller
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly DateTime CreatedTime;

        public Strategy1Controller()
        {
            CreatedTime = DateTime.Now;
        }
    }
}
