<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FeedView.ascx.cs" Inherits="ASC.Web.Community.News.Controls.FeedView" %>
<%@ Import Namespace="ASC.Web.Community.News.Resources" %>
<div id="feedPrevDiv">
    <div class="feedPrevCredits">
        <span class="gray-text">
            <%=NewsResource.PostedBy%>:
        </span>
        <span class="splitter"></span>
        <asp:Literal ID="profileLink" runat="server"></asp:Literal>
        <span id="feedPrevDiv_PostedOn" class="gray-text" style="padding-left: 20px;">
            <asp:Literal ID="Date" runat="server"></asp:Literal>
        </span>
    </div>
    <div id="feedPrevDiv_Body" class="feedPrevBody clearFix longWordsBreak">
        <asp:PlaceHolder runat="server" ID="pollHolder"/>
        <asp:Literal runat="server" ID="newsText"></asp:Literal>
    </div>
</div>

<div id="eventsActionsMenuPanel" class="studio-action-panel">
<ul class="dropdown-content">
    <asp:Literal runat="server" ID="EditorButtons"></asp:Literal>

    </ul>
</div>
