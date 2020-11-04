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
using ASC.Web.Community.News.Code;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Community.News.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Studio;
using ASC.Web.Studio.UserControls.Common.PollForm;
using ASC.Web.Studio.Utility;
using FeedNS = ASC.Web.Community.News.Code;

namespace ASC.Web.Community.News
{
    public partial class EditPoll : MainPage
    {
        protected string PageTitle { get; private set; }

        private RequestInfo info;

        private RequestInfo Info
        {
            get { return info ?? (info = new RequestInfo(Request)); }
        }

        public long FeedId
        {
            get { return ViewState["FeedID"] != null ? Convert.ToInt32(ViewState["FeedID"]) : 0; }
            set { ViewState["FeedID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CommunitySecurity.CheckPermissions(NewsConst.Action_Add))
                Response.Redirect(FeedUrls.MainPageUrl, true);

            var storage = FeedStorageFactory.Create();
            FeedNS.Feed feed = null;
            long docID = 0;
            if (!string.IsNullOrEmpty(Request["docID"]) && long.TryParse(Request["docID"], out docID))
            {
                feed = storage.GetFeed(docID);
            }
            if (!IsPostBack)
            {
                _errorMessage.Text = "";
                if (feed != null)
                {
                    if (!CommunitySecurity.CheckPermissions(feed, NewsConst.Action_Edit))
                    {
                        Response.Redirect(FeedUrls.MainPageUrl, true);
                    }

                    FeedId = docID;
                    var pollFeed = feed as FeedPoll;
                    if (pollFeed != null)
                    {
                        _pollMaster.QuestionFieldID = "feedName";
                        var question = pollFeed;
                        _pollMaster.Singleton = (question.PollType == FeedPollType.SimpleAnswer);
                        _pollMaster.Name = feed.Caption;
                        _pollMaster.ID = question.Id.ToString(CultureInfo.CurrentCulture);

                        foreach (var variant in question.Variants)
                        {
                            _pollMaster.AnswerVariants.Add(new PollFormMaster.AnswerViarint
                                {
                                    ID = variant.ID.ToString(CultureInfo.CurrentCulture),
                                    Name = variant.Name
                                });
                        }
                    }
                }
                else
                {
                    _pollMaster.QuestionFieldID = "feedName";
                }
            }
            else
            {
                SaveFeed();
            }

            if (feed != null)
            {
                PageTitle = NewsResource.NewsEditBreadCrumbsPoll;
                Title = HeaderStringHelper.GetPageTitle(PageTitle);
                lbCancel.NavigateUrl = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/") + "?docid=" + docID + Info.UserIdAttribute;
            }
            else
            {
                PageTitle = NewsResource.NewsAddBreadCrumbsPoll;
                Title = HeaderStringHelper.GetPageTitle(PageTitle);
                lbCancel.NavigateUrl = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/") + (string.IsNullOrEmpty(Info.UserIdAttribute) ? string.Empty : "?" + Info.UserIdAttribute.Substring(1));
            }

        }

        protected void SaveFeed()
        {
            if (String.IsNullOrEmpty(_pollMaster.Name))
            {
                _errorMessage.Text = "<div class='errorBox'>" + NewsResource.ErrorEmptyQuestion + "</div>";
                return;
            }

            if (_pollMaster.AnswerVariants.Count < 2)
            {
                _errorMessage.Text = "<div class='errorBox'>" + NewsResource.ErrorPollVariantCount + "</div>";
                return;
            }

            var isEdit = FeedId != 0;
            var storage = FeedStorageFactory.Create();

            var feed = isEdit ? (FeedPoll)storage.GetFeed(FeedId) : new FeedPoll();
            feed.Caption = _pollMaster.Name;
            feed.PollType = _pollMaster.Singleton ? FeedPollType.SimpleAnswer : FeedPollType.MultipleAnswer;

            int i = 0;
            foreach (var answVariant in _pollMaster.AnswerVariants)
            {
                FeedPollVariant answerVariant = null;
                try
                {
                    answerVariant = feed.Variants[i];
                }
                catch
                {
                }
                if (answerVariant == null)
                {
                    answerVariant = new FeedPollVariant();
                    feed.Variants.Add(answerVariant);
                }
                answerVariant.Name = answVariant.Name;
                i++;
            }
            while (i != feed.Variants.Count)
            {
                feed.Variants.RemoveAt(i);
            }

            storage.SaveFeed(feed, isEdit, FeedType.Poll);

            Response.Redirect(VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/") + "?docid=" + feed.Id + Info.UserIdAttribute);
        }
    }
}