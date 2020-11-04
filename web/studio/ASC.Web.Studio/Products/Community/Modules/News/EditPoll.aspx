<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/News/News.Master" AutoEventWireup="true" CodeBehind="EditPoll.aspx.cs" Inherits="ASC.Web.Community.News.EditPoll" Title="Untitled Page" %>

<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.UserControls.Common.PollForm" TagPrefix="sc" %>

<%@ Import Namespace="ASC.Web.Community.News.Resources" %>


<asp:Content ContentPlaceHolderID="NewsTitleContent" runat="server">
    <div class="eventsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon events"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(PageTitle)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="NewsContents" runat="server">
    <div  id="actionNewsPage" style="margin-top: 15px;">
        <asp:Label ID="_errorMessage" runat="server"></asp:Label>
        <sc:PollFormMaster runat="server" ID="_pollMaster" />
        <div class="big-button-container">
            <a ID="lbSave" class="button blue big" onclick="submitPollData(this)"><%=NewsResource.SaveButton%></a>
            <span class="splitter-buttons"></span>
            <asp:HyperLink ID="lbCancel" CssClass="button gray big" runat="server"><%=NewsResource.CancelButton%></asp:HyperLink>
        </div>
    </div>
</asp:Content>
