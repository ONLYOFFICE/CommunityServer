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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Controls
{
    public partial class Tree : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("Tree/Tree.ascx"); }
        }

        public object FolderIDCurrentRoot { get; set; }
        public string AdditionalCssClass { get; set; }
        public bool WithoutTrash { get; set; }
        public bool WithoutAdditionalFolder { get; set; }
        public bool WithNew { get; set; }

        protected bool IsVisitor;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterClientScript(new Masters.ClientScripts.FilesConstantsResources());

            Page.RegisterStyle(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                               "App_Themes/<theme_folder>/leftmenu.less");

            if (Global.IsOutsider)
            {
                WithoutTrash = true;
                WithoutAdditionalFolder = true;
            }

            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();
        }
    }
}