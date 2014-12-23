<%@ Assembly Name="ASC.Web.Community.Blogs" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Community/Community.master" AutoEventWireup="true" CodeBehind="AddBlog.aspx.cs" Inherits="ASC.Web.Community.Blogs.AddBlog" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Blogs.Core.Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CommunityPageHeader" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="CommunityPageContent" runat="server">
    <sc:Container id="mainContainer" runat="server">
        <header></header>
        <body>
         <div id="actionBlogPage">
              <table width="100%">
                <tr>
                  <td>
                <div class="headerPanel-splitter requiredField">
                    <span class="requiredErrorText"><%=BlogsResource.BlogTitleEmptyMessage %></span>
                    <asp:Panel ID="pnlHeader" runat="server">
                        <div id="postHeader" class="headerPanelSmall-splitter headerPanelSmall">
                            <b><%=BlogsResource.BlogTitleLabel%>:</b>
                        </div>
                        <div>
                            <asp:TextBox ID="txtTitle" MaxLength="255"  CssClass="textEdit" runat="server" Width="100%"></asp:TextBox>
                        </div>
                    </asp:Panel>
                </div>
                <div class="headerPanel-splitter">
                    <div class="headerPanelSmall-splitter">
                        <b><%=BlogsResource.ContentTitle %>:</b>
                    </div>
                    <textarea id="ckEditor" name="blog_text" style="width:100%; height:400px;" autocomplete="off"><%=_text%></textarea>
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
                <div>
                    <input type="checkbox" id="notify_comments" name="notify_comments" checked class="float-left"/><label
                        for="notify_comments"><%=BlogsResource.SubscribeOnNewCommentsAction%></label>
                </div>
                   </td>
                   <td class="teamlab-cut">
                        <div class="title-teamlab-cut"><%= BlogsResource.TeamlabCutTitle %></div>
                        <div class="text-teamlab-cut"><%= BlogsResource.TeamlabCutText %></div>
                    </td>
                    </tr>
                    </table>

                <div class="big-button-container">
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
                <asp:PlaceHolder ID="PlaceHolderPreview" runat="server"></asp:PlaceHolder>
            </div>
        </body>
    </sc:Container>
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="CommunitySidePanel" runat="server">
</asp:Content>

