using Crypto.Exchange.Mexc;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates
{
    /// <summary>
    /// Initialize functions
    /// </summary>
    internal partial class FundingRatesBot 
    {

        private Dictionary<ExchangeType, FundingBotExchangeData>? m_aDictExchanges = null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task GetExchangesSymbols()
        {
            if (m_aDictExchanges == null) return;
            foreach( ExchangeType eType in m_aDictExchanges.Keys )
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];

                IFuturesSymbol[]? aSymbols = await oData.Exchange.GetSymbols();
                if (aSymbols == null) { throw new Exception(string.Format("Error on exchange {0}. No symbols", eType.ToString())); }
                oData.Symbols = aSymbols;   
            }
        }

        /// <summary>
        /// Put common symbols
        /// </summary>
        private void PutCommonSymbols()
        {
            if (m_aDictExchanges == null) return;

            foreach( ExchangeType eType1 in m_aDictExchanges.Keys )
            {
                FundingBotExchangeData oData1 = m_aDictExchanges[eType1];

                IFuturesSymbol[] aSymbols1 = oData1.Symbols!;

                foreach (ExchangeType eType2 in m_aDictExchanges.Keys)
                {
                    if (eType1 == eType2) continue;
                    FundingBotExchangeData oData2 = m_aDictExchanges[eType2];
                    IFuturesSymbol[] aSymbols2 = oData2.Symbols!;

                    aSymbols1 = aSymbols1.Where(p => aSymbols2.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();
                }

                oData1.Symbols = aSymbols1; 
            }
        }

        /// <summary>
        /// Create and start websockets
        /// </summary>
        /// <returns></returns>
        private async Task CreateWebsockets()
        {
            if (m_aDictExchanges == null) return;
            foreach (ExchangeType eType in m_aDictExchanges.Keys)
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];
                ICryptoWebsocket? oWs = await oData.Exchange.CreateWebsocket();   
                if( oWs == null ) { throw new Exception(string.Format("Error on exchange {0}. Could not create websocket", eType.ToString())); }
                oData.Websocket = oWs;
                bool bResult = await oData.Websocket.Start();
                if( !bResult ) { throw new Exception(string.Format("Error on exchange {0}. Could not start websocket", eType.ToString())); }
            }

        }

        /// <summary>
        /// Subscribe to symbols
        /// </summary>
        /// <returns></returns>
        private async Task Subscribe()
        {
            if (m_aDictExchanges == null) return;
            foreach (ExchangeType eType in m_aDictExchanges.Keys)
            {
                FundingBotExchangeData oData = m_aDictExchanges[eType];
                Logger.Info(string.Format("             Subscribe to {0}", eType.ToString()));
                await oData.Websocket!.SubscribeToMarket(oData.Symbols!);

            }

        }

        /// <summary>
        /// Initialize bot
        /// </summary>
        /// <returns></returns>
        private async Task Initialize()
        {
            // Init objects 
            m_aDictExchanges = new Dictionary<ExchangeType, FundingBotExchangeData>();
            foreach( var oExchange in m_aExchanges)
            {
                m_aDictExchanges[oExchange.ExchangeType] = new FundingBotExchangeData(oExchange);
            }

            Logger.Info("Initialize.- Started");
            // Get all symbols from exchanges
            Logger.Info("         1.- Load symbols");
            await GetExchangesSymbols();
            // Get common symbols from exchanges 
            Logger.Info("         2.- Common symbols");
            PutCommonSymbols();
            // Create all websockets
            Logger.Info("         3.- Create and start websockets");
            await CreateWebsockets();
            // Subscribe to common symbols
            Logger.Info("         4.- Subscribe to symbols");
            await Subscribe();
            // Create main loop
            await CreateMainLoop();
            // Create chances

            Logger.Info("Initialize.- Completed");
        }
    }
}
