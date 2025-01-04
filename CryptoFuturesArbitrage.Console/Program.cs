using Crypto.Common;
using Crypto.Interface;
using Crypto.Trading.Bot;
using Crypto.Trading.Bot.FundingRates;
using System.Text;

namespace CryptoFuturesArbitrage.Console
{


    public class Program
    {
        private const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";
        private static bool TEST = true;



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
                ITradingBot oBot = BotFactory.CreateFundingRatesBot(oSetup, oLogger, TEST);

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


        /// <summary>
        /// Create strategy tester
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        private static async Task DoTester(ICryptoSetup oSetup, ICommonLogger oLogger)
        {
            DateTime dFrom = DateTime.Today.AddMonths(-2);
            DateTime dTo = DateTime.Today;
            IFundingTestData oTestData = TesterFactory.CreateFundingTestData(oSetup, oLogger, dFrom, dTo);

            // Load symbols
            bool bOk = await oTestData.LoadSymbols();
            if (!bOk || NeedsCancel()) return;
            // Load funding rates
            bOk = await oTestData.LoadRates();  
            if (!bOk || NeedsCancel()) return;

            DateTime? dActual = null;
            List<decimal> aFound = new List<decimal>();
            // Start processing
            IFundingDate? oDate = null;
            while ( ( oDate = oTestData.GetNext( (oDate == null ? null : oDate.DateTime) ) )!= null )
            {
                IFundingPair? oPair = oDate.GetBest();
                string strBest = "NONE";
                decimal nActual = 0;
                if ( oPair != null && oPair.Percent > 0 )
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
                if( dActual == null )
                {
                    dActual = oDate.DateTime.Date;
                    aFound.Add(nActual);
                }
                else
                {
                    if( dActual.Value.Date != oDate.DateTime.Date )
                    {
                        dActual = oDate.DateTime.Date;
                        aFound.Add(nActual);
                    }
                    else
                    {
                        aFound[aFound.Count - 1] += nActual;
                    }
                }
                oLogger.Info($"   Processing {oDate.DateTime.ToShortDateString()} {oDate.DateTime.ToShortTimeString()} {strBest}");
            }

            decimal nAverage = aFound.Average();
            await Task.Delay(2000);
        }


        public static async Task<int> Main(string[] args)
        {


            ICryptoSetup oSetup = CommonFactory.CreateSetup(SETUP_FILE);
            CancellationTokenSource oSource = new CancellationTokenSource();    
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingRateBot", oSource.Token);

            if( TEST ) await DoTester(oSetup, oLogger); 
            else await DoBot(oSetup, oLogger);  

            return 0;
        }

    }
}


