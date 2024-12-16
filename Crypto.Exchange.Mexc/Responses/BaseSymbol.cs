using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{
    internal class BaseSymbol
    {

        public BaseSymbol( SpotSymbolParsed oParsed )
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
        }

        public BaseSymbol(FuturesSymbolParsed oParsed )
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
        }

        public string Symbol { get; }
        public string Base { get; }
        public string Quote { get; }

    }
}
