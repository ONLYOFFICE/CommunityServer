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


using System.Collections.Generic;
using System.Web;

using ASC.Common.Logging;
using ASC.Files.Core;
using ASC.MessagingSystem;

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

            SendHeadersMessage(headers, action, MessageTarget.Create(entry.ID), description);
        }

        public static void Send(FileEntry entry1, FileEntry entry2, Dictionary<string, string> headers, MessageAction action, params string[] description)
        {
            // do not log actions in users folder
            if (entry1 == null || entry2 == null || entry1.RootFolderType == FolderType.USER || entry2.RootFolderType == FolderType.USER) return;

            SendHeadersMessage(headers, action, MessageTarget.Create(new[] { entry1.ID, entry2.ID }), description);
        }

        private static void SendHeadersMessage(Dictionary<string, string> headers, MessageAction action, MessageTarget target, params string[] description)
        {
            if (headers == null)
            {
                log.Debug(string.Format("Empty Request Headers for \"{0}\" type of event", action));
                return;
            }

            MessageService.Send(headers, action, target, description);
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

            MessageService.Send(request, action, MessageTarget.Create(entry.ID), description);
        }


        public static void Send(FileEntry entry, MessageInitiator initiator, MessageAction action, params string[] description)
        {
            if (entry == null || entry.RootFolderType == FolderType.USER) return;

            MessageService.Send(initiator, action, MessageTarget.Create(entry.ID), description);
        }
    }
}