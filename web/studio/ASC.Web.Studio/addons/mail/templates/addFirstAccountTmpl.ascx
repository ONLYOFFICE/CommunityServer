<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="addFirstAccountTmpl" type="text/x-jquery-tmpl">
    <div id="add-first-account" class="popup">
        <table>
            <tbody>
                <tr>
                    <td class="img_accounts" />
                    <td class="body">
                        <p class="title"><%: MailScriptResource.AddAccountInfoTitle %></p>
                        <p class="info"><%: MailScriptResource.AddAccountInfoText %></p>
                        <p class="question"><%: MailScriptResource.AddAccountInfoQuestion %></p>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="buttons">
            <a class="button blue middle ok" onclick="accountsModal.addBox();"><%: MailScriptResource.AddAccountBtnLabel %></a>
            <a class="button gray middle cancel"><%: MailScriptResource.CancelBtnLabel %></a>
        </div>
    </div>
</script>
