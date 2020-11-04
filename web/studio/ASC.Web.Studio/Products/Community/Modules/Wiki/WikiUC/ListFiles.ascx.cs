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
using System.Web.UI.WebControls;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ListFiles : BaseUserControl
    {
        public string mainPath
        {
            get
            {
                if (ViewState["mainPath"] == null)
                    return string.Empty;
                else
                    return ViewState["mainPath"].ToString();
            }
            set { ViewState["mainPath"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindListFiles();
            }
        }

        protected void cmdDeleteFile_Click(object sender, EventArgs e)
        {
            string fileName = (sender as LinkButton).CommandName;
            Wiki.RemoveFile(fileName);
            BindListFiles();
        }

        protected string GetFileLink(string FileName)
        {
            return ActionHelper.GetViewFilePath(mainPath, FileName);
        }


        private void BindListFiles()
        {
            rptListFiles.DataSource = Wiki.GetFiles();
            rptListFiles.DataBind();
        }     
    }
}