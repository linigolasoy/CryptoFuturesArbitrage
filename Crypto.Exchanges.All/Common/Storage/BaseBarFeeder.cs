using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class BaseBarFeeder : IFuturesBarFeeder
    {
        private ILocalStorage m_oLocalStorage;
        public BaseBarFeeder(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
            m_oLocalStorage = new BaseLocalStorage(oExchange);
        }

        public IFuturesExchange Exchange { get; }

        public event IFuturesBarFeeder.GetBarsDayDelegate? OnGetBarsDay;

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await GetBars(new IFuturesSymbol[] { oSymbol }, eTimeframe, dFrom, dTo);
        }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            foreach (var oSymbol in aSymbols)
            {
                DateTime dStart = dFrom.Date;
                DateTime dEnd = dTo.Date;
                while (dStart <= dEnd)
                {
                    IFuturesBar[]? aBarsDay = m_oLocalStorage.GetBars(oSymbol, eTimeframe, dStart);
                    if (aBarsDay == null)
                    {
                        // Load
                        if (OnGetBarsDay != null)
                        {
                            aBarsDay = await OnGetBarsDay(oSymbol, eTimeframe, dStart);
                            if (aBarsDay != null)
                            {
                                if (dStart.Date < DateTime.Today.Date)
                                {
                                    m_oLocalStorage.SetBars(aBarsDay);
                                }
                            }
                        }
                    }
                    if (aBarsDay != null)
                    {
                        aResult.AddRange(aBarsDay);
                    }
                    dStart = dStart.AddDays(1);
                }
            }
            return aResult.ToArray();
        }
    }
}
