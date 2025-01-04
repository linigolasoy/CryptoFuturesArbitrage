using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Tester
{
    internal class FundingRateChance
    {

        private FundingDataHistory m_oHistoryBuy;
        private FundingDataHistory m_oHistorySell;
        public FundingRateChance( IBotSymbolData oSymbolBuy, IBotSymbolData oSymbolSell) 
        { 
            m_oHistoryBuy = (FundingDataHistory)oSymbolBuy; 
            m_oHistorySell = (FundingDataHistory)oSymbolSell;   
        }


        
        public decimal ProfitOnDate( DateTime dDate )
        {
            return -10;
        }
    }
}
