<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ButtonSidePanel.ascx.cs" Inherits="ASC.Web.Community.Controls.ButtonSidePanel" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>


<div class="page-menu">
    <% if (!IsVisitor) %>
    <% { %>
    <div id="createNewButton" class="studio-action-panel community">
        <ul class="dropdown-content">
            <% if (IsBlogsAvailable) %>
            <% { %>
            <li>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Blogs/AddBlog.aspx")%>">
                        <%= CommunityResource.Post%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsEventsAvailable) %>
            <% { %>
            <li>
                <ul class="create-new-link-list">
                    <li>
                        <div class="dropdown-item-seporator"></div>
                    </li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/EditNews.aspx") + "?type=News"%>">
                        <%= CommunityResource.News%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/EditNews.aspx") + "?type=Order"%>">
                        <%= CommunityResource.Order%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/EditNews.aspx") + "?type=Advert"%>">
                        <%= CommunityResource.Announcement%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/EditPoll.aspx")%>">
                        <%= CommunityResource.Poll%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsForumsAvailable && (IsAdmin || (!IsAdmin && ForumsHasThreadCategories))) %>
            <% { %>
            <li id="createNewButton_Forums">
                <ul class="create-new-link-list">

                    <li>
                        <div class="dropdown-item-seporator"></div>
                    </li>

                    <% if (IsAdmin) %>
                    <% { %>
                    <li id="createNewButton_Forums_newForum">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/NewForum.aspx")%>">
                            <%= CommunityResource.Forum%>
                        </a>
                    </li>
                    <% } %>
                    <% if (ForumsHasThreadCategories) %>
                    <% { %>
                    <li id="createNewButton_Forums_newTopic">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/NewPost.aspx") + "?m=0" + (ForumID != 0 ? "&f=" + ForumID : "")%>">
                            <%= CommunityResource.Topic%>
                        </a>
                    </li>
                    <li id="createNewButton_Forums_newPoll">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/NewPost.aspx") + "?m=1"+ (ForumID != 0 ? "&f=" + ForumID : "")%>">
                            <%= CommunityResource.Poll%>
                        </a>
                    </li>
                    <% if (InAParticularTopic && MakeCreateNewTopic && TopicID > 0) %>
                    <% { %>
                    <li id="createNewButton_Forums_newPost">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/NewPost.aspx") + "?t="  + TopicID %>">
                            <%= CommunityResource.ForumPost%>
                        </a>
                    </li>
                    <% } %>
                    <% } %>
                </ul>
            </li>
            <% } %>

            <% if (IsBookmarksAvailable || IsWikiAvailable) %>
            <% { %>
            <li>
                <div class="dropdown-item-seporator"></div>
            </li>
            <% } %>

            <% if (IsBookmarksAvailable) %>
            <% { %>
            <li>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/CreateBookmark.aspx")%>">
                            <%= CommunityResource.Bookmark%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsWikiAvailable) %>
            <% { %>
            <li>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?action=New"%>">
                            <%= CommunityResource.Page%>
                    </a></li>
                </ul>
            </li>
            <% } %>
        </ul>
    </div>

    <div id="otherActions" class="studio-action-panel display-none">
        <ul class="dropdown-content"></ul>
    </div>
    <% } %>

    <% if (!IsVisitor) %>
    <% { %>
    <ul class="menu-actions clearFix">
    <li id="menuCreateNewButton" class="menu-main-button without-separator big" title="<%=CommunityResource.CreateNew%>">
        <span class="main-button-text override"><%=CommunityResource.CreateNew%></span>
        <span class="white-combobox">&nbsp;</span>
    </li>
    <li id="menuOtherActionsButton" class="menu-gray-button display-none">
        <span>...</span>
    </li>
    </ul>
    <% } %>

</div>
