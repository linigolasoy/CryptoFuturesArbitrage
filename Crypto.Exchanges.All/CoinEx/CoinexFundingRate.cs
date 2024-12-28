using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexFundingRate : IFundingRate
    {

        public CoinexFundingRate(IFuturesSymbol oSymbol, CoinExFundingRateHistory oData)
        {
            Symbol = oSymbol;
            Rate = oData.ActualFundingRate;
            DateTime = oData.FundingTime!.Value.ToLocalTime();
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }

        public int Cycle { get => 8; }
    }

    internal class CoinexFundingRateSnapshot : IFundingRateSnapShot
    {
        public CoinexFundingRateSnapshot(IFuturesSymbol oSymbol, CoinExFundingRate oData)
        {
            Symbol = oSymbol;
            NextSettle = oData.LastFundingTime!.Value.ToLocalTime();
            Rate = oData.LastFundingRate;
        }
        public decimal Maximum { get => 9999; }

        public decimal Minimum { get => -9999; }

        public DateTime NextSettle { get; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }

        public int Cycle { get => 8; }
    }
}
