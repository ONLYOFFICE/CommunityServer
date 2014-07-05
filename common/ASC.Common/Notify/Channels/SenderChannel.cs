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
