using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface IRateLimitManager
    {
        public int Seconds { get; }
        public int Limit { get; }

        public Task Wait();

    }
}
