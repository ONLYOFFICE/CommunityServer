<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/News/news.Master" AutoEventWireup="true" CodeBehind="editpoll.aspx.cs" Inherits="ASC.Web.Community.News.EditPoll" Title="Untitled Page" %>

<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.UserControls.Common.PollForm" TagPrefix="sc" %>

<%@ Import Namespace="ASC.Web.Community.News.Resources" %>

<asp:Content ID="PageContent" ContentPlaceHolderID="NewsContents" runat="server">
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
