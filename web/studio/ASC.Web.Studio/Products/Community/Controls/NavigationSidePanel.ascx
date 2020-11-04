<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.Community.Controls.NavigationSidePanel" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>


<div class="page-menu">

    <ul class="menu-list">
        <% if (IsBlogsAvailable) %>
        <% { %>
        <li class="menu-item none-sub-list<%if(CurrentPage=="blogs"){%> active currentCategory<%}%> ">
            <span class="menu-item-icon blogs"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconsblogs"></use></svg></span>
            <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Blogs/Default.aspx")%>" title="<%= CommunityResource.Blogs%>">
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
                <span class="menu-item-icon events"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconspin"></use></svg></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/Default.aspx")%>" title="<%= CommunityResource.Events%>">
                    <%= CommunityResource.Events%>
                </a>
                <span id="feed-new-events-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item<%if(CurrentPage=="news"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/Default.aspx") + "?type=News"%>" title="<%= CommunityResource.News%>">
                        <%= CommunityResource.News%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="order"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/Default.aspx") + "?type=Order"%>" title="<%= CommunityResource.Orders%>">
                        <%= CommunityResource.Orders%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="advert"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/Default.aspx") + "?type=Advert"%>" title="<%= CommunityResource.Announcements%>">
                        <%= CommunityResource.Announcements%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="poll"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/News/Default.aspx") + "?type=Poll"%>" title="<%= CommunityResource.Polls%>">
                        <%= CommunityResource.Polls%>
                    </a>
                </li>
            </ul>
        </li>
        <% } %>

        <% if (IsForumsAvailable) %>
        <% { %>
            <li class="menu-item none-sub-list<%if(CurrentPage=="forum"){%> active currentCategory<%}%>">
                <span class="menu-item-icon forum"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconsforum"></use></svg></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/Default.aspx")%>" title="<%= CommunityResource.Forums%>">
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
                <span class="menu-item-icon bookmarks"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconsbookmark"></use></svg></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/Default.aspx")%>" title="<%= CommunityResource.Bookmarks%>">
                    <%= CommunityResource.Bookmarks%>
                </a>
                <span id="feed-new-bookmarks-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item<%if(CurrentPage=="bookmarkingfavourite"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/FavouriteBookmarks.aspx")%>" title="<%= CommunityResource.Favorites%>">
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
                <span class="menu-item-icon wiki"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconswiki"></use></svg></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx")%>" title="<%= CommunityResource.Wiki%>">
                    <%= CommunityResource.Wiki%>
                </a>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item<%if(CurrentPage=="wikicategories"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:Categories"%>" title="<%= CommunityResource.Categories%>">
                        <%= CommunityResource.Categories%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="wikiindex"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:Index"%>" title="<%= CommunityResource.Index%>">
                        <%= CommunityResource.Index%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="wikinew"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:NewPages"%>" title="<%= CommunityResource.NewPages%>">
                        <%= CommunityResource.NewPages%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="wikirecently"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:Recently"%>" title="<%= CommunityResource.RecentlyEdited%>">
                        <%= CommunityResource.RecentlyEdited%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="wikifiles"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:Files"%>" title="<%= CommunityResource.Files%>">
                        <%= CommunityResource.Files%>
                    </a>
                </li>
                <li class="menu-sub-item<%if(CurrentPage=="wikihelp"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Wiki/Default.aspx") + "?page=Internal:Help"%>" title="<%= CommunityResource.Help%>">
                        <%= CommunityResource.Help%>
                    </a>
                </li>
            </ul>
        </li>
        <% } %>

        <%if (IsBirthdaysAvailable)
            { %>
            <li class="menu-item none-sub-list<%if(CurrentPage=="birthdays"){%> active currentCategory<%}%>">
                <span class="menu-item-icon group"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconsgroup"></use></svg></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Birthdays/")%>" title="<%= CommunityResource.Birthdays%>">
                    <%= CommunityResource.Birthdays%>
                </a>
            </li>
        <%} %>

        <asp:PlaceHolder ID="InviteUserHolder" runat="server"/>
        <% if (IsAdmin && (IsForumsAvailable || IsFullAdministrator)) %>
        <% { %>
        <li id="menuSettings" class="menu-item add-block sub-list<%if(IsInSettings){%> currentCategory<%}%>">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href="<%=GetDefaultSettingsPageUrl()%>" title="<%= CommunityResource.Settings %>">
                    <span class="menu-item-icon settings"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span>
                    <span class="menu-item-label inner-text<%if(!IsInSettings){%> gray-text<%}%>"><%= CommunityResource.Settings %></span>
                </a>
            </div>
            <ul class="menu-sub-list">
                <% if (IsForumsAvailable) %>
                <% { %>
                <li class="menu-sub-item<%if(CurrentPage=="forumeditor"){%> active<%}%>">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Forum/ManagementCenter.aspx")%>" title="<%= CommunityResource.ForumEditor%>">
                        <%= CommunityResource.ForumEditor%>
                    </a>
                </li>
                <% } %>
                <% if (IsFullAdministrator) %>
                <% { %>
                <li id="menuAccessRights" class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow" href="<%=VirtualPathUtility.ToAbsolute("~/Management.aspx")+ "?type=" + (int)ASC.Web.Studio.Utility.ManagementType.AccessRights +"#community"%>" title="<%= CommunityResource.AccessRightsSettings%>">
                        <%= CommunityResource.AccessRightsSettings%>
                    </a>
                </li>
                <% } %>
            </ul>
        </li>
        <% } %>

        <asp:PlaceHolder ID="HelpHolder" runat="server"/>
        <asp:PlaceHolder ID="SupportHolder" runat="server"/>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"/>
        <asp:PlaceHolder ID="VideoGuides" runat="server"/>
    </ul>
</div>
