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