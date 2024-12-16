using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface IFundingRate
    {
        public IFuturesSymbol Symbol { get; }   
        public decimal Rate { get; }
        public decimal Maximum { get; }
        public decimal Minimum { get; }

        public int Cycle { get; }   
        public DateTime NextSettle { get; } 
        public DateTime ActualDate { get; } 
    }
}
