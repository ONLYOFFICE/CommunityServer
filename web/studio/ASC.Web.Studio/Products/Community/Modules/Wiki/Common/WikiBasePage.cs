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