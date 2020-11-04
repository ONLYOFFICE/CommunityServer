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
using System.IO;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Json;
using System.Text;
using ASC.Common.Security;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;
using ASC.Web.Studio;
using Autofac;

namespace ASC.Web.CRM
{
    public abstract class BasePage : MainPage
    {
        protected ILifetimeScope Scope { get; set; }
        protected internal DaoFactory DaoFactory { get; set; }

        protected BasePage()
        {
            Scope = DIHelper.Resolve();
            DaoFactory = Scope.Resolve<DaoFactory>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageLoad();
        }

        public void JsonPublisher<T>(T data, String jsonClassName) where T : class
        {
            String json;

            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, data);
                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            Page.RegisterInlineScript(String.Format(" var {1} = {0};", json, jsonClassName), onReady: false);
        }
      
        protected abstract void PageLoad();

        protected override void OnUnload(EventArgs e)
        {
            if (Scope != null)
            {
                Scope.Dispose();
            }
            base.OnUnload(e);
        }
    }
}
