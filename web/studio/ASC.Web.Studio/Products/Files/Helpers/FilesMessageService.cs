/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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