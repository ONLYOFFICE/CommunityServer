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


using System.IO;
using System.Web.UI;
using ASC.Web.Sample.Classes;
using System;
using System.Web;

namespace ASC.Web.Sample.Controls
{
    public partial class NavigationSidePanel : UserControl
    {
        public static string Location
        {
            get
            {
                return PathProvider.GetFileStaticRelativePath("NavigationSidePanel.ascx");
            }
        }

        protected string CurrentPage { get; set; }
        protected string ExceptionMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var page = HttpContext.Current.Request.CurrentExecutionFilePath;

                if (!string.IsNullOrEmpty(page))
                    page = Path.GetFileNameWithoutExtension(page);

                CurrentPage = (page ?? string.Empty).ToLowerInvariant();
            }
            catch(Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
            finally
            {
                if (string.IsNullOrEmpty(CurrentPage))
                    CurrentPage = "default";
            }
        }
    }
}