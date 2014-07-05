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
using System.Collections.Generic;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

namespace ASC.Notify.Messages
{
    [Serializable]
    public class NoticeMessage : INoticeMessage
    {
        [NonSerialized]
        private readonly List<ITagValue> arguments = new List<ITagValue>();

        [NonSerialized]
        private IPattern pattern;

        public NoticeMessage()
        {
        }

        public NoticeMessage(IDirectRecipient recipient, INotifyAction action, string objectID)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            Recipient = recipient;
            Action = action;
            ObjectID = objectID;
        }

        public NoticeMessage(IDirectRecipient recipient, INotifyAction action, string objectID, IPattern pattern)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (pattern == null) throw new ArgumentNullException("pattern");
            Recipient = recipient;
            Action = action;
            Pattern = pattern;
            ObjectID = objectID;
            ContentType = pattern.ContentType;
        }

        public NoticeMessage(IDirectRecipient recipient, string subject, string body, string contentType)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (body == null) throw new ArgumentNullException("body");
            Recipient = recipient;
            Subject = subject;
            Body = body;
            ContentType = contentType;
        }

        public string ObjectID { get; private set; }

        public IDirectRecipient Recipient { get; private set; }

        public IPattern Pattern
        {
            get { return pattern; }
            internal set { pattern = value; }
        }

        public INotifyAction Action { get; private set; }

        public ITagValue[] Arguments
        {
            get { return arguments.ToArray(); }
        }

        public void AddArgument(params ITagValue[] tagValues)
        {
            if (tagValues == null) throw new ArgumentNullException("tagValues");
            Array.ForEach(tagValues,
                tagValue => 
                {   
                    if (!arguments.Exists(tv => Equals(tv.Tag, tagValue.Tag)))
                    {
                        arguments.Add(tagValue);
                    }
                });
        }

        public ITagValue GetArgument(string tag)
        {
            return arguments.Find(r => r.Tag == tag);
        }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string ContentType { get; internal set; }
    }
}