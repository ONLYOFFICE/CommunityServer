/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Web.Files.Controls;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
//using ASC.Web.Studio.UserControls.Common.DocumentsPopup;

namespace ASC.Web.Mail.Controls
{
  public partial class MailBox : System.Web.UI.UserControl
  {
    public static string Location { get { return "~/addons/mail/Controls/MailBox/MailBox.ascx"; } }
    public const int EntryCountOnPage_def = 25;
    public const int VisiblePageCount_def = 10;

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js",
                                 "~/UserControls/Common/ckeditor/ckeditor-connector.js",
                                 "~/Products/Files/js/common.js",
                                 "~/Products/Files/js/templatemanager.js",
                                 "~/Products/Files/js/servicemanager.js",
                                 "~/Products/Files/js/ui.js",
                                 "~/Products/Files/js/eventhandler.js",
                                 "~/Products/Files/Controls/EmptyFolder/emptyfolder.js",
                                 "~/Products/Files/Controls/FileSelector/fileselector.js",
                                 "~/Products/Files/Controls/Tree/tree.js"
            )
            .RegisterStyle("~/Products/Files/Controls/FileSelector/fileselector.css",
                           "~/Products/Files/Controls/ThirdParty/thirdparty.css",
                           "~/Products/Files/Controls/ContentList/contentlist.css",
                           "~/Products/Files/Controls/EmptyFolder/emptyfolder.css",
                           "~/Products/Files/Controls/Tree/tree.css"
            );

        ControlPlaceHolder.Controls.Add(LoadControl(Studio.UserControls.Common.MediaPlayer.Location));

        TagsPageHolder.Controls.Add(LoadControl(TagsPage.Location) as TagsPage);
        TagsPageHolder.Controls.Add(LoadControl(UserFoldersPage.Location) as UserFoldersPage);
        TagsPageHolder.Controls.Add(LoadControl(FiltersPage.Location) as FiltersPage);
        TagsPageHolder.Controls.Add(LoadControl(AccountsPage.Location) as AccountsPage);
        TagsPageHolder.Controls.Add(LoadControl(ContactsPage.Location) as ContactsPage);
        TagsPageHolder.Controls.Add(LoadControl(CommonSettingsPage.Location) as CommonSettingsPage);

        if (Configuration.Settings.IsAdministrationPageAvailable())
            TagsPageHolder.Controls.Add(LoadControl(AdministrationPage.Location) as AdministrationPage);
        //init Page Navigator
        _phPagerContent.Controls.Add(new PageNavigator
        {
            ID = "mailPageNavigator",
            CurrentPageNumber = 1,
            VisibleOnePage = false,
            EntryCount = 0,
            VisiblePageCount = VisiblePageCount_def,
            EntryCountOnPage = EntryCountOnPage_def
        });

        var documentsPopup = (DocumentsPopup)LoadControl(DocumentsPopup.Location);
        _phDocUploader.Controls.Add(documentsPopup);

        QuestionPopup.Options.IsPopup = true;

        var fileSelector = (FileSelector) LoadControl(FileSelector.Location);
        fileSelector.DialogTitle = MailResource.SelectFolderDialogTitle;
        fileSelector.OnlyFolder = true;
        fileholder.Controls.Add(fileSelector);
    }

    protected String RenderRedirectUpload()
    {
        return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=mail");
    }

    public bool IsMailPrintAvailable()
    {
        return SetupInfo.IsVisibleSettings("MailPrint");
    }
  }
}
