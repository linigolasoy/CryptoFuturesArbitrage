namespace Crypto.Interface
{

    public interface IApiKey
    {
        public ExchangeType ExchangeType { get; }   
        public string ApiKey { get; }
        public string ApiSecret { get; }   
    }
    public interface ICryptoSetup
    {
        public IApiKey[] ApiKeys { get; }   
    }
}
