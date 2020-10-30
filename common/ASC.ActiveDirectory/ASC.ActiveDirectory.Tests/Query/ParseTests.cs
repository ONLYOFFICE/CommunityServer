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
