/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using ASC.Common.Logging;
using ASC.Core.Notify;

namespace ASC.Data.Backup
{
    internal static class NotifyHelper
    {
        private const string NotifyService = "ASC.Web.Studio.Core.Notify.StudioNotifyService, ASC.Web.Studio";
        private const string MethodTransferStart = "MigrationPortalStart";
        private const string MethodTransferCompleted = "MigrationPortalSuccess";
        private const string MethodTransferError = "MigrationPortalError";
        private const string MethodBackupCompleted = "SendMsgBackupCompleted";
        private const string MethodRestoreStarted = "SendMsgRestoreStarted";
        private const string MethodRestoreCompleted = "SendMsgRestoreCompleted";

        public static void SendAboutTransferStart(int tenantId, string targetRegion, bool notifyUsers)
        {
            SendNotification(MethodTransferStart, tenantId, targetRegion, notifyUsers);
        }

        public static void SendAboutTransferComplete(int tenantId, string targetRegion, string targetAddress, bool notifyOnlyOwner)
        {
            SendNotification(MethodTransferCompleted, tenantId, targetRegion, targetAddress, !notifyOnlyOwner);
        }

        public static void SendAboutTransferError(int tenantId, string targetRegion, string resultAddress, bool notifyOnlyOwner)
        {
            SendNotification(MethodTransferError, tenantId, targetRegion, resultAddress, !notifyOnlyOwner);
        }

        public static void SendAboutBackupCompleted(int tenantId, Guid userId, string link)
        {
            SendNotification(MethodBackupCompleted, tenantId, userId, link);
        }

        public static void SendAboutRestoreStarted(int tenantId, bool notifyAllUsers)
        {
            SendNotification(MethodRestoreStarted, tenantId, notifyAllUsers);
        }

        public static void SendAboutRestoreCompleted(int tenantId, bool notifyAllUsers)
        {
            SendNotification(MethodRestoreCompleted, tenantId, notifyAllUsers);
        }

        private static void SendNotification(string method, int tenantId, params object[] args)
        {
            try
            {
                using (var notifyClient = new NotifyServiceClient())
                {
                    notifyClient.InvokeSendMethod(NotifyService, method, tenantId, args);
                }
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Backup").Warn("Error while sending notification", error);
            }
        }
    }
}
