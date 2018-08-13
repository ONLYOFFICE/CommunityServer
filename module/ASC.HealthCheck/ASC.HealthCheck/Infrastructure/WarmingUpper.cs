/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
