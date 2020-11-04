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


using System;
using System.Web;
using ASC.Common.Logging;

namespace ASC.Api
{
    public class ApiSetupHttpModule : IHttpModule
    {
        private static volatile bool initialized = false;
        private static object locker = new object();


        public void Init(HttpApplication context)
        {
            if (!initialized)
            {
                lock (locker)
                {
                    if (!initialized)
                    {
                        try
                        {
                            ApiSetup.Init();
                            initialized = true;
                        }
                        catch (Exception err)
                        {
                            if (err is TypeInitializationException && err.InnerException != null)
                            {
                                err = err.InnerException;
                            }
                            LogManager.GetLogger("ASC").Error(err);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
