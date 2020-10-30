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


using System.ServiceModel;
using ASC.Common.Module;

namespace ASC.Data.Storage.Migration
{
    public class Launcher : IServiceController
    {
        private ServiceHost host;
        private Service service;

        public void Start()
        {
            host = new ServiceHost(typeof(Service));
            host.Open();

            service = new Service();
        }

        public void Stop()
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }

            if (service != null)
            {
                service.StopMigrate();
            }
        }
    }
}
