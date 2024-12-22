using Crypto.Common;
using Crypto.Interface;
using Crypto.Trading.Bot;

namespace CryptoFuturesArbitrage.Console
{
    public class Program
    {
        private const string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";
        public static async Task<int> Main(string[] args)
        {


            ICryptoSetup oSetup = CommonFactory.CreateSetup(SETUP_FILE);
            CancellationTokenSource oSource = new CancellationTokenSource();    
            ICommonLogger oLogger = CommonFactory.CreateLogger(oSetup, "FundingRateBot", oSource.Token);

            try
            {
                ITradingBot oBot = BotFactory.CreateFundingRatesBot(oSetup, oLogger);

                oLogger.Info("Enter main program");
                await oBot.Start();
                bool bResult = true;
                while (bResult)
                {
                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo oKeyInfo = System.Console.ReadKey();
                        if (oKeyInfo.KeyChar == 'F' || oKeyInfo.KeyChar == 'f') { bResult = false; break; }
                    }
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

            return 0;
        }

    }
}


