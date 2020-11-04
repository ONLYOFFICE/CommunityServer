<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="SaveAs.aspx.cs" Inherits="ASC.Web.Files.SaveAs" %>

<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <div>
        <div><%= FilesUCResource.SaveAsFileTitle %></div>
        <input type="text" id="saveAsTitle" class="textEdit" value="<%= HttpUtility.HtmlEncode(RequestFileTitle) %>" maxlength="<%= Global.MaxTitle %>"
            data-title="<%= HttpUtility.HtmlEncode(RequestFileTitle) %>"
            data-url="<%= HttpUtility.HtmlEncode(RequestUrl) %>" />
        <br />
        <br />
    </div>
    <div id="saveAsOpenTabPanel">
        <br />
        <label>
            <input type="checkbox" id="saveAsOpenTab" class="checkbox" /><%= FilesUCResource.SaveAsOpenTab %></label>
    </div>

    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
