<%@ Assembly Name="ASC.Web.Community" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.Master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Blogs.Default" Title="Untitled Page" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Blogs.Core.Resources" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility.HtmlUtility" %>


<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">

    <asp:PlaceHolder ID="placeContent" runat="server"/>

     <% if (!String.IsNullOrEmpty(UserID) && PostsAndCommentsCount != null && PostsAndCommentsCount.Count > 0) { %>
        <div class="BlogsHeaderBlock header-with-menu" style="margin-bottom:16px;">
            <span class="header">
                <%= CoreContext.UserManager.GetUsers(PostsAndCommentsCount[0].Item1.UserID).DisplayUserName() %>
            </span>
        </div>
    <% } %>

    <div id="postList">
         <% foreach (var item in PostsAndCommentsCount) { %>

        <div class="container-list">
        <div class="header-list">

        <div class="avatar-list">
            <a href="ViewBlog.aspx?blogid=<%= item.Item1.ID.ToString() %>">
                <img class="userMiniPhoto" alt="" src="<%= UserPhotoManager.GetBigPhotoURL(item.Item1.UserID) %>"/></a>
            </div>
            <div class="describe-list">
                <div class="title-list">
                    <a href="ViewBlog.aspx?blogid=<%= item.Item1.ID.ToString() %>"><%= HttpUtility.HtmlEncode(item.Item1.Title) %></a>
                </div>

                <div class="info-list">
                    <span class="caption-list"><%= BlogsResource.PostedTitle %>:</span>
                    <%= CoreContext.UserManager.GetUsers(item.Item1.UserID).RenderCustomProfileLink("name-list", "link") %>
                </div>

                <% if (String.IsNullOrEmpty(UserID)) { %>
                    <div class="info-list">
                        <a class="link gray-text"
                            href="<%= VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath) %>?userid=<%= item.Item1.UserID %>">
                            <%= BlogsResource.AllRecordsOfTheAutor%>
                        </a>
                    </div>
                <% } %>

                <div class="date-list">
                <%= item.Item1.Datetime.ToString("d") %><span class="time-list"><%= item.Item1.Datetime.ToString("t") %></span>
                </div>
            </div>
        </div>

        <div class="content-list">
            <%= HtmlUtility.GetFull(item.Item1.Content, false) %>
            <div id="postIndividualLink" class="display-none">ViewBlog.aspx?blogid=<%= item.Item1.ID.ToString() %></div>              
                <div class="comment-list">
                    <a href="ViewBlog.aspx?blogid=<%= item.Item1.ID %>#comments"><%= BlogsResource.CommentsTitle + ": " + item.Item2.ToString() %></a>
                
                    <% if (!CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOutsider()) { %>
                    <a href="ViewBlog.aspx?blogid=<%= item.Item1.ID %>#addcomment"><%= BlogsResource.CommentsAddButtonTitle %></a>
                    <% } %>
                </div>
            </div>
        </div>
        <% } %>
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPagingContent" runat="server">

    <% if (blogsCount > 20) { %>
    <div class="navigationLinkBox news">
        <table id="tableForNavigation" cellpadding="0" cellspacing="0">
            <tbody>
                <tr>
                    <td>
                        <div id="pnh"></div>
                        <div style="clear: right; display: inline-block;">
                            <asp:PlaceHolder ID="pageNavigatorHolder" runat="server"/>
                        </div>
                    </td>
                    <td style="text-align:right;">
                        <span class="gray-text"><%=BlogsResource.TotalPages%>: </span>
                        <span class="gray-text" style="margin-right: 20px;"><%=blogsCount%></span>
                        <span class="gray-text"><%=BlogsResource.ShowOnPage%>: </span>
                        <select class="top-align">
                            <option value="20">20</option>
                            <option value="50">50</option>
                            <option value="75">75</option>
                            <option value="100">100</option>
                        </select>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <% } %>

</asp:Content>
