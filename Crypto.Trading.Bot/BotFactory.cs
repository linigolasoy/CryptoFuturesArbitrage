using Crypto.Interface;
using Crypto.Trading.Bot.Common;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.Tester;

namespace Crypto.Trading.Bot
{

    /// <summary>
    /// Instances bos
    /// </summary>
    public class BotFactory
    {
        public static ITradingBot CreateFundingRatesBot( ICryptoSetup oSetup, ICommonLogger oLogger, bool bTester )
        {
            if( bTester ) 
            { 
                BaseTester oTester = new BaseTester( oSetup, oLogger );   
                oTester.Strategy = new FundingRateTester(oTester);
                return oTester;
            }
            BaseBot oBot = new BaseBot( oSetup, oLogger );
            oBot.Strategy = new FundingRateStrategy(oBot);
            return oBot;
        }



    }
}
