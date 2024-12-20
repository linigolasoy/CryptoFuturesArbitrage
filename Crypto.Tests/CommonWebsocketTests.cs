using Crypto.Common;
using Crypto.Exchange.Mexc;
using Crypto.Interface;

namespace Crypto.Tests
{
    [TestClass]
    public class CommonWebsocketTests   
    {
        [TestMethod]
        public async Task BasicPing()
        {

            ICommonWebsocket oWs = CommonFactory.CreateWebsocket("wss://contract.mexc.com/edge", 20);

            oWs.OnPing += BasicOnPing;
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

        private string BasicOnPing()
        {
            return "{\r\n  \"method\": \"ping\"\r\n}";
        }
    }
}