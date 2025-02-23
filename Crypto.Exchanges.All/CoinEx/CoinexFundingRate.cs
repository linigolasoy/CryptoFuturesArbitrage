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
    internal class CoinexFundingRate : BaseFundingRate, IFundingRate
    {

        public CoinexFundingRate(IFuturesSymbol oSymbol, CoinExFuturesTickerUpdate oData) :
             base(oSymbol, oData.NextFundingRate, oData.LastFundingTime!.Value.ToLocalTime())
        { 
        }
        public CoinexFundingRate(IFuturesSymbol oSymbol, CoinExFundingRateHistory oData):
             base(oSymbol, oData.ActualFundingRate, oData.FundingTime!.Value.ToLocalTime())
        {
        }
    }

    internal class CoinexFundingRateSnapshot : IFundingRateSnapShot
    {
        public CoinexFundingRateSnapshot(IFuturesSymbol oSymbol, CoinExFundingRate oData)
        {
            Symbol = oSymbol;
            SettleDate = oData.LastFundingTime!.Value.ToLocalTime();
            Rate = oData.LastFundingRate;
            SnapshotDate = DateTime.Now;
        }
        public decimal Maximum { get => 9999; }

        public decimal Minimum { get => -9999; }

        public DateTime SettleDate { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; private set; }

        public DateTime SnapshotDate { get; }

        public int Cycle { get => 8; }

        public void Update(IFundingRate obj)
        {
            Rate = obj.Rate;
            SettleDate = obj.SettleDate;
        }
    }
}
