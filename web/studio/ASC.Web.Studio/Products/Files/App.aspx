<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="App.aspx.cs" Inherits="ASC.Web.Files.App" %>

<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server">
        <div class="files-app-center">
            <div class="files-app-content <%= IsConvert ? "" : "files-app-content-create" %>">
                <% if (IsConvert)
                   { %>

                <div class="files-app-baner"></div>
                <%: FilesCommonResource.AppConvertCopy %>
                <asp:CheckBox runat="server" ID="ConvertCheck" CssClass="files-app-checkbox" />
                <asp:Button runat="server" ID="ButtonConvert" CssClass="files-app-convert button big blue" />

                <% }
                   else
                   { %>

                <span class="header-base middle"><%= FilesCommonResource.AppEnterName %></span>
                <asp:TextBox runat="server" ID="InputName" CssClass="app-filename textEdit"></asp:TextBox>
                <span class="header-base middle"><%= FilesCommonResource.AppEnterType %></span>
                <div class="files-app-types">
                    <asp:Button runat="server" ID="ButtonCreateDocument" CssClass="files-app-create document" />
                    <asp:Button runat="server" ID="ButtonCreateSpreadsheet" CssClass="files-app-create spreadsheet" />
                    <asp:Button runat="server" ID="ButtonCreatePresentation" CssClass="files-app-create presentation" />
                </div>

                <% } %>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
