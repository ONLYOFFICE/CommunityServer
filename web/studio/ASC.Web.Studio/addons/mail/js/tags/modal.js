/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

    function init() {
        if (isInit)
            return;

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

        tagsManager.bind(tagsManager.events.OnDelete, onTagsChange);
        tagsManager.bind(tagsManager.events.OnCreate, onTagsChange);
        tagsManager.bind(tagsManager.events.OnUpdate, onTagsChange);
        tagsManager.bind(tagsManager.events.OnError, onTagsError);
    }

    function onTagsChange() {
        hide();
    }

    function prepareOptions(opts) {
        if (!opts || typeof opts !== "object") {
            opts = {};
        }

        var options = {};

        options.onSuccess = !opts.hasOwnProperty("onSuccess") || typeof (opts.onSuccess) !== "function"
            ? function() { console.log("tag default success callback", arguments); }
            : opts.onSuccess;

        options.onError = !opts.hasOwnProperty("onError") || typeof (opts.onError) !== "function"
            ? function () { console.log("tag default error callback", arguments); }
            : opts.onError;

        return options;
    }

    function showCreate(opts) {
        var options = prepareOptions(opts);
        return show({ id: 0, name: '', addresses: {}, style: tagsManager.getVacantStyle() }, 'new', options);
    }

    function showEdit(tag, opts) {
        var options = prepareOptions(opts);
        return show(tag, 'edit', options);
    }

    function showDelete(tag, opts) {
        var options = prepareOptions(opts);
        return show(tag, 'delete', options);
    }

    function show(tag, type, options) {
        init();

        var headerText;

        if (type === 'delete') {
            wnd.find('.del').show();
            wnd.find('.save').hide();
            var text = MailScriptResource.DeleteTagShure;
            wnd.find('#deleteTagShure').text(text.replace(/%1/g, tag.name));
            headerText = wnd.attr('deletetag');
        } else {
            wnd.find('.del').hide();
            wnd.find('.save').show();

            if (type === 'new') {
                headerText = wnd.attr('newtag');
            } else {
                headerText = wnd.attr('savetag');
            }

            if (!tag.addresses.length)
                wnd.find('#mail_EmailsContainer').hide();
        }

        changeWndTagColor(undefined, tag.style);

        wnd.find('div.containerHeaderBlock:first td:first').html(headerText);
        wnd.find('#mail_CreateTag_Name input').val(tag.name);
        wnd.find('#mail_EmailsContainer').empty();
        wnd.attr('tagid', tag.id);

        var saveBtn = wnd.find('.buttons .save');

        saveBtn.unbind('click')
            .bind('click',
                function() {
                    if (saveBtn.attr('disabled') || saveBtn.hasClass('disable')) {
                        return false;
                    }

                    if (wnd.find('#mail_tag_email').val()) {
                        wnd.find("#mail_CreateTag_Email .plusmail").trigger("click");
                        return false;
                    }

                    TMMail.setRequiredError('mail_CreateTag_Email', false);
                    TMMail.setRequiredError('mail_CreateTag_Name', false);

                    tag = getTagFromWnd();
                    if (tag.name.length === 0) {
                        TMMail.setRequiredHint('mail_CreateTag_Name', MailScriptResource.ErrorEmptyField);
                        TMMail.setRequiredError('mail_CreateTag_Name', true);

                        wnd.find('#mail_tag_name').focus();
                        return false;
                    }

                    showLoader(MailScriptResource.TagCreation);

                    if ('new' === type) {
                        tagsManager.createTag(tag)
                            .then(function(tag) {
                                    hide();
                                    options.onSuccess(tag);
                                },
                                options.onError);
                    } else {
                        var currentValue = tagsManager.getTag(tag.id);
                        tag.lettersCount = currentValue.lettersCount;

                        tagsManager.updateTag(tag)
                            .then(function(tag) {
                                    hide();
                                    options.onSuccess(tag);
                                },
                                options.onError);
                    }

                    return true;
                });

        wnd.find('input.addemail')
            .EmailsSelector("init",
            {
                isInPopup: true,
                items: tag.addresses,
                container: wnd.find("#mail_EmailsContainer")
            });

        var defaultOptions = {
            css: {
                marginLeft: '-167px',
                marginTop: '-135px'
            },
            bindEvents: false
        }

        StudioBlockUIManager.blockUI(wnd, 350, null, null, null, defaultOptions);

        wnd = $('#tagWnd');

        TMMail.setRequiredError('mail_CreateTag_Email', false);
        TMMail.setRequiredError('mail_CreateTag_Name', false);

        window.PopupKeyUpActionProvider.EnterAction = "jq('#tagWnd .containerBodyBlock .buttons .save').click();";
    }

    function showLoader(message) {
        wnd.find('.progressContainer').show();
        wnd.find('.progressContainer .loader').show().html(message || '');
        wnd.find('.save #mail_tag_name').attr('disabled', 'true');
        wnd.find('.save .tag.color .outer .inner').attr('disabled', 'true').css('cursor', 'default');
        wnd.find('.save .tag.color').css('cursor', 'default');
        wnd.find('.linked_addresses.save #mail_tag_email').attr('disabled', 'true');
        wnd.find('.linked_addresses a.plusmail').attr('disabled', 'true').css('cursor', 'default');
        wnd.find('.removeTagAddress').attr('disabled', 'true').css('cursor', 'default');
        wnd.find('.buttons .save').attr('disabled', 'true').removeClass("disable").addClass("disable");
        wnd.find('.buttons .cancel').attr('disabled', 'true').removeClass("disable").addClass("disable");
    }

    function hideLoader() {
        wnd.find('.progressContainer').hide();
        wnd.find('.save #mail_tag_name').removeAttr('disabled');
        wnd.find('.save .tag.color .outer .inner').removeAttr('disabled').css('cursor', 'pointer');
        wnd.find('.save .tag.color').css('cursor', 'pointer');
        wnd.find('.linked_addresses.save #mail_tag_email').removeAttr('disabled');
        wnd.find('.linked_addresses a.plusmail').removeAttr("disabled").css('cursor', 'pointer');
        wnd.find('.removeTagAddress').removeAttr("disabled").css('cursor', 'pointer');
        wnd.find('.buttons .save').removeAttr('disabled').removeClass("disable");
        wnd.find('.buttons .cancel').removeAttr('disabled').removeClass("disable");
    }

    function changeWndTagColor(obj, newStyle) {
        wnd.find('a.square').removeClass().addClass('square tag' + newStyle);
        wnd.find('.tag.color .inner').removeClass().addClass('inner tag' + newStyle);
        wnd.find('.tag.color .inner').attr('colorstyle', newStyle);
    }

    function deleteTag() {
        var tag = getTagFromWnd();
        tagsManager.deleteTag(tag.id);
    }

    function hide(error) {
        wnd.find('.buttons .save').unbind('click');

        if (typeof error === 'object') {
            onTagsError(null, error);
            return;
        } else {
            hideLoader();
        }

        tagsColorsPopup.hide();

        if (wnd.is(':visible')) {
            $.unblockUI();
        }
    }

    function getTagFromWnd() {
        var id = wnd.attr('tagid'),
            style = wnd.find('.tag.color .inner').attr('colorstyle'),
            name = $.trim(wnd.find('#mail_tag_name').val()),
            addresses = wnd.find('input.addemail').EmailsSelector("get");

        return {
            id: id,
            name: name,
            style: style,
            addresses: jq.map(addresses, function(a) {
                return a.email;
            })
        };
    }

    function onTagsError(e, error) {
        setErrorMessage(error.message + (error.comment ? ': ' + error.comment : ''));
        wnd.find('.removeTagAddress').removeAttr("disabled").css('cursor', 'pointer');
    }

    function setErrorMessage(errorMessage) {
        hideLoader();
        window.toastr.error(errorMessage);
    }

    return {
        init: init,
        hide: hide,
        showCreate: showCreate,
        showDelete: showDelete,
        showEdit: showEdit
    };
})(jQuery);