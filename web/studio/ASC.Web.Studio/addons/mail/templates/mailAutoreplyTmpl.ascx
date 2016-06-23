<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Core.Helpers" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Import Namespace="Resources" %>

<script id="mailAutoreplyTmpl" type="text/x-jquery-tmpl">
    <div class="mail_autoreply">
        <% var autoreplyDaysInterval = Convert.ToInt32(ConfigurationManager.AppSettings["mail.autoreply-days-interval"] ?? "4"); %>
        <span class="info"><%: String.Format(MailResource.AutoreplyInformationText, String.Format(GrammaticalHelper.ChooseNumeralCase(
                               autoreplyDaysInterval, Resource.DrnAgoDaysI, Resource.DrnAgoDaysR1, Resource.DrnAgoDaysRm),
                               autoreplyDaysInterval)) %></span>
        <label for="turnAutoreplyFlag" class="checkbox turn_autoreply">
            <input type="checkbox" id="turnAutoreplyFlag" name="turnAutoreply" {{if turnOn}} checked {{/if}}/>
            <span><%: MailResource.TurnAutoreplyOn %></span>
        </label>
        <label for="onlyContactsFlag" class="checkbox only_contacts">
            <input type="checkbox" id="onlyContactsFlag" name="onlyContacts" {{if onlyContacts}} checked {{/if}}/>
            <span><%: MailResource.OnlySendAutoreplyToContacts %></span>
        </label>
        <div class="mail_autoreply_from_to_date">
            <div class="mail_autoreply_from_date">
                <div class="bold"><%: MailScriptResource.MailAutoreplyFromDateLabel %></div>
                <input type="text" id="autoreplyStartDate" class="textEditCalendar" autocomplete="off"/>
            </div>
            <div class="mail_autoreply_to_date">
                <div class="mail_autoreply_to_date_bold bold">
                    <label for="turnOnToDateFlag" class="checkbox">
                        <input type="checkbox" id="turnOnToDateFlag" name="turnOnToDate" {{if turnOnToDate}} checked {{/if}}/>
                        <span><%: MailScriptResource.MailAutoreplyToDateLabel %></span>
                    </label>
                </div>
                <input type="text" id="autoreplyDueDate" class="textEditCalendar" autocomplete="off"/>
            </div>
        </div>
        <div class="mail_autoreply_subject">
            <div class="bold"><%: MailScriptResource.SubjectLabel %></div>
            <input type="text" class="textEdit" id="autoreplySubject" value="${subject}"/>
        </div>
        <div class="mail_autoreply_body requiredField">
            <span class="requiredErrorText"><%: MailResource.ErrorEmptyBody %></span>
            <div class="bold headerPanelSmall"><%: MailResource.BodyLabel %></div>
            <div id="MailAutoreplyWYSIWYGEditor" class="mail_autoreply_wysiwyg_editor">
                <textarea id="ckMailAutoreplyEditor" style="height:200px;"></textarea>
            </div>
        </div>
        <div class="buttons">
            <a class="button blue middle ok"><%: MailResource.SaveBtnLabel %></a>
            <a class="button gray middle cancel"><%: MailScriptResource.CancelBtnLabel %></a>
        </div>
    </div>
</script>