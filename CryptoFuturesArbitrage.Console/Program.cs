using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Trading.Bot;
using Crypto.Trading.Bot.Arbitrage;
using Crypto.Trading.Bot.FundingRates;
using Crypto.Trading.Bot.FundingRates.Model;
using System.Text;

namespace CryptoFuturesArbitrage.Console
{


    public class Program
    {
        private const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";
        private static bool TEST = false;



        /// <summary>
        /// Return if user hit <F> key to end
        /// </summary>
        /// <returns></returns>
        private static bool NeedsCancel()
        {
            if (System.Console.KeyAvailable)
            {
                ConsoleKeyInfo oKeyInfo = System.Console.ReadKey();
                if (oKeyInfo.KeyChar == 'F' || oKeyInfo.KeyChar == 'f') return true;
            }
            return false;

        }
        /*

        /// <summary>
        /// Bot loop
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        private static async Task DoBot( ICryptoSetup oSetup, ICommonLogger oLogger )
        {
            try
            {
                ITradingBot oBot = BotFactory.CreateFundingRatesBot(oSetup, oLogger);

                oLogger.Info("Enter main program");
                await oBot.Start();
                bool bResult = true;
                while (bResult)
                {
                    bResult = !NeedsCancel();    
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
            await Task.Delay(2000);

        }
        */


        /// <summary>
        /// Create strategy tester
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        private static async Task DoTester(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            
            DateTime dFrom = DateTime.Today.AddMonths(-1);
            DateTime dTo = DateTime.Today;

            throw new NotImplementedException();    
            /*
            IFundingTestData oTestData = TesterFactory.CreateFundingTestData(oSetup, oLogger, dFrom, dTo);

            // Load symbols
            bool bOk = await oTestData.LoadSymbols();
            if (!bOk || NeedsCancel()) return;
            // Load funding rates
            bOk = await oTestData.LoadRates();  
            if (!bOk || NeedsCancel()) return;

            Dictionary<DateTime, decimal> aFound = new Dictionary<DateTime, decimal>();
            // Start processing
            IFundingDate? oDate = null;
            while ( ( oDate = await oTestData.GetNext( (oDate == null ? null : oDate.DateTime) ) )!= null )
            {
                IFundingPair? oPair = oDate.GetBest();
                string strBest = "NONE";
                decimal nActual = 0;
                if ( oPair != null && oPair.Percent > 0.1M )
                {
                    StringBuilder oBuild = new StringBuilder();
                    oBuild.Append(" ");
                    oBuild.Append(oPair.Percent.ToString("0.##"));
                    oBuild.Append(" ");
                    oBuild.Append(oPair.BuySymbol.Base);
                    oBuild.Append(" Buy on ");
                    oBuild.Append(oPair.BuySymbol.Exchange.ExchangeType.ToString());
                    oBuild.Append(" Sell on ");
                    oBuild.Append(oPair.SellSymbol.Exchange.ExchangeType.ToString());
                    strBest = oBuild.ToString();
                    nActual = oPair.Percent;
                }
                DateTime dActual = oDate.DateTime.Date;
                if( !aFound.ContainsKey( dActual ) ) { aFound[dActual] = 0; }
                aFound[dActual] += nActual;
                oLogger.Info($"   Processing {oDate.DateTime.ToShortDateString()} {oDate.DateTime.ToShortTimeString()} {strBest}");
            }


            oLogger.Info($"==== RESULTS");
            foreach ( DateTime dDate in aFound.Keys.OrderBy(p=> p))
            {
                oLogger.Info($"    {aFound[dDate].ToString("0.##")} {dDate.ToShortDateString()}");
            }
            decimal nAverage = aFound.Values.Average();
            oLogger.Info($"==== AVERAGE : [{nAverage.ToString("0.##")}]");
            await Task.Delay(2000);
            */
        }


