using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Market
{
    public interface IFuturesSymbol: ISymbol
    {

        public IFuturesExchange Exchange { get; }
        public int LeverageMax { get; }
        public int LeverageMin { get; }

        public decimal FeeMaker { get; }    
        public decimal FeeTaker { get; }

        public int Decimals { get; }  
        public decimal ContractSize { get; }    
        public bool UseContractSize { get; }    
        public int QuantityDecimals { get; }    

        public decimal Minimum { get; }
    }
}
