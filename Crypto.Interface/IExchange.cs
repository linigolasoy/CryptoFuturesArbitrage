
namespace Crypto.Interface
{


    public enum ExchangeType
    {
        // MexcSpot,
        // MexcFutures,
        //BingxSpot,
        CoinExFutures,
        BingxFutures,
        BitgetFutures,
        BitmartFutures,
        BitUnixFutures
    }
    /// <summary>
    /// Exchange base, independent of futures or spot
    /// </summary>
    public interface IExchange
    {
        public ICryptoSetup Setup { get; }

        public ExchangeType ExchangeType { get; }

        // public Task<ICryptoWebsocket?> CreateWebsocket();

        // public Task<ISymbol[]?> GetRawSymbols();

    }
}
