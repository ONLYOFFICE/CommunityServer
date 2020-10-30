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


#if DEBUG
using System;

namespace ASC.Common.Tests.Security.Cryptography
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ASC.Security.Cryptography;
    using System.Security.Cryptography;
    using ASC.Core;

    [TestClass]
    public class EmailValidationKeyPairProvider_Test
    {
        public EmailValidationKeyPairProvider_Test()
        {
            CoreContext.TenantManager.SetCurrentTenant(1);
        }

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