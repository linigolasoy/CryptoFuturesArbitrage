using Crypto.Interface;

namespace Crypto.Exchange.Mexc
{
    public class MexcSpotExchange : ICryptoSpotExchange
    {
        public MexcSpotExchange(ICryptoSetup oSetup)
        {
            Setup = oSetup; 
        }
        public ICryptoSetup Setup { get; }

        public async Task<ISymbol[]?> GetSymbols()
        {
            throw new NotImplementedException();
        }
    }
}
