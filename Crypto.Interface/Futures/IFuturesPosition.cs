using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{

    /// <summary>
    /// Position data
    /// </summary>
    public interface IFuturesPosition
    {
        public IFuturesSymbol Symbol { get; }   
        public string Id { get; }   

        public FuturesPositionDirection Direction { get; }

        public int Leverage { get; }    
        public decimal Quantity { get; }    
        public decimal AveragePrice { get; }        

        public decimal ProfitRealized { get; }

        public decimal ProfitUnRealized { get; }

        public void Update(IFuturesPosition oPosition);
    }
}
