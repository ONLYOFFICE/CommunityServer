<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" AutoEventWireup="true"
    EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Forum.Default" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Web.Community.Forum.Resources" %>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <asp:PlaceHolder ID="_headerHolder" runat="server">
        <div class="forum_columnHeader">
            <table width="100%" style="height: 100%" border="0" cellpadding="0" cellspacing="0">
                <tr>
                    <td style="padding-left: 5px;" valign="middle">
                        <%=ForumResource.Thread%>
                    </td>
                    <td style="width: 100px;" align="center"
                        valign="middle">
                        <%=ForumResource.TopicCount%>
                    </td>
                    <td style="width: 100px;" align="center"
                        valign="middle">
                        <%=ForumResource.PostCount%>
                    </td>
                    <td style="width: 180px;" valign="middle">
                        <%=ForumResource.RecentUpdate%>
                    </td>
                </tr>
            </table>
        </div>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="forumListHolder" runat="server"/>
</asp:Content>
