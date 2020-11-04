<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/BaseTemplate.master" CodeBehind="Wizard.aspx.cs" Inherits="ASC.Web.Studio.Wizard" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<asp:Content ContentPlaceHolderID="PageContent" runat="server">

    <div class="wizardContent">
        <div class="wizardTitle">
            <%: Resources.Resource.WelcomeTitle %>
        </div>
        <div class="wizardDesc"><%: Resources.Resource.WelcomeDescription %></div>
        <asp:PlaceHolder ID="content" runat="server"/>
    </div>

</asp:Content>
