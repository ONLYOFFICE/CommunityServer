<%@ Assembly Name="ASC.Web.Community"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopicEditorControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.TopicEditorControl" %>

<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.UserControls.Common.PollForm" TagPrefix="sc" %>

<%@ Import Namespace="ASC.Web.UserControls.Forum.Resources" %>
<%@ Import Namespace="ASC.Forum" %>

<input type="hidden" id="forum_topicType" value="<%=((int)EditableTopic.Type).ToString()%>" />
<div id="forum_errorMessage">
<%=_errorMessage%>
</div>

    <div class="clearFix">
        <%
            if (EditableTopic.Type == TopicType.Informational)
            {%>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter\">
                <b>
                    <%= ForumUCResource.Topic%></b></div>
            <div>
                <input class="textEdit" style="width: 100%;" maxlength="300" name="forum_subject"
                    id="forum_subject" type="text" value="<%=HttpUtility.HtmlEncode(_subject)%>"></div>
        </div>
        <% }
        %>
    </div>
    <sc:PollFormMaster runat="server" ID="_pollMaster" />
    <%=RenderAddTags()%>
<div class="big-button-container">
    <a class="button blue big" href="javascript:ForumManager.SaveEditTopic();">
        <%=ForumUCResource.SaveButton%>
    </a><span class="splitter-buttons"></span><a class="button gray big" href="<%=PreviousPageUrl%>">
        <%=ForumUCResource.CancelButton%>
    </a>
</div>
