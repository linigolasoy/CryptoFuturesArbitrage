using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxSymbol : IFuturesSymbol
    {
        public BingxSymbol(BingXContract oContract) 
        {
            Symbol = oContract.Symbol;
            Base = oContract.Asset;
            Quote = oContract.Currency;

            LeverageMax = (int)( oContract.MaxLongLeverage < oContract.MaxShortLeverage? oContract.MaxLongLeverage: oContract.MaxShortLeverage);
            FeeMaker = oContract.MakerFeeRate;
            FeeTaker = oContract.TakerFeeRate;
        }
        public int LeverageMax { get; }

        public int LeverageMin { get => 1; }

        public decimal FeeMaker { get; }

        public decimal FeeTaker { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
