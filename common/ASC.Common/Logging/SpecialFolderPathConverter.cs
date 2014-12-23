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
                if ("SystemLogFolder".Equals(Option, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = Path.DirectorySeparatorChar == '/' ? "/var/log/" : @"..\Logs\";
                }
                else if ("ParentFolder".Equals(Option, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        result = new DirectoryInfo(Path.DirectorySeparatorChar == '/' ?
                            AppDomain.CurrentDomain.BaseDirectory : ".").Name + Path.DirectorySeparatorChar;
                    }
                    catch
                    {
                        result = new DirectoryInfo(Path.DirectorySeparatorChar == '/' ?
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : ".").Name + Path.DirectorySeparatorChar;
                    }
                }
                else
                {
                    var repo = LogManager.GetRepository();
                    if (repo != null && repo.Properties.Contains(Option))
                    {
                        var val = repo.Properties[Option];
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
                    else
                    {
                        var sf = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), Option, true);
                        result = Environment.GetFolderPath(sf);
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
                LogLog.Error("Can not convert " + Option, err);
            }
        }
    }
}
