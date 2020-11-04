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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AjaxPro;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Notify.Recipients;
using ASC.Web.Community.Product;
using ASC.Web.Community.Search;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.UserControls.Wiki.UC;

namespace ASC.Web.Community.Wiki
{
    [AjaxNamespace("_Default")]
    public partial class _Default : WikiBasePage, IContextInitializer
    {
        protected string WikiPageName { get; set; }
        private bool _isEmptyPage;

        protected int Version
        {
            get
            {
                int result;
                if (Request["ver"] == null || !int.TryParse(Request["ver"], out result))
                    return 0;

                return result;
            }
        }

        protected bool m_IsCategory
        {
            get { return Action == ActionOnPage.CategoryView || Action == ActionOnPage.CategoryEdit; }
        }

        private string _categoryName;

        protected string m_categoryName
        {
            get
            {
                if (_categoryName == null)
                {
                    _categoryName = string.Empty;
                    if (m_IsCategory)
                    {
                        var str = PageNameUtil.Decode(WikiPage);
                        _categoryName = str.Substring(str.IndexOf(':') + 1).Trim();
                    }
                }

                return _categoryName;
            }
        }

        protected string PrintPageName
        {
            get
            {
                var pageName = PageNameUtil.Decode(WikiPage);
                if (string.IsNullOrEmpty(pageName))
                {
                    pageName = WikiResource.MainWikiCaption;
                }
                return pageName;
            }
        }

        protected string PrintPageNameEncoded
        {
            get { return HttpUtility.HtmlEncode(PrintPageName); }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            wikiViewPage.TenantId = wikiViewFile.TenantId = wikiEditFile.TenantId = wikiEditPage.TenantId = TenantId;
        }

