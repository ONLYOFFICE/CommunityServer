/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


#if DEBUG
namespace ASC.Common.Tests.Utils
{
    using ASC.Common.Utils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;

    [TestClass]
    public class HtmlUtil_Test
    {
        [TestMethod]
        public void GetTextBr()
        {
            string html = "Hello";
            Assert.AreEqual("Hello", HtmlUtil.GetText(html));

            html = "Hello    anton";
            Assert.AreEqual("Hello    anton", HtmlUtil.GetText(html));

            html = "Hello<\\ br>anton";
            //Assert.AreEqual("Hello\n\ranton", HtmlUtil.GetText(html));
        }

        public void Hard()
        {
            string html = @"<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4""><font size=""+1"">XXX</font></a>
<div class=""moz-text-html"" lang=""x-unicode""><hr />
A &quot;b&quot; c, d:<br />
<blockquote>mp3 &quot;s&quot;<br />
<br />
&quot;s&quot; - book...</blockquote>... <br />
<hr />
w <a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/UserPage.aspx?userid=731fa2f6-0283-41ab-b4a6-b014cc29f358"">AA</a> 20 a 2009 15:53<br />
<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4#comments"">fg</a></div>";

            System.Diagnostics.Trace.Write(HtmlUtil.GetText(html));
        }

        [TestMethod]
        public void FromFile()
        {
            var html = File.ReadAllText("tests/utils/html_test.html");//Include file!
            //var text = HtmlUtil.GetText(html);

            //var advancedFormating = HtmlUtil.GetText(html, true);
            var advancedFormating2 = HtmlUtil.GetText(html,40);
            Assert.IsTrue(advancedFormating2.Length <= 40);

            var advancedFormating3 = HtmlUtil.GetText(html, 40, "...");
            Assert.IsTrue(advancedFormating3.Length <= 40);
            StringAssert.EndsWith(advancedFormating3, "...");

            var empty = HtmlUtil.GetText(string.Empty);
            Assert.AreEqual(string.Empty, empty);

            var invalid = HtmlUtil.GetText("This is not html <div>");
            Assert.AreEqual(invalid, "This is not html");

            var xss = HtmlUtil.GetText("<script>alert(1);</script> <style>html{color:#444}</style>This is not html <div on click='javascript:alert(1);'>");
            Assert.AreEqual(xss, "This is not html");

            //var litleText = HtmlUtil.GetText("12345678901234567890", 20, "...",true);

            var test1 = HtmlUtil.GetText(null);
            Assert.AreEqual(string.Empty, test1);

            var test2 = HtmlUtil.GetText("text with \r\n line breaks",20);
            Assert.IsTrue(test2.Length <= 20);

            var test3 = HtmlUtil.GetText("long \r\n text \r\n with \r\n text with \r\n line breaks", 20);
            Assert.IsTrue(test3.Length <= 20);

            var test4 = HtmlUtil.GetText("text text text text text text text text!", 20);
            Assert.IsTrue(test3.Length <= 20);
            StringAssert.StartsWith(test4, "text text text");
        }
    }
}
#endif