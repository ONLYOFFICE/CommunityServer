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
using ASC.Common.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Common.Tests.Data
{
    [TestClass]
    public class MultiRegionalDbTest
    {
        [TestMethod]
        public void ExecuteListTest()
        {
            var db = new MultiRegionalDbManager("core");
            var r1 = db.ExecuteList("select 1");
            Assert.IsTrue(r1.Count > 1);
        }
    }
}
#endif