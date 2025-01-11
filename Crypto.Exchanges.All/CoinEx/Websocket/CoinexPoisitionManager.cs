using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexPoisitionManager : IWebsocketManager<IFuturesPosition>
    {

        private ConcurrentDictionary<string, IFuturesPosition> m_aPositions = new ConcurrentDictionary<string, IFuturesPosition>(); 
        private CoinexWebsocketPrivate m_oWebsocket;
        private Task m_oMainTask;
        public int ReceiveCount { get; private set; } = 0;
        public CoinexPoisitionManager(CoinexWebsocketPrivate oWebsocket)
        {
            m_oWebsocket = oWebsocket;
            m_oMainTask = UpdateExisting();
        }
    
        public IFuturesPosition[] GetData()
        {
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( string strId in m_aPositions.Keys )
            {
                IFuturesPosition? oFound = null;
                if( m_aPositions.TryGetValue( strId, out oFound ) ) aResult.Add( oFound );
            }
            return aResult.ToArray();
        }

        public IFuturesPosition? GetData(string strSymbol)
        {
            IFuturesPosition[] aAll = GetData();
            return aAll.FirstOrDefault(p=> p.Symbol.Symbol ==  strSymbol);  
        }

        /// <summary>
        /// Update existing positions loop
        /// </summary>
        /// <returns></returns>
        private async Task UpdateExisting()
        {
            while( true )
            {
                try
                {
                    if (m_oWebsocket.Exchange.Account != null)
                    {
                        IFuturesPosition[]? aPositions = await m_oWebsocket.Exchange.Account.GetPositions();
                        if (aPositions != null && aPositions.Length > 0)
                        {
                            foreach (var oPos in aPositions) { UpdateLocal(oPos); }
                        }
                    }
                }
                catch( Exception e )
                {

                }
                await Task.Delay(100);
            }
        }

        private void UpdateLocal(IFuturesPosition oPosition)
        {
            ReceiveCount++;
            if (oPosition.Quantity <= 0)
            {
                IFuturesPosition? oFound = null;
                m_aPositions.TryRemove(oPosition.Id, out oFound);
            }
            else
            {
                m_aPositions.AddOrUpdate(oPosition.Id, p => oPosition, (s, p) => { p.Update(oPosition); return p; });
            }

        }

        public void Put(CoinExPositionUpdate oUpdate)
        {
            IFuturesSymbol? oSymbol = m_oWebsocket.FuturesSymbols.FirstOrDefault(p => p.Symbol == oUpdate.Position.Symbol);
            if (oSymbol == null) return;
            IFuturesPosition oPosition = new CoinexPoisitionLocal(oSymbol, oUpdate);
            UpdateLocal(oPosition);
            return;
        }
    }
}
