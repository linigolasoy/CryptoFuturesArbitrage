using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Tester
{
    internal class FundingTestDate : IFundingDate
    {

        private List<IFundingPair> m_aPairs = new List<IFundingPair> ();    
        public FundingTestDate( DateTime dDate ) 
        { 
            DateTime = dDate;   
        }
        public DateTime DateTime { get; }

        public IFundingPair[] Pairs { get => m_aPairs.ToArray(); }

        public IFundingPair? GetBest()
        {
            if (m_aPairs.Count <= 0) return null;
            return m_aPairs.OrderByDescending(p=> p.Percent).FirstOrDefault();  

        }


        internal void Put(IFuturesSymbol[] aSymbols, IFundingRate[] aRates )
        {
            IFundingPair[]? aCreated = FundingTestPair.Create(this, aSymbols, aRates);
            if (aCreated == null) return;
            m_aPairs.AddRange(aCreated);
        }
    }
}
