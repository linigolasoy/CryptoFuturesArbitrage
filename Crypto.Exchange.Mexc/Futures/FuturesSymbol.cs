using Crypto.Exchange.Mexc.Responses;
using Crypto.Exchange.Mexc.Spot;
using Crypto.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Futures
{
    internal class FuturesSymbol: BaseSymbol, IFuturesSymbol
    {
        internal FuturesSymbol(FuturesSymbolParsed oParsed ) : base(oParsed) 
        { 
        }    

        public static IFuturesSymbol? Create( JObject oObject )
        {
            FuturesSymbolParsed? oParsed = oObject.ToObject<FuturesSymbolParsed>();
            if (oParsed == null) return null;
            return new FuturesSymbol(oParsed);

        }
    }
}
