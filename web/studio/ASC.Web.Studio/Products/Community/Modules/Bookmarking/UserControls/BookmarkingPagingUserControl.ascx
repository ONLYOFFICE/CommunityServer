<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkingPagingUserControl.ascx.cs" Inherits="ASC.Web.UserControls.Bookmarking.BookmarkingPagingUserControl" %>

<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>


<% if (!SingleBookmark && ItemCounter > 20) { %>
<div class="navigationLinkBox bookmarks">
    <table id="tableForNavigation" cellpadding="0" cellspacing="0">
        <tbody>
        <tr>
            <td>
                <div style="clear: right; display: inline-block;">
                    <div id="BookmarkingPaginationContainer" runat="server" class="bookmarkingPagination"></div>
                </div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%=BookmarkingUCResource.TotalPages%>: </span>
                <span class="gray-text" style="margin-right: 20px;"><%=ItemCounter%></span>
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
<% } %>

