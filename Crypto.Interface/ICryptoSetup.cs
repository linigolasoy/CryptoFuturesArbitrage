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


        public ExchangeType[] ExchangeTypes { get; }
        public decimal Amount { get; }  
        public int Leverage {  get; }   
        public string LogPath { get; }  
        public decimal PercentMinimum { get; }  
    }
}
