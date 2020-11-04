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
using System.Web;
using System.Web.Hosting;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.Community.Wiki.Common
{
    public enum ActionOnPage
    {
        None = 0,
        AddNew,
        AddNewFile,
        Edit,
        View,
        CategoryView,
        CategoryEdit
    }

    public class WikiBasePage : MainPage
    {
        protected WikiEngine Wiki
        {
            get { return new WikiEngine(); }
        }

        private bool? _isFile;

        public bool IsFile
        {
            get { return _isFile != null && _isFile.Value; }
        }

        protected string ConfigLocation
        {
            get { return HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig; }
        }

        protected WikiSection _wikiSection = null;

        protected WikiSection PageWikiSection
        {
            get
            {
                if (_wikiSection == null)
                {
                    _wikiSection = WikiSection.Section;
                }
                return _wikiSection;
            }
        }

        protected string _rootPath = null;

        protected string RootPath
        {
            get { return _rootPath ?? (_rootPath = MapPath("~")); }
        }

        private ActionOnPage _action = ActionOnPage.None;

        public ActionOnPage Action
        {
            get
            {
                if (_action.Equals(ActionOnPage.None))
                {
                    if (Request["action"] == null)
                    {
                        _action = ActionOnPage.View;
                    }
                    else
                    {
                        if (Request["action"].Equals("edit", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _action = ActionOnPage.Edit;
                        }
                        else if (Request["action"].Equals("newfile", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _action = ActionOnPage.AddNewFile;
                        }
                        else
                        {
                            _action = ActionOnPage.AddNew;
                        }
                    }

                    if (_action == ActionOnPage.View || _action == ActionOnPage.Edit)
                    {
                        if (WikiPage.StartsWith(UserControls.Wiki.Constants.WikiCategoryKeyCaption, StringComparison.InvariantCultureIgnoreCase) && PageNameUtil.Decode(WikiPage).Contains(":"))
                        {
                            _action = _action == ActionOnPage.View
                                          ? ActionOnPage.CategoryView
                                          : ActionOnPage.CategoryEdit;
                        }
                    }
                }

                return _action;
            }
        }

        private string _wikiPage;

        public string WikiPage
        {
            get
            {
                if (_wikiPage == null)
                {
                    _isFile = false;
                    if (string.IsNullOrEmpty(Request["page"]))
                    {
                        _wikiPage = string.Empty;
                        if (!string.IsNullOrEmpty(Request["file"]))
                        {
                            _isFile = true;
                            _wikiPage = Request["file"];
                        }
                    }
                    else
                    {
                        _wikiPage = Request["page"];
                    }
                }

                return _wikiPage;
            }
        }

        protected WikiMaster WikiMaster
        {
            get { return (WikiMaster)Master; }
        }

        protected int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Title = HeaderStringHelper.GetPageTitle(WikiMaster.CurrentPageCaption ?? WikiResource.ModuleName);
        }

        protected string GetPageInfo(string name, Guid userID, DateTime date)
        {
            return string.Format(WikiResource.wikiPageInfoFormat,
                                 string.Format("<span class=\"splitter\"></span>{0}<span class=\"splitter\"></span>", CoreContext.UserManager.GetUsers(userID).RenderCustomProfileLink("", "linkMedium")),
                                 string.Format("<span class=\"splitter\"></span>{0} {1}", date.ToString("t"), date.ToString("d")));
        }

        protected string ProcessVersionInfo(string name, Guid userID, DateTime date, int version, bool isFile, bool versionIsLink)
        {
            return string.Format(@"{0}&nbsp;{2}&nbsp;{1}",
                                 CoreContext.UserManager.GetUsers(userID).RenderCustomProfileLink("describe-text", "linkMedium gray-text"),
                                 date.AgoSentence(),
                                 isFile ? string.Empty :
                                     versionIsLink ?
                                         string.Format(@"<a href=""{0}?page={3}"">{2}{1}</a>&nbsp;",
                                                       this.ResolveUrlLC("PageHistoryList.aspx"),
                                                       version,
                                                       WikiResource.wikiVersionCaption,
                                                       name) :
                                         string.Format(@"{1}{0}&nbsp;",
                                                       version,
                                                       WikiResource.wikiVersionCaption)
                );
        }

        protected string GetPageName(Page page)
        {
            return string.IsNullOrEmpty(page.PageName)
                       ? WikiResource.MainWikiCaption
                       : HttpUtility.HtmlEncode(page.PageName);
        }

        protected string GetPageViewLink(Page page)
        {
            return ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), HttpUtility.HtmlDecode(page.PageName));
        }

        protected string ProcessVersionInfo(string name, Guid userID, DateTime date, int version, bool isFile)
        {
            return ProcessVersionInfo(name, userID, date, version, isFile, true);
        }

        protected static string GetFileLengthToString(long length)
        {
            return FileSizeComment.FilesSizeToString(length);
        }
    }
}