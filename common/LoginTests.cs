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
