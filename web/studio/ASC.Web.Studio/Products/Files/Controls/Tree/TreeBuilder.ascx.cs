/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web.UI;

using ASC.Files.Core;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Controls
{
    public partial class TreeBuilder : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("Tree/TreeBuilder.ascx"); }
        }

        public object FolderIDCurrentRoot { get; set; }
        public Folder ExternalFolder;

        protected string AdditionalCssClass
        {
            get { return FolderIDCurrentRoot != null ? "jstree-inprojects " : string.Empty; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (FolderIDCurrentRoot == null)
            {
                var treeViewContainer = (Tree)LoadControl(Tree.Location);
                treeViewContainer.ID = "treeViewContainer";
                treeViewContainer.AdditionalCssClass = AdditionalCssClass;
                treeViewContainer.WithNew = true;
                treeViewContainer.ExternalFolder = ExternalFolder;
                TreeViewHolder.Controls.Add(treeViewContainer);
            }

            var treeSelectorContainer = (Tree)LoadControl(Tree.Location);
            treeSelectorContainer.ID = "treeViewSelector";
            treeSelectorContainer.FolderIDCurrentRoot = FolderIDCurrentRoot;
            treeSelectorContainer.WithoutTrash = true;
            treeSelectorContainer.WithoutAdditionalFolder = true;
            treeSelectorContainer.ExternalFolder = ExternalFolder;
            TreeSelectorHolder.Controls.Add(treeSelectorContainer);
        }
    }
}