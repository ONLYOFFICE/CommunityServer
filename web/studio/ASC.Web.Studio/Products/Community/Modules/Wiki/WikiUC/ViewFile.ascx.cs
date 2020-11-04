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
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ViewFile : BaseUserControl
    {
        private string _fileName = string.Empty;
        public string FileName
        {
            get {
                return /*PageNameUtil.Encode*/(_fileName); }
            set { _fileName = /*PageNameUtil.Decode*/(value); }
        }


        private File _fileInfo;
        protected File CurrentFile
        {
            get
            {
                if (_fileInfo == null)
                {
                    if (string.IsNullOrEmpty(FileName))
                        return null;

                    _fileInfo = Wiki.GetFile(FileName);
                }
                return _fileInfo;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if(CurrentFile != null && !string.IsNullOrEmpty(CurrentFile.FileName))
                {
                    RiseWikiPageLoaded(CurrentFile);
                    RisePublishVersionInfo(CurrentFile);
                }
            }
        }


        protected string GetFileRender()
        {
            var file = Wiki.GetFile(_fileName);
            if(file == null)
            {               
                RisePageEmptyEvent();
                return string.Empty;// "nonefile.png";
            }

            string ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return string.Format(@"<a class=""linkHeaderMedium"" href=""{0}"" title=""{1}"">{2}</a>",
                    ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)),
                    file.FileName,
                    Resources.WikiUCResource.wikiFileDownloadCaption);
            }

            return string.Format(@"<img src=""{0}"" style=""max-width:300px; max-height:200px"" />",
                ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)));                      
        }
    }
}