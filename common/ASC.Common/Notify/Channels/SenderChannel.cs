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
using ASC.Notify.Messages;
using ASC.Notify.Sinks;

namespace ASC.Notify.Channels
{
    public class SenderChannel : ISenderChannel
    {
        private ISink firstSink;
        private ISink senderSink;
        private Context context;


        public string SenderName
        {
            get;
            private set;
        }


        public SenderChannel(Context context, string senderName, ISink decorateSink, ISink senderSink)
        {
            if (senderName == null) throw new ArgumentNullException("senderName");
            if (context == null) throw new ArgumentNullException("context");
            if (senderSink == null) throw new ApplicationException(string.Format("channel with tag {0} not created sender sink", senderName));

            this.context = context;
            this.SenderName = senderName;
            this.firstSink = decorateSink;
            this.senderSink = senderSink;
            
            var dispatcherSink = new DispatchSink(SenderName, this.context.DispatchEngine);
            this.firstSink = AddSink(firstSink, dispatcherSink);
        }

        public void SendAsync(INoticeMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            firstSink.ProcessMessageAsync(message);
        }

        public SendResponse DirectSend(INoticeMessage message)
        {
            return senderSink.ProcessMessage(message);
        }

        
        private ISink AddSink(ISink firstSink, ISink addedSink)
        {
            if (firstSink == null) return addedSink;
            if (addedSink == null) return firstSink;

            var current = firstSink;
            while (current.NextSink != null)
            {
                current = current.NextSink;
            }
            current.NextSink = addedSink;
            return firstSink;
        }
    }
}
