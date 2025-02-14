using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.BitUnix.Rest
{
    internal class BitunixSymbolParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // Coin pair name i.e.BTCUSDT
        [JsonProperty("base")]
        public string Base { get; set; } = string.Empty; // Base currency Specifically refers to ETH as in ETHUSDT
        [JsonProperty("quote")]
        public string Quote { get; set; } = string.Empty; // string Base currency Specifically refers to USDT as in ETHUSDT
        [JsonProperty("minTradeVolume")]
        public string MinTradeVolume { get; set; } = string.Empty; // Minimum opening amount(base currency)
        [JsonProperty("minBuyPriceOffset")]
        public string MinBuyPriceOffset { get; set; } = string.Empty; // Minimum price offset for buy orders
        [JsonProperty("maxSellPriceOffset")]
        public string MaxSellPriceOffset { get; set; } = string.Empty; // Maximum price offset for sell orders
        [JsonProperty("maxLimitOrderVolume")]
        public string MaxLimitOrderVolume { get; set; } = string.Empty; // Maximum limit order base amount
        [JsonProperty("maxMarketOrderVolume")]
        public string MaxMarketOrderVolume { get; set; } = string.Empty; // Maximum market order base amount
        [JsonProperty("basePrecision")]
        public int BasePrecision { get; set; } = 0; // Max precision of opening amount
        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; } = 0; // Max precision of order price
        [JsonProperty("maxLeverage")]
        public int MaxLeverage { get; set; } = 0; // Max leverage
        [JsonProperty("minLeverage")]
        public int MinLeverage { get; set; } = 0; //  Min leverage
        [JsonProperty("defaultLeverage")]
        public int DefaultLeverage { get; set; } = 0; // Default Leverage
        [JsonProperty("defaultMarginMode")]
        public string DefaultMarginMode { get; set; } = string.Empty; // Default margin mode Isolation Cross
        [JsonProperty("PriceProtectScope")]
        public string PriceProtectScope { get; set; } = string.Empty; // Price protection scope.For example: current mark price is:10000 priceProtectScope= 0.02, the minimum sell order price = 10000 * (1 - 0.02) = 9800; the maximum buy order price = 10000*(1+0.02) = 10200
        [JsonProperty("symbolStatus")]
        public string SymbolStatus { get; set; } = string.Empty; // OPEN: trade normal CANCEL_ONLY: cancel only STOP: can't open/close position
    }
}
