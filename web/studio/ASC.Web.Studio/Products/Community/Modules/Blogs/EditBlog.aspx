<%@ Assembly Name="ASC.Web.Community" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.Master" AutoEventWireup="true" CodeBehind="EditBlog.aspx.cs" Inherits="ASC.Web.Community.Blogs.EditBlog" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Blogs.Core.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<asp:Content ContentPlaceHolderID="CommunityPageHeader" runat="server">
 
    <script type="text/javascript">
    function callback(result)
    {
        if(result.value != null)
        {
            document.getElementById("tag_" + result.value).innerHTML = "";
        }
    }
    </script>

</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityTitleContent" runat="server">
    <div class="BlogsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon blogs"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(BlogsResource.EditPostTitle)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">
    <sc:Container id="mainContainer" runat="server">
        <header>
        </header>
        <body>
         <div id="actionBlogPage">
            <asp:HiddenField ID="hidBlogID" runat="server" />
            <asp:HiddenField ID="hdnUserID" runat="server" />
            <table width="100%">
                <tr>
                <td>
            <a name="post"></a>
            <div id="postHeader" class="headerPanel-splitter requiredField">
                <span class="requiredErrorText"><%=BlogsResource.BlogTitleEmptyMessage %></span>
                <asp:Panel ID="pnlHeader" runat="server">
                    <div class="headerPanelSmall-splitter headerPanelSmall">
                        <b><%=BlogsResource.BlogTitleLabel%>:</b>
                    </div>
                    <div>
                        <asp:TextBox ID="txtTitle" MaxLength="255" CssClass="textEdit" runat="server" Width="100%"></asp:TextBox>
                    </div>
                </asp:Panel>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=BlogsResource.ContentTitle %>:</b>
                </div>
                <textarea id="ckEditor" name="blog_text" style="width:100%; height:400px;visibility:hidden;" autocomplete="off"><%=_text%></textarea>
            </div>
            <div class="headerPanel-splitter">
                <div class="headerPanelSmall-splitter">
                    <b><%=BlogsResource.TagsTitle%>:</b>
                </div>
                <div>
                    <asp:TextBox CssClass="textEdit" ID="txtTags" runat="server" Width="100%"
                        autocomplete="off" onkeydown="return blogTagsAutocompleteInputOnKeyDown(event);"></asp:TextBox>
                    <div class="text-medium-describe" style="text-align: left;">
                        <%=BlogsResource.EnterTagsMessage%>
                    </div>
                </div>
            </div>
             </td>
               <td class="teamlab-cut">
                    <div class="title-teamlab-cut"><%: BlogsResource.TeamlabCutTitle %></div>
                    <div class="text-teamlab-cut"><%: BlogsResource.TeamlabCutText %></div>
                </td>
                </tr>
                </table>
            <div class="big-button-container" >
                <a class="button blue big" onclick="BlogsManager.SubmitData(this)"><%=BlogsResource.PostButton%></a>
                <span class="splitter-buttons"></span>
                    <% if (string.IsNullOrEmpty(_text))
                       { %>
                        <a id="btnPreview" class="button blue big disable" onclick="BlogsManager.ShowPreview('<%= txtTitle.ClientID %>')"><%= BlogsResource.PreviewButton %></a>
                    <% } else { %>
                        <a id="btnPreview" class="button blue big" onclick="BlogsManager.ShowPreview('<%= txtTitle.ClientID %>')"><%= BlogsResource.PreviewButton %></a>
                    <% } %>
                <span class="splitter-buttons"></span>
                <asp:LinkButton ID="lbCancel" OnClientClick="javascript:BlogsManager.OnClickCancel();"
                    CssClass="button gray big cancelFckEditorChangesButtonMarker" runat="server"
                    OnClick="lbCancel_Click"><%=BlogsResource.CancelButton %></asp:LinkButton>
            </div>
            </div>
            <div id="previewHolder" style="display: none;">
                <asp:PlaceHolder ID="PlaceHolderPreview" runat="server"/>
            </div>
        </body>
    </sc:Container>
</asp:Content>
