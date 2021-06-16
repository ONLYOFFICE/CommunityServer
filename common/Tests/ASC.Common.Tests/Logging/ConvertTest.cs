/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using log4net;
using log4net.Appender;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASC.Common.Tests.Logging
{
    [TestClass]
    public class ConvertTest
    {
        [TestMethod]
        public void FolderTest()
        {
            XmlConfigurator.Configure();
            var appenders = LogManager.GetLogger("ASC").Logger.Repository.GetAppenders();
            Assert.AreEqual("..\\Logs\\onlyoffice\\8.0\\bin\\Test." + DateTime.Now.ToString("MM-dd") + ".log", ((FileAppender)appenders[0]).File);
        }

        [TestMethod]
        public void CommandLineTest()
        {
            XmlConfigurator.Configure();
            var appenders = LogManager.GetLogger("ASC").Logger.Repository.GetAppenders();
            var fname = ((FileAppender)appenders[0]).File;
            //Assert.AreEqual(Path.GetTempPath() + "onlyoffice\\8.0\\bin\\Test." + DateTime.Now.ToString("MM-dd") + ".log", ((FileAppender)appenders[0]).File);
        }
    }
}
#endif