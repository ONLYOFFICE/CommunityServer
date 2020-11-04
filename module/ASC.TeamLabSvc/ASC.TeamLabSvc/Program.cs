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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.TeamLabSvc.Configuration;

namespace ASC.TeamLabSvc
{
    sealed class Program : ServiceBase
    {
        private static readonly ILog log;
        private static List<IServiceController> services;

        static Program()
        {
            log = LogManager.GetLogger("ASC.TeamLabSvc");
            services = new List<IServiceController>();
        }

        private static void Main(string[] args)
        {
#if DEBUG
            if (Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["debugBreak"]))
            {
                Debugger.Launch();
            }
#endif

            var program = new Program();
            if (Environment.UserInteractive || args.Contains("--console") || args.Contains("-c"))
            {
                program.OnStart(args);

                Console.WriteLine("\r\nPress any key to stop...\r\n");
                Console.ReadKey();

                program.OnStop();
            }
            else
            {
                Run(program);
            }
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                // start all services from config or start only one service from parameter -t ServiceType
                if (args.Length == 0)
                {
                    args = Environment.GetCommandLineArgs();
                }

                var serviceType = string.Empty;
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-s" || args[i] == "--service")
                    {
                        if (string.IsNullOrEmpty(args[i + 1]))
                        {
                            throw new ArgumentNullException("--service", "Type of service not found.");
                        }
                        serviceType = args[i + 1].Trim().Trim('"');
                    }
                }

                if (!string.IsNullOrEmpty(serviceType))
                {
                    services.Add((IServiceController)Activator.CreateInstance(Type.GetType(serviceType, true)));
                }
                else
                {
                    var section = TeamLabSvcConfigurationSection.GetSection();
                    if (section != null)
                    {
                        foreach (TeamLabSvcConfigurationElement e in section.TeamlabServices)
                        {
                            services.Add((IServiceController)Activator.CreateInstance(Type.GetType(e.Type, true)));
                        }
                    }
                }

                if (!services.Any())
                {
                    throw new InvalidOperationException("No services to start.");
                }
            }
            catch (Exception error)
            {
                log.ErrorFormat("Can not start services: {0}", error);
                return;
            }

            foreach (var s in services)
            {
                try
                {
                    s.Start();
                    log.InfoFormat("Service {0} started.", GetServiceName(s));
                }
                catch (Exception error)
                {
                    log.ErrorFormat("Can not start service {0}: {1}", GetServiceName(s), error);
                }
            }
        }

        protected override void OnStop()
        {
            foreach (var s in services)
            {
                try
                {
                    s.Stop();
                    log.InfoFormat("Service {0} stopped.", GetServiceName(s));
                }
                catch (Exception error)
                {
                    log.ErrorFormat("Can not stop service {0}: {1}", GetServiceName(s), error);
                }
            }

            services.Clear();
        }

        private string GetServiceName(IServiceController controller)
        {
            var type = controller.GetType();
            var attributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            return 0 < attributes.Length ? ((DisplayNameAttribute)attributes[0]).DisplayName : type.Name;
        }
    }
}
