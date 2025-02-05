using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class TradingResult<T> : ITradingResult<T>
    {
        public TradingResult( T oResult ) 
        { 
            Result = oResult;
            Success = true;
        }

        public TradingResult( Exception ex ) 
        { 
            Exception = ex;
            Success = false;
        }

        public TradingResult( string strMessage )
        {
            Success = false;
            Message = strMessage;
            Exception = new Exception( strMessage );
        }
        public bool Success { get; private set; } = false;

        public Exception? Exception { get; private set; } = null;

        public string? Message { get; private set; } = null;

        public T? Result { get; private set; }
    }
}
