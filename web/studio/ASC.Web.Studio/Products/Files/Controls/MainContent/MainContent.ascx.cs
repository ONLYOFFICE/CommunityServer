/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Web.Files.Controls
{
    public partial class MainContent : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("MainContent/MainContent.ascx"); }
        }

        public object FolderIDCurrentRoot { get; set; }

        public String TitlePage { get; set; }

        public bool NoMediaViewers { get; set; }

        protected bool ProductMailAvailable;

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            InitScripts();

            var mail = WebItemManager.Instance[WebItemManager.MailProductID];
            ProductMailAvailable = mail != null && !mail.IsDisabled();
        }

        private void InitControls()
        {
            confirmRemoveDialog.Options.IsPopup = true;
            confirmOverwriteDialog.Options.IsPopup = true;
            changeOwnerDialog.Options.IsPopup = true;

            var contenList = (ContentList)LoadControl(ContentList.Location);
            contenList.FolderIDCurrentRoot = FolderIDCurrentRoot;
            ListHolder.Controls.Add(contenList);

            ControlPlaceHolder.Controls.Add(LoadControl(ConvertFile.Location));
            ControlPlaceHolder.Controls.Add(LoadControl(ConfirmConvert.Location));

            ControlPlaceHolder.Controls.Add(LoadControl(TariffLimitExceed.Location));

            if (!NoMediaViewers)
            {
                ControlPlaceHolder.Controls.Add(LoadControl(Studio.UserControls.Common.MediaPlayer.Location));
            }

            UploaderPlaceHolder.Controls.Add(LoadControl(ChunkUploadDialog.Location));
        }

        private void InitScripts()
        {
            string tasksStatuses;

            if (!GetTasksStatuses(out tasksStatuses))
                return;

            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("ASC.Files.EventHandler.onGetTasksStatuses({0}, {{doNow: true}});", tasksStatuses);

            Page.RegisterInlineScript(inlineScript.ToString());
        }

        private static bool GetTasksStatuses(out string taskStatuses)
        {
            taskStatuses = null;

            List<FileOperationResult> tasks;
            try
            {
                tasks = Global.FileStorageService.GetTasksStatuses();
            }
            catch (Exception err)
            {
                Global.Logger.Error(err);
                return false;
            }
            if (tasks.Count == 0) return false;

            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof (ItemList<FileOperationResult>));
                serializer.WriteObject(ms, tasks);
                ms.Seek(0, SeekOrigin.Begin);

                taskStatuses = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                return true;
            }
        }
    }
}