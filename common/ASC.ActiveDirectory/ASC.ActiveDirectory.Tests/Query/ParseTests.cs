/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.ActiveDirectory.Base.Expressions;
using NUnit.Framework;

namespace ASC.ActiveDirectory.Tests.Query
{
    [TestFixture]
    public class ParseTests
    {
        [Test]
        public void EscapeAsterisk()
        {
            const string cn_with_asterisk = "CN=test*";

            var exp = Expression.Parse(cn_with_asterisk);

            Assert.AreEqual("CN", exp.Name);
            Assert.AreEqual("test*", exp.Value);
            Assert.AreEqual(Op.Equal, exp.Operation);
            Assert.AreEqual("(CN=test\\2a)", exp.ToString());
        }

        [Test]
        public void EscapeParentheses()
        {
            const string cn_with_asterisk = "CN=ИТ-ПРЦ (Все домены)";

            var exp = Expression.Parse(cn_with_asterisk);

            Assert.AreEqual("CN", exp.Name);
            Assert.AreEqual("ИТ-ПРЦ (Все домены)", exp.Value);
            Assert.AreEqual(Op.Equal, exp.Operation);
            Assert.AreEqual("(CN=ИТ-ПРЦ \\28Все домены\\29)", exp.ToString());
        }

        [Test]
        public void EscapeNul()
        {
            var cnWithAsterisk = "CN=" + '\u0000';

            var exp = Expression.Parse(cnWithAsterisk);

            Assert.AreEqual("CN", exp.Name);
            Assert.AreEqual("\0", exp.Value);
            Assert.AreEqual(Op.Equal, exp.Operation);
            Assert.AreEqual("(CN=\\00)", exp.ToString());
        }

        [Test]
        public void EscapeSlash()
        {
            const string cn_with_asterisk = "CN=test/";

            var exp = Expression.Parse(cn_with_asterisk);

            Assert.AreEqual("CN", exp.Name);
            Assert.AreEqual("test/", exp.Value);
            Assert.AreEqual(Op.Equal, exp.Operation);
            Assert.AreEqual("(CN=test\\2f)", exp.ToString());
        }
    }
}
