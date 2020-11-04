<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DeactivatePortal.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DeactivatePortal" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="accountDeactivationBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeactivation%></div>
        <div><%=Resource.DeactivationDesc%></div>
        <div class="middle-button-container">
                <a id="sendDeactivateInstructionsBtn" class="button blue middle"><%=Resource.DeactivateButton%></a>
        </div>
        <p id="deativate_sent" class="display-none"></p>
    </div>
    <div class="settings-help-block">
        <p><%=String.Format(Resource.HelpAnswerAccountDeactivation.HtmlEncode(), "<br />", "<b>", "</b>")%></p>
    </div>
</div>

<div class="clearFix">
    <div id="accountDeletionBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeletion%></div>
        <div><%=Resource.DeletionDesc%></div>
        <div class="middle-button-container">
            <a id="showDeleteDialogBtn" class="button blue middle"><%=Resource.DeleteButton%></a>
        </div>
        <p id="delete_sent" class="display-none"></p>
    </div>

    <% if (ShowAutoRenew)
       { %>
    <div id="deleteDialog" class="display-none">
        <div class="popupContainerClass">
            <div class="containerHeaderBlock">
                <table cellspacing="0" cellpadding="0" border="0" style="width: 100%; height: 0px;">
                    <tbody>
                        <tr valign="top">
                            <td><%= Resource.AccountDeletion %></td>
                            <td class="popupCancel">
                                <div class="cancelButton" onclick="PopupKeyUpActionProvider.CloseDialog();">&times;</div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="containerBodyBlock">
                <table>
                    <tbody>
                        <tr style="vertical-align: top;">
                            <td>
                                <div class="exclamation"></div>
                            </td>
                            <td style="padding-left: 8px;">
                                <%= String.Format(Resource.AccountDeactivationConfirmMsg, "<a href=\"https://secure.avangate.com/myaccount\" target=\"_blank\">", "</a>") %>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div class="middle-button-container">
                    <a class="button blue middle" id="sendDeleteInstructionsBtn">
                        <%= Resource.DeleteButton %>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a onclick="PopupKeyUpActionProvider.CloseDialog();" class="button gray middle">
                        <%= Resource.CancelButton %>
                    </a>
                </div>
            </div>
        </div>
    </div>
    <% } %>

</div>