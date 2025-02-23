using Crypto.Interface.Futures.Market;
using Crypto.Trading.Bot.BackTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{

    internal class DummyFundingRate : IFundingRate
    {
        public DummyFundingRate( IFuturesSymbol oSymbol, DateTime dDate ) 
        { 
            Symbol = oSymbol;
            SettleDate = dDate; 
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get => 0; }

        public DateTime SettleDate { get; }

        public int Cycle => throw new NotImplementedException();

        public void Update(IFundingRate obj)
        {
            throw new NotImplementedException();
        }
    }
    internal class FundingRateData
    {
        public FundingRateData(FundingRateDate oFundingDate, string strCurrency, IFundingRate[] aRates, IFuturesSymbol[] aCurrencySymbols) 
        { 

            if( aCurrencySymbols.Length > aRates.Length )
            {
                List<IFundingRate> aNewRates = new List<IFundingRate>();
                aNewRates.AddRange( aRates );   
                foreach( var oSymbol in aCurrencySymbols.Where(p=> !aRates.Any(q=> p.Exchange.ExchangeType == q.Symbol.Exchange.ExchangeType)))
                {
                    aNewRates.Add(new DummyFundingRate(oSymbol, oFundingDate.DateTime));
                }
                FundingRates = aNewRates.ToArray();
            }
            else
            {
                FundingRates = aRates;
            }
            FundingDate = oFundingDate;
            Currency = strCurrency;
            EquivalentSymbols = aCurrencySymbols;
        }
        public FundingRateDate FundingDate { get; }
        public string Currency { get; }

        public IFundingRate[] FundingRates { get; }
        public IFuturesSymbol[] EquivalentSymbols { get; }  

        public FundingRateChance? ToChance(decimal nMoney)
        {
            FundingRateChance oChance = new FundingRateChance(this, nMoney);
            if (oChance.ProfitPercent < FundingDate.Setup.ThresHold) return null;

            if( oChance.PositionBuy!.FundingRate == null || oChance.PositionSell!.FundingRate == null)
            {
                Console.WriteLine("AAA");
            }
            return oChance;
        }
    }
}
