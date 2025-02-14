using Crypto.Exchanges.All.BitUnix.Rest;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.Json;

namespace Crypto.Exchanges.All.BitUnix
{


    /// <summary>
    /// Symbol mapping
    /// </summary>
    internal class BitunixSymbol : BaseFuturesSymbol, IFuturesSymbol
    {
        private BitunixSymbol( IFuturesExchange oExchange, BitunixSymbolParsed oParsed ) : base( oExchange )
        {

            Symbol = oParsed.Symbol;
            Base = oParsed.Base;
            Quote = oParsed.Quote;
            LeverageMax = oParsed.MaxLeverage;
            FeeMaker = 0.0002M;
            FeeTaker = 0.0006M;
            
            Decimals = oParsed.QuotePrecision;
            Minimum = Decimal.Parse( oParsed.MinTradeVolume, CultureInfo.InvariantCulture);
            ContractSize = 1;
            QuantityDecimals = oParsed.BasePrecision;

            return;
        }


        public static IFuturesSymbol Parse(IFuturesExchange oExchange, JToken oToken )
        {
            var oParsed = ((JObject)oToken).ToObject<BitunixSymbolParsed>();    
            return new BitunixSymbol(oExchange, oParsed!);   
        }
    }
}
