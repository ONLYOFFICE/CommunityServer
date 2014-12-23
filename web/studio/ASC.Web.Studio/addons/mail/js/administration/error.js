/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.administrationError = (function ($) {
    function getErrorHandler(header) {
        return function (e, error) {
            showErrorToastr(header, error);
        };
    }

    function showErrorToastr(header, error) {
        if (!error) return;

        var text = "";
        if (error.length > 1) {
            switch (error[1].hresult) {
            case ASC.Mail.ErrorConstants.COR_E_SECURITY:
                text = MailApiErrorsResource.ErrorAccessDenied;
                break;
            case ASC.Mail.ErrorConstants.COR_E_ARGUMENT:
                text = argumentExceptionProcessing(header);
                break;
            case ASC.Mail.ErrorConstants.COR_E_ARGUMENTOUTOFRANGE:
                text = argumentOutOfRangeProcessing(header);
                break;
            case ASC.Mail.ErrorConstants.COR_E_INVALIDDATA:
                text = invalidDataProcessing(header);
                break;
            case ASC.Mail.ErrorConstants.COR_E_DUPLICATENANE:
                text = duplicateNameProcessing(header);
                break;
            default:
                text = MailApiErrorsResource.ErrorInternalServer;
            }
        }
        else text = error[0];

        window.toastr.error(text);
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
        showErrorToastr: showErrorToastr
    };
})(jQuery);