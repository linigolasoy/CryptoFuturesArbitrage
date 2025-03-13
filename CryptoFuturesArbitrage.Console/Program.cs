using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Trading.Bot;
using Crypto.Trading.Bot.Arbitrage;
using Crypto.Trading.Bot.BackTest;
using Crypto.Trading.Bot.Common;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.Model;
using Serilog.Core;
using System.Text;

namespace CryptoFuturesArbitrage.Console
{


    public class Program
    {
        private const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";
        private static bool TEST = false;


        private enum eAction
        {
            None,
            Cancel,
            Close
        }

        /// <summary>
        /// Return if user hit <F> key to end
        /// </summary>
        /// <returns></returns>
        private static eAction NeedsAction()
        {
            eAction eResult = eAction.None; 
            if (System.Console.KeyAvailable)
            {
                ConsoleKeyInfo oKeyInfo = System.Console.ReadKey();
                if (oKeyInfo.KeyChar == 'F' || oKeyInfo.KeyChar == 'f') eResult = eAction.Cancel;
                if (oKeyInfo.KeyChar == 'C' || oKeyInfo.KeyChar == 'c') eResult = eAction.Close;
            }
            return eResult;

        }

        /// <summary>
        /// Bot loop
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        private static async Task DoBot( ICryptoSetup oSetup )
        {
            CancellationTokenSource oSource = new CancellationTokenSource();
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "SpreadRateBot", oSource.Token);
            try
            {
                // ITradingBot oBot = BotFactory.CreateFuturesArbitrageBot(oSetup, oLogger);
                // ITradingBot oBot = BotFactory.CreateFundingRatesBot(oSetup, oLogger);
                ITradingBot oBot = BotFactory.CreateSpreadBot(oSetup, oLogger);
                // ITradingBot oBot = new OppositeOrderTester(oSetup, oLogger);

                oLogger.Info("Enter main program");
                await oBot.Start();
                eAction eResult = eAction.None;
                while (eResult != eAction.Cancel)
                {
                    eResult = NeedsAction();    
                    await Task.Delay(500);
                }

                oLogger.Info("Exit main program");
                await Task.Delay(1000);
                await oBot.Stop();
            }
            catch (Exception ex)
            {
                oLogger.Error("Error on main program", ex);
            }
            oSource.Cancel();   
            await Task.Delay(2000);

        }




        /// <summary>
        /// Create strategy tester
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        private static async Task DoTester(ICryptoSetup oSetup)
        {
            CancellationTokenSource oSource = new CancellationTokenSource();    
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingRateTester", oSource.Token);

            DateTime dFrom = DateTime.Today.AddDays(-7);
            DateTime dTo = DateTime.Today;


            IBackTester oTester = TesterFactory.CreateFundingRateTester(oSetup, oLogger, dFrom, dTo);

            bool bOk = await oTester.Start();
            if (!bOk) return;

            List<IBackTestResult> aResults = new List<IBackTestResult>();

            while( !oTester.Ended )
            {
                IBackTestResult? oResult = await oTester.Step();
                if (oResult != null)
                {
                    aResults.Add(oResult);

                    decimal nTotalProfit = aResults.Select(p => p.Profit).Sum();
                    decimal nPercentWon = Math.Round((100 * aResults.Where(p=> p.Profit > 0).Count()) / (decimal)aResults.Count,2); 
                    oLogger.Info($"  {oResult.StartDate.ToShortDateString()} {oResult.StartDate.ToShortTimeString()} ({nPercentWon}%) ({oResult.Profit}) ==> {nTotalProfit}");
                }
                else break;
            }
            await oTester.Stop();
            oSource.Cancel();
            await Task.Delay(1000);
        }


