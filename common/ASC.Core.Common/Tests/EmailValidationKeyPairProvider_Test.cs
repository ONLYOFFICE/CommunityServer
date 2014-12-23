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

#if DEBUG
using System;

namespace ASC.Common.Tests.Security.Cryptography
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ASC.Security.Cryptography;
    using System.Security.Cryptography;

    [TestClass]
    public class EmailValidationKeyPairProvider_Test
    {
        public void PasswordDerivedBytes_Test()
        {

            byte[] randBytes = new byte[5];
            new Random(10032010).NextBytes(randBytes);


            var tdes = new TripleDESCryptoServiceProvider();
            var pwddb = new PasswordDeriveBytes("1", new byte[] {1});
            tdes.Key = pwddb.CryptDeriveKey("TripleDES", "SHA1", 192, tdes.IV);
            //string s = Convert.ToBase64String(tdes.Key);

        }

        [TestMethod]
        public void GetEmailKey_MillisecondDistanceDifference()
        {
            var k1 = EmailValidationKeyProvider.GetEmailKey("sm_anton@mail.ru");
            System.Threading.Thread.Sleep(15);
            var k2 = EmailValidationKeyProvider.GetEmailKey("sm_anton@mail.ru");

            Assert.AreNotEqual(k1, k2);
        }

        [TestMethod]
        public void ValidateKeyImmediate()
        {
            var k1 = EmailValidationKeyProvider.GetEmailKey("sm_anton@mail.ru");
            Assert.AreEqual(EmailValidationKeyProvider.ValidateEmailKey("sm_anton@mail.ru", k1), EmailValidationKeyProvider.ValidationResult.Ok);
            Assert.AreEqual(EmailValidationKeyProvider.ValidateEmailKey("sm_anton@mail.ru2", k1), EmailValidationKeyProvider.ValidationResult.Invalid);
        }

        [TestMethod]
        public void ValidateKey_Delayed()
        {
            var k1 = EmailValidationKeyProvider.GetEmailKey("sm_anton@mail.ru");
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(EmailValidationKeyProvider.ValidateEmailKey("sm_anton@mail.ru", k1, TimeSpan.FromMilliseconds(150)), EmailValidationKeyProvider.ValidationResult.Ok);
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(EmailValidationKeyProvider.ValidateEmailKey("sm_anton@mail.ru", k1, TimeSpan.FromMilliseconds(150)), EmailValidationKeyProvider.ValidationResult.Expired);
        }
    }
}

#endif