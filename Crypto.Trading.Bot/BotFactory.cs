using Crypto.Interface;
using Crypto.Trading.Bot.FundingRates;

namespace Crypto.Trading.Bot
{

    /// <summary>
    /// Instances bos
    /// </summary>
    public class BotFactory
    {
        public static ITradingBot CreateFundingRatesBot( ICryptoSetup oSetup, ICommonLogger oLogger )
        {
            return new FundingRatesBot( oSetup, oLogger );   
        }
    }
}
