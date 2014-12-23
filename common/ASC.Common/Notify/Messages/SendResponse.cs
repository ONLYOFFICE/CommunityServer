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

using System;
using ASC.Notify.Model;
using ASC.Notify.Recipients;

namespace ASC.Notify.Messages
{
    [Serializable]
    public class SendResponse
    {
        public SendResponse()
        {
            Result = SendResult.OK;
        }

        public SendResponse(INotifyAction action, IRecipient recipient, Exception exc)
        {
            Result = SendResult.Impossible;
            Exception = exc;
            Recipient = recipient;
            NotifyAction = action;
        }

        public SendResponse(INotifyAction action, string senderName, IRecipient recipient, Exception exc)
        {
            Result = SendResult.Impossible;
            SenderName = senderName;
            Exception = exc;
            Recipient = recipient;
            NotifyAction = action;
        }

        public SendResponse(INotifyAction action, string senderName, IRecipient recipient, SendResult sendResult)
        {
            SenderName = senderName;
            Recipient = recipient;
            Result = sendResult;
            NotifyAction = action;
        }

        public SendResponse(INoticeMessage message, string sender, SendResult result)
        {
            NoticeMessage = message;
            SenderName = sender;
            Result = result;
            if (message != null)
            {
                Recipient = message.Recipient;
                NotifyAction = message.Action;
            }
        }

        public SendResponse(INoticeMessage message, string sender, Exception exc)
        {
            NoticeMessage = message;
            SenderName = sender;
            Result = SendResult.Impossible;
            Exception = exc;
            if (message != null)
            {
                Recipient = message.Recipient;
                NotifyAction = message.Action;
            }
        }

        public INoticeMessage NoticeMessage { get; internal set; }

        public INotifyAction NotifyAction { get; internal set; }

        public SendResult Result { get; set; }

        public Exception Exception { get; set; }

        public string SenderName { get; internal set; }

        public IRecipient Recipient { get; internal set; }
    }
}