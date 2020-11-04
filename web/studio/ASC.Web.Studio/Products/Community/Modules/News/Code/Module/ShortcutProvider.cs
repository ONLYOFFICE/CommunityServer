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
using System.Web;
using ASC.Core;
using ASC.Web.Core.ModuleManagement.Common;

namespace ASC.Web.Community.News.Code.Module
{
	public class ShortcutProvider : IShortcutProvider
	{

        public static string GetCreateContentPageUrl()
        {
            if(SecurityContext.CheckPermissions(NewsConst.Action_Add))
                return FeedUrls.EditNewsUrl;
            
            return null;
        }

		public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
		{
			if (shortcutID.Equals(new Guid("499FCB8B-F715-45b2-A112-E99826F4B401")))//News
			{
				return FeedUrls.EditNewsUrl;
			}
			return string.Empty;
		}

		public bool CheckPermissions(Guid shortcutID, string currentUrl)
		{
			if (shortcutID.Equals(new Guid("499FCB8B-F715-45b2-A112-E99826F4B401")))//News
			{
				return SecurityContext.CheckPermissions(NewsConst.Action_Edit);
			}
			return true;
		}
	}
}