using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartSymbol: BaseFuturesSymbol, IFuturesSymbol
    {
        public BitmartSymbol(IFuturesExchange oExchange, BitMartContract oContract ) : base(oExchange) 
        { 
            Symbol = oContract.Symbol;
            Base = oContract.BaseAsset;
            Quote = oContract.QuoteAsset;
            LeverageMax = (int)oContract.MaxLeverage;
            LeverageMin = (int)oContract.MinLeverage;
            FeeMaker = 0.0002M;
            FeeTaker = 0.0006M;
            Decimals = (int)Math.Log10( (double)( 1M / oContract.PricePrecision ) );
            Minimum = oContract.MinQuantity * oContract.ContractQuantity;
            ContractSize = oContract.ContractQuantity;
            UseContractSize = true;
        }

    }
}
