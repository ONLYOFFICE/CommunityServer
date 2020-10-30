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
namespace ASC.Core.Common.Tests
{
    using System;
    using System.Configuration;


    public class DbBaseTest<TDbService>
    {
        protected TDbService Service
        {
            get;
            private set;
        }

        protected int Tenant
        {
            get;
            private set;
        }


        protected DbBaseTest()
        {
            Service = (TDbService)Activator.CreateInstance(typeof(TDbService), ConfigurationManager.ConnectionStrings["core"]);
            Tenant = 1024;

            //var pattern = "%message (%property{duration} ms)     %property{sql}    %property{sqlParams}%newline";
            //BasicConfigurator.Configure(new DebugAppender { Layout = new PatternLayout(pattern) });
        }
    }
}
#endif
