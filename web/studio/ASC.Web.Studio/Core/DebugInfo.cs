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
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using ASC.Core;

namespace ASC.Web.Studio.Core
{
    public  class DebugInfo
    {
        private static readonly DateTime compileDateTime;

        public static bool ShowDebugInfo
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static string DebugString
        {
            get
            {
                if (HttpContext.Current == null) return "Unknown (HttpContext is null)";

                var pathToRoot = HttpContext.Current.Server.MapPath("~/");

                var pathToFile = String.Concat(pathToRoot, "change.log");

                if (!File.Exists(pathToFile))
                    return "File 'change.log' is not exists";

                var fileContent = File.ReadAllText(pathToFile, UnicodeEncoding.Default);

                fileContent = fileContent.Replace("{BuildDate}", compileDateTime.ToString("yyyy-MM-dd hh:mm"));
                fileContent = fileContent.Replace("{User}", SecurityContext.CurrentAccount.ToString());
                fileContent = fileContent.Replace("{UserAgent}", HttpContext.Current.Request.UserAgent);
                fileContent = fileContent.Replace("{Url}", HttpContext.Current.Request.Url.ToString());
                fileContent = fileContent.Replace("{RewritenUrl}", HttpContext.Current.Request.GetUrlRewriter().ToString());

                return fileContent;
            }
        }


        static DebugInfo()
        {
            try
            {
                const int PE_HEADER_OFFSET = 60;
                const int LINKER_TIMESTAMP_OFFSET = 8;
                var b = new byte[2048];
                using (var s = new FileStream(Assembly.GetCallingAssembly().Location, FileMode.Open, FileAccess.Read))
                {
                    s.Read(b, 0, 2048);
                }
                var i = BitConverter.ToInt32(b, PE_HEADER_OFFSET);
                var secondsSince1970 = BitConverter.ToInt32(b, i + LINKER_TIMESTAMP_OFFSET);
                compileDateTime = new DateTime(1970, 1, 1).AddSeconds(secondsSince1970);
            }
            catch { }
        }
    }
}
