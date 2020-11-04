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