        private static void LogFundingPair( ICommonLogger oLogger, IFundingPair oPair )
        {

            StringBuilder oBuild = new StringBuilder();
            oBuild.Append($"Next {oPair.FundingDate.DateTime.ToShortTimeString()}: {oPair.BuySymbol.Base} {oPair.Percent.ToString("0.###")} % ");
            oBuild.Append($" Long on {oPair.BuySymbol.Exchange.ExchangeType.ToString()} Short on {oPair.SellSymbol.Exchange.ExchangeType.ToString()}");

            oLogger.Info(oBuild.ToString());

        }

        private static async Task<decimal> TryClosePositions(IOppositeOrder[]? aOpposite, ICommonLogger oLogger, decimal nLastProfit, bool bForce )
        {
            if( aOpposite == null || aOpposite.Length <= 0 ) return 0;
            decimal nResult = 0;
            foreach( IOppositeOrder o in aOpposite )
            {
                if( o.Closed ) continue;    
                ICloseResult oResult = await o.TryCloseMarket(bForce);
                decimal nProfit = Math.Round( o.Profit,3);
                nResult += nProfit;
                if( nProfit != nLastProfit )
                {
                    oLogger.Info($"{o.LongData.Symbol.Base}-{o.ShortData.Symbol.Quote} Profit or Loss {nProfit}");
                }
                if (oResult.Success )
                {
                    if( oResult.ProfitOrLoss > 0 )
                    {
                        oLogger.Info("SUCCESSS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    }
                }
            }
            return nResult;
        }

        private static async Task DoWebsocketFundingData(ICryptoSetup oSetup)
        {
            CancellationTokenSource oSource = new CancellationTokenSource();
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingSocket", oSource.Token);

            IFundingSocketData oSocketData = BotFactory.CreateFundingSocket(oSetup, oLogger);

            try
            {
                bool bStarted = await oSocketData.Start();
                if (!bStarted) { oLogger.Error("Could not start funding socket data"); return; }


                IFundingPair? oLast = null;

                IFundingPair? oTraded;
                decimal nQuantity = 30;
                decimal nStartBalance = 0;
                decimal nLastProfit = -9E10M;
                await Task.Delay(5000);

                DateTime dLast = DateTime.Now;
                if (oSocketData.Websockets == null) return;
                IOppositeOrder[]? aOpposite = await ArbitrageFactory.CreateOppositeOrderFromExchanges(oSocketData.Websockets.Select(p => (IFuturesExchange)p.Exchange).ToArray(), oSetup);

                eAction eResult = eAction.None;
                while (eResult != eAction.Cancel)
                {
                    eResult = NeedsAction();
                    IFundingDate[]? aDates = await oSocketData.GetFundingDates();
                    if (aDates == null) continue;
                    if( eResult == eAction.Close )
                    {
                        await TryClosePositions(aOpposite, oLogger, nLastProfit, true);
                        await Task.Delay(200);
                        continue;
                    }
                    IFundingPair? oBest = null;
                    IFundingDate? oNext = await oSocketData.GetNext(null);
                    while (oNext != null)
                    {
                        IFundingPair? oPair = oNext.GetBest();
                        if (oPair != null)
                        {
                            if (oBest == null)
                            {
                                oBest = oPair;
                            }
                            else if (oBest.Percent < oPair.Percent)
                            {
                                oBest = oPair;
                            }

                        }
                        oNext = await oSocketData.GetNext(oNext.DateTime);

                    }

                    if (oBest == null) continue;
                    bool bLog = true;

                    if (oLast != null)
                    {
                        if (oLast.Percent == oBest.Percent)
                        {
                            if (oLast.BuySymbol.Symbol == oBest.BuySymbol.Symbol && oLast.SellSymbol.Symbol == oBest.SellSymbol.Symbol)
                            {
                                if (oLast.BuySymbol.Exchange.ExchangeType == oBest.BuySymbol.Exchange.ExchangeType && oLast.SellSymbol.Exchange.ExchangeType == oBest.SellSymbol.Exchange.ExchangeType)
                                {
                                    bLog = false;
                                }

                            }
                        }
                    }

                    if (bLog)
                    {
                        LogFundingPair(oLogger, oBest);
                    }

                    if ((DateTime.Now - dLast).TotalMinutes >= 5)
                    {
                        dLast = DateTime.Now;
                        oLogger.Info("...");
                    }

                    if (aOpposite != null && aOpposite.Length > 0)
                    {
                        decimal nActual = await TryClosePositions(aOpposite, oLogger, nLastProfit, false);
                        nLastProfit = nActual;
                    }
                    else
                    {

                    }
                    oLast = oBest;

                    await Task.Delay(1000);
                }


                await oSocketData.Stop();
            }
            catch (Exception ex)
            {
                oLogger.Error("Error en main function", ex);
            }

            await Task.Delay(2000);

        }


        private static async Task DoSocketManager(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            ISocketManager oManager = BotFactory.CreateSocketManager(oSetup, oLogger);

            bool bStarted = await oManager.Start();

            await Task.Delay(5000);

            var aFundings = oManager.GetFundingRates();
            var aOrderbooks = oManager.GetOrderbooks();

            decimal nBestArbitrage = -99.0M;
            decimal nMoney = 1000;
            int nLoops = 10000;

            while (nLoops-- >= 0)
            {
                foreach (var eTypeBuy in aOrderbooks.Keys)
                {
                    foreach (var eTypeSell in aOrderbooks.Keys)
                    {
                        if (eTypeBuy == eTypeSell) continue;
                        foreach (var oBookBuy in aOrderbooks[eTypeBuy])
                        {
                            IOrderbookPrice? oPriceBuy = oBookBuy.GetBestPrice(true, 0, null, nMoney);
                            if (oPriceBuy == null) continue;
                            IOrderbook? oBookSell = aOrderbooks[eTypeSell].FirstOrDefault(p => p.Symbol.Base == oBookBuy.Symbol.Base && p.Symbol.Quote == oBookBuy.Symbol.Quote);
                            if (oBookSell == null) continue;
                            IOrderbookPrice? oPriceSell = oBookSell.GetBestPrice(false, 0, null, nMoney);
                            if (oPriceSell == null) continue;
                            if (oPriceBuy.Price > oPriceSell.Price) continue;

                            decimal nPercent = Math.Round((oPriceSell.Price - oPriceBuy.Price) * 100M / oPriceBuy.Price, 3);
                            if (nPercent > nBestArbitrage)
                            {
                                nBestArbitrage = nPercent;
                                oLogger.Info($"Best {nBestArbitrage}");
                            }

                        }

                    }
                }

                if( nLoops % 100 == 0 )
                {
                    oLogger.Info($"  {nLoops} Loop");
                }
                await Task.Delay(200);
            }
            await Task.Delay(2000);


            await oManager.Stop();

        }

        private static async Task DoMoney(ICryptoSetup oSetup)
        {
            CancellationTokenSource oSource = new CancellationTokenSource();
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "MoneyTester", oSource.Token);

            MoneyTester oTester = new MoneyTester(oSetup, oLogger);

            await oTester.Run();

            oSource.Cancel();
            await Task.Delay(1000);

        }
        public static async Task<int> Main(string[] args)
        {


            ICryptoSetup oSetup = CommonFactory.CreateSetup(SETUP_FILE);

            // await DoWebsocketFundingData(oSetup);
            await DoBot(oSetup);
            // await DoTester(oSetup);
            // await DoSocketManager(oSetup, oLogger);
            // await DoMoney(oSetup);
            /*
            if (TEST) 
            else
            {
                // await DoWebsocketFundingData(oSetup, oLogger);
                await DoBot(oSetup, oLogger);
            }
            */
            return 0;
        }

    }
}


