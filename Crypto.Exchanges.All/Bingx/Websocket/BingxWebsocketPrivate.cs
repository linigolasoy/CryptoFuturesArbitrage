using BingX.Net.Clients;
using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxWebsocketPrivate : BasePrivateQueueManager, IFuturesWebsocketPrivate
    {

        private BingXSocketClient? m_oAccountSocketClient = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private IFuturesExchange m_oExchange;

        private BingxBalanceManager m_oBalanceManager;
        private BingxOrderManager m_oOrderManager;
        private BingxPositionManager m_oPositionManager;

        private string? m_strListenKey = null;

        private Task? m_oRenewTask = null;
        public BingxWebsocketPrivate(IFuturesAccount oAccount) : base(oAccount)
        {
            m_oExchange = oAccount.Exchange;
            m_oBalanceManager = new BingxBalanceManager(this);
            m_oOrderManager = new BingxOrderManager(this);  
            m_oPositionManager = new BingxPositionManager(this);    
        }

        public IFuturesExchange Exchange { get => m_oExchange; }


        public IWebsocketPrivateManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

        public IWebsocketPrivateManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketPrivateManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        /// <summary>
        /// Start websocket
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            await Stop();
            await StartLoop();  

            m_oCancelSource = new CancellationTokenSource();

            bool bResult = await RenewListenKey();
            return bResult;

        }

        public async Task Stop()
        {
            if (m_oAccountSocketClient != null)
            {
                await m_oAccountSocketClient.UnsubscribeAllAsync();
                await Task.Delay(1000);

                var oResult = await ((BingxFutures)m_oExchange).GlobalClient.BingX.PerpetualFuturesApi.Account.StopUserStreamAsync(m_strListenKey!);

                m_oAccountSocketClient.Dispose();
                m_oAccountSocketClient = null;
            }
            await StopLoop();

        }

        /// <summary>
        /// Renew listen key
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RenewListenKey()
        {
            var oResult = await ((BingxFutures)m_oExchange).GlobalClient.BingX.PerpetualFuturesApi.Account.StartUserStreamAsync(m_oCancelSource.Token);
            if (oResult == null || !oResult.Success) return false;
            m_strListenKey = oResult.Data;
            // await ((BingxFutures)m_oExchange).GlobalClient.BingX.PerpetualFuturesApi.Account.KeepAliveUserStreamAsync(m_strListenKey);
            if( m_oAccountSocketClient == null ) m_oAccountSocketClient = new BingXSocketClient();
            var oResultSubscribe = await m_oAccountSocketClient.PerpetualFuturesApi.SubscribeToUserDataUpdatesAsync(
                m_strListenKey,
                OnAccountUpdate,
                OnOrderUpdate,
                OnConfigUpdate,
                OnKeyExpired
                );

            if (oResultSubscribe == null || !oResultSubscribe.Success) return false;
            if ( m_oExchange.Logger != null ) m_oExchange.Logger.Info("Bingx renew key");
            return true;
        }
        /// <summary>
        /// Account data update
        /// </summary>
        /// <param name="oUpdate"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAccountUpdate(DataEvent<BingXFuturesAccountUpdate> oUpdate)
        {
            DateTime dDate = oUpdate.Timestamp.ToLocalTime();


            if (oUpdate.Data == null) return;
            if (oUpdate.Data.Update == null) return;
            if (oUpdate.Data.Update.Balances != null)
            {
                foreach (var oBalance in oUpdate.Data.Update.Balances)
                {
                    m_oBalanceManager.Put(oBalance);
                }
            }

            if( oUpdate.Data.Update.Positions != null)
            {
                m_oPositionManager.Put(oUpdate.Data.Update.Positions);  
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
            if( oUpdate == null || oUpdate.Data == null) return;    
            m_oOrderManager.Put(oUpdate.Data);  
        }
        private void OnConfigUpdate(DataEvent<BingXConfigUpdate> oUpdate)
        {
            return;
            // throw new NotImplementedException();
        }
        private void OnKeyExpired(DataEvent<BingXListenKeyExpiredUpdate> oUpdate)
        {
            m_oRenewTask = RenewListenKey();
            return;
        }

    }
}
