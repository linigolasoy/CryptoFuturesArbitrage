using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{
    internal class SpotSymbolParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; //

        [JsonProperty("status")]
        public string Status { get; set; } = "3"; // status String  status:1 - online, 2 - Pause, 3 - offline
        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; } = string.Empty; // baseAsset String	base Asset

        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; } = string.Empty; // quoteAsset String  quote Asset

        /*
            [JsonProperty("month")]
            baseAssetPrecision	Int	base Asset Precision
            [JsonProperty("month")]
            quotePrecision	Int	quote Precision
            [JsonProperty("month")]
            quoteAssetPrecision	Int	quote Asset Precision
            [JsonProperty("month")]
            baseCommissionPrecision	Int	base Commission Precision
            [JsonProperty("month")]
            quoteCommissionPrecision	Int	quote Commission Precision
            [JsonProperty("month")]
            orderTypes	Array	Order Type
            [JsonProperty("month")]
            quoteOrderQtyMarketAllowed	Boolean	quoteOrderQtyMarketAllowed
            [JsonProperty("month")]
            isSpotTradingAllowed	Boolean	allow api spot trading
            [JsonProperty("month")]
            isMarginTradingAllowed	Boolean	allow api margin trading
            [JsonProperty("month")]
            permissions	Array	permissions
            [JsonProperty("month")]
            maxQuoteAmount	String	max Quote Amount
            [JsonProperty("month")]
            makerCommission	String	marker Commission
            [JsonProperty("month")]
            takerCommission	String	taker Commission
            [JsonProperty("month")]
            quoteAmountPrecision	string	min order amount
            [JsonProperty("month")]
            baseSizePrecision	string	min order quantity
            [JsonProperty("month")]
            quoteAmountPrecisionMarket	string	min order amount in market order
            [JsonProperty("month")]
            maxQuoteAmountMarket	String	max quote Amount in market order
            [JsonProperty("month")]
            tradeSideType	String	tradeSide Type:1 - All, 2 - buy order only, 3 - Sell order only, 4 - Close             
        */
    }
}
