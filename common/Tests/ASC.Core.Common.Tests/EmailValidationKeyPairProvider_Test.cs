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