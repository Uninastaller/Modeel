﻿using Common.Enum;
using Common.Interface;
using ConfigManager;
using Logger;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace Common.Model
{
   public static class ResourceInformer
   {

      #region Properties



      #endregion Properties

      #region PublicFields



      #endregion PublicFields

      #region PrivateFields

      private const int _kilobyte = 0x400;
      private const int _megabyte = 0x100000;
      private const string _cftsDirectoryName = "CFTS";
      private const string _cftsFileExtensions = ".cfts";

      #endregion PrivateFields

      #region ProtectedFields



      #endregion ProtectedFields

      #region Ctor



      #endregion Ctor

      #region PublicMethods

      public static string FormatDataTransferRate(long bytesSent)
      {
         return $"{BytesToFormatedText(bytesSent)}/s";
      }

      public static string BytesToFormatedText(long bytes)
      {
         string unit;
         double transferRate;

         if (bytes < _kilobyte)
         {
            transferRate = bytes;
            unit = "B";
         }
         else if (bytes < _megabyte)
         {
            transferRate = (double)bytes / _kilobyte;
            unit = "KB";
         }
         else
         {
            transferRate = (double)bytes / _megabyte;
            unit = "MB";
         }

         return $"{transferRate:F2} {unit}";
      }

      public static void LoadCftsJsonsAndSendToSession(string uploadingDirectoryPath, ISession session)
      {
         string cftsFilesDirectory = Path.Combine(uploadingDirectoryPath, _cftsDirectoryName);
         if (Directory.Exists(cftsFilesDirectory))
         {
            string[] files = Directory.GetFiles(cftsFilesDirectory);

            // Filter the list to include only files with the desired extension
            var filteredFiles = files.Where(f => Path.GetExtension(f).Equals(_cftsFileExtensions)).ToList();

            for (int i = 0; i < files.Length; i++)
            {
               string jsonString = File.ReadAllText(files[i]);    // Read content of file

               Log.WriteLog(LogLevel.INFO, $"Reading content of file: {files[i]}, content: {jsonString}");
               // Validate if conten is valid json
               try
               {
                  // Attempt to parse the JSON string
                  OfferingFileDto? offeringFileDto = OfferingFileDto.ToObjectFromJson(jsonString);

                  // If parsing succeeds, the JSON is valid
                  Log.WriteLog(LogLevel.INFO, "Content is valid");
                  FlagMessagesGenerator.GenerateOfferingFile(jsonString, i == files.Count() - 1, session);
               }
               catch (JsonException ex)
               {
                  // Parsing failed, so the JSON is not valid
                  Log.WriteLog(LogLevel.WARNING, "Content is invalid! " + ex.Message);
               }

            }
         }
      }

      public static void CreateAndSendOfferingFilesToCentralServer(IPAddress ipAddress, int port, ISession session) => CreateAndSendOfferingFilesToCentralServer(ipAddress.ToString(), port, session);

      public static void CreateAndSendOfferingFilesToCentralServer(string ipAddress, int port, ISession session)
      {
         string UploadingDirectoryPath = MyConfigManager.GetConfigStringValue("UploadingDirectory");
         CreateJsonFiles(ipAddress, port, UploadingDirectoryPath);
         LoadCftsJsonsAndSendToSession(UploadingDirectoryPath, session);
      }

      public static void CreateJsonFiles(IPAddress ipAddress, int port, string uploadingDirectoryPath) => CreateJsonFiles(ipAddress.ToString(), port, uploadingDirectoryPath);

      public static void CreateJsonFiles(string ipAddress, int port, string uploadingDirectoryPath)
      {
         string directoryPathToCftsDirectory = Path.Combine(uploadingDirectoryPath, _cftsDirectoryName);
         if (!Directory.Exists(directoryPathToCftsDirectory))
         {
            Directory.CreateDirectory(directoryPathToCftsDirectory);
         }

         string[] files = Directory.GetFiles(uploadingDirectoryPath);

         foreach (string filePath in files)
         {
            FileInfo fileInfo = new FileInfo(filePath);
            CreateJsonFile(ipAddress, port, fileInfo);
         }
         Log.WriteLog(LogLevel.INFO, "Created .ctfs files in director: " + directoryPathToCftsDirectory);
      }

      public static void CreateJsonFile(string ipAddress, int port, FileInfo fileInfo)
      {
         bool useSsl = true;
         if (!MyConfigManager.TryGetBoolConfigValue("CreateSslOfferingFiles", out useSsl))
         {
            Log.WriteLog(LogLevel.WARNING, $"Could not find useSsl propertie in config, using default: {useSsl}");
         }

         var offeringFileDto = new OfferingFileDto($"{ipAddress}:{port}", useSsl ? TypeOfServerSocket.TCP_SERVER_SSL : TypeOfServerSocket.TCP_SERVER)
         {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
         };

         string json = offeringFileDto.GetJson();
         string jsonFileName = $"{offeringFileDto.OfferingFileIdentificator}{_cftsFileExtensions}";
         string jsonFilePath = Path.Combine(fileInfo.DirectoryName, _cftsDirectoryName, jsonFileName);

         File.WriteAllText(jsonFilePath, json);
      }

      #endregion PublicMethods

      #region PrivateMethods



      #endregion PrivateMethods

      #region ProtectedMethods



      #endregion ProtectedMethods

      #region Events



      #endregion Events

      #region OverridedMethods



      #endregion OverridedMethods

   }
}
