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


using System;
using NUnit.Framework;
using ASC.ActiveDirectory.Base.Data;
using ASC.Core.Users;

namespace ASC.ActiveDirectory.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private const string LDAP_DOMAIN = "avsmedia.net";

        private static readonly UserInfo BaseUserInfo = new UserInfo
        {
            ID = Guid.NewGuid(),
            UserName = "Alexey.Safronov",
            Email = "Alexey.Safronov@onlyoffice.com",
        };

        private static readonly LdapLogin LdapLogin = new LdapLogin("Alexey.Safronov", "onlyoffice.com");

        public void LoginEmptyTest()
        {
            Assert.Equals(LdapUtils.IsLoginAccepted(null, BaseUserInfo, LDAP_DOMAIN), false);
        }

        public void UserEmptyTest()
        {
            Assert.Equals(LdapUtils.IsLoginAccepted(LdapLogin, null, LDAP_DOMAIN), false);
            Assert.Equals(LdapUtils.IsLoginAccepted(LdapLogin, Constants.LostUser, LDAP_DOMAIN), false);
            Assert.Equals(LdapUtils.IsLoginAccepted(LdapLogin, new UserInfo
            {
                ID = Guid.NewGuid(),
                Email = "Alexey.Safronov@onlyoffice.com",
            }, LDAP_DOMAIN), false);
            Assert.Equals(LdapUtils.IsLoginAccepted(LdapLogin, new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = "Alexey.Safronov",
            }, LDAP_DOMAIN), false);
        }
    }
}
