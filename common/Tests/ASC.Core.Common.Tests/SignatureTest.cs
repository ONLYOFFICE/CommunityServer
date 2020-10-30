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


#if (DEBUG)
using ASC.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASC.Core.Common.Tests
{
    [TestClass]
    public class SignatureTest
    {
        [TestMethod]
        public void TestSignature()
        {
            var validObject = new { expire = DateTime.UtcNow.AddMinutes(15), key = "345jhndfg", ip = "192.168.1.1" };
            var encoded = Signature.Create(validObject, "ThE SeCret Key!");
            Assert.IsNotNull(Signature.Read<object>(encoded, "ThE SeCret Key!"));
            Assert.IsNull(Signature.Read<object>(encoded, "ThE SeCret Key"));
        }
    }
}
#endif