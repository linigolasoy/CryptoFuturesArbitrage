using Crypto.Common;
using Crypto.Interface;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using CryptoClients.Net.Models;
using CryptoExchange.Net.SharedApis;
using System.Net.Mail;
using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Crypto.Interface.Futures;
using Crypto.Tests.Bitget;
using Newtonsoft.Json.Linq;

namespace Crypto.Tests
{
    [TestClass]
    public class CommonWebsocketTests   
    {

        private static DateTime m_dLastReceived = DateTime.MinValue;    
        [TestMethod]
        public async Task GmailTest()
        {
            try
            {
                UserCredential credential;
                FileStream oStream = new FileStream("D:/Bolsa/PennyStocks/GmailAppCredentials.json", FileMode.Open, FileAccess.Read);
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(oStream).Secrets,
                        new[] { GmailService.Scope.GmailReadonly },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                

                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "IpfGmailClient1",
                });

                // Retrieve emails
                var emailListRequest = service.Users.Messages.List("me");
                emailListRequest.MaxResults = 1000;
                var emailListResponse = emailListRequest.Execute();               
                
                foreach( var oMsg in emailListResponse.Messages )
                {
                    var oResultRequest = service.Users.Messages.Get("me", oMsg.Id);
                    var oResult = oResultRequest.Execute();

                    var oFrom = oResult.Payload.Headers.Where(p => p.Name == "From").FirstOrDefault();
                    var oSubject = oResult.Payload.Headers.Where(p => p.Name == "Subject").FirstOrDefault();
                    if ( oFrom != null && oSubject != null )
                    {
                        string strFrom = oFrom.Value;
                        string strSubject = oSubject.Value; 
                        if( strFrom.Contains("support@stockstotrade.com") && strSubject.Contains("Double Down"))
                        {
                            Console.WriteLine("Founs");
                        }
                    }
                    // Console.WriteLine(oResult.Snippet);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());   
            }
            Console.WriteLine("End");
        }

        [TestMethod]
        public async Task CommonSocketsTest()
        {
            IExchangeSocketClient oSocketClient = new ExchangeSocketClient();
            var oSymbol = new SharedSymbol(TradingMode.Spot, "ETH", "USDT");
            SubscribeOrderBookRequest oRequest = new SubscribeOrderBookRequest(oSymbol);
            var oResult = await oSocketClient.SubscribeToOrderBookUpdatesAsync(oRequest, OnOrderBook);
            await Task.Delay(60000);
        }

        private void OnOrderBook( ExchangeEvent<SharedOrderBook> oEvent )
        {
            Console.WriteLine(oEvent.ToString());
        }


        private string GetSubscribeOrderbook(string[] aSymbols)
        {
            JObject oObject = new JObject();
            oObject["op"] = "subscribe";

            JArray oArray = new JArray();
            foreach (string aSymbol in aSymbols)
            {
                JObject oNew = new JObject();
                oNew["instType"] = "USDT-FUTURES";
                oNew["channel"] = "books15";
                oNew["instId"] = aSymbol;
                oArray.Add(oNew);
            }
            oObject["args"] = oArray;

            return oObject.ToString();  
            // "{\"op\": \"subscribe\", \"args\": [ {\r\n            \"instType\": \"USDT-FUTURES\",\r\n            \"channel\": \"books15\",\r\n            \"instId\": \"BTCUSDT\"\r\n        }\r\n    ]\r\n}";

        }

