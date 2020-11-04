<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Talk.DefaultTalk" Title="Untitled Page" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Talk.Addon" %>
<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="TalkContent" ContentPlaceHolderID="PageContent" runat="server">
  <sc:Container ID="mainContainer" runat="server">
	  <Body>
		  <table border="0" cellpadding="0" cellspacing="0" width="100%">
			  <tr>
				  <td colspan="3" class="header-base" style="padding: 0 0 15px">
					 <%=TalkOverviewResource.OverviewSectionTitle%>
				  </td>
			  </tr>
			  <tr valign="top">
				  <td colspan="3" style="border-bottom: 1px solid #d1d1d1; padding: 0 35px 15px 0;">
					  <div><%=TalkOverviewResource.OverviewContent%></div>                     
					  <div style="padding-top: 10px;padding-top: 0px;"><%=TalkOverviewResource.OverviewContentDescription%></div>
					  <div style="padding-top: 10px;padding-top: 0px;"><%=TalkOverviewResource.AutoupdateContactListDescription%></div>
					  <div style="padding-top: 10px;padding-top: 0px;"><%=string.Format(TalkOverviewResource.OverviewWebClientDescription, TalkAddon.GetTalkClientURL())%></div>
                      <div class="header-base" style="padding-top: 20px;"><%=TalkOverviewResource.NotificationSetup %></div>
                      <div style="padding-top: 10px;"><%=TalkOverviewResource.DefaultNotificationText%></div>
                      <div><%=string.Format(TalkOverviewResource.SubscriptionSectionText, "<a href='/Products/People/Profile.aspx'>" + TalkOverviewResource.ProfilePage + "</a>") %></div> 
                      <% if (!string.IsNullOrEmpty(HelpLink)) { %>
                      <div><%=string.Format(TalkOverviewResource.MoreInformation, "<a href='" + HelpLink + "/gettingstarted/talk.aspx#SchedulingWorkflow_block'>" + TalkOverviewResource.Here + "</a>") %></div>
                      <% } %>
				  </td>
				  <td colspan="2" style="border-bottom: 1px solid #d1d1d1; padding-bottom: 15px; vertical-align:middle">
					  <div class="tintMedium" style=" width: 200px;">
						  <span class="button blue huge open-client" onclick="ASC.Controls.JabberClient.open()"><%=TalkOverviewResource.StartWebClientLink%></span>
						  <div class="text-medium-describe" style="margin: 12px 0px; width: 200px;"><%=TalkOverviewResource.StartWebClientLinkDescription%></div>
					  </div>
				  </td>
			  </tr>
			  <tr>
				  <td colspan="5" class="talkAreaWithBottomBorder">
                         <div class="talkScreenshots">
					        <%-- <img src="images/screenshot01.png" alt="" /> --%>
						    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot01.png", TalkAddon.AddonID)%>" alt="" />
						    <div class="describe-text talkScreenshot"><%=TalkOverviewResource.ChatTabs%></div>
				         </div>
					      <div class="talkScreenshots">
					        <%-- <img src="images/screenshot02.png" alt="" /> --%>
					        <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot02.png", TalkAddon.AddonID)%>" alt="" />
						      <div class="describe-text talkScreenshot"><%=TalkOverviewResource.ContactListWithSettings%></div>
					      </div>
					      <div class="talkScreenshots">
						      <%-- <img src="images/screenshot04.png" alt="" /> --%>
						      <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot04.png", TalkAddon.AddonID)%>" alt="" />
						      <div class="describe-text talkScreenshot"><%=TalkOverviewResource.SettingsOfInputField%></div>
					      </div>
				  </td>
			  </tr>
		  </table>
	  </Body>
  </sc:Container>
</asp:Content>
