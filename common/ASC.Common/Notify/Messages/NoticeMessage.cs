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