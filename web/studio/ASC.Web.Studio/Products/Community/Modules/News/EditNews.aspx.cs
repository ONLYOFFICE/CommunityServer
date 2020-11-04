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
using System.Globalization;
using System.Web;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News
{
    [AjaxNamespace("EditNews")]
    public partial class EditNews : MainPage
    {
        protected string PageTitle { get; private set; }

        private RequestInfo info;
        protected string _text = "";

        private RequestInfo Info
        {
            get { return info ?? (info = new RequestInfo(Request)); }
        }

        public long FeedId
        {
            get { return ViewState["FeedID"] != null ? Convert.ToInt32(ViewState["FeedID"], CultureInfo.CurrentCulture) : 0; }
            set { ViewState["FeedID"] = value; }
        }

        private void BindNewsTypes()
        {
            feedType.DataSource = new[]
                {
                    FeedTypeInfo.FromFeedType(FeedType.News),
                    FeedTypeInfo.FromFeedType(FeedType.Order),
                    FeedTypeInfo.FromFeedType(FeedType.Advert)
                };
            feedType.DataBind();

            if (!string.IsNullOrEmpty(Request["type"]))
            {
                var requestFeedType = (FeedType)Enum.Parse(typeof(FeedType), Request["type"], true);
                var feedTypeInfo = FeedTypeInfo.FromFeedType(requestFeedType);

                var item = feedType.Items.FindByText(feedTypeInfo.TypeName);

                feedType.SelectedValue = item.Value;
            }
            else
            {
                feedType.SelectedIndex = 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(GetType());

            if (!CommunitySecurity.CheckPermissions(NewsConst.Action_Add))
                Response.Redirect(FeedUrls.MainPageUrl, true);

            var storage = FeedStorageFactory.Create();
            FeedNS.Feed feed = null;
            if (!string.IsNullOrEmpty(Request["docID"]))
            {
                long docID;
                if (long.TryParse(Request["docID"], out docID))
                {
                    feed = storage.GetFeed(docID);
                    PageTitle = NewsResource.NewsEditBreadCrumbsNews;
                    Title = HeaderStringHelper.GetPageTitle(PageTitle);
                    _text = (feed != null ? feed.Text : "").HtmlEncode();
                }
            }
            else
            {
                _text = "";
                PageTitle = NewsResource.NewsAddBreadCrumbsNews;
                Title = HeaderStringHelper.GetPageTitle(NewsResource.NewsAddBreadCrumbsNews);
            }

            if (!IsPostBack)
            {
                BindNewsTypes();

                if (feed != null)
                {
                    if (!CommunitySecurity.CheckPermissions(feed, NewsConst.Action_Edit))
                    {
                        Response.Redirect(FeedUrls.MainPageUrl, true);
                    }
                    feedName.Text = feed.Caption;
                    _text = feed.Text;
                    FeedId = feed.Id;
                    feedType.SelectedIndex = (int)Math.Log((int)feed.FeedType, 2);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request["type"]))
                    {
                        var requestFeedType = (FeedType)Enum.Parse(typeof(FeedType), Request["type"], true);
                        var feedTypeInfo = FeedTypeInfo.FromFeedType(requestFeedType);
                        var item = feedType.Items.FindByText(feedTypeInfo.TypeName);

                        feedType.SelectedValue = item.Value;
                        feedType.SelectedIndex = (int)Math.Log((int)requestFeedType, 2);
                    }
                }
            }
            else
            {
                var control = FindControl(Request.Params["__EVENTTARGET"]);
                if (lbCancel.Equals(control))
                {
                    CancelFeed(sender, e);
                }
                else
                {
                    SaveFeed();
                }
            }

            RenderScripts();
        }

        protected void RenderScripts()
        {
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");

            //Page.RegisterInlineScript("ckeditorConnector.load(function () {window.newsEditor = jq('#ckEditor').ckeditor({ toolbar : 'ComNews', extraPlugins: '', filebrowserUploadUrl: '" + RedirectUpload() + @"'}).editor;});");
            Page.RegisterInlineScript("ckeditorConnector.load(function () {" +
                          "window.newsEditor = CKEDITOR.replace('ckEditor', { toolbar : 'ComBlog', filebrowserUploadUrl: '" + RedirectUpload() + "'});" +
                          "window.newsEditor.on('change',  function() {if (this.getData() == '') {jq('#btnPreview').addClass('disable');} else {jq('#btnPreview').removeClass('disable');}});" +
                           "});");

            var scriptSb = new System.Text.StringBuilder();
            scriptSb.AppendLine(@"function FeedPrevShow(text) {
                    jq('#feedPrevDiv_Caption').val(jq('#" + feedName.ClientID + @"').val());
                    jq('#feedPrevDiv_Body').html(text);
                    jq('#feedPrevDiv').show();
                    jq.scrollTo(jq('#feedPrevDiv').position().top, {speed:500});
                }"
                );
            scriptSb.AppendLine(@"function HidePreview() {
                    jq('#feedPrevDiv').hide();
                    jq.scrollTo(jq('#newsCaption').position().top, {speed:500});
                }"
                );
            scriptSb.AppendLine(@"jq(function() {
                    jq('#" + feedType.ClientID + @"').tlCombobox();
                    jq('#" + feedType.ClientID + @"').removeClass('display-none');
                });"
                );

            Page.RegisterInlineScript(scriptSb.ToString(), true, false);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreviewFull(string html)
        {
            return HtmlUtility.GetFull(html);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public FeedAjaxInfo FeedPreview(string captionFeed, string bodyFeed)
        {
            var feed = new FeedAjaxInfo
                {
                    FeedCaption = captionFeed,
                    FeedText = HtmlUtility.GetFull(bodyFeed),
                    Date = TenantUtil.DateTimeNow().Ago(),
                    UserName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).RenderProfileLink(CommunityProduct.ID)
                };
            return feed;
        }

        protected void CancelFeed(object sender, EventArgs e)
        {
            var url = FeedUrls.MainPageUrl;

            if (FeedId != 0)
            {
                CommonControlsConfigurer.FCKEditingCancel("news", FeedId.ToString());
                url += "?docid=" + FeedId.ToString();
            }
            else
                CommonControlsConfigurer.FCKEditingCancel("news");

            Response.Redirect(url, true);
        }

        protected void SaveFeed()
        {

            if (string.IsNullOrEmpty(feedName.Text))
            {
                ((NewsMaster)Master).SetInfoMessage(NewsResource.RequaredFieldValidatorCaption, InfoType.Alert);
                return;
            }

            var storage = FeedStorageFactory.Create();
            var isEdit = (FeedId != 0);
            var feed = isEdit ? storage.GetFeed(FeedId) : new FeedNews();
            feed.Caption = feedName.Text;
            feed.Text = (Request["news_text"] ?? "");
            feed.FeedType = (FeedType)int.Parse(feedType.SelectedValue, CultureInfo.CurrentCulture);
            storage.SaveFeed(feed, isEdit, FeedType.News);

            CommonControlsConfigurer.FCKEditingComplete("news", feed.Id.ToString(), feed.Text, isEdit);

            Response.Redirect(FeedUrls.GetFeedUrl(feed.Id, Info.UserId));
        }

        protected string RedirectUpload()
        {
            return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port,
                                 VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?esid=news&newEditor=true" + (FeedId != 0 ? "&iid=" + FeedId.ToString() : ""));
        }
    }

    public class FeedAjaxInfo
    {
        public string FeedCaption { get; set; }

        public string FeedText { get; set; }

        public string Date { get; set; }

        public string UserName { get; set; }
    }
}