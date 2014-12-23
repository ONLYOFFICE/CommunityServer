/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Common.Module;
using ASC.TeamLabSvc.Configuration;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace ASC.TeamLabSvc
{
    sealed class Program : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.TeamLabSvc");
        private static List<IServiceController> services = new List<IServiceController>();

        private static void Main(string[] args)
        {
#if DEBUG
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["debugBreak"]))
            {
                Debugger.Launch();
            }
#endif
            XmlConfigurator.Configure();

            var program = new Program();
            if (Environment.UserInteractive || args.Contains("--console"))
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
                var section = TeamLabSvcConfigurationSection.GetSection();
                foreach (TeamLabSvcConfigurationElement e in section.TeamlabServices)
                {
                    if (!e.Disable)
                    {
                        services.Add((IServiceController)Activator.CreateInstance(Type.GetType(e.Type, true)));
                    }
                    else
                    {
                        log.InfoFormat("Skip service {0}", e.Type);
                    }
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
