/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;

using ASC.Core.Common.Notify.FireBase.Dao;
using ASC.Core.Notify.Senders;
using ASC.Core.Tenants;
using ASC.Notify.Messages;
using ASC.Notify.Sinks;

using Newtonsoft.Json;

namespace ASC.Core.Notify
{
    class PushSenderSink : Sink
    {
        private readonly string senderName = ASC.Core.Configuration.Constants.NotifyPushSenderSysName;
        private readonly INotifySender sender;

        public PushSenderSink(INotifySender sender)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            this.sender = sender;
        }


        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            try
            {
                var result = SendResult.OK;
                var user = CoreContext.UserManager.GetUsers(new Guid(message.Recipient.ID));
                var username = user.UserName;
                if (string.IsNullOrEmpty(username))
                {
                    result = SendResult.IncorrectRecipient;
                }
                else
                {
                    var fromTag = message.Arguments.FirstOrDefault(x => x.Tag == "MessageFrom");
                    var productID = message.Arguments.FirstOrDefault(x => x.Tag == "__ProductID");
                    var originalUrl = message.Arguments.FirstOrDefault(x => x.Tag == "DocumentURL");

                    var folderId = message.Arguments.FirstOrDefault(x => x.Tag == "FolderId");
                    var rootFolderId = message.Arguments.FirstOrDefault(x => x.Tag == "FolderParentId");
                    var rootFolderType = message.Arguments.FirstOrDefault(x => x.Tag == "FolderRootFolderType");
                  
                    NotifyData notifyData = new NotifyData()
                    {
                        Email = user.Email,
                        Portal = CoreContext.TenantManager.GetCurrentTenant().TenantDomain,
                        OriginalUrl = originalUrl != null && originalUrl.Value != null ? originalUrl.Value.ToString() : "",
                        Folder = new NotifyFolderData
                        {
                            Id = folderId != null && folderId.Value != null ? folderId.Value.ToString() : "",
                            ParentId = rootFolderId != null && rootFolderId.Value != null ? rootFolderId.Value.ToString() : "",
                            RootFolderType = rootFolderType != null && rootFolderType.Value != null ? (int)rootFolderType.Value : 0
                        },
                    };

                    var msg = (NoticeMessage)message;

                    if (msg.ObjectID.StartsWith("file_"))
                    {
                        var documentTitle = message.Arguments.FirstOrDefault(x => x.Tag == "DocumentTitle");
                        var documentExtension = message.Arguments.FirstOrDefault(x => x.Tag == "DocumentExtension");

                        notifyData.File = new NotifyFileData()
                        {
                            Id = msg.ObjectID.Substring(5),
                            Title = documentTitle != null && documentTitle.Value != null ? documentTitle.Value.ToString() : "",
                            Extension = documentExtension != null && documentExtension.Value != null ? documentExtension.Value.ToString() : ""

                        };
                    }
                    var jsonNotifyData = JsonConvert.SerializeObject(notifyData);
                    var tenant = CoreContext.TenantManager.GetCurrentTenant(false);

                    var m = new NotifyMessage
                    {
                        To = username,
                        Subject = fromTag != null && fromTag.Value != null ? fromTag.Value.ToString() : message.Subject,
                        ContentType = message.ContentType,
                        Content = message.Body,
                        Sender = senderName,
                        CreationDate = DateTime.UtcNow,
                        ProductID = fromTag != null && fromTag.Value != null ? productID.Value.ToString() : null,
                        ObjectID = msg.ObjectID,
                        Tenant = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId,
                        Data = jsonNotifyData
                    };

                    sender.Send(m);
                }
                return new SendResponse(message, senderName, result);
            }
            catch (Exception ex)
            {
                return new SendResponse(message, senderName, ex);
            }
        }
    }
}