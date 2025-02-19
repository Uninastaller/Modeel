﻿using Common.Enum;
using Common.Interface;
using Common.Model;
using Common.ThreadMessages;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Media;
using TcpSession;

namespace Modeel
{
    /// <summary>
    /// Interaction logic for TorOnionInterfaceWindow.xaml
    /// </summary>
    public partial class TorOnionInterfaceWindow : BaseWindowForWPF
    {
        private readonly string _torDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tor");
        private readonly string _defaultTorrcContent = "SocksPort 9050\nControlPort 9051";
        private readonly IPAddress _ipAddress = IPAddress.Loopback;
        private readonly int _sockPort = 9050;
        private readonly int _controlPort = 9051;
        private IUniversalClientSocket _controlSocket;


        public bool TorDirectoryExist => Directory.Exists(_torDirectoryPath);
        public string TorrcFilePath => Path.Combine(_torDirectoryPath, "torrc");
        public bool TorrcFileExist => File.Exists(TorrcFilePath);

        public TorOnionInterfaceWindow()
        {
            InitializeComponent();
            contract.Add(MsgIds.MessageReceiveMessage, typeof(MessageReceiveMessage));
            contract.Add(MsgIds.ClientSocketStateChangeMessage, typeof(ClientSocketStateChangeMessage));

            Init();

            Closed += Window_closedEvent;


            if (!TorDirectoryExist)
            {
                Directory.CreateDirectory(_torDirectoryPath);
            }

            if (!TorrcFileExist)
            {
                FillTorrcWithDefaultContent();
            }

            _controlSocket = new ClientBussinesLogic2(_ipAddress, _controlPort, this, typeOfSession: TypeOfSession.TOR_CONTROL_SESSION);
        }

        internal void Init()
        {
            msgSwitch
             .Case(contract.GetContractId(typeof(MessageReceiveMessage)), (MessageReceiveMessage x) => MessageReceiveMessageHandler(x.Message))
             .Case(contract.GetContractId(typeof(ClientSocketStateChangeMessage)), (ClientSocketStateChangeMessage x) => ClientSocketStateChangeMessageHandler(x))
             ;
        }

        private void MessageReceiveMessageHandler(string message)
        {
            tbkTextForControlSocket.Text += "[SERVER]: " + message;
            tbkTextForControlSocket.ScrollToEnd();
        }

        private void ClientSocketStateChangeMessageHandler(ClientSocketStateChangeMessage message)
        {
            if (message.TypeOfSession == TypeOfSession.TOR_CONTROL_SESSION)
            {
                if (message.ClientSocketState == ClientSocketState.CONNECTED)
                {
                    rtgControlSocketState.Fill = new SolidColorBrush(Colors.Green);
                    SendStringToControlSocket("AUTHENTICATE ");
                    //SendStringToControlSocket("SETEVENTS CIRC");

                    //SendStringToControlSocket("SETEVENTS CIRC INFO WARN ERR");
                    SendStringToControlSocket("SETEVENTS CIRC STREAM DEBUG INFO NOTICE WARN ERR");

                }
                else if (message.ClientSocketState == ClientSocketState.DISCONNECTED)
                {
                    rtgControlSocketState.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void FillTorrcWithDefaultContent()
        {
            File.WriteAllText(TorrcFilePath, _defaultTorrcContent);
        }

        private void Window_closedEvent(object? sender, EventArgs e)
        {
            Closed -= Window_closedEvent;

            _controlSocket.DisconnectAndStop();
            _controlSocket.Dispose();
        }

        private void SendStringToControlSocket(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message += "\r\n";
                _controlSocket.Send(Encoding.ASCII.GetBytes(message));
                tbkTextForControlSocket.Text += "[CLIENT]: " + message;
                tbkTextForControlSocket.ScrollToEnd();
            }
        }

        private void btSendToControlSocket_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendStringToControlSocket(tbTextForControlSocket.Text);
        }

        private void btStartTor_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process[] runningProcessByName = Process.GetProcessesByName("tor");
            if (runningProcessByName.Length == 0)
            {
                Process.Start("tor.exe");
            }
        }
    }
}
