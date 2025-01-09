using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using Crypto.Tests.Coinex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Tests.Coinex
{
    [TestClass]
    public class CoinexTradingTests
    {


        [TestMethod]
        public async Task CoinexLeverageTests()
        {
            ICryptoFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            IFuturesSymbol? oBtc = aSymbols.FirstOrDefault(p => p.Base == "BTC" && p.Quote == "USDT");
            Assert.IsNotNull(oBtc);

            IFuturesLeverage? oLeverageBtc = await oExchange.Trading.GetLeverage(oBtc);
            Assert.IsNotNull(oLeverageBtc);

            IFuturesLeverage[]? aLeverages = await oExchange.Trading.GetLeverages(aSymbols.Take(30).ToArray());
            Assert.IsNotNull(aLeverages);

            bool bResult = await oExchange.Trading.SetLeverage(oBtc, 5);
            Assert.IsTrue(bResult);


        }

        [TestMethod]
        public async Task CoinexBasicOrderTests()
        {
            ICryptoFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            await Task.Delay(1000);
            IFuturesSymbol[]? aSymbols = await oExchange.GetSymbols();
            Assert.IsNotNull(aSymbols);

            IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
            Assert.IsNotNull(oSymbol);


            bool bResult = await oExchange.Trading.SetLeverage(oSymbol, 5);
            Assert.IsTrue(bResult);

            IFuturesLeverage? oNewLeverage = await oExchange.Trading.GetLeverage(oSymbol);
            Assert.IsNotNull(oNewLeverage);
            Assert.IsTrue(oNewLeverage.LongLeverage == 5);
            Assert.IsTrue(oNewLeverage.ShortLeverage == 5);
            decimal nPrice = 0.5M;
            IFuturesOrder? oOrder = await oExchange.Trading.CreateLimitOrder(oSymbol, true, true, 10, nPrice);
            Assert.IsNotNull(oOrder);
            await Task.Delay(2000);
            IFuturesOrder[] aOrdersWs = oExchange.Account.OrderManager.GetData();
            Assert.IsNotNull(aOrdersWs);
            IFuturesOrder? oOrderWs = aOrdersWs.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oOrderWs);

            await Task.Delay(1000);
            IFuturesOrder[]? aOrders = await oExchange.Trading.GetOrders();
            Assert.IsNotNull(aOrders);


            Assert.IsTrue(aOrders.Any());

            IFuturesOrder? oFound = aOrders.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oFound);

            bool bCanceled = await oExchange.Trading.CancelOrder(oFound);
            Assert.IsTrue(bCanceled);
            await Task.Delay(1000); 
            Assert.IsTrue(oOrderWs.OrderStatus == FuturesOrderStatus.Canceled);

            decimal nQuantity = 5;
            IFuturesOrder? oMarketOpen = await oExchange.Trading.CreateMarketOrder(oSymbol, true, true, nQuantity);
            Assert.IsNotNull(oMarketOpen);
            await Task.Delay(1000);
            IFuturesPosition[] aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            IFuturesPosition? oPosition = aRest.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
            Assert.IsNotNull(oPosition);
            Assert.IsTrue(oPosition.Quantity == nQuantity);

            IFuturesOrder? oMarketOpen2 = await oExchange.Trading.CreateMarketOrder(oSymbol, true, true, nQuantity);
            Assert.IsNotNull(oMarketOpen2);
            await Task.Delay(1000);
            Assert.IsTrue(oPosition.Quantity == nQuantity * 2M);


            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();
            Assert.IsNotNull(aPositions);
            Assert.IsTrue(aPositions.Any());

            IFuturesOrder? oMarketClose = await oExchange.Trading.CreateMarketOrder(oSymbol, false, true, nQuantity * 2M);
            Assert.IsNotNull(oMarketClose);
            await Task.Delay(2000);
            aRest = oExchange.Account.PositionManager.GetData();
            Assert.IsNotNull(aRest);
            Assert.IsTrue(!aRest.Any(p => p.Symbol.Symbol == oSymbol.Symbol));
        }

        [TestMethod]
        public async Task CoinexAccountTests()
        {
            ICryptoFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            IFuturesBalance[]? aBalances = await oExchange.Account.GetBalances();
            Assert.IsNotNull(aBalances);

            IFuturesPosition[]? aPositions = await oExchange.Account.GetPositions();
            Assert.IsNotNull(aPositions);

        }
    }
}
