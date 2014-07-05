<%@ Assembly Name="ASC.Web.UserControls.SocialMedia" %>
<%@ Import Namespace="System" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListActivityMessageView.ascx.cs"
    Inherits="ASC.Web.UserControls.SocialMedia.UserControls.ListActivityMessageView" %>
<asp:Repeater runat="server" ID="_ctrlRptrUserActivity" OnItemDataBound="_ctrlRptrUserActivity_ItemDataBound">
    <ItemTemplate>
        <div class="sm_message_line tintMedium clearFix">
            <div class="sn_user_icon">
                <img alt="" src="<%# Eval("UserImageUrl") %>" width="30" />&nbsp;
            </div>
            <div class="sn_message_block longWordsBreak">
                <span class="header-base-small sn_userName">
                    <asp:Image runat="server" ID="_ctrlImgSocialMediaIcon" CssClass="sn_small_img" />
                    <%# Eval("UserName") %></span><br />
                <div class="sn_message_text">
                    <%# Eval("Text") %>
                </div>
                <span class="describe-text">
                    <%# ((DateTime)Eval("PostedOn")).ToShortString() %></span>
            </div>
        </div>
    </ItemTemplate>
    <AlternatingItemTemplate>
        <div class="sm_message_line tintLight clearFix">
            <div class="sn_user_icon">
                <img alt="" src="<%# Eval("UserImageUrl") %>" width="32" />
            </div>
            <div class="sn_message_block longWordsBreak">
                <span class="header-base-small sn_userName">
                    <asp:Image runat="server" ID="_ctrlImgSocialMediaIcon" CssClass="sn_small_img" />
                    <%# Eval("UserName") %></span><br />
                <div class="sn_message_text">
                    <%# Eval("Text") %>
                </div>
                <span class="describe-text">
                    <%# ((DateTime)Eval("PostedOn")).ToShortString() %></span>
            </div>
        </div>
    </AlternatingItemTemplate>
</asp:Repeater>
