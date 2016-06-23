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


using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Files.Core;
using ASC.MessagingSystem;
using log4net;

namespace ASC.Web.Files.Helpers
{
    public static class FilesMessageService
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");


        public static void Send(Dictionary<string, string> headers, MessageAction action)
        {
            SendHeadersMessage(headers, action, null);
        }

        public static void Send(FileEntry entry, Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            SendHeadersMessage(headers, action, description);
        }

        public static void Send(FileEntry entry1, FileEntry entry2, Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry1 == null || entry2 == null || entry1.RootFolderType == FolderType.USER || entry2.RootFolderType == FolderType.USER) return;

            SendHeadersMessage(headers, action, description);
        }

        private static void SendHeadersMessage(Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            if (headers == null)
            {
                log.Debug(string.Format("Empty Request Headers for \"{0}\" type of event", action));
                return;
            }

            MessageService.Send(headers, action, description);
        }


        public static void Send(FileEntry entry, HttpRequest request, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            if (request == null)
            {
                log.Debug(string.Format("Empty Http Request for \"{0}\" type of event", action));
                return;
            }

            MessageService.Send(request, action, description);
        }


        public static void Send(FileEntry entry, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            SendInitiatorMessage(entry, initiator, action, description);
        }

        private static void SendInitiatorMessage(FileEntry entry, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            MessageService.Send(initiator, action, description);
        }
    }
}