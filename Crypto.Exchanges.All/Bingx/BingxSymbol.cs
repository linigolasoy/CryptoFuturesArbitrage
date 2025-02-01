using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxSymbol : BaseFuturesSymbol, IFuturesSymbol
    {
        public BingxSymbol(IFuturesExchange oExchange, BingXContract oContract) :
            base(oExchange)
        {
            Symbol = oContract.Symbol;
            Base = oContract.Asset;
            Quote = oContract.Currency;

            LeverageMax = (int)( oContract.MaxLongLeverage < oContract.MaxShortLeverage? oContract.MaxLongLeverage: oContract.MaxShortLeverage);
            FeeMaker = oContract.MakerFeeRate;
            FeeTaker = oContract.TakerFeeRate;
            Decimals = oContract.QuantityPrecision;
            Minimum = oContract.MinOrderQuantity;
            ContractSize = oContract.ContractSize;
            QuantityDecimals = oContract.QuantityPrecision;
        }

    }
}
