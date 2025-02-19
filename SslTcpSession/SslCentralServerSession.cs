﻿using Common.Enum;
using Common.Model;
using Logger;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SslTcpSession
{
   class SslCentralServerSession : SslSession
   {

      #region Properties

      public SessionState SessionState
      {
         get => _sessionState;

         set
         {
            if (value != _sessionState)
            {
               _sessionState = value;
               ServerSessionStateChange?.Invoke(this, value);
            }
         }
      }

      #endregion Properties

      #region PublicFields


      #endregion PublicFields

      #region PrivateFields

      private SessionState _sessionState = SessionState.NONE;

      #endregion PrivateFields

      #region ProtectedFields



      #endregion ProtectedFields

      #region Ctor

      public SslCentralServerSession(SslServer server) : base(server)
      {
         Log.WriteLog(LogLevel.INFO, $"Guid: {Id}, Starting");

         _flagSwitch.OnNonRegistered(OnNonRegistredMessage);
         _flagSwitch.Register(SocketMessageFlag.OFFERING_FILE, OnOfferingFilesHandler);
         _flagSwitch.Register(SocketMessageFlag.OFFERING_FILES_REQUEST, OnOfferingFilesRequestHandler);
      }

      #endregion Ctor

      #region PublicMethods



      #endregion PublicMethods

      #region PrivateMethods

      private void OnClientDisconnected()
      {
         ClientDisconnected?.Invoke(this);
      }

      private void OnNewOfferingFile(List<OfferingFileDto> offeringFilesDto)
      {
         NewOfferingFiles?.Invoke(offeringFilesDto);
      }

      private void OnRequestForOfferingFiles()
      {
         RequestForOfferingFiles?.Invoke(this.Id);
      }

      //private async Task OnNewOfferingFilesReceived(List<OfferingFileDto> offeringFilesDto)
      //{
      //   foreach (OfferingFileDto offeringFileDto in offeringFilesDto)
      //   {
      //      await SqliteDataAccess.InsertOrUpdateOfferingFileDtoAsync(offeringFileDto);
      //   }
      //}

      //private async Task SendOfferingFilesToClient()
      //{
      //   SessionState = SessionState.OFFERING_FILES_SENDING;
      //   List<OfferingFileDto> offeringFiles = await SqliteDataAccess.GetAllOfferingFilesWithEndpointsAsync();
      //   for (int i = 0; i < offeringFiles.Count; i++)
      //   {
      //      FlagMessagesGenerator.GenerateOfferingFile(offeringFiles[i].GetJson(), i == offeringFiles.Count - 1, this);
      //   }
      //}

      #endregion PrivateMethods

      #region ProtectedMethods

      protected override void OnHandshaked()
      {
         Log.WriteLog(LogLevel.INFO, $"Ssl session with Id {Id} handshaked!");

         //int maxRepeatCounter = 3;
         //await Task.Delay(100);

         //SessionState = SessionState.OFFERING_FILES_RECEIVING;

         //// Staf to do after 200ms => waiting without blocking thread
         //while (FlagMessagesGenerator.GenerateOfferingFilesRequest(this) == MethodResult.ERROR && maxRepeatCounter-- >= 0)
         //{
         //    await Task.Delay(200);
         //}

         //if (maxRepeatCounter < -1)
         //{
         //    Disconnect();
         //}
      }

      protected override void OnDisconnected()
      {
         OnClientDisconnected();
         Log.WriteLog(LogLevel.INFO, $"Ssl session with Id {Id} disconnected!");
      }

      protected override void OnReceived(byte[] buffer, long offset, long size)
      {
         _flagSwitch.Switch(buffer, offset, size);
      }

      protected override void OnError(SocketError error)
      {
         Log.WriteLog(LogLevel.ERROR, $"Ssl session caught an error with code {error}");
      }

      #endregion ProtectedMethods

      #region Events

      public delegate void ClientDisconnectedHandler(SslSession sender);
      public event ClientDisconnectedHandler? ClientDisconnected;

      public delegate void ServerSessionStateChangeEventHandler(SslSession sender, SessionState serverSessionState);
      public event ServerSessionStateChangeEventHandler? ServerSessionStateChange;

      public delegate void NewOfferingFilesHandler(List<OfferingFileDto> offeringFilesDto);
      public event NewOfferingFilesHandler? NewOfferingFiles;

      public delegate void RequestForOfferingFilesHandler(Guid sessionId);
      public event RequestForOfferingFilesHandler? RequestForOfferingFiles;

      private void OnNonRegistredMessage(string message)
      {
         SessionState = SessionState.NONE;
         this.Server?.FindSession(this.Id)?.Disconnect();
         Log.WriteLog(LogLevel.WARNING, $"Non registered message received, disconnecting client!");
      }

      private void OnOfferingFilesHandler(byte[] buffer, long offset, long size)
      {

         if (SessionState == SessionState.NONE)
         {
            SessionState = SessionState.OFFERING_FILES_RECEIVING;
         }

         if (SessionState == SessionState.OFFERING_FILES_RECEIVING)
         {
            if (FlagMessageEvaluator.EvaluateOfferingFileMessage(buffer, offset, size, out List<OfferingFileDto> offeringFilesDto, out bool endOfMessageGroup))
            {
               //await OnNewOfferingFilesReceived(offeringFilesDto);
               OnNewOfferingFile(offeringFilesDto);
               if (endOfMessageGroup)
               {
                  // Client should disconnect automatically after send all data, but if no, manually destroy session
                  Log.WriteLog(LogLevel.INFO, $"All Offering File was receiver! Destroying session");
                  Disconnect();
               }
            }
         }
         else
         {
            Log.WriteLog(LogLevel.WARNING, $"Offering File received, but session is not in default state, so message can not be proceed!");
         }
      }

      /// <summary>
      /// If server have no offering files, so server will not automatically send him offering files bcs of leak end of message group. He send offering files request to get offering files from server
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="offset"></param>
      /// <param name="size"></param>
      private void OnOfferingFilesRequestHandler(byte[] buffer, long offset, long size)
      {
         Log.WriteLog(LogLevel.DEBUG, $"Offering File request received");
         //if (SessionState == SessionState.NONE)
         //{
         //   await SendOfferingFilesToClient();
         //   Log.WriteLog(LogLevel.INFO, $"All Offering File was sended to client! Destroying session");
         //   // Client should disconnect automatically after send all data, but if no, manually destroy session
         //   Disconnect();
         //}
         //else
         //{
         //   Log.WriteLog(LogLevel.WARNING, $"Offering File request received, but session is not in default state for this message, so message will not be proceed!");
         //}
         if (SessionState == SessionState.NONE)
         {
            SessionState = SessionState.WAITING_TO_CENTRAL_SERVER_TO_SEND_OFFERING_FILES;
            OnRequestForOfferingFiles();
         }
         else
         {
            Log.WriteLog(LogLevel.WARNING, $"Offering File request received, but session is not in default state for this message, so message will not be proceed!");
         }
      }

      #endregion Events

      #region OverridedMethods



      #endregion OverridedMethods

   }
}

