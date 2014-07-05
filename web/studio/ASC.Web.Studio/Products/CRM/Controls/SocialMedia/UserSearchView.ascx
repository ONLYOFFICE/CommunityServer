<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserSearchView.ascx.cs" Inherits="ASC.Web.CRM.Controls.SocialMedia.UserSearchView" EnableViewState="false" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="divSMUserSearchContainer" style="display: none;">
    <sc:Container ID="_ctrlUserSearchContainer" runat="server">
        <Header>
            <%= CRMSocialMediaResource.SearchInSocialMedia %>
        </Header>
        <Body>
            <div id="divModalContent">
                <div id="divSearchContent">
                    <%= CRMSocialMediaResource.SearchText %>
                    <table id="sm_tbl_UserSearchSettings">
                        <tr>
                            <td>
                                <input id="_ctrlSocialMediaSearch" type="text" style="width: 100%;" onkeypress="if (event.keyCode == 13) {ASC.CRM.SocialMedia.FindUsers(); }" />
                            </td>
                            <td style="width: 30px;">
                            </td>
                            <td style="width: 70px;">
                                <input type="button" value="Find" onclick="ASC.CRM.SocialMedia.FindUsers();" style="width: 100%;" />
                            </td>
                        </tr>
                    </table>
                    <div style="margin: 10px 0; width: 100%;">
                        <input type="radio" name="SelectedSocialMedia" id="_ctrlRadioTwitter" checked="checked"
                            value="Twitter" /><label for="_ctrlRadioTwitter">Twitter</label>
                        <input type="radio" name="SelectedSocialMedia" id="_ctrlRadioFacebook" value="Facebook" /><label
                            for="_ctrlRadioFacebook">Facebook</label>
                        <!--<input type="radio" name="SelectedSocialMedia" id="_ctrlRadioLinkedIn" value="LinkedIn" /><label
                            for="_ctrlRadioLinkedIn">LinkedIn</label>-->
                    </div>
                    <div id="divSearchResults">
                    </div>
                </div>
                <div id="divUserContent" style="display: none; width: 100%;">
                </div>
            </div>
            <div id="_ctrlUserSearchViewErrorContainer" class="infoPanel sm_UserSearchView_ErrorDescription"
                style="display: none;">
                <div id="_ctrlUserSearchViewErrorDescription">
                </div>
            </div>
            <div id="divAjaxCloserBox" style="background-color: white; position: fixed; display: none;">
                &nbsp;
            </div>
        </Body>
    </sc:Container>
</div>
