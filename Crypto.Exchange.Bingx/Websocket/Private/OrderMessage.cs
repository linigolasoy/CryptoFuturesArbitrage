using Crypto.Interface.Futures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket.Private
{

    internal class OrderParsed
    {
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("i")]
        public long OrderId { get; set; } = 0; // i Order ID:1627970445070303232
        [JsonProperty("S")]
        public string Direction { get; set; } = string.Empty; // S order direction:SELL
        [JsonProperty("o")]
        public string OrderType { get; set; } = string.Empty; // o order type:MARKET
        [JsonProperty("ps")]
        public string PositionDirection { get; set; } = string.Empty; // ps Position direction: LONG or SHORT or BOTH
        [JsonProperty("q")]
        public string OrderQuantity { get; set; } = string.Empty; // q order quantity:5.00000000
        [JsonProperty("p")]
        public string? OrderPrice { get; set; } = null; // p order price:7.82700000
        /*
        
        
        
        
        c client custom order ID
        sp trigger price:7.82700000
        ap order average price:7.82690000
        x The specific execution type of this event:TRADE
        X current status of the order:FILLED
        N Fee asset type:USDT
        n handling fee:-0.01369708
        T transaction time:1676973375149
        wt trigger price type: MARK_PRICE mark price, CONTRACT_PRICE latest price, INDEX_PRICE index price
        rp The transaction achieves profit and loss: 0.00000000
        z Order Filled Accumulated Quantity: 0.00000000
        sg true: Enables the guaranteed stop-loss and take-profit feature; false: Disables the feature.
        ti Conditional Order ID associated with this order:1771124709866754048
        ro reduceOnly
        td Trade ID
        tv Trade Value         
        */
    }

    internal class OrderParentParsed
    {
        [JsonProperty("e")]
        public string EventType { get; set; } = string.Empty;

        [JsonProperty("E")]
        public long EventTime { get; set; } = 0;
        [JsonProperty("T")]
        public long OrderTime { get; set; } = 0;

        [JsonProperty("o")]
        public OrderParsed? Order { get; set; } = null;

    }

    internal class OrderMessage : IWebsocketMessage, IFuturesOrder
    {

        public OrderMessage(OrderParentParsed oParsed, IFuturesSymbol oSymbol )
        {
            Symbol = oSymbol;
            Id = oParsed.Order!.OrderId;
            FuturesOrderDirection eDirection = FuturesOrderDirection.Buy;
            if( oParsed.Order!.Direction == FuturesOrderDirection.Sell.ToString().ToUpper() ) eDirection = FuturesOrderDirection.Sell;  
            OrderDirection = eDirection;

            FuturesPositionDirection ePosDirection = FuturesPositionDirection.Long;
            if( oParsed.Order!.PositionDirection == FuturesPositionDirection.Short.ToString().ToUpper() ) ePosDirection = FuturesPositionDirection.Short;
            PositionDirection = ePosDirection;

            FuturesOrderType eType = FuturesOrderType.Market;
            if( oParsed.Order!.OrderType == FuturesOrderType.Limit.ToString().ToUpper() ) eType = FuturesOrderType.Limit;
            OrderType = eType;

            TimeCreated = BingxCommon.ParseUnixTimestamp(oParsed.OrderTime);
            TimeUpdated = BingxCommon.ParseUnixTimestamp(oParsed.EventTime);
            Quantity = decimal.Parse(oParsed.Order!.OrderQuantity, CultureInfo.InvariantCulture);
            if( oParsed.Order.OrderPrice != null )
            {
                Price = decimal.Parse(oParsed.Order!.OrderPrice, CultureInfo.InvariantCulture);
            }

        }

        public void Update( IFuturesOrder oSource )
        {
            return;
        }
        public WebsocketMessageType MessageType { get => WebsocketMessageType.PrivateOrder; }

        public IFuturesSymbol Symbol { get; }

        public long Id { get; }

        public FuturesOrderDirection OrderDirection { get; }

        public FuturesPositionDirection PositionDirection { get; }

        public FuturesOrderType OrderType { get; }

        public DateTime TimeCreated { get; }

        public DateTime TimeUpdated { get; }

        public decimal Quantity { get; }

        public decimal? Price { get; }

        public static OrderMessage? Create( JObject oObject, IFuturesSymbol[] aSymbols )
        {
            OrderParentParsed? oParent = oObject.ToObject<OrderParentParsed>();
            if (oParent == null) return null;
            if( oParent.Order == null ) return null;
            IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oParent.Order.Symbol );
            if (oFound == null) return null;
            return new OrderMessage(oParent, oFound);
        }
    }
}
