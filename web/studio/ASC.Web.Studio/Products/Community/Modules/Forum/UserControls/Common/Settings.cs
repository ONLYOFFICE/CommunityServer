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
using System.Web;
using ASC.Web.Core.Utility;
using System.IO;
using ASC.Forum;
using System.Collections.Generic;

namespace ASC.Web.UserControls.Forum.Common
{
    public class Settings : IDisposable
    {
        public ForumManager ForumManager { get; internal set; }

        public LinkProvider LinkProvider { get; private set; }

        public Guid ID { get; set; }

        public Guid ModuleID { get; set; }

        public Guid ImageItemID { get; set; }

        public int TopicCountOnPage { get; set; }

        public int PostCountOnPage { get; set; }

        public string ConfigPath { get; set; }

        public string UserControlsVirtualPath { get; set; }

        public string FileStoreModuleID { get; set; }

        public string ThreadParamName{get;set;}

        public string TopicParamName { get; set; }

        public string TagParamName { get; set; }

        public string ActionParamName { get; set; }
        public string PostParamName { get; set; }

        public string NewPostPageVirtualPath { get; set; }
        public string NewPostPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.NewPostPageVirtualPath);
            }
        }

        public string SearchPageVirtualPath { get; set; }
        public string SearchPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.SearchPageVirtualPath);
            }
        }
        
        public string StartPageVirtualPath { get; set; }
        public string StartPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.StartPageVirtualPath);
            }
        }

        public string TopicPageVirtualPath { get; set; }
        public string TopicPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.TopicPageVirtualPath);
            }
        }

        public string EditTopicPageVirtualPath { get; set; }
        public string EditTopicPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.EditTopicPageVirtualPath);
            }
        }

        public string PostPageVirtualPath { get; set; }
        public string PostPageAbsolutePath
        {
            get
            {
                return GetAbsolutePathWithParams(this.PostPageVirtualPath);
            }
        }

        private string GetAbsolutePathWithParams(string virtualPath)
        {
            if (!String.IsNullOrEmpty(virtualPath))
            {
                if(virtualPath.IndexOf("?")!=-1)
                    return VirtualPathUtility.ToAbsolute(virtualPath.Split('?')[0]) + "?" + virtualPath.Split('?')[1];
                else
                    return VirtualPathUtility.ToAbsolute(virtualPath) + "?";
            }

            return String.Empty;
        }

        internal string[] GetPageAdditionalParams(string url)
        {
            if (!String.IsNullOrEmpty(url) && url.IndexOf("?") != -1)
            {
                string query = url.Split('?')[1];
                if (!String.IsNullOrEmpty(query))
                {
                    List<string> result = new List<string>(); 
                    foreach (var param in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))                    
                        result.Add(param.Split('=')[0]);

                    return result.ToArray();
                }
            }

            return new string[0];
        }

        internal string[] GetAllPageAdditionalParams()
        {
            List<string> result = new List<string>();

            result.AddRange(GetPageAdditionalParams(this.PostPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(this.TopicPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(this.SearchPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(this.SearchPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(this.NewPostPageVirtualPath));
            result.AddRange(GetPageAdditionalParams(this.EditTopicPageVirtualPath));

            return result.ToArray();
        }

        public Settings()
        {
            this.ID = Guid.NewGuid();
            this.TopicCountOnPage = 20;
            this.PostCountOnPage = 20;

            this.ThreadParamName = "f";
            this.TopicParamName = "t";
            this.TagParamName = "tag";
            this.ActionParamName = "a";
            this.PostParamName = "m";            
            this.LinkProvider = new LinkProvider(this);
            //registry
            this.ForumManager = new ForumManager(this);
            ForumManager.RegistrySettings(this);
        }

        #region IDisposable Members

        public void Dispose()
        {
            ForumManager.UnRegistrySettings(this.ID);
        }

        #endregion
    }
}
