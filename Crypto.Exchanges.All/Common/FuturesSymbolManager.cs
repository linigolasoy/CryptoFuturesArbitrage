using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class FuturesSymbolManager: IFuturesSymbolManager
    {

        private SortedDictionary<string, IFuturesSymbol> m_oDictSymbols;

        public FuturesSymbolManager(IFuturesSymbol[] aSymbols) 
        {
            m_oDictSymbols = new SortedDictionary<string, IFuturesSymbol>();
            foreach (var oSymbol in aSymbols) m_oDictSymbols.Add(oSymbol.Symbol, oSymbol);
        }

        public IFuturesSymbol? GetSymbol(string strSymbol)
        {
            IFuturesSymbol? oResult = null;
            if (m_oDictSymbols.TryGetValue(strSymbol, out oResult)) return oResult;
            return null;
        }

        public string[] GetAllKeys()
        {
            return m_oDictSymbols.Keys.ToArray();
        }

        public IFuturesSymbol[] GetAllValues()
        {
            return m_oDictSymbols.Values.ToArray(); 
        }
    }
}
