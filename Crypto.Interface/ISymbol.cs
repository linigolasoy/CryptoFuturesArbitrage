﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface ISymbol
    {
        public string Symbol { get; }
        public string Base { get; }
        public string Quote { get; }

    }
}
