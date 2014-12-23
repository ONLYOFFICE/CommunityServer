<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.Community.Controls.NavigationSidePanel" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>
<%@ Import Namespace="Resources" %>


<div class="page-menu">
    <% if (!IsVisitor) %>
<% { %>
<div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <% if (IsBlogsAvailable) %>
            <% { %>
            <li>
                <div class="describe-text create-new-list-header"><%= CommunityResource.Blogs%></div>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/addblog.aspx")%>">
                        <%= CommunityResource.Post%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsEventsAvailable) %>
            <% { %>
            <li>
                <div class="describe-text create-new-list-header"><%= CommunityResource.Events%></div>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editnews.aspx") + "?type=News"%>">
                        <%= CommunityResource.News%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editnews.aspx") + "?type=Order"%>">
                        <%= CommunityResource.Order%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editnews.aspx") + "?type=Advert"%>">
                        <%= CommunityResource.Announcement%>
                    </a></li>
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editpoll.aspx")%>">
                        <%= CommunityResource.Poll%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsForumsAvailable && (IsAdmin || (!IsAdmin && ForumsHasThreadCategories))) %>
            <% { %>
            <li id="createNewButton_Forums">
                <div class="describe-text create-new-list-header"><%= CommunityResource.Forums%></div>
                <ul class="create-new-link-list">
                    <% if (IsAdmin) %>
                    <% { %>
                    <li id="createNewButton_Forums_newForum">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/newforum.aspx")%>">
                            <%= CommunityResource.Forum%>
                        </a>
                    </li>
                    <% } %>
                    <% if (ForumsHasThreadCategories) %>
                    <% { %>
                    <li id="createNewButton_Forums_newTopic">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/newpost.aspx") + "?m=0" + (ForumID != 0 ? "&f=" + ForumID : "")%>">
                            <%= CommunityResource.Topic%>
                        </a>
                    </li>
                    <li id="createNewButton_Forums_newPoll">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/newpost.aspx") + "?m=1"+ (ForumID != 0 ? "&f=" + ForumID : "")%>">
                            <%= CommunityResource.Poll%>
                        </a>
                    </li>
                    <% if (InAParticularTopic && MakeCreateNewTopic && TopicID > 0) %>
                    <% { %>
                    <li id="createNewButton_Forums_newPost">
                        <a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/newpost.aspx") + "?t="  + TopicID %>">
                            <%= CommunityResource.Post%>
                        </a>
                    </li>
                    <% } %>
                    <% } %>
                </ul>
            </li>
            <% } %>

            <% if (IsBookmarksAvailable) %>
            <% { %>
            <li>
                <div class="describe-text create-new-list-header"><%= CommunityResource.Bookmarks%></div>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/bookmarking/createbookmark.aspx")%>">
                            <%= CommunityResource.Bookmark%>
                    </a></li>
                </ul>
            </li>
            <% } %>

            <% if (IsWikiAvailable) %>
            <% { %>
            <li>
                <div class="describe-text create-new-list-header"><%= CommunityResource.Wiki%></div>
                <ul class="create-new-link-list">
                    <li><a class="dropdown-item" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?action=New"%>">
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
        <li id="menuCreateNewButton" class="menu-main-button without-separator big">
            <span class="main-button-text override"><%=CommunityResource.CreateNew%></span>
            <span class="white-combobox">&nbsp;</span>
        </li>
        <li id="menuOtherActionsButton" class="menu-gray-button display-none">
            <span>...</span>
        </li>
        </ul>
        <% } %>

        <ul class="menu-list">
            <% if (IsBlogsAvailable) %>
            <% { %>
            <li class="menu-item none-sub-list<%if(CurrentPage=="blogs"){%> active currentCategory<%}%> ">
                <span class="menu-item-icon blogs"></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/default.aspx")%>">
                    <%= CommunityResource.Blogs%>
                </a>
                <span id="feed-new-blogs-count" class="feed-new-count"></span>
            </li>
            <% } %>

            <% if (IsEventsAvailable) %>
            <% { %>
            <li class="menu-item sub-list<%if(CurrentPage=="events"){%> active currentCategory<%}%><%if(IsInEvents){%> currentCategory<%}%>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <span class="menu-item-icon events"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/default.aspx")%>">
                        <%= CommunityResource.Events%>
                    </a>
                    <span id="feed-new-events-count" class="feed-new-count"></span>
                </div>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item<%if(CurrentPage=="news"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/default.aspx") + "?type=News"%>">
                            <%= CommunityResource.News%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="order"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/default.aspx") + "?type=Order"%>">
                            <%= CommunityResource.Orders%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="advert"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/default.aspx") + "?type=Advert"%>">
                            <%= CommunityResource.Announcements%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="poll"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/news/default.aspx") + "?type=Poll"%> ">
                            <%= CommunityResource.Polls%>
                        </a>
                    </li>
                </ul>
            </li>
            <% } %>

            <% if (IsForumsAvailable) %>
            <% { %>
                <li class="menu-item none-sub-list<%if(CurrentPage=="forum"){%> active currentCategory<%}%>">
                    <span class="menu-item-icon forum"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/default.aspx")%>">
                        <%= CommunityResource.Forums%>
                    </a>
                    <span id="feed-new-forums-count" class="feed-new-count"></span>
                </li>
            <% } %>

            <% if (IsBookmarksAvailable) %>
            <% { %>
            <li class="menu-item sub-list<%if(CurrentPage=="bookmarking"){%> active currentCategory<%}%><%if(IsInBookmarks){%> currentCategory<%}%>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <span class="menu-item-icon bookmarks"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/bookmarking/default.aspx")%>">
                        <%= CommunityResource.Bookmarks%>
                    </a>
                    <span id="feed-new-bookmarks-count" class="feed-new-count"></span>
                </div>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item<%if(CurrentPage=="bookmarkingfavourite"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/bookmarking/favouritebookmarks.aspx")%>">
                            <%= CommunityResource.Favorites%>
                        </a>
                    </li>
                </ul>
            </li>
            <% } %>

            <% if (IsWikiAvailable) %>
            <% { %>
            <li class="menu-item sub-list<%if(CurrentPage=="wiki"){%> active currentCategory<%}%><%if(IsInWiki){%> currentCategory<%}%>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <span class="menu-item-icon wiki"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx")%>">
                        <%= CommunityResource.Wiki%>
                    </a>
                </div>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item<%if(CurrentPage=="wikicategories"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:Categories"%>">
                            <%= CommunityResource.Categories%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="wikiindex"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:Index"%>">
                            <%= CommunityResource.Index%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="wikinew"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:NewPages"%>">
                            <%= CommunityResource.NewPages%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="wikirecently"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:Recently"%>">
                            <%= CommunityResource.RecentlyEdited%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="wikifiles"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:Files"%>">
                            <%= CommunityResource.Files%>
                        </a>
                    </li>
                    <li class="menu-sub-item<%if(CurrentPage=="wikihelp"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/wiki/default.aspx") + "?page=Internal:Help"%> ">
                            <%= CommunityResource.Help%>
                        </a>
                    </li>
                </ul>
            </li>
            <% } %>
            
            <%if (IsBirthdaysAvailable)
              { %>
                <li class="menu-item none-sub-list<%if(CurrentPage=="birthdays"){%> active currentCategory<%}%>">
                    <span class="menu-item-icon group"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/birthdays/")%>">
                        <%= CommunityResource.Birthdays%>
                    </a>
                </li>
            <%} %>

            <% if (IsAdmin) %>
            <% { %>
            <li id="menuSettings" class="menu-item add-block sub-list<%if(IsInSettings){%> currentCategory<%}%>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow" href="<%=GetDefaultSettingsPageUrl()%>">
                        <span class="menu-item-icon settings"></span>
                        <span class="menu-item-label inner-text<%if(!IsInSettings){%> gray-text<%}%>"><%= CommunityResource.Settings %></span>
                    </a>
                </div>
                <ul class="menu-sub-list">
                    <% if (IsForumsAvailable) %>
                    <% { %>
                    <li class="menu-sub-item<%if(CurrentPage=="forumeditor"){%> active<%}%>">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/products/community/modules/forum/managementcenter.aspx")%>">
                            <%= CommunityResource.ForumEditor%>
                        </a>
                    </li>
                    <% } %>
                    <li id="menuAccessRights" class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/management.aspx")+ "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights +"#community"%>">
                            <%= CommunityResource.AccessRightsSettings%>
                        </a>
                    </li>
                </ul>
            </li>
            <% } %>
            
            <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
        </ul>
    </div>
