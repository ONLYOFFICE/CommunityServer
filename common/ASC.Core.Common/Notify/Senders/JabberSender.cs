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

using ASC.Core.Notify.Jabber;
using ASC.Notify.Messages;
using log4net;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ASC.Core.Notify.Senders
{
    public class JabberSender : INotifySender
    {
        private JabberServiceClient service = new JabberServiceClient();
        private static readonly ILog log = LogManager.GetLogger(typeof(JabberServiceClient));

        public void Init(IDictionary<string, string> properties)
        {
        }

        public NoticeSendResult Send(NotifyMessage m)
        {
            var text = m.Content;
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("\r\n", "\n").Trim('\n', '\r');
                text = Regex.Replace(text, "\n{3,}", "\n\n");
            }
            try
            {
                service.SendMessage(m.Tenant, null, m.To, text, m.Subject);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unexpected error, {1}, {2}, {3}",
                       e.Message, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
            return NoticeSendResult.OK;
        }
    }
}
