using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexSymbol : BaseFuturesSymbol, IFuturesSymbol
    {

        public CoinexSymbol(IFuturesExchange oExchange, CoinExFuturesSymbol oParsed) :
            base(oExchange)
        {
            Symbol = oParsed.Symbol;
            Base = oParsed.BaseAsset;
            Quote = oParsed.QuoteAsset;
            LeverageMax = oParsed.Leverage.Max();
            LeverageMin = oParsed.Leverage.Min();   
            FeeMaker = oParsed.MakerFeeRate;
            FeeTaker = oParsed.MakerFeeRate;    
            Decimals = oParsed.QuantityPrecision;
            Minimum = oParsed.MinOrderQuantity;
        }
    }
}
