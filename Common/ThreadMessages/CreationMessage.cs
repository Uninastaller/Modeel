﻿using Common.Enum;
using Common.Model;
using System;

namespace Common.ThreadMessages
{
   public class CreationMessage : MsgBase<CreationMessage>
   {
      public CreationMessage(Guid sessionGuid, TypeOfSocket typeOfSocket, TypeOfSession typeOfSession) : base(typeof(CreationMessage))
      {
         SessionGuid = sessionGuid;
         TypeOfSocket = typeOfSocket;
         TypeOfSession = typeOfSession;
      }

      public CreationMessage() : base(typeof(CreationMessage))
      {

      }

      public Guid SessionGuid { get; set; }
      public TypeOfSocket TypeOfSocket { get; set; }
      public TypeOfSession TypeOfSession { get; set; }

   }
}
