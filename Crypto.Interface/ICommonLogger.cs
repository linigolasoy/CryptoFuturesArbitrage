using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    /// <summary>
    /// Logger
    /// </summary>
    public interface ICommonLogger
    {
        public void Info(string strMessage);
        public void Warning(string strMessage, Exception? ex = null);
        public void Error(string strMessage, Exception? ex = null);

    }
}