        private void CheckSpetialSymbols()
        {
            var spetialName = PrintPageName;
            if (!spetialName.Contains(":"))
                return;

            var spetial = spetialName.Split(':')[0];
            spetialName = spetialName.Split(':')[1];

            /*if (spetial.Equals(ASC.Web.UserControls.Wiki.Resources.WikiResource.wikiCategoryKeyCaption, StringComparison.InvariantCultureIgnoreCase))
            {
                Response.RedirectLC(string.Format("ListPages.aspx?cat={0}", spetialName.Trim()), this);
            }
            else*/
            if (spetial.Equals(UserControls.Wiki.Constants.WikiInternalKeyCaption, StringComparison.InvariantCultureIgnoreCase))
            {
                spetialName = spetialName.Trim();
                var anchors = spetialName;
                if (spetialName.Contains("#"))
                {
                    spetialName = spetialName.Split('#')[0];
                    anchors = anchors.Remove(0, spetialName.Length).TrimStart('#');
                }
                else
                {
                    anchors = string.Empty;
                }

                if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalIndexKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx", this);
                }
                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalCategoriesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListCategories.aspx", this);
                }
                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalFilesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListFiles.aspx", this);
                }

                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalHomeKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(anchors))
                    {
                        Response.RedirectLC("Default.aspx", this);
                    }
                    else
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page=#{0}", anchors), this);
                    }
                }
                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalNewPagesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx?n=", this);
                }
                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalRecentlyKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx?f=", this);
                }
                else if (spetialName.Equals(UserControls.Wiki.Constants.WikiInternalHelpKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(anchors))
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page={0}", UserControls.Wiki.Resources.WikiUCResource.HelpPageCaption), this);
                    }
                    else
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page={0}#{1}", UserControls.Wiki.Resources.WikiUCResource.HelpPageCaption, anchors), this);
                    }
                }
            }
        }

        protected void wikiEditPage_SetNewFCKMode(bool isWysiwygDefault)
        {
            WikiModuleSettings.SetIsWysiwygDefault(isWysiwygDefault);
        }

        protected string wikiEditPage_GetUserFriendlySizeFormat(long size)
        {
            return GetFileLengthToString(size);
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(WikiPage) || IsFile) return;
                var pageName = PageNameUtil.Decode(WikiPage);

                var page = Wiki.GetPage(pageName);
                CommunitySecurity.DemandPermissions(new WikiObjectsSecurityObject(page), Common.Constants.Action_RemovePage);

                foreach (var cat in Wiki.GetCategoriesRemovedWithPage(pageName))
                {
                    WikiNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(Common.Constants.AddPageToCat, cat.CategoryName);
                }

                Wiki.RemoveCategories(pageName);

                WikiNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(Common.Constants.EditPage, pageName);

                foreach (var comment in Wiki.GetComments(pageName))
                {
                    CommonControlsConfigurer.FCKUploadsRemoveForItem("wiki_comments", comment.Id.ToString());
                }
                Wiki.RemovePage(pageName);

                FactoryIndexer<WikiWrapper>.DeleteAsync(page);

                Response.RedirectLC("Default.aspx", this);
            }
            catch (Exception err)
            {
                WikiMaster.PrintInfoMessage(err.Message, InfoType.Alert);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            WikiPageName = PrintPageName;
            (Master as WikiMaster).GetDelUniqId += new WikiMaster.GetDelUniqIdHandle(_Default_GetDelUniqId);

            Utility.RegisterTypeForAjax(typeof(_Default), Page);
            RegisterInlineScript();
            LoadViews();

            if (IsPostBack) return;

            if (IsFile)
            {
                Response.RedirectLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, WikiPage, TenantId), this);
            }

            pCredits.Visible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);

            CheckSpetialSymbols();

            wikiEditPage.mainPath = this.ResolveUrlLC("Default.aspx");
            InitEditsLink();

            var mainStudioCss = WebSkin.BaseCSSFileAbsoluteWebPath;

            wikiEditPage.CanUploadFiles = CommunitySecurity.CheckPermissions(Common.Constants.Action_UploadFile);
            wikiEditPage.MainCssFile = mainStudioCss;

            if (Action == ActionOnPage.CategoryView)
            {
                BindPagesByCategory();
            }
        }

        private void RegisterInlineScript()
        {
            var script = @"
                    window.scrollPreview = function() {
                        jq.scrollTo(jq('#_PrevContainer').position().top, { speed: 500 });
                    }
                    window.HidePreview = function() {
                        jq('#_PrevContainer').hide();
                        jq.scrollTo(jq('#edit_container').position().top, { speed: 500 });
                    }
                    window.WikiEditBtns = function() {
                        window.checkUnload=false;
                        LoadingBanner.showLoaderBtn('#actionWikiPage');
                    }
                    window.checkUnload=true;
                    window.checkUnloadFunc = function() {
                        return checkUnload;
                    }
                    window.panelEditBtnsID = '" + pEditButtons.ClientID + @"';
                    jq.dropdownToggle({
                        dropdownID: 'WikiActionsMenuPanel',
                        switcherSelector: '.WikiHeaderBlock .menu-small',
                        addLeft: -11,
                        showFunction: function (switcherObj, dropdownItem) {
                            jq('.WikiHeaderBlock .menu-small.active').removeClass('active');
                            if (dropdownItem.is(':hidden')) {
                                switcherObj.addClass('active');
                            }
                        },
                        hideFunction: function () {
                            jq('.WikiHeaderBlock .menu-small.active').removeClass('active');
                        }
                    });
                    if (jq('#WikiActionsMenuPanel .dropdown-content a').length == 0) {
                        jq('span.menu-small').hide();
                    }
                    jq('input[id$=txtPageName]').focus();";

            if (Action == ActionOnPage.AddNew || Action == ActionOnPage.Edit)
                script += "jq.confirmBeforeUnload(window.checkUnloadFunc);";

            Page.RegisterInlineScript(script);
        }

        private IWikiObjectOwner _wikiObjOwner;

        protected void InitPageActionPanel()
        {
            var sb = new StringBuilder();
            var canEdit = false;
            var canDelete = false;
            var subscribed = false;

            if (_wikiObjOwner != null)
            {
                var secObj = new WikiObjectsSecurityObject(_wikiObjOwner);
                canEdit = CommunitySecurity.CheckPermissions(secObj, Common.Constants.Action_EditPage);
                canDelete = CommunitySecurity.CheckPermissions(secObj, Common.Constants.Action_RemovePage) &&
                            !string.IsNullOrEmpty(_wikiObjOwner.GetObjectId().ToString());
            }

            if (SecurityContext.IsAuthenticated && (Action.Equals(ActionOnPage.CategoryView) || Action.Equals(ActionOnPage.View)))
            {
                var subscriptionProvider = WikiNotifySource.Instance.GetSubscriptionProvider();
                var userList = new List<string>();
                var IAmAsRecipient = (IDirectRecipient)WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());

                if (IAmAsRecipient != null)
                {
                    userList = new List<string>(
                        subscriptionProvider.GetSubscriptions(
                            Community.Wiki.Common.Constants.EditPage,
                            IAmAsRecipient)
                        );
                }

                var pageName = WikiPage ?? string.Empty;
                subscribed = userList.Exists(s => string.Compare(s, pageName, StringComparison.InvariantCultureIgnoreCase) == 0 || (s == null && string.Empty.Equals(pageName)));
                var SubscribeTopicLink = string.Format(CultureInfo.CurrentCulture, string.Format(CultureInfo.CurrentCulture,
                                                                                                 "<a id=\"statusSubscribe\" class=\"follow-status " +
                                                                                                 (subscribed ? "subscribed" : "unsubscribed") +
                                                                                                 "\" title=\"{0}\" href=\"#\"></a>",
                                                                                                 !subscribed ? WikiResource.NotifyOnEditPage : WikiResource.UnNotifyOnEditPage));

                SubscribeLinkBlock.Text = SubscribeTopicLink;
            }

            var delURL = string.Format(@"javascript:if(confirm('{0}')) __doPostBack('{1}', '');", WikiResource.cfmDeletePage, _Default_GetDelUniqId());

            sb.Append("<div id=\"WikiActionsMenuPanel\" class=\"studio-action-panel\">");
            sb.Append("<ul class=\"dropdown-content\">");
            sb.AppendFormat("<li><a class=\"dropdown-item\" href=\"{0}\">{1}</a></li>",
                            ActionHelper.GetViewPagePath(this.ResolveUrlLC("pagehistorylist.aspx"), WikiPage),
                            WikiResource.menu_ShowVersions);

            sb.AppendFormat("<li><a class=\"dropdown-item\" href=\"javascript:window.print();\">{0}</a></li>", WikiResource.menu_PrintThePage);

            if (canEdit)
                sb.AppendFormat("<li><a class=\"dropdown-item\" href=\"{0}\">{1}</a></li>",
                                ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), WikiPage),
                                WikiResource.menu_EditThePage);

            if (canDelete)
                sb.AppendFormat("<li><a class=\"dropdown-item\" href=\"{0}\">{1}</a></li>", delURL, WikiResource.menu_DeleteThePage);

            sb.Append("</ul>");
            sb.Append("</div>");

            ActionPanel.Text = sb.ToString();

            var script = String.Format("ASC.Community.Wiki.BindSubscribeEvent({0}, \"{1}\", \"{2}\", \"{3}\")",
                subscribed.ToString().ToLower(CultureInfo.CurrentCulture),
                HttpUtility.HtmlEncode((Page as WikiBasePage).WikiPage).EscapeString(),
                WikiResource.NotifyOnEditPage,
                WikiResource.UnNotifyOnEditPage
                );

            Page.RegisterInlineScript(script);
        }

        protected void InitCategoryActionPanel()
        {
            var sb = new StringBuilder();

            sb.Append("<div id=\"WikiActionsMenuPanel\" class=\"studio-action-panel\">");
            sb.Append("<ul class=\"dropdown-content\">");

            sb.AppendFormat("<li><a class=\"dropdown-item\" href=\"{0}\">{1}</a></li>",
                            ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), WikiPage),
                            WikiResource.cmdEdit);

            sb.Append("</ul>");
            sb.Append("</div>");

            ActionPanel.Text = sb.ToString();
        }

        protected void wikiViewPage_WikiPageLoaded(bool isNew, IWikiObjectOwner owner)
        {
            _wikiObjOwner = owner;
            wikiViewPage.CanEditPage = CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(_wikiObjOwner), Common.Constants.Action_EditPage);
            UpdateEditDeleteVisible(owner);
            //WikiMaster.UpdateNavigationItems();
            InitPageActionPanel();
        }

        protected void wikiEditPage_WikiPageLoaded(bool isNew, IWikiObjectOwner owner)
        {
            if (!isNew)
            {
                _wikiObjOwner = owner;
            }

            if ((isNew && !CommunitySecurity.CheckPermissions(Common.Constants.Action_AddPage))
                ||
                (!isNew && !(CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(owner), Common.Constants.Action_EditPage))))
            {
                Response.RedirectLC("Default.aspx", this);
            }
        }

        protected void wikiEditPage_SaveNewCategoriesAdded(object sender, List<string> categories, string pageName)
        {
            //var authorId = SecurityContext.CurrentAccount.ID.ToString();
            //foreach (var catName in categories)
            //{
            //    WikiNotifyClient.SendNoticeAsync(
            //        authorId,
            //        Common.Constants.AddPageToCat,
            //        catName,
            //        null,
            //        GetListOfTagValForCategoryNotify(catName, pageName));
            //}
        }

        private string _Default_GetDelUniqId()
        {
            return cmdDelete.UniqueID;
        }

        internal string GetCategoryName()
        {
            return m_categoryName;
        }

        protected void OnPageEmpty(object sender, EventArgs e)
        {
            var pageName = PageNameUtil.Decode(WikiPage);

            wikiViewPage.Visible = false;
            wikiEditPage.Visible = false;
            wikiViewFile.Visible = false;
            wikiEditFile.Visible = false;
            pPageIsNotExists.Visible = true;

            if (!(Action.Equals(ActionOnPage.CategoryView) || Action.Equals(ActionOnPage.CategoryEdit)))
            {
                if (IsFile)
                {
                    txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiFileIsNotExists, true, false);
                }
                else
                {
                    if (Wiki.SearchPagesByName(pageName).Count > 0)
                    {
                        txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiPageIsNotExists, false, true);
                    }
                    else
                    {
                        txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiPageIsNotExists, false, false);
                    }
                }
            }

            _isEmptyPage = true;
            InitEditsLink();
            //WikiMaster.UpdateNavigationItems();
        }

        private string PrepereEmptyString(string format, bool isFile, bool isSearchResultExists)
        {
            commentList.Visible = false;
            var mainOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            var rxLinkCreatePlace = new Regex(@"{0([\s\S]+?)}", mainOptions);
            var rxLinkSearchResult = new Regex(@"{1([\s\S]+?)}", mainOptions);
            var rxSearchResultParth = new Regex(@"\[\[([\s\S]+?)\]\]", mainOptions);
            var result = format;

            foreach (Match match in rxLinkCreatePlace.Matches(format))
            {
                if (isFile)
                {
                    if (CommunitySecurity.CheckPermissions(Common.Constants.Action_UploadFile))
                    {
                        result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), match.Groups[1].Value));
                    }
                }
                else
                {
                    if (CommunitySecurity.CheckPermissions(Common.Constants.Action_AddPage))
                    {
                        result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), match.Groups[1].Value));
                    }
                    else
                    {
                        result = result.Replace(match.Value, match.Groups[1].Value);
                    }
                }
            }

            if (isSearchResultExists && !isFile)
            {
                result = rxSearchResultParth.Replace(result, SearchResultParthMatchEvaluator);

                foreach (Match match in rxLinkSearchResult.Matches(format))
                {
                    result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", this.ResolveUrlLC(string.Format("Search.aspx?Search={0}&pn=", HttpUtility.UrlEncode(PageNameUtil.Decode(WikiPage)))), match.Groups[1].Value));
                }
            }
            else
            {
                result = rxSearchResultParth.Replace(result, string.Empty);
            }

            return result;
        }

        private string SearchResultParthMatchEvaluator(Match match)
        {
            return match.Groups[1].Value;
        }

        private string GetAbsolutePath(string relative)
        {
            return string.Format(@"{0}://{1}{2}{3}",
                                 Request.GetUrlRewriter().Scheme,
                                 Request.GetUrlRewriter().Host,
                                 (Request.GetUrlRewriter().Port != 80 ? string.Format(":{0}", Request.GetUrlRewriter().Port) : string.Empty),
                                 this.ResolveUrlLC(relative));
        }

        private void LoadViews()
        {
            wikiEditPage.AlaxUploaderPath = GetAbsolutePath("~/js/uploader/ajaxupload.js");
            wikiEditPage.JQPath = GetAbsolutePath("~/js/third-party/jquery/jquery.core.js");

            wikiEditPage.CurrentUserId = SecurityContext.CurrentAccount.ID;
            wikiViewPage.Visible = false;
            wikiEditPage.Visible = false;
            wikiViewFile.Visible = false;
            wikiEditFile.Visible = false;
            pPageIsNotExists.Visible = false;
            pView.Visible = false;
            PrintHeader.Visible = false;
            phCategoryResult.Visible = Action == ActionOnPage.CategoryView;

            var pageName = PrintPageName;
            var _mobileVer = MobileDetector.IsMobile;


            //fix for IE 10
            var browser = HttpContext.Current.Request.Browser.Browser;

            var userAgent = Context.Request.Headers["User-Agent"];
            var regExp = new Regex("MSIE 10.0", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var regExpIe11 = new Regex("(?=.*Trident.*rv:11.0).+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (browser == "IE" && regExp.Match(userAgent).Success || regExpIe11.Match(userAgent).Success)
            {
                _mobileVer = true;
            }

            switch (Action)
            {
                case ActionOnPage.AddNew:
                    pageName = WikiResource.MainWikiAddNewPage;
                    wikiEditPage.IsWysiwygDefault = !_mobileVer && WikiModuleSettings.GetIsWysiwygDefault();
                    wikiEditPage.Visible = true;
                    wikiEditPage.IsNew = true;
                    WikiPageName = pageName;
                    break;
                case ActionOnPage.AddNewFile:
                    pageName = WikiResource.MainWikiAddNewFile;
                    wikiEditFile.Visible = true;
                    WikiPageName = pageName;
                    break;
                case ActionOnPage.Edit:
                case ActionOnPage.CategoryEdit:
                    if (IsFile)
                    {
                        wikiEditFile.FileName = WikiPage;
                        wikiEditFile.Visible = true;
                        WikiPageName = WikiResource.MainWikiEditFile;
                    }
                    else
                    {
                        wikiEditPage.PageName = WikiPage;
                        wikiEditPage.IsWysiwygDefault = !_mobileVer && WikiModuleSettings.GetIsWysiwygDefault();
                        wikiEditPage.Visible = true;
                        if (m_IsCategory)
                            wikiEditPage.IsSpecialName = true;

                        WikiPageName = WikiResource.MainWikiEditPage;
                    }
                    break;
                case ActionOnPage.View:
                case ActionOnPage.CategoryView:
                    pView.Visible = true;
                    if (IsFile)
                    {
                        wikiViewFile.FileName = WikiPage;
                        wikiViewFile.Visible = true;
                    }
                    else
                    {
                        PrintHeader.Visible = true;
                        wikiViewPage.PageName = WikiPage;
                        wikiViewPage.Version = Version;
                        if (Version == 0)
                        {
                            if (m_IsCategory)
                            {
                                var name = HttpUtility.HtmlDecode(m_categoryName);
                                WikiPageName = string.Format(WikiResource.menu_ListPagesCatFormat, name);
                            }
                        }
                        else
                        {
                            if (m_IsCategory)
                            {
                                var name = HttpUtility.HtmlDecode(m_categoryName);
                                WikiPageName = string.Format(WikiResource.menu_ListPagesCatFormat, name);
                            }
                            WikiMaster.CurrentPageCaption = string.Format("{0}{1}", WikiResource.wikiVersionCaption, Version);
                        }
                        wikiViewPage.Visible = true;
                    }
                    InitCommentsView();
                    break;
            }
            if (SecurityContext.IsAuthenticated && (Action.Equals(ActionOnPage.CategoryView) || Action.Equals(ActionOnPage.View)))
            {
                var subscriptionProvider = WikiNotifySource.Instance.GetSubscriptionProvider();
                var userList = new List<string>();
                var IAmAsRecipient = (IDirectRecipient)WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
                if (IAmAsRecipient != null)
                {
                    userList = new List<string>(
                        subscriptionProvider.GetSubscriptions(
                            Community.Wiki.Common.Constants.EditPage,
                            IAmAsRecipient)
                        );
                }

                pageName = WikiPage ?? string.Empty;
                var subscribed = userList.Exists(s => string.Compare(s, pageName, StringComparison.InvariantCultureIgnoreCase) == 0 || (s == null && string.Empty.Equals(pageName)));
                var SubscribeTopicLink = string.Format(CultureInfo.CurrentCulture, string.Format(CultureInfo.CurrentCulture,
                                                                                                 "<a id=\"statusSubscribe\" class=\"follow-status " +
                                                                                                 (subscribed ? "subscribed" : "unsubscribed") +
                                                                                                 "\" title=\"{0}\" href=\"#\"></a>",
                                                                                                 !subscribed ? WikiResource.NotifyOnEditPage : WikiResource.UnNotifyOnEditPage));

                SubscribeLinkBlock.Text = SubscribeTopicLink;
            }

        }

        protected void BindPagesByCategory()
        {
            if (Action != ActionOnPage.CategoryView || string.IsNullOrEmpty(m_categoryName))
                return;

            var result = Wiki.GetPages(m_categoryName);

            result.RemoveAll(pemp => string.IsNullOrEmpty(pemp.PageName));

            var letters = new List<string>(WikiResource.wikiCategoryAlfaList.Split(','));

            var otherSymbol = string.Empty;
            if (letters.Count > 0)
            {
                otherSymbol = letters[0];
                letters.Remove(otherSymbol);
            }

            var dictList = new List<PageDictionary>();
            foreach (var page in result)
            {
                var firstLetter = new string(page.PageName[0], 1);

                if (!letters.Exists(lt => lt.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    firstLetter = otherSymbol;
                }

                PageDictionary pageDic;
                if (!dictList.Exists(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    pageDic = new PageDictionary { HeadName = firstLetter };
                    pageDic.Pages.Add(page);
                    dictList.Add(pageDic);
                }
                else
                {
                    pageDic = dictList.Find(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase));
                    pageDic.Pages.Add(page);
                }
            }

            dictList.Sort(SortPageDict);

            var countAll = dictList.Count*3 + result.Count; //1 letter is like 2 links to category
            var perColumn = (int)(Math.Round((decimal)countAll/3));

            var mainDictList = new List<List<PageDictionary>>();

            int index = 0, lastIndex = 0, count = 0;

            for (var i = 0; i < dictList.Count; i++)
            {
                var p = dictList[i];

                count += 3;
                count += p.Pages.Count;
                index++;
                if (count >= perColumn || i == dictList.Count - 1)
                {
                    count = count - perColumn;
                    mainDictList.Add(dictList.GetRange(lastIndex, index - lastIndex));
                    lastIndex = index;
                }
            }

            rptCategoryPageList.DataSource = mainDictList;
            rptCategoryPageList.DataBind();
        }


        private int SortPageDict(PageDictionary cd1, PageDictionary cd2)
        {
            return cd1.HeadName.CompareTo(cd2.HeadName);
        }

        protected void On_PublishVersionInfo(object sender, VersionEventArgs e)
        {
            if (!e.UserID.Equals(Guid.Empty))
            {
                litAuthorInfo.Text = GetPageInfo(PageNameUtil.Decode(WikiPage), e.UserID, e.Date);
            }
            else
            {
                litAuthorInfo.Text = string.Empty;
            }

            hlVersionPage.Text = string.Format(WikiResource.cmdVersionTemplate, e.Version);
            hlVersionPage.NavigateUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("PageHistoryList.aspx"), PageNameUtil.Decode(WikiPage));
            hlVersionPage.Visible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);
            //litVersionSeparator.Visible = hlEditPage.Visible;
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            if (!IsFile)
            {
                Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), this);
            }
            else
            {
                Response.RedirectLC(ActionHelper.GetViewFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), this);
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            SaveResult result;
            string pageName;
            if (IsFile || Action.Equals(ActionOnPage.AddNewFile))
            {
                result = wikiEditFile.Save(SecurityContext.CurrentAccount.ID, out pageName);
            }
            else
            {
                result = wikiEditPage.Save(SecurityContext.CurrentAccount.ID, out pageName);
            }

            PrintResultBySave(result, pageName);
            if (result == SaveResult.OkPageRename)
            {
                //Redirect
                Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(pageName)), this);
            }
        }

        private void PrintInfoMessage(string info, InfoType type)
        {
            WikiMaster.PrintInfoMessage(info, type);
        }

        private void PrintResultBySave(SaveResult result, string pageName)
        {
            var infoType = InfoType.Info;
            if (!result.Equals(SaveResult.Ok) && !result.Equals(SaveResult.NoChanges))
            {
                infoType = InfoType.Alert;
            }

            switch (result)
            {
                case SaveResult.SectionUpdate:
                    Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), pageName), this);
                    break;
                case SaveResult.OkPageRename:
                case SaveResult.Ok:
                    PrintInfoMessage(WikiResource.msgSaveSucess, infoType);
                    if (Action.Equals(ActionOnPage.AddNew) || Action.Equals(ActionOnPage.Edit))
                    {
                        Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), pageName), this);
                    }
                    else if (Action.Equals(ActionOnPage.AddNewFile))
                    {
                        Response.RedirectLC(ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), pageName), this);
                    }
                    break;
                case SaveResult.FileEmpty:
                    PrintInfoMessage(WikiResource.msgFileEmpty, infoType);
                    break;
                case SaveResult.FileSizeExceeded:
                    PrintInfoMessage(FileSizeComment.GetFileSizeExceptionString(FileUploader.MaxUploadSize), infoType);
                    break;
                case SaveResult.NoChanges:
                    PrintInfoMessage(WikiResource.msgNoChanges, infoType);
                    break;
                case SaveResult.PageNameIsEmpty:
                    PrintInfoMessage(WikiResource.msgPageNameEmpty, infoType);
                    break;
                case SaveResult.PageNameIsIncorrect:
                    PrintInfoMessage(WikiResource.msgPageNameIncorrect, infoType);
                    break;
                case SaveResult.SamePageExists:
                    PrintInfoMessage(WikiResource.msgSamePageExists, infoType);
                    break;
                case SaveResult.UserIdIsEmpty:
                    PrintInfoMessage(WikiResource.msgInternalError, infoType);
                    break;
                case SaveResult.OldVersion:
                    PrintInfoMessage(WikiResource.msgOldVersion, infoType);
                    break;
                case SaveResult.Error:
                    PrintInfoMessage(WikiResource.msgMarkupError, InfoType.Alert);
                    break;
                case SaveResult.PageTextIsEmpty:
                    PrintInfoMessage(WikiResource.msgPageTextEmpty, infoType);
                    break;
            }
        }

        private void InitEditsLink()
        {
            //hlEditPage.Text = WikiResource.cmdEdit;
            cmdSave.Text = WikiResource.cmdPublish;
            hlPreview.Text = WikiResource.cmdPreview;
            hlPreview.Attributes["onclick"] = string.Format("{0}();return false;", wikiEditPage.GetShowPrevFunctionName());
            //hlPreview.NavigateUrl = string.Format("javascript:{0}();", wikiEditPage.GetShowPrevFunctionName());
            hlPreview.NavigateUrl = string.Format("javascript:void(0);");
            cmdCancel.Text = WikiResource.cmdCancel;
            cmdCancel.Attributes["name"] = wikiEditPage.WikiFckClientId;
            cmdDelete.Text = WikiResource.cmdDelete;
            cmdDelete.OnClientClick = string.Format("javascript:return confirm(\"{0}\");", WikiResource.cfmDeletePage);

            hlPreview.Visible = Action.Equals(ActionOnPage.AddNew) || Action.Equals(ActionOnPage.Edit) || Action.Equals(ActionOnPage.CategoryEdit);

            if (_isEmptyPage)
            {
                //hlEditPage.Visible = pEditButtons.Visible = false;
                cmdDelete.Visible = false;
                if (Action.Equals(ActionOnPage.CategoryView))
                {
                    //hlEditPage.Visible = true;
                    InitCategoryActionPanel();
                }
            }
            else
            {
                UpdateEditDeleteVisible(_wikiObjOwner);
            }

            litVersionSeparatorDel.Visible = cmdDelete.Visible;
        }

        private void UpdateEditDeleteVisible(IWikiObjectOwner obj)
        {
            var canDelete = false;
            var editVisible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);
            if (obj != null)
            {
                var secObj = new WikiObjectsSecurityObject(obj);
                canDelete = CommunitySecurity.CheckPermissions(secObj, Common.Constants.Action_RemovePage) &&
                            !string.IsNullOrEmpty(obj.GetObjectId().ToString());
            }

            pEditButtons.Visible = !editVisible;
            //hlEditPage.Visible = editVisible && canEdit;

            if (Version > 0 && (Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView)))
            {
                //hlEditPage.Visible = pEditButtons.Visible = false;
            }

            cmdDelete.Visible = editVisible && canDelete;
            litVersionSeparatorDel.Visible = cmdDelete.Visible;
        }

        #region Comments Functions

        private void InitCommentsView()
        {
            if (m_IsCategory) return;

            int totalCount;
            var pageName = PageNameUtil.Decode(WikiPage);
            commentList.Visible = true;

            commentList.Items = GetCommentsList(pageName, out totalCount);
            ConfigureComments(commentList);
            commentList.TotalCount = totalCount;
        }

        private IList<CommentInfo> GetCommentsList(string pageName, out int totalCount)
        {
            var comments = Wiki.GetComments(pageName);
            totalCount = comments.Count;

            return BuildCommentsList(comments);
        }

        private List<CommentInfo> BuildCommentsList(List<Comment> loaded)
        {
            return BuildCommentsList(loaded, Guid.Empty);
        }

        private List<CommentInfo> BuildCommentsList(List<Comment> loaded, Guid parentId)
        {
            var result = new List<CommentInfo>();
            foreach (var comment in SelectChildLevel(parentId, loaded))
            {
                var info = GetCommentInfo(comment);
                info.CommentList = BuildCommentsList(loaded, comment.Id);
                result.Add(info);
            }
            return result;
        }

        private static List<Comment> SelectChildLevel(Guid forParentId, List<Comment> from)
        {
            return from.FindAll(comm => comm.ParentId == forParentId);
        }

        private static void ConfigureComments(CommentsList commentList)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.BehaviorID = "_commentsWikiObj";
            commentList.IsShowAddCommentBtn = CommunitySecurity.CheckPermissions(Common.Constants.Action_AddComment);
            commentList.ModuleName = "wiki";
            commentList.FckDomainName = "wiki_comments";
            commentList.ObjectID = "wiki_page";
        }

        public CommentInfo GetCommentInfo(Comment comment)
        {
            var info = new CommentInfo
                {
                    CommentID = comment.Id.ToString(),
                    UserID = comment.UserId,
                    TimeStamp = comment.Date,
                    TimeStampStr = comment.Date.Ago(),
                    IsRead = true,
                    Inactive = comment.Inactive,
                    CommentBody = HtmlUtility.GetFull(comment.Body),
                    UserFullName = DisplayUserSettings.GetFullUserName(comment.UserId),
                    UserProfileLink = CommonLinkUtility.GetUserProfile(comment.UserId),
                    UserAvatarPath = UserPhotoManager.GetBigPhotoURL(comment.UserId),
                    IsEditPermissions = CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(comment), Common.Constants.Action_EditRemoveComment),
                    IsResponsePermissions = CommunitySecurity.CheckPermissions(Common.Constants.Action_AddComment),
                    UserPost = CoreContext.UserManager.GetUsers(comment.UserId).Title
                };

            return info;
        }

        #region Ajax functions for comments management

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string ConvertWikiToHtml(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                                        string imageHandlerUrl)
        {
            return EditPage.ConvertWikiToHtml(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                                              imageHandlerUrl, TenantId);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string ConvertWikiToHtmlWysiwyg(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                                               string imageHandlerUrl)
        {
            return EditPage.ConvertWikiToHtmlWysiwyg(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                                                     imageHandlerUrl, TenantId);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string CreateImageFromWiki(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                                          string imageHandlerUrl)
        {
            return EditPage.CreateImageFromWiki(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                                                imageHandlerUrl, TenantId);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string UpdateTempImage(string fileName, string UserId, string tempFileName)
        {
            string outFileName;
            EditFile.MoveContentFromTemp(new Guid(UserId), tempFileName, fileName, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current, RootPath, out outFileName);
            return outFileName;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void CancelUpdateImage(string UserId, string tempFileName)
        {
            EditFile.DeleteTempContent(tempFileName, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current);
        }

        #endregion

        #endregion

        #region IContextInitializer Members

        public void InitializeContext(HttpContext context)
        {
            _rootPath = context.Server.MapPath("~");
            _wikiSection = WikiSection.Section;
        }

        #endregion
    }
}