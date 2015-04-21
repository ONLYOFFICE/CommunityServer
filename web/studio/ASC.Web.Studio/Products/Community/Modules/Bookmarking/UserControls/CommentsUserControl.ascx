<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommentsUserControl.ascx.cs" Inherits="ASC.Web.UserControls.Bookmarking.CommentsUserControl" %>

<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>

<div class="clearFix" style="margin-top: 5px;">
    <scl:CommentsList ID="CommentList" runat="server">
    </scl:CommentsList>
</div>
