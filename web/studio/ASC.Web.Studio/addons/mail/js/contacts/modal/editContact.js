/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/

window.editContactModal = (function ($) {
    var isNewContact;

    function show(contact, isNew) {
        var html = $.tmpl('editContactTmpl', { contact: contact, isNew: isNew });

        html.find('.contactDescription').unbind('keyup').bind('keyup', function() {
            if (this.value.length > 100)
                this.value = this.value.substr(0, 100);
        });

        html.find('.addEmail').unbind('click').bind('click', addEmail);
        html.find('.contactEmails .delete_entity').unbind('click').bind('click', deleteEmail);
        html.find('.addPhone').unbind('click').bind('click', addPhone);
        html.find('.contactPhones .delete_entity').unbind('click').bind('click', deletePhone);
        html.find('.save').unbind('click').bind('click', function() { saveContact(isNew); });
        html.find('.cancel').unbind('click').bind('click', window.PopupKeyUpActionProvider.CloseDialog);
        html.find('.contactEmail').off("keyup").keyup(function (e) {
            if (e.which == 13) {
                addEmail();
            } else {
                TMMail.setRequiredError('contact-add-emails', false);
            }
        });
        html.find('.contactEmail').off("blur").blur(function (e) {
            var contactEmail = $('#mail-contact-edit').find('.contactEmail').val();
            var emailCollection = getEmailList();
            if (contactEmail.length !== 0) {
                emailValidation(contactEmail, emailCollection);
            }
        });
        html.find('.contactPhone').off("keyup").keyup(function (e) {
            if (e.which == 13) {
                addPhone();
            } else {
                TMMail.setRequiredError('contact-add-phones', false);
            }
        });

        isNewContact = isNew;

        var header;
        if (isNew) {
            header = MailScriptResource.CreateNewContactLabel;
        } else if (contact.type === 'auto') {
            header = MailResource.AddToContacts;
        } else {
            header = MailScriptResource.EditContactcLabel;
        }

        popup.addPopup(header, html, 392);
        updateEmailList();
    }

    function emailValidation(contactEmail, emailCollection) {
        var errorExists = false;

        if (contactEmail.length === 0) {
            TMMail.setRequiredHint('contact-add-emails', window.MailScriptResource.ErrorEmptyField);
            errorExists = true;
        } else if (!ASC.Mail.Utility.IsValidEmail(contactEmail)) {
            TMMail.setRequiredHint("contact-add-emails", window.MailScriptResource.ErrorIncorrectEmail);
            errorExists = true;
        } else {
            if (emailCollection.indexOf(contactEmail) != -1) {
                TMMail.setRequiredHint('contact-add-emails', window.MailResource.ErrorEmailExist);
                errorExists = true;
            }
        }

        if (errorExists) {
            TMMail.setRequiredError('contact-add-emails', true);
            return false;
        }
        return true;
    }

    function addEmail() {
        var contactEmail = $('#mail-contact-edit').find('.contactEmail').val();
        var emailCollection = getEmailList();

        if (!emailValidation(contactEmail, emailCollection)) return;

        TMMail.setRequiredError('contact-add-emails', false);

        var email = {
            value: contactEmail,
            isPrimary: false,
            id: -1
        };

        if (emailCollection.count == 0)
            email.isPrimary = true;

        var html = $.tmpl('contactDataTableRowTmpl', email);
        var $html = $(html);
        $html.find('.delete_entity').unbind('click').bind('click', deleteEmail);
        $('#mail-contact-edit').find('.contactEmails').show();
        $('#mail-contact-edit').find('.contactEmails table').append(html);
        $('#mail-contact-edit').find('.contactEmail').val('');
        $('#mail-contact-edit').find('.contactEmail').focus();
        updateEmailList();
    }

    function deleteEmail() {
        var row = $(this).closest('.dataRow');
        row.remove();
        updateEmailList();
        $('#mail-contact-edit').find('.contactEmail').focus();
    }

    function addPhone() {
        var contactPhone = $('#mail-contact-edit').find('.contactPhone').val();
        var phoneCollection = getPhoneList();
        var errorExists = false;

        if (contactPhone.length === 0) {
            return;
        } else {
            if (phoneCollection.indexOf(contactPhone) != -1) {
                TMMail.setRequiredHint('contact-add-phones', window.MailScriptResource.ErrorPhoneExist);
                errorExists = true;
            }
        }

        if (errorExists) {
            TMMail.setRequiredError('contact-add-phones', true);
            return;
        }

        TMMail.setRequiredError('contact-add-phones', false);

        var phone = {
            value: contactPhone,
            isPrimary: false,
            id: -1
        };

        if (phoneCollection.count == 0)
            phone.isPrimary = true;

        var html = $.tmpl('contactDataTableRowTmpl', phone);
        var $html = $(html);
        $html.find('.delete_entity').unbind('click').bind('click', deletePhone);
        $('#mail-contact-edit').find('.contactPhones').show();
        $('#mail-contact-edit').find('.contactPhones table').append(html);
        $('#mail-contact-edit').find('.contactPhone').val('');
        $('#mail-contact-edit').find('.contactPhone').focus();
    }

    function deletePhone() {
        var row = $(this).closest('.dataRow');
        row.remove();
        var rows = $('#mail-contact-edit').find('.contactPhones dataRow');
        if (rows.length)
            $('#mail-contact-edit').find('.contactPhones').hide();
        $('#mail-contact-edit').find('.contactPhone').focus();
    }

    function saveContact() {
        TMMail.setRequiredHint('contact-add-emails', '');
        TMMail.setRequiredError('contact-add-emails', false);
        TMMail.setRequiredHint('contact-add-phones', '');
        TMMail.setRequiredError('contact-add-phones', false);

        var name = $('#mail-contact-edit').find('.contactName').val();
        var desctiption = $('#mail-contact-edit').find('.contactDescription').val();
        var email = $('#mail-contact-edit').find('.contactEmail').val();
        var phoneNumber = $('#mail-contact-edit').find('.contactPhone').val();
        var emailCollection = getEmailList();
        var phoneCollection = getPhoneList();

        if ((email != "" || emailCollection.length==0) && !emailValidation(email, emailCollection)) return;

        if (email != "") {
            emailCollection.push(email);
        }

        if (phoneNumber != undefined && phoneNumber != "") {
            if (phoneCollection.indexOf( phoneNumber) == -1) {
                phoneCollection.push(phoneNumber);
            } else {
                TMMail.setRequiredHint('contact-add-phones', window.MailScriptResource.ErrorPhoneExist);
                TMMail.setRequiredError('contact-add-phones', true);
                return;
            }
        }

        if (isNewContact) {
            for (var i = 0, n = emailCollection.length; i < n; i++) {
                window.contactsManager.forgetPersonalContact(emailCollection[i]);
            }
            serviceManager.createMailContact(name, desctiption, emailCollection, phoneCollection, {},
                {
                    error: function(e, error) {
                        window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);
        } else {
            var id = $('#mail-contact-edit').attr('data_id');
            serviceManager.updateMailContact(id, name, desctiption, emailCollection, phoneCollection, {},
                {
                    error: function (e, error) {
                        window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);
        }

        window.PopupKeyUpActionProvider.CloseDialog();
    }

    function getPhoneList() {
        var phoneCollection = [];
        var phoneRows = $('#mail-contact-edit').find('.contactPhones .value');
        $.each(phoneRows, function (index, value) {
            phoneCollection.push($(value).text());
        });
        return phoneCollection;
    }

    function getEmailList() {
        var emailCollection = [];
        var emailRows = $('#mail-contact-edit').find('.contactEmails .value');
        $.each(emailRows, function (index, value) {
            emailCollection.push($(value).text());
        });
        return emailCollection;
    }

    function updateEmailList() {
        var emailRows = $('#mail-contact-edit .contactEmails').find('.dataRow');
        if (emailRows.length == 1) {
            $(emailRows[0]).find('.delete_entity').hide();
        } else if (emailRows.length >= 2) {
            for (var i = 0; i < emailRows.length; i++) {
                $(emailRows[i]).find('.delete_entity').show();
            }
        }
    }

    return {
        show: show
    };
})(jQuery);