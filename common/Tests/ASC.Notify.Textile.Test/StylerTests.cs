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


using ASC.Notify.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Notify.Textile.Test
{
    [TestClass]
    public class StylerTests
    {
        private readonly string pattern = "h1.New Post in Forum Topic: \"==Project(%: \"Sample Title\"==\":\"==http://sssp.teamlab.com==\"" + System.Environment.NewLine +
            "25/1/2022 \"Jim\":\"http://sssp.teamlab.com/myp.aspx\"" + System.Environment.NewLine +
            "has created a new post in topic:" + System.Environment.NewLine +
            "==<b>- The text!&nbsp;</b>==" + System.Environment.NewLine +
            "\"Read More\":\"http://sssp.teamlab.com/forum/post?id=4345\"" + System.Environment.NewLine +
            "Your portal address: \"http://sssp.teamlab.com\":\"http://teamlab.com\" " + System.Environment.NewLine +
            "\"Edit subscription settings\":\"http://sssp.teamlab.com/subscribe.aspx\"";

        [TestMethod]
        public void TestJabberStyler()
        {
            var message = new NoticeMessage() { Body = pattern };
            new JabberStyler().ApplyFormating(message);
        }

        [TestMethod]
        public void TestTextileStyler()
        {
            var message = new NoticeMessage() { Body = pattern };
            new TextileStyler().ApplyFormating(message);
        }
    }
}