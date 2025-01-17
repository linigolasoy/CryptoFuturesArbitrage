using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxSymbol : IFuturesSymbol
    {
        public BingxSymbol(IFuturesExchange oExchange, BingXContract oContract) 
        {
            Exchange = oExchange;   
            Symbol = oContract.Symbol;
            Base = oContract.Asset;
            Quote = oContract.Currency;

            LeverageMax = (int)( oContract.MaxLongLeverage < oContract.MaxShortLeverage? oContract.MaxLongLeverage: oContract.MaxShortLeverage);
            FeeMaker = oContract.MakerFeeRate;
            FeeTaker = oContract.TakerFeeRate;
            Decimals = oContract.QuantityPrecision;
            Minimum = oContract.MinOrderQuantity;
        }

        public IFuturesExchange Exchange { get; }
        public int LeverageMax { get; }

        public int LeverageMin { get => 1; }

        public decimal FeeMaker { get; }

        public decimal FeeTaker { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }
        public int Decimals { get; }
        public decimal Minimum { get; }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
