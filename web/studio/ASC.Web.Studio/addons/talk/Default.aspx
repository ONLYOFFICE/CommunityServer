<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Talk.DefaultTalk" Title="Untitled Page" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
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
                      <div><%=string.Format(TalkOverviewResource.SubscriptionSectionText, "<a href = '/products/people/profile.aspx'>" + TalkOverviewResource.ProfilePage + "</a>") %></div> 
                      <div><%=string.Format(TalkOverviewResource.MoreInformation, "<a href = 'http://helpcenter.onlyoffice.com/gettingstarted/talk.aspx#SchedulingWorkflow_block'>" + TalkOverviewResource.Here + "</a>") %></div>
                              
           
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
						      <%-- <img src="images/screenshot03.png" alt="" /> --%>
						      <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot03.png", TalkAddon.AddonID)%>" alt="" />
						      <div class="describe-text talkScreenshot"><%=TalkOverviewResource.YourStatusChanger%></div>
					      </div>
					      <div class="talkScreenshots">
						      <%-- <img src="images/screenshot04.png" alt="" /> --%>
						      <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot04.png", TalkAddon.AddonID)%>" alt="" />
						      <div class="describe-text talkScreenshot"><%=TalkOverviewResource.SettingsOfInputField%></div>
					      </div>
					  
				  </td>
			  </tr>
			  <tr>
				  <td colspan="5" class="header-base" style="padding: 20px 0 15px">
				    <%=TalkOverviewResource.IntegrationWith3rdPartyAppsSectionTitle%>
				  </td>
			  </tr>
			  <tr valign="top">
				  <td colspan="3" style="padding: 0 35px 15px 0">
                      
					  <div style = "padding-top: 0px;">
					    <%=string.Format(TalkOverviewResource.IntegrationWith3rdPartyAppsSectionContent,
                        "<a href = 'http://helpcenter.onlyoffice.com/tipstricks/integrating-talk.aspx'>" + TalkOverviewResource.Here + "</a>")
                            %>
					  </div>
					  <div style="margin-top: 10px; padding-top: 0px;">
					    <%=string.Format(TalkOverviewResource.ThirdPartyAppsSettingsSectionContent,
                                "<b>"+JID+"</b>",
				                "<ul><li style ='list-style-type: none'>", 
					             "<b>"+ServerName+"</b></li><li style ='list-style-type: none'>",
                                 "<b>" + ServerAddress + "</b></li><li style ='list-style-type: none'>",
                                 "<b>" + ServerPort + "</b></li><li style ='list-style-type: none'>",
                                 "<b>" + UserName + "</b></li></ul>") %>
					  </div>
                      <% if (!ASC.Web.Studio.Utility.TenantExtra.Saas)
                         { %>
                      <div class="middle-button-container">
                          <div class="header-base red-text">
                              <%= TalkOverviewResource.WarningHeader %>
                          </div>
                          <div style="margin-top: 8px;">
                              <%= string.Format(TalkOverviewResource.WarningText,
                                                "<b>",
                                                "</b>",
                                                "<a href='/management.aspx'>",
                                                "</a>") %>
                          </div>
                      </div>
                      <% } %>
				  </td>
				  <td style="padding-bottom: 15px; vertical-align:middle">
					  <div class="tintMedium" style="width: 122px">
                          <div class="header-base" style="margin-bottom: 15px; column-span:all"><%=TalkOverviewResource.IMClients%></div>
						  <div class="talkTrillianClientImage">
						    <a href="http://www.trillian.im/download/" class="external" title="<%=TalkOverviewResource.TrillianLink%>" target="_blank">Trillian</a>
						  </div>
						  <div class="talkMirandaIMClientImage">
						    <a href="http://www.miranda-im.org/" class="external" title="<%=TalkOverviewResource.MirandaIMLink%>" target="_blank">Miranda</a>
						    </div>
						  <div class="talkPidginClientImage">
						    <a href="http://pidgin.im/" class="external" title="<%=TalkOverviewResource.PidginLink%>" target="_blank">Pidgin</a>
						  </div>
						  <div class="talkPsiClientImage">
						    <a href="http://psi-im.org/" class="external" title="<%=TalkOverviewResource.PsiLink%>" target="_blank">Psi</a>
						  </div>							
					  </div>
				  </td>
                   <td>
					  <div class="tintMedium" style="width: 122px;">
						  <div class="header-base" style="margin-top: 40px;"></div>
						  <div class="talkAdiumClientImage">
						    <a href="https://adium.im/" class="external" title="<%=TalkOverviewResource.Adium%>" target="_blank">Adium</a>
						  </div>
						  <div class="talkqutimClientImage">
						    <a href="https://qutim.org/" class="external" title="<%=TalkOverviewResource.qutlIM%>" target="_blank">qutIM</a>
						    </div>
						  <div class="talkMailRuAgentClientImage">
						    <a href="https://agent.mail.ru/" class="external" title="<%=TalkOverviewResource.MailRuAgent%>" target="_blank">Mail.ru Agent</a>
						  </div>
						  <div class="talkQipClientImage">
						    <a href="http://welcome.qip.ru/im" class="external" title="<%=TalkOverviewResource.QIP%>" target="_blank">QIP</a>
						  </div>							
					  </div>
				  </td>
			  </tr>
		  </table>
	  </Body>
  </sc:Container>
</asp:Content>
