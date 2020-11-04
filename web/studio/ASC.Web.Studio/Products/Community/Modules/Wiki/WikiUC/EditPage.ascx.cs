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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Core.Tenants;
using ASC.Web.Core.Mobile;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki.UC
{
    public enum SaveResult
    {
        Ok = 0,
        OkPageRename,
        PageNameIsEmpty,
        PageNameIsIncorrect,
        SamePageExists,
        FileEmpty,
        OldVersion,
        UserIdIsEmpty,
        NoChanges,
        SectionUpdate,
        FileSizeExceeded,
        Error,
        PageTextIsEmpty
    }

    public partial class EditPage : BaseUserControl
    {
        public delegate void SaveNewCategoriesAddedHandler(object sender, List<string> categories, string pageName);

        public event SaveNewCategoriesAddedHandler SaveNewCategoriesAdded;

        public delegate void SetNewFCKModeHandler(bool isWysiwygDefault);

        public event SetNewFCKModeHandler SetNewFCKMode;

        public delegate string GetUserFriendlySizeFormatHandler(long size);

        public event GetUserFriendlySizeFormatHandler GetUserFriendlySizeFormat;

        public Guid CurrentUserId { get; set; }

        private int PageVersion
        {
            get { return ViewState["pageVersion"] == null ? 0 : Convert.ToInt32(ViewState["pageVersion"]); }
            set { ViewState["pageVersion"] = value; }
        }

        public string AlaxUploaderPath
        {
            get { return ViewState["AlaxUploaderPath"] == null ? string.Empty : ViewState["AlaxUploaderPath"].ToString().ToLower(); }

            set { ViewState["AlaxUploaderPath"] = value; }
        }

        public string PleaseWaitMessage
        {
            get { return ViewState["PleaseWaitMessage"] == null ? string.Empty : ViewState["PleaseWaitMessage"].ToString(); }

            set { ViewState["PleaseWaitMessage"] = value; }
        }

        public string JQPath
        {
            get { return ViewState["JQPath"] == null ? string.Empty : ViewState["JQPath"].ToString().ToLower(); }

            set { ViewState["JQPath"] = value; }
        }

        private int PageSection
        {
            get
            {
                int result;
                if (!int.TryParse(Request["section"], out result))
                {
                    result = -1;
                }
                return result;
            }
        }

        /// <summary>
        /// The Main Container of the Preview. The Container display style will be setted to '' when the preview result will be ready.
        /// </summary>
        public string PreviewContainer
        {
            get { return ViewState["PreviewContainer"] == null ? string.Format("{0}_PrevContainer", ClientID) : ViewState["PreviewContainer"].ToString(); }
            set { ViewState["PreviewContainer"] = value; }
        }

        public string OnPreviewReadyHandler
        {
            get { return ViewState["OnPreviewReadyHandler"] == null ? string.Format("{0}_OnPreviewReadyHandler", ClientID) : ViewState["OnPreviewReadyHandler"].ToString(); }
            set { ViewState["OnPreviewReadyHandler"] = value; }
        }

        /// <summary>
        /// Div DOM Id where result of the preview will be setted.
        /// </summary>
        public string PreviewView
        {
            get { return ViewState["PreviewView"] == null ? string.Format("{0}_PrevValue", ClientID) : ViewState["PreviewView"].ToString(); }
            set { ViewState["PreviewView"] = value; }
        }

        public bool IsWysiwygDefault
        {
            get
            {
                //if (ViewState["IsWysiwygDefault"] == null)
                //    return true;
                //return Convert.ToBoolean(ViewState["IsWysiwygDefault"]);
                return true;
            }
            set { ViewState["IsWysiwygDefault"] = value; }
        }

        public bool CanUploadFiles
        {
            get { return ViewState["CanUploadFiles"] != null && Convert.ToBoolean(ViewState["CanUploadFiles"]); }
            set { ViewState["CanUploadFiles"] = value; }
        }

        public bool IsNew
        {
            get { return ViewState["IsNew"] != null && Convert.ToBoolean(ViewState["IsNew"]); }
            set { ViewState["IsNew"] = value; }
        }

        public string MainCssFile
        {
            get { return ViewState["MainCssFile"] == null ? string.Empty : ViewState["MainCssFile"].ToString(); }
            set { ViewState["MainCssFile"] = value; }
        }

        public bool IsSpecialName
        {
            get { return ViewState["IsSpecialName"] != null && Convert.ToBoolean(ViewState["IsSpecialName"]); }
            set { ViewState["IsSpecialName"] = value; }
        }

        public string PageName
        {
            get { return ViewState["PageName"] == null ? string.Empty : ViewState["PageName"].ToString(); }
            set { ViewState["PageName"] = value; }
        }

        public string WikiFckClientId
        {
            get { return Wiki_FCKEditor.ClientID; }
        }

        private bool IsNameReserved(string pageName)
        {
            if (!pageName.Contains(':'))
                return false;

            var exitReserv = (
                                 from rn in reservedPrefixes
                                 where rn.Equals(pageName.Split(':')[0], StringComparison.InvariantCultureIgnoreCase)
                                 select rn);

            return exitReserv.Any();
        }

        private string RenderPageContent(string pageName, string wiki)
        {
            return HtmlWikiUtil.WikiToHtml(pageName, wiki, Page.ResolveUrl(Request.AppRelativeCurrentExecutionFilePath), Wiki.GetPagesAndFiles(wiki),
                                           Page.ResolveUrl(ImageHandlerUrlFormat), TenantId, ConvertType.Wysiwyg);
        }

        public string mainPath
        {
            get { return ViewState["mainPath"] == null ? string.Empty : ViewState["mainPath"].ToString().ToLower(); }
            set { ViewState["mainPath"] = value; }
        }
        private void SetWikiFCKEditorValue(string pageName, string pageBody)
        {
            Wiki_FCKEditor.Value = IsWysiwygDefault ? RenderPageContent(pageName, pageBody) : pageBody;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;

            Wiki_FCKEditor.BasePath = VirtualPathUtility.ToAbsolute(BaseFCKRelPath);
            Wiki_FCKEditor.ToolbarSet = "WikiPanel";

            if (PageName != null && Visible)
            {
                var page = Wiki.GetPage(PageNameUtil.Decode(PageName));
                var isPageNew = (page == null || IsNew);

                RiseWikiPageLoaded(isPageNew, page);

                if (!isPageNew)
                {
                    txtPageName.Value = page.PageName;
                    if (PageSection < 0)
                    {
                        SetWikiFCKEditorValue(page.PageName, page.Body);
                    }
                    else
                    {
                        SetWikiFCKEditorValue(page.PageName, HtmlWikiUtil.GetWikiSectionBySectionNumber(page.Body, PageSection));
                        txtPageName.Attributes["readonly"] = "readonly";
                        txtPageName.Value += string.Format(WikiUCResource.wikiPageEditSectionCaptionFormat, HtmlWikiUtil.GetWikiSectionNameBySectionNumber(page.Body, PageSection));
                    }

                    //Check for 'help' and 'main' page
                    if (IsStandartName(page))
                    {
                        txtPageName.Attributes["readonly"] = "readonly";
                    }
                    PageVersion = page.Version;

                    //if (page.PageName.Equals(string.Empty))
                    //txtPageName.ReadOnly = true; //We need to disable any changes for the one time saved page.

                    RisePublishVersionInfo(page);
                }
                else if (!string.IsNullOrEmpty(PageName))
                {
                    txtPageName.Value = PageNameUtil.Decode(PageName);
                    if (IsSpecialName)
                    {
                        txtPageName.Attributes["readonly"] = "readonly";
                    }
                }

                phPageName.Visible = txtPageName.Attributes["readonly"] == null;
            }

            hfFCKLastState.Value = (!IsWysiwygDefault).ToString().ToLower();
            //txtPageName.Focus();
        }

        private static bool IsStandartName(Page page)
        {
            return page.PageName == WikiUCResource.HelpPageCaption || page.PageName == ""; //Main or help
        }

        public SaveResult Save(Guid userId)
        {
            string pageName;
            return Save(userId, out pageName);
        }

        public SaveResult Save(Guid userId, out string pageName)
        {
            try
            {
                if (SetNewFCKMode != null)
                {
                    bool isSource;
                    bool.TryParse(hfFCKLastState.Value, out isSource);
                    SetNewFCKMode(!isSource);

                    IsWysiwygDefault = !isSource; //We need to update the variable if SetNewFCKMode is used only!!!
                }

                Page page;
                var currentPageName = txtPageName.Value.Trim();
                if (currentPageName.Length > 240) currentPageName = currentPageName.Substring(0, 240).Trim();
                currentPageName = PageNameUtil.Clean(currentPageName);
                if (PageSection >= 0)
                {
                    currentPageName = PageNameUtil.Decode(PageName);
                }

                pageName = currentPageName;
                var isPageRename = pageName != PageNameUtil.Decode(PageName) && !string.IsNullOrEmpty(PageName);
                var oldPageName = PageName;

                if (currentPageName.Equals(string.Empty) && txtPageName.Attributes["readonly"] == null)
                {
                    SetWikiFCKEditorValue(currentPageName, Wiki_FCKEditor.Value);
                    return SaveResult.PageNameIsEmpty;
                }

                if (!IsSpecialName && IsNameReserved(currentPageName))
                {
                    SetWikiFCKEditorValue(currentPageName, Wiki_FCKEditor.Value);
                    return SaveResult.PageNameIsIncorrect;
                }

                if (userId.Equals(Guid.Empty))
                {
                    SetWikiFCKEditorValue(currentPageName, Wiki_FCKEditor.Value);
                    return SaveResult.UserIdIsEmpty;
                }

                if (!PageNameUtil.Decode(PageName).Equals(currentPageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    page = Wiki.GetPage(currentPageName);
                    if (page != null)
                    {
                        SetWikiFCKEditorValue(currentPageName, Wiki_FCKEditor.Value);
                        return SaveResult.SamePageExists;
                    }
                    page = new Page();
                }
                else
                {
                    page = Wiki.GetPage(currentPageName) ?? new Page();
                }

                page.PageName = currentPageName;
                var pageResult = PageSection < 0
                                     ? Wiki_FCKEditor.Value
                                     : HtmlWikiUtil.SetWikiSectionBySectionNumber(page.Body, PageSection, Wiki_FCKEditor.Value);

                if (string.IsNullOrEmpty(pageResult))
                {
                    SetWikiFCKEditorValue(page.PageName, Wiki_FCKEditor.Value);
                    return SaveResult.PageTextIsEmpty;
                }

                if (pageResult.Equals(page.Body))
                {
                    SetWikiFCKEditorValue(page.PageName, Wiki_FCKEditor.Value);
                    return SaveResult.NoChanges;
                }

                page.Body = pageResult;

                if (PageVersion > 0 && PageVersion < Wiki.GetPageMaxVersion(page.PageName))
                {
                    SetWikiFCKEditorValue(page.PageName, Wiki_FCKEditor.Value);
                    return SaveResult.OldVersion;
                }
                page.Date = TenantUtil.DateTimeNow();
                page.UserID = userId;

                page.Version++;
                Wiki.SavePage(page);

                var newCats = Wiki.UpdateCategoriesByPageContent(page);
                if (newCats.Count > 0 && SaveNewCategoriesAdded != null)
                {
                    SaveNewCategoriesAdded(this, newCats, page.PageName);
                }

                PageVersion = page.Version;
                RisePublishVersionInfo(page);

                SetWikiFCKEditorValue(page.PageName, Wiki_FCKEditor.Value);

                if (PageSection >= 0)
                {
                    return SaveResult.SectionUpdate;
                }
                if (isPageRename)
                {
                    //create dumb page
                    var oldpage = Wiki.GetPage(PageNameUtil.Decode(oldPageName));
                    if (oldpage != null)
                    {
                        oldpage.Date = TenantUtil.DateTimeNow();
                        oldpage.UserID = userId;
                        oldpage.Body = string.Format(WikiUCResource.wikiPageMoved, pageName); //Dummy
                        oldpage.Version++;
                        Wiki.SavePage(oldpage);

                        return SaveResult.OkPageRename;
                    }
                    return SaveResult.SamePageExists;
                }
                return SaveResult.Ok;
            }
            catch (Exception)
            {
                pageName = txtPageName.Value.Trim();
                return SaveResult.Error;
            }
        }

        protected string InitVariables()
        {
            return string.Format(@"var pageName = '{0}';
                            var appRelativeCurrentExecutionFilePath = '{1}';
                            var imageHandlerUrl = '{2}';
                            var wikiInternalStart = '{3}';
                            var wikiInternalFile = '{4}';
                            var wikiMaxFileUploadSizeString = '{5}'
                            var wikiDenyFileUpload = {6};",
                                 PageNameUtil.Decode(PageName).EscapeString(),
                                 Page.ResolveUrl(Request.AppRelativeCurrentExecutionFilePath).EscapeString().ToLower(),
                                 Page.ResolveUrl(ImageHandlerUrlFormat).EscapeString().ToLower(),
                                 string.Format("{0}?page=", mainPath).EscapeString(),
                                 string.Format("{0}?file=", mainPath).EscapeString(),
                                 GetMaxFileUploadString(),
                                 (!CanUploadFiles).ToString().ToLower()
                );
        }


        protected string GetMaxFileUploadString()
        {
            var result = string.Empty;
            var lMaxFileSize = FileUploader.MaxUploadSize;
            if (lMaxFileSize == 0)
                return result;
            result = GetUserFriendlySizeFormat != null ? GetUserFriendlySizeFormat(lMaxFileSize) : lMaxFileSize.ToString();

            result = string.Format(WikiUCResource.wikiMaxUploadSizeFormat, result);

            return result;
        }

        protected string GetPageClassName()
        {
            return Page.GetType().BaseType.Name;
        }

        protected string GetAllStyles()
        {
            const string linkCssFormat = "<link rel='stylesheet' text='text/css' href='{0}' />";
            const string scriptFormat = "<script language='javascript' type='text/javascript' src='{0}'>\" + \"</\" + \"script>";
            var script = "\"";
            //if (!string.IsNullOrEmpty(MainCssFile))
            //{
            //    script += string.Format(linkCssFormat, MainCssFile);
            //}

            script += string.Format(linkCssFormat, VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/content/main.css"));

            script += string.Format(scriptFormat, VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/scripts/editpage.js"));

            script += "\";";

            return script;
        }

        protected string ToLowerPath(string url)
        {
            if (!url.Contains("?"))
                return url.ToLower();
            var _url = url.Split('?')[0];
            var _params = url.Split('?')[1];
            return string.Format("{0}?{1}", _url.ToLower(), _params);
        }


        public static string ConvertWikiToHtml(string pageName, string value, string appRelativeCurrentExecutionFilePath, string imageHandlerUrl, int tenantId)
        {
            return HtmlWikiUtil.WikiToHtml(pageName, value, appRelativeCurrentExecutionFilePath, new WikiEngine().GetPagesAndFiles(value), imageHandlerUrl, tenantId, ConvertType.NotEditable);
        }

        public static string ConvertWikiToHtmlWysiwyg(string pageName, string value, string appRelativeCurrentExecutionFilePath, string imageHandlerUrl, int tenantId)
        {
            return HtmlWikiUtil.WikiToHtml(pageName, value, appRelativeCurrentExecutionFilePath, new WikiEngine().GetPagesAndFiles(value), imageHandlerUrl, tenantId, ConvertType.Wysiwyg);
        }

        public static string CreateImageFromWiki(string pageName, string value, string appRelativeCurrentExecutionFilePath, string imageHandlerUrl, int tenantId)
        {
            return HtmlWikiUtil.CreateImageFromWiki(pageName, value, appRelativeCurrentExecutionFilePath, imageHandlerUrl, tenantId);
        }

        public string GetShowPrevFunctionName()
        {
            return string.Format("{0}_ShowPreview", ClientID);
        }
    }
}