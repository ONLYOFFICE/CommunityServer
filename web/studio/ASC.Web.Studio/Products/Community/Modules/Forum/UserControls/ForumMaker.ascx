<%@ Assembly Name="ASC.Web.Community" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumMaker.ascx.cs" Inherits="ASC.Web.Community.Forum.ForumMaker" %>

<%@ Import Namespace="ASC.Web.Community.Forum.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<input type="hidden" id="forum_fmCallback" value="" />
<div id="forum_fmDialog" style="display: none;">
    <sc:Container ID="_forumMakerContainer" runat="server">
        <header>
            <%=ForumResource.AddThreadCategoryTitle%>
          </header>
        <body>
            
        </body>
        <options ispopup="true" />
    </sc:Container>
</div>
