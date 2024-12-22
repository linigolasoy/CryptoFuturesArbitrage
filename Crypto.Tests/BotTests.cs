using Crypto.Common;
using Crypto.Exchange.Bingx;
using Crypto.Exchange.Mexc;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;

namespace Crypto.Tests
{
    [TestClass]
    public class BotTests
    {

        [TestMethod]
        public async Task MatchFundingTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchangeMexc  = new MexcFuturesExchange(oSetup);
            ICryptoFuturesExchange oExchangeBingx = new BingxFuturesExchange(oSetup);


            IFuturesSymbol[]? aSymbolsMexc = await oExchangeMexc.GetSymbols();  
            Assert.IsNotNull(aSymbolsMexc);

            IFuturesSymbol[]? aSymbolsBingx = await oExchangeBingx.GetSymbols();
            Assert.IsNotNull(aSymbolsBingx);

            string strBaseFind  = "LUNC";
            string strQuoteFind = "USDT";

            IFuturesSymbol? oSymbolMexc = aSymbolsMexc.FirstOrDefault(p => p.Base == strBaseFind && p.Quote == strQuoteFind);
            Assert.IsNotNull(oSymbolMexc);

            IFuturesSymbol? oSymbolBingx = aSymbolsBingx.FirstOrDefault(p => p.Base == strBaseFind && p.Quote == strQuoteFind);
            Assert.IsNotNull(oSymbolBingx);

            IFundingRateSnapShot? oShotMex = await oExchangeMexc.GetFundingRates(oSymbolMexc);
            Assert.IsNotNull(oShotMex);

            IFundingRateSnapShot? oShotBing = await oExchangeBingx.GetFundingRates(oSymbolBingx);
            Assert.IsNotNull(oShotBing);

            IFundingRateSnapShot[]? aFundingMexc = await oExchangeMexc.GetFundingRates(aSymbolsMexc);
            Assert.IsNotNull(aFundingMexc);

            IFundingRateSnapShot[]? aFundingBingx = await oExchangeBingx.GetFundingRates(aSymbolsBingx);
            Assert.IsNotNull(aFundingBingx);

            SortedDictionary<decimal, IFundingRate[]> aSorted = new SortedDictionary<decimal, IFundingRate[]>();
            decimal nBestFound = 0;
            foreach( IFundingRateSnapShot oFundingBing in aFundingBingx )
            {
                string strBase = oFundingBing.Symbol.Base;
                string strQuote = oFundingBing.Symbol.Quote;

                IFundingRateSnapShot? oFundingMexc = aFundingMexc.Where( p=> p.Symbol.Base == strBase && p.Symbol.Quote == strQuote ).FirstOrDefault();
                if (oFundingMexc == null) continue;

                decimal nRate = Math.Abs(oFundingBing.Rate - oFundingMexc.Rate) * 100M;

                if (nRate > nBestFound)
                {
                    nBestFound = nRate;
                }

                if ( nRate > 0.10M )
                {
                    aSorted[nRate] = new IFundingRate[] { oFundingBing, oFundingMexc };
                }

            }
            
            Assert.IsTrue( aSorted.Count > 0 ); 

        }


        /// <summary>
        /// Evaluates funding rates using websockets
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MatchFundingWebsocketTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchangeMexc = new MexcFuturesExchange(oSetup);
            ICryptoFuturesExchange oExchangeBingx = new BingxFuturesExchange(oSetup);


            IFuturesSymbol[]? aSymbolsMexc = await oExchangeMexc.GetSymbols();
            Assert.IsNotNull(aSymbolsMexc);

            IFuturesSymbol[]? aSymbolsBingx = await oExchangeBingx.GetSymbols();
            Assert.IsNotNull(aSymbolsBingx);


            ICryptoWebsocket? oWsMexc = await oExchangeMexc.CreateWebsocket();   
            Assert.IsNotNull(oWsMexc);
            ICryptoWebsocket? oWsBingx = await oExchangeBingx.CreateWebsocket();
            Assert.IsNotNull(oWsBingx);

            aSymbolsBingx = aSymbolsBingx.Where(p => aSymbolsMexc.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();

            IFundingRateSnapShot[]? aFundingsBingx = await oExchangeBingx.GetFundingRates(aSymbolsBingx);
            Assert.IsNotNull (aFundingsBingx);

            
            aSymbolsMexc = aSymbolsMexc.Where(p => aSymbolsBingx.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();

            aSymbolsBingx = aFundingsBingx.OrderByDescending(p => Math.Abs(p.Rate)).Take(200).Select(p => p.Symbol).ToArray();


            await oWsMexc.Start();
            await oWsBingx.Start();


            await oWsMexc.SubscribeToMarket(aSymbolsMexc);
            await oWsBingx.SubscribeToMarket(aSymbolsBingx);


            await Task.Delay(20000);

            IWebsocketManager<ITicker> oMexcManager = oWsMexc.TickerManager;
            IWebsocketManager<ITicker> oBingxManager = oWsBingx.TickerManager;


            SortedDictionary<decimal, ITicker[]> aSorted = new SortedDictionary<decimal, ITicker[]>();
            decimal nBestFound = 0;

            ITicker[] aTickersBingx = oBingxManager.GetData();
            ITicker[] aTickersMexc  = oMexcManager.GetData();
            foreach (ITicker oTickerBing in aTickersBingx)
            {
                string strBase = oTickerBing.Symbol.Base;
                string strQuote = oTickerBing.Symbol.Quote;

                ITicker? oTickerMexc = aTickersMexc.Where(p => p.Symbol.Base == strBase && p.Symbol.Quote == strQuote).FirstOrDefault();
                if (oTickerMexc == null) continue;


                decimal nRate = Math.Abs(oTickerBing.FundingRate - oTickerMexc.FundingRate) * 100M;

                if (nRate > nBestFound)
                {
                    nBestFound = nRate;
                }

                
                if (nRate > 0.10M)
                {
                    aSorted[nRate] = new ITicker[] { oTickerBing, oTickerMexc };
                }
                

            }

            Assert.IsTrue(aSorted.Count > 0);

        }


    }
}