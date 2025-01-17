using Crypto.Interface;
using Crypto.Trading.Bot.Common;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.Bot;
using Crypto.Trading.Bot.FundingRates.Model;

namespace Crypto.Trading.Bot
{

    /// <summary>
    /// Instances bos
    /// </summary>
    public class BotFactory
    {
        public static ITradingBot CreateFundingRatesBot(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            // BaseBot oBot = new BaseBot( oSetup, oLogger );
            // oBot.Strategy = new FundingRateStrategy(oBot);
            ITradingBot oBot = new FundingRateBot(oSetup, oLogger);
            return oBot;
        }

        public static IFundingSocketData CreateFundingSocket( ICryptoSetup oSetup, ICommonLogger oLogger )
        {
            return new FundingSocketData(oLogger, oSetup);
        }

    }
}
