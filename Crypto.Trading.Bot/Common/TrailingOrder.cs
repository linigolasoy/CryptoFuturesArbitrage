using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Common
{
    internal class TrailingOrder : ITrailingOrder
    {

        public TrailingOrder( IFuturesPosition oPosition, decimal nDistance) 
        { 
            Position = oPosition;
            Distance = nDistance;
            Exchange = oPosition.Symbol.Exchange;
        }


        public IFuturesExchange Exchange { get; }

        public IFuturesPosition Position { get; }

        public decimal Distance { get; }

        public decimal? PriceTakeProfit { get; private set; } = null;
        public decimal? PriceStopLoss { get; private set; } = null;

        private IOrderbook? m_oOrderbook = null;

        /// <summary>
        /// Start trailing order
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ITradingResult<bool>> Start()
        {
            if( Exchange.Market.Websocket == null ) return new TradingResult<bool>("No websocket");
            IOrderbook? oOrderbook = Exchange.Market.Websocket.OrderbookManager.GetData(Position.Symbol.Symbol);
            if( oOrderbook == null ) return new TradingResult<bool>("No orderbook");
            m_oOrderbook = oOrderbook;  
            decimal nDistance = Distance;
            if( Position.Direction == FuturesPositionDirection.Short ) nDistance = -nDistance;
            decimal nTp = Position.AveragePrice + nDistance;
            decimal nSl = Position.AveragePrice - nDistance;
            var oResult = await Exchange.Trading.SetStopLossTakeProfit(Position, nSl, nTp);
            if (oResult == null) return new TradingResult<bool>("Could not set take profit or stop loss");
            if( !oResult.Success) return oResult;

            PriceTakeProfit = nTp;
            PriceStopLoss = nSl;    
            return oResult;

        }

        public async Task<ITradingResult<bool>> Trail()
        {
            if( m_oOrderbook == null ) return new TradingResult<bool>("No orderbook");
            IOrderbookPrice? oPrice = null;
            if( Position.Direction == FuturesPositionDirection.Long )
            {
                if( m_oOrderbook.Bids.Length <= 0 ) return new TradingResult<bool>("No bids");
                oPrice = m_oOrderbook.Bids[0];
            }
            else
            {
                if (m_oOrderbook.Asks.Length <= 0) return new TradingResult<bool>("No asks");
                oPrice = m_oOrderbook.Asks[0];

            }
            if( oPrice == null ) return new TradingResult<bool>("No orderbook price");
            if( PriceStopLoss == null || PriceTakeProfit == null ) return new TradingResult<bool>("Sl & TP should not be null");


            ITradingResult<bool> oDefault = new TradingResult<bool>(true);

            decimal? nNewSl = null;
            if( Position.Direction == FuturesPositionDirection.Long )
            {
                decimal nDistance = oPrice.Price - PriceStopLoss.Value;
                if( nDistance > Distance )
                {
                    nNewSl = oPrice.Price - Distance;   
                }
            }
            else
            {
                decimal nDistance = PriceStopLoss.Value - oPrice.Price;
                if (nDistance > Distance)
                {
                    nNewSl = oPrice.Price + Distance;
                }

            }

            if (nNewSl == null) return oDefault;
            decimal nNewTp = 0;
            var oSlResult = await Exchange.Trading.SetStopLossTakeProfit(Position, nNewSl.Value, nNewTp);
            if (oSlResult == null) return new TradingResult<bool>("SL set result null");
            PriceStopLoss = nNewSl.Value;   
            return oSlResult;
        }
    }
}
