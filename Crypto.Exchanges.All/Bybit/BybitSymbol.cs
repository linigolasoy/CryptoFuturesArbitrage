using Bybit.Net.Objects.Models.V5;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bybit
{
    internal class BybitSymbol : BaseFuturesSymbol, IFuturesSymbol
    {

        private static List<string> aBases = new List<string>();
        public BybitSymbol(IFuturesExchange oExchange, BybitLinearInverseSymbol oParsed) :
            base(oExchange)
        { 
            Symbol = oParsed.Name;
            string strBase = oParsed.BaseAsset;

            if( strBase.StartsWith("10"))
            {
                aBases.Add(strBase);    
            }
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;

            LeverageMin = 1;
            LeverageMax = (oParsed.LeverageFilter == null ? 1 : (int)(oParsed.LeverageFilter.MaxLeverage));
            FeeMaker = 0.0002M;
            FeeTaker = 0.00055M;

        }

    }
}
