using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class BaseLocalStorage : ILocalStorage
    {

        public BaseLocalStorage(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }

        public IFuturesExchange Exchange { get; }


        /// <summary>
        /// Creates local folders
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <param name="dDate"></param>
        /// <returns></returns>
        private string? CreateLocalFolders(string strSymbol, DateTime dDate)
        {
            string strBasePath = $"{Exchange.Setup.HistoryPath}/{Exchange.ExchangeType.ToString()}";
            try
            {
                if (!Directory.Exists(strBasePath))
                {
                    Directory.CreateDirectory(strBasePath);
                }
                strBasePath = $"{strBasePath}/{strSymbol}";
                if (!Directory.Exists(strBasePath))
                {
                    Directory.CreateDirectory(strBasePath);
                }
                string strDate = dDate.Date.ToString("yyyyMM");
                strBasePath = $"{strBasePath}/{strDate}";
                if (!Directory.Exists(strBasePath))
                {
                    Directory.CreateDirectory(strBasePath);
                }

            }
            catch (Exception e)
            {
                return null;
            }
            return strBasePath;

        }

        private string? GetLocalPath(string strSymbol, DateTime dDate, Timeframe eFrame )
        {
            string? strPath = CreateLocalFolders(strSymbol, dDate);
            if (strPath == null) return null;
            string strFileName = $"Bar_{strSymbol}_{eFrame.ToString()}_{dDate.Date.ToString("yyyyMMdd")}.json";
            strPath = $"{strPath}/{strFileName}";
            return strPath;
        }
        public IFuturesBar[]? GetBars(IFuturesSymbol oSymbol, Timeframe eFrame, DateTime dDate)
        {
            string? strPath = GetLocalPath(oSymbol.Symbol, dDate, eFrame);
            if (!File.Exists(strPath)) return null;

            List<BarJson>? aBarsJson = JsonConvert.DeserializeObject<List<BarJson>>(File.ReadAllText(strPath));
            if (aBarsJson == null || aBarsJson.Count <= 0 ) return null;
            List<IFuturesBar> aResult = new List<IFuturesBar> ();   
            foreach( var oJson in aBarsJson )
            {
                aResult.Add( new BarStorage(oSymbol, eFrame, oJson) );  
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Bars to file
        /// </summary>
        /// <param name="aBars"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public void SetBars(IFuturesBar[] aBars)
        {
            if (aBars.Length < 1) return;
            IFuturesSymbol oFirst = aBars[0].Symbol;
            if (aBars.Any(p => p.Symbol.Symbol != oFirst.Symbol)) throw new Exception("Not saving more than one symbol");
            DateTime[] aDates = aBars.Select(p => p.DateTime.Date).Distinct().ToArray();
            foreach (DateTime dDate in aDates)
            {
                IFuturesBar[] aBarsDate = aBars.Where(p => p.DateTime.Date == dDate).ToArray();
                Timeframe[] aFrames = aBarsDate.Select(p => p.Timeframe).Distinct().ToArray();
                foreach (Timeframe eFrame in aFrames)
                {
                    IFuturesBar[] aBarsFrame = aBarsDate.Where(p => p.Timeframe == eFrame).OrderBy(p => p.DateTime).ToArray();
                    SaveBars(oFirst, dDate, eFrame, aBarsFrame);
                }
            }

        }

        /// <summary>
        /// Single bars file
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="dDate"></param>
        /// <param name="eFrame"></param>
        /// <param name="aBars"></param>
        private void SaveBars(IFuturesSymbol oSymbol, DateTime dDate, Timeframe eFrame, IFuturesBar[] aBars)
        {
            string? strPath = GetLocalPath(oSymbol.Symbol, dDate, eFrame);
            if (strPath == null) return;
            List<BarJson> aToSave = new List<BarJson>();

            foreach(var oBar in aBars)
            {
                BarJson oNew = new BarJson()
                {
                    Symbol = oSymbol.Symbol,
                    DateTime = oBar.DateTime,
                    Open = oBar.Open,
                    Close = oBar.Close,
                    High = oBar.High,
                    Low = oBar.Low,
                    Volume = oBar.Volume
                };
                aToSave.Add(oNew);
            }
            string strToSave = JsonConvert.SerializeObject(aToSave, Formatting.Indented);
            File.WriteAllText(strPath, strToSave);  
        }

        public IFundingRate[]? GetFundingRates(string strSymbol, DateTime dDate)
        {
            throw new NotImplementedException();
        }
    }
}
