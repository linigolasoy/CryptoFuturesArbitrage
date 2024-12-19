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
            LeverageMin = oParsed.MinLeverage;
            LeverageMax = oParsed.MaxLeverage;
            FeeMaker = (decimal)oParsed.FeeMaker * 100M;
            FeeTaker = (decimal)oParsed.FeeTaker * 100M;
        }

        public static IFuturesSymbol? Create( JObject oObject )
        {
            FuturesSymbolParsed? oParsed = oObject.ToObject<FuturesSymbolParsed>();
            if (oParsed == null) return null;
            return new FuturesSymbol(oParsed);

        }

        public int LeverageMax { get; }
        public int LeverageMin { get; }

        public decimal FeeMaker { get; }
        public decimal FeeTaker { get; }

    }
}
