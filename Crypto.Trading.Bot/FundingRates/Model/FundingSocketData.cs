using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{

    /// <summary>
    /// Creates funding socket data
    /// </summary>
    internal class FundingSocketData : IFundingSocketData
    {

        private Dictionary<string, IFuturesSymbol[]>? m_aSymbols = null;

        private const string QUOTE = "USDT";
        public FundingSocketData( ICommonLogger oLogger, ICryptoSetup oSetup ) 
        { 
            Logger = oLogger;
            List<ICryptoFuturesExchange> aExchanges = new List<ICryptoFuturesExchange>();
            foreach (var eType in oSetup.ExchangeTypes)
            {
                ICryptoFuturesExchange oNew = ExchangeFactory.CreateExchange(eType, oSetup);
                aExchanges.Add(oNew);
            }
            Exchanges = aExchanges.ToArray();   

        }

        public ICommonLogger Logger { get; }

        public ICryptoWebsocket[]? Websockets { get; private set; } = null;

        public ICryptoFuturesExchange[] Exchanges { get; }

        /// <summary>
        /// Creates symbols
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateSymbols()
        {
            if (m_aSymbols != null) return true;
            Logger.Info("FundingSocketData: Symbol load start");
            Dictionary<string, List<IFuturesSymbol>> aResult = new Dictionary<string, List<IFuturesSymbol>>();
            foreach( var oExchange in Exchanges )
            {
                Logger.Info($"FundingSocketData:    {oExchange.ExchangeType.ToString()}");
                IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
                if( aSymbols == null ) return false;
                foreach( var oSymbol in aSymbols ) 
                { 
                    if( oSymbol.Quote != QUOTE ) continue;  
                    if(!aResult.ContainsKey(oSymbol.Base) ) aResult.Add(oSymbol.Base, new List<IFuturesSymbol>());
                    aResult[oSymbol.Base].Add(oSymbol); 
                }
            }

            // Convert into dictionary with array
            Dictionary<string, IFuturesSymbol[]> aCorrect = new Dictionary<string, IFuturesSymbol[]>(); 

            foreach( string strBase in aResult.Keys ) 
            { 
                List<IFuturesSymbol> aBase = aResult[strBase];  
                if( aBase.Count < 2 ) continue; 
                aCorrect.Add(strBase, aBase.ToArray()); 
            }
            m_aSymbols = aCorrect;  
            return true;
        }


        /// <summary>
        /// Create websockets
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateWebsockets()
        {
            Logger.Info("FundingSocketData: Create websockets");
            List<ICryptoWebsocket> aWebsockets = new List<ICryptoWebsocket>();
            foreach (var oExchange in Exchanges)
            {
                ICryptoWebsocket? oNew = await oExchange.CreateWebsocket();
                if (oNew == null) return false;
                aWebsockets.Add(oNew);  
            }
            Websockets = aWebsockets.ToArray();
            return true;
        }

        /// <summary>
        /// Subscribe to symbols
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Subscribe()
        {
            if (Websockets == null) return false;

            Logger.Info("FundingSocketData: Subscribe to funding rates");
            foreach (var oWebsocket in Websockets)
            {
                Logger.Info($"FundingSocketData:    {oWebsocket.Exchange.ExchangeType.ToString()}");
                IFuturesSymbol[] aSubscribe = oWebsocket.FuturesSymbols.Where(p=> p.Quote == QUOTE).ToArray();
                bool bResult = await oWebsocket.Start();
                if( !bResult ) return false;
                await Task.Delay(1000);
                await oWebsocket.SubscribeToFundingRates(aSubscribe);   
            }

            await Task.Delay(1000);


            return true;
        }

        /// <summary>
        /// Starts
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();   
            // Create symbols
            bool bResult = await CreateSymbols();
            if( !bResult ) return false;    
            // Create websockets
            bResult = await CreateWebsockets();
            if (!bResult) return false;
            // Subscribe to funding rates
            bResult = await Subscribe();
            if ( !bResult ) return false;
            return true;
        }
        public async Task Stop()
        {
            if( Websockets == null ) return;
            return;
        }

        public Task<IFundingDate[]?> GetFundingDates()
        {
            throw new NotImplementedException();
        }


        public IFundingDate? GetNext(DateTime? dActual)
        {
            throw new NotImplementedException();
        }
    }
}
