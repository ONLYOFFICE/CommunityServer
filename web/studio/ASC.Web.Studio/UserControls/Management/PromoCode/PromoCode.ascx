<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PromoCode.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PromoCode" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="promoCodeSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.EmailAndPasswordPromocode %>
        </div>
        <input type="text" id="promoCodeSettings_input" class="textEdit clearFix" maxlength="30" style="width: 100%" />
        <div class="middle-button-container">
            <a class="float-left button blue" onclick="PromoCodeManagement.ActivateKey();" href="javascript:void(0);">
                <%= Resource.SaveButton %></a>
        </div>
    </div>
</div>
