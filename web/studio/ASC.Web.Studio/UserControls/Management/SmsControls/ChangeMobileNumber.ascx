<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChangeMobileNumber.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ChangeMobileNumber" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_mobilePhoneChangeDialog" style="display: none;">
    <sc:Container runat="server" ID="_changePhoneContainer">
        <Header>
            <%= Resource.MobilePhoneChangeTitle %>
        </Header>
        <Body>
            <input type="hidden" id="hiddenUserInfoId" value="<%= User.IsMe() ? "" : User.ID.ToString() %>" />

            <div id="changeMobileContent">
                <%= User.IsMe()
                        ? (StudioSmsNotificationSettings.Enable 
                            ? Resource.MobilePhoneChangeDescriptionSms
                            : Resource.MobilePhoneChangeDescription)
                    : Resource.MobilePhoneEraseDescription %>

                <div class="middle-button-container">
                    <a id="changeMobileSend" class="button blue"><%= User.IsMe() ? Resource.OKButton : Resource.SendButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;"><%= Resource.CancelButton %></a>
                </div>
            </div>

            <div id="changeMobileProgress" style="display: none;">
                <div class="text-medium-describe"><%= Resource.PleaseWaitMessage %></div>
            </div>

            <div id="changeMobileResult" style="display: none;"><%= Resource.MobilePhoneChangeSent %></div>
        </Body>
    </sc:Container>
</div>
