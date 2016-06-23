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
    class BadContentTests : MessageParserTestsBase
    {
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

        // message is corrupted and it has generated stack overflow
        [Test]
        public void LongCombinedEncodedMimePart()
        {
            Test("long_encoded_mime_part.eml");
        }

        [Test]
        public void NctNoAttachment()
        {
            Test("nct_attachment_not_parsed.eml");
        }

        [Test]
        public void BadAttachBody()
        {
            Test("empty_attach_body.eml");
        }

        [Test]
        public void ShouldParseContentFilenameRfc5987()
        {
            Test("content_disposition_filename_rfc5987.eml");
        }
        
        [Test]
        public void ShouldParseEmbeddedMessageRfc822()
        {
            Test("yandex_with_embedded_message_in.eml");
        }

        [Test]
        public void ShouldParseICloudCalendarEvent()
        {
            Test("icloud_ics.eml");
        }

        [Test]
        public void ShouldParseInvalidAttachmentsNames()
        {
            Test("invalid_attachments_filenames.eml");
        }
    }
}
