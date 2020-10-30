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
using ASC.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ASC.Common.Tests.Security.Cryptography
{
    [TestClass]
    public class Hasher_Test
    {
        [TestMethod]
        public void DoHash()
        {
            string str = "Hello, Jhon!";
            
            Assert.AreEqual(
                Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str))),
                Hasher.Base64Hash(str,HashAlg.MD5)
                );

            Assert.AreEqual(
               Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(str))),
               Hasher.Base64Hash(str, HashAlg.SHA1)
               );

            Assert.AreEqual(
               Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))),
               Hasher.Base64Hash(str, HashAlg.SHA256)
               );

            Assert.AreEqual(
               Convert.ToBase64String(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(str))),
               Hasher.Base64Hash(str, HashAlg.SHA512)
               );

            Assert.AreEqual(
              Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))),
              Hasher.Base64Hash(str) //DEFAULT
              );
        }
    }
}
#endif