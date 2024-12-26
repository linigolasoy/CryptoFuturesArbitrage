using Crypto.Exchange.Mexc;
using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{
    internal partial class FundingRatesBot : ITradingBot
    {

        private ICryptoFuturesExchange[] m_aExchanges;

        public FundingRatesBot( ICryptoSetup oSetup, ICommonLogger oLogger ) 
        { 
            Setup = oSetup;
            Logger = oLogger;   

            List<ICryptoFuturesExchange> aExchanges = new List<ICryptoFuturesExchange>();
            foreach( var eType in oSetup.ExchangeTypes )
            {
                switch( eType )
                {
                    case ExchangeType.MexcFutures:
                        aExchanges.Add(new MexcFuturesExchange(oSetup));
                        break;
                    case ExchangeType.BingxFutures:
                        aExchanges.Add(ExchangeFactory.CreateExchange( ExchangeType.BingxFutures, oSetup));
                        break;
                }
            }
            m_aExchanges = aExchanges.ToArray();    
            Exchanges = m_aExchanges;   
        }
        public ICryptoSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public IExchange[] Exchanges { get; }

        /// <summary>
        /// Start bot
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Start()
        {
            await Initialize();
        }

        public async Task Stop()
        {
            if (m_aDictExchanges == null) return;
            await StopMainLoop();
            foreach ( ExchangeType eType in m_aDictExchanges.Keys )
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];
                if (oData.Websocket == null) continue;
                await oData.Websocket.Stop();
            }

            await Task.Delay(2000);
        }
    }
}
