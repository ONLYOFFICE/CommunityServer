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


using System;
using ASC.Mail.Server.Utils;
using NUnit.Framework;

namespace HelpUtilsTests
{
    [TestFixture]
    public class SubArrayTests
    {
        [Test]
        public void SubArrayWorksCorrectWithLengthOne()
        {
            var array = new[]{1, 2, 3, 4, 5 , 6};
            var splittedArray = array.SubArray(2, 1);
            Assert.AreEqual(1, splittedArray.Length);
            Assert.AreEqual(3, splittedArray[0]);
        }

        [Test]
        public void SubArrayWorksCorrectWithLengthMoreThanOne()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6 };
            var splittedArray = array.SubArray(1, 3);
            Assert.AreEqual(3, splittedArray.Length);
            Assert.AreEqual(2, splittedArray[0]);
            Assert.AreEqual(3, splittedArray[1]);
            Assert.AreEqual(4, splittedArray[2]);
        }

        [Test]
        public void SubArrayWorksCorrectWithZeroLength()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6 };
            var splittedArray = array.SubArray(1, 0);
            Assert.AreEqual(0, splittedArray.Length);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), UserMessage = "Source array was not long enough. Check srcIndex and length, and the array's lower bounds.")]
        public void SubArrayThrowsExceptionWithOutboundIndex()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6 };
            array.SubArray(6, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), UserMessage = "Source array was not long enough. Check srcIndex and length, and the array's lower bounds.")]
        public void SubArrayThrowsExceptionWithSubArrayLengthMoreThanArrayLength()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6 };
            array.SubArray(4, 3);
        }
    }
}
