using Crypto.Common.Websockets;
using Crypto.Interface;

namespace Crypto.Common
{





    public class CommonFactory
    {
        public static ICryptoSetup CreateSetup( string strFile ) 
        { 
            ICryptoSetup? oSetup = CryptoSetup.LoadFromFile( strFile );
            if (oSetup == null) throw new Exception("Error loading setup file");
            return oSetup; 
        }

        public static IRequestHelper CreateRequestHelper(HttpClient oClient, int nRequestsMinute ) { return new BaseRequestHelper(oClient, nRequestsMinute); }

        public static ITaskManager<T> CreateTaskManager<T>( int nMaximum )
        {
            return new BaseTaskManager<T>(nMaximum);
        }


        public static ICommonWebsocket CreateWebsocket( string strUrl, int nPingSeconds )
        {
            return new BaseWebsocket(strUrl, nPingSeconds);
        }


        /// <summary>
        /// Number of days in single request
        /// </summary>
        /// <param name="eFrame"></param>
        /// <returns></returns>
        public static int DaysFromTimeframe(Timeframe eFrame)
        {
            switch (eFrame)
            {
                case Timeframe.M1:
                    return 1;
                case Timeframe.M5:
                    return 5;
                case Timeframe.M15:
                    return 15;
                case Timeframe.H1:
                    return 60;
                case Timeframe.H4:
                    return 240;
                case Timeframe.D1:
                    return 365 * 2;
            }
            return 0;
        }
    }
}
