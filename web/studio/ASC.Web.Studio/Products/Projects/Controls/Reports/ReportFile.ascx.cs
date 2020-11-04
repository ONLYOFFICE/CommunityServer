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
using System.Web.UI;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Attachments;

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportFile : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var discussionFilesControl = (Attachments)LoadControl(Attachments.Location);
            discussionFilesControl.EmptyScreenVisible = false;
            discussionFilesControl.ModuleName = "projects";
            discussionFilesControl.DocUploaderHolder.Controls.Add(LoadControl(ProjectDocumentsPopup.Location));
            discussionFilesControl.CanAddFile = false;
            phAttachmentsControl.Controls.Add(discussionFilesControl);
        }
    }
}