        [TestMethod]
        public async Task BasicBitgetWs()
        {

            IFuturesExchange oExchange = await BitgetCommon.CreateExchange();
            string[] aSymbolString = oExchange.SymbolManager.GetAllKeys();
            ICommonWebsocket oWs = CommonFactory.CreateWebsocket("wss://ws.bitget.com/v2/ws/public", 20);

            oWs.OnReceived += OWs_OnReceived;
            // oWs.OnPing += BasicMexcOnPing;
            bool bResult = await oWs.Start();
            Assert.IsTrue(bResult);

            // Send subscribe

            // string strSend = "{\r\n    \"method\": \"SUBSCRIPTION\",\r\n    \"params\": [\r\n                \"spot@public.limit.depth.v3.api@BTCUSDT@5\"\r\n\r\n   ]\r\n}";

            string strSend = GetSubscribeOrderbook(aSymbolString);
            bool bSent = await oWs.Send(strSend);
            Assert.IsTrue(bSent);
            // Send subscribe message
            await Task.Delay(50000);

            decimal nDelay = (decimal)(DateTime.Now - m_dLastReceived).TotalMilliseconds;
            await oWs.Stop();

            Assert.IsTrue(oWs.Statistics.PingCount > 0);
            Assert.IsTrue(oWs.Statistics.ReceivedCount > 10);
            Assert.IsTrue(oWs.Statistics.SentCount > 1);

        }

        private void OWs_OnReceived(string strMessage)
        {
            JObject oObject = JObject.Parse(strMessage);
            if (oObject.ContainsKey("event")) return;
            if (!oObject.ContainsKey("action")) return;
            m_dLastReceived = DateTime.Now;
            return;
        }

        [TestMethod]
        public async Task BasicPingMexc()
        {


            ICommonWebsocket oWs = CommonFactory.CreateWebsocket("wss://contract.mexc.com/edge", 20);

            oWs.OnPing += BasicMexcOnPing;
            bool bResult = await oWs.Start();
            Assert.IsTrue(bResult);

            // Send subscribe
            // string strSend = "{\r\n    \"method\": \"SUBSCRIPTION\",\r\n    \"params\": [\r\n                \"spot@public.limit.depth.v3.api@BTCUSDT@5\"\r\n\r\n   ]\r\n}";

            string strSend = "{\r\n  \"method\": \"sub.tickers\",\r\n  \"param\": {}\r\n} ";
            bool bSent = await oWs.Send(strSend);   
            Assert.IsTrue(bSent);
            // Send subscribe message
            await Task.Delay(50000);

            await oWs.Stop();  
            
            Assert.IsTrue(oWs.Statistics.PingCount > 0 );
            Assert.IsTrue(oWs.Statistics.ReceivedCount > 10);
            Assert.IsTrue(oWs.Statistics.SentCount > 1);

        }

        private string BasicMexcOnPing()
        {
            return "{\r\n  \"method\": \"ping\"\r\n}";
        }


        [TestMethod]
        public async Task BasicPingBingx()
        {

            ICommonWebsocket oWs = CommonFactory.CreateWebsocket("wss://open-api-swap.bingx.com/swap-market", 0);

            oWs.OnReceived += BingxOnReceived;
            oWs.OnDisConnect += OWs_OnDisConnect;
            oWs.OnConnect += OWs_OnConnect;
            // oWs.OnPing += BasicMexcOnPing;
            bool bResult = await oWs.Start();
            Assert.IsTrue(bResult);

            // Send subscribe
            // string strSend = "{\r\n    \"method\": \"SUBSCRIPTION\",\r\n    \"params\": [\r\n                \"spot@public.limit.depth.v3.api@BTCUSDT@5\"\r\n\r\n   ]\r\n}";

            // string strSend = "{ \"id\": \"id1\", \"reqType\": \"sub\", \"dataType\": \"BTC-USDT@lastPrice\" }";
            string strSend = "{\"id\":\"Tururu\",\"reqType\": \"sub\",\"dataType\":\"BTC-USDT@bookTicker\"}";
            bool bSent = await oWs.Send(strSend);
            Assert.IsTrue(bSent);
            // Send subscribe message
            await Task.Delay(200000);

            await oWs.Stop();

            Assert.IsTrue(oWs.Statistics.PingCount > 0);
            Assert.IsTrue(oWs.Statistics.ReceivedCount > 10);
            Assert.IsTrue(oWs.Statistics.SentCount > 1);

        }

        private void OWs_OnConnect()
        {
            return;
        }

        private void OWs_OnDisConnect()
        {
            return;
        }

        private void BingxOnReceived(string strMessage)
        {
            if( string.IsNullOrEmpty(strMessage)) return;   
            if( strMessage.ToUpper().Contains("PING"))
            {
                return;
            }
            return;
        }
    }
}