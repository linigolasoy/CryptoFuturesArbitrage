using Crypto.Exchange.Mexc.Futures;
using Crypto.Exchange.Mexc.Responses;
using Crypto.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Spot
{
    internal class SpotSymbol: BaseSymbol, ISpotSymbol
    {

        internal SpotSymbol( SpotSymbolParsed oParsed  ) : base( oParsed ) 
        { 
        }


        public static ISpotSymbol? Create(JObject oObject)
        {
            SpotSymbolParsed? oParsed = oObject.ToObject<SpotSymbolParsed>();
            if (oParsed == null) return null;
            return new SpotSymbol(oParsed);

        }

    }
}
