using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    internal class PriceUtil
    {

        public static decimal SpreadPercent( decimal nPrice1, decimal nPrice2 )
        {
            decimal nMax = Math.Max( nPrice1, nPrice2 );
            decimal nMin = Math.Min(nPrice1, nPrice2);

            if (nMin <= 0) return -99;
            decimal nPercent = Math.Round( 100.0M * (nMax - nMin) / nMin, 2);

            return nPercent;    
        }


        public static decimal CalculateProfit( bool bBuy, decimal nQuantity, decimal nPriceOpen, decimal nPriceClose )
        {
            decimal nProfit = (nPriceClose - nPriceOpen) * nQuantity;
            if (!bBuy) nProfit *= -1.0M;
            nProfit = Math.Round( nProfit,2 );
            return nProfit; 
        }
    }
}
