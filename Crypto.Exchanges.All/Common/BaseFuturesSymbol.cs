using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseFuturesSymbol
    {

        private string? m_strToString = null;
        public BaseFuturesSymbol(IFuturesExchange oExchange) 
        { 
            Exchange = oExchange;   
        }
        public IFuturesExchange Exchange { get; }
        public int LeverageMax { get; internal set; }

        public int LeverageMin { get; internal set; } = 1;

        public decimal FeeMaker { get; internal set; }

        public decimal FeeTaker { get; internal set; }

        public string Symbol { get; internal set; } = string.Empty;

        public string Base { get; internal set; } = string.Empty;

        public string Quote { get; internal set; } = string.Empty;
        public int Decimals { get; internal set; }
        public decimal ContractSize { get; internal set; } = 1;
        public bool UseContractSize { get; internal set; } = false;
        public int QuantityDecimals { get; internal set; } = 0; 
        public decimal Minimum { get; internal set; }

        public override string ToString()
        {
            if( m_strToString == null)
            {
                m_strToString = $"({Exchange.ExchangeType.ToString()}) {Symbol}";
            }
            return m_strToString;
        }
    }
}
