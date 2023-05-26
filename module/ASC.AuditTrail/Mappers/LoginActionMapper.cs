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


using System.Collections.Generic;

using ASC.AuditTrail.Types;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public class LoginActionsMapper : IProductActionMapper
    {
        public ProductType Product { get; }
        public List<IModuleActionMapper> Mappers { get; }

        public LoginActionsMapper()
        {
            Product = ProductType.Login;

            Mappers = new List<IModuleActionMapper>()
            {
                new LoginNoneModuleActionMapper()
            };
        }
    }

    public class LoginNoneModuleActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public LoginNoneModuleActionMapper()
        {
            Module = ModuleType.None;

            Actions = new MessageMapsDictionary()
            {
                MessageAction.LoginSuccess,
                MessageAction.LoginSuccessViaSms,MessageAction.LoginSuccessViaApi,MessageAction.LoginSuccessViaApiSms,
                MessageAction.LoginSuccessViaApiTfa,MessageAction.LoginSuccessViaApiSocialAccount,MessageAction.LoginSuccessViaSSO,
                MessageAction.LoginSuccesViaTfaApp,MessageAction.LoginFailInvalidCombination,MessageAction.LoginFailSocialAccountNotFound,
                MessageAction.LoginFailDisabledProfile, MessageAction.LoginFail,MessageAction.LoginFailViaSms,MessageAction.LoginFailViaApi,
                MessageAction.LoginFailViaApiSms,MessageAction.LoginFailViaApiTfa,MessageAction.LoginFailViaApiSocialAccount,
                MessageAction.LoginFailViaTfaApp,MessageAction.LoginFailIpSecurity,MessageAction.LoginFailViaSSO,MessageAction.LoginFailBruteForce,
                MessageAction.LoginFailRecaptcha,MessageAction.Logout,MessageAction.SessionStarted,MessageAction.SessionCompleted
            };

            Actions.Add(MessageAction.LoginSuccessViaSocialAccount, new MessageMaps("LoginSuccessSocialAccount"));
            Actions.Add(MessageAction.LoginSuccessViaSocialApp, new MessageMaps("LoginSuccessSocialApp"));
        }
    }
}