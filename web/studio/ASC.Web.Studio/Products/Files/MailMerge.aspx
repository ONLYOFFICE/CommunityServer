<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="MailMerge.aspx.cs" Inherits="ASC.Web.Files.MailMerge" %>

<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <div>
        <div><%= FilesUCResource.MailMergeFileTitle %></div>
        <input type="text" id="mailMergeTitle" class="textEdit" value="<%= HttpUtility.HtmlEncode(RequestFileTitle) %>" maxlength="<%= Global.MaxTitle %>"
            data-title="<%= HttpUtility.HtmlEncode(RequestFileTitle) %>"
            data-url="<%= HttpUtility.HtmlEncode(RequestUrl) %>" />
        <br />
        <br />
    </div>
    <div id="mailMergeOpenTabPanel">
        <br />
        <label>
            <input type="checkbox" id="mailMergeOpenTab" class="checkbox" /><%= FilesUCResource.MailMergeOpenTab %></label>
    </div>

    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
