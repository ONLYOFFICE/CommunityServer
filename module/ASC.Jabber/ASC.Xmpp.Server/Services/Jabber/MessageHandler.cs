/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.Script.Serialization;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Thrdparty.Configuration;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Streams;
using log4net;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Message))]
	class MessageHandler : XmppStanzaHandler
	{
        private DbPushStore pushStore;

        private static readonly ILog log = LogManager.GetLogger(typeof(XmppHandlerManager));
        
		public override void HandleMessage(XmppStream stream, Message message, XmppHandlerContext context)
		{
            
			if (!message.HasTo || message.To.IsServer)
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(message));
				return;
			}

			var sessions = context.SessionManager.GetBareJidSessions(message.To);
            if (0 < sessions.Count)
            {
                foreach(var s in sessions)
                {
                    try
                    {
                        context.Sender.SendTo(s, message);
                    }
                    catch
                    {
                        context.Sender.SendToAndClose(s.Stream, message);
                    }
                }
            }
            else
            {
                pushStore = new DbPushStore();
                var properties = new Dictionary<string, string>(1);
                properties.Add("connectionStringName", "default");
                pushStore.Configure(properties);

                if (message.HasTag("active"))
                {
                    var fromFullName = message.HasAttribute("username") ? 
                                        message.GetAttribute("username") : message.From.ToString();

                    var tenantId = message.HasAttribute("tenantid") ?
                                        Convert.ToInt32(message.GetAttribute("tenantid"), 16) : -1;
                        
                    var userPushList = new List<UserPushInfo>();
                    userPushList = pushStore.GetUserEndpoint(message.To.ToString().Split(new char[] { '@' })[0]);

                    var firebaseAuthorization = "";
                    try
                    {
                        CallContext.SetData(TenantManager.CURRENT_TENANT, new Tenant(tenantId, ""));
                        firebaseAuthorization = KeyStorage.Get("firebase_authorization");
                    }
                    catch (Exception exp)
                    {
                        log.DebugFormat("firebaseAuthorizationERROR: {0}", exp);
                    }
                    foreach (var user in userPushList)
                    {
                        try{ 
                            var tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                            tRequest.Method = "post";
                            tRequest.ContentType = "application/json";
                            var data = new
                            {
                                to = user.endpoint,
                                data = new
                                {
                                    msg = message.Body,
                                    fromFullName = fromFullName
                                }    
                            };       
                            var serializer = new JavaScriptSerializer();
                            var json = serializer.Serialize(data);
                            var byteArray = Encoding.UTF8.GetBytes(json);
                            tRequest.Headers.Add(string.Format("Authorization: key={0}", firebaseAuthorization));
                            tRequest.ContentLength = byteArray.Length; 
                            using (var dataStream = tRequest.GetRequestStream())
                            {
                                dataStream.Write(byteArray, 0, byteArray.Length);   
                                using (var tResponse = tRequest.GetResponse())
                                {
                                    using (var dataStreamResponse = tResponse.GetResponseStream())
                                    {
                                        using (var tReader = new StreamReader(dataStreamResponse))
                                        {
                                            var sResponseFromServer = tReader.ReadToEnd();
                                            var str = sResponseFromServer;
                                        }    
                                    }    
                                }    
                            }    
                        }        
                        catch (Exception ex)
                        {
                            var str = ex.Message;
                            log.DebugFormat("PushRequestERROR: {0}", str);
                        }          
                    }
                }
                StoreOffline(message, context.StorageManager.OfflineStorage);
            }
		}

		private void StoreOffline(Message message, IOfflineStore offlineStore)
		{
			if ((message.Type == MessageType.normal || message.Type == MessageType.chat) && !string.IsNullOrEmpty(message.To.User))
			{
				offlineStore.SaveOfflineMessages(message);
			}
		}
	}
}