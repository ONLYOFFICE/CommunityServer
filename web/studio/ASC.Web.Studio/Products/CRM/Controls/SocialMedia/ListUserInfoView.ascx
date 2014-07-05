<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListUserInfoView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.SocialMedia.ListUserInfoView" EnableViewState="false" %>
<asp:Repeater runat="server" ID="_ctrlRptrUsers" OnItemDataBound="_ctrlRptrUsers_ItemDataBound">
    <HeaderTemplate>
        <table cellpadding="0" cellspacing="0" style="width: 100%">
    </HeaderTemplate>
    <ItemTemplate>
        <tr class="tintMedium">
            <td style="width: 50px; text-align: center; padding: 5px 0;">
                <div style="min-height: 40px;">
                    <img alt="" src="<%# Eval("SmallImageUrl") %>" width="40" />&nbsp;
                </div>
            </td>
            <td style="padding: 5px 0 5px 10px;">
                <span class="headerBaseSmall sn_userName" style="color: Black !important;">
                    <%# Eval("UserName") %></span>
                <br />
                <%# Eval("Description") %>
            </td>
            <td style="width: 80px; padding: 5px 0 5px 10px;">
                <a href="#" id="_ctrlRelateContactWithAccount" runat="server">
                    <%= SocialMediaResource.Relate %></a>
            </td>
        </tr>
    </ItemTemplate>
    <AlternatingItemTemplate>
        <tr class="tintLight">
            <td style="width: 50px; text-align: center; padding: 5px 0;">
                <div style="min-height: 40px;">
                    <img alt="" src="<%# Eval("SmallImageUrl") %>" width="40" />&nbsp;
                </div>
            </td>
            <td style="padding: 5px 0 5px 10px;">
                <span class="headerBaseSmall sn_userName" style="color: Black !important;">
                    <%# Eval("UserName") %></span>
                <br />
                <%# Eval("Description") %>
            </td>
            <td style="width: 80px; padding: 5px 0 5px 10px;">
                <a href="#" id="_ctrlRelateContactWithAccount" runat="server">
                    <%= SocialMediaResource.Relate %></a>
            </td>
        </tr>
    </AlternatingItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>
<div runat="server" id="_ctrlDivNotFound" style="text-align: center; margin: 10px;">
    <%= SocialMediaResource.NoResults %></div>
