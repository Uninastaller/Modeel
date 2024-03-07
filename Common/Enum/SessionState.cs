﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enum
{
    public enum SessionState
    {
        NONE,
        FILE_REQUEST,
        FILE_PART_REQUEST,
        OFFERING_FILES_RECEIVING,
        OFFERING_FILES_SENDING,
        NODE_LIST_SENDING,
        WAITING_TO_CENTRAL_SERVER_TO_SEND_OFFERING_FILES
    }
}
