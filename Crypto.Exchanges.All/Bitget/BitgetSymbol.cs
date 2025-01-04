using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetSymbol : IFuturesSymbol
    {
        public BitgetSymbol( ICryptoFuturesExchange oExchange, BitgetContract oParsed) 
        {
            Exchange = oExchange;
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = (oParsed.MaxLeverage == null? 1: (int)(oParsed.MaxLeverage.Value));
            LeverageMin = 1;
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.TakerFeeRate;    
        }

        public ICryptoFuturesExchange Exchange { get; }
        public int LeverageMax { get; }

        public int LeverageMin { get; }

        public decimal FeeMaker { get; }

        public decimal FeeTaker { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
