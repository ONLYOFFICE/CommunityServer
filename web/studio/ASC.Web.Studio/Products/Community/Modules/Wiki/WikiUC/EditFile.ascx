<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditFile.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.EditFile" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Resources" %>
<div class="<%=MainWikiClassName %>">
    <div class="wikiEdit">
        <div class="headerPanel">
            <%=WikiUCResource.editWiki_FileName%>
        </div>
        <div class="subHeaderPanel">
            <%=FileName%>
        </div>
        <div class="headerPanel">
            <%=WikiUCResource.editWiki_UploadFileName%>
        </div>
        <div class="subHeaderPanel">
            <%=GetUploadFileName()%>
        </div>
        <div class="headerPanel">
            <%=WikiUCResource.editWiki_File%>
        </div>
        <div class="subHeaderPanel">
            <%=GetFileLink() %>
        </div>
        <div class="headerPanel">
            <%=WikiUCResource.editWiki_UploadCaption%>
        </div>
        <div class="subHeaderPanel">
            <asp:FileUpload ID="fuFile" runat="server" />
        </div>
    </div>
</div>
