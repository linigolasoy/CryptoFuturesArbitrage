using CoinEx.Net.Objects.Models.V2;
using CoinEx.Net.Enums;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexOrder : IFuturesOrder
    {
        public CoinexOrder( IFuturesSymbol oSymbol, bool bBuy, bool bLong, CoinExFuturesOrder oParsed, OrderUpdateType eType) 
        {
            Symbol = oSymbol;
            Id = oParsed.Id;
            PutStatus(oParsed.Status, eType, oParsed.Quantity, oParsed.QuantityFilled);

            PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            OrderDirection = (bBuy ? FuturesOrderDirection.Buy : FuturesOrderDirection.Sell);
            PutType(oParsed.OrderType);
            Quantity = oParsed.Quantity;    
            Price = (oParsed.Price == null ? 0: oParsed.Price.Value);   
        }


        private void PutType( OrderTypeV2 eType)
        {
            switch(eType)
            {
                case OrderTypeV2.Limit:
                    OrderType = FuturesOrderType.Limit;
                    break;
                case OrderTypeV2.Market:
                    OrderType = FuturesOrderType.Market;
                    break;
                default:
                    break;
            }
        }
        private void PutStatus( OrderStatusV2? eStatus, OrderUpdateType eUpdateType, decimal nQuantity, decimal? nFilled)
        {
            if( eStatus == null )
            {
                if (eUpdateType == OrderUpdateType.Finish)
                {
                    if ( nFilled != null)
                    {
                        if( nFilled.Value >= nQuantity )
                        {
                            OrderStatus = FuturesOrderStatus.Filled;
                        }
                        else
                        {
                            OrderStatus = (nFilled.Value <= 0 ? FuturesOrderStatus.Canceled: FuturesOrderStatus.PartialFilled);
                        }
                    }
                    else
                    {
                        OrderStatus = FuturesOrderStatus.Canceled;
                    }
                }
                return;
            }

            switch (eStatus.Value)
            {
                case OrderStatusV2.Filled:
                    OrderStatus = FuturesOrderStatus.Filled;
                    break;
                case OrderStatusV2.PartiallyFilled:
                    OrderStatus = FuturesOrderStatus.PartialFilled;
                    break;
                case OrderStatusV2.Canceled:
                case OrderStatusV2.PartiallyCanceled:
                    OrderStatus = FuturesOrderStatus.Canceled;
                    break;
                case OrderStatusV2.Open:
                    OrderStatus = FuturesOrderStatus.New;
                    break;
            }

        }

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Order; }
        public long Id { get; }

        public IFuturesSymbol Symbol { get; }

        public FuturesOrderDirection OrderDirection { get; }

        public FuturesPositionDirection PositionDirection { get; }

        public FuturesOrderType OrderType { get; private set; } = FuturesOrderType.Market;

        public FuturesOrderEvent OrderEvent { get; private set; } = FuturesOrderEvent.New;

        public FuturesOrderStatus OrderStatus { get; private set; } = FuturesOrderStatus.New;

        public DateTime TimeCreated { get; } = DateTime.Now;

        public DateTime TimeUpdated { get; private set; } = DateTime.Now;

        public decimal Quantity { get; }

        public decimal? Price { get; } = 0;

        public void Update(IFuturesOrder oOrder)
        {
            TimeUpdated = DateTime.Now; 
            OrderStatus = oOrder.OrderStatus;   
        }
    }
}
