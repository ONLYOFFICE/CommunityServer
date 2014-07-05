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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using ASC.Mail.Aggregator;

namespace ASC.Mail.DomainParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(".Begin");

            var parse_path = @"../../XmlData";

            try
            {
                var manger = new MailBoxManager(25);

                var list_of_config = new List<clientConfig>();

                if (args != null && args.Any())
                {
                    parse_path = args[0];
                }

                Console.WriteLine("\r\nParser path: '{0}'", parse_path);

                if (File.GetAttributes(parse_path) == FileAttributes.Directory)
                {
                    var parse_path_info = new DirectoryInfo(parse_path);

                    var files = parse_path_info.GetFiles();

                    Console.WriteLine("\r\n{0} file(s) found!", files.Count());
                    Console.WriteLine("");

                    var index = 0;
                    var count = files.Count();

                    files
                        .ToList()
                        .ForEach(f =>
                        {
                            if (f.Attributes == FileAttributes.Directory) return;
                            clientConfig obj;
                            if (!ParseXml(f.FullName, out obj)) return;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("                                 ");
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("{0} from {1}", ++index, count);
                            list_of_config.Add(obj);
                        });
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("\r\n1 file found!");

                    clientConfig obj;
                    if (ParseXml(parse_path, out obj))
                    {
                        list_of_config.Add(obj);
                    }
                }

                Console.WriteLine("\r\n{0} config(s) parsed!", list_of_config.Count);

                if (list_of_config.Count > 0)
                {
                    do
                    {
                        Console.Write("\r\nDo you want add configs to DB? [y, n]: ");
                        var info = Console.ReadKey();
                        if (info.Key == ConsoleKey.Y)
                        {
                            var index = 0;
                            var count = list_of_config.Count;
                            
                            Console.WriteLine("\r\n");

                            list_of_config.ForEach(c =>
                            {
                                Console.Write("{0} from {1}", ++index, count);

                                if (!manger.SetMailBoxSettings(c)) return;
                                if (index >= count) return;
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write("                                 ");
                                Console.SetCursorPosition(0, Console.CursorTop);
                            });

                            Console.WriteLine("\r\n");
                            Console.WriteLine("{0} config(s) added to DB!", list_of_config.Count);
                            Console.WriteLine("");
                            break;
                        }
                        if (info.Key != ConsoleKey.N) continue;
                        Console.WriteLine("\r\n");
                        break;
                    } while (true);
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Such path not exists: '{0}'", parse_path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine(".End");
            Console.ReadKey();
        }

        static bool ParseXml(string filepath, out clientConfig obj)
        {
            obj = null;

            try
            {
                obj = clientConfig.LoadFromFile(filepath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
