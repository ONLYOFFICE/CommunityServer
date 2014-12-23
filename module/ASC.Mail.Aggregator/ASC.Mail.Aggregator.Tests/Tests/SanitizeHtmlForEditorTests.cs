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

using System;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests
{
    [TestFixture]
    class SanitizeHtmlForEditorTests
    {
        [Test]
        public void TestForRemovingHtml()
        {
            const string html = "<html></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual(String.Empty, res);
        }

        [Test]
        public void TestForRemovingHead1()
        {
            const string html = "<html><head></head></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual(String.Empty, res);
        }

        [Test]
        public void TestForRemovingHead2()
        {
            const string html = "<html><head><style>some content inside styles</style></head></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual(String.Empty, res);
        }
        [Test]
        public void TestForRemovingHead3()
        {
            const string html = "<html>\r\n<head> \r\n<style> \r\n some \r\n content \r\n inside \r\n styles \r\n </style> \r\n </head> \r\n</html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual(String.Empty, res);
        }

        [Test]
        public void TestForSimpleBodyReplace()
        {
            const string html = "<body></body>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div></div>", res);
        }

        [Test]
        public void TestForComplexBodyReplace()
        {
            const string html = "<html><head></head><body></body></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div></div>", res);
        }

        [Test]
        public void TestForWrongBodyReplacements()
        {
            const string html = "<html><head></head><body>I used tag <body></body> thats problem.</body></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div>I used tag <body></body> thats problem.</div>", res);
        }

        [Test]
        public void TestForWrongHtmlReplacements()
        {
            const string html = "<html><head></head><body>I used tag <html></html> thats problem.</body></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div>I used tag <html></html> thats problem.</div>", res);
        }

        [Test]
        public void TestForWrongHeadReplacements()
        {
            const string html = "<html><head></head><body>I used tag <head></head> thats problem.</body></html>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div>I used tag <head></head> thats problem.</div>", res);
        }

        [Test]
        public void TestForAttributeInBodySaving()
        {
            const string html = "<body class='test'></body>";
            var res = HtmlSanitizer.SanitizeHtmlForEditor(html);
            Assert.AreEqual("<div class='test'></div>", res);
        }
    }
}
