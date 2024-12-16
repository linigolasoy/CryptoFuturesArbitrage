using Crypto.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{

    internal class MexcSymbolParsed
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


    internal class MexcSymbolFuturesParsed
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

    internal class MexcSymbol: ISymbol
    {
        public MexcSymbol( MexcSymbolParsed oParsed ) 
        {
            Symbol = oParsed.Symbol;    
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset; 
        }

        public MexcSymbol( MexcSymbolFuturesParsed oParsed )
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
        }

        public string Symbol { get; }
        public string Base { get;  }
        public string Quote { get; }

        public static MexcSymbol? Create( JObject oObject ) 
        {
            MexcSymbolParsed? oParsed = oObject.ToObject<MexcSymbolParsed>();
            if (oParsed == null) return null;
            return new MexcSymbol( oParsed );   
        }
    }

    internal class MexcFuturesSymbol : MexcSymbol, IFuturesSymbol
    {
        public MexcFuturesSymbol( MexcSymbolFuturesParsed oParsed ): base(oParsed ) 
        { 
        }


        public static MexcFuturesSymbol? CreateFutures(JObject oObject)
        {
            MexcSymbolFuturesParsed? oParsed = oObject.ToObject<MexcSymbolFuturesParsed>();
            if (oParsed == null) return null;
            return new MexcFuturesSymbol(oParsed);
        }

    }
}
