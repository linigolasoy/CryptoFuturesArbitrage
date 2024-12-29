using Crypto.Common;
using Crypto.Exchanges.All;
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

            ICryptoFuturesExchange oExchangeCoinex = ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, oSetup);
            ICryptoFuturesExchange oExchangeBingx = ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);


            IFuturesSymbol[]? aSymbolsCoinex = await oExchangeCoinex.GetSymbols();  
            Assert.IsNotNull(aSymbolsCoinex);

            IFuturesSymbol[]? aSymbolsBingx = await oExchangeBingx.GetSymbols();
            Assert.IsNotNull(aSymbolsBingx);


            // Common symbols 
            aSymbolsCoinex = aSymbolsCoinex.Where(p => aSymbolsBingx.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();
            aSymbolsBingx = aSymbolsBingx.Where(p => aSymbolsCoinex.Any(q => p.Base == q.Base && p.Quote == q.Quote)).ToArray();


            // Websockets y subscribe symbols
            ICryptoWebsocket? oWsCoinex = await oExchangeCoinex.CreateWebsocket();
            Assert.IsNotNull(oWsCoinex);

            ICryptoWebsocket? oWsBingx = await oExchangeBingx.CreateWebsocket();
            Assert.IsNotNull(oWsBingx);

            await oWsCoinex.Start();
            await oWsCoinex.SubscribeToMarket(aSymbolsCoinex);

            await oWsBingx.Start();
            await oWsBingx.SubscribeToMarket(aSymbolsBingx);


            await Task.Delay(10000);

            IFundingRateSnapShot[] aFundingsCoinex = oWsCoinex.FundingRateManager.GetData();
            IFundingRateSnapShot[] aFundingsBingx  = oWsBingx.FundingRateManager.GetData();



            SortedDictionary<decimal, IFundingRate[]> aSorted = new SortedDictionary<decimal, IFundingRate[]>();
            decimal nBestFound = 0;
            foreach( IFundingRateSnapShot oFundingBing in aFundingsBingx)
            {
                string strBase = oFundingBing.Symbol.Base;
                string strQuote = oFundingBing.Symbol.Quote;

                IFundingRateSnapShot? oFundingCoinex = aFundingsCoinex.Where( p=> p.Symbol.Base == strBase && p.Symbol.Quote == strQuote ).FirstOrDefault();
                if (oFundingCoinex == null) continue;

                decimal nRate = 0;
                if (oFundingBing.NextSettle < oFundingCoinex.NextSettle)
                {
                    Console.WriteLine("Pinx");
                }
                else if (oFundingBing.NextSettle > oFundingCoinex.NextSettle)
                {
                    Console.WriteLine("Poinex");
                }
                else
                {
                    nRate = Math.Abs(oFundingBing.Rate - oFundingCoinex.Rate) * 100M;
                }

                if (nRate > nBestFound)
                {
                    nBestFound = nRate;
                }

                if ( nRate > 0.10M )
                {
                    aSorted[nRate] = new IFundingRate[] { oFundingBing, oFundingCoinex };
                }

            }
            
            Assert.IsTrue( aSorted.Count > 0 ); 

        }
        

        /*
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

            IWebsocketManager<IOrderbook> oMexcManager = oWsMexc.OrderbookManager;
            IWebsocketManager<IOrderbook> oBingxManager = oWsBingx.OrderbookManager;


            SortedDictionary<decimal, IOrderbook[]> aSorted = new SortedDictionary<decimal, IOrderbook[]>();
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
        */

    }
}