<%@ Assembly Name="ASC.Web.Community"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopicListControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.TopicListControl" %>

<%@ Import Namespace="ASC.Web.UserControls.Forum.Resources" %>


<div class="forum_columnHeader">
    <table width="100%" style="height: 100%" border="0" cellpadding="0" cellspacing="0">
        <tr>
            <td align="left" valign="bottom">
                <%=ForumUCResource.Topic%>
            </td>
            <td style="width: 100px;" align="center" valign="bottom">
                <%=ForumUCResource.ViewCount%>
            </td>
            <td style="width: 100px;"align="center" valign="bottom">
                <%=ForumUCResource.PostCount%>
            </td>
            <td style="width: 180px;" valign="middle">
                <%=ForumUCResource.RecentUpdate%>
            </td>
        </tr>
    </table>
</div>

<div class="clearFix">
    <asp:PlaceHolder ID="topicListHolder" runat="server"/>
</div>