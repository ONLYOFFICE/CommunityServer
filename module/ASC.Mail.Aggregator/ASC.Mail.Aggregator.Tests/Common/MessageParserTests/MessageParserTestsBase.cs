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


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;
using MimeKit;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    internal class MessageParserTestsBase
    {
        protected const string TestFolderPath = @"..\..\Data\";
        protected const string RightParserResultsPath = @"..\..\Right_Parsing\";
        protected const string TestResultsPath = @"..\..\Out_Eml\";

        protected readonly string utf8Charset = Encoding.UTF8.HeaderName;

        protected RightParserResult ConvertToRightResult(MimeMessage emlMessage)
        {
            var from = emlMessage.From.Mailboxes.FirstOrDefault();

            var result = new RightParserResult
            {
                From = new TestAddress
                {
                    Email = from == null ? "" : from.Address,
                    Name = from == null ? "" : from.Name
                },
                To = new List<TestAddress>()
            };

            foreach (var toAdresses in emlMessage.To.Mailboxes)
            {
                result.To.Add(new TestAddress {Name = toAdresses.Name, Email = toAdresses.Address});
            }

            result.Cc = new List<TestAddress>();
            foreach (var ccAdresses in emlMessage.Cc.Mailboxes)
            {
                result.Cc.Add(new TestAddress {Name = ccAdresses.Name, Email = ccAdresses.Address});
            }

            result.Subject = emlMessage.Subject;
            result.AttachmentCount = emlMessage.Attachments.Count();

            //result.UnknownPatsCount = emlMessage.Body.;

            result.TextBody = emlMessage.TextBody ?? "";
            result.HtmlBody = emlMessage.HtmlBody ?? "";

            var bodyParts = emlMessage.BodyParts;

            var mimeEntities = bodyParts as IList<MimeEntity> ?? bodyParts.ToList();
            var textPart = mimeEntities.FirstOrDefault(b => b.ContentType.MimeType == "text/plain") as TextPart;
            var htmlPart = mimeEntities.FirstOrDefault(b => b.ContentType.MimeType == "text/html") as TextPart;

            var internalMessages = mimeEntities.Where(t => t.ContentType.IsMimeType("message", "*")).ToList();
            if(internalMessages.Any())
            {
                result.AttachmentCount += internalMessages.Count();
            }

            if (textPart != null)
            {
                if (textPart.ContentType.Charset != null)
                {
                    var encoding = EncodingTools.GetEncodingByCodepageName(textPart.ContentType.Charset);

                    if (!encoding.HeaderName.Equals(textPart.ContentType.Charset,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.TextBody = textPart.GetText(encoding);
                        result.TextCharset = encoding.HeaderName.ToLowerInvariant();
                    }
                    else
                    {
                        result.TextCharset = textPart.ContentType.Charset.ToLowerInvariant();
                    }
                }
                else
                {
                    //TODO: Try to find charset in other parts and detect encoding and right text
                    result.TextCharset = "utf-8";
                }
            }
            else
            {
                result.TextCharset = "utf-8";
            }

            if (htmlPart != null)
            {
                if (htmlPart.ContentType.Charset != null)
                {
                    var encoding = EncodingTools.GetEncodingByCodepageName(htmlPart.ContentType.Charset);

                    if (!encoding.HeaderName.Equals(htmlPart.ContentType.Charset,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.HtmlBody = htmlPart.GetText(encoding);
                        result.HtmlCharset = htmlPart.ContentType.Charset;
                    }
                    else
                    {
                        result.HtmlCharset = htmlPart.ContentType.Charset.ToLowerInvariant();
                    }
                }
                else
                {
                    //TODO: Try to find charset in other parts and detect encoding and right text
                    result.HtmlCharset = "utf-8";
                }
            }
            else
            {
                result.HtmlCharset = "utf-8";
            }

            return result;
        }

        protected void CreateRightResult(MimeMessage emlMessage, string outFilePath)
        {
            var result = ConvertToRightResult(emlMessage);
            result.ToXml(outFilePath);
        }

        protected void Test(string emlFileName)
        {
            var mimeMessage = MailClient.ParseMimeMessage(TestFolderPath + emlFileName);
            var emlMessage = ConvertToRightResult(mimeMessage);
            var rightResult = RightParserResult.FromXml(RightParserResultsPath + emlFileName.Replace(".eml", ".xml"));
            
#if NEED_OUT
            mimeMessage.WriteTo(TestFolderPath + emlFileName);
#endif
            Assert.AreEqual(rightResult.From.Email, emlMessage.From.Email);
            Assert.AreEqual(rightResult.From.Name, emlMessage.From.Name);
            Assert.AreEqual(rightResult.To.Count, emlMessage.To.Count);

            var toEnumerator = emlMessage.To.OrderBy(x => x.Email).GetEnumerator();
            foreach (var adress in rightResult.To.OrderBy(x => x.Email))
            {
                toEnumerator.MoveNext();
                Assert.AreEqual(adress.Email, toEnumerator.Current.Email);
                Assert.AreEqual(adress.Name, toEnumerator.Current.Name);
            }

            var ccEnumerator = emlMessage.Cc.OrderBy(x=>x.Email).GetEnumerator();
            foreach (var adress in rightResult.Cc.OrderBy(x=>x.Email))
            {
                ccEnumerator.MoveNext();
                Assert.AreEqual(adress.Email, ccEnumerator.Current.Email);
                Assert.AreEqual(adress.Name, ccEnumerator.Current.Name);
            }

            Assert.AreEqual(rightResult.Subject, emlMessage.Subject);
            Assert.AreEqual(rightResult.AttachmentCount, emlMessage.AttachmentCount);
            //Assert.AreEqual(rightResult.UnknownPatsCount, emlMessage.UnknownDispositionMimeParts.Count);

            Assert.AreEqual(rightResult.TextBody, emlMessage.TextBody);
            Assert.AreEqual(rightResult.TextCharset, emlMessage.TextCharset);
            //Assert.AreEqual(rightResult.TextContentDisposition, emlMessage.TextContentDisposition);

            Assert.AreEqual(rightResult.HtmlBody, emlMessage.HtmlBody);
            Assert.AreEqual(rightResult.HtmlCharset, emlMessage.HtmlCharset);
            //Assert.AreEqual(rightResult.HtmlContentDisposition, emlMessage.HtmlContentDisposition);
        }
    }

    public class TestAddress
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class RightParserResult
    {
        public TestAddress From { get; set; }
        public List<TestAddress> To { get; set; }
        public List<TestAddress> Cc { get; set; }
        public string Subject { get; set; }

        //public ContentTransferEncoding TextEncoding { get; set; }
        public string TextCharset { get; set; }
        public string TextBody { get; set; }
        //public ContentTransferEncoding HtmlEncoding { get; set; }
        public string HtmlCharset { get; set; }
        public string HtmlBody { get; set; }

        public int AttachmentCount { get; set; }
        public int UnknownPatsCount { get; set; }

        public void ToXml(string filePath)
        {
            var settings = new XmlWriterSettings
            {
                NewLineHandling = NewLineHandling.None,
                Indent = false
            };

            var serializer = new XmlSerializer(typeof (RightParserResult));
            using (var textWriter = XmlWriter.Create(new StreamWriter(filePath), settings))
            {
                serializer.Serialize(textWriter, this);
            }
        }

        public static RightParserResult FromXml(string filePath)
        {
            var deserializer = new XmlSerializer(typeof (RightParserResult));
            RightParserResult result;
            using (var textReader = new XmlTextReader(new StreamReader(filePath)))
            {
                textReader.Normalization = false;
                result = (RightParserResult) deserializer.Deserialize(textReader);
            }

            return result;
        }
    }
}