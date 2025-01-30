using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    internal class BaseSocketManager : ISocketManager
    {

        private IFuturesExchange[] m_aExchanges = Array.Empty<IFuturesExchange>();
        public BaseSocketManager( ICryptoSetup oSetup, ICommonLogger oLogger) 
        { 
            Setup = oSetup;
            Logger = oLogger;
        }
        public ICryptoSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public ExchangeType[]? ExchangeTypes { get; set; } = null;

        public IFuturesExchange[] Exchanges { get => m_aExchanges; }

        public IFuturesWebsocketPublic[]? SocketsPublic { get; } = null;

        public IFuturesWebsocketPrivate[]? SocketsPrivate { get; } = null;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<IFuturesExchange[]?> CreateExchanges()
        {
            ExchangeType[]? aTypes = ExchangeTypes;
            if( aTypes == null) { aTypes = Setup.ExchangeTypes; }
            Logger.Info("SocketManager:    Creating exchanges");
            List<IFuturesExchange> aResult = new List<IFuturesExchange>();
            foreach( ExchangeType eType in aTypes )
            {
                Logger.Info($"                     {eType.ToString()}");
                IFuturesExchange? oExchange = await ExchangeFactory.CreateExchange(eType, Setup);
                if (oExchange == null)
                {
                    Logger.Error("COULD NOT CREATE EXCHANGE!");
                    return null;
                }
                aResult.Add(oExchange); 
            }

            Logger.Info("SocketManager:    Created exchanges");
            await Task.Delay(1000);
            return aResult.ToArray();
        }


        /// <summary>
        /// Boot private sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BootPrivateSockets(IFuturesExchange[] aExchanges )
        {
            Logger.Info("SocketManager:    Starting account sockets");

            foreach( IFuturesExchange oExchange  in aExchanges )
            {
                Logger.Info($"                     {oExchange.ExchangeType.ToString()}");
                bool bStarted = await oExchange.Account.StartSockets();
                if (!bStarted)
                {
                    Logger.Error("COULD NOT START SOCKET!");
                    return false;
                }
            }

            Logger.Info("SocketManager:    Started account sockets");
            await Task.Delay(1000);
            return true;
        }


        /// <summary>
        /// Boot private sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BootPublicSockets(IFuturesExchange[] aExchanges)
        {
            Logger.Info("SocketManager:    Starting market sockets");

            foreach (IFuturesExchange oExchange in aExchanges)
            {
                Logger.Info($"                     {oExchange.ExchangeType.ToString()}");
                bool bStarted = await oExchange.Market.StartSockets();
                if (!bStarted)
                {
                    Logger.Error("COULD NOT START SOCKET!");
                    return false;
                }
            }

            Logger.Info("SocketManager:    Started market sockets");
            await Task.Delay(1000);
            return true;
        }

        /// <summary>
        /// Start websocket manager
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();
            Logger.Info("SocketManager: Starting");
            IFuturesExchange[]? aExchanges = await CreateExchanges();
            if (aExchanges == null) return false;

            bool bPrivate = await BootPrivateSockets(aExchanges);
            if( !bPrivate ) return false;

            bool bPublic = await BootPublicSockets(aExchanges); 
            if( !bPublic ) return false;

            Logger.Info("SocketManager: Started");
            m_aExchanges = aExchanges.ToArray();
            return true;
        }

        public async Task Stop()
        {
            if (Exchanges.Length <= 0) return;

            foreach( IFuturesExchange oExchange in Exchanges)
            {
                await oExchange.Market.EndSockets();
                await oExchange.Account.StopSockets();
            }

            m_aExchanges = Array.Empty<IFuturesExchange>(); 

        }

        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<ExchangeType, IFundingRate[]> GetFundingRates()
        {
            Dictionary<ExchangeType, IFundingRate[]> aResultRaw = new Dictionary<ExchangeType, IFundingRate[]>();
            foreach( IFuturesExchange oExchange in Exchanges )
            {
                aResultRaw[oExchange.ExchangeType] = oExchange.Market.Websocket!.FundingRateManager.GetData();
            }

            return aResultRaw.AsReadOnly();
        }

        /// <summary>
        /// Get all orderbooks
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<ExchangeType, IOrderbook[]> GetOrderbooks()
        {
            Dictionary<ExchangeType, IOrderbook[]> aResultRaw = new Dictionary<ExchangeType, IOrderbook[]>();
            foreach (IFuturesExchange oExchange in Exchanges)
            {
                aResultRaw[oExchange.ExchangeType] = oExchange.Market.Websocket!.OrderbookManager.GetData();
            }

            return aResultRaw.AsReadOnly();
        }
    }
}
