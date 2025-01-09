﻿using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoClients.Net.Interfaces;
using Bitget.Net.Enums;
using Crypto.Exchanges.All.Bitget.Websocket;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetAccount : IFuturesAccount
    {
        private BitgetWebsocketPrivate m_oWebsocketPrivate;
        public ICryptoFuturesExchange Exchange { get; }

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oWebsocketPrivate.BalanceManager; }

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oWebsocketPrivate.OrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oWebsocketPrivate.PositionManager; }

        private IExchangeRestClient m_oGlobalClient;

        public BitgetAccount( BitgetFutures oExchange, IExchangeRestClient oGlobalClient)
        {
            Exchange = oExchange;
            m_oGlobalClient = oGlobalClient;
            m_oWebsocketPrivate = new BitgetWebsocketPrivate(oExchange);
        }

        /// <summary>
        /// Get balances
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesBalance[]?> GetBalances()
        {
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Account.GetBalancesAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null;
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( var oData  in oResult.Data )
            {
                IFuturesBalance oNew = new BitgetBalance( oData );  
                aResult.Add( oNew );
            }
            return aResult.ToArray();   
        }

        public async Task<IFuturesPosition[]?> GetPositions()
        {
            var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.Trading.GetPositionsAsync(BitgetProductTypeV2.UsdtFutures, BitgetTrading.USDT);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            IFuturesSymbol[]? aSymbols = await Exchange.GetSymbols();
            if( aSymbols == null ) return null;
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( var oData in oResult.Data )
            {
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol); 
                if( oFound == null ) continue;
                IFuturesPosition oPosition = new BitgetPositionLocal(oFound, oData);
                aResult.Add( oPosition );
            }
            return aResult.ToArray();
        }

        public async Task<bool> StartSockets()
        {
            IFuturesSymbol[]? aSymbols = await Exchange.GetSymbols();
            if (aSymbols == null) return false;
            m_oWebsocketPrivate.FuturesSymbols = aSymbols;
            bool bResult = await m_oWebsocketPrivate.Start();

            return bResult;
        }

    }
}
