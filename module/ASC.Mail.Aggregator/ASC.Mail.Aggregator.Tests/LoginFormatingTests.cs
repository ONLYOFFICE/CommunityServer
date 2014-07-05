/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests
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