        private static void LogFundingPair( ICommonLogger oLogger, IFundingPair oPair )
        {

            StringBuilder oBuild = new StringBuilder();
            oBuild.Append($"Next {oPair.FundingDate.DateTime.ToShortTimeString()}: {oPair.BuySymbol.Base} {oPair.Percent.ToString("0.###")} % ");
            oBuild.Append($" Long on {oPair.BuySymbol.Exchange.ExchangeType.ToString()} Short on {oPair.SellSymbol.Exchange.ExchangeType.ToString()}");

            oLogger.Info(oBuild.ToString());

        }

        private static async Task TryClosePositions(IOppositeOrder[]? aOpposite, ICommonLogger oLogger)
        {
            if( aOpposite == null || aOpposite.Length <= 0 ) return;

            foreach( IOppositeOrder o in aOpposite )
            {
                decimal nProfit = o.Profit;
                ICloseResult oResult = await o.TryCloseMarket();

                if (oResult.ProfitOrLoss != nProfit)
                {
                    oLogger.Info($"{o.SymbolLong.Base}-{o.SymbolLong.Quote} Profit or Loss {oResult.ProfitOrLoss}");
                    if( oResult.ProfitOrLoss > 0 )
                    {
                        oLogger.Info("SUCCESSS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    }
                }
            }
        }

        private static async Task DoWebsocketFundingData(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            IFundingSocketData oSocketData = BotFactory.CreateFundingSocket(oSetup, oLogger);

            bool bStarted = await oSocketData.Start();
            if( !bStarted ) { oLogger.Error("Could not start funding socket data"); return; }


            IFundingPair? oLast = null;

            IFundingPair? oTraded;
            decimal nQuantity = 30;
            decimal nStartBalance = 0;

            await Task.Delay(5000);

            DateTime dLast = DateTime.Now;
            if (oSocketData.Websockets == null) return;
            IOppositeOrder[]? aOpposite = await ArbitrageFactory.CreateOppositeOrderFromExchanges(oSocketData.Websockets.Select(p => (IFuturesExchange)p.Exchange).ToArray());

            bool bResult = true;
            while (bResult)
            {
                bResult = !NeedsCancel();
                IFundingDate[]? aDates = await oSocketData.GetFundingDates();
                if (aDates == null) continue;

                IFundingPair? oBest = null;
                IFundingDate? oNext = await oSocketData.GetNext(null);
                while( oNext != null )
                {
                    IFundingPair? oPair = oNext.GetBest();
                    if (oPair != null)
                    {
                        if (oBest == null)
                        {
                            oBest = oPair;  
                        }
                        else if( oBest.Percent < oPair.Percent )
                        {
                            oBest = oPair;
                        }

                    }
                    oNext = await oSocketData.GetNext(oNext.DateTime);

                }

                if (oBest == null) continue;
                bool bLog = true;

                if( oLast != null )
                {
                    if( oLast.Percent == oBest.Percent )
                    {
                        if( oLast.BuySymbol.Symbol == oBest.BuySymbol.Symbol && oLast.SellSymbol.Symbol == oBest.SellSymbol.Symbol )
                        {
                            if (oLast.BuySymbol.Exchange.ExchangeType == oBest.BuySymbol.Exchange.ExchangeType && oLast.SellSymbol.Exchange.ExchangeType == oBest.SellSymbol.Exchange.ExchangeType)
                            {
                                bLog = false;   
                            }

                        }
                    }
                }

                if( bLog )
                {
                    LogFundingPair(oLogger, oBest);
                }

                if( (DateTime.Now - dLast).TotalMinutes >= 5 )
                {
                    dLast = DateTime.Now;
                    oLogger.Info("...");
                }

                if( aOpposite != null && aOpposite.Length >0 )
                {
                    await TryClosePositions(aOpposite, oLogger);
                }
                else 
                {

                }
                oLast = oBest;

                await Task.Delay(1000);
            }


            await oSocketData.Stop();

            await Task.Delay(2000);

        }



        public static async Task<int> Main(string[] args)
        {


            ICryptoSetup oSetup = CommonFactory.CreateSetup(SETUP_FILE);
            CancellationTokenSource oSource = new CancellationTokenSource();    
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingRateBot", oSource.Token);

            /*
            if (TEST) await DoTester(oSetup, oLogger);
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


