using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class BaseFundingFeeder : IFundingRateFeeder
    {
        private ILocalStorage m_oLocalStorage;

        public BaseFundingFeeder( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;
            m_oLocalStorage = new BaseLocalStorage( Exchange ); 
        }
        public IFuturesExchange Exchange { get; }

        public event IFundingRateFeeder.GetFundingDelegate? OnGetFunding;

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            return await GetFundingRatesHistory( new IFuturesSymbol[] { oSymbol }, dFrom);  
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            DateTime dStart = new DateTime(dFrom.Year, dFrom.Month, 1, 0,0,0, DateTimeKind.Local);
            List<IFundingRate> aResults = new List<IFundingRate>();

            foreach( var oSymbol in aSymbols ) 
            {
                IFundingRate[]? aRates = m_oLocalStorage.GetFundingRates(oSymbol.Symbol);
                bool bSaveRates = false;    
                if( aRates == null || aRates.Length <= 0 )
                {
                    if( OnGetFunding != null )
                    {
                        aRates = await OnGetFunding(oSymbol, dFrom);
                        bSaveRates = true;
                    }
                }
                else 
                {
                    DateTime dMax = aRates.Select(p => p.SettleDate).Max().Date;
                    if( dMax < DateTime.Today.Date )
                    {
                        if (OnGetFunding != null)
                        {
                            aRates = await OnGetFunding(oSymbol, dFrom);
                            bSaveRates = true;
                        }
                    }
                }

                if( aRates != null  )
                {
                    if (bSaveRates)
                    {
                        m_oLocalStorage.SetFundingRates(oSymbol.Symbol, aRates);
                    }
                    aResults.AddRange(aRates);  
                }
            }
            return aResults.ToArray();
        }
    }
}
