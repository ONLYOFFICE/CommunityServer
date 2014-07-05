/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using ASC.Mail.Aggregator.Common.Logging;
using CommandLine.Text;


namespace ASC.Mail.Aggregator.CollectionService
{
    internal static class ConsoleHandler
    {

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public static readonly HandlerRoutine GoodHandler = ConsoleCtrlCheck;

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            AggregatorLogger.Instance.Stop();
            return true;
        }
    }

    sealed partial class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            ConsoleHandler.SetConsoleCtrlHandler(ConsoleHandler.GoodHandler, true);

            if (Environment.UserInteractive)
            {
                var options = new Options();

                if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                                                                    () =>
                                                                        Console.WriteLine("Bad command line parameters.")))
                {
                    var service = new CollectorService(options.OnlyUsers);
                    service.StartDaemon();
                }
                Console.ReadKey();
            }
            else
            {
                ServiceBase.Run(new CollectorService());
            }
        }


        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // ToDo: getcurrentclasslogger were removed from here - should be refactored
            var log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "CollectionService");
            log.Fatal("Unhandled exception: {0}", e.ExceptionObject.ToString());
            log.Flush();
        }
    }
}
