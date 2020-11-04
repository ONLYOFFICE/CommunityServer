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

        StudioBlockUIManager.blockUI(wnd, 350, { bindEvents: false });

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