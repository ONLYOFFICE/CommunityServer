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
using System.Collections;
using System.Collections.Generic;
using ASC.Notify.Messages;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using log4net;

namespace ASC.Notify.Engine
{
    public class NotifyRequest
    {
        public INotifySource NotifySource { get; internal set; }

        public INotifyAction NotifyAction { get; internal set; }

        public string ObjectID { get; internal set; }

        public IRecipient Recipient { get; internal set; }

        public List<ITagValue> Arguments { get; internal set; }

        public string CurrentSender { get; internal set; }

        public INoticeMessage CurrentMessage { get; internal set; }

        public Hashtable Properties { get; private set; }

        internal string[] SenderNames { get; set; }

        internal IPattern[] Patterns { get; set; }

        internal List<string> RequaredTags { get; set; }

        internal List<ISendInterceptor> Interceptors { get; set; }

        internal bool IsNeedCheckSubscriptions { get; set; }


        public NotifyRequest(INotifySource notifySource, INotifyAction action, string objectID, IRecipient recipient)
        {
            if (notifySource == null) throw new ArgumentNullException("notifySource");
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            Properties = new Hashtable();
            Arguments = new List<ITagValue>();
            RequaredTags = new List<string>();
            Interceptors = new List<ISendInterceptor>();

            NotifySource = notifySource;
            Recipient = recipient;
            NotifyAction = action;
            ObjectID = objectID;

            IsNeedCheckSubscriptions = true;
        }

        internal bool Intercept(InterceptorPlace place)
        {
            var result = false;
            foreach (var interceptor in Interceptors)
            {
                if ((interceptor.PreventPlace & place) == place)
                {
                    try
                    {
                        if (interceptor.PreventSend(this, place))
                        {
                            result = true;
                        }
                    }
                    catch (Exception err)
                    {
                        LogManager.GetLogger("ASC.Notify").ErrorFormat("{0} {1} {2}: {3}", interceptor.Name, NotifyAction, Recipient, err);
                    }
                }
            }
            return result;
        }

        internal IPattern GetSenderPattern(string senderName)
        {
            if (SenderNames == null || Patterns == null ||
                SenderNames.Length == 0 || Patterns.Length == 0 ||
                SenderNames.Length != Patterns.Length)
            {
                return null;
            }

            int index = Array.IndexOf(SenderNames, senderName);
            if (index < 0)
            {
                throw new ApplicationException(String.Format("Sender with tag {0} dnot found", senderName));
            }
            return Patterns[index];
        }

        internal NotifyRequest Split(IRecipient recipient)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            var newRequest = new NotifyRequest(NotifySource, NotifyAction, ObjectID, recipient);
            newRequest.SenderNames = SenderNames;
            newRequest.Patterns = Patterns;
            newRequest.Arguments = new List<ITagValue>(Arguments);
            newRequest.RequaredTags = RequaredTags;
            newRequest.CurrentSender = CurrentSender;
            newRequest.CurrentMessage = CurrentMessage;
            newRequest.Interceptors.AddRange(Interceptors);
            return newRequest;
        }

        internal NoticeMessage CreateMessage(IDirectRecipient recipient)
        {
            return new NoticeMessage(recipient, NotifyAction, ObjectID);
        }
    }
}