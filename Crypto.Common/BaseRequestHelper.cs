using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{
    internal class BaseRequestHelper : IRequestHelper
    {

        public BaseRequestHelper( HttpClient oClient, int nRequestsPerMinute ) 
        { 
            Client = oClient;
            RequestsPerMinute = nRequestsPerMinute;
            m_nRequestsPerSecond = nRequestsPerMinute / 60;
            if (m_nRequestsPerSecond <= 0) m_nRequestsPerSecond = 1;
        }
        private SemaphoreSlim m_oSemaphore = new SemaphoreSlim(1, 1);
        public HttpClient Client { get; }

        public int RequestsPerMinute { get; }
        private int m_nRequestsPerSecond = 0;

        private DateTime m_dLastRequest = DateTime.Now;
        private int m_nRequestCount = 0;

        public async Task<string?> GetRequest(string strUrl)
        {
            DateTime dNow = DateTime.Now;
            double nSeconds = (dNow - m_dLastRequest).TotalSeconds;
            try
            {
                await m_oSemaphore.WaitAsync();
                m_nRequestCount++;
                if (m_nRequestCount >= m_nRequestsPerSecond )
                {
                    await Task.Delay(1000);
                    m_nRequestCount = 0;
                }
                m_dLastRequest = DateTime.Now;
            }
            finally
            {
                m_oSemaphore.Release(); 
            }
            HttpResponseMessage oResponse = await Client.GetAsync(strUrl);
            if (!oResponse.IsSuccessStatusCode) return null;
            string strResponse = await oResponse.Content.ReadAsStringAsync();
            return strResponse;
        }
    }
}
