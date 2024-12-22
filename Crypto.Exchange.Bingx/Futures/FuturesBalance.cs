using Crypto.Interface.Futures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Futures
{

    internal class FuturesBalanceParsed
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;
        [JsonProperty("asset")]
        public string Asset { get; set; } = string.Empty;
        [JsonProperty("balance")]
        public string Balance { get; set; } = string.Empty;
        [JsonProperty("equity")]
        public string Equity { get; set; } = string.Empty;
        [JsonProperty("unrealizedProfit")]
        public string UnrealizedProfit { get; set; } = string.Empty;
        [JsonProperty("realisedProfit")]
        public string RealisedProfit { get; set; } = string.Empty;

        [JsonProperty("availableMargin")]
        public string AvailableMargin { get; set; } = string.Empty;
        [JsonProperty("usedMargin")]
        public string UsedMargin { get; set; } = string.Empty;
        [JsonProperty("freezedMargin")]
        public string FreezedMargin { get; set; } = string.Empty;
    }
    internal class FuturesBalance : IFuturesBalance
    {

        internal FuturesBalance( FuturesBalanceParsed oParsed ) 
        {
            Currency = oParsed.Asset;
            Equity = decimal.Parse( oParsed.Equity, CultureInfo.InvariantCulture );
            ProfitUnrealized = decimal.Parse(oParsed.UnrealizedProfit, CultureInfo.InvariantCulture);
            ProfitRealized = decimal.Parse(oParsed.RealisedProfit, CultureInfo.InvariantCulture);
            MarginAvaliable = decimal.Parse(oParsed.AvailableMargin, CultureInfo.InvariantCulture);
            MarginUsed = decimal.Parse(oParsed.UsedMargin, CultureInfo.InvariantCulture);
            MarginFreezed = decimal.Parse(oParsed.FreezedMargin, CultureInfo.InvariantCulture);
        }
        public string Currency { get; }

        public decimal Equity { get; }

        public decimal ProfitUnrealized { get; }

        public decimal ProfitRealized { get; }

        public decimal MarginAvaliable { get; }

        public decimal MarginUsed { get; }

        public decimal MarginFreezed { get; }

        /// <summary>
        /// Create balance from token
        /// </summary>
        /// <param name="oToken"></param>
        /// <returns></returns>
        public static IFuturesBalance[]? Create( JToken oToken )
        {
            if (!(oToken is JArray)) return null;
            JArray oArray = (JArray)oToken;
            List<FuturesBalanceParsed>? aParsed = oArray.ToObject<List<FuturesBalanceParsed>?>();

            if (aParsed == null) return null;
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( var oParsed in aParsed )
            {
                aResult.Add( new FuturesBalance(oParsed) ); 
            }
            /*
            {"code":0,"msg":"","data":[
            {"userId":"1325391540804313096","asset":"USDT","balance":"120.2596","equity":"120.2596","unrealizedProfit":"0.0000","realisedProfit":"0","availableMargin":"120.2596","usedMargin":"0.0000","freezedMargin":"0.0000","shortUid":"23669055"}
            ,{"userId":"1325391540804313096","asset":"USDC","balance":"0.0000","equity":"0.0000","unrealizedProfit":"0.0000","realisedProfit":"0","availableMargin":"0.0000","usedMargin":"0.0000","freezedMargin":"0.0000","shortUid":"23669055"}]}             
            */

            return aResult.ToArray();
        }
    }
}
