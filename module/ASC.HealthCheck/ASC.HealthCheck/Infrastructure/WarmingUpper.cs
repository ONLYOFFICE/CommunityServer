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


using log4net;
using RestSharp;
using System.Threading.Tasks;

namespace ASC.HealthCheck.Infrastructure
{
    public class WarmingUpper
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WarmingUpper));
        private const int ApiCount = 5;
        private readonly Task[] tasks = new Task[ApiCount];

        public void WarmingUp(string rootUrl)
        {
            tasks[0] = Task.Run(() => MakeHealthCheckRequest(rootUrl + "api/PortsCheckApi/GetPortList", Method.GET));
            tasks[1] = Task.Run(() => MakeHealthCheckRequest(rootUrl + "api/PortsCheckApi/GetPortStatus", Method.GET));
            tasks[2] = Task.Run(() => MakeHealthCheckRequest(rootUrl + "api/NotifiersApi//GetNotifySettings", Method.GET));
            tasks[3] = Task.Run(() => MakeHealthCheckRequest(rootUrl + "api/NotifiersApi/GetNotifiers", Method.GET));
            tasks[4] = Task.Run(() => MakeHealthCheckRequest(rootUrl + "api/DriveSpaceApi/GetDriveSpace", Method.GET));
            Task.WaitAll(tasks);
            log.Debug("Warming up done.");
        }

        public void MakeHealthCheckRequest(string url, Method method)
        {
            var client = new RestClient(url);
            var request = new RestRequest(method);
            var response = client.Execute(request);
            log.DebugFormat("Request executed {0} , response.Content {1}", url, response.Content);
        }
    }
}
