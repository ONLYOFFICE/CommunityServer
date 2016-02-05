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
using ActiveUp.Net.Mail;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common
{
    [TestFixture]
    class MailerParserTests
    {
        [Test]
        public void ShouldDecodeWithTextInTheMiddleRfc2047()
        {
            const string control_base64_string =
                "=?UTF-8?B?0JjQstCw0L0g0JjQstCw0L3QvtCy0LjRhyDQmNCy0LDQvdC+0LIg0KHQsNC80YvQuSDQu9GD0Yc=?=" +
                "=?UTF-8?B?0YjQuNC5INGA0LDQsdC+0YLQvdC40Log0LPQvtC00LA=?= <profi.troll.4test@gmail.com>, " +
                "=?UTF-8?B?0KTQtdC00L7RgCDQpNC10LTQvtGA0L7QstC40Ycg0JXQvNC10LvRjNGP0L3QuNC90LrQviDQk9C70LA=?=" +
                "=?UTF-8?B?0LLQvdGL0Lkg0YfQtdC70L7QstC10Log0L3QsCDQn9C70LDQvdC10YLQtSDQt9C10LzQu9GP?= <asc.4test@gmail.com>, " +
                "test <profi.troll@aol.com>, " +
                "=?UTF-8?B?0KHQtdC80LXQvSDQodC10LzQtdC90L7QstC40Ycg0KjQv9Cw0Log0J/RgNC+0YHRgtC+INGF0L7RgNC+?=" +
                "=?UTF-8?B?0YjQuNC5INGH0LXQu9C+0LLQtdC6?= <profi.troll@hotmail.com>";

            const string expected =
                "Иван Иванович Иванов Самый лучший работник года <profi.troll.4test@gmail.com>, " +
                "Федор Федорович Емельянинко Главный человек на Планете земля <asc.4test@gmail.com>, " +
                "test <profi.troll@aol.com>, " +
                "Семен Семенович Шпак Просто хороший человек <profi.troll@hotmail.com>";

            var res = Codec.RFC2047Decode(control_base64_string);

            Assert.AreNotEqual(String.Empty, res);

            Assert.AreEqual(expected, res);
        }

        [Test]
        public void ShouldDecodeWithSamePartsUtf8Rfc2047()
        {
            const string control_base64_string =
                "=?UTF-8?B?0JjQstCw0L0g0JjQstCw0L3QvtCy0LjRhyDQmNCy0LDQvdC+0LIg0KHQsNC80YvQuSDQu9GD0Yc=?= " +
                "=?UTF-8?B?0YjQuNC5INGA0LDQsdC+0YLQvdC40Log0LPQvtC00LA=?=";

            const string expected =
                "Иван Иванович Иванов Самый лучший работник года";

            var res = Codec.RFC2047Decode(control_base64_string);

            Assert.AreNotEqual(String.Empty, res);

            Assert.AreEqual(expected, res);
        }

        [Test]
        public void ShouldDecodeWithSamePartsKoi8rRfc2047()
        {
            const string control_base64_string =
                "=?KOI8-R?B?9yD3wdsgwcvLwdXO1CDX2dDPzM7FziDXyM/EIA==?=" +
                "=?KOI8-R?B?0yDV09TSz8rT1NfBIFdpbmRvd3Mg3sXSxdog0NLJzM/Wxc4=?=" +
                "=?KOI8-R?B?ycUgQ2hyb21l?=";

            const string expected =
                "В Ваш аккаунт выполнен вход с устройства Windows через приложение Chrome";

            var res = Codec.RFC2047Decode(control_base64_string);

            Assert.AreNotEqual(String.Empty, res);

            Assert.AreEqual(expected, res);
        }

        [Test]
        public void ShouldDecodeStringWithLanguageCodeRfc5987()
        {
            const string input = "iso-8859-1'en'%A3%20rates";

            var result = Codec.RFC5987Decode(input);

            Assert.AreEqual("£ rates", result);
        }

        [Test]
        public void ShouldDecodeStringWithoutLanguageCodeRfc5987()
        {
            const string input = "iso-8859-1''%A3%20rates";

            var result = Codec.RFC5987Decode(input);

            Assert.AreEqual("£ rates", result);
        }

        [Test]
        public void ShouldHandleUtf8Rfc5987()
        {
            const string input = "UTF-8''%c2%a3%20and%20%e2%82%ac%20rates";

            var result = Codec.RFC5987Decode(input);

            Assert.AreEqual("£ and € rates", result);
        }

        [Test]
        public void ShouldDecodeUtf8FilenameRfc5987()
        {
            const string input = "UTF-8''foo-%c3%a4.html";

            var result = Codec.RFC5987Decode(input);

            Assert.AreEqual("foo-ä.html", result);
        }

        [Test]
        public void ShouldDecodeUtf8CyrillicFilenameRfc5987()
        {
            const string input = "utf-8''%D0%9F%D1%80%D0%B5%D0%B4%D0%BB%D0%BE%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5%20Danhaus%20KUBIK.pdf";

            var result = Codec.RFC5987Decode(input);

            Assert.AreEqual("Предложение Danhaus KUBIK.pdf", result);
        }
    }
}
