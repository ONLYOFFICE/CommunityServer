/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.administrationError = (function() {

    function getErrorHandler(header) {
        return function(e, error) {
            showErrorToastr(header, error);
        };
    }

    function showErrorToastr(header, error) {
        if (!error) {
            return;
        }

        var text;
        if (error.length > 1) {
            text = getErrorText(header, error);
        } else {
            text = error[0];
        }

        window.toastr.error(text);
    }

    function getErrorText(header, error) {
        if (!error || !error.length || error.length == 1) {
            return null;
        }

        switch (error[1].hresult) {
            case ASC.Mail.ErrorConstants.COR_E_SECURITY:
                return MailApiErrorsResource.ErrorAccessDenied;
            case ASC.Mail.ErrorConstants.COR_E_ARGUMENT:
                return argumentExceptionProcessing(header);
            case ASC.Mail.ErrorConstants.COR_E_ARGUMENTOUTOFRANGE:
                return argumentOutOfRangeProcessing(header);
            case ASC.Mail.ErrorConstants.COR_E_INVALIDDATA:
                return invalidDataProcessing(header);
            case ASC.Mail.ErrorConstants.COR_E_DUPLICATENANE:
                return duplicateNameProcessing(header);
            case ASC.Mail.ErrorConstants.COR_E_INVALIDOPERATION:
                return invalidOperationProcessing(header);
            default:
                return MailApiErrorsResource.ErrorInternalServer;
        }
    }

    function invalidDataProcessing(header) {
        var text;
        switch (header) {
            case "addMailDomain":
                text = MailApiErrorsResource.ErrorDkimPublicKeyInUse;
                break;
            case "addMailbox":
                text = MailApiErrorsResource.ErrorUnknownUser;
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
        }
        return text;
    }

    function invalidOperationProcessing(header) {
        var text;
        switch (header) {
            case "createMailGroup":
            case "addMailboxAlias":
                text = MailApiErrorsResource.InvalidOpearationForSharedDomain;
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
        }
        return text;
    }

    function duplicateNameProcessing(header) {
        var text;
        switch (header) {
            case "addMailDomain":
                text = MailApiErrorsResource.ErrorDuplicateDomain;
                break;
            case "addMailboxAlias":
                text = MailApiErrorsResource.ErrorDuplicateAlias;
                break;
            case "addMailbox":
                text = MailApiErrorsResource.ErrorDuplicateMailbox;
                break;
            case "addMailGroupAddress":
                text = MailApiErrorsResource.ErrorDuplicateMailGroupAddress;
                break;
            case "addMailGroup":
                text = MailApiErrorsResource.ErrorDuplicateGroup;
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
        }
        return text;
    }

    function argumentOutOfRangeProcessing(header) {
        var text;
        switch (header) {
            case "addMailbox":
                text = MailApiErrorsResource.ErrorMailboxLimit;
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
        }
        return text;
    }

    function argumentExceptionProcessing(header) {
        var text;
        switch (header) {
            case "addMailDomain":
            case "checkDomainExistance":
            case "checkDomainOwnership":
                text = MailApiErrorsResource.ErrorInvalidDomain;
                break;
            case "addMailbox":
                text = MailApiErrorsResource.ErrorInvalidMailbox;
                break;
            case "addMailboxAlias":
                text = MailApiErrorsResource.ErrorInvalidAlias;
                break;
            case "addMailGroupAddress":
                text = MailApiErrorsResource.ErrorInvalidAddress;
                break;
            case "addMailGroup":
                text = MailApiErrorsResource.ErrorInvalidMailGroup;
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
        }
        return text;
    }

    return {
        getErrorHandler: getErrorHandler,
        getErrorText: getErrorText,
        showErrorToastr: showErrorToastr
    };
})(jQuery);