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
using System.Linq;

namespace ASC.MessagingSystem
{
    public class MessagePolicy
    {
        private static readonly string[] secretIps =
            ConfigurationManagerExtension.AppSettings["messaging.secret-ips"] == null
                ? new string[] {}
                : ConfigurationManagerExtension.AppSettings["messaging.secret-ips"]
                      .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

        public static bool Check(EventMessage message)
        {
            if (message == null) return false;
            if (string.IsNullOrEmpty(message.IP)) return true;

            var ip = GetIpWithoutPort(message.IP);
            return secretIps.All(x => x != ip);
        }

        private static string GetIpWithoutPort(string ip)
        {
            if (ip == null) return null;

            var portIdx = ip.IndexOf(':');
            return portIdx > -1 ? ip.Substring(0, portIdx) : ip;
        }
    }
}