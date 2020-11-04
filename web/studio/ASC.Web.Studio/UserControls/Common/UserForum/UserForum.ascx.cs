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


using ASC.Web.Core.WhiteLabel;
using System;
using System.Globalization;
using System.Web.UI;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.UserForum
{
    public partial class UserForum : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/UserForum/UserForum.ascx"; }
        }

        protected String UserForumLink
        {
            get
            {
                var settings = AdditionalWhiteLabelSettings.Instance;

                if (!settings.UserForumEnabled || String.IsNullOrEmpty(settings.UserForumUrl))
                    return null;

                return CommonLinkUtility.GetRegionalUrl(settings.UserForumUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            }
        }        

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}