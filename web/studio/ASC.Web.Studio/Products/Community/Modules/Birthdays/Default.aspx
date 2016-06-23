<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.master" AutoEventWireup="true"
 CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Birthdays.Default" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Community.Birthdays.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:Content ID="SettingsHeaderContent" ContentPlaceHolderID="CommunityPageHeader" runat="server">
</asp:Content>

<asp:Content ID="SettingsPageContent" ContentPlaceHolderID="CommunityPageContent"  runat="server">
    
    
    <asp:repeater ID="todayRpt" runat="server">
        <HeaderTemplate>
            <div class="birthday-header birthday-content-splitter">
                <%= BirthdaysResource.BirthdaysTodayTitle%>
            </div>
            <table class="birthday-content-splitter" cellspacing="0" cellpadding="0">
            <colgroup>
                <col width="166px"/>
                <col/>
            </colgroup>
            <tbody>
                <tr>
                    <td class="birthday-date birthday-header">
                        <%= DateTime.UtcNow.ToString("dd MMMM").ToLower()%>
                    </td>
                    <td>
                        <div class="user-card-list">
        </HeaderTemplate>
        <ItemTemplate>
                            <div class="user-card clearFix">

                                <div class="user-avatar borderBase">
                                    <img src="<%# ((ASC.Core.Users.UserInfo)Container.DataItem).GetBigPhotoURL() %>">
                                </div>

                                <div class="user-info">
                                    <a class="linkHeader" href="<%#CommonLinkUtility.GetUserProfile(((ASC.Core.Users.UserInfo)Container.DataItem).ID) %>">
                                        <%# ((ASC.Core.Users.UserInfo)Container.DataItem).DisplayUserName()%>
                                    </a>
                                    <div class="user-info-title gray-text">
                                        <%# ((ASC.Core.Users.UserInfo)Container.DataItem).Title%>
                                    </div>
                                    <a class="<%# ((UserInfo)Container.DataItem).ID == SecurityContext.CurrentAccount.ID ? "display-none" : "button blue middle" %>"
                                         onclick="ASC.Community.Birthdays.openContact(this);"
                                         username="<%# ((ASC.Core.Users.UserInfo)Container.DataItem).UserName.HtmlEncode() %>">
                                        <%= BirthdaysResource.BirthdayCongratulateLinkTitle%>
                                    </a>
                                </div>
                                
                            </div>
        </ItemTemplate>
        <FooterTemplate>
                        </div>
                    </td>
                </tr>
            </tbody>
            </table>
        </FooterTemplate>
    </asp:repeater>

    <div class="birthday-header birthday-content-splitter">
        <%= BirthdaysResource.BirthdaysUpcomingTitle%>
    </div>

    <asp:PlaceHolder ID="upcomingEmptyContent" runat="server"></asp:PlaceHolder>

    <asp:repeater ID="upcomingRpt" runat="server">
        <HeaderTemplate>
            <table class="tableBase birthday-content-splitter" cellspacing="0" cellpadding="8">
                <colgroup>
                    <col width="80px" />
                    <col />
                </colgroup>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
                    <tr>
                        <td class="birthday-date birthday-header borderBase">
                            <div class="birthday-date">
                                <%# ((ASC.Web.Community.Birthdays.Default.BirthdayWrapper)Container.DataItem).Date.ToShortDayMonth()%>
                            </div>
                        </td>
                        <td class="borderBase">
                            <div class="user-card-list">
                                <asp:repeater ID="uinnerRpt" runat="server" DataSource="<%# ((ASC.Web.Community.Birthdays.Default.BirthdayWrapper)Container.DataItem).Users %>">
                                    <ItemTemplate>
                                        <div class="small-user-card clearFix <%# IsInRemindList(((ASC.Core.Users.UserInfo)Container.DataItem).ID) ? "active" : "" %>">
                                            <div class="user-avatar">
                                                <img src="<%# UserPhotoManager.GetMediumPhotoURL(((ASC.Core.Users.UserInfo)Container.DataItem).ID) %>">
                                            </div>
                                            <div class="user-info">
                                                <a class="link bold" href="<%# CommonLinkUtility.GetUserProfile(((ASC.Core.Users.UserInfo)Container.DataItem).ID) %>">
                                                    <%# ((ASC.Core.Users.UserInfo)Container.DataItem).DisplayUserName()%>
                                                </a>
                                                <div class="user-info-title <%# ((ASC.Core.Users.UserInfo)Container.DataItem).ID == SecurityContext.CurrentAccount.ID ? "display-none" : "" %>">
                                                    <a class="baseLinkAction gray-text birthday-remind" onclick="ASC.Community.Birthdays.remind(this);">
                                                        <%= BirthdaysResource.BirthdayRemindLinkTitle%>
                                                    </a>
                                                    <span class="birthday-remind-active">
                                                        <%= BirthdaysResource.BirthdayRemindReadyLinkTitle%>
                                                        <a onclick="ASC.Community.Birthdays.clearRemind(this);"></a>
                                                    </span>
                                                </div>
                                                <input type="hidden" value="<%# ((ASC.Core.Users.UserInfo)Container.DataItem).ID %>"/>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:repeater>
                            </div>
                        </td>
                    </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:repeater>
               
</asp:Content>