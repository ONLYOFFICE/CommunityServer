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