/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.ActiveDirectory.Base.Data;
using ASC.Core.Users;
using NUnit.Framework;

namespace ASC.ActiveDirectory.Tests.Login
{
    [TestFixture]
    public class LoginAcceptedTests
    {
        private const string BASE_LDAP_DOMAIN_MIN = "avsmedia";
        private const string BASE_LDAP_DOMAIN = BASE_LDAP_DOMAIN_MIN + ".net";
        private const string BASE_USERNAME = "Alexey.Safronov";
        private const string BASE_EMAIL = "Alexey.Safronov@onlyoffice.com";
        private const string BASE_LDAP_LOGIN_FORM1 = BASE_LDAP_DOMAIN + "\\" + BASE_USERNAME;
        private const string BASE_LDAP_LOGIN_FORM2 = BASE_LDAP_DOMAIN_MIN + "\\" + BASE_USERNAME;
        private const string BASE_LDAP_LOGIN_FORM3 = BASE_USERNAME + "@" + BASE_LDAP_DOMAIN;
        private const string BASE_LDAP_LOGIN_FORM4 = BASE_USERNAME + "@" + BASE_LDAP_DOMAIN_MIN;

        private static readonly UserInfo BaseUserInfo = new UserInfo
        {
            ID = Guid.NewGuid(),
            UserName = BASE_USERNAME,
            Email = BASE_EMAIL
        };

        private static readonly LdapLogin BaseLdapLogin = new LdapLogin(BASE_USERNAME, BASE_LDAP_DOMAIN);

        [Test]
        public void CheckEmptyLdapLoginTest()
        {
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(null, BaseUserInfo, BASE_LDAP_DOMAIN));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(new LdapLogin("", ""), BaseUserInfo, BASE_LDAP_DOMAIN));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(new LdapLogin(null, null), BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckEmptyUserTest()
        {
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, null, BASE_LDAP_DOMAIN));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, Constants.LostUser, BASE_LDAP_DOMAIN));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, new UserInfo
            {
                ID = Guid.NewGuid(),
                Email = BASE_EMAIL,
            }, BASE_LDAP_DOMAIN));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, new UserInfo
            {
                ID = Guid.NewGuid(),
                UserName = BASE_USERNAME,
            }, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckEmptyDomainTest()
        {
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, BaseUserInfo, null));
            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(BaseLdapLogin, Constants.LostUser, ""));
        }

        [Test]
        public void CheckAllBaseTest()
        {
            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(BaseLdapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseUsernameTest()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_USERNAME);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseUsernameAndBaseDominTest()
        {
            var ldapLogin = new LdapLogin(BASE_USERNAME, BASE_LDAP_DOMAIN);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseEmailTest()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_EMAIL);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseForm1Test()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_LDAP_LOGIN_FORM1);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseForm2Test()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_LDAP_LOGIN_FORM2);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseForm3Test()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_LDAP_LOGIN_FORM3);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckLdapLoginByBaseForm4Test()
        {
            var ldapLogin = LdapLogin.ParseLogin(BASE_LDAP_LOGIN_FORM4);

            Assert.AreEqual(true, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckIncorrectLdapLoginByUsernameTest()
        {
            var ldapLogin = LdapLogin.ParseLogin("Ivan.Ivanov");

            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckIncorrectLdapLoginByEmailTest()
        {
            var ldapLogin = LdapLogin.ParseLogin("Ivan.Ivanov@example.com");

            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }

        [Test]
        public void CheckIncorrectLdapLoginByEmailWithBaseDomainTest()
        {
            var ldapLogin = new LdapLogin("Ivan.Ivanov", BASE_LDAP_DOMAIN);

            Assert.AreEqual(false, LdapUtils.IsLoginAccepted(ldapLogin, BaseUserInfo, BASE_LDAP_DOMAIN));
        }
    }
}
