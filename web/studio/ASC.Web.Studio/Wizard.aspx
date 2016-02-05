<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Wizard.aspx.cs" Inherits="ASC.Web.Studio.Wizard" Title="ONLYOFFICE™" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<asp:Content ContentPlaceHolderID="PageContent" runat="server">

    <div class="wizardContent">
        <div class="wizardTitle">
            <%: Resources.Resource.WelcomeTitle %>
        </div>
        <div class="wizardDesc"><%: Resources.Resource.WelcomeDescription %></div>
        <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
    </div>

    <asp:PlaceHolder ID="WizardThirdPartyPlaceHolder" runat="server"></asp:PlaceHolder>

</asp:Content>
