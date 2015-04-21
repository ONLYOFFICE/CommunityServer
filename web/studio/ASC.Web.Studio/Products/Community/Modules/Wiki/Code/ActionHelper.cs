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


using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.UserControls.Wiki.Handlers;

namespace ASC.Web.UserControls.Wiki
{
    public class ActionHelper
    {
        public static string GetAddPagePath(string mainPath)
        {
            return string.Format("{0}?action=New", mainPath);
        }

        public static string GetAddFilePath(string mainPath)
        {
            return string.Format("{0}?action=NewFile", mainPath);
        }

        public static string GetEditPagePath(string mainPath, string pageName)
        {
            return string.Format("{0}?page={1}&action=Edit", mainPath, PageNameUtil.Encode(pageName));
        }

        public static string GetViewPagePath(string mainPath, string pageName, string spetial)
        {
            return string.Format("{0}?page={1}", mainPath, string.IsNullOrEmpty(spetial) ? PageNameUtil.Encode(pageName) : string.Format(@"{0}:{1}", spetial, PageNameUtil.Encode(pageName)));
        }
        public static string GetViewPagePath(string mainPath, string pageName)
        {
			if (pageName == null)
			{
				return mainPath;
			}
            return string.Format("{0}?page={1}", mainPath, PageNameUtil.Encode(pageName));
        }

        public static string GetViewPagePathWithVersion(string mainPath, string pageName, int version)
        {
            return string.Format("{0}?page={1}&ver={2}", mainPath, PageNameUtil.Encode(pageName), version);
        }

        public static string GetEditFilePath(string mainPath, string pageName)
        {
            return string.Format("{0}?file={1}&action=Edit", mainPath, PageNameUtil.Encode(pageName));
        }

        public static string GetViewFilePath(string mainPath, string pageName)
        {
            return string.Format("{0}?file={1}", mainPath, pageName);
        }
    }
}
