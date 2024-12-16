using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Responses
{
    internal class FuturesSymbolParsed
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // symbol	string	the name of the contract

        [JsonProperty("displayNameEn")]
        public string Name { get; set; } = "3"; // displayNameEn	string	english display name
        [JsonProperty("baseCoin")]
        public string BaseAsset { get; set; } = string.Empty; // baseCoin	string	base currency such as BTC

        [JsonProperty("quoteCoin")]
        public string QuoteAsset { get; set; } = string.Empty; // quoteAsset String  quote Asset

        /*
            [JsonProperty("month")]
            displayName	string	display name           
            positionOpenType	int	position open type,1：isolated，2：cross，3：both          
            quoteCoin	string	quote currency such as USDT
            settleCoin	string	liquidation currency such as USDT
            contractSize	decimal	contract value
            minLeverage	int	minimum leverage
            maxLeverage	int	maximum leverage
            priceScale	int	price scale
            volScale	int	quantity scale
            amountScale	int	amount scale
            priceUnit	int	price unit
            volUnit	int	volume unit
            minVol	decimal	minimum volume
            maxVol	decimal	maximum volume
            bidLimitPriceRate	decimal	bid limit price rate
            askLimitPriceRate	decimal	ask limit price rate
            takerFeeRate	decimal	taker rate
            makerFeeRate	decimal	maker rate
            maintenanceMarginRate	decimal	maintenance margin rate
            initialMarginRate	decimal	initial margin rate
            riskBaseVol	decimal	initial volume
            riskIncrVol	decimal	risk increasing volume
            riskIncrMmr	decimal	maintain increasing margin rate
            riskIncrImr	decimal	initial increasing margin rate
            riskLevelLimit	int	risk level limit
            priceCoefficientVariation	decimal	fair price coefficient variation
            indexOrigin	List	index origin
            state	int	status, 0:enabled,1:delivery, 2:completed, 3: offline, 4: pause
            apiAllowed	bool	whether support api
            conceptPlate	List	The zone, corresponding to the entryKey field of the section list
            riskLimitType	List	Risk limit type, BY_VOLUME: by the volume, BY_VALUE: by the position        */
    }
}
