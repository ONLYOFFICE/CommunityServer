<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="editContactTmpl" type="text/x-jquery-tmpl">
    <div id="mail-contact-edit" {{if contact}} data_id="${contact.id}" {{else}} data_id="-1" {{/if}}>
        <table>
            <tbody>
                <tr>
                    <td>
                        <div id="block-сontact-name" class="contactNmeBlock">
                            <div class="headerPanelSmall bold"><%= MailResource.NameLabel %></div>
                            <input type="text" class="contactName textEdit" maxlength="255" {{if contact}} value="${contact.name}" {{/if}} />
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="contact-add-emails" class="contactAddlBlock requiredField">
                            <span class="requiredErrorText"><%=MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall bold"><%= MailResource.EMailLabel %></div>
                            <input type="email" class="contactEmail textEdit" maxlength="255"
                                {{if isNew && contact}} value="${contact.emails[0].value}" {{/if}} />
                            <a class="plus plusmail addEmail" title="<%= MailAdministrationResource.AddButton %>" style="float: none;"></a>
                            <div class="contactEmails" {{if isNew}} style="display: none;" {{/if}}>
                                <table>
                                    {{if !isNew}}
                                        {{tmpl(contact.emails) "contactDataTableRowTmpl"}}
                                    {{/if}}
                                </table>
                            </div>
                        </div>

                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="contact-add-phones" class="contactAddlBlock requiredField">
                            <span class="requiredErrorText"><%=MailScriptResource.ErrorEmptyField %></span>
                            <div class="headerPanelSmall bold"><%= MailResource.PhoneLabel %></div>
                            <input type="tel" class="contactPhone textEdit" maxlength="255"/>
                            <a class="plus plusmail addPhone" title="<%= MailAdministrationResource.AddButton %>" style="float: none;"></a>
                            <div class="contactPhones" {{if !contact || !contact.phones.length}} style="display: none;" {{/if}}>
                                <table>
                                    {{if contact}}
                                        {{tmpl(contact.phones) "contactDataTableRowTmpl"}}
                                    {{/if}}
                                </table>
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="contact-description">
                            <div class="headerPanelSmall bold"><%= MailResource.DescriptionLabel %></div>
                            {{if contact}}
                                <textarea class="contactDescription textEdit">${contact.description}</textarea>
                            {{else}}
                                <textarea class="contactDescription textEdit"></textarea>
                            {{/if}}
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="buttons">
            <button class="button middle blue save" type="button"><%= MailResource.SaveBtnLabel %></button>
            <button class="button middle gray cancel" type="button"><%= MailScriptResource.CancelBtnLabel %></button>
        </div>
    </div>
</script>>

<script id="contactDataTableRowTmpl" type="text/x-jquery-tmpl">
    <tr class='dataRow' data_id="${id}">
        <td>
            <span class="value">${value}</span>
            <div class="delete_entity"></div>
        </td>
    </tr>
</script>