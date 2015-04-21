<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FeedItem.ascx.cs" Inherits="ASC.Web.Community.News.Controls.FeedItem" %>

<td class="borderBase gray-text events_date">
    <asp:Label runat="server" ID="Date"></asp:Label>
</td>
<td class="borderBase events_type">
    <b><asp:Literal runat="server" ID="Type"></asp:Literal></b>
</td>
<td class="borderBase events_name">
    <div class="news-link-container">
        <asp:HyperLink runat="server" ID="NewsLink" class="linkMedium longWordsBreak"></asp:HyperLink>
    </div>
</td>
<td class="borderBase events_autor">
    <asp:Literal runat="server" ID="profileLink"></asp:Literal>
</td>
