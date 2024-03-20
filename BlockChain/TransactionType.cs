﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain
{
    public enum TransactionType
    {
        ADD_FILE = 0,
        ADD_FILE_REQUEST = 1,
        REMOVE_FILE = 2,
        REMOVE_FILE_REQUEST = 3,
        ADD_CREDIT = 4
    }
}
