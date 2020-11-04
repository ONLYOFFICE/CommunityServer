<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/News/News.Master" AutoEventWireup="true" CodeBehind="EditNews.aspx.cs" Inherits="ASC.Web.Community.News.EditNews" %>

<%@ Import Namespace="ASC.Web.Community.News.Resources" %>


<asp:Content ContentPlaceHolderID="NewsTitleContent" runat="server">
    <div class="eventsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon events"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(PageTitle)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="NewsContents" runat="server">
    <div id="actionNewsPage" style="margin-top: 15px;">
    <div class="headerPanel-splitter requiredField">
        <span class="requiredErrorText"><%=NewsResource.RequaredFieldValidatorCaption%></span>
        <div class="headerPanelSmall-splitter headerPanelSmall" id="newsCaption">
            <b><%=NewsResource.NewsCaption%>:</b>
        </div>
        <asp:TextBox runat="server" ID="feedName" class="textEdit" Style="width: 100%" />
    </div>
    <div class="headerPanel-splitter">
        <div style="float: left; margin-right: 8px;">
            <b><%=NewsResource.NewsType%>:</b>
        </div>
        <asp:DropDownList runat="server" ID="feedType" class="comboBox" DataTextField="TypeName" DataValueField="id" CssClass="display-none" />
    </div>
    <div class="headerPanel-splitter">
        <div class="headerPanelSmall-splitter">
            <b><%=NewsResource.NewsBody%>:</b>
        </div>
        <textarea id="ckEditor" name="news_text" style="width:100%; height:400px;visibility:hidden;" autocomplete="off"><%=_text%></textarea>
    </div>

    <div class="big-button-container" id="panel_buttons">
        <a id="lbSave" class="button blue big" onclick="submitNewsData(this)"><%=NewsResource.PostButton%></a>
        <span class="splitter-buttons"></span>
        <% if (string.IsNullOrEmpty(_text))
        { %>
        <a id="btnPreview" class="button blue big disable" onclick="GetPreviewFull();"><%= NewsResource.Preview %></a>
    <% } else { %>
        <a id="btnPreview" class="button blue big" onclick="GetPreviewFull();"><%= NewsResource.Preview %></a>
    <% } %>

        <span class="splitter-buttons"></span>
        <asp:LinkButton ID="lbCancel" CssClass="button gray big cancelFckEditorChangesButtonMarker"
        OnClick="CancelFeed" CausesValidation="true" OnClientClick="NewsBlockButtons()" runat="server"><%=NewsResource.CancelButton%></asp:LinkButton>
    </div>
    </div>
    <div id="feedPrevDiv" style="display: none; padding-top: 20px">
        <div class="headerPanel">
            <%=NewsResource.FeedPrevCaption%>
        </div>
        <input id="feedPrevDiv_Caption" class="feedPrevCaption" />
        <div id="feedPrevDiv_Body" class="feedPrevBody clearFix longWordsBreak">
        </div>
        <div style='margin-top:25px;'><a class="button blue big" href='javascript:void(0);' onclick='HidePreview(); return false;'><%= NewsResource.HideButton%></a></div>
    </div>

</asp:Content>
