using Crypto.Common;
using Crypto.Exchange.Bingx;
using Crypto.Exchange.Mexc;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;

namespace Crypto.Tests
{
    [TestClass]
    public class BingxTests
    {
        
        /*
        [TestMethod]
        public async Task BingxSpotMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();  

            ICryptoSpotExchange oSpot = new BingxSpotExchange(oSetup);

            ISymbol[]? aSymbols = await oSpot.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            ISymbol? oEth = aSymbols.FirstOrDefault(p => p.Base == "ETH" && p.Quote == "USDT");
            Assert.IsNotNull(oEth); 


        }
        */

        [TestMethod]
        public async Task BingxFuturesMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = new BingxFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            // IFuturesSymbol[] aFirst = aSymbols.Take(60).ToArray();

            IFuturesSymbol? oToFind = aSymbols.FirstOrDefault(p => p.Base == "XRP");
            Assert.IsNotNull(oToFind);


            IFundingRate[]? aHistorySingle = await oFutures.GetFundingRatesHistory(oToFind);
            Assert.IsNotNull(aHistorySingle);
            Assert.IsTrue(aHistorySingle.Length > 10);

            IFundingRate[]? aHistoryMulti = await oFutures.GetFundingRatesHistory(aSymbols.Take(30).ToArray());
            Assert.IsNotNull(aHistoryMulti);
            Assert.IsTrue(aHistoryMulti.Length > 100);


            IFundingRateSnapShot? oRateFound = await oFutures.GetFundingRates(oToFind);

            IFundingRateSnapShot[]? aRates = await oFutures.GetFundingRates(aSymbols);    
            Assert.IsNotNull(aRates);
            Assert.IsTrue(aRates.Length >= 10);

            IFundingRateSnapShot[] aOrdered = aRates.OrderByDescending(p => p.Rate).ToArray();
            Assert.IsTrue(aOrdered.Any());



        }


        [TestMethod]
        public async Task BingxFuturesBarsTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = new BingxFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oFutures.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "BTC");
            Assert.IsNotNull(oSymbol);

            IFuturesBar[]? aBars = await oFutures.BarFeeder.GetBars(oSymbol, Timeframe.H1, DateTime.Today.AddDays(-120), DateTime.Today);
            Assert.IsNotNull(aBars);
            Assert.IsTrue(aBars.Length > 24);
            Assert.IsTrue(aBars[aBars.Length - 1].DateTime.Date == DateTime.Today);

            IFuturesBar[]? aBarsMulti = await oFutures.BarFeeder.GetBars(aSymbols.Take(30).ToArray(), Timeframe.H1, DateTime.Today.AddDays(-2), DateTime.Today);
            Assert.IsNotNull(aBarsMulti);
            Assert.IsTrue(aBarsMulti.Length > 100);

        }


        [TestMethod]
        public async Task BingxAccountTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oFutures = new BingxFuturesExchange(oSetup);

            IFuturesBalance[]? aBalances = await oFutures.GetBalances();
            Assert.IsNotNull(aBalances);

        }


        [TestMethod]
        public async Task BingxMarketWebsocketTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            ICryptoFuturesExchange oExchange = new BingxFuturesExchange(oSetup);
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            ICryptoWebsocket? oWebsockets = await oExchange.CreateWebsocket();
            Assert.IsNotNull(oWebsockets);

            bool bStarted = await oWebsockets.Start();
            Assert.IsTrue(bStarted);


            int nSymbols = 30;
            await oWebsockets.SubscribeToMarket(aSymbols.Take(nSymbols).ToArray());


            await Task.Delay(30000);

            IWebsocketManager<ITicker> oTickerManager = oWebsockets.TickerManager;
            ITicker[] aTickers = oTickerManager.GetData();
            Assert.IsNotNull(aTickers);
            Assert.IsTrue(aTickers.Length == nSymbols);
            DateTime dMax = aTickers.Select(p=> p.DateTime).Max();  
            double nDiff = (DateTime.Now - dMax).TotalSeconds;

            Assert.IsTrue(nDiff <= 1.5);
            Assert.IsTrue(!aTickers.Any(p => p.FundingRate == 0));

            await Task.Delay(10000);


            await oWebsockets.Stop();

        }

    }
}