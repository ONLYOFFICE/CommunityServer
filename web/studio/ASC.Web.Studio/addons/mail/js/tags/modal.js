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


window.tagsModal = (function($) {
    var isInit = false,
        wnd;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            wnd = $('#tagWnd');

            wnd.find('.tag.color .outer .inner').unbind('click').bind('click', function(e) {
                if ($('.tag.color .outer .inner').attr('disabled')) {
                    return false;
                }

                tagsColorsPopup.show(this, changeWndTagColor);
                e.stopPropagation();
                return false;
            });

            wnd.find('.linked_addresses a.plusmail').unbind('click').bind('click', function() {
                if ($('.linked_addresses a.plusmail').attr('disabled')) {
                    return false;
                }
                addAddressHandler(this);
                return false;
            });

            wnd.find('input.addemail').emailAutocomplete({ emailOnly: true });

            wnd.find('.buttons .del').unbind('click').bind('click', function() {
                if ($('.buttons .del').attr('disabled')) {
                    return false;
                }
                hide();
                deleteTag();
                return false;
            });

            wnd.find('.buttons .cancel').unbind('click').bind('click', function() {
                if ($('.buttons .cancel').attr('disabled')) {
                    return false;
                }
                hide();
                return false;
            });

            tagsManager.events.bind('delete', onTagsChange);
            tagsManager.events.bind('create', onTagsChange);
            tagsManager.events.bind('update', onTagsChange);
            tagsManager.events.bind('error', onTagsError);

            $(document).keyup(function(e) {
                if (e.which == 13) {
                    if ($('#tagWnd').is(':visible')) {
                        if ($('#tagWnd input.addemail').is(':focus')) {
                            addAddressHandler(this);
                        } else {
                            $('#tagWnd .containerBodyBlock .buttons .button.blue:visible').trigger('click');
                        }

                    }
                }
            });
        }
    };

    var onTagsChange = function() {
        hide();
    };

    var showCreate = function() {
        return show({ id: 0, name: '', addresses: {}, style: tagsManager.getVacantStyle() }, 'new');
    };

    var showDelete = function(tag) {
        return show(tag, 'delete');
    };

    var showEdit = function(tag) {
        return show(tag, 'edit');
    };

    var show = function(tag, type) {
        if (type === 'delete') {
            wnd.find('.del').show();
            wnd.find('.save').hide();
            var text = MailScriptResource.DeleteTagShure;
            wnd.find('#deleteTagShure').text(text.replace(/%1/g, tag.name));
        } else {
            wnd.find('.del').hide();
            wnd.find('.save').show();
        }

        var headerText;
        if (type === 'delete') {
            headerText = wnd.attr('deletetag');
        } else if (type === 'new') {
            headerText = wnd.attr('newtag');
        } else {
            headerText = wnd.attr('savetag');
        }
        wnd.find('div.containerHeaderBlock:first td:first').html(headerText);


        wnd.find('#mail_CreateTag_Name input').val(tag.name);
        changeWndTagColor(undefined, tag.style);
        wnd.find('#mail_EmailsContainer').empty();

        if (type != 'delete' && tag.addresses.length > 0) {
            wnd.find('.tagEditEmailList').show();
            $.each(tag.addresses, function(i) {
                addAddressHtml(tag.addresses[i]);
            });
        } else {
            wnd.find('.tagEditEmailList').hide();
        }

        wnd.attr('tagid', tag.id);

        wnd.find('.buttons .save').unbind('click').bind('click', function() {
            if (wnd.find('.buttons .save').attr('disabled') || wnd.find('.buttons .save').hasClass('disable')) {
                return false;
            }

            TMMail.setRequiredError('mail_CreateTag_Email', false);
            TMMail.setRequiredError('mail_CreateTag_Name', false);

            if (wnd.find('input.addemail').val() && !wnd.find('input.addemail').hasClass('placeholder')) {
                if (!addAddressHandler(this)) {
                    return false;
                }
            }

            tag = getTagFromWnd();
            if (tag.name.length == 0) {
                TMMail.setRequiredHint('mail_CreateTag_Name', MailScriptResource.ErrorEmptyField);
                TMMail.setRequiredError('mail_CreateTag_Name', true);
                return false;
            }

            showLoader(MailScriptResource.TagCreation);

            if ('new' === type) {
                createTag(tag);
            } else {
                updateTag(tag);
            }
        });

        wnd.find('input.addemail').val('');

        wnd.find('.addemail_error').hide();
        TMMail.setRequiredError('mail_CreateTag_Name', false);
        TMMail.setRequiredError('mail_CreateTag_Email', false);

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';

        wnd.find('input[placeholder]').placeholder();

        jq.blockUI({
            message: wnd,
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '350px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-175px',
                'margin-top': margintop,
                'background-color': 'White'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: true,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {
            }
        });
    };

    var showLoader = function(message) {
        $('#tagWnd').find('.addemail_error').hide();
        $('#tagWnd').find('.progressContainer').show();
        $('#tagWnd').find('.progressContainer .loader').show().html(message || '');
        $('#tagWnd .save #mail_tag_name').attr('disabled', 'true');
        $('#tagWnd .save .tag.color .outer .inner').attr('disabled', 'true').css('cursor', 'default');
        $('#tagWnd .save .tag.color').css('cursor', 'default');
        $('#tagWnd .linked_addresses.save #mail_tag_email').attr('disabled', 'true');
        $('#tagWnd .linked_addresses a.plusmail').attr('disabled', 'true').css('cursor', 'default');
        $('#tagWnd .delete_tag_address').attr('disabled', 'true').css('cursor', 'default');
        $('#tagWnd .buttons .save').attr('disabled', 'true').removeClass("disable").addClass("disable");
        $('#tagWnd .buttons .cancel').attr('disabled', 'true').removeClass("disable").addClass("disable");
    };

    var hideLoader = function() {
        $('#tagWnd').find('.progressContainer').hide();
        $('#tagWnd .save #mail_tag_name').removeAttr('disabled');
        $('#tagWnd .save .tag.color .outer .inner').removeAttr('disabled').css('cursor', 'pointer');
        $('#tagWnd .save .tag.color').css('cursor', 'pointer');
        $('#tagWnd .linked_addresses.save #mail_tag_email').removeAttr('disabled');
        $('#tagWnd .linked_addresses a.plusmail').removeAttr("disabled").css('cursor', 'pointer');
        $('#tagWnd .delete_tag_address').removeAttr("disabled").css('cursor', 'pointer');
        $('#tagWnd .buttons .save').removeAttr('disabled').removeClass("disable");
        $('#tagWnd .buttons .cancel').removeAttr('disabled').removeClass("disable");
    };

    var changeWndTagColor = function(obj, newStyle) {
        wnd.find('a.square').removeClass().addClass('square tag' + newStyle);
        wnd.find('.tag.color .inner').removeClass().addClass('inner tag' + newStyle);
        wnd.find('.tag.color .inner').attr('colorstyle', newStyle);
    };

    var updateTag = function(tag) {
        var currentValue = tagsManager.getTag(tag.id);
        tag.lettersCount = currentValue.lettersCount;
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.tagsManagement, ga_Actions.update, "update_tag");
        tagsManager.updateTag(tag);
    };

    var createTag = function(tag) {
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.tagsManagement, ga_Actions.createNew, "create_new_tag");
        tagsManager.createTag(tag);
    };

    var deleteTag = function() {
        var tag = getTagFromWnd();
        tagsManager.deleteTag(tag.id);
    };

    var hide = function(error) {
        wnd.find('.buttons .save').unbind('click');

        if (typeof error === 'object') {
            return onTagsError(null, error);
        } else {
            hideLoader();
        }

        tagsColorsPopup.hide();

        if (wnd.is(':visible')) {
            $.unblockUI();
        }
    };

    var addAddressHandler = function() {
        var address = wnd.find('input.addemail').val();

        //check on errors
        if (address == undefined || address.length == 0) {
            TMMail.setRequiredHint('mail_CreateTag_Email', MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('mail_CreateTag_Email', true);
            return undefined;
        }

        var fromArray = [];
        var $addresses = wnd.find('.linked_address');
        var itmInd = $addresses.length;
        while (itmInd--) {
            var $item = $($addresses[itmInd]);
            fromArray.push($item.html().toLowerCase());
        }

        if (TMMail.in_array(address.toLowerCase(), fromArray)) {
            TMMail.setRequiredHint('mail_CreateTag_Email', MailResource.ErrorEmailExist);
            TMMail.setRequiredError('mail_CreateTag_Email', true);
            return undefined;
        }

        if (!TMMail.reEmailStrict.test(address)) {
            TMMail.setRequiredHint('mail_CreateTag_Email', MailScriptResource.ErrorIncorrectEmail);
            TMMail.setRequiredError('mail_CreateTag_Email', true);
            return undefined;
        }

        wnd.find('.addemail_error').hide();
        TMMail.setRequiredError('mail_CreateTag_Email', false);

        addAddressHtml(address);
        wnd.find('input.addemail').val('');
        return true;
    };

    var addAddressHtml = function(address) {
        if (address) {
            var html = $.tmpl('tagEmailInEditPopupTmpl', { address: address });

            html.find('.delete_tag_address').unbind('.click').bind('click', function() {
                if ($('.delete_tag_address').attr('disabled')) {
                    return false;
                }

                $(this).closest('tr').remove();

                if ($('#mail_EmailsContainer tbody:empty').length > 0) {
                    wnd.find('.tagEditEmailList').hide();
                }

                return false;
            });

            wnd.find('.tagEditEmailList').show();
            wnd.find('#mail_EmailsContainer').append(html);
        }
    };

    var getTagFromWnd = function() {
        var id = wnd.attr('tagid'),
            style = wnd.find('.tag.color .inner').attr('colorstyle'),
            name = $.trim(wnd.find('#mail_tag_name').val()),
            addresses = [];

        wnd.find('.linked_address').each(function(i, v) {
            addresses.push($(v).html());
        });

        return { id: id, name: name, style: style, addresses: addresses };
    };

    var onTagsError = function(e, error) {
        setErrorMessage(error.message + (error.comment ? ': ' + error.comment : ''));
        $('#tagWnd .delete_tag_address').removeAttr("disabled").css('cursor', 'pointer');
    };

    var setErrorMessage = function(errorMessage) {
        hideLoader();
        wnd.find('.addemail_error').show().html(errorMessage);
        wnd.find('.addemail_error').css('display', 'inline-block');
    };

    return {
        init: init,
        hide: hide,
        showCreate: showCreate,
        showDelete: showDelete,
        showEdit: showEdit
    };
})(jQuery);