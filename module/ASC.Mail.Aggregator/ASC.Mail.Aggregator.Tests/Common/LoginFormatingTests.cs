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


using System.Net.Mail;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common
{
    [TestFixture]
    public class LoginFormatingTests
    {
        [Test]
        public void TestForAllName()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "localpart@domain.ru");
            Assert.AreEqual("%EMAILADDRESS%", result);
        }

        [Test]
        public void TestForAllNameCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("LocaLpart@domain.ru"),
                                                             "lOcalPart@domain.ru");
            Assert.AreEqual("%EMAILADDRESS%", result);
        }

        [Test]
        public void TestForLocalPart()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "localpart");
            Assert.AreEqual("%EMAILLOCALPART%", result);
        }

        [Test]
        public void TestForLocalPartCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("LocaLpart@domain.ru"),
                                                             "lOcalPart");
            Assert.AreEqual("%EMAILLOCALPART%", result);
        }

        [Test]
        public void TestForError()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "asdasd");
            Assert.AreEqual("", result);
        }

        [Test]
        public void TestForErrorCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localPart@domain.ru"),
                                                             "asDasD");
            Assert.AreEqual("", result);
        }

        [Test]
        public void TestForHost()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "domain");
            Assert.AreEqual("%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForHostCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@doMain.ru"),
                                                             "DomAin");
            Assert.AreEqual("%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForDomain()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "domain.ru");
            Assert.AreEqual("%EMAILDOMAIN%", result);
        }

        [Test]
        public void TestForDomainCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domAin.rU"),
                                                             "doMain.Ru");
            Assert.AreEqual("%EMAILDOMAIN%", result);
        }

        [Test]
        public void TestForComplexFormat()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("localpart@domain.ru"),
                                                             "localpart.domain");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForComplexFormatCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("LocalPart@doMain.ru"),
                                                             "lOcalpaRt.dOmaIn");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForEqualLocalpartAndDomainName()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("equal@equal.ru"), "equal.equal");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForEqualLocalpartAndDomainNameCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("eQuaL@eqUAl.ru"), "EquaL.EQual");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForMultiplePointInDomain()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("equal@notequal.co.uk"), "equal.notequal");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForMultiplePointInDomainCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("EquaL@notEqual.co.uk"), "eQUal.nOtequal");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForMultiplePointInDomain2()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("equal@notequal.mail.pala.jp"), "equal.notequal.mail.pala");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForMultiplePointInDomain2Case()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("eqUal@noTequal.mail.pAlA.jp"), "eQual.notequaL.mail.PaLa");
            Assert.AreEqual("%EMAILLOCALPART%.%EMAILHOSTNAME%", result);
        }

        [Test]
        public void TestForGmailAnalgues()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("equal@equal.ru"), "recent:equal@equal.ru");
            Assert.AreEqual("recent:%EMAILADDRESS%", result);
        }

        [Test]
        public void TestForGmailAnalguesCase()
        {
            var result = MailBoxManager.GetLoginFormatFrom(new MailAddress("equAl@equaL.ru"), "recent:eqUal@eQual.ru");
            Assert.AreEqual("recent:%EMAILADDRESS%", result);
        }
    }
}
