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
<%if (!singleBookmark && itemCounter>20) {%>
<div class="navigationLinkBox bookmarks">
            <table id="tableForNavigation" cellpadding="4" cellspacing="0">
                <tbody>
                <tr>
                    <td>
                        <div style="clear: right; display: inline-block;">
                            <div id="BookmarkingPaginationContainer" runat="server" class="bookmarkingPagination"></div>
                        </div>
                    </td>
                    <td style="text-align:right;">
                        <span class="gray-text"><%=BookmarkingUCResource.TotalPages%>: </span>
                        <span class="gray-text" style="margin-right: 20px;"><%=itemCounter%></span>
                        <span class="gray-text"><%=BookmarkingUCResource.ShowOnPage%>: </span>
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
    <%} %>

