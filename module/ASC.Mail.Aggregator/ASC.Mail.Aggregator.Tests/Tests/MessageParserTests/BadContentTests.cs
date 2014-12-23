/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.MessageParserTests
{
    [TestFixture]
    class BadContentTests : MessageParserTestsBase
    {
        [Test]
        public void BadContentTypeTest()
        {
            Test("dostavka_ru_bad_content_type.eml");
        }


        [Test]
        public void EmbedImageTest()
        {
            Test("embed_image.eml");
        }


        [Test]
        public void JavaScriptTest()
        {
            Test("exo__with_javascript.eml");
        }


        [Test]
        public void HtmlCharsIntroductionTest()
        {
            Test("html_chars_in_introduction.eml");
        }

        [Test]
        public void ManyAttachmentsTest()
        {
            Test("letter_with_58_attachments.eml");
        }


        [Test]
        public void BadSanitazeTest()
        {
            Test("Mail_ru_bad_sanitaze.eml");
        }


        [Test]
        public void StyleWithClassesTest()
        {
            Test("mail_ru_style_with_classes.eml");
        }

        [Test]
        public void NoImageAfterSanitizeTest()
        {
            Test("message-no-image-after-sanitize.eml");
        }


        [Test]
        public void MailRuMessageTest()
        {
            Test("message_mailru.eml");
        }

        [Test]
        public void UnsanitizedImage1Test()
        {
            Test("message_mailru_with_unsanitized_images.eml");
        }


        [Test]
        public void UnsanitizedImage2Test()
        {
            Test("message_mailru_with_unsanitized_images2.eml");
        }

        [Test]
        public void MessageWithRussianAttachmentTest()
        {
            Test("message_with_russian_attachment.eml");
        }


        [Test]
        public void MessageWithSubmessagesTest()
        {
            Test("message_with_submessages.eml");
        }

        [Test]
        public void MessageWithUnknownDispositionMimePartsTest()
        {
            Test("message_with_UnknownDispositionMimeParts.eml");
        }


        [Test]
        public void OnlyTextBodyTest()
        {
            Test("only_text_body.eml");
        }


        [Test]
        public void SlashInAddressNameTest()
        {
            Test("slash_in_address_name.eml");
        }

        [Test]
        public void SotmarketSubjectTest()
        {
            Test("sotmarket-subject-err.eml");
        }


        [Test]
        public void BaseTagTest()
        {
            Test("test_base_tag.eml");
        }

        [Test]
        public void SendPreparedTest()
        {
            Test("test_send_prepared.eml");
        }


        [Test]
        public void WithBaseTagTest()
        {
            Test("with_base_tag.eml");
        }


        [Test]
        public void SkypeScrollEmailTest()
        {
            Test("yandex_skype_scroll_email.eml");
        }


        [Test]
        public void UnlimitedScrollTest()
        {
            Test("printdirect_ru__unlimit_scroll.eml");
        }

        [Test]
        public void BadSubjectEncodingTest()
        {
            Test("kulichiki.eml");
        }

        [Test]
        public void BadAttachContentDisposition()
        {
            Test("bad_content_disposition_in_attaches.eml");
        }

        // message is corrupted and were generating stack overflow
        [Test]
        public void LongCombinedEncodedMimePart()
        {
            Test("long_encoded_mime_part.eml");
        }
    }
}
