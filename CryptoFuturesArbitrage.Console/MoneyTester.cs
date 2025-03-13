using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoFuturesArbitrage.Console
{
    internal class MoneyTester
    {

        public MoneyTester(ICryptoSetup oSetup, ICommonLogger oLogger) 
        { 
            Setup = oSetup;
            Logger = oLogger;   
        }

        private ICryptoSetup Setup { get; }
        private ICommonLogger Logger { get; }

        private ExchangeType m_eExchangeFrom = ExchangeType.CoinExFutures;
        private ExchangeType m_eExchangeTo = ExchangeType.BingxFutures;
        private decimal m_nMoney = 10;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            Logger.Info("Money tester starting...");

            Logger.Info("   Create exchanges...");
            IFuturesExchange oExchangeFrom = await ExchangeFactory.CreateExchange(m_eExchangeFrom, Setup, Logger);
            IFuturesExchange oExchangeTo = await ExchangeFactory.CreateExchange(m_eExchangeTo, Setup, Logger);

            IMoneyTransfer oTransfer = ExchangeFactory.CreateMoneyTransfer(oExchangeFrom, oExchangeTo, m_nMoney);

            while( oTransfer.Status != MoneyTransferStatus.Done ) 
            {
                ITradingResult<decimal> oSended = await oTransfer.Step();
                if( !oSended.Success )
                {
                    Logger.Error( (oSended.Message == null ? "Error sending": oSended.Message));
                    break;
                }
                Logger.Info($"   Transfering. Status = {oTransfer.Status.ToString()}");  
            }

            Logger.Info("Money tester end.");



        }
    }
}
