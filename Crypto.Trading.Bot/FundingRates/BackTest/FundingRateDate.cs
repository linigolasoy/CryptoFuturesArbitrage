using Crypto.Interface;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{
    internal class FundingRateDate
    {

        public FundingRateDate(ICryptoSetup oSetup, DateTime dDate, Dictionary<string, List<IFundingRate>> aRates, CurrencySymbols[] aCurrencySymbols ) 
        {
            Setup = oSetup;
            DateTime = dDate;
            List<FundingRateData> aData = new List<FundingRateData>();  
            foreach( var oData in aRates )
            {

                CurrencySymbols? oCurSymbol = aCurrencySymbols.FirstOrDefault(p => p.Currency == oData.Value[0].Symbol.Base);
                if (oCurSymbol == null) continue;
                aData.Add(new FundingRateData(this, oData.Key, oData.Value.ToArray(), oCurSymbol.Symbols));
            }
            Data = aData.ToArray();

        }

        public ICryptoSetup Setup { get; }
        public DateTime DateTime { get; }

        public FundingRateData[] Data { get; }  

        public FundingRateChance[] ToChances(decimal nMoney)
        {
            List<FundingRateChance> aResult = new List<FundingRateChance>();    
            foreach( var oData in Data )
            {
                FundingRateChance? oChance = oData.ToChance(nMoney);
                if (oChance != null) aResult.Add(oChance);
            }
            return aResult.ToArray();   
        }
    }
}
