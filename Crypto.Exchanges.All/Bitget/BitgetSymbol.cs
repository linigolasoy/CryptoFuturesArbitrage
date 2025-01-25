using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetSymbol : BaseFuturesSymbol, IFuturesSymbol
    {
        public BitgetSymbol( IFuturesExchange oExchange, BitgetContract oParsed) :
            base(oExchange)
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = (oParsed.MaxLeverage == null? 1: (int)(oParsed.MaxLeverage.Value));
            LeverageMin = 1;
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.TakerFeeRate;    
            Decimals = oParsed.QuantityDecimals;
            Minimum = oParsed.MinOrderQuantity;

        }

    }
}
