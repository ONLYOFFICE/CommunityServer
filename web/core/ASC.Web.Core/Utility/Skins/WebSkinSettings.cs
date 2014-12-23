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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using ASC.Data.Storage;

namespace ASC.Web.Core.Utility.Skins
{
    public class WebSkin
    {
        public static string BaseCSSFileAbsoluteWebPath
        {
            get { return WebPath.GetPath("/skins/default/common_style.css"); }
        }


        private static readonly HashSet<string> BaseCultureCss = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static bool HasCurrentCultureCssFile
        {
            get { return BaseCultureCss.Contains(CultureInfo.CurrentCulture.Name); }
        }

        static WebSkin()
        {
            if (HttpContext.Current == null) return;

            try
            {
                var dir = HttpContext.Current.Server.MapPath("~/skins/default/");
                if (!Directory.Exists(dir)) return;

                foreach (var f in Directory.GetFiles(dir, "common_style.*.css"))
                {
                    BaseCultureCss.Add(Path.GetFileName(f).Split('.')[1]);
                }
            }
            catch
            {
            }
        }
    }
}