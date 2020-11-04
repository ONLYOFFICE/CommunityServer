<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkInfoUserControl.ascx.cs" Inherits="ASC.Web.UserControls.Bookmarking.BookmarkInfoUserControl" %>

<div id="BookmarkingInfoContainer" class="clearFix bookmarkingAreaWithMargin10">

    <%--Bookmark info--%>
    <div class="clearFix" id="BookmarkInfoHolder" runat="server" style="margin-bottom: 20px;">
    </div>
    <%--Div to attach 'Add to Favourite' panel--%>
    <div class="clearFix" id="<%=GetUniqueId()%>ToAppend"></div>

    <div class="clearFix" id="BookmarkInfoTabsContainer" runat="server">
    </div>

    <%--Comments section--%>
    <div class="clearFix" id="BookmarkCommentsPanel" style="margin-top: -5px; display: none;">
        <div class="clearFix" id="CommentsHolder" runat="server">
        </div>
    </div>

    <%--List of user who added this bookmark as favourite with their own description and date added--%>
    <div class="clearFix" id="BookmarkedByPanel" style="display: none;">
        <asp:Repeater runat="server" ID="AddedByRepeater">
            <HeaderTemplate>
            </HeaderTemplate>
            <ItemTemplate>

                <%#GetAddedByTableItem(	Container.ItemIndex % 2 != 0,
										GetUserImage(DataBinder.Eval(Container.DataItem, "UserID")),
										GetUserPageLink(DataBinder.Eval(Container.DataItem, "UserID")),
										GetUserBookmarkDescriptionIfChanged(Container.DataItem),
										GetDateAddedAsString(DataBinder.Eval(Container.DataItem, "DateAdded")),
										DataBinder.Eval(Container.DataItem, "UserID"))%>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <input type="hidden" id="SelectedBookmarkID" value="<%=GetBookmarkID()%>" />
</div>
