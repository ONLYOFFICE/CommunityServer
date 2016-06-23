/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using ASC.Core.Notify;
using log4net;

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
