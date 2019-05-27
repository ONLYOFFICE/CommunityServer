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
    var popup,
        popupId = '#addTagsPanel',
        isInit = false,
        showButton;

    function init() {
        if (isInit === false) {
            isInit = true;

            popup = $(popupId);
        }
    }

    function prepareOptions(opts) {
        if (!opts)
            opts = {};

        var options = {};

        options.hideMarkRecipients = typeof opts !== "object" ||
            typeof opts.hideMarkRecipients === "undefined" ||
            opts.hideMarkRecipients === null
            ? false
            : opts.hideMarkRecipients;

        options.onSelect = typeof opts !== "object" ||
            typeof opts.onSelect === "undefined" ||
            typeof opts.onSelect !== "function"
            ? function (e, tag) {
                console.log("Default onSelect callback has been called", tag);
            }
            : opts.onSelect;

        options.onDeselect = typeof opts !== "object" ||
            typeof opts.onDeselect === "undefined" ||
            typeof opts.onDeselect !== "function"
            ? function (e, tag) {
                console.log("Default onDeselect callback has been called", tag);
            }
            : opts.onDeselect;

        options.getUsedTagsIds = typeof opts !== "object" ||
            typeof opts.getUsedTagsIds === "undefined" ||
            typeof opts.getUsedTagsIds !== "function"
            ? function () {
                console.log("Default getUsedTagsIds callback has been called");
                return [];
            }
            : opts.getUsedTagsIds;

        return options;
    }

    function draw() {
        var offset = $(showButton).offset();
        popup.css({ left: offset.left - 24, top: offset.top - 54 }).show();
    }

    function showMarkRecipientsCheckbox() {
        var cbx = popup.find("#markallrecipients");
        cbx.prop('checked', false);
        cbx.show();
        cbx.unbind('click').bind('click', manageCrmTags);

        var cbxLbl = popup.find("#markallrecipientsLabel");
        cbxLbl.show();

        var fromAddress = messagePage.getFromAddress(messagePage.getActualConversationLastMessageId());
        fromAddress = ASC.Mail.Utility.ParseAddress(fromAddress).email;

        if (accountsManager.getAccountByAddress(fromAddress)) {
            cbxLbl.text(MailScriptResource.MarkAllRecipientsLabel);
        } else {
            cbxLbl.text(MailScriptResource.MarkAllSendersLabel);
        }
    }

    function hideMarkRecipientsCheckbox() {
        var cbx = popup.find("#markallrecipients");
        cbx.prop('checked', false);
        cbx.hide();

        var cbxLbl = popup.find("#markallrecipientsLabel");
        cbxLbl.hide();
    }

    function manageCrmTags(e) {
        var tags = popup.find("#tagsPanelContent .tag");

        tags.each(function() {
            var el = $(this);
            if (e.target.checked) {
                if (el.attr('tag_id') < 0) {
                    el.addClass("disabled");
                }
            } else {
                if (el.attr('tag_id') < 0) {
                    el.removeClass("disabled");
                }
            }
        });
    }

    function getTagContent(options) {
        var tags = tagsManager.getAllTags();
        var usedTagsIds = options.getUsedTagsIds();

        for (var i = 0; i < tags.length; i++) {
            tags[i].used = usedTagsIds.length > 0 ? ($.inArray(tags[i].id, usedTagsIds) > -1) : false;
        }

        var res = $.tmpl('tagInPanelTmpl', tags, { htmlEncode: TMMail.htmlEncode });
        return res;
    }

    // Rerurns flag value. Flag: Is needed to set tag for all messages sended from that user.
    function checked() {
        var cbx = popup.find("#markallrecipients");
        return cbx.prop('checked');
    }

    function createLabel(options) {
        hide();

        tagsModal.showCreate({
            onSuccess: function (tag) {
                options.onSelect(
                {
                    id: tag.id,
                    name: tag.name
                });
            }
        });
    }

    function filteredHide(event) {
        var elt = (event.target) ? event.target : event.srcElement;
        if (!($(elt).is(popupId + ' *') || $(elt).is(popupId) || $(elt).is('div[colorstyle]'))) {
            hide();
        }
    }

    // shows add tags panel near obj element depending on current page
    function show(obj, opts) {
        if (popup.is(":visible")) {
            hide();
            return;
        }

        var options = prepareOptions(opts);

        var tagContent = getTagContent(options);

        popup.find(".existsTags").html(tagContent);

        var $tag = popup.find(".tag.inactive");
        $tag.unbind('click').bind('click', function (e) {
            var el = $(this);

            if (el.hasClass("disabled")) return;

            options.onSelect.call(e,
                {
                    id: el.attr('tag_id'),
                    name: el.text()
                });
            hide();
        });

        $tag = popup.find(".tag.tagArrow");
        $tag.unbind('click').bind('click', function (e) {
            var el = $(this);

            if (el.hasClass("disabled")) return;

            options.onDeselect.call(e,
                {
                    id: el.attr('tag_id'),
                    name: el.text()
                });
            hide();
        });

        if (options.hideMarkRecipients) {
            hideMarkRecipientsCheckbox();
        } else {
            showMarkRecipientsCheckbox();
        }

        TMMail.setRequiredError('addTagsPanel', false);

        showButton = obj;

        draw();

        popup.find('.popup-corner').removeClass('bottom');

        dropdown.regHide(filteredHide);
        dropdown.regScroll(draw);

        popup.find('.entertag_button')
            .unbind('click')
            .bind('click',
                function() {
                    createLabel(options);
                });
    }

    function hide() {
        popup.hide();
        tagsColorsPopup.hide();
        dropdown.unregHide(filteredHide);
        dropdown.unregHide(draw);
        popup.find('.entertag_button').unbind('click');
    }

    return {
        init: init,
        show: show,
        hide: hide,

        isMarkChecked: checked
    };
})(jQuery);