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


            // Create websockets
            bool bResultSocket = await oExchangeLong.Market.StartSockets();
            Assert.IsTrue(bResultSocket);
            bResultSocket = await oExchangeShort.Market.StartSockets();
            Assert.IsTrue(bResultSocket);

            bResultSocket = await oExchangeLong.Account.StartSockets();
            Assert.IsTrue(bResultSocket);
            bResultSocket = await oExchangeShort.Account.StartSockets();
            Assert.IsTrue(bResultSocket);

            IFuturesSymbol? oSymbolLong = aSymbolsLong.FirstOrDefault(p=> p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolLong);
            IFuturesSymbol? oSymbolShort = aSymbolsShort.FirstOrDefault(p => p.Base == CURRENCY && p.Quote == USDT);
            Assert.IsNotNull(oSymbolShort);

            Assert.IsNotNull(oExchangeLong.Market.Websocket);
            Assert.IsNotNull(oExchangeShort.Market.Websocket);

            await Task.Delay(5000);
            IOrderbook? oOrderbookLong = oExchangeLong.Market.Websocket.OrderbookManager.GetData(oSymbolLong.Symbol);
            Assert.IsNotNull(oOrderbookLong);
            IOrderbook? oOrderbookShort = oExchangeShort.Market.Websocket.OrderbookManager.GetData(oSymbolShort.Symbol);
            Assert.IsNotNull(oOrderbookShort);



            /*
            IOppositeOrder[]? aOpposite = await ArbitrageFactory.CreateOppositeOrderFromExchanges(new ICryptoFuturesExchange[] { oExchangeShort, oExchangeLong });
            Assert.IsNotNull(aOpposite);
            Assert.IsTrue(aOpposite.Any());


            foreach (var oOpposite in aOpposite)
            {
                ICloseResult oResult =  await oOpposite.TryCloseMarket();
            }
            */
            IOppositeOrder oOrder = ArbitrageFactory.CreateOppositeOrder(oSymbolLong, oSymbolShort, 10, DateTime.Now);

            bool bResult = await oOrder.TryOpenLimit(13);
            Assert.IsTrue(bResult);

            await Task.Delay(3000);

        }
    }
}
