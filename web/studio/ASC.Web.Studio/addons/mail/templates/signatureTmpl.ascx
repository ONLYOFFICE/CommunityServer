<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="composeSignatureTmpl" type="text/x-jquery-tmpl">
  <div class="tlmail_signature" mailbox_id="${mailboxId}"><div>{{html html}}</div></div>
</script>


<script id="manageSignatureTmpl" type="text/x-jquery-tmpl">
    <div class="manage_signature">
        <span class="info"><%: MailScriptResource.SignatureInformationText %></span>
        <label for="useSignatureFlag" class="checkbox use_singature">
            <input type="checkbox" id="useSignatureFlag" name="UseSignature"  {{if isActive}} checked="true" {{/if}}/>
            <span><%: MailScriptResource.UseSingatureLabel %></span>
        </label>
            <div id="SignatureWYSIWYGEditor" class="signature_wysiwyg_editor">
                <textarea id="ckMailSignatureEditor" style ="height:200px;"></textarea>
            </div>
        <div class="buttons">
            <a class="button blue middle ok"><%: MailResource.SaveBtnLabel %></a>
            <a class="button gray middle cancel"><%: MailScriptResource.CancelBtnLabel %></a>
        </div>
    </div>
</script>