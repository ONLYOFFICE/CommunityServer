/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.Studio.UserControls.Common.PollForm;
using AjaxPro;
using ASC.ElasticSearch;
using ASC.Forum;
using ASC.Web.Community.Search;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Web.UserControls.Forum
{
    public partial class TopicEditorControl : UserControl
    {  
        public Topic EditableTopic { get; set; }
        public Guid SettingsID { get; set; }
        public int topicId { get; set; }

        protected string _subject;
        protected string _errorMessage = "";
        protected Settings _settings;

        protected string PreviousPageUrl
        {
            get { return _forumManager.PreviousPage.Url; }
        }

        private string _tagString = "";
        private string _tagValues = "";
        private ForumManager _forumManager;

        protected void Page_Load(object sender, EventArgs e)
        {
            _settings = Community.Forum.ForumManager.Settings;
            _forumManager = _settings.ForumManager;

            Utility.RegisterTypeForAjax(typeof(TagSuggest));
            
            _subject = "";

            int idTopic = 0;
            if (!String.IsNullOrEmpty(Request[_settings.TopicParamName]))
            {
                try
                {
                    idTopic = Convert.ToInt32(Request[_settings.TopicParamName]);
                }
                catch { idTopic = 0; }
            }

            if (idTopic == 0)
            {
                Response.Redirect(_forumManager.PreviousPage.Url);
                return;
            }
            topicId = idTopic;

            EditableTopic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, idTopic);
            if (EditableTopic == null)
            {
                Response.Redirect(_forumManager.PreviousPage.Url);
                return;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.TopicEdit, EditableTopic))
            {
                Response.Redirect(_forumManager.PreviousPage.Url);
                return;
            }

            _subject = EditableTopic.Title;

            _tagString = String.Join(",", EditableTopic.Tags.Select(x=>x.Name));
            _tagValues = JsonConvert.SerializeObject(EditableTopic.Tags.Select(x => new List<object> { x.Name, x.ID }));

            if (EditableTopic.Type == TopicType.Informational)
                _pollMaster.Visible = false;
            else
            {
                _pollMaster.QuestionFieldID = "forum_subject";                
                if (IsPostBack == false)
                {
                    var question = ForumDataProvider.GetPollByID(TenantProvider.CurrentTenantID, EditableTopic.QuestionID);                        
                    _pollMaster.Singleton = (question.Type == QuestionType.OneAnswer);
                    _pollMaster.Name = question.Name;
                    _pollMaster.ID = question.ID.ToString();

                    foreach (var variant in question.AnswerVariants)
                    {   
                        _pollMaster.AnswerVariants.Add(new PollFormMaster.AnswerViarint()
                        {
                            ID = variant.ID.ToString(),
                            Name = variant.Name
                        });
                    }
                }
            }


            #region IsPostBack
            if (IsPostBack)
            {
                if (EditableTopic.Type == TopicType.Informational)
                    _subject = Request["forum_subject"].Trim();
                else
                    _subject = (_pollMaster.Name??"").Trim();               

                if (String.IsNullOrEmpty(_subject))
                {
                    _subject = "";
                    _errorMessage = "<div class=\"errorBox\">" + Resources.ForumUCResource.ErrorSubjectEmpty + "</div>";
                    return;
                }

                if (EditableTopic.Type == TopicType.Poll && _pollMaster.AnswerVariants.Count < 2)
                {
                    _errorMessage = "<div class=\"errorBox\">" + Resources.ForumUCResource.ErrorPollVariantCount + "</div>";
                    return;
                }

                if (!String.IsNullOrEmpty(Request["forum_tags"]))
                    _tagString = Request["forum_tags"].Trim();
                else
                    _tagString = "";

                if (!String.IsNullOrEmpty(Request["forum_search_tags"]))
                    _tagValues = Request["forum_search_tags"].Trim();

                EditableTopic.Title = _subject;

                if (_forumManager.ValidateAccessSecurityAction(ForumAction.TagCreate, new Thread() { ID = EditableTopic.ThreadID }))
                {
                    var removeTags = EditableTopic.Tags;
                    EditableTopic.Tags = CreateTags();

                    removeTags.RemoveAll(t => EditableTopic.Tags.Find(nt => nt.ID == t.ID) != null);                    

                    foreach (var tag in EditableTopic.Tags)
                    {
                        if (tag.ID == 0)
                            ForumDataProvider.CreateTag(TenantProvider.CurrentTenantID, EditableTopic.ID, tag.Name, tag.IsApproved);
                        else
                            ForumDataProvider.AttachTagToTopic(TenantProvider.CurrentTenantID, tag.ID, EditableTopic.ID);
                    }

                    removeTags.ForEach(t =>
                    {
                        ForumDataProvider.RemoveTagFromTopic(TenantProvider.CurrentTenantID, t.ID, EditableTopic.ID);
                    });
                }

                try
                {
                    if (EditableTopic.Type == TopicType.Poll)
                    {
                        List<AnswerVariant> variants = new List<AnswerVariant>();
                        int i = 1;
                        foreach (var answVariant in _pollMaster.AnswerVariants)
                        {
                            variants.Add(new AnswerVariant()
                            {
                                ID = (String.IsNullOrEmpty(answVariant.ID) ? 0 : Convert.ToInt32(answVariant.ID)),
                                Name = answVariant.Name,
                                SortOrder = i - 1
                            });
                            i++;
                        }                    

                        ForumDataProvider.UpdatePoll(TenantProvider.CurrentTenantID, EditableTopic.QuestionID, 
                            _pollMaster.Singleton ? QuestionType.OneAnswer : QuestionType.SeveralAnswer,
                            EditableTopic.Title, variants);
                    }
                    
                    ForumDataProvider.UpdateTopic(TenantProvider.CurrentTenantID, EditableTopic.ID, EditableTopic.Title,
                                                    EditableTopic.Sticky, EditableTopic.Closed);
                    FactoryIndexer<TopicWrapper>.UpdateAsync(EditableTopic); 
                    _errorMessage = "<div class=\"okBox\">" + Resources.ForumUCResource.SuccessfullyEditTopicMessage + "</div>";
                    Response.Redirect(_forumManager.PreviousPage.Url);

                
                }
                catch(Exception ex)                
                {
                    _errorMessage = "<div class=\"errorBox\">" + ex.Message.HtmlEncode() + "</div>";
                    return;
                }
            }
            #endregion
        }

        private List<Tag> CreateTags()
        {
            List<Tag> list = new List<Tag>(0);

            _tagString = _tagString.TrimEnd(',');
            if (!String.IsNullOrEmpty(_tagString))
            {

                var searchTags = new List<Tag>(0);
                if (!String.IsNullOrEmpty(_tagValues))
                {
                    var values = JsonConvert.DeserializeObject<List<JArray>>(_tagValues);

                    foreach (var tagItem in values)
                    {
                        var tag = new Tag
                        {
                            ID = Int32.Parse(tagItem[1].ToString()),
                            Name = tagItem[0].ToString()
                        };

                        if(searchTags.Find(t=> t.ID == tag.ID)==null)
                            searchTags.Add(tag);
                    }
                }

                foreach (string inputTagName in _tagString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Tag tag = new Tag()
                    {
                        ID = 0,
                        Name = inputTagName.Trim()
                    };
                    foreach (Tag _tag in searchTags)
                    {
                        if (String.Compare(inputTagName.Trim(), _tag.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            tag = _tag;
                            break;
                        }
                    }

                    if (list.Find(t => t.ID == tag.ID && t.ID != 0) == null)
                        list.Add(tag);
                }
            }
            return list;
        }

        protected string RenderAddTags()
        {
            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.TagCreate, this.EditableTopic))
                return "";

            var sb = new StringBuilder();
            sb.AppendLine("var ForumTagSearchHelper = new SearchHelper('forum_tags','forum_sh_item','forum_sh_itemselect','',\"ForumManager.SaveSearchTags(\'forum_search_tags\',ForumTagSearchHelper.SelectedItem.Value,ForumTagSearchHelper.SelectedItem.Help);\",\"TagSuggest\", \"GetSuggest\",\"'" + SettingsID + "',\",true,false);");
            Page.RegisterInlineScript(sb.ToString());
            
            sb = new StringBuilder();
            sb.Append("<div class=\"headerPanel-splitter\">");
            sb.Append("<div class=\"headerPanelSmall-splitter\"><b>" + Resources.ForumUCResource.Tags + ":</b></div>");
            sb.Append("<div>");
            sb.Append("<input autocomplete=\"off\" class=\"textEdit\" style=\"width:100%\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(_tagString) + "\" maxlength=\"3000\" id=\"forum_tags\" name=\"forum_tags\"/>");
            sb.Append("<input type='hidden' value=\"" + HttpUtility.HtmlEncode(_tagValues) + "\" id='forum_search_tags' name='forum_search_tags'/>");
            sb.Append("</div>");
            sb.Append("<div class=\"text-medium-describe\">" + Resources.ForumUCResource.HelpForTags + "</div>");
            sb.Append("</div>");

            return sb.ToString();
        }
    }
}