using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class MoneyTransfer : IMoneyTransfer
    {
        private const string USDT = "USDT";
        public MoneyTransfer(IFuturesExchange oFrom, IFuturesExchange oTo, decimal nQuantity) 
        { 
            From = oFrom;
            To = oTo;
            Quantity = nQuantity;
        }
        public IFuturesExchange From { get; }

        public IFuturesExchange To { get; }

        public decimal Quantity { get; }

        public MoneyTransferStatus Status { get; private set; } = MoneyTransferStatus.Initial;

        private decimal m_nFromBalance = 0;
        private decimal m_nToBalance = 0;


        /// <summary>
        /// Get initial balances
        /// </summary>
        /// <returns></returns>
        private async Task<ITradingResult<decimal>> GetInitialBalances()
        {
            try
            {
                IFuturesBalance[]? aFromBalances = await From.Account.GetBalances();
                if (aFromBalances == null) return new TradingResult<decimal>("No balance returned on from");
                IFuturesBalance[]? aToBalances = await To.Account.GetBalances();
                if (aToBalances == null) return new TradingResult<decimal>("No balance returned on to");

                IFuturesBalance? oFromUsdt = aFromBalances.FirstOrDefault(p=> p.Currency == USDT);
                if (oFromUsdt == null) return new TradingResult<decimal>("No USDT balance returned on from");
                IFuturesBalance? oToUsdt = aToBalances.FirstOrDefault(p => p.Currency == USDT);
                if (oToUsdt == null) return new TradingResult<decimal>("No USDT balance returned on to");

                m_nFromBalance = oFromUsdt.Equity;
                m_nToBalance += oToUsdt.Equity;
                Status = MoneyTransferStatus.FuturesToSpot;
                return new TradingResult<decimal>(0);
            }
            catch (Exception ex)
            {
                return new TradingResult<decimal>(ex);
            }
        }


        /// <summary>
        /// Step depending on status
        /// </summary>
        /// <returns></returns>
        public async Task<ITradingResult<decimal>> Step()
        {
            try
            {
                ITradingResult<decimal>? oResult = null;    
                switch(Status)
                {
                    case MoneyTransferStatus.Initial:
                        oResult = await GetInitialBalances();
                        break;
                    case MoneyTransferStatus.FuturesToSpot:
                        break;
                    case MoneyTransferStatus.Transfer:
                        break;
                    case MoneyTransferStatus.WaitForArrival:
                        break;
                    case MoneyTransferStatus.SpotToFutures:
                        break;
                    case MoneyTransferStatus.Done:
                        break;
                }
                if (oResult == null )
                {
                    return new TradingResult<decimal>("No results");
                }
                return oResult; 
            }
            catch (Exception ex)
            {
                return new TradingResult<decimal>(ex);
            }

        }
    }
}
