using BitMart.Net.Objects.Models;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartOrder : IFuturesOrder
    {
        public BitmartOrder( IFuturesSymbol oSymbol, BitMartFuturesOrder oOrder) 
        { 
            Symbol = oSymbol;   
            Id = oOrder.OrderId;
            TimeCreated = oOrder.CreateTime.ToLocalTime();  
            TimeUpdated = (oOrder.UpdateTime == null ? TimeCreated : oOrder.UpdateTime.Value.ToLocalTime() );    
            PutDirections(oOrder.Side);
            switch(oOrder.OrderType)
            {
                case BitMart.Net.Enums.FuturesOrderType.Market:
                    this.OrderType = FuturesOrderType.Market;
                    break;
                case BitMart.Net.Enums.FuturesOrderType.Limit:
                    this.OrderType = FuturesOrderType.Limit;
                    break;
            }
            Quantity = CalculateQuantity(oOrder.Quantity);
            PutStatus(oOrder.Status, ( oOrder.Quantity <= oOrder.QuantityFilled) );
            Price = oOrder.Price;

        }

        public BitmartOrder( IFuturesSymbol oSymbol, long nId )
        {
            Symbol = oSymbol;
            TimeCreated = DateTime.Now;
            TimeUpdated = TimeCreated;
            Id = nId.ToString();
        }

        public BitmartOrder( IFuturesSymbol oSymbol, BitMart.Net.Enums.OrderEvent eEvent, BitMartFuturesOrderUpdate oUpdate)
        {
            Symbol = oSymbol;
            Id = oUpdate.OrderId;
            TimeCreated = oUpdate.CreateTime.ToLocalTime();
            TimeUpdated = (oUpdate.UpdateTime == null ? TimeCreated : oUpdate.UpdateTime.Value.ToLocalTime());
            PutDirections(oUpdate.Side);
            switch (oUpdate.OrderType)
            {
                case BitMart.Net.Enums.FuturesOrderType.Market:
                    this.OrderType = FuturesOrderType.Market;
                    break;
                case BitMart.Net.Enums.FuturesOrderType.Limit:
                    this.OrderType = FuturesOrderType.Limit;
                    break;
            }
            Quantity = CalculateQuantity( oUpdate.Quantity );
            PutStatus(oUpdate.Status, (oUpdate.Quantity <= oUpdate.QuantityFilled));
            Price = oUpdate.Price;
        }

        private decimal CalculateQuantity( decimal nQuantity )
        {
            decimal nContractSize = ((BitmartSymbol)Symbol).ContractSize;
            return nQuantity * nContractSize;
        }
        private void PutDirections(BitMart.Net.Enums.FuturesSide eSide)
        {
            switch (eSide)
            {
                case BitMart.Net.Enums.FuturesSide.BuyOpenLong:
                    OrderDirection = FuturesOrderDirection.Buy;
                    PositionDirection = FuturesPositionDirection.Long;
                    break;
                case BitMart.Net.Enums.FuturesSide.SellCloseLong:
                    OrderDirection = FuturesOrderDirection.Sell;
                    PositionDirection = FuturesPositionDirection.Long;
                    break;
                case BitMart.Net.Enums.FuturesSide.SellOpenShort:
                    OrderDirection = FuturesOrderDirection.Sell;
                    PositionDirection = FuturesPositionDirection.Short;
                    break;
                case BitMart.Net.Enums.FuturesSide.BuyCloseShort:
                    OrderDirection = FuturesOrderDirection.Buy;
                    PositionDirection = FuturesPositionDirection.Short;
                    break;
            }

        }

        private void PutStatus(BitMart.Net.Enums.FuturesOrderStatus eStatus, bool bFilled )
        {

            switch(eStatus)
            {
                case BitMart.Net.Enums.FuturesOrderStatus.Approval:
                case BitMart.Net.Enums.FuturesOrderStatus.Check:
                    this.OrderStatus = FuturesOrderStatus.New;
                    break;
                case BitMart.Net.Enums.FuturesOrderStatus.Finish:
                    this.OrderStatus = (bFilled? FuturesOrderStatus.Filled : FuturesOrderStatus.Canceled);  
                    break;
            }
        }
        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public FuturesOrderDirection OrderDirection { get; internal set; }

        public FuturesPositionDirection PositionDirection { get; internal set; }

        public FuturesOrderType OrderType { get; internal set; }

        public FuturesOrderEvent OrderEvent { get => FuturesOrderEvent.New; }

        public FuturesOrderStatus OrderStatus { get; internal set; } = FuturesOrderStatus.New;

        public DateTime TimeCreated { get; }

        public DateTime TimeUpdated { get; private set; }

        public decimal Quantity { get; internal set; }

        public decimal? Price { get; internal set; } = null;

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Order; }

        public void Update(IFuturesOrder oOrder)
        {
            TimeUpdated = oOrder.TimeUpdated;
            OrderStatus = oOrder.OrderStatus;
        }
    }
}
