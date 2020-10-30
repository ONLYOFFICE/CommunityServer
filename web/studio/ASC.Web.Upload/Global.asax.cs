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
using System.Configuration;
using System.Web;
using ASC.Common.Data;
using ASC.Common.DependencyInjection;
using ASC.Data.Storage.S3;
using ASC.Web.Core;
using TMResourceData;
using log4net.Config;

namespace ASC.Web.Upload
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();
            DbRegistry.Configure();
            ConfigureServiceLocator();
            (new S3UploadGuard()).DeleteExpiredUploadsAsync(TimeSpan.FromDays(1));

            if (ConfigurationManagerExtension.AppSettings["resources.from-db"] == "true")
            {
                DBResourceManager.PatchAssemblies();
            }
        }

        private static readonly object locker = new object();
        private static volatile bool applicationStarted;

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!applicationStarted)
            {
                lock (locker)
                {
                    if (!applicationStarted)
                    {
                        applicationStarted = true;
                        WebItemManager.Instance.LoadItems();
                    }
                }
            }

            var app = (HttpApplication) sender;
            if (app.Context.Request.HttpMethod == "OPTIONS")
            {
                app.Context.Response.StatusCode = 200;
                app.Context.Response.StatusDescription = "OK";
                app.Context.Response.End();
            }
        }

        private static void ConfigureServiceLocator()
        {
            AutofacConfigLoader.Load("files").Build();
        }
    }
}