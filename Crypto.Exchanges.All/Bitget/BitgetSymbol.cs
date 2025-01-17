using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetSymbol : IFuturesSymbol
    {
        public BitgetSymbol( IFuturesExchange oExchange, BitgetContract oParsed) 
        {
            Exchange = oExchange;
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = (oParsed.MaxLeverage == null? 1: (int)(oParsed.MaxLeverage.Value));
            LeverageMin = 1;
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.TakerFeeRate;    
            Decimals = oParsed.QuantityDecimals;
            Minimum = oParsed.MinOrderQuantity;

        }

        public IFuturesExchange Exchange { get; }
        public int LeverageMax { get; }

        public int LeverageMin { get; }

        public decimal FeeMaker { get; }

        public decimal FeeTaker { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }
        public int Decimals { get; }    
        public decimal Minimum { get; } 

        public override string ToString()
        {
            return Symbol;
        }
    }
}
