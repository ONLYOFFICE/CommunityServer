/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
            case ASC.Mail.Constants.Errors.COR_E_SECURITY:
                return error[1].message === "Not available in unpaid version"
                    ? MailApiErrorsResource.ErrorUnpaidAccessDenied
                    : MailApiErrorsResource.ErrorAccessDenied;
            case ASC.Mail.Constants.Errors.COR_E_ARGUMENT:
                return argumentExceptionProcessing(header);
            case ASC.Mail.Constants.Errors.COR_E_ARGUMENTOUTOFRANGE:
                return argumentOutOfRangeProcessing(header);
            case ASC.Mail.Constants.Errors.COR_E_INVALIDDATA:
                return invalidDataProcessing(header);
            case ASC.Mail.Constants.Errors.COR_E_DUPLICATENANE:
                return duplicateNameProcessing(header);
            case ASC.Mail.Constants.Errors.COR_E_INVALIDOPERATION:
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
            case "addMailDomain":
                text = MailAdministrationResource.DomainWizardDomainNotVerified.format(window.createDomainModal.getCurrentDomainName());
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
            case "updateMailbox":
                text = MailApiErrorsResource.ErrorDuplicateAlias;
                break;
            case "addMailbox":
                text = MailApiErrorsResource.ErrorDuplicateMailbox;
                break;
            case "updateMailgroup":
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
            case "updateMailbox":
                text = MailApiErrorsResource.ErrorInvalidAlias;
                break;
            case "updateMailgroup":
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