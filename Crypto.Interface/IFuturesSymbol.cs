using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface IFuturesSymbol: ISymbol
    {

        public int LeverageMax { get; }
        public int LeverageMin { get; }

        public decimal FeeMaker { get; }    
        public decimal FeeTaker { get; }
    }
}
