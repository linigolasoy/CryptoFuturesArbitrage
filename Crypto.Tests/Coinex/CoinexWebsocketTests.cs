﻿using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures;

namespace Crypto.Tests.Coinex
{
    [TestClass]
    public class CoinexWebsocketTests
    {
        [TestMethod]
        public async Task CoinexFundingAccountSocket()
        {
            IFuturesExchange oExchange = await CoinexCommon.CreateExchange();



            await Task.Delay(1000);
            bool bResultStart = await oExchange.Account.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(1000);
            IFuturesBalance[] aBalances = oExchange.Account.BalanceManager.GetData();
            Assert.IsTrue(aBalances.Length > 0);
        }


        [TestMethod]
        public async Task CoinexAllSymbolsSocket()
        {
            IFuturesExchange oExchange = await CoinexCommon.CreateExchange();

            await Task.Delay(1000);
            IFuturesSymbol[]? aSymbols = await oExchange.Market.GetSymbols();
            Assert.IsNotNull(aSymbols);
            bool bResultStart = await oExchange.Market.StartSockets();
            Assert.IsTrue(bResultStart);

            await Task.Delay(4000);

            Assert.IsNotNull(oExchange.Market.Websocket);
            Assert.IsTrue(oExchange.Market.Websocket.OrderbookManager.Count == aSymbols.Length);
            Assert.IsTrue(oExchange.Market.Websocket.FundingRateManager.Count == aSymbols.Length);

            IOrderbook[]? aData = oExchange.Market.Websocket.OrderbookManager.GetData();
            Assert.IsNotNull(aData);
            Assert.IsTrue(aData.Length == aSymbols.Length);

            IFundingRate[]? aFunding = oExchange.Market.Websocket.FundingRateManager.GetData();
            Assert.IsNotNull(aFunding);
            Assert.IsTrue(aFunding.Length == aSymbols.Length);

            bool bResultStop = await oExchange.Market.EndSockets();
            Assert.IsTrue(bResultStop);
        }
    }
}
