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


using System.Web;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Projects
{
    public partial class Contacts : BasePage
    {
        public bool CanLinkContact { get; set; }

        protected override bool CanRead
        {
            get
            {
                var crmEnabled = WebItemManager.Instance[WebItemManager.CRMProductID];

                return crmEnabled != null && !crmEnabled.IsDisabled() && ProjectSecurity.CanReadContacts(Project);
            }
        }

        protected override void PageLoad()
        {
            CanLinkContact = ProjectSecurity.CanLinkContact(Project);

            var button = "";

            if(CanLinkContact)
            {
                button = "<a class='link-with-entity link dotline'>" + ProjectsCommonResource.EmptyScreenContactsButton + "</a>";
            }

            var escNoContacts = new Studio.Controls.Common.EmptyScreenControl
            {
                Header = ProjectsCommonResource.EmptyScreenContasctsHeader,
                ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_persons.png", ProductEntryPoint.ID),
                Describe = ProjectsCommonResource.EmptyScreenContactsDescribe,
                ID = "escNoContacts",
                ButtonHTML = button,
                CssClass = "display-none"
            };
            emptyScreen.Controls.Add(escNoContacts);

            Page.Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.ModuleContacts);

            Master.RegisterCRMResources();
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath, "contacts.js");
        }
    }
}
