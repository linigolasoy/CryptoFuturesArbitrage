using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{

    internal class FundingChanceData
    {
        public FundingChanceData( ExchangeType eType, IFuturesSymbol oSymbol, decimal nPrice, decimal nRate, DateTime dNext ) 
        { 
            ExchangeType = eType;
            Price = nPrice;
            Rate = nRate;
            NextPay = dNext;    
            Symbol = oSymbol;
        }  
        public ExchangeType ExchangeType { get; }
        public decimal Price { get; }   
        public decimal Rate { get; }
        public DateTime NextPay { get; }    
        public IFuturesSymbol Symbol { get; }
    }

    /// <summary>
    /// Funding chance
    /// </summary>
    internal class FundingChance
    {
        public FundingChance(decimal ratePercent, FundingChanceData buyData, FundingChanceData sellData)
        {
            RatePercent = ratePercent;
            BuyData = buyData;
            SellData = sellData;
        }

        public decimal RatePercent { get; } 

        public FundingChanceData BuyData { get; } 
        public FundingChanceData SellData { get; }  
    }
}
