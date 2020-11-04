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
using System.Reflection;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Notify.Messages;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Notify
{
    class NotifyService : INotifyService
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");

        private readonly DbWorker db = new DbWorker();


        public void SendNotifyMessage(NotifyMessage notifyMessage)
        {
            try
            {
                db.SaveMessage(notifyMessage);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public void InvokeSendMethod(string service, string method, int tenant, params object[] parameters)
        {
            var serviceType = Type.GetType(service, true);
            var getinstance = serviceType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

            var instance = getinstance.GetValue(serviceType, null);
            if (instance == null)
            {
                throw new Exception("Service instance not found.");
            }

            var methodInfo = serviceType.GetMethod(method);
            if (methodInfo == null)
            {
                throw new Exception("Method not found.");
            }

            CoreContext.TenantManager.SetCurrentTenant(tenant);
            TenantWhiteLabelSettings.Apply(tenant);
            methodInfo.Invoke(instance, parameters);
        }
    }
}
