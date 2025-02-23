using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    internal class FundingDate: IFundingDate
    {
        private List<IFundingPair> m_aPairs = new List<IFundingPair>();
        public FundingDate(DateTime dDate)
        {
            DateTime = dDate;
        }
        public DateTime DateTime { get; }

        public IFundingPair[] Pairs { get => m_aPairs.ToArray(); }

        public IFundingPair? GetBest()
        {
            if (m_aPairs.Count <= 0) return null;
            /*
            IFundingPair? oBest = null;
            decimal nBestResult = -1000;
            foreach( var oPair in m_aPairs.OrderByDescending(p => p.Percent).Take(10) )
            {
                oPair.
            }
            */

            return m_aPairs.OrderByDescending(p => p.Percent).FirstOrDefault();

        }


        internal void Put(IFuturesSymbol[] aSymbols, IFundingRate[] aRates)
        {
            IFundingPair[]? aCreated = FundingPair.Create(this, aSymbols, aRates);
            if (aCreated == null) return;
            m_aPairs.AddRange(aCreated);
        }
    }
}
