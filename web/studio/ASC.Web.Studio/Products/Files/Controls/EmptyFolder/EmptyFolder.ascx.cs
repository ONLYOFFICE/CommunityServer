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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

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
            var currUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var isVisitor = currUser.IsVisitor();
            var isOutsider = currUser.IsOutsider();
            var isAdmin = currUser.IsAdmin();

            var strCreateFile =
                !HideAddActions && !isVisitor
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
                !HideAddActions && !isOutsider
                    ? string.Format(@"<div class=""empty-folder-create""><a class=""empty-folder-create-folder link dotline plus"">{0}</a></div>", FilesUCResource.ButtonCreateFolder)
                    : string.Empty;

            var strDragDrop =
                !HideAddActions
                    ? string.Format("<div class=\"emptyContainer_dragDrop\" > {0}</div>", FilesUCResource.EmptyScreenDescrDragDrop.HtmlEncode())
                    : string.Empty;

            var strToParent = string.Format("<div><a class=\"empty-folder-toparent link dotline up\" >{0}</a></div>", FilesUCResource.ButtonToParentFolder);

            var strButtons = new StringBuilder();
            strButtons.Append(strCreateFile);
            strButtons.Append(strCreateFolder);

            if (AllContainers)
            {
                //my
                if (!isVisitor)
                {
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
                            ButtonHTML = strButtons.ToString()
                        });

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_recent",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_recent.png"),
                        Header = FilesUCResource.Recent,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrRecent.HtmlEncode()
                    });

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_favorites",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_favorites.png"),
                        Header = FilesUCResource.Favorites,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrFavorites.HtmlEncode()
                    });

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_templates",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_templates.png"),
                        Header = FilesUCResource.Templates,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrTemplates.HtmlEncode()
                    });

                    if (PrivacyRoomSettings.Available)
                    {
                        var privacyWithButton = false;
                        var privacyDescr = string.Format(FilesUCResource.EmptyScreenDescrPrivacyList,
                            "<div class=\"empty-folder-privacy-li\">",
                            "</div>"
                            );
                        if (!PrivacyRoomSettings.Enabled)
                        {
                            if (isAdmin)
                            {
                                var link =
                                    TenantExtra.EnableControlPanel
                                    ? SetupInfo.ControlPanelUrl.TrimEnd('/') + "/privacyRoom"
                                    : CommonLinkUtility.GetAdministration(ManagementType.PrivacyRoom);

                                privacyDescr += string.Format("<p>" + FilesUCResource.EmptyScreenDescrPrivacyEnable + "</p>",
                                    "<a href=\"" + CommonLinkUtility.GetFullAbsolutePath(link) + "\" "
                                        + (Request.DesktopApp() ? "target=\"_blank\"" : "")
                                        + ">",
                                    "</a>"
                                    );
                            }
                            else
                            {
                                privacyDescr += "<p>" + FilesUCResource.EmptyScreenDescrPrivacyAsk + "</p>";
                            }
                        }
                        else if (Request.DesktopApp())
                        {
                            privacyWithButton = true;
                        }

                        privacyDescr += string.Format("<p id=\"privacyForDesktop\">" + FilesUCResource.EmptyScreenDescrPrivacyBrowser.HtmlEncode() + "</p>",
                            "<a href=\"https://www.onlyoffice.com/private-rooms.aspx\" target=\"_blank\">",
                            "</a>"
                            );

                        EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_privacy",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_privacy.png"),
                            Header = FilesUCResource.PrivacyRoomEmptyFolder.HtmlEncode(),
                            Describe = privacyDescr,
                            ButtonHTML = privacyWithButton ? strButtons.ToString() : ""
                        });
                    }
                }

                if (!CoreContext.Configuration.Personal)
                {
                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_forme",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_forme.png"),
                            Header = FilesUCResource.SharedForMe,
                            HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                            Describe = FilesUCResource.EmptyScreenDescrForme.HtmlEncode()
                        });

                    EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                        {
                            ID = "emptyContainer_corporate",
                            ImgSrc = PathProvider.GetImagePath("empty_screen_corporate.png"),
                            Header = FilesUCResource.CorporateFiles,
                            HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                            Describe = FilesUCResource.EmptyScreenDescrCorporate.HtmlEncode() + strDragDrop,
                            ButtonHTML = isAdmin ? strButtons.ToString() : string.Empty
                        });
                }

                //trash
                EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_trash",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_trash.png"),
                        Header = FilesUCResource.Trash,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = FilesUCResource.EmptyScreenDescrTrash.HtmlEncode(),
                        ButtonHTML = !isVisitor ? string.Format("<div><a href=\"#{1}\" class=\"empty-folder-goto link dotline up\">{0}</a></div>", FilesUCResource.ButtonGotoMy, Global.FolderMy) : string.Empty
                    });
            }

            if (!CoreContext.Configuration.Personal)
            {
                EmptyScreenFolder.Controls.Add(new EmptyScreenControl
                    {
                        ID = "emptyContainer_project",
                        ImgSrc = PathProvider.GetImagePath("empty_screen_project.png"),
                        Header = FilesUCResource.ProjectFiles,
                        HeaderDescribe = FilesUCResource.EmptyScreenHeader,
                        Describe = (!isVisitor
                                        ? FilesUCResource.EmptyScreenDescrProject.HtmlEncode()
                                        : FilesUCResource.EmptyScreenDescrProjectVisitor.HtmlEncode())
                                   + strDragDrop
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
                    ButtonHTML = string.Format("<a class=\"clearFilterButton link dotline files-clear-filter\" >{0}</a>", FilesUCResource.ButtonClearFilter)
                });

            strButtons.Append(strToParent);

            //Subfolder
            EmptyScreenFolder.Controls.Add(new EmptyScreenControl
            {
                ID = "emptyContainer_subfolder",
                ImgSrc = PathProvider.GetImagePath("empty_screen.png"),
                HeaderDescribe = FilesUCResource.EmptyScreenFolderHeader,
                ButtonHTML = strButtons.ToString()
            });
        }
    }
}
