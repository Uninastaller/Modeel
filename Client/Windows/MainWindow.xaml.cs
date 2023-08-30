﻿using Common.Enum;
using Common.Interface;
using Common.Model;
using Common.ThreadMessages;
using ConfigManager;
using Logger;
using SslTcpSession;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;

namespace Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindowForWPF
    {

        #region Properties

        public ClientSocketState ConnectionWithCentralServerSocketState
        {
            get => _connectionWithCentralServerSocketState;
            set
            {
                _connectionWithCentralServerSocketState = value;
                Log.WriteLog(LogLevel.INFO, "Connection With Central Server Socekt State Change To " + value);

                switch (value)
                {
                    case ClientSocketState.CONNECTED:
                        elpServerStatus.Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x26, 0x3F, 0x03)); // Using Color class
                        break;
                    case ClientSocketState.DISCONNECTED:
                        elpServerStatus.Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x74, 0x1B, 0x0C)); // Using Color class
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion Properties

        #region PublicFields



        #endregion PublicFields

        #region PrivateFields

        private IUniversalClientSocket? _socketToCentralServer;
        private readonly string _certificateNameForCentralServerConnect = "MyTestCertificateClient.pfx";
        private SslContext? _contextForCentralServerConnect;
        private IPAddress _centralServerIpAddress = NetworkUtils.GetLocalIPAddress() ?? IPAddress.Loopback;
        private int _centralServerPort = 34258;
        private ClientSocketState _connectionWithCentralServerSocketState = ClientSocketState.DISCONNECTED;

        #endregion PrivateFields

        #region ProtectedFields



        #endregion ProtectedFields

        #region Ctor

        public MainWindow()
        {
            InitializeComponent();

            contract.Add(MsgIds.ClientSocketStateChangeMessage, typeof(ClientSocketStateChangeMessage));

            if (MyConfigManager.TryGetConfigValue<Int32>("CeentralServerPort", out Int32 centralServerPort))
            {
                _centralServerPort = centralServerPort;
            }

            Init();
        }

        #endregion Ctor

        #region PublicMethods



        #endregion PublicMethods

        #region PrivateMethods

        private void Init()
        {

            msgSwitch
             .Case(contract.GetContractId(typeof(ClientSocketStateChangeMessage)), (ClientSocketStateChangeMessage x) => ClientSocketStateChangeMessageHandler(x))
             ;

            tbTitle.Text = $"Custom File Transfer System [v.{Assembly.GetExecutingAssembly().GetName().Version}]";

            _contextForCentralServerConnect = new SslContext(SslProtocols.Tls12, new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _certificateNameForCentralServerConnect), ""), (sender, certificate, chain, sslPolicyErrors) => true);
            _socketToCentralServer = new SslClientBussinesLogic(_contextForCentralServerConnect, _centralServerIpAddress, _centralServerPort, this,
                typeOfSession: Common.Enum.TypeOfSession.SESSION_WITH_CENTRAL_SERVER, optionReceiveBufferSize: 0x2000, optionSendBufferSize: 0x2000);

        }

        private void ClientSocketStateChangeMessageHandler(ClientSocketStateChangeMessage message)
        {
            if (message.TypeOfSession == TypeOfSession.SESSION_WITH_CENTRAL_SERVER)
            {
                ConnectionWithCentralServerSocketState = message.ClientSocketState;
            }
        }

        #endregion PrivateMethods

        #region ProtectedMethods



        #endregion ProtectedMethods

        #region Events

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            this.DragMove();
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }


        private void btnMinimize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal; // Restore window size
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized; // Maximize window
            }
        }

        #endregion Events

        #region OverridedMethods



        #endregion OverridedMethods

    }
}
