/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;
using Microsoft.Practices.ServiceLocation;

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

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            InitScripts();
        }

        private void InitControls()
        {
            confirmRemoveDialog.Options.IsPopup = true;
            confirmOverwriteDialog.Options.IsPopup = true;

            var contenList = (ContentList)LoadControl(ContentList.Location);
            contenList.FolderIDCurrentRoot = FolderIDCurrentRoot;
            ListHolder.Controls.Add(contenList);

            if (TenantExtra.GetTenantQuota().DocsEdition)
                ControlPlaceHolder.Controls.Add(LoadControl(ConvertFile.Location));

            ControlPlaceHolder.Controls.Add(LoadControl(TariffLimitExceed.Location));

            if (FileUtility.ExtsImagePreviewed.Count != 0)
                ControlPlaceHolder.Controls.Add(LoadControl(FileViewer.Location));

            UploaderPlaceHolder.Controls.Add(LoadControl(ChunkUploadDialog.Location));

            if (CoreContext.Configuration.Personal)
                ControlPlaceHolder.Controls.Add(LoadControl(MoreFeatures.Location));
        }

        private void InitScripts()
        {
            string tasksStatuses;
            
            if (!GetTasksStatuses(out tasksStatuses))
                return;

            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("ASC.Files.EventHandler.onGetTasksStatuses({0}, {{doNow: true}});", tasksStatuses);

            Page.RegisterInlineScript(inlineScript.ToString(), onReady: false);
        }

        private static bool GetTasksStatuses(out string taskStatuses)
        {
            taskStatuses = null;

            List<FileOperationResult> tasks;
            try
            {
                var docService = ServiceLocator.Current.GetInstance<IFileStorageService>();
                tasks = docService.GetTasksStatuses();
            }
            catch
            {
                return false;
            }
            if (tasks.Count == 0) return false;

            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(ItemList<FileOperationResult>));
                serializer.WriteObject(ms, tasks);
                ms.Seek(0, SeekOrigin.Begin);

                taskStatuses= Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                return true;
            }
        }
    }
}