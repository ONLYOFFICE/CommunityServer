<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="confirm.aspx.cs" Inherits="ASC.Web.Studio.Confirm" %>
<%@ Import Namespace="ASC.Core" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder runat="server" ID="_contentWithControl">
        <div class="confirm-block-page <%= CoreContext.Configuration.Personal ? "confirm-personal" : "" %>">
            <div class="confirm-block-cnt">
                <div class="confirm-block-header">
                    <a href="Auth.aspx">
                        <img class="logo" src="/TenantLogo.ashx?logotype=2" border="0" alt="" /></a>
                    <div class="header-base big blue-text"><%=HttpUtility.HtmlEncode(CoreContext.TenantManager.GetCurrentTenant().Name)%></div>
                </div>

                <%if (!String.IsNullOrEmpty(ErrorMessage))
                  {%>
                <div id="studio_confirmMessage" class="message-box">
                    <div class="errorText"><%=ErrorMessage%></div>
                </div>
                <%} %>

                <asp:PlaceHolder runat="server" ID="_confirmHolder"/>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="_confirmHolder2"/>
</asp:Content>
