<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkingRemoverFromFavouritePopup.ascx.cs" Inherits="ASC.Web.UserControls.Bookmarking.BookmarkingRemoverFromFavouritePopup" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Register Assembly="ASC.Web.Community" Namespace="ASC.Web.UserControls.Bookmarking.Common" TagPrefix="ascbc" %>

<div id="removeBookmarkConfirmDialog" style="display: none;">
    <sc:Container runat="server" ID="BookmarkingRemoveFromFavouriteContainer">
        <header>
            <%=BookmarkingUCResource.RemoveFromFavouriteTitle%>
        </header>
        <body>

            <%--Are you sure you want to remove the bookmark?--%>
            <div class="clearFix"><%=string.Format(BookmarkingUCResource.RemoveFromFavouriteConfirmMessage, "<a href='javascript: void(0);' id='BookmarkToRemoveFromFavouriteName'>&nbsp;</a>")%></div>

            <div class="middle-button-container" id="bookmarkingRemoveFromFavouriteButtonsDiv">
                <ascbc:ActionButton ButtonID="BookmarkingRemoveFromFavouriteLink" ID="BookmarkingRemoveFromFavouriteLink" ButtonContainer="removeBookmarkConfirmDialog"
                    ButtonCssClass="button blue middle" runat="server"></ascbc:ActionButton>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:jq.unblockUI();">
                    <%=BookmarkingUCResource.Cancel%></a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="removeBookmarkDialog" style="display: none;">
    <sc:Container runat="server" ID="BookmarkingRemoveContainer">
        <header>
            <%=BookmarkingUCResource.RemoveFromFavouriteTitle%>
        </header>
        <body>

            <%--Are you sure you want to remove the bookmark?--%>
            <div class="clearFix"><%=string.Format(BookmarkingUCResource.RemoveConfirmMessage, "<a href='javascript: void(0);' id='BookmarkToRemoveName'>&nbsp;</a>")%></div>

            <div class="middle-button-container">
                <ascbc:ActionButton ButtonID="BookmarkingRemoveLink" ID="BookmarkingRemoveLink" ButtonContainer="removeBookmarkDialog"
                    ButtonCssClass="button blue middle" runat="server"></ascbc:ActionButton>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="javascript:jq.unblockUI();">
                    <%=BookmarkingUCResource.Cancel%></a>
            </div>
        </body>
    </sc:Container>
</div>
