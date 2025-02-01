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


    internal interface IArbitrageChance
    {
        public ChanceStatus ChanceStatus { get; set; }

        public IArbitragePosition BuyPosition { get; }
        public IArbitragePosition SellPosition { get; }

        public decimal Profit { get; }

        public decimal Quantity { get; }    
        public decimal BuyOpenPrice { get; set; }
        public decimal SellOpenPrice { get; set; }

        public decimal BuyClosePrice { get; }
        public decimal SellClosePrice { get; }

        public decimal Percent { get; }
        public bool CalculateArbitrage(decimal nMoney);
        public bool CalculateProfit();

        public void Reset();
    }
}
