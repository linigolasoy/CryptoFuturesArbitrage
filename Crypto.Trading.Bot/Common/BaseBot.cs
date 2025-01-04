using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{

    /// <summary>
    /// Base bot
    /// </summary>
    internal class BaseBot : ITradingBot
    {

        public ICryptoSetup Setup { get; }

        public IBotExchangeData[] ExchangeData { get; }

        public ICommonLogger Logger { get; }

        public IBotStrategy? Strategy { get; internal set; } = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private Task? m_oMainTask = null;

        public BaseBot( ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            List<IBotExchangeData> aExchanges = new List<IBotExchangeData>();

            foreach (var eType in oSetup.ExchangeTypes)
            {
                switch (eType)
                {
                    case ExchangeType.CoinExFutures:
                        aExchanges.Add( new BaseExchangeData(ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, oSetup)));
                        break;
                    case ExchangeType.BingxFutures:
                        aExchanges.Add(new BaseExchangeData(ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup)));
                        break;
                }
            }
            ExchangeData = aExchanges.ToArray();
        }


        /// <summary>
        /// Load symbols on exchange
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        private async Task<bool> LoadSymbols( IBotExchangeData oData )
        {
            Logger.Info($"   Load symbols for {oData.Exchange.ExchangeType.ToString()}...");
            IFuturesSymbol[]? aSymbols = await oData.Exchange.GetSymbols();
            if (aSymbols == null) throw new Exception(string.Format("Could not load symbols for exchange {0}", oData.Exchange.ExchangeType.ToString()));
            List<IBotSymbolData> aSymbolData = new List<IBotSymbolData>();  
            foreach( var oSymbol in aSymbols)
            {
                IBotSymbolData? oSymbolData = Strategy!.CreateSymbolData(oData, oSymbol);
                if (oSymbolData == null) continue;
                if (!Strategy!.EvalSymbol(oSymbolData)) continue;
                aSymbolData.Add(oSymbolData);
            }

            ((BaseExchangeData)oData).Symbols = aSymbolData.ToArray();  

            return true;
        }


        /// <summary>
        /// Load exchange balances
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        private async Task<bool> LoadBalances( IBotExchangeData oData )
        {
            Logger.Info($"   Load balances for {oData.Exchange.ExchangeType.ToString()}...");
            IFuturesBalance[]? aBalances = await oData.Exchange.GetBalances(); 
            if( aBalances == null) { Logger.Error($"   Could not load balances for {oData.Exchange.ExchangeType.ToString()}..."); return false; }
            ((BaseExchangeData)oData).Balances = aBalances;
            return true;
        }

        /// <summary>
        /// Creates websockets
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        private async Task<bool> CreateWebsockets( IBotExchangeData oData )
        {
            Logger.Info($"   Create websockets for {oData.Exchange.ExchangeType.ToString()}...");
            ICryptoWebsocket? oWebsocket = await oData.Exchange.CreateWebsocket();
            if (oWebsocket == null) { Logger.Error($"   Could not create websocket for {oData.Exchange.ExchangeType.ToString()}..."); return false; }
            ((BaseExchangeData)oData).Websocket = oWebsocket;
            await oWebsocket.Start();
            return true;
        }

        /// <summary>
        /// Subscribe to websockets
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        private async Task<bool> SubscribeWebsockets( IBotExchangeData oData )
        {
            Logger.Info($"   Subscribe to market on {oData.Exchange.ExchangeType.ToString()}...");
            IFuturesSymbol[] aSymbols = oData.Symbols!.Select(p=> p.Symbol).ToArray();
            bool bResult = await oData.Websocket!.SubscribeToMarket( aSymbols );
            if (!bResult) { Logger.Error($"   Could not subscribe to market on {oData.Exchange.ExchangeType.ToString()}..."); return false; }
            return true;

        }
        /// <summary>
        /// Load data
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Start()
        {
            if (Strategy == null) throw new Exception("Can no start without strategy");
            Logger.Info("Bot starting...");
            foreach (var oExchangeData in ExchangeData) 
            {
                if (m_oCancelSource.IsCancellationRequested) break;
                // Load symbols
                bool bResult = await LoadSymbols( oExchangeData );
                if (!bResult) break;

                // Load balances
                bResult = await LoadBalances( oExchangeData );
                if (!bResult) break;
                if (m_oCancelSource.IsCancellationRequested) break;
                // TODO: Load orders
                // TODO: Load positions
                // Create websockets
                bResult = await CreateWebsockets(oExchangeData);
                if (!bResult) break;
                if (m_oCancelSource.IsCancellationRequested) break;
                // Subscribe websockets
                // bResult = await SubscribeWebsockets(oExchangeData);
                // if (!bResult) break;
                if (m_oCancelSource.IsCancellationRequested) break;

            }
            if( !m_oCancelSource.IsCancellationRequested )
            {
                m_oMainTask = MainLoop();
            }
            Logger.Info("Bot started...");
        }


        /// <summary>
        /// Stop bot
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            Logger.Info("Bot stopping...");
            m_oCancelSource.Cancel();   
            if( m_oMainTask != null )
            {
                await m_oMainTask;
                m_oMainTask = null; 
            }

            foreach (var oExchangeData in ExchangeData)
            {
                if( oExchangeData.Websocket != null ) 
                { 
                    await oExchangeData.Websocket.Stop();
                    await Task.Delay(1000); 
                }
                oExchangeData.Reset();
            }

            Logger.Info("Bot stopped...");
        }


        private async Task MainLoop()
        {

            Logger.Info("Main loop started");
            while( !m_oCancelSource.IsCancellationRequested )
            {
                bool bUpdated = true;
                foreach (var oExchangeData in ExchangeData)
                {
                    bUpdated = bUpdated & (await oExchangeData.Update());
                    if( !bUpdated ) break;   
                }
                if( bUpdated )
                {
                    await Strategy!.Process( ExchangeData );
                }
                await Task.Delay(200);
            }
            Logger.Info("Main loop ended");
        }
    }
}
