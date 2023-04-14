﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Modeel.P2P
{
    public interface IP2pMasterClass
    {
        public void CloseAllConnections();
        public void CreateNewServer(ServerBussinesLogic serverBussinesLogic);
        public void CreateNewClient(ClientBussinesLogic clientBussinesLogic);

    }
}
