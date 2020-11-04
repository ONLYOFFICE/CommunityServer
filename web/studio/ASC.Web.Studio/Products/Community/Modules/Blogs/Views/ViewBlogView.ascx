<%@ Assembly Name="ASC.Web.Community" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewBlogView.ascx.cs"
    Inherits="ASC.Web.Community.Blogs.Views.ViewBlogView" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>
<%@ Import Namespace="ASC.Blogs.Core.Resources" %>
<%@ Import Namespace="ASC.Web.Community.Blogs" %>
<%@ Import Namespace="ASC.Web.Studio.Utility.HtmlUtility" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>


<% if (IsPreview)  { %>
<div style="margin-bottom: 20px;">
    <div id="previewTitle" class="containerHeaderBlock" style="padding: 0 8px;"><%= HttpUtility.HtmlEncode(post.Title) %></div>
<% } else { %>
<div>
<% } %>
    <table class='MainBlogsTable' cellspacing='0' cellpadding='8' border='0'>
    <tr>
        <td valign='top' class='avatarContainer'>
            <div>
                <img class="userMiniPhoto" alt="" src="<%= UserPhotoManager.GetBigPhotoURL(post.UserID) %>"/>
            </div>
        </td>

        <td valign='top'>
            <div class="author-title describe-text"><%= ASC.Blogs.Core.Resources.BlogsResource.PostedTitle %>:</div>
            <div class="author-name">
                <a class='linkMedium' href="<%= postUser.GetUserProfilePageURL() %>">
                    <%= postUser.DisplayUserName() %>
                </a>
            </div>
            <div>
                <a class='linkMedium gray-text' href='<%=VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath) %>?userid=<%= postUser.ID %>'>
                    <%= BlogsResource.AllRecordsOfTheAutor %>
                </a>
            </div>
            <div class='describe-text' style='margin-top:10px'>
                <%= post.Datetime.ToString("d") %><span style='margin-left:20px'><%= post.Datetime.ToString("t") %></span>
            </div>


            <% if (!IsPreview) %>
            <% { %>
            <div id="blogActionsMenuPanel" class="studio-action-panel">
                <ul class="dropdown-content">
                <% if (CommunitySecurity.CheckPermissions(post, ASC.Blogs.Core.Constants.Action_EditRemovePost)) %>
                <% { %>
                <li><a class="dropdown-item" href="EditBlog.aspx?blogid=<%= Request.QueryString["blogid"] %>"><%= BlogsResource.EditBlogLink %></a></li>
                <li><a class="dropdown-item" onclick="javascript:return confirm('<%= BlogsResource.ConfirmRemovePostMessage %>')"
                    href="EditBlog.aspx?blogid=<%= Request.QueryString["blogid"] %>&action=delete" ><%= BlogsResource.DeleteBlogLink %></a></li>
                <% } %>
                </ul>
            </div>
        <% } %>
  
        </td>
    </tr>
    </table>

    <div id='previewBody' class='longWordsBreak ContentMainBlog'>
        <%= HtmlUtility.GetFull(post.Content) %>
    </div>

    <% if (!IsPreview) %>
    <% { %>
    <div  class='clearFix'>


        <% if (post.TagList.Count > 0) %>
        <% { %>
            <div class="text-medium-describe TagsMainBlog">
            <img class="TagsImgMainBlog" src="<%= WebImageSupplier.GetAbsoluteWebPath("tags.png", ASC.Blogs.Core.BlogsSettings.ModuleID) %>" />
            <% for (var i = 0; i < post.TagList.Count; i++) %>
            <% {
                    var tag = post.TagList[i];
                    %>
                <%= (i!=0) ? ", " : "" %><a style="margin-left:5px;" class="linkMedium gray-text" href="./?tagname=<%= HttpUtility.UrlEncode(tag.Content)%>"><%= HttpUtility.HtmlEncode(tag.Content)%></a>
            <% } %>
        <% } %>
    </div>
    <% } %>

</div>