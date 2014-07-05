<%@ Assembly Name="ASC.Web.Community.Birthdays" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Community/Community.master" AutoEventWireup="true"
 CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Birthdays.Default" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Community.Birthdays.Resources" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Community.Birthdays" %>

<asp:Content ID="SettingsHeaderContent" ContentPlaceHolderID="CommunityPageHeader" runat="server">
</asp:Content>

<asp:Content ID="SettingsPageContent" ContentPlaceHolderID="CommunityPageContent"  runat="server">
    
    
    <asp:repeater ID="todayRpt" runat="server" ItemType="ASC.Core.Users.UserInfo">
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
                                    <img src="<%# Item.GetBigPhotoURL() %>">
                                </div>

                                <div class="user-info">
                                    <a class="linkHeader" href="<%#CommonLinkUtility.GetUserProfile(Item.ID) %>">
                                        <%# Item.DisplayUserName()%>
                                    </a>
                                    <div class="user-info-title gray-text">
                                        <%# Item.Title%>
                                    </div>
                                    <a class="<%# ((UserInfo)Container.DataItem).ID == SecurityContext.CurrentAccount.ID ? "display-none" : "button blue middle" %>"
                                         onclick="ASC.Community.Birthdays.openContact(this);"
                                         username="<%# Item.UserName.HtmlEncode() %>">
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

    <asp:repeater ID="upcomingRpt" runat="server" ItemType="ASC.Web.Community.Birthdays.Default.BirthdayWrapper">
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
                                <%# Item.Date.ToShortDayMonth()%>
                            </div>
                        </td>
                        <td class="borderBase">
                            <div class="user-card-list">
                                <asp:repeater ID="uinnerRpt" runat="server" DataSource="<%# Item.Users %>" ItemType="ASC.Core.Users.UserInfo">
                                    <ItemTemplate>
                                        <div class="small-user-card clearFix <%# IsInRemindList(Item.ID) ? "active" : "" %>">
                                            <div class="user-avatar">
                                                <img src="<%# UserPhotoManager.GetMediumPhotoURL(Item.ID) %>">
                                            </div>
                                            <div class="user-info">
                                                <a class="link bold" href="<%# CommonLinkUtility.GetUserProfile(Item.ID) %>">
                                                    <%# Item.DisplayUserName()%>
                                                </a>
                                                <div class="user-info-title <%# Item.ID == SecurityContext.CurrentAccount.ID ? "display-none" : "" %>">
                                                    <a class="baseLinkAction gray-text birthday-remind" onclick="ASC.Community.Birthdays.remind(this);">
                                                        <%= BirthdaysResource.BirthdayRemindLinkTitle%>
                                                    </a>
                                                    <span class="birthday-remind-active">
                                                        <%= BirthdaysResource.BirthdayRemindReadyLinkTitle%>
                                                        <a onclick="ASC.Community.Birthdays.clearRemind(this);"></a>
                                                    </span>
                                                </div>
                                                <input type="hidden" value="<%# Item.ID %>"/>
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