using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Market
{
    public interface IUpdateableObject<T>
    {

        public void Update(T obj);
    }
}
