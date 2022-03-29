using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class TriggerController
    {
        public readonly DateTime CreatedTime;

        public TriggerController()
        {
            CreatedTime = DateTime.Now;
        }
    }
}
