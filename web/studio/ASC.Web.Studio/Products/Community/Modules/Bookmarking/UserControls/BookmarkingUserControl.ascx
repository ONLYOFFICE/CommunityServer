<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkingUserControl.ascx.cs"
    Inherits="ASC.Web.UserControls.Bookmarking.BookmarkingUserControl" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>

<div class="clearFix" id="BookmarksMainPanel" runat="server">
    <%if(!IsSelectedBookmarkDisplayMode()){ %>
        <div class="clearFix" style="margin-bottom: 10px;">
            <%--Sorting--%>
            <div style="float: left;" runat="server" id="BookmarkingSortPanel">
            </div>
        </div>
        <%--Create bookmark permission check--%>
        <%if(ShowCreateBookmarkLink() && PermissionCheckCreateBookmark()){ %>
        <!--	<a href="<%=GetCreateBookmarkPageUrl() %>" class="button" style="float: right;"
					        title="<%=BookmarkingUCResource.CreateNewBookmarkLinkTitle %>">
				        <%= BookmarkingUCResource.AddBookmarkLink %>
			        </a> --->
        <%}%>
    <%} %>
    <%--Create bookmark panel--%>
    <div class="clearFix" id="CreateBookmarkPanel" runat="server">
    </div>
    <%--Remove bookmark panel--%>
    <div id="BookmarkingRemoveFromFavouritePopupContainer" runat="server">
    </div>
</div>
<div class="longWordsBreak" id="BookmarksHolder" runat="server">
</div>