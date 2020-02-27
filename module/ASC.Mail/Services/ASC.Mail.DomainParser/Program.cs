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
using System.Linq;
using System.IO;
using ASC.Mail.Core;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.DomainParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(".Begin");

            var parsePath = @"../../XmlData";

            try
            {
                var listOfConfig = new List<ClientConfig>();

                if (args != null && args.Any())
                {
                    parsePath = args[0];
                }

                Console.WriteLine("\r\nParser path: '{0}'", parsePath);

                if (File.GetAttributes(parsePath) == FileAttributes.Directory)
                {
                    var parsePathInfo = new DirectoryInfo(parsePath);

                    var files = parsePathInfo.GetFiles();

                    Console.WriteLine("\r\n{0} file(s) found!", files.Count());
                    Console.WriteLine("");

                    var index = 0;
                    var count = files.Count();

                    files
                        .ToList()
                        .ForEach(f =>
                        {
                            if (f.Attributes == FileAttributes.Directory) return;
                            ClientConfig obj;
                            if (!ParseXml(f.FullName, out obj)) return;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("                                 ");
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("{0} from {1}", ++index, count);
                            listOfConfig.Add(obj);
                        });
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("\r\n1 file found!");

                    ClientConfig obj;
                    if (ParseXml(parsePath, out obj))
                    {
                        listOfConfig.Add(obj);
                    }
                }

                Console.WriteLine("\r\n{0} config(s) parsed!", listOfConfig.Count);

                var engine = new EngineFactory(-1, ASC.Core.Configuration.Constants.CoreSystem.ToString());

                if (listOfConfig.Any())
                {
                    do
                    {
                        Console.Write("\r\nDo you want add configs to DB? [y, n]: ");
                        var info = Console.ReadKey();
                        if (info.Key == ConsoleKey.Y)
                        {
                            var index = 0;
                            var count = listOfConfig.Count;

                            Console.WriteLine("\r\n");

                            listOfConfig.ForEach(c =>
                            {
                                Console.Write("{0} from {1}", ++index, count);

                                if (!engine.MailBoxSettingEngine.SetMailBoxSettings(c, false)) return;
                                if (index >= count) return;
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write("                                 ");
                                Console.SetCursorPosition(0, Console.CursorTop);
                            });

                            Console.WriteLine("\r\n");
                            Console.WriteLine("{0} config(s) added to DB!", listOfConfig.Count);
                            Console.WriteLine("");
                            break;
                        }
                        if (info.Key != ConsoleKey.N) continue;
                        Console.WriteLine("\r\n");
                        break;
                    } while (true);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine(".End");
            Console.ReadKey();
        }

        private static bool ParseXml(string filepath, out ClientConfig obj)
        {
            obj = null;

            try
            {
                obj = ClientConfig.LoadFromFile(filepath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
