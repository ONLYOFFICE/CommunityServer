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
            var array = new int[]{1, 2, 3, 4, 5 , 6};
            var splitted_array = array.SubArray(2, 1);
            Assert.AreEqual(1, splitted_array.Length);
            Assert.AreEqual(3, splitted_array[0]);
        }

        [Test]
        public void SubArrayWorksCorrectWithLengthMoreThanOne()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6 };
            var splitted_array = array.SubArray(1, 3);
            Assert.AreEqual(3, splitted_array.Length);
            Assert.AreEqual(2, splitted_array[0]);
            Assert.AreEqual(3, splitted_array[1]);
            Assert.AreEqual(4, splitted_array[2]);
        }

        [Test]
        public void SubArrayWorksCorrectWithZeroLength()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6 };
            var splitted_array = array.SubArray(1, 0);
            Assert.AreEqual(0, splitted_array.Length);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), UserMessage = "Source array was not long enough. Check srcIndex and length, and the array's lower bounds.")]
        public void SubArrayThrowsExceptionWithOutboundIndex()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6 };
            array.SubArray(6, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), UserMessage = "Source array was not long enough. Check srcIndex and length, and the array's lower bounds.")]
        public void SubArrayThrowsExceptionWithSubArrayLengthMoreThanArrayLength()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6 };
            array.SubArray(4, 3);
        }
    }
}
