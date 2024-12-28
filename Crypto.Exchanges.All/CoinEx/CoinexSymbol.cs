using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexSymbol : IFuturesSymbol
    {

        public CoinexSymbol(CoinExFuturesSymbol oParsed) 
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = oParsed.Leverage.Max();
            LeverageMin = oParsed.Leverage.Min();   
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.MakerFeeRate;    
        }
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
