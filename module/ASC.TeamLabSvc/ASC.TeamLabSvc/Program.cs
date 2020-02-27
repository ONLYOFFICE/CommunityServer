/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["debugBreak"]))
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
