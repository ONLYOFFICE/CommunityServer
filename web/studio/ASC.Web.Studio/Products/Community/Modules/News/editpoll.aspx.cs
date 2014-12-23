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
                (Master as NewsMaster).CurrentPageCaption = NewsResource.NewsEditBreadCrumbsPoll;
                Title = HeaderStringHelper.GetPageTitle(NewsResource.NewsEditBreadCrumbsPoll);
                lbCancel.NavigateUrl = VirtualPathUtility.ToAbsolute("~/products/community/modules/news/") + "?docid=" + docID + Info.UserIdAttribute;
            }
            else
            {
                (Master as NewsMaster).CurrentPageCaption = NewsResource.NewsAddBreadCrumbsPoll;
                Title = HeaderStringHelper.GetPageTitle(NewsResource.NewsAddBreadCrumbsPoll);
                lbCancel.NavigateUrl = VirtualPathUtility.ToAbsolute("~/products/community/modules/news/") + (string.IsNullOrEmpty(Info.UserIdAttribute) ? string.Empty : "?" + Info.UserIdAttribute.Substring(1));
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

            Response.Redirect(VirtualPathUtility.ToAbsolute("~/products/community/modules/news/") + "?docid=" + feed.Id + Info.UserIdAttribute);
        }
    }
}