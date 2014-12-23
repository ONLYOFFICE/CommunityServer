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

#if (DEBUG)
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASC.Common.Tests.Notify
{
    class TestPatternFormatter : IPatternFormatter
    {
        public string[] GetTags(IPattern pattern)
        {
            throw new NotImplementedException();
        }

        public void FormatMessage(ASC.Notify.Messages.INoticeMessage message, ITagValue[] tagsValues)
        {
            throw new NotImplementedException();
        }
    }

    class PatternResource
    {
        internal static string resource1
        {
            get { return "resource1"; }
        }

        internal static string resource2
        {
            get { return "resource2"; }
        }

        internal static string resource3
        {
            get { return "resource3"; }
        }
    }

    [TestClass]
    public class XmlPatternProvider2Test
    {
        private readonly string xml =
            @"<patterns>
                <formatter type='ASC.Common.Tests.Notify.TestPatternFormatter, ASC.Common'/>
  
                <pattern id='id1' sender='email.sender'>
                    <subject resource='|resource1|ASC.Common.Tests.Notify.PatternResource, ASC.Common' />
                    <body styler='ASC.Notify.Textile.TextileStyler,ASC.Notify.Textile' resource='|resource2|ASC.Common.Tests.Notify.PatternResource, ASC.Common' />
                </pattern>
                <pattern id='id1' sender='messanger.sender'>
                    <subject />
                    <body styler='ASC.Notify.Textile.JabberStyler,ASC.Notify.Textile' resource='|resource3|ASC.Common.Tests.Notify.PatternResource, ASC.Common' />
                </pattern>

                <pattern id='id2'>
                    <body styler='ASC.Notify.Textile.JabberStyler,ASC.Notify.Textile'>NVelocity Template</body>
                </pattern>

                <pattern id='id3' reference='id2'/>
            </patterns>";


        [TestMethod]
        public void XmlParseTest()
        {
            var xmlPatternProvider2 = new XmlPatternProvider2(xml);

            var pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id1"), "email.sender");
            Assert.IsNotNull(pattern);
            Assert.AreEqual("id1", pattern.ID);
            Assert.AreEqual(PatternResource.resource1, pattern.Subject);
            Assert.AreEqual(PatternResource.resource2, pattern.Body);
            Assert.AreEqual("ASC.Notify.Textile.TextileStyler,ASC.Notify.Textile", pattern.Styler);
            Assert.AreEqual("html", pattern.ContentType);

            pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id1"), "messanger.sender");
            Assert.IsNotNull(pattern);
            Assert.AreEqual("id1", pattern.ID);
            Assert.AreEqual(string.Empty, pattern.Subject);
            Assert.AreEqual(PatternResource.resource3, pattern.Body);
            Assert.AreEqual("ASC.Notify.Textile.JabberStyler,ASC.Notify.Textile", pattern.Styler);
            Assert.AreEqual("html", pattern.ContentType);

            pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id1"), null);
            Assert.IsNull(pattern);

            pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id2"), "email.sender");
            Assert.IsNotNull(pattern);
            Assert.AreEqual("id2", pattern.ID);
            Assert.AreEqual(string.Empty, pattern.Subject);
            Assert.AreEqual("NVelocity Template", pattern.Body);
            Assert.AreEqual("ASC.Notify.Textile.JabberStyler,ASC.Notify.Textile", pattern.Styler);
            Assert.AreEqual("html", pattern.ContentType);

            pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id2"), null);
            Assert.IsNotNull(pattern);

            pattern = xmlPatternProvider2.GetPattern(new NotifyAction("id3"), null);
            Assert.IsNotNull(pattern);

            var formatter = xmlPatternProvider2.GetFormatter(pattern);
            Assert.AreEqual(typeof(TestPatternFormatter), formatter.GetType());
        }
    }
}
#endif