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
using System.Web;
using System.Web.UI;
using ASC.Web.Core.Files;
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

        protected string AdditionalCssClass
        {
            get
            {
                return FolderIDCurrentRoot != null ? "jstree-inprojects " : string.Empty;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/files/controls/tree/treebuilder.js"));

            Page.RegisterStyleControl(FilesLinkUtility.FilesBaseAbsolutePath + "controls/tree/treebuilder.css");

            var treeViewContainer = (Tree)LoadControl(Tree.Location);
            treeViewContainer.ID = "treeViewContainer";
            treeViewContainer.AdditionalCssClass = AdditionalCssClass;
            treeViewContainer.WithNew = true;
            treeViewContainer.WithoutBaseScripts = true;
            TreeViewHolder.Controls.Add(treeViewContainer);

            var treeSelectorContainer = (Tree)LoadControl(Tree.Location);
            treeSelectorContainer.ID = "treeViewSelector";
            treeSelectorContainer.FolderIDCurrentRoot = FolderIDCurrentRoot;
            treeSelectorContainer.WithoutTrash = true;
            treeSelectorContainer.WithoutBaseScripts = true;
            TreeSelectorHolder.Controls.Add(treeSelectorContainer);
        }
    }
}