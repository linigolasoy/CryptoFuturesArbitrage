using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{
    public interface ITaskManager<T>
    {

        public int Maximum { get; }
        public Task Add(Task<T> oTask);

        public Task<T[]> GetResults();
    }
}
