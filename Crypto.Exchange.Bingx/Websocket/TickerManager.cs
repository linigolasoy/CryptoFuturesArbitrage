using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket
{

    /// <summary>
    /// Ticker to add
    /// </summary>
    internal class WsTicker : ITicker
    {
        public WsTicker( IFuturesSymbol oSymbol, TickerMessage oMsg )
        {
            Symbol = oSymbol;
            Bid = decimal.Parse(oMsg.BidPrice, CultureInfo.InvariantCulture);
            Ask = decimal.Parse(oMsg.AskPrice, CultureInfo.InvariantCulture);
            Price = Bid;
            DateTime = BingxCommon.ParseUnixTimestamp(oMsg.EventTime);  
        }
        public ISymbol Symbol { get; }

        public decimal Price { get; private set; }

        public decimal Ask { get; private set; }

        public decimal Bid { get; private set; }

        public decimal FundingRate { get; private set; } = 0;

        public DateTime DateTime { get; private set; }

        internal void Update( ITicker oTicker )
        {
            Price = oTicker.Price;
            DateTime = oTicker.DateTime;
            Ask = oTicker.Ask;
            Bid = oTicker.Bid;
        }

        internal void UpdateFunding( IFundingRateSnapShot oSnapshot )
        {
            FundingRate = oSnapshot.Rate;
        }
    }

    /// <summary>
    /// Ticker manager for websockets
    /// </summary>
    internal class TickerManager : IWebsocketManager<ITicker>
    {

        private ConcurrentDictionary<string, ITicker> m_aTickers = new ConcurrentDictionary<string, ITicker>();
        private ICryptoFuturesExchange m_oExchange;
        private Task m_oFundingTask;

        public TickerManager( ICryptoFuturesExchange oExchange )
        {
            m_oExchange = oExchange;
            m_oFundingTask = FundingLoopTask();
        }


        private async Task FundingLoopTask()
        {
            while(true)
            {
                try
                {
                    IFuturesSymbol[]? aSymbols = await m_oExchange.GetSymbols();
                    if( aSymbols != null )
                    {
                        IFundingRateSnapShot[]? aSnapshots = await m_oExchange.GetFundingRates(aSymbols);
                        if( aSnapshots != null )
                        {
                            // m_aTickers.up
                            foreach( var oFunding in  aSnapshots )
                            {
                                ITicker? oTicker = null;    
                                if( m_aTickers.TryGetValue(oFunding.Symbol.Symbol, out oTicker) )
                                {
                                    ((WsTicker)oTicker).UpdateFunding(oFunding);    
                                }
                            }
                        }
                    }
                }
                catch( Exception e ) { }
                await Task.Delay(5000);
            }
        }


        public void Put(IWebsocketMessage oMessage, IFuturesSymbol[] aSymbols)
        {
            if (!(oMessage is TickerMessage)) return;
            TickerMessage oTickerMessage = (TickerMessage)oMessage; 
            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p=> p.Symbol == oTickerMessage.Symbol);
            if (oSymbol == null) return;
            ITicker oTicker = new WsTicker(oSymbol, oTickerMessage);

            m_aTickers.AddOrUpdate(oSymbol.Symbol, (_) => oTicker, (_, o) => { ((WsTicker)o).Update(oTicker); return o; } );

        }


        public ITicker[] GetData()
        {
            List<ITicker> aResult = new List<ITicker>();

            foreach (string strKey in m_aTickers.Keys)
            {
                aResult.Add(m_aTickers[strKey]);
            }
            return aResult.ToArray();
        }

        public ITicker? GetData(string strSymbol)
        {
            if (!m_aTickers.ContainsKey(strSymbol)) return null;
            return m_aTickers[strSymbol];
        }

    }
}
