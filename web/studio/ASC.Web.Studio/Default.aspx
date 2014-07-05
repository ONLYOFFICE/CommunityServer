<%@ Page Language="C#" MasterPageFile="~/Masters/basetemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Studio._Default" Title="ONLYOFFICE™" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="GreetingBlock" class="greating-block">
        <div class="greating-modules-block">
            <div class="header-base big">
                <%=String.Format(Resources.Resource.WelcomeUserMessage, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(true))%>
            </div>

            <% if (_showDocs != null)
               { %>
            <div class="docs-default-page clearFix">
                <a class="docs-default-logo" href="<%=VirtualPathUtility.ToAbsolute(_showDocs.StartURL)%>"></a>
                <h2 class="title">
                    <a class="link header" href="<%=VirtualPathUtility.ToAbsolute(_showDocs.StartURL)%>">
                        <%=_showDocs.Name %></a>
                </h2>
                <span class="description">
                    <%=(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) ? _showDocs.ExtendedDescription : _showDocs.Description%></span>
            </div>
            <% } %>

            <asp:Repeater runat="server" ID="_productRepeater" ItemType="ASC.Web.Core.IWebItem">
                <HeaderTemplate>
                    <div class="default-list-products">
                </HeaderTemplate>
                <ItemTemplate>
                    <div class="product clearFix">
                        <a class="image-link" href="<%#VirtualPathUtility.ToAbsolute(Item.StartURL)%>">
                            <img alt="" src="<%#Item.GetLargeIconAbsoluteURL()%>" /></a>
                        <h2 class="title">
                            <a class="link header" href="<%#VirtualPathUtility.ToAbsolute(Item.StartURL)%>">
                                <%#HttpUtility.HtmlEncode(Item.Name)%></a>
                        </h2>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

    <%if (ShowWelcomePopupForCollaborator)
      { %>
    <asp:PlaceHolder runat="server" ID="_welcomePopupForCollaborators">
        <div id="studio_welcomeCollaboratorContainer" class="display-none">
            <sc:Container runat="server" ID="_welcomeCollaboratorContainer">
                <Header>
                    <%=Resources.Resource.WelcomeCollaboratorPopupHeader%>
                </Header>
                <Body>
                    <div class="welcome-to-teamlab-with-logo">
                        <p class="welcome">
                            <%=Resources.Resource.WelcomeToTeamlab%>
                        </p>
                        <p>
                            <%=Resources.Resource.WelcomeCollaboratorRole%>
                        </p>
                        <%=String.Format(Resources.Resource.WelcomeCollaboratorCan, "<p>", "</p><ul class='welcome-collaborator-can'><li>", "</li><li>", "</li><li>", "</li></ul>")%>
                        <p>
                            <%= Resources.Resource.WelcomeCollaboratorOtherActions %>
                        </p>
                    </div>
                    <div class="middle-button-container">
                        <a class="button" onclick="StudioDefault.CloseWelcomePopup();javascript:jq.unblockUI();">
                            <%=Resources.Resource.WelcomeCollaboratorStartWork %></a>
                    </div>
                </Body>
            </sc:Container>
        </div>
    </asp:PlaceHolder>
    <%} %>

    <% if (!string.IsNullOrEmpty(SetupInfo.UserVoiceURL))
       { %>
    <script type="text/javascript" src="<%= SetupInfo.UserVoiceURL %>"></script>
    <% } %>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="FooterContent" runat="server">
    <%if (IsAutorizePartner.HasValue && Partner != null) { %>
    <div class="footerAuth">
        <span class="float-right">
            <%= IsAutorizePartner.Value ? (Partner.DisplayName ?? Partner.CompanyName) + " • <a class=\"link\" href=\"" + Partner.Url +"\" target=\"_blank\">" + Partner.Url + "</a>" 
            : Resource.HostedNonAuthorizedVersion%>
        </span>
    </div>
    <%} %>
</asp:Content>