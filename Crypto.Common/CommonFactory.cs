using Crypto.Interface;

namespace Crypto.Common
{

    internal class DummySetup : ICryptoSetup
    {

    }


    public class CommonFactory
    {
        public static ICryptoSetup CreateSetup() { return new DummySetup(); }
    }
}
