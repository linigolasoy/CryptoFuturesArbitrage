using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface IBalance
    {
        public string Currency { get; }
        public decimal Avaliable { get; }   
        public decimal Locked { get; }
        public decimal Total { get; }
    }
}
