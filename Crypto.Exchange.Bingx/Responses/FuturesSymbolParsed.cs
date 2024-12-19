using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Responses
{
    internal class FuturesSymbolParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // symbol string trading pair, for example: BTC...

        [JsonProperty("asset")]
        public string Base { get; set; } = string.Empty; // asset string contract trading asset
        [JsonProperty("currency")]
        public string Quote { get; set; } = string.Empty; // currency string settlement and margin currency...

        [JsonProperty("takerFeeRate")]
        public double FeeTaker { get; set; } = 0; // takerFeeRate float64 take transaction fee
        [JsonProperty("makerFeeRate")]
        public double FeeMaker { get; set; } = 0; // makerFeeRate float64 make transaction fee

        [JsonProperty("status")]
        public int Status { get; set; } = 1; // status int 1 online, 25 forbidden to open...

        /*
            contractId string contract ID 
            
            quantityPrecision int transaction quantity precision
            pricePrecision int price precision
            tradeMinQuantity float64 The minimum trading unit(COIN)
            tradeMinUSDT float64 The minimum trading unit(USDT)
            
            
            
            apiStateOpen string Whether the API can open a pos...
            apiStateClose string Whether API can close position...
            ensureTrigger bool Whether to support guaranteed ...
            triggerFeeRate string The fee rate for guaranteed st...
            brokerState bool Whether to prohibit broker use...
            launchTime long shelf time; The status of the ...
            maintainTime long The start time of the prohibit...
            offTime long Down line time, after the time...         
        */
    }
}
