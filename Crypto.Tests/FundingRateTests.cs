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
using Crypto.Interface.Futures.Market;

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
            const string CURRENCY = "VANA";
            IFuturesExchange oExchangeLong = await ExchangeFactory.CreateExchange(ExchangeType.BitgetFutures, oSetup);
            IFuturesExchange oExchangeShort = await ExchangeFactory.CreateExchange(ExchangeType.CoinExFutures, oSetup);

            IFuturesSymbol[]? aSymbolsLong = await oExchangeLong.Market.GetSymbols();
            Assert.IsNotNull(aSymbolsLong);
            IFuturesSymbol[]? aSymbolsShort = await oExchangeShort.Market.GetSymbols();
            Assert.IsNotNull(aSymbolsShort);


            IFuturesSymbol? oSymbolLong = aSymbolsLong.FirstOrDefault(p=> p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolLong);
            IFuturesSymbol? oSymbolShort = aSymbolsShort.FirstOrDefault(p => p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolShort);

            /*
            IOppositeOrder[]? aOpposite = await ArbitrageFactory.CreateOppositeOrderFromExchanges(new ICryptoFuturesExchange[] { oExchangeShort, oExchangeLong });
            Assert.IsNotNull(aOpposite);
            Assert.IsTrue(aOpposite.Any());


            foreach (var oOpposite in aOpposite)
            {
                ICloseResult oResult =  await oOpposite.TryCloseMarket();
            }
            */
            IOppositeOrder oOrder = ArbitrageFactory.CreateOppositeOrder(oSymbolLong, oSymbolShort);

            oOrder.Quantity = 5;
            oOrder.Leverage = 10;
            bool bResult = await oOrder.TryOpenMarket();
            Assert.IsTrue(bResult);

            await Task.Delay(3000);

        }
    }
}
