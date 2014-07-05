/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        public static void SendAboutBackupCompleted(int tenantId, Guid userId, string link, DateTime expireDate)
        {
            SendNotification(MethodBackupCompleted, tenantId, userId, link, expireDate);
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
