<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="Share.aspx.cs" Inherits="ASC.Web.Files.Share" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTHeaderContent">
    <style type="text/css">
        body {
          overflow: hidden;
        }
        #studioPageContent {
            min-width: auto;
        }
        #studioPageContent .page-content {
            overflow: visible;
            margin: 0;
            padding: 0;
        }
        #studioPageContent .paging-content {
            display: none;
        }
        #studioPageContent .layout-bottom-spacer {
          padding: 12px 0;
        }
        @media (min-width: 1200px) {
            #studioPageContent main {
                height: calc(100vh - 24px);
                height: calc(var(--vh, 1vh) * 100 - 24px);
            }
        }
    </style>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
