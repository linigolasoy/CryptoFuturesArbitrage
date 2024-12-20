using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Futures
{
    internal class FuturesSymbol : IFuturesSymbol
    {
        public FuturesSymbol( FuturesSymbolParsed oParsed ) 
        { 
            Symbol = oParsed.Symbol;
            Base = oParsed.Base;
            Quote = oParsed.Quote;
            FeeMaker = (decimal)oParsed.FeeMaker * 100M;
            FeeTaker = (decimal)oParsed.FeeTaker * 100M;
        }
        public int LeverageMax { get => 20; }

        public int LeverageMin { get => 1; }

        public decimal FeeMaker { get; }

        public decimal FeeTaker { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }
    }
}
