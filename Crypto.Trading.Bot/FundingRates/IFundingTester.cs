using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{

    /// <summary>
    /// Represents a funding pair with its possible gain
    /// </summary>
    public interface IFundingPair
    {
        public IFundingDate FundingDate { get; }

        public decimal Percent { get; } 

        public IFuturesSymbol BuySymbol{ get; }
        public IFuturesSymbol SellSymbol { get; }

        public IFundingRate? BuyFunding {  get; }
        public IFundingRate? SellFunding { get; }
    }


    /// <summary>
    /// Single funding date with all its pairs
    /// </summary>
    public interface IFundingDate
    {
        public DateTime DateTime { get; }

        public IFundingPair[] Pairs { get; }

        public IFundingPair? GetBest();
    }



    /// <summary>
    /// Tester funding rate strategy
    /// </summary>
    public interface IFundingTestData
    {
        public ICryptoFuturesExchange[] Exchanges { get; }

        public ICommonLogger Logger { get; }    

        public DateTime From {  get; }
        public DateTime To { get; }
        public Task<bool> LoadSymbols();

        public Task<bool> LoadRates();

        public IFundingDate? GetNext(DateTime? dActual);

    }



}
