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


using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ActiveUp.Net.Mail;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common.MessageParserTests
{
    internal class MessageParserTestsBase
    {
        protected const string TestFolderPath = @"..\..\Data\";
        protected const string RightParserResultsPath = @"..\..\Right_Parsing\";
        protected const string TestResultsPath = @"..\..\Out_Eml\";

        protected readonly string utf8Charset = Encoding.UTF8.HeaderName;

        protected void CreateRightResult(Message emlMessage, string outFilePath)
        {
            var result = new RightParserResult();
            result.From = new TestAddress();

            result.From.Email = emlMessage.From.Email;
            result.From.Name = emlMessage.From.Name;

            result.To = new List<TestAddress>();
            foreach (var to_adresses in emlMessage.To)
            {
                result.To.Add(new TestAddress {Name = to_adresses.Name, Email = to_adresses.Email});
            }

            result.Cc = new List<TestAddress>();
            foreach (var cc_adresses in emlMessage.Cc)
            {
                result.Cc.Add(new TestAddress {Name = cc_adresses.Name, Email = cc_adresses.Email});
            }

            result.Subject = emlMessage.Subject;
            result.AttachmentCount = emlMessage.Attachments.Count;
            result.UnknownPatsCount = emlMessage.UnknownDispositionMimeParts.Count;

            result.HtmlBody = emlMessage.BodyHtml.Text;
            result.HtmlCharset = emlMessage.BodyHtml.Charset;
            result.HtmlEncoding = emlMessage.BodyHtml.ContentTransferEncoding;

            result.TextBody = emlMessage.BodyText.Text;
            result.TextCharset = emlMessage.BodyText.Charset;
            result.TextEncoding = emlMessage.BodyText.ContentTransferEncoding;

            result.ToXml(outFilePath);
        }

        protected Message ParseEml(string emlFileName)
        {
            var emlMessage = Parser.ParseMessageFromFile(TestFolderPath + emlFileName);
            return emlMessage;
        }

        protected void Test(string emlFileName)
        {
            var emlMessage = ParseEml(emlFileName);
            var rightResult = RightParserResult.FromXml(RightParserResultsPath + emlFileName.Replace(".eml", ".xml"));
#if NEED_OUT
            eml_message.StoreToFile(test_results_path + eml_file_name);
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
            Assert.AreEqual(rightResult.AttachmentCount, emlMessage.Attachments.Count);
            Assert.AreEqual(rightResult.UnknownPatsCount, emlMessage.UnknownDispositionMimeParts.Count);

            //Replace needed for correct file loading
            Assert.AreEqual(rightResult.HtmlBody, emlMessage.BodyHtml.Text);
            Assert.AreEqual(rightResult.HtmlEncoding, emlMessage.BodyHtml.ContentTransferEncoding);
            Assert.AreEqual(rightResult.HtmlCharset, emlMessage.BodyHtml.Charset);

            Assert.AreEqual(rightResult.TextBody, emlMessage.BodyText.Text);
            Assert.AreEqual(rightResult.TextCharset, emlMessage.BodyText.Charset);
            Assert.AreEqual(rightResult.TextEncoding, emlMessage.BodyText.ContentTransferEncoding);
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

        public ContentTransferEncoding TextEncoding { get; set; }
        public string TextCharset { get; set; }
        public string TextBody { get; set; }
        public ContentTransferEncoding HtmlEncoding { get; set; }
        public string HtmlCharset { get; set; }
        public string HtmlBody { get; set; }

        public int AttachmentCount { get; set; }
        public int UnknownPatsCount { get; set; }


        public void ToXml(string file_path)
        {
            var settings = new XmlWriterSettings();
            settings.NewLineHandling = NewLineHandling.None;
            settings.Indent = false;

            var serializer = new XmlSerializer(typeof (RightParserResult));
            using (var text_writer = XmlWriter.Create(new StreamWriter(file_path), settings))
            {
                serializer.Serialize(text_writer, this);
            }
        }


        public static RightParserResult FromXml(string file_path)
        {
            var deserializer = new XmlSerializer(typeof (RightParserResult));
            RightParserResult result;
            using (var text_reader = new XmlTextReader(new StreamReader(file_path)))
            {
                text_reader.Normalization = false;
                result = (RightParserResult) deserializer.Deserialize(text_reader);
            }

            return result;
        }
    }
}