using BingX.Net.Clients;
using BingX.Net.Objects.Models;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxWebsocket : ICryptoWebsocket
    {

        private BingXSocketClient? m_oAccountSocketClient = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private string? m_strListenKey = null;

        private BingxBalanceManager m_oBalanceManager;
        public BingxWebsocket(BingxFutures oExchange) 
        { 
            m_oExchange = oExchange;
            m_oBalanceManager = new BingxBalanceManager(this);
        }
        private BingxFutures m_oExchange;
        public IExchange Exchange { get => m_oExchange; }

        public IWebsocketManager<IFuturesOrder> FuturesOrderManager => throw new NotImplementedException();

        public IWebsocketManager<IOrderbook> OrderbookManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesPosition> FuturesPositionManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

      



        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();

            m_oCancelSource = new CancellationTokenSource();
            var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.Account.StartUserStreamAsync(m_oCancelSource.Token);
            if (oResult == null || !oResult.Success) return false;
            m_strListenKey = oResult.Data;

            m_oAccountSocketClient = new BingXSocketClient();
            var oResultSubscribe = await m_oAccountSocketClient.PerpetualFuturesApi.SubscribeToUserDataUpdatesAsync(
                m_strListenKey,
                OnAccountUpdate,
                OnOrderUpdate,
                OnConfigUpdate,
                null
                );

            if( oResultSubscribe == null ||  !oResultSubscribe.Success) return false;   
            return true;
        }


        /// <summary>
        /// Account data update
        /// </summary>
        /// <param name="oUpdate"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAccountUpdate( DataEvent<BingXFuturesAccountUpdate> oUpdate )
        {
            DateTime dDate = oUpdate.Timestamp.ToLocalTime();   
            if( oUpdate.Data == null ) return;
            if (oUpdate.Data.Update == null) return;
            if( oUpdate.Data.Update.Balances != null )
            {
                foreach( var oBalance in oUpdate.Data.Update.Balances )
                {
                    m_oBalanceManager.Put(oBalance);
                }
            }


            return;
        }

        /// <summary>
        /// Order data update
        /// </summary>
        /// <param name="oUpdate"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnOrderUpdate(DataEvent<BingXFuturesOrderUpdate> oUpdate)
        {
            throw new NotImplementedException();
        }
        private void OnConfigUpdate(DataEvent<BingXConfigUpdate> oUpdate)
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Starts socket client
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            m_oCancelSource.Cancel();
            await Task.Delay(500);
            if( m_oAccountSocketClient != null)
            {
                m_oAccountSocketClient.Dispose();
                await Task.Delay(1000);
                m_oAccountSocketClient = null;
            }
        }

        public async Task<bool> SubscribeToMarket(ISymbol[] aSymbols)
        {
            throw new NotImplementedException();
        }
    }
}
