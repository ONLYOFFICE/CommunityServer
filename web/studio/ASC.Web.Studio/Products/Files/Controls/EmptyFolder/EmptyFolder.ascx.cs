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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Core.Mobile;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Files.Controls
{
    public partial class EmptyFolder : UserControl
    {
        #region Property

        public static string Location
        {
            get { return PathProvider.GetFileControlPath("EmptyFolder/EmptyFolder.ascx"); }
        }

        public bool AllContainers { get; set; }

        public bool HideAddActions { get; set; }

        #endregion

        #region Members

        protected string ExtsWebPreviewed = string.Join(", ", FileUtility.ExtsWebPreviewed.ToArray());
        protected string ExtsWebEdited = string.Join(", ", FileUtility.ExtsWebEdited.ToArray());

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/products/files/controls/emptyfolder/emptyfolder.js");
            Page.RegisterStyle(FilesLinkUtility.FilesBaseAbsolutePath + "controls/emptyfolder/emptyfolder.css");

            var isMobile = MobileDetector.IsMobile;
            var currUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var isVisitor = currUser.IsVisitor();
            var isOutsider = currUser.IsOutsider();

            var strCreateFile =
                !HideAddActions && !isMobile && !isVisitor
                    ? string.Format(@"<div class=""empty-folder-create empty-folder-create-editor"">
<a class=""link dotline plus empty-folder-create-document"">{0}</a>,
<a class=""link dotline empty-folder-create-spreadsheet"">{1}</a>,
<a class=""link dotline empty-folder-create-presentation"">{2}</a>
</div>",
                                    FilesUCResource.ButtonCreateText,
                                    FilesUCResource.ButtonCreateSpreadsheet,
                                    FilesUCResource.ButtonCreatePresentation)
                    : string.Empty;

            var strCreateFolder =
                !HideAddActions && !isMobile && !isOutsider
                    ? string.Format(@"<div class=""empty-folder-create""><a class=""empty-folder-create-folder link dotline plus"">{0}</a></div>", FilesUCResource.ButtonCreateFolder)
                    : string.Empty;

            var strDragDrop =
                !HideAddActions
                    ? string.Format("<div class=\"emptyContainer_dragDrop\" > {0}</div>", FilesUCResource.EmptyScreenDescrDragDrop.HtmlEncode())
                    : string.Empty;

            var strToParent = string.Format("<div><a class=\"empty-folder-toparent link dotline up\" >{0}</a></div>", FilesUCResource.ButtonToParentFolder);

            if (AllContainers)
            {
                //my
                if (!isVisitor)
                {
                    var myButton = new StringBuilder();
                    myButton.Append(strCreateFile);
                    myButton.Append(strCreateFolder);
                    myButton.Append(strToParent);

                    var descrMy = string.Format(FileUtility.ExtsWebEdited.Any() ? FilesUCResource.EmptyScreenDescrMy.HtmlEncode() : FilesUCResource.EmptyScreenDescrMyPoor.HtmlEncode(),
                                                //create
                                                "<span class=\"hintCreate baseLinkAction\" >", "</span>",
                                                //upload
                                                "<span class=\"hintUpload baseLinkAction\" >", "</span>",
                                                //open
                                                "<span class=\"hintOpen baseLinkAction\" >", "</span>",
                                                //edit
                                                "<span class=\"hintEdit baseLinkAction\" >", "</span>"
                        );
                    descrMy += strDragDrop;

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_my",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_my.png"),
                            Header = FilesUCResource.MyFiles,
                            HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                            Describe = descrMy,
                            ButtonHTML = myButton.ToString()
                        });
                }

                if (!CoreContext.Configuration.Personal)
                {
                    //forme
                    var formeButton = new StringBuilder();
                    formeButton.Append(strCreateFile);
                    formeButton.Append(strCreateFolder);
                    formeButton.Append(strToParent);

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_forme",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_forme.png"),
                            Header = FilesUCResource.SharedForMe,
                            HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                            Describe = FilesUCResource.EmptyScreenDescrForme.HtmlEncode(),
                            ButtonHTML = formeButton.ToString()
                        });

                    //corporate
                    var corporateButton = new StringBuilder();
                    corporateButton.Append(strCreateFile);
                    corporateButton.Append(strCreateFolder);
                    corporateButton.Append(strToParent);

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_corporate",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_corporate.png"),
                            Header = FilesUCResource.CorporateFiles,
                            HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                            Describe = FilesUCResource.EmptyScreenDescrCorporate.HtmlEncode() + strDragDrop,
                            ButtonHTML = corporateButton.ToString()
                        });
                }

                var strGotoMy = !isVisitor ? string.Format("<div><a href=\"#{1}\" class=\"empty-folder-goto link dotline up\">{0}</a></div>", FilesUCResource.ButtonGotoMy, Global.FolderMy) : string.Empty;
                //trash
                EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_trash",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_trash.png"),
                        Header = FilesUCResource.Trash,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrTrash.HtmlEncode(),
                        ButtonHTML = strGotoMy
                    });
            }

            if (!CoreContext.Configuration.Personal)
            {
                //project
                var projectButton = new StringBuilder();
                projectButton.Append(strCreateFile);
                projectButton.Append(strCreateFolder);
                projectButton.Append(strToParent);

                EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_project",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_project.png"),
                        Header = FilesUCResource.ProjectFiles,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrProject.HtmlEncode() + strDragDrop,
                        ButtonHTML = projectButton.ToString()
                    });
            }

            //Filter
            EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                {
                    ID = "emptyContainer_filter",
                    ImgSrc = PathProvider.GetImagePath("empty_screen_filter.png"),
                    Header = FilesUCResource.Filter,
                    HeaderDescribe = FilesUCResource.EmptyScreenFilter,
                    Describe = FilesUCResource.EmptyScreenFilterDescr.HtmlEncode(),
                    ButtonHTML = string.Format("<a id=\"files_clearFilter\" class=\"clearFilterButton link dotline\" >{0}</a>",
                                               FilesUCResource.ButtonClearFilter)
                });
        }
    }
}