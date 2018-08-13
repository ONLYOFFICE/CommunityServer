/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.tagsDropdown = (function($) {
    var popup;
    var popupId = '#addTagsPanel';
    var isInit = false;
    var arrow;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            popup = $(popupId);

            bindInputs();

            tagsManager.events.bind('delete', onDeleteTag);
            tagsManager.events.bind('create', onCreateTag);
        }
    };

    var onDeleteTag = function() {
        hide();
    };

    var onCreateTag = function(e, tag) {
        if (popup.is(":visible")) {
            setTag(tag.id);
        }
        hide();
    };

    // shows add tags panel near obj element depending on current page
    var showAddTagsPanel = function(obj) {
        if ($(popupId + ':visible').length) {
            hide();
            return;
        }

        var tagContent = getTagContent();

        $('#tagsPanelContent .existsTags').html(tagContent);

        var tagNameInput = $(popupId + ' #createnewtag');
        tagNameInput.val('');
        tagNameInput.unbind('focus').bind('focus', function() {
            TMMail.setRequiredError('addTagsPanel', false);
        });

        var $tag = $(popupId + ' .tag.inactive');
        $tag.bind('click', function() {
            setTag($(this).attr('tag_id'));
        });

        $tag = $(popupId + ' .tag.tagArrow');
        $tag.bind('click', function() {
            mailBox.unsetConversationsTag($(this).attr('tag_id'));
        });

        if ((TMMail.pageIs('message') || TMMail.pageIs('conversation')) &&
            messagePage.getMessageFolder(messagePage.getActualConversationLastMessageId()) != TMMail.sysfolders.trash.id) {
            showMarkRecipientsCheckbox();
        } else {
            hideMarkRecipientsCheckbox();
        }

        setTagColor(undefined, tagsManager.getVacantStyle());

        popup.find('.square').click(function() {
            tagsColorsPopup.show(this, setTagColor);
        });

        TMMail.setRequiredError('addTagsPanel', false);

        arrow = $(obj).find('.down_arrow').offset() != null ? $(obj).find('.down_arrow') : $(obj).find('.arrow-down');
        popup.css({ left: $(obj).offset().left - 24, top: arrow.offset().top - 64 }).show();

        $(popupId).find('.popup-corner').removeClass('bottom');

        dropdown.regHide(filteredHide);
        dropdown.regScroll(onScroll);

        $(popupId).find('input[placeholder]').placeholder();

        tagNameInput.focus(function() {
            $(this).closest('.entertagname').css({ 'border-color': '#3186af' });
        });

        tagNameInput.blur(function() {
            $(this).closest('.entertagname').css({ 'border-color': '#c7c7c7' });
        });

        tagNameInput.focus();
    };

    var onScroll = function() {
        popup.css({ top: arrow.offset().top - 64 });
    };

    var showMarkRecipientsCheckbox = function() {
        $(popupId + ' #markallrecipients').prop('checked', false);
        $(popupId + ' #markallrecipients').show();
        $(popupId + ' #markallrecipientsLabel').show();
        $(popupId + ' #markallrecipients').unbind('click').bind('click', manageCrmTags);
        var fromAddress = messagePage.getFromAddress(messagePage.getActualConversationLastMessageId());
        fromAddress = ASC.Mail.Utility.ParseAddress(fromAddress).email;
        if (accountsManager.getAccountByAddress(fromAddress)) {
            $(popupId + ' #markallrecipientsLabel').text(MailScriptResource.MarkAllRecipientsLabel);
        } else {
            $(popupId + ' #markallrecipientsLabel').text(MailScriptResource.MarkAllSendersLabel);
        }
    };

    var hideMarkRecipientsCheckbox = function() {
        $(popupId + ' #markallrecipients').prop('checked', false);
        $(popupId + ' #markallrecipients').hide();
        $(popupId + ' #markallrecipientsLabel').hide();
    };

    var manageCrmTags = function(e) {
        if (e.target.checked) {
            $('#tagsPanelContent .tag').each(function() {
                if ($(this).attr('tag_id') < 0) {
                    $(this).addClass("disabled");
                    $(this).unbind("click");
                }
            });
        } else {
            $('#tagsPanelContent .tag').each(function() {
                if ($(this).attr('tag_id') < 0) {
                    $(this).removeClass("disabled");
                    var usedTagsIds = getUsedTagsIds();
                    var tagId = parseInt($(this).attr("tag_id"));
                    if (-1 == $.inArray(tagId, usedTagsIds)) {
                        $(this).bind('click', function() {
                            setTag($(this).attr('tag_id'));
                        });
                    } else {
                        $(this).bind('click', function() {
                            mailBox.unsetConversationsTag($(this).attr('tag_id'));
                        });
                    }
                }
            });
        }
    };

    var getTagContent = function() {
        var res;
        var tags = tagsManager.getAllTags();
        var usedTagsIds = getUsedTagsIds();
        for (var i = 0; i < tags.length; i++) {
            tags[i].used = ($.inArray(tags[i].id, usedTagsIds) > -1);
        }
        res = $.tmpl('tagInPanelTmpl', tags, { htmlEncode: TMMail.htmlEncode });
        return res;
    };

    var setTag = function(id) {
        if (checked()) {
            var tag = tagsManager.getTag(id);
            var addresses = [];

            var from = ASC.Mail.Utility.ParseAddress(messagePage.getFromAddress(messagePage.getActualConversationLastMessageId()));

            if (accountsManager.getAccountByAddress(from.email)) {
                addresses = ASC.Mail.Utility.ParseAddresses(messagePage.getToAddresses(messagePage.getActualConversationLastMessageId())).addresses;
            } else
                addresses.push(from);

            for (var i = 0; i < addresses.length; i++) {
                var address = addresses[i];
                var tagAlreadyAdded = 0 < $.grep(tag.addresses, function(val) { return address.EqualsByEmail(val); }).length;
                if (!tagAlreadyAdded) {
                    tag.addresses.push(address.email);
                }
            }
            tagsManager.updateTag(tag);
        }

        hide();
        mailBox.setTag(id);
    };


    // Get a list of tags that are set for all selected messages, or for a particular message.
    var getUsedTagsIds = function() {
        if (TMMail.pageIs('sysfolders')) {
            return mailBox.getSelectedMessagesUsedTags();
        } else {
            return messagePage.getTags();
        }
    };

    var setTagColor = function(obj, style) {
        popup.find('.entertagname .square').removeClass().addClass('square tag' + style);
        popup.find('.entertagname .square').attr('colorstyle', style);
    };

    // Rerurns flag value. Flag: Is needed to set tag for all messages sended from that user.
    var checked = function() {
        return $(popupId + ' input#markallrecipients').prop('checked');
    };

    var createLabel = function() {
        var tagName = $.trim(popup.find('input[type="text"]').val());
        if (tagName.length == 0) {
            TMMail.setRequiredError('addTagsPanel', true);
            return false;
        }

        TMMail.setRequiredError('addTagsPanel', false);

        var tag = { name: tagName, style: popup.find('.actionPanelSection .square').attr('colorstyle'), addresses: [] };
        tagsManager.createTag(tag);

        return false;
    };

    var bindInputs = function() {
        popup.find('.entertag_button').click(createLabel);

        popup.find('input[type="text"]').keypress(function(e) {
            if (e.which != 13) {
                return;
            }
            return createLabel();
        });
    };

    var hide = function() {
        popup.hide();
        tagsColorsPopup.hide();
        dropdown.unregHide(filteredHide);
        dropdown.unregHide(onScroll);
    };

    var filteredHide = function(event) {
        var elt = (event.target) ? event.target : event.srcElement;
        if (!($(elt).is(popupId + ' *') || $(elt).is(popupId) || $(elt).is('div[colorstyle]'))) {
            hide();
        }
    };


    return {
        init: init,
        show: showAddTagsPanel,
        hide: hide,
        checked: checked
    };
})(jQuery);