<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="questionBoxTmpl" type="text/x-jquery-tmpl">
    <div>
        <div class="mail-confirmationAction">
            <p class="attentionText">${attentionText}</p>
            <p class="questionText">${questionText}</p>
        </div>
        <div class="buttons">
            <button class="button middle blue remove" type="button"><%=MailResource.DeleteBtnLabel%></button>
            <button class="button middle gray cancel" type="button"><%=MailScriptResource.CancelBtnLabel%></button>
        </div>
    </div>
</script>
