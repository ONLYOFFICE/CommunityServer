/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using log4net;
using log4net.Util;
using System;
using System.IO;
using System.Reflection;

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
                    var repo = LogManager.GetRepository();
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
