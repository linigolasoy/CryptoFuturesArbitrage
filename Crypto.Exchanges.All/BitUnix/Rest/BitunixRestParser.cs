using Binance.Net.Objects.Models.Spot.Mining;
using Bitfinex.Net.Enums;
using Crypto.Exchanges.All.IpfCryptoClients.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using Kraken.Net.Enums;
using Nethereum.ABI.CompilationMetadata;
using Nethereum.Model;
using Nethereum.Signer;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XT.Net.Objects.Models;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Exchanges.All.BitUnix.Rest
{
    internal class BitunixRestParser : ICryptoRestParser
    {

        public const string URL_BASE           = "https://fapi.bitunix.com";

        public const string ENDPOINT_SYMBOLS   = "/api/v1/futures/market/trading_pairs";

        private static Dictionary<int, string>? m_aErrors = null;

        public BitunixRestParser(IFuturesExchange oExchange )
        {
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }


        private void CreateErrorDict()
        {
            if (m_aErrors != null) return;
            m_aErrors = new Dictionary<int, string>();
            m_aErrors.Add(0      , "Success");
            m_aErrors.Add(10001, "Network Error");
            m_aErrors.Add(10002, "Parameter Error");
            m_aErrors.Add(10003, "api - key can't be empty");
            m_aErrors.Add(10004, "The current ip is not in the apikey ip whitelist   ");
            m_aErrors.Add(10005, "Too many requests, please try again later  ");
            m_aErrors.Add(10006, "Request too frequently ");
            m_aErrors.Add(10007, "Sign signature error   ");
            m_aErrors.Add(10008, "{ value} does not comply with the rule, optional[correctValue] ");
            m_aErrors.Add(20001, "Market not exists  ");
            m_aErrors.Add(20002, "The current positions amount has exceeded the maximum open limit, please adjust the risk limit ");
            m_aErrors.Add(20003, "Insufficient balance   ");
            m_aErrors.Add(20004, "Insufficient Trader");
            m_aErrors.Add(20005, "Invalid leverage   ");
            m_aErrors.Add(20006, "You can't change leverage or margin mode as there are open orders");
            m_aErrors.Add(20007, "Order not found, please try it later   ");
            m_aErrors.Add(20008, "Insufficient amount");
            m_aErrors.Add(20009, "Position exists, so positions mode cannot be updated   ");
            m_aErrors.Add(20010, "Activation failed, the available balance in the futures account does not meet the conditions for activation of the coupon  ");
            m_aErrors.Add(20011, "Account not allowed to trade   ");
            m_aErrors.Add(20012, "This futures does not allow trading");
            m_aErrors.Add(20013, "Function disabled due tp pending account deletion request  ");
            m_aErrors.Add(20014, "Account deleted");
            m_aErrors.Add(20015, "This futures is not supported  ");
            m_aErrors.Add(30001, "Failed to order.Please adjust the order price or the leverage as the order price dealt may immediately liquidate. ");
            m_aErrors.Add(30002, "Price below liquidated price   ");
            m_aErrors.Add(30003, "Price above liquidated price   ");
            m_aErrors.Add(30004, "Position not exist ");
            m_aErrors.Add(30005, "The trigger price is closer to the current price and may be triggered immediately  ");
            m_aErrors.Add(30006, "Please select TP or SL ");
            m_aErrors.Add(30007, "TP trigger price is greater than average entry price   ");
            m_aErrors.Add(30008, "TP trigger price is less than average entry price  ");
            m_aErrors.Add(30009, "SL trigger price is less than average entry price  ");
            m_aErrors.Add(30010, "SL trigger price is greater than average entry price   ");
            m_aErrors.Add(30011, "Abnormal order status  ");
            m_aErrors.Add(30012, "Already added to favorite  ");
            m_aErrors.Add(30013, "Exceeded the maximum order quantity");
            m_aErrors.Add(30014, "Max Buy Order Price");
            m_aErrors.Add(30015, "Mini Sell Order Price  ");
            m_aErrors.Add(30016, "The qty should be larger than  ");
            m_aErrors.Add(30017, "The qty cannot be less than the minimum qty");
            m_aErrors.Add(30018, "Order failed.No position opened.Cancel[Reduce - only] settings and retry later");
            m_aErrors.Add(30019, "Order failed.A[Reduce - only] order can not be in the same direction as the open position  ");
            m_aErrors.Add(30020, "Trigger price for TP should be higher than mark price:");
            m_aErrors.Add(30021, "Trigger price for TP should be lower than mark price:");
            m_aErrors.Add(30022, "Trigger price for SL should be higher than mark price:");
            m_aErrors.Add(30023, "Trigger price fo SL should be lower than mark price:");
            m_aErrors.Add(30024, "Trigger price for SL should be lower than liq price:");
            m_aErrors.Add(30025, "Trigger price for SL should be higher than liq price:");
            m_aErrors.Add(30026, "TP price must be greater than last price:");
            m_aErrors.Add(30027, "TP price must be greater than mark price:");
            m_aErrors.Add(30028, "SL price must be less than last price:");
            m_aErrors.Add(30029, "SL price must be less than mark price:");
            m_aErrors.Add(30030, "SL price must be greater than last price:");
            m_aErrors.Add(30031, "SL price must be greater than mark price:");
            m_aErrors.Add(30032, "TP price must be less than last price:");
            m_aErrors.Add(30033, "TP price must be less than mark price:");
            m_aErrors.Add(30034, "TP price must be less than mark price:");
            m_aErrors.Add(30035, "SL price must be greater than trigger price:");
            m_aErrors.Add(30036, "TP price must be greater than trigger price:");
            m_aErrors.Add(30037, "TP price must be greater than trigger price:");
            m_aErrors.Add(30038, "TP / SL amount must be less than the size of the position.   ");
            m_aErrors.Add(30039, "The order qty can't be greater than the max order qty:");
            m_aErrors.Add(30040, "Futures trading is prohibited, please contact customer service.");
            m_aErrors.Add(30041, "Trigger price must be greater than 0   ");
            m_aErrors.Add(30042, "Client ID duplicate");
        }

        public string? ErrorToMessage(int nError)
        {
            CreateErrorDict();
            if (m_aErrors == null) return "ERROR MATRIX NOT FOUND";
            string? strResult = null;
            if (m_aErrors.TryGetValue(nError, out strResult)) return strResult;
            return "Unknown Error";
        }


    }
}
