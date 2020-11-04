<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListFiles.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.ListFiles" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Data" %>
<div class="wikiList">
    <asp:Repeater ID="rptListFiles" runat="Server">
        <ItemTemplate>
            <div>
                <a class = "linkHeaderMedium" href="<%#GetFileLink((Container.DataItem as File).FileName)%>" title="<%#(Container.DataItem as File).UploadFileName%>">
                    <%#(Container.DataItem as File).FileName%></a> &nbsp;
                <asp:LinkButton ID="cmdDeleteFile" runat="Server" Text="Del" OnClientClick="javascript:return confirm('Delete?');"
                    OnClick="cmdDeleteFile_Click" CommandName='<%# (Container.DataItem as File).FileName%>' />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
