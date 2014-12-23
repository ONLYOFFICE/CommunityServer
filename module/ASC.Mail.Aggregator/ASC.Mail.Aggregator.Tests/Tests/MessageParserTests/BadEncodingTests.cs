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

using System.IO;
using System.Linq;
using NUnit.Framework;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Tests.MessageParserTests
{
    [TestFixture]
    class BadEncodingTests : MessageParserTestsBase
    {
        //
        //This test creates right parsing xml. Use it for new test adding.
        //

        [Test]
        [Ignore("This text need for right answers generation")]
        public void RecerateRight_ParsingResults()
        {
            var eml_files = Directory.GetFiles(TestFolderPath, "*.eml")
                                     .Select(path => Path.GetFileName(path));

            foreach (var eml_file in eml_files)
            {
                var eml_message = Parser.ParseMessageFromFile(TestFolderPath + eml_file);
                CreateRightResult(eml_message, RightParserResultsPath + eml_file.Replace(".eml", ".xml"));
            }
        }

        [Test]
        [Ignore("This text need for right answers generation")]
        public void RecerateRight_ParsingResult()
        {
            var eml_file = "groupon_incorrect_quoted_decoding.eml";
            var eml_message = Parser.ParseMessageFromFile(TestFolderPath + eml_file);
            CreateRightResult(eml_message, RightParserResultsPath + eml_file.Replace(".eml", ".xml"));
        }


        [Test]
        public void BadEncodingTest()
        {
            Test("bad_encoding.eml");
        }


        [Test]
        public void BadEncoding2Test()
        {
            Test("bad_encoding_2.eml");
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
    }
}
