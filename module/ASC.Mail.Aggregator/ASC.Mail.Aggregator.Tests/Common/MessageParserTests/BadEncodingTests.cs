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


using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    [TestFixture]
    class BadEncodingTests : MessageParserTestsBase
    {
        [Test]
        public void BadEncodingTest()
        {
            Test("bad_encoding.eml");
        }

        [Test]
        public void BadAddressNameEncodingTest()
        {
            Test("bad_address_name_encoding.eml");
        }

        [Test]
        public void BadCharsInSubject()
        {
            Test("bad_chars_in_subject.eml");
        }

        [Test]
        public void BadSubjectUtf8()
        {
            Test("bad_subject_utf8.eml");
        }

        [Test]
        public void EncriptedUtf8()
        {
            Test("utf8_encripted_teamlab.eml");
        }

        [Test]
        public void BadDecodedBody()
        {
            Test("kuponika_ru_bad_decoding_body.eml");
        }

        [Test]
        public void BadEncodedMailHundle()
        {
            Test("hundle_bad_encoding_mail.eml");
        }

        [Test]
        public void BadFromInQuotedPrintable()
        {
            Test("bad_quoted_printable_from_teamlab.eml");
        }

        [Test]
        public void BadQoutedPrintableDecodingWithFirstCharPersentTest()
        {
            Test("groupon_incorrect_quoted_decoding.eml");
        }

        [Test]
        public void BadFromEmailParsedNoUnderscoreTest()
        {
            Test("bugs_16361_28305.eml");
        }

        [Test]
        public void ContentTransferEncodingInUpperCaseTest()
        {
            Test("Your app status is In Review.eml");
        }

        [Test]
        public void BigMessageHtmlBody()
        {
            Test("big_message.html.eml");
        }

        [Test]
        public void ShouldParseEmptyTextBodyWithHeader()
        {
            Test("empty_text_body_lingua_leo.eml");
        }

        [Test]
        public void EmptyBodyCharset()
        {
            Test("mail_delivery_failed_rambler.eml");
        }

        [Test]
        public void IncorrectCharsetSubjectAndFromName()
        {
            Test("empty_charset_subject_from.eml");
        }
    }
}
