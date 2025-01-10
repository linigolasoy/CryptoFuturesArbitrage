using Crypto.Common;
using Crypto.Exchanges.All;
using Crypto.Interface.Futures;
using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Trading.Bot.Arbitrage;

namespace Crypto.Tests
{
    [TestClass]
    public class FundingRateTests
    {
        [TestMethod]
        public async Task OppositePairTest()
        {
            ICryptoSetup oSetup = CommonFactory.CreateSetup(TestConstants.SETUP_FILE);

            const string USDT = "USDT";
            const string CURRENCY = "GAS";
            ICryptoFuturesExchange oExchangeLong = await ExchangeFactory.CreateExchange(ExchangeType.BingxFutures, oSetup);
            ICryptoFuturesExchange oExchangeShort = await ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, oSetup);

            IFuturesSymbol[]? aSymbolsLong = await oExchangeLong.GetSymbols();
            Assert.IsNotNull(aSymbolsLong);
            IFuturesSymbol[]? aSymbolsShort = await oExchangeShort.GetSymbols();
            Assert.IsNotNull(aSymbolsShort);


            IFuturesSymbol? oSymbolLong = aSymbolsLong.FirstOrDefault(p=> p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolLong);
            IFuturesSymbol? oSymbolShort = aSymbolsShort.FirstOrDefault(p => p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolShort);
            await Task.Delay(2000);
            IOppositeOrder oOrder = ArbitrageFactory.CreateOppositeOrder(oSymbolLong, oSymbolShort);
            oOrder.Quantity = 5;
            oOrder.Leverage = 10;
            bool bResult = await oOrder.TryOpenMarket();
            Assert.IsTrue(bResult);

        }
    }
}
