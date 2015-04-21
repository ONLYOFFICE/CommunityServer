/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
