﻿using Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface IP2pMasterClass
    {
        void CloseAllConnections();
        void CreateNewServer(IUniversalServerSocket socketServer);
        void CreateNewClient(IUniversalClientSocket socketClient);
        void RemoveServer(IUniversalServerSocket socketServer);
        void RemoveClient(IUniversalClientSocket socketClient);
        long GetTotalUploadingSpeedOfAllRunningServersInBytes();
        long GetTotalUploadingSpeedOfAllRunningServersInKiloBytes();
        long GetTotalUploadingSpeedOfAllRunningServersInMegaBytes();
        long GetTotalDownloadingSpeedOfAllRunningClientsInBytes();
        long GetTotalDownloadingSpeedOfAllRunningClientsInKiloBytes();
        long GetTotalDownloadingSpeedOfAllRunningClientsInMegaBytes();
    }
}
