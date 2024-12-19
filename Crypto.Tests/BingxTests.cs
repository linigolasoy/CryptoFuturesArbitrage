using Crypto.Common;
using Crypto.Exchange.Bingx;
using Crypto.Exchange.Mexc;
using Crypto.Interface;

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
            ICryptoSetup oSetup = CommonFactory.CreateSetup();

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

            IFundingRateSnapShot[]? aRates = await oFutures.GetFundingRates(aSymbols.Take(60).ToArray());    
            Assert.IsNotNull(aRates);
            Assert.IsTrue(aRates.Length >= 10);

            IFundingRateSnapShot[] aOrdered = aRates.OrderByDescending(p => p.Rate).ToArray();
            Assert.IsTrue(aOrdered.Any());



        }





    }
}