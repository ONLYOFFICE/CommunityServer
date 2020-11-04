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
using System.Linq;
using System.ServiceProcess;
using ASC.Common.Logging;

namespace ASC.Mail.Watchdog.Service
{
    partial class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(params string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (Environment.UserInteractive || (args.Any()))
            {
                var service = new Watchdog();
                service.StartConsole();
            }
            else
            {
                ServiceBase.Run(new Watchdog());
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var log = LogManager.GetLogger("ASC.Mail.Watchdog");
            log.FatalFormat("Unhandled exception: {0}", e.ExceptionObject.ToString());
        }
    }
}
