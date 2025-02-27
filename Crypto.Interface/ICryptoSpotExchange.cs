﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface ICryptoSpotExchange: IExchange
    {

        public Task<ISpotSymbol[]?> GetSymbols();
    }
}
