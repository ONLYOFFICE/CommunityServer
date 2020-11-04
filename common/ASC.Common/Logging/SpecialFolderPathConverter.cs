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


using log4net.Util;
using System;
using System.IO;

namespace ASC.Common.Logging
{
    public class SpecialFolderPathConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            if (string.IsNullOrEmpty(Option))
            {
                return;
            }
            try
            {
                var result = string.Empty;
                const string CMD_LINE = "CommandLine:";
                if (Option.StartsWith(CMD_LINE))
                {
                    var args = Environment.CommandLine.Split(' ');
                    for (var i = 0; i < args.Length - 1; i++)
                    {
                        if (args[i].Equals(Option.Substring(CMD_LINE.Length), StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = args[i + 1];
                        }
                    }
                }
                else
                {
                    var repo = log4net.LogManager.GetRepository();
                    if (repo != null)
                    {
                        var realKey = Option;
                        foreach (var key in repo.Properties.GetKeys())
                        {
                            if (Path.DirectorySeparatorChar == '/' && key == "UNIX:" + Option)
                            {
                                realKey = "UNIX:" + Option;
                            }
                            if (Path.DirectorySeparatorChar == '\\' && key == "WINDOWS:" + Option)
                            {
                                realKey = "WINDOWS:" + Option;
                            }
                        }

                        var val = repo.Properties[realKey];
                        if (val is PatternString)
                        {
                            ((PatternString)val).ActivateOptions();
                            ((PatternString)val).Format(writer);
                        }
                        else if (val != null)
                        {
                            result = val.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    result = result.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                    writer.Write(result);
                }
            }
            catch (Exception err)
            {
                LogLog.Error(GetType(), "Can not convert " + Option, err);
            }
        }
    }
}
