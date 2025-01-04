using Crypto.Interface.Futures;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Tester
{
    internal class FundingDataHistory:BaseSymbolData
    {
        public FundingDataHistory(IBotExchangeData oData, IFuturesSymbol oSymbol) : base(oData, oSymbol)
        { 
        }


        public IFundingRate[]? FundingHistory { get; private set; } = null;
        public async Task LoadHistory(DateTime dFrom)
        {
            FundingHistory = await this.ExchangeData.Exchange.GetFundingRatesHistory(this.Symbol, dFrom);
        }

        public async Task<IFuturesBar[]?> GetMinuteBars()
        {
            throw new NotImplementedException();
        }
    }
}
