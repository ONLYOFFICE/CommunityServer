/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using DotNetOpenAuth.OAuth2;

namespace ASC.Thrdparty.Web.Google
{
    public class GoogleUserInfo
    {
        public String id { get; set; }
        public String email { get; set; }
        public Boolean verified_email { get; set; }
        public String name { get; set; }
        public String given_name { get; set; }
        public String family_name { get; set; }
        public String link { get; set; }
        public String picture { get; set; }
        public String gender { get; set; }
        public String locale { get; set; }
    }

    public class AuthorizationTracker : IClientAuthorizationTracker
    {
        private readonly List<string> _scope;

        public AuthorizationTracker(List<string> scope)
        {
            _scope = scope;
        }

        public IAuthorizationState GetAuthorizationState(
          Uri callback_url, string client_state)
        {
            return new AuthorizationState(_scope)
            {
                Callback = callback_url
            };
        }
    }
}