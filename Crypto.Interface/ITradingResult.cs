using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface ITradingResult<T>
    {
        public bool Success { get; }    
        public Exception? Exception { get; }

        public string? Message { get; } 
        public T? Result { get; }
    }
}
