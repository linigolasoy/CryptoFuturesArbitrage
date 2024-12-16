using Crypto.Common;
using Crypto.Exchange.Mexc;
using Crypto.Interface;

namespace Crypto.Tests
{
    [TestClass]
    public class MexcTests
    {
        [TestMethod]
        public async Task MexcSpotMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();  

            ICryptoSpotExchange oSpot = new MexcSpotExchange(oSetup);

            ISymbol[]? aSymbols = await oSpot.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);

            ISymbol? oEth = aSymbols.FirstOrDefault(p => p.Base == "ETH" && p.Quote == "USDT");
            Assert.IsNotNull(oEth); 


        }

        [TestMethod]
        public async Task MexcFuturesMarketDataTests()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup();

            ICryptoFuturesExchange oSpot = new MexcFuturesExchange(oSetup);

            IFuturesSymbol[]? aSymbols = await oSpot.GetSymbols();
            Assert.IsNotNull(aSymbols);
            Assert.IsTrue(aSymbols.Length > 100);


        }

    }
}