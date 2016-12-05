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


using ASC.Core;
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
        private readonly ILog log = LogManager.GetLogger(typeof(AuthHandler));
        private readonly IDictionary<string, AuthData> authData = new Dictionary<string, AuthData>();

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
                        bool isAuth = false;
                        User user = context.UserManager.GetUser(new Jid(userName, stream.Domain, null));
                        if (user != null)
                        {
                            if (user.Sid != null)
                            {
                                if (!user.Sid.StartsWith("l"))
                                {
                                    var storage = new DbLdapSettingsStore();
                                    storage.GetLdapSettings(stream.Domain);
                                    ILdapHelper ldapHelper = !WorkContext.IsMono ?
                                        (ILdapHelper)new SystemLdapHelper() : new NovellLdapHelper();
                                    var accountName = ldapHelper.GetAccountNameBySid(user.Sid, storage.Authentication,
                                        storage.Login, storage.Password, storage.Server, storage.PortNumber,
                                        storage.UserDN, storage.LoginAttribute, storage.StartTls);
                                    if (accountName != null && ldapHelper.CheckCredentials(accountName,
                                        password, storage.Server, storage.PortNumber, storage.Login, storage.StartTls))
                                    {
                                        // ldap user
                                        isAuth = true;
                                    }
                                }
                            }
                            else if (user.Password == password)
                            {
                                // usual user
                                isAuth = true;
                            }
                        }
                        if (isAuth)
                        {
                            log.DebugFormat("User {0} authorized, Domain = {1}", userName, stream.Domain);
                            context.Sender.ResetStream(stream);
                            stream.Authenticate(userName);
                            context.Sender.SendTo(stream, new Success());
                        }
                        else
                        {
                            log.DebugFormat("User {0} not authorized, Domain = {1}", userName, stream.Domain);
                            context.Sender.SendToAndClose(stream, XmppFailureError.NotAuthorized);
                        }
                    }
                    else
                    {
                        context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
                    }
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
                var success = ProcessStep2(stream, context);
                context.Sender.SendTo(stream, success);
            }
            else
            {
                context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
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

        private Success ProcessStep2(XmppStream stream, XmppHandlerContext ctx)
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

            public void DoStep()
            {
                Step++;
            }
        }
    }
}