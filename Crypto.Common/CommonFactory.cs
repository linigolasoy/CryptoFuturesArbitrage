using Crypto.Interface;

namespace Crypto.Common
{

    internal class DummySetup : ICryptoSetup
    {

    }


    public class CommonFactory
    {
        public static ICryptoSetup CreateSetup() { return new DummySetup(); }

        public static IRequestHelper CreateRequestHelper(HttpClient oClient, int nRequestsMinute ) { return new BaseRequestHelper(oClient, nRequestsMinute); }

        public static ITaskManager<T> CreateTaskManager<T>( int nMaximum )
        {
            return new BaseTaskManager<T>(nMaximum);
        }
    }
}
