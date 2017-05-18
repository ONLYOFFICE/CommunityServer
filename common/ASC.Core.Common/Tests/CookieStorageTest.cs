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


#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System;
    using ASC.Core.Security.Authentication;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CookieStorageTest
    {
        [TestMethod]
        public void Validate()
        {
            var t1 = 1;
            var id1 = Guid.NewGuid();
            var login1 = "l1";
            var pwd1 = "p1";
            var it1 = 1;
            var expire1 = DateTime.UtcNow;
            var iu1 = 1;

            var cookie = CookieStorage.EncryptCookie(t1, id1, login1, pwd1, it1, expire1, iu1);

            int t2;
            Guid id2;
            string login2;
            string pwd2;
            int it2;
            DateTime expire2;
            int iu2;

            CookieStorage.DecryptCookie(cookie, out t2, out id2, out login2, out pwd2, out it2, out expire2, out iu2);

            Assert.AreEqual(t1, t2);
            Assert.AreEqual(id1, id2);
            Assert.AreEqual(login1, login2);
            Assert.AreEqual(pwd1, pwd2);
            Assert.AreEqual(it1, it2);
            Assert.AreEqual(expire1, expire2);
            Assert.AreEqual(iu1, iu2);
        }
    }
}
#endif
