/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
namespace ASC.Notify.Textile
{
    using ASC.Notify.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
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
#endif