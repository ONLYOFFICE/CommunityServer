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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Forum.Resources;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.ElasticSearch;
using ASC.Forum;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Community.Search;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.UserControls.Forum
{
    public class ForumAttachmentUploadHanler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult { Success = false };
            try
            {
                if (FileToUpload.HasFilesToUpload(context))
                {
                    var settingsID = new Guid(context.Request["SettingsID"]);
                    var settings = Community.Forum.ForumManager.Settings;
                    var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, Convert.ToInt32(context.Request["ThreadID"]));
                    if (thread == null) return result;

                    var forumManager = settings.ForumManager;
                    var offsetPhysicalPath = string.Empty;
                    forumManager.GetAttachmentVirtualDirPath(thread, settingsID, new Guid(context.Request["UserID"]), out offsetPhysicalPath);

                    var file = new FileToUpload(context);

                    var newFileName = GetFileName(file.FileName);
                    var origFileName = newFileName;

                    var i = 1;
                    var store = forumManager.GetStore();
                    while (store.IsFile(offsetPhysicalPath + "\\" + newFileName))
                    {
                        var ind = origFileName.LastIndexOf(".");
                        newFileName = ind != -1 ? origFileName.Insert(ind, "_" + i.ToString()) : origFileName + "_" + i.ToString();
                        i++;
                    }

                    result.FileName = newFileName;
                    result.FileURL = store.Save(offsetPhysicalPath + "\\" + newFileName, file.InputStream).ToString();
                    result.Data = new
                        {
                            OffsetPhysicalPath = offsetPhysicalPath + "\\" + newFileName,
                            FileName = newFileName,
                            Size = file.ContentLength,
                            ContentType = file.FileContentType,
                            SettingsID = settingsID
                        };
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }

    public enum NewPostType
    {
        Post = 0,
        Topic = 1,
        Poll = 2
    }

    public enum PostAction
    {
        Normal = 0,
        Quote = 1,
        Reply = 2,
        Edit = 3
    }

    public enum SubscriveViewType
    {
        Checked,
        Unchecked,
        Disable
    }


    internal class TagComparer : IEqualityComparer<Tag>
    {

        #region IEqualityComparer<Tag> Members

        public bool Equals(Tag x, Tag y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Tag obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            return string.IsNullOrEmpty(obj.Name) ? 0 : obj.Name.GetHashCode();
        }

        #endregion
    }

    [AjaxNamespace("PostCreator")]
    public partial class NewPostControl : UserControl,
                                          INotifierView,
                                          ISubscriberView,
                                          IContextInitializer
    {
        [Serializable]
        internal class FileObj
        {
            public string fileName { get; set; }
            public string size { get; set; }
            public string offsetPhysicalPath { get; set; }
            public string contentType { get; set; }
        }

        public Guid SettingsID { get; set; }
        public NewPostType PostType { get; set; }
        public PostAction PostAction { get; set; }
        public Topic Topic { get; set; }
        public Thread Thread { get; set; }
        public Post ParentPost { get; set; }
        public Post EditedPost { get; set; }

        protected string _text = "";
        private string _subject = "";
        private string _tagString = "";
        private ForumManager _forumManager;
        private bool _isSelectForum;
        private List<ThreadCategory> _categories = null;
        private List<Thread> _threads = null;

        private List<FileObj> Attachments { get; set; }

        protected string _errorMessage = "";
        protected SubscriveViewType _subscribeViewType;
        protected PostTextFormatter _formatter = PostTextFormatter.FCKEditor;
        protected UserInfo _currentUser = null;
        protected Settings _settings;
        protected int _threadForAttachFiles = 0;
        protected string _attachmentsString = "";

        private void InitScripts()
        {
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");

            //Page.RegisterInlineScript("ckeditorConnector.load(function () {ForumManager.forumEditor = jq('#ckEditor').ckeditor({ toolbar : 'ComForum', filebrowserUploadUrl: '" + RenderRedirectUpload() + @"'}).editor;});");
            Page.RegisterInlineScript("ckeditorConnector.load(function () {" +
              "ForumManager.forumEditor = CKEDITOR.replace('ckEditor', { toolbar : 'ComBlog', filebrowserUploadUrl: '" + RenderRedirectUpload() + "'});" +
              "ForumManager.forumEditor.on('change',  function() {if (this.getData() == '') {jq('#btnPreview').addClass('disable');} else {jq('#btnPreview').removeClass('disable');}});" +
               "});");

            var script = @"FileUploadManager.Init({
                        DropZone: 'forum_uploadDialogContainer',
                        TargetContainerID: 'forum_uploadContainer',
                        BrowseButton: 'forum_uploadButton',
                        MaxSize: '" + SetupInfo.MaxUploadSize + @"',
                        FileUploadHandler: 'ASC.Web.UserControls.Forum.ForumAttachmentUploadHanler,ASC.Web.Community',
                        Data: { 'SettingsID': '" + SettingsID + @"',
                            'ThreadID': '" + _threadForAttachFiles + @"',
                            'UserID': '" + SecurityContext.CurrentAccount.ID + @"'
                        },
                        OverAllProcessHolder: jq('#forum_overallprocessHolder'),
                        OverAllProcessBarCssClass: 'forum_overAllProcessBarCssClass',
                        OverallProgressText: '" + ForumUCResource.OverallProgress + @"',
                        DeleteLinkCSSClass: 'forum_deleteLinkCSSClass',
                        LoadingImageCSSClass: 'forum_loadingCSSClass',
                        CompleteCSSClass: 'forum_completeCSSClass',
                        DeleteAfterUpload: false,
                        Events:
                            {
                                OnPreUploadStart: function() { 
                                    FileUploadManager.DisableBrowseBtn(true);
                                    ForumManager.BlockButtons();
                                },
                                OnPostUploadStop: function() { 
                                    jq('#forum_attachments').val(JSON.stringify(FileUploadManager.uploadedFiles));
                                    FileUploadManager.DisableBrowseBtn(false);
                                    ForumManager.UnblockButtons();
                                }
                            },
                        EmptyFileErrorMsg: """ + ForumUCResource.EmptyFileErrorMsg + @""",
                        FileSizeErrorMsg: """ + ForumUCResource.FileSizeErrorMsg + @"""
                    });";

            Page.RegisterInlineScript(script);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _settings = ASC.Web.Community.Forum.ForumManager.Settings;
            _forumManager = _settings.ForumManager;
            _currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            Utility.RegisterTypeForAjax(typeof(TagSuggest));
            Utility.RegisterTypeForAjax(this.GetType());
            Utility.RegisterTypeForAjax(typeof(PostControl));

            Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js", "~/js/uploader/jquery.fileuploadmanager.js");

            PostType = NewPostType.Topic;
            PostAction = PostAction.Normal;


            int idTopic = 0;
            int idThread = 0;


            if (!String.IsNullOrEmpty(Request[_settings.TopicParamName]))
            {
                try
                {
                    idTopic = Convert.ToInt32(Request[_settings.TopicParamName]);
                }
                catch
                {
                    idTopic = 0;
                }

                if (idTopic == 0)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }
                else
                    PostType = NewPostType.Post;

            }
            else if (!String.IsNullOrEmpty(Request[_settings.ThreadParamName]))
            {
                try
                {
                    idThread = Convert.ToInt32(Request[_settings.ThreadParamName]);
                }
                catch
                {
                    idThread = 0;
                }

                if (idThread == 0)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }
                else
                    PostType = NewPostType.Topic;

                int topicType = 0;
                try
                {
                    topicType = Convert.ToInt32(Request[_settings.PostParamName]);
                }
                catch
                {
                    topicType = 0;
                }
                if (topicType == 1)
                    PostType = NewPostType.Poll;
            }
            else
            {
                int topicType = 0;
                try
                {
                    topicType = Convert.ToInt32(Request[_settings.PostParamName]);
                }
                catch
                {
                    topicType = 0;
                }
                if (topicType == 1)
                    PostType = NewPostType.Poll;


                if (IsPostBack)
                {
                    if (!String.IsNullOrEmpty(Request["forum_thread_id"]))
                    {
                        try
                        {
                            idThread = Convert.ToInt32(Request["forum_thread_id"]);
                        }
                        catch
                        {
                            idThread = 0;
                        }
                    }
                }
                else
                {
                    ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out _categories, out _threads);


                    foreach (var thread in _threads)
                    {
                        bool isAllow = false;
                        if (PostType == NewPostType.Topic)
                            isAllow = _forumManager.ValidateAccessSecurityAction(ForumAction.TopicCreate, thread);
                        else if (PostType == NewPostType.Poll)
                            isAllow = _forumManager.ValidateAccessSecurityAction(ForumAction.PollCreate, thread);

                        if (isAllow)
                        {
                            idThread = thread.ID;
                            Thread = thread;
                            break;
                        }
                    }
                }

                if (idThread == 0)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }

                _isSelectForum = true;
            }


            if (PostType == NewPostType.Topic || PostType == NewPostType.Poll)
            {
                if (Thread == null)
                    Thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, idThread);

                if (Thread == null)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }

                if (Thread.Visible == false)
                    Response.Redirect(_settings.StartPageAbsolutePath);

                _threadForAttachFiles = Thread.ID;

                if (PostType == NewPostType.Topic)
                {
                    if (!_forumManager.ValidateAccessSecurityAction(ForumAction.TopicCreate, Thread))
                    {
                        Response.Redirect(_settings.StartPageAbsolutePath);
                        return;
                    }
                }
                else if (PostType == NewPostType.Poll)
                {
                    if (!_forumManager.ValidateAccessSecurityAction(ForumAction.PollCreate, Thread))
                    {
                        Response.Redirect(_settings.StartPageAbsolutePath);
                        return;
                    }
                }
            }
            else if (PostType == NewPostType.Post)
            {
                int parentPostId = 0;

                if (!String.IsNullOrEmpty(Request[_settings.ActionParamName]) && !String.IsNullOrEmpty(Request[_settings.PostParamName]))
                {
                    try
                    {
                        PostAction = (PostAction)Convert.ToInt32(Request[_settings.ActionParamName]);
                    }
                    catch
                    {
                        PostAction = PostAction.Normal;
                    }
                    try
                    {
                        parentPostId = Convert.ToInt32(Request[_settings.PostParamName]);
                    }
                    catch
                    {
                        parentPostId = 0;
                        PostAction = PostAction.Normal;
                    }
                }


                Topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, idTopic);
                if (Topic == null)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }

                if (new Thread() { ID = Topic.ThreadID }.Visible == false)
                {
                    Response.Redirect(_settings.StartPageAbsolutePath);
                    return;
                }



                var recentPosts = ForumDataProvider.GetRecentTopicPosts(TenantProvider.CurrentTenantID, Topic.ID, 5,
                                                                        (PostAction == PostAction.Normal || PostAction == PostAction.Edit) ? 0 : parentPostId);

                if (recentPosts.Count > 0)
                {
                    Label titleRecentPosts = new Label();
                    titleRecentPosts.Text = "<div class=\"headerPanelSmall-splitter\" style='margin-top:20px;'><b>" + Resources.ForumUCResource.RecentPostFromTopic + ":</b></div>";
                    _recentPostsHolder.Controls.Add(titleRecentPosts);


                    int i = 0;
                    foreach (Post post in recentPosts)
                    {
                        PostControl postControl = (PostControl)LoadControl(_settings.UserControlsVirtualPath + "/PostControl.ascx");
                        postControl.Post = post;
                        postControl.SettingsID = SettingsID;
                        postControl.IsEven = (i%2 == 0);
                        _recentPostsHolder.Controls.Add(postControl);
                        i++;
                    }
                }


                _threadForAttachFiles = Topic.ThreadID;

                if (PostAction == PostAction.Quote || PostAction == PostAction.Reply || PostAction == PostAction.Normal)
                {
                    if (!_forumManager.ValidateAccessSecurityAction(ForumAction.PostCreate, Topic))
                    {
                        Response.Redirect(_settings.StartPageAbsolutePath);
                        return;
                    }

                    if (PostAction == PostAction.Quote || PostAction == PostAction.Reply)
                    {
                        ParentPost = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, parentPostId);

                        if (ParentPost == null)
                        {
                            Response.Redirect(_settings.StartPageAbsolutePath);
                            return;
                        }
                    }

                    _subject = Topic.Title;

                    if (PostAction == PostAction.Quote)
                    {
                        _text = String.Format(@"<div class=""mainQuote"">
                                                    <div class=""quoteCaption""><span class=""bold"">{0}</span>&nbsp;{1}</div>
                                                    <div id=""quote"" >
                                                    <div class=""bord""><div class=""t""><div class=""r"">
                                                    <div class=""b""><div class=""l""><div class=""c"">
                                                        <div class=""reducer"">
                                                            {2}
                                                        </div>
                                                    </div></div></div>
                                                    </div></div></div>
                                                </div>
                                                </div><br/>",
                                              ParentPost.Poster.DisplayUserName(),
                                              ForumUCResource.QuoteCaptioon_Wrote + ":",
                                              ParentPost.Text);
                    }

                    if (PostAction == PostAction.Reply)
                        _text = "<span class=\"headerPanelSmall-splitter\"><b>To: " + ParentPost.Poster.DisplayUserName() + "</b></span><br/><br/>";

                }
                else if (PostAction == PostAction.Edit)
                {
                    EditedPost = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, parentPostId);

                    if (EditedPost == null)
                    {
                        Response.Redirect(_settings.StartPageAbsolutePath);
                        return;
                    }

                    if (!_forumManager.ValidateAccessSecurityAction(ForumAction.PostEdit, EditedPost))
                    {
                        Response.Redirect("Default.aspx");
                        return;
                    }

                    Topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, EditedPost.TopicID);
                    _text = EditedPost.Text;
                    _subject = EditedPost.Subject;
                }
            }

            if (PostType != NewPostType.Poll)
                _pollMaster.Visible = false;
            else
                _pollMaster.QuestionFieldID = "forum_subject";


            InitScripts();


            if (IsPostBack)
            {
                _attachmentsString = Request["forum_attachments"] ?? "";

                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Attachments = jsSerializer.Deserialize<List<FileObj>>(_attachmentsString);

                #region IsPostBack

                if (PostType == NewPostType.Topic)
                {
                    _subject = string.IsNullOrEmpty(Request["forum_subject"]) ? string.Empty : Request["forum_subject"].Trim();
                }
                else if (PostType == NewPostType.Poll)
                {
                    _subject = string.IsNullOrEmpty(_pollMaster.Name) ? string.Empty : _pollMaster.Name.Trim();
                }

                if (String.IsNullOrEmpty(_subject) && PostType != NewPostType.Post)
                {
                    _subject = "";
                    _errorMessage = "<div class=\"errorBox\">" + Resources.ForumUCResource.ErrorSubjectEmpty + "</div>";
                    return;
                }

                if (!String.IsNullOrEmpty(Request["forum_tags"]))
                {
                    Regex r = new Regex(@"\s*,\s*", RegexOptions.Compiled);
                    _tagString = r.Replace(Request["forum_tags"].Trim(), ",");
                }

                _text = Request["forum_text"].Trim();
                if (String.IsNullOrEmpty(_text))
                {
                    _text = "";

                    Page.RegisterInlineScript("ForumManager.ShowInfoMessage('" + Resources.ForumUCResource.ErrorTextEmpty + "');");
                    return;
                }
                else
                {
                    _text = _text.Replace("<br />", "<br />\r\n").TrimEnd('\r', '\n');

                }

                if (String.IsNullOrEmpty(Request["forum_topSubscription"]))
                    _subscribeViewType = SubscriveViewType.Disable;
                else
                {
                    if (String.Equals(Request["forum_topSubscription"], "1", StringComparison.InvariantCultureIgnoreCase))
                        _subscribeViewType = SubscriveViewType.Checked;
                    else
                        _subscribeViewType = SubscriveViewType.Unchecked;
                }


                if (PostType == NewPostType.Post)
                {
                    if (PostAction == PostAction.Edit)
                    {

                        EditedPost.Subject = _subject;
                        EditedPost.Text = _text;
                        EditedPost.Formatter = _formatter;

                        try
                        {
                            ForumDataProvider.UpdatePost(TenantProvider.CurrentTenantID, EditedPost.ID, EditedPost.Subject, EditedPost.Text, EditedPost.Formatter);
                            FactoryIndexer<PostWrapper>.UpdateAsync(EditedPost);

                            if (IsAllowCreateAttachment)
                                CreateAttachments(EditedPost);
                            int postsOnPageCount = -1;
                            int postsPageNumber = -1;

                            CommonControlsConfigurer.FCKEditingComplete(_settings.FileStoreModuleID, EditedPost.ID.ToString(), EditedPost.Text, true);
                            var redirectUrl = _settings.PostPageAbsolutePath + "&t=" + EditedPost.TopicID.ToString();
                            if (!String.IsNullOrEmpty(Request["p"]))
                            {
                                postsPageNumber = Convert.ToInt32(Request["p"]);
                                redirectUrl += "&p=" + postsPageNumber.ToString();
                            }
                            if (!String.IsNullOrEmpty(Request["size"]))
                            {
                                postsOnPageCount = Convert.ToInt32(Request["size"]);
                                redirectUrl += "&size=" + postsOnPageCount.ToString();
                            }
                            Response.Redirect(redirectUrl);
                            return;
                        }
                        catch (Exception ex)
                        {
                            _errorMessage = "<div class=\"errorBox\">" + ex.Message.HtmlEncode() + "</div>";
                            return;
                        }

                    }
                    else
                    {

                        var post = new Post(Topic.Title, _text);
                        post.TopicID = Topic.ID;
                        post.ParentPostID = ParentPost == null ? 0 : ParentPost.ID;
                        post.Formatter = _formatter;
                        post.IsApproved = _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Topic);

                        try
                        {
                            post.ID = ForumDataProvider.CreatePost(TenantProvider.CurrentTenantID, post.TopicID, post.ParentPostID,
                                                                   post.Subject, post.Text, post.IsApproved, post.Formatter);
                            FactoryIndexer<PostWrapper>.IndexAsync(post);
                            Topic.PostCount++;

                            CommonControlsConfigurer.FCKEditingComplete(_settings.FileStoreModuleID, post.ID.ToString(), post.Text, false);

                            if (IsAllowCreateAttachment)
                                CreateAttachments(post);

                            NotifyAboutNewPost(post);

                            if (_subscribeViewType != SubscriveViewType.Disable)
                            {
                                _forumManager.PresenterFactory.GetPresenter<ISubscriberView>().SetView(this);
                                if (this.GetSubscriptionState != null)
                                    this.GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic, Topic.ID.ToString(), SecurityContext.CurrentAccount.ID));

                                if (this.IsSubscribe && _subscribeViewType == SubscriveViewType.Unchecked)
                                {
                                    if (this.UnSubscribe != null)
                                        this.UnSubscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic, Topic.ID.ToString(), SecurityContext.CurrentAccount.ID));
                                }
                                else if (!this.IsSubscribe && _subscribeViewType == SubscriveViewType.Checked)
                                {
                                    if (this.Subscribe != null)
                                        this.Subscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic, Topic.ID.ToString(), SecurityContext.CurrentAccount.ID));
                                }
                            }

                            int numb_page = Convert.ToInt32(Math.Ceiling(Topic.PostCount/(_settings.PostCountOnPage*1.0)));

                            var postURL = _settings.LinkProvider.Post(post.ID, Topic.ID, numb_page);

                            Response.Redirect(postURL);
                            return;
                        }
                        catch (Exception ex)
                        {
                            _errorMessage = "<div class=\"errorBox\">" + ex.Message.HtmlEncode() + "</div>";
                            return;
                        }

                        #endregion
                    }
                }
                if (PostType == NewPostType.Topic || PostType == NewPostType.Poll)
                {
                    if (PostType == NewPostType.Poll && _pollMaster.AnswerVariants.Count < 2)
                    {
                        _errorMessage = "<div class=\"errorBox\">" + Resources.ForumUCResource.ErrorPollVariantCount + "</div>";
                        return;
                    }

                    try
                    {
                        var topic = new Topic(_subject, TopicType.Informational);
                        topic.ThreadID = Thread.ID;
                        topic.Tags = CreateTags(Thread);
                        topic.Type = (PostType == NewPostType.Poll ? TopicType.Poll : TopicType.Informational);

                        topic.ID = ForumDataProvider.CreateTopic(TenantProvider.CurrentTenantID, topic.ThreadID, topic.Title, topic.Type);

                        foreach (var ace in ASC.Forum.Module.Constants.Aces)
                        {
                            CoreContext.AuthorizationManager.AddAce(new AzRecord(SecurityContext.CurrentAccount.ID, ace, ASC.Common.Security.Authorizing.AceType.Allow, topic));
                        }

                        Topic = topic;
                        FactoryIndexer<TopicWrapper>.IndexAsync(topic);

                        foreach (var tag in topic.Tags)
                        {
                            if (tag.ID == 0)
                                ForumDataProvider.CreateTag(TenantProvider.CurrentTenantID, topic.ID, tag.Name, tag.IsApproved);
                            else
                                ForumDataProvider.AttachTagToTopic(TenantProvider.CurrentTenantID, tag.ID, topic.ID);
                        }

                        var post = new Post(topic.Title, _text);
                        post.TopicID = topic.ID;
                        post.ParentPostID = 0;
                        post.Formatter = _formatter;
                        post.IsApproved = _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Topic);

                        post.ID = ForumDataProvider.CreatePost(TenantProvider.CurrentTenantID, post.TopicID, post.ParentPostID,
                                                               post.Subject, post.Text, post.IsApproved, post.Formatter);

                        FactoryIndexer<PostWrapper>.IndexAsync(post);
                        CommonControlsConfigurer.FCKEditingComplete(_settings.FileStoreModuleID, post.ID.ToString(), post.Text, false);

                        if (IsAllowCreateAttachment)
                            CreateAttachments(post);

                        if (PostType == NewPostType.Poll)
                        {
                            var answerVariants = new List<string>();
                            foreach (var answVariant in _pollMaster.AnswerVariants)
                                answerVariants.Add(answVariant.Name);

                            topic.QuestionID = ForumDataProvider.CreatePoll(TenantProvider.CurrentTenantID, topic.ID,
                                                                            _pollMaster.Singleton ? QuestionType.OneAnswer : QuestionType.SeveralAnswer,
                                                                            topic.Title, answerVariants);

                            var topicWrapper = (TopicWrapper) topic;

                            FactoryIndexer<TopicWrapper>.IndexAsync(topicWrapper);
                        }

                        NotifyAboutNewPost(post);

                        if (_subscribeViewType == SubscriveViewType.Checked)
                        {
                            _forumManager.PresenterFactory.GetPresenter<ISubscriberView>().SetView(this);
                            if (this.Subscribe != null)
                                this.Subscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic, topic.ID.ToString(), SecurityContext.CurrentAccount.ID));
                        }

                        Response.Redirect(_settings.LinkProvider.Post(post.ID, topic.ID, 1));
                        return;

                    }
                    catch (Exception ex)
                    {
                        _errorMessage = "<div class=\"errorBox\">" + ex.Message.HtmlEncode() + "</div>";
                        return;
                    }
                }
            }
            else
            {
                _forumManager.PresenterFactory.GetPresenter<ISubscriberView>().SetView(this);
                if (PostType == NewPostType.Poll || PostType == NewPostType.Topic)
                {
                    if (this.GetSubscriptionState != null)
                        this.GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInThread, Thread.ID.ToString(), SecurityContext.CurrentAccount.ID));

                    if (this.IsSubscribe)
                        _subscribeViewType = SubscriveViewType.Disable;
                    else
                        _subscribeViewType = SubscriveViewType.Checked;
                }
                else if (PostType == NewPostType.Post && PostAction != PostAction.Edit)
                {
                    if (this.GetSubscriptionState != null)
                        this.GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInThread, Topic.ThreadID.ToString(), SecurityContext.CurrentAccount.ID));

                    if (this.IsSubscribe)
                        _subscribeViewType = SubscriveViewType.Disable;
                    else
                    {
                        if (SubscriptionConstants.SubscriptionProvider.IsUnsubscribe(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name),
                                                                                     SubscriptionConstants.NewPostInTopic, Topic.ID.ToString()))
                        {
                            _subscribeViewType = SubscriveViewType.Unchecked;
                        }
                        else
                            _subscribeViewType = SubscriveViewType.Checked;
                    }
                }
                else
                    _subscribeViewType = SubscriveViewType.Disable;
            }
        }

        private void NotifyAboutNewPost(Post post)
        {
            int numb_page = Convert.ToInt32(Math.Ceiling(Topic.PostCount/(_settings.PostCountOnPage*1.0)));

            string hostUrl = CommonLinkUtility.ServerRootPath;

            string topicURL = hostUrl + _settings.LinkProvider.PostList(Topic.ID);
            string postURL = hostUrl + _settings.LinkProvider.Post(post.ID, Topic.ID, numb_page);
            string threadURL = hostUrl + _settings.LinkProvider.TopicList(Topic.ThreadID);
            string userURL = hostUrl + CommonLinkUtility.GetUserProfile(post.PosterID);

            string postText = Regex.Replace(HtmlUtility.GetFull(post.Text), @"\r\n?|\n", string.Empty);

            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name));
            try
            {
                SubscriptionConstants.NotifyClient.AddInterceptor(initatorInterceptor);

                var poster = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                _forumManager.PresenterFactory.GetPresenter<INotifierView>().SetView(this);
                SubscriptionConstants.NotifyClient.BeginSingleRecipientEvent(SubscriptionConstants.SyncName);
                if (SendNotify != null)
                {
                    if (PostType == NewPostType.Poll || PostType == NewPostType.Topic)
                        SendNotify(this, new NotifyEventArgs(SubscriptionConstants.NewTopicInForum,
                                                             null) { ThreadTitle = Topic.ThreadTitle, TopicId = Topic.ID, TopicTitle = Topic.Title, Poster = poster, Date = post.CreateDate.ToShortString(), PostURL = postURL, TopicURL = topicURL, ThreadURL = threadURL, UserURL = userURL, PostText = postText });


                    SendNotify(this, new NotifyEventArgs(SubscriptionConstants.NewPostInThread,
                                                         Topic.ThreadID.ToString()) { ThreadTitle = Topic.ThreadTitle, TopicTitle = Topic.Title, Poster = poster, Date = post.CreateDate.ToShortString(), PostURL = postURL, TopicURL = topicURL, ThreadURL = threadURL, UserURL = userURL, PostText = postText, TopicId = Topic.ID, PostId = post.ID, TenantId = Topic.TenantID });

                    SendNotify(this, new NotifyEventArgs(SubscriptionConstants.NewPostInTopic,
                                                         Topic.ID.ToString()) { ThreadTitle = Topic.ThreadTitle, TopicTitle = Topic.Title, Poster = poster, Date = post.CreateDate.ToShortString(), PostURL = postURL, TopicURL = topicURL, ThreadURL = threadURL, UserURL = userURL, PostText = postText, TopicId = Topic.ID, PostId = post.ID, TenantId = Topic.TenantID });


                }
                SubscriptionConstants.NotifyClient.EndSingleRecipientEvent(SubscriptionConstants.SyncName);
            }
            finally
            {
                SubscriptionConstants.NotifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }

        protected void TopicHeader()
        {
            if (PostType == NewPostType.Post || PostType == NewPostType.Poll)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"headerPanel-splitter requiredField\">");
            sb.Append("<span class=\"requiredErrorText\"></span>");
            sb.Append("<div class=\"headerPanelSmall-splitter headerPanelSmall\"><b>");
            sb.Append(Resources.ForumUCResource.Topic);
            sb.Append(":</b></div>");
            sb.Append("<div>");
            sb.Append("<input class=\"textEdit\" style=\"width:100%;\" maxlength=\"450\" name=\"forum_subject\" id=\"forum_subject\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(_subject) + "\" />");
            sb.Append("</div>");
            sb.Append("</div>");
            Response.Write(sb.ToString());
        }

        protected string RenderForumSelector()
        {
            if (!_isSelectForum)
                return "";

            if (_threads == null)
            {
                ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out _categories, out _threads);

                _threads.RemoveAll(t => !t.Visible);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"headerPanel-splitter\">");
            sb.Append("<div class=\"headerPanelSmall-splitter\"><b>");
            sb.Append(Resources.ForumUCResource.Thread);
            sb.Append(":</b></div>");

            sb.Append("<div>");
            sb.Append("<select name=\"forum_thread_id\" class=\"comboBox\" style='width:400px;'>");

            var grouppedThread = _threads.GroupBy(t => t.CategoryID);

            foreach (var forumGroup in grouppedThread)
            {
                var category = _categories.FirstOrDefault(c => c.ID == forumGroup.Key);

                if(category != null)
                    sb.Append("<optgroup label=\"" + category.Title  + "\">");

                foreach (var forum in forumGroup)
                {
                    bool isAllow = false;
                    if (PostType == NewPostType.Topic)
                        isAllow = _forumManager.ValidateAccessSecurityAction(ForumAction.TopicCreate, forum);
                    else if (PostType == NewPostType.Poll)
                        isAllow = _forumManager.ValidateAccessSecurityAction(ForumAction.PollCreate, forum);

                    if (isAllow)
                        sb.Append("<option value=\"" + forum.ID + "\">" + forum.Title.HtmlEncode() + "</option>");
                }

                if (category != null)
                    sb.Append("</optgroup>");
            }

            sb.Append("</select>");
            sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }



        private List<Tag> CreateTags(Thread thread)
        {
            if (string.IsNullOrEmpty(_tagString.Trim(',')) || !_forumManager.ValidateAccessSecurityAction(ForumAction.TagCreate, thread))
            {
                return new List<Tag>();
            }

            List<Tag> list;
            List<Tag> existingTags = ForumDataProvider.GetAllTags(TenantProvider.CurrentTenantID);

            var newTags = (from tagName in _tagString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                           where !existingTags.Exists(et => et.Name.Equals(tagName, StringComparison.InvariantCultureIgnoreCase))
                           select new Tag() { ID = 0, Name = tagName }).Distinct(new TagComparer());

            var exTags = from exTag in existingTags
                         from tagName in _tagString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                         where tagName.Equals(exTag.Name, StringComparison.InvariantCultureIgnoreCase)
                         select exTag;

            list = new List<Tag>(newTags.ToArray());
            list.AddRange(exTags.ToArray());
            return list;
        }

        protected void AddTags()
        {
            if (PostType != NewPostType.Topic && PostType != NewPostType.Poll)
                return;

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.TagCreate, Thread))
                return;

            var sb = new StringBuilder();
            sb.AppendLine("var ForumTagSearchHelper = new SearchHelper('forum_tags','forum_sh_item','forum_sh_itemselect','',\"ForumManager.SaveSearchTags(\'forum_search_tags\',ForumTagSearchHelper.SelectedItem.Value,ForumTagSearchHelper.SelectedItem.Help);\",\"TagSuggest\", \"GetSuggest\",\"'" + _settings.ID + "',\",true,false);");
            Page.RegisterInlineScript(sb.ToString());
            
            sb = new StringBuilder();
            sb.Append("<div class=\"headerPanel-splitter\">");
            sb.Append("<div class=\"headerPanelSmall-splitter\"><b>" + Resources.ForumUCResource.Tags + ":</b></div>");
            sb.Append("<div>");
            sb.Append("<input autocomplete=\"off\" class=\"textEdit\" style=\"width:100%\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(_tagString) + "\" maxlength=\"3000\" id=\"forum_tags\" name=\"forum_tags\"/>");
            sb.Append("<input type='hidden' id='forum_search_tags' name='forum_search_tags'/>");
            sb.Append("</div>");
            sb.Append("<div class=\"text-medium-describe\">" + Resources.ForumUCResource.HelpForTags + "</div>");
            sb.Append("</div>");

            Response.Write(sb.ToString());
        }

        #region Attachments

        public bool IsAllowCreateAttachment
        {
            get
            {
                if (PostType == NewPostType.Post)
                    return _forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentCreate, Topic);

                else if (PostType == NewPostType.Poll || PostType == NewPostType.Topic)
                    return _forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentCreate, Thread);

                return false;
            }
        }

        private void CreateAttachments(Post post)
        {
            if (Attachments != null)
            {
                foreach (var attachItem in Attachments)
                {
                    var attachment = new Attachment();
                    attachment.OffsetPhysicalPath = attachItem.offsetPhysicalPath;

                    if (_forumManager.GetStore().IsFile(attachment.OffsetPhysicalPath) == false)
                        continue;

                    attachment.PostID = post.ID;
                    attachment.Name = attachItem.fileName;
                    attachment.Size = int.Parse(attachItem.size);
                    attachment.MIMEContentType = attachItem.contentType;

                    attachment.ID = ForumDataProvider.CreateAttachment(TenantProvider.CurrentTenantID, post.ID, attachment.Name, attachment.OffsetPhysicalPath,
                                                                       attachment.Size, attachment.ContentType, attachment.MIMEContentType);

                    post.Attachments.Add(attachment);
                }
            }
        }

        #region Array Items Helper

        private static string GetNotNullArrayItem(string[] a, int index)
        {
            if (a == null || a.Length == 0)
            {
                return string.Empty;
            }
            return a.Length > index ? a[index] : string.Empty;
        }

        #endregion

        #endregion

        protected string RenderSubscription()
        {
            if (_subscribeViewType == SubscriveViewType.Disable)
                return "";

            return "<input id='forum_topSubscriptionState' name='forum_topSubscription' value='" + (_subscribeViewType == SubscriveViewType.Checked ? "1" : "0") + "' type='hidden'/><input id=\"forum_topSubscription\" " + (_subscribeViewType == SubscriveViewType.Checked ? "checked='checked'" : "") + " type=\"checkbox\"/><label style='margin-left:5px; vertical-align: top;' for=\"forum_topSubscription\">" + Resources.ForumUCResource.SubscribeOnTopic + "</label>";
        }


        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void RemoveAttachment(Guid settingsID, string offsetPath)
        {
            Community.Forum.ForumManager.Settings.ForumManager.RemoveAttachments(offsetPath);

        }

        #region INotifierView Members

        public event EventHandler<NotifyEventArgs> SendNotify;

        #endregion

        #region ISubscriberView Members

        public event EventHandler<SubscribeEventArgs> GetSubscriptionState;

        public bool IsSubscribe { get; set; }

        public event EventHandler<SubscribeEventArgs> Subscribe;

        public event EventHandler<SubscribeEventArgs> UnSubscribe;

        public event EventHandler<SubscribeEventArgs> UnSubscribeForSubscriptionType;

        #endregion


        protected string RenderRedirectUpload()
        {
            return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port,
                                 VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=" + _settings.FileStoreModuleID + (PostAction == PostAction.Edit ? "&iid=" + EditedPost.ID.ToString() : ""));
        }

        protected void OnUnSubscribeForSubscriptionType(INotifyAction action, string objId, Guid userId)
        {
            if (UnSubscribeForSubscriptionType != null) UnSubscribeForSubscriptionType(this, new SubscribeEventArgs(action, objId, userId));
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string CancelPost(Guid settingsID, string itemID)
        {
            var _settings = Community.Forum.ForumManager.Settings;
            if (String.IsNullOrEmpty(itemID) == false)
                CommonControlsConfigurer.FCKEditingCancel(_settings.FileStoreModuleID, itemID);
            else
                CommonControlsConfigurer.FCKEditingCancel(_settings.FileStoreModuleID);

            if (httpContext != null && httpContext.Request != null &&
                httpContext.Request.UrlReferrer != null &&
                !string.IsNullOrEmpty(httpContext.Request.UrlReferrer.Query))
            {
                var q = httpContext.Request.UrlReferrer.Query;
                var start = q.IndexOf("t=");

                if (start == -1)
                    start = q.IndexOf("f=");

                start += 2;


                var end = q.IndexOf("&", start, StringComparison.CurrentCultureIgnoreCase);
                if (end == -1)
                    end = q.Length;

                var t = q.Substring(start, end - start);

                return (q.IndexOf("t=") > 0 ? _settings.PostPageAbsolutePath + "t=" : _settings.TopicPageAbsolutePath + "f=") + t;
            }
            else
                return _settings.StartPageAbsolutePath;
        }

        #region IContextInitializer Members

        public void InitializeContext(HttpContext context)
        {
            httpContext = context;
        }

        private HttpContext httpContext = null;

        #endregion
    }
}