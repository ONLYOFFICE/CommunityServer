/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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