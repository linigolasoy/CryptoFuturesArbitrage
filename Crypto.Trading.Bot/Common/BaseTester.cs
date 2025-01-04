using Crypto.Exchanges.All;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    internal class BaseTester : ITradingBot
    {

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;

        private const int MAX_SYMBOLS = 30;
        public BaseTester( ICryptoSetup oSetup, ICommonLogger oLogger ) 
        {
            Setup = oSetup; 
            Logger = oLogger;   
        }
        public ICryptoSetup Setup { get; }

        private List<IBotExchangeData> m_aData = new List<IBotExchangeData>();

        public IBotExchangeData[] ExchangeData { get=> m_aData.ToArray(); }

        public IBotStrategy? Strategy { get; internal set; } = null;

        public ICommonLogger Logger { get; }




        private async Task AddSingleExchange( ExchangeType eType )
        {
            Logger.Info($"   Load symbols for {eType.ToString()}...");
            BaseExchangeData oData = new BaseExchangeData(ExchangeFactory.CreateExchange(eType, Setup));
            IFuturesSymbol[]? aSymbols = await oData.Exchange.GetSymbols();
            if (aSymbols == null) return;
            List<IBotSymbolData> aSymbolData = new List<IBotSymbolData>();
            foreach (var aSymbol in aSymbols.OrderBy(p=> p.Base).Take(MAX_SYMBOLS)) 
            {
                IBotSymbolData? oSymbolData = Strategy!.CreateSymbolData(oData, aSymbol);
                if (oSymbolData == null) continue;
                aSymbolData.Add(oSymbolData);
            }
            oData.Symbols = aSymbolData.ToArray();
            m_aData.Add(oData); 

        }
        /// <summary>
        /// Create exchanges
        /// </summary>
        /// <returns></returns>
        private async Task CreateExchanges()
        {
            Logger.Info("Creating exchanges...");
            // Create exchange data
            m_aData.Clear();

            foreach (var eType in Setup.ExchangeTypes)
            {
                if (eType == ExchangeType.CoinExFutures || eType == ExchangeType.BingxFutures)
                {
                    await AddSingleExchange(eType);

                }
            }
            Logger.Info("Created...");


        }

        public async Task Start()
        {
            await Stop();   
            if (Strategy == null) throw new Exception("Can no start without strategy");
            Logger.Info("Tester starting...");

            await CreateExchanges();    
            m_oMainTask = MainLoop();

        }

        public async Task Stop()
        {
            if (m_oMainTask == null) return;
            m_oCancelSource.Cancel();
            await m_oMainTask;
            m_oMainTask = null;
        }


        private async Task MainLoop()
        {

            Logger.Info("Tester Main loop started");
            while (!m_oCancelSource.IsCancellationRequested)
            {
                await Strategy!.Process(ExchangeData);
                await Task.Delay(200);
            }
            Logger.Info("Tester Main loop ended");
        }

    }
}
