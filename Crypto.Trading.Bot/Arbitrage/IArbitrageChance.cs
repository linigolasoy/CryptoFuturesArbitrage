using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{

    internal enum ChanceStatus
    {
        None,
        Leverage,
        Active,
        OrderOpen,
        Position,
        OrderClose,
        Closed,
        Reverted
    }

    internal interface IArbitragePosition
    {
        public IArbitrageChance Chance { get; } 
        public IOrderbook Orderbook { get; }    
        public IFuturesSymbol Symbol { get; }   

        public IFuturesPosition? Position { get; set; }
    }

    internal interface IArbitrageMoney
    {
        public IArbitrageChance Chance { get; }
        public decimal Profit { get; set; }

        public decimal Quantity { get; }
        public decimal BuyOpenPrice { get; set; }
        public decimal SellOpenPrice { get; set; }

        public decimal BuyClosePrice { get; set; }
        public decimal SellClosePrice { get; set; }

        public decimal Percent { get; }
    }


    internal interface IArbitrageChance
    {
        public ChanceStatus ChanceStatus { get; set; }

        public string Currency { get; }

        public IOrderbook[] Orderbooks { get; }

        public IArbitragePosition? BuyPosition { get; }
        public IArbitragePosition? SellPosition { get; }

        public IArbitrageMoney? Money { get; }  
        public bool CalculateArbitrage(decimal nMoney);
        public bool CalculateProfit();
        public bool SetPositions( IFuturesPosition oLongPosition, IFuturesPosition oShortPosition );    
        public void Reset();
    }
}
