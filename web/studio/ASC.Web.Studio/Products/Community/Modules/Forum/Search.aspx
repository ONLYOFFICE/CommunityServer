<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Search.aspx.cs" Inherits="ASC.Web.Community.Forum.Search" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Web.Community.Forum.Resources" %>


<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">

    <div class="forum_columnHeader" style="<%=(_isFind?"":"display:none;")%>">
        <table width="100%" style="height:100%" border="0" cellpadding="0" cellspacing="0">
            <tr>                    
                <td style="padding-left:5px;" valign="middle">
                    <%=ForumResource.Topic%>
                </td>                 
                <td style="width: 100px;" align="center"
                    valign="middle">
                    <%=ForumResource.ViewCount%>
                </td>
                    <td style="width: 100px;" align="center"
                    valign="middle">
                    <%=ForumResource.ShotPostCount%>
                </td>
                <td style="width: 180px;" valign="middle">
                    <%=ForumResource.RecentUpdate%>
                </td>
            </tr>
        </table>
    </div>

    <div class="clearFix">
        <asp:PlaceHolder ID="topicListHolder" runat="server"/>
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPagingContent" runat="server">
    <div class="clearFix" style="padding-top:20px;">
        <asp:PlaceHolder ID="bottomPageNavigatorHolder" runat="server"/>
    </div>
</asp:Content>