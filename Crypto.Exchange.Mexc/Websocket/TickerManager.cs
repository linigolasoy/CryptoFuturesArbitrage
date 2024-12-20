using Crypto.Interface;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{

    internal class WsTicker : ITicker
    {
        public WsTicker( IFuturesSymbol oSymbol, TickerMessage oTicker ) 
        { 
            Symbol = oSymbol;
            Price = (decimal)oTicker.LastPrice; 
            Ask = (decimal)oTicker.Ask;
            Bid = (decimal)oTicker.Bid;
            FundingRate = (decimal)oTicker.FundingRate;
            DateTime = MexcCommon.ParseUnixTimestamp(oTicker.UnixTime);
        }
        public ISymbol Symbol { get; }

        public decimal Price { get; }

        public decimal Ask { get; }

        public decimal Bid { get; }

        public decimal FundingRate { get; }

        public DateTime DateTime { get; }
    }
    internal class TickerManager: IWebsocketManager<ITicker>
    {


        private ConcurrentDictionary<string, ITicker> m_aTickers = new ConcurrentDictionary<string, ITicker>();
        public TickerManager() 
        { 
        }


        public void Put(IWebsocketMessage oMessage, IFuturesSymbol[] aSymbols)
        {
            if (!(oMessage is TickerMessage)) return;
            TickerMessage oTickerMessage = (TickerMessage)oMessage;    
            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p=> p.Symbol == oTickerMessage.Symbol);
            if (oSymbol == null) return;
            ITicker oTicker = new WsTicker(oSymbol, oTickerMessage);

            m_aTickers[oSymbol.Symbol] = oTicker;
        }

        public ITicker[] GetData()
        {
            List<ITicker> aResult = new List<ITicker>();

            foreach( string strKey in m_aTickers.Keys )
            {
                aResult.Add(m_aTickers[strKey]);
            }
            return aResult.ToArray();
        }

        public ITicker? GetData( string strSymbol )
        {
            if( !m_aTickers.ContainsKey(strSymbol) ) return null;   
            return m_aTickers[strSymbol];   
        }
    }
}
