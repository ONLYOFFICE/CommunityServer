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
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using System;
using System.Web.UI;

namespace ASC.Web.Files.Controls
{
    public partial class FileSelector : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("FileSelector/FileSelector.ascx"); }
        }

        public string DialogTitle = FilesUCResource.SelectFolder;

        public bool OnlyFolder;
        public bool IsFlat;
        public bool Multiple;
        public string SuccessButton = FilesUCResource.ButtonOk;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["warmup"] == "true") return;

            Page.RegisterClientScript(new Masters.ClientScripts.FilesLocalizationResources());

            FileSelectorTemp.Options.IsPopup = !IsFlat;
            FileSelectorTemp.Options.OnCancelButtonClick = "ASC.Files.FileSelector.onCancel();";

            var tree = (Tree) LoadControl(Tree.Location);
            tree.ID = "fileSelectorTree";
            tree.WithoutTrash = true;
            tree.WithoutAdditionalFolder = OnlyFolder;
            TreeHolder.Controls.Add(tree);

            if (!OnlyFolder)
            {
                var contentList = (ContentList) LoadControl(ContentList.Location);
                contentList.HideAddActions = true;
                ContentHolder.Controls.Add(contentList);
            }
        }
    }
}