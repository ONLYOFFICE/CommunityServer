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
using System.Configuration;
using System.Web;
using System.Web.UI;

using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.PersonalFooter
{
    public partial class PersonalFooter : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/PersonalFooter/PersonalFooter.ascx"; }
        }

        public static string LocationCustomMode
        {
            get { return "~/UserControls/Common/PersonalFooter/PersonalFooterCustomMode.ascx"; }
        }

        protected string HelpLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/UserControls/Common/PersonalFooter/css/personalfooter.less")
                .RegisterBodyScripts("~/UserControls/Common/PersonalFooter/js/personalfooter.js");

            HelpLink = GetHelpLink();
        }

        private static string GetHelpLink()
        {
            var baseHelpLink = CommonLinkUtility.GetHelpLink();

            if (string.IsNullOrEmpty(baseHelpLink))
                baseHelpLink = ConfigurationManagerExtension.AppSettings["web.faq-url"] ?? string.Empty;

            return baseHelpLink.TrimEnd('/');
        }
    }
}