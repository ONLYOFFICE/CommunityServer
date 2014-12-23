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

using ASC.Xmpp.Core.authorization.DigestMD5;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.sasl;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;
using ASC.Xmpp.Server.Utils;
using log4net;
using System;
using System.Collections.Generic;

namespace ASC.Xmpp.Server.Authorization
{
    [XmppHandler(typeof(Auth))]
    [XmppHandler(typeof(Response))]
    [XmppHandler(typeof(Abort))]
    class AuthHandler : XmppStreamHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthHandler));
        private IDictionary<string, AuthData> authData = new Dictionary<string, AuthData>();

        public override void StreamEndHandle(XmppStream stream, ICollection<Node> notSendedBuffer, XmppHandlerContext context)
        {
            lock (authData)
            {
                authData.Remove(stream.Id);
            }
        }

        public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
        {
            if (stream.Authenticated) return;

            if (element is Auth) ProcessAuth(stream, (Auth)element, context);
            if (element is Response) ProcessResponse(stream, (Response)element, context);
            if (element is Abort) ProcessAbort(stream, (Abort)element, context);
        }

        private void ProcessAuth(XmppStream stream, Auth auth, XmppHandlerContext context)
        {
            AuthData authStep;
            lock (authData)
            {
                authData.TryGetValue(stream.Id, out authStep);
            }

            if (auth.MechanismType == MechanismType.DIGEST_MD5)
            {
                if (authStep != null)
                {
                    context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
                }
                else
                {
                    lock (authData)
                    {
                        authData[stream.Id] = new AuthData();
                    }
                    var challenge = GetChallenge(stream.Domain);
                    context.Sender.SendTo(stream, challenge);
                }
            }
            else if (auth.MechanismType == MechanismType.PLAIN)
            {
                if (auth.TextBase64 == null)
                {
                    context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
                }
                else
                {
                    string[] array = auth.TextBase64.Split('\0');
                    if (array.Length == 3)
                    {
                        string userName = array[1];
                        string password = array[2];
                        var storage = new DbLdapSettingsStore();
                        storage.GetLdapSettings(stream.Domain);
                        User user = context.UserManager.GetUser(new Jid(userName, stream.Domain, null));
                        if (user != null)
                        {
                            if (user.Sid != null)
                            {
                                var accountName = storage.getAccountNameBySid(user.Sid);
                                if (accountName != null && storage.CheckCredentials(accountName, password))
                                {
                                    // ldap user
                                    lock (authData)
                                    {
                                        authData[stream.Id] = new AuthData(true);
                                        authData[stream.Id].UserName = userName;
                                        authData[stream.Id].IsAuth = true;
                                    }
                                }
                            }
                            else if (user.Password == password)
                            {
                                // usual user
                                lock (authData)
                                {
                                    authData[stream.Id] = new AuthData(true);
                                    authData[stream.Id].UserName = userName;
                                    authData[stream.Id].IsAuth = true;
                                }
                            }
                        }
                    }
                    lock (authData)
                    {
                        if (!authData.ContainsKey(stream.Id))
                        {
                            authData[stream.Id] = new AuthData(true);
                        }
                    }
                    context.Sender.SendTo(stream, new Challenge());
                }
            }
            else
            {
                context.Sender.SendToAndClose(stream, XmppFailureError.InvalidMechanism);
            }
        }

        private void ProcessResponse(XmppStream stream, Response response, XmppHandlerContext context)
        {
            AuthData authStep;
            lock (authData)
            {
                authData.TryGetValue(stream.Id, out authStep);
            }

            if (authStep == null)
            {
                context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
                return;
            }
            if (!authStep.IsPlain)
            {
                if (authStep.Step == AuthStep.Step1)
                {
                    var challenge = ProcessStep1(stream, response, context);
                    if (challenge != null)
                    {
                        context.Sender.SendTo(stream, challenge);
                        authStep.DoStep();
                    }
                    else
                    {
                        context.Sender.SendToAndClose(stream, XmppFailureError.NotAuthorized);
                    }
                }
                else if (authStep.Step == AuthStep.Step2)
                {
                    var success = ProcessStep2(stream, response, context);
                    context.Sender.SendTo(stream, success);
                }
                else
                {
                    context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
                }
            }
            else
            {
                if (authStep.IsAuth)
                {
                    lock (authData)
                    {
                        stream.Authenticate(authData[stream.Id].UserName);
                        authData.Remove(stream.Id);
                    }
                    log.DebugFormat("User authorized");
                    context.Sender.ResetStream(stream);
                    context.Sender.SendTo(stream, new Success());
                }
                else
                {
                    log.DebugFormat("User not authorized");
                    context.Sender.SendTo(stream, new Failure(FailureCondition.not_authorized));
                }
            }
        }

        private void ProcessAbort(XmppStream stream, Abort abort, XmppHandlerContext context)
        {
            context.Sender.SendToAndClose(stream, XmppFailureError.Aborted);
        }

        private Challenge GetChallenge(string domain)
        {
            var challenge = new Challenge();
            challenge.TextBase64 = string.Format("realm=\"{0}\",nonce=\"{1}\",qop=\"auth\",charset=utf-8,algorithm=md5-sess", domain, UniqueId.CreateNewId());
            return challenge;
        }

        private Challenge GetPlainChallenge(string domain)
        {
            var challenge = new Challenge();
            return challenge;
        }

        private Challenge ProcessStep1(XmppStream stream, Response response, XmppHandlerContext ctx)
        {
            var step = new Step2(response.TextBase64);
            var userName = step.Username;
            var user = ctx.UserManager.GetUser(new Jid(userName, stream.Domain, null));

            log.DebugFormat("User {0} {1}. Realm={2}", userName, user == null ? "not found" : user.ToString(), step.Realm);

            if (user != null && string.Compare(stream.Domain, step.Realm, StringComparison.OrdinalIgnoreCase) == 0 && user.Sid == null)
            {
                if (step.Authorize(userName, user.Password))
                {
                    log.DebugFormat("User authorized");
                    lock (authData)
                    {
                        authData[stream.Id].UserName = userName;
                    }
                    var challenge = new Challenge();
                    challenge.TextBase64 = string.Format("rspauth={0}", step.GenerateResponse(userName, user.Password, string.Empty));
                    return challenge;
                }
                else
                {
                    log.DebugFormat("User not authorized");
                }
            }
            return null;
        }

        private Success ProcessStep2(XmppStream stream, Response response, XmppHandlerContext ctx)
        {
            lock (authData)
            {
                stream.Authenticate(authData[stream.Id].UserName);
                authData.Remove(stream.Id);
            }
            ctx.Sender.ResetStream(stream);
            return new Success();
        }

        private enum AuthStep
        {
            Step1,
            Step2
        }

        private class AuthData
        {
            public AuthData()
            {
            }

            public AuthData(bool isPlain)
            {
                IsPlain = isPlain;
            }

            public string UserName
            {
                get;
                set;
            }

            public AuthStep Step
            {
                get;
                private set;
            }

            public bool IsPlain
            {
                get;
                set; 
            }

            public bool IsAuth 
            {
                get; 
                set; 
            }

            public void DoStep()
            {
                Step++;
            }
        }
    }
}