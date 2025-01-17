using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexSymbol : IFuturesSymbol
    {

        public CoinexSymbol(IFuturesExchange oExchange, CoinExFuturesSymbol oParsed) 
        {
            Exchange = oExchange;
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = oParsed.Leverage.Max();
            LeverageMin = oParsed.Leverage.Min();   
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.MakerFeeRate;    
            Decimals = oParsed.QuantityPrecision;
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
