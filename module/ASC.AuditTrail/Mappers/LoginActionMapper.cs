/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System.Collections.Generic;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    internal class LoginActionsMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    {MessageAction.LoginSuccess, new MessageMaps {ActionTextResourceName = "LoginSuccess"}},
                    {MessageAction.LoginSuccessViaSocialAccount, new MessageMaps {ActionTextResourceName = "LoginSuccessSocialAccount"}},
                    {MessageAction.LoginSuccessViaSocialApp, new MessageMaps {ActionTextResourceName = "LoginSuccessSocialApp"}},
                    {MessageAction.LoginSuccessViaSms, new MessageMaps {ActionTextResourceName = "LoginSuccessViaSms"}},
                    {MessageAction.LoginSuccessViaApi, new MessageMaps {ActionTextResourceName = "LoginSuccessViaApi"}},
                    {MessageAction.LoginSuccessViaApiSms, new MessageMaps {ActionTextResourceName = "LoginSuccessViaApiSms"}},
                    {MessageAction.LoginSuccessViaApiTfa, new MessageMaps {ActionTextResourceName = "LoginSuccessViaApiTfa"}},
                    {MessageAction.LoginSuccessViaApiSocialAccount, new MessageMaps {ActionTextResourceName = "LoginSuccessViaSocialAccount"}},
                    {MessageAction.LoginSuccessViaSSO, new MessageMaps {ActionTextResourceName = "LoginSuccessViaSSO"}},
                    {MessageAction.LoginSuccesViaTfaApp, new MessageMaps {ActionTextResourceName = "LoginSuccesViaTfaApp"}},
                    {MessageAction.LoginFailInvalidCombination, new MessageMaps {ActionTextResourceName = "LoginFailInvalidCombination"}},
                    {MessageAction.LoginFailSocialAccountNotFound, new MessageMaps {ActionTextResourceName = "LoginFailSocialAccountNotFound"}},
                    {MessageAction.LoginFailDisabledProfile, new MessageMaps {ActionTextResourceName = "LoginFailDisabledProfile"}},
                    {MessageAction.LoginFail, new MessageMaps {ActionTextResourceName = "LoginFail"}},
                    {MessageAction.LoginFailViaSms, new MessageMaps {ActionTextResourceName = "LoginFailViaSms"}},
                    {MessageAction.LoginFailViaApi, new MessageMaps {ActionTextResourceName = "LoginFailViaApi"}},
                    {MessageAction.LoginFailViaApiSms, new MessageMaps {ActionTextResourceName = "LoginFailViaApiSms"}},
                    {MessageAction.LoginFailViaApiTfa, new MessageMaps {ActionTextResourceName = "LoginFailViaApiTfa"}},
                    {MessageAction.LoginFailViaApiSocialAccount, new MessageMaps {ActionTextResourceName = "LoginFailViaApiSocialAccount"}},
                    {MessageAction.LoginFailViaTfaApp, new MessageMaps {ActionTextResourceName = "LoginFailViaTfaApp"}},
                    {MessageAction.LoginFailIpSecurity, new MessageMaps {ActionTextResourceName = "LoginFailIpSecurity"}},
                    {MessageAction.LoginFailViaSSO, new MessageMaps {ActionTextResourceName = "LoginFailViaSSO"}},
                    {MessageAction.LoginFailBruteForce, new MessageMaps {ActionTextResourceName = "LoginFailBruteForce"}},
                    {MessageAction.LoginFailRecaptcha, new MessageMaps {ActionTextResourceName = "LoginFailRecaptcha"}},
                    {MessageAction.Logout, new MessageMaps {ActionTextResourceName = "Logout"}},
                    {MessageAction.SessionStarted, new MessageMaps {ActionTextResourceName = "SessionStarted"}},
                    {MessageAction.SessionCompleted, new MessageMaps {ActionTextResourceName = "SessionCompleted"}}
                };
        }
    }
}