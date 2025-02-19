using Crypto.Interface;
using Crypto.Trading.Bot.BackTest;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.BackTest;
using Crypto.Trading.Bot.FundingRates.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot
{
    public class TesterFactory
    {

        /*
        public static IFundingTestData CreateFundingTestData( ICryptoSetup oSetup, ICommonLogger oLogger, DateTime dFrom, DateTime dTo )
        {
            return new FundingTestData(oSetup, oLogger, dFrom, dTo);    
        }
        */
        
        public static IBackTester CreateFundingRateTester(ICryptoSetup oSetup, ICommonLogger oLogger, DateTime dFrom, DateTime dTo)
        {
            return new FundingRatesTester(oSetup, oLogger, dFrom, dTo);
        }
    }
}
