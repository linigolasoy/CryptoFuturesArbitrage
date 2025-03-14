﻿using Crypto.Interface;
using Crypto.Interface.Futures.Account;
using Crypto.Trading.Bot.Arbitrage;
using Crypto.Trading.Bot.Common;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.Bot;
using Crypto.Trading.Bot.FundingRates.Model;
using Crypto.Trading.Bot.Spread;

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
        public static ITradingBot CreateSpreadBot(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            ITradingBot oBot = new SpreadBot(oSetup, oLogger);
            return oBot;
        }

        public static ITradingBot CreateFuturesArbitrageBot(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            ITradingBot oBot = new FuturesArbitrageBot(oSetup, oLogger);
            return oBot;
        }

        public static IFundingSocketData CreateFundingSocket( ICryptoSetup oSetup, ICommonLogger oLogger )
        {
            return new FundingSocketData(oLogger, oSetup);
        }

        public static ISocketManager CreateSocketManager(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            return new BaseSocketManager(oSetup, oLogger);  
        }

        public static ITrailingOrder CreateTrailingOrder( IFuturesPosition oPosition, decimal nDistance )
        {
            return new TrailingOrder(oPosition, nDistance);
        }

    }
}
