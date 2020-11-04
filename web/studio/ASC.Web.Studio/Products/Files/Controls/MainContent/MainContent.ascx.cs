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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Studio.UserControls.EmptyScreens;
using ASC.Web.Studio.UserControls.Management;
using ASC.Core;

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

        protected bool IsFirstVisit;

        public void Page_Init(object sender, EventArgs e)
        {
            var mail = WebItemManager.Instance[WebItemManager.MailProductID];

            ProductMailAvailable = mail != null && !mail.IsDisabled();

            IsFirstVisit = Global.IsFirstVisit();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            InitScripts();
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

            if (IsFirstVisit && !CoreContext.Configuration.Personal && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor() && (Page is _Default))
            {
                ControlPlaceHolder.Controls.Add(LoadControl(FilesDashboardEmptyScreen.Location));
            }
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