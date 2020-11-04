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
using System.Text;
using ASC.Notify.Messages;

namespace ASC.Notify
{
    public class NotifyResult
    {
        public SendResult Result { get; internal set; }

        public List<SendResponse> Responses { get; set; }


        internal NotifyResult(SendResult result, List<SendResponse> responses)
        {
            Result = result;
            Responses = responses ?? new List<SendResponse>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SendResult: {0} whith {1} sub-results", Result, Responses.Count);
            foreach (SendResponse responce in Responses)
            {
                string recipient = "<recipient:nomessage>";
                string error = "";
                if (responce.NoticeMessage != null)
                {
                    if (responce.NoticeMessage.Recipient != null)
                    {
                        recipient = responce.NoticeMessage.Recipient.Addresses.Length > 0 ?
                            responce.NoticeMessage.Recipient.Addresses[0] :
                            "<no-address>";
                    }
                    else
                    {
                        recipient = "<null-address>";
                    }
                }
                if (responce.Exception != null) error = responce.Exception.Message;
                sb.AppendLine();
                sb.AppendFormat("   {3}->{0}({1})={2} {4}", recipient, responce.SenderName, responce.Result, responce.NotifyAction.ID, error);
            }
            return sb.ToString();
        }
    }
}