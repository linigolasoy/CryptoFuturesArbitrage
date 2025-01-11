using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{


    internal class OppositeCloseResult : ICloseResult
    {
        public bool Success { get; internal set; } = false;

        public decimal ProfitOrLoss { get; internal set; } = 0;
    }

    internal class OppositeOrder : IOppositeOrder
    {
        private bool m_bLeverageSet = false;
        private const string USDT = "USDT";
        public OppositeOrder(IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort) 
        { 
            SymbolLong = oSymbolLong;
            SymbolShort = oSymbolShort;
        }

        public IFuturesSymbol SymbolLong { get; }

        public IFuturesSymbol SymbolShort { get; }

        public decimal Profit { get; set; } = 0;
        public decimal ProfitBalance { get; set; } = 0;
        public int Leverage { get; set; } = 1;

        public decimal Quantity { get; set; } = 0;

        public IFuturesOrder? OpenOrderLong { get; private set; } = null;

        public IFuturesOrder? OpenOrderShort { get; private set; } = null;

        public IFuturesOrder? CloseOrderLong { get; private set; } = null;

        public IFuturesOrder? CloseOrderShort { get; private set; } = null;

        public IFuturesPosition? PositionLong { get; private set; } = null;

        public IFuturesPosition? PositionShort { get; private set; } = null;

        public async Task<ICloseResult> TryCloseLimit()
        {
            throw new NotImplementedException();
        }

        public async Task<ICloseResult> TryCloseMarket()
        {
            OppositeCloseResult oResult = new OppositeCloseResult();
            if (PositionLong == null || PositionShort == null) return oResult;
            decimal nPnl = PositionLong.ProfitUnRealized + PositionShort.ProfitUnRealized;

            IFuturesBalance? oBalanceLong = SymbolLong.Exchange.Account.BalanceManager.GetData().FirstOrDefault(p => p.Currency == USDT);
            IFuturesBalance? oBalanceShort = SymbolShort.Exchange.Account.BalanceManager.GetData().FirstOrDefault(p => p.Currency == USDT);
            if( oBalanceLong != null && oBalanceShort != null )
            {
                this.ProfitBalance = oBalanceLong.ProfitUnrealized + oBalanceShort.ProfitUnrealized;
            }
            if ( nPnl > 0M )
            {

            }
            oResult.ProfitOrLoss = nPnl;
            this.Profit = nPnl;
            return oResult;
        }

        public async Task<bool> TryOpenLimit()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Set leverates
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SetLeverages()
        {
            if (m_bLeverageSet) return true;
            bool bResult = await SymbolLong.Exchange.Trading.SetLeverage(SymbolLong, Leverage);
            if (!bResult) return false;
            bResult = await SymbolShort.Exchange.Trading.SetLeverage(SymbolShort, Leverage);
            if (!bResult) return false;
            m_bLeverageSet = true;  
            return true;
        }
        public async Task<bool> TryOpenMarket()
        {
            if( Leverage < 1 || Quantity < 1 ) return false;
            // Set leverages
            bool bResult = await SetLeverages();
            List<Task<IFuturesOrder?>> aTasks = new List<Task<IFuturesOrder?>>();
            aTasks.Add(SymbolLong.Exchange.Trading.CreateMarketOrder(SymbolLong, true, Quantity));
            aTasks.Add(SymbolShort.Exchange.Trading.CreateMarketOrder(SymbolShort, false, Quantity));


            await Task.WhenAll(aTasks);
            if( aTasks.Any(p=> p.Result == null)) return false;
            // TODO: Rollback
            // Wait until we have orders and positions on websockets
            int nRetries = 10;
            while( nRetries >= 0 )
            {
                await Task.Delay(500);

                if( OpenOrderLong == null )
                {
                    IFuturesOrder[] aOrdersLong = SymbolLong.Exchange.Account.OrderManager.GetData();
                    OpenOrderLong = aOrdersLong.FirstOrDefault(p=> p.Symbol.Symbol == SymbolLong.Symbol && p.Quantity == Quantity && p.OrderDirection == FuturesOrderDirection.Buy);
                }
                else
                {
                    IFuturesPosition[] aPositions = SymbolLong.Exchange.Account.PositionManager.GetData();  
                    PositionLong = aPositions.FirstOrDefault(p=> p.Symbol.Symbol == SymbolLong.Symbol && p.Quantity == Quantity && p.Direction == FuturesPositionDirection.Long);
                }
                if ( OpenOrderShort == null ) 
                {
                    IFuturesOrder[] aOrdersShort = SymbolShort.Exchange.Account.OrderManager.GetData();
                    OpenOrderShort = aOrdersShort.FirstOrDefault(p => p.Symbol.Symbol == SymbolShort.Symbol && p.Quantity == Quantity && p.OrderDirection == FuturesOrderDirection.Sell);
                }
                else
                {
                    IFuturesPosition[] aPositions = SymbolShort.Exchange.Account.PositionManager.GetData();
                    PositionShort= aPositions.FirstOrDefault(p => p.Symbol.Symbol == SymbolShort.Symbol && p.Quantity == Quantity && p.Direction == FuturesPositionDirection.Short);

                }

                if (PositionLong != null && PositionShort != null) return true;

                nRetries++; 
            }

            return false;
        }

        /// <summary>
        /// Create from positions
        /// </summary>
        /// <param name="aExchanges"></param>
        /// <returns></returns>
        public static async Task<IOppositeOrder[]?> CreateFromExchanges(ICryptoFuturesExchange[] aExchanges)
        {
            await Task.Delay(2000);
            List<IOppositeOrder> aResult = new List<IOppositeOrder>();  
            for( int i = 0; i < aExchanges.Length; i++ )
            {
                ICryptoFuturesExchange oExchange1 = aExchanges[i];
                IFuturesPosition[] aPositions1 = oExchange1.Account.PositionManager.GetData();
                if (aPositions1.Length <= 0) continue;
                for( int j = i +1; j < aExchanges.Length; j++ )
                {
                    ICryptoFuturesExchange oExchange2 = aExchanges[j];
                    IFuturesPosition[] aPositions2 = oExchange2.Account.PositionManager.GetData();
                    if (aPositions2.Length <= 0) continue;

                    foreach( IFuturesPosition oPosition1 in aPositions1)
                    {
                        IFuturesPosition? oPosition2 = aPositions2
                            .FirstOrDefault(p => p.Symbol.Base == oPosition1.Symbol.Base &&
                                                p.Symbol.Quote == oPosition1.Symbol.Quote && 
                                                p.Quantity == oPosition1.Quantity &&
                                                p.Direction != oPosition1.Direction);
                        if( oPosition2 == null ) continue;  
                        if( oPosition1.Direction == FuturesPositionDirection.Long )
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition1.Symbol, oPosition2.Symbol);
                            oOrder.PositionLong = oPosition1;
                            oOrder.PositionShort = oPosition2;
                            aResult.Add(oOrder);    
                        }
                        else
                        {
                            OppositeOrder oOrder = new OppositeOrder(oPosition2.Symbol, oPosition2.Symbol);
                            oOrder.PositionLong = oPosition2;
                            oOrder.PositionShort = oPosition1;
                            aResult.Add(oOrder);

                        }
                    }
                }
            }

            return aResult.ToArray();
        }
    }
}
