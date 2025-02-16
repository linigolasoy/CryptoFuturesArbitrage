using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class BarJson
    {


        public BarJson() 
        { 
        }

        public string Symbol { get; set; } = string.Empty;        
        public DateTime DateTime { get; set; }  

        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }

    }
}
