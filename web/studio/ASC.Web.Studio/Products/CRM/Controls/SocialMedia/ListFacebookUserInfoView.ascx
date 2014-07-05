<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListFacebookUserInfoView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.SocialMedia.ListFacebookUserInfoView" EnableViewState="false" %>
<asp:Repeater runat="server" ID="_ctrlRptrUsers" OnItemDataBound="_ctrlRptrUsers_ItemDataBound">
    <HeaderTemplate>
        <table id="sm_tbl_UserList">
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td class="sm_tbl_UserList_clmnAvatar">
                <div style="min-height: 40px;">
                    <img alt="" src="<%# Eval("SmallImageUrl") %>" width="40" />&nbsp;
                </div>
            </td>
            <td class="sm_tbl_UserList_clmnUserName">
                <span class="header-base-small sn_userName" style="color: Black !important;">
                    <%# Eval("UserName") %></span>
            </td>
            <td class="sm_tbl_UserList_clmnBtRelate">
                <a href="#" id="_ctrlRelateContactWithAccount" runat="server">
                    <%= CRMSocialMediaResource.Relate %></a>
            </td>
        </tr>
    </ItemTemplate>
    <AlternatingItemTemplate>
        <tr class="tintLight">
            <td class="sm_tbl_UserList_clmnAvatar">
                <div style="min-height: 40px;">
                    <img alt="" src="<%# Eval("SmallImageUrl") %>" width="40" />&nbsp;
                </div>
            </td>
            <td class="sm_tbl_UserList_clmnUserName">
                <span class="header-base-small sn_userName" style="color: Black !important;">
                    <%# Eval("UserName") %></span>
            </td>
            <td class="sm_tbl_UserList_clmnBtRelate">
                <a href="#" id="_ctrlRelateContactWithAccount" runat="server">
                    <%= CRMSocialMediaResource.Relate %></a>
            </td>
        </tr>
    </AlternatingItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>
<div runat="server" id="_ctrlDivNotFound" style="text-align: center; margin: 10px;">
    <%= CRMSocialMediaResource.NoResults %></div>
