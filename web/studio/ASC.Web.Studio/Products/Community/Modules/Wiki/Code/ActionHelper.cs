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
