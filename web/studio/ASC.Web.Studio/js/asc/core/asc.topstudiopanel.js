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

;
jq(document).ready(function () {

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .product-menu",
        dropdownID: "studio_productListPopupPanel",
        addTop: 2,
        addLeft: -14,
        toggleOnOver: true,
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .staff-profile-box",
        dropdownID: "studio_myStaffPopupPanel",
        addTop: 0,
        addLeft: 16,
        rightPos: true
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .searchActiveBox",
        dropdownID: "studio_searchPopupPanel",
        addTop: 8,
        addLeft: 0,
        afterShowFunction: function () {
            jq("#studio_search").focus();

            var w = jq(window),
                scrWidth = w.width(),
                leftPadding = w.scrollLeft(),
                elem = jq(".studio-top-panel .searchActiveBox"),
                dropElem = jq("#studio_searchPopupPanel");

            if ((elem.offset().left + dropElem.outerWidth()) > scrWidth + leftPadding) {
                dropElem.css("left", Math.max(0, elem.offset().left - dropElem.outerWidth() + elem.outerWidth()) + "px");
            }
        }
    });

    jq("#studio_search").keydown(function (event) {
        var code;

        if (!e) {
            var e = event;
        }
        if (e.keyCode) {
            code = e.keyCode;
        } else if (e.which) {
            code = e.which;
        }

        if (code == 13) {
            Searcher.Search();
            return false;
        }

        if (code == 27) {
            jq("#studio_search").val("");
        }
    });

    jq("#studio_searchPopupPanel .search-btn").on("click", function() {
        Searcher.Search();
        return false;
    });
    UnreadMailManager.Init();
});

var Searcher = new function () {
    this.Search = function () {
        var text = encodeURIComponent(jq("#studio_search").val());

        if (text == "") {
            return false;
        }

        var url = jq("#studio_search").attr("data-url");

        var selectedProducts = [];
        jq("#studio_searchPopupPanel .search-options-box :checkbox:checked").each(function (i, val) {
            selectedProducts.push(jq(val).attr("data-product-id"));
        });
        var productIds = selectedProducts.join(",");

        var productsUriPart = (productIds ? "&products=" + productIds : "");
        url += "?search=" + text + productsUriPart;

        window.open(url, "_self");
    };
};


var UnreadMailManager = new function () {

    this.Init = function () {
        jq.dropdownToggle({
            switcherSelector: '.studio-top-panel .mailActiveBox',
            dropdownID: 'studio_dropMailPopupPanel',
            addTop: 5,
            addLeft: -392
        });

        jq('.studio-top-panel .mailActiveBox').on('click', function (event) {
            if (jq("#studio_dropMailPopupPanel").is(":visible")) {
                event.preventDefault();
                return;
            }
            var unreadMailCount = 0;
            if (Modernizr && Modernizr.localstorage) {
                unreadMailCount = window.localStorage.getItem("TPUnreadMessagesCount");
            }
            if (event.which == 2 && unreadMailCount && event.which != 1) {
                return true;
            }
            if (unreadMailCount && jq(this).hasClass("has-led")) {
                showLoaderMail();
                Teamlab.getMailFilteredConversations({}, {
                    unread: true
                },
                {
                    success: onGetDropMail
                });
                event.preventDefault();
            } else {
                event.stopPropagation();
            }
            return true;
        });

        jq('#drop-mail-box').on("click", ".mark-all-btn", markAsReadedLetters);

    }

    function showLoaderMail(){
        var $dropMailBox = jq('#drop-mail-box'),
            $loader = $dropMailBox.find('.loader-text-block'),
            $seeAllBtn = $dropMailBox.find('.see-all-btn'),
            $markAllBtn = $dropMailBox.find('.mark-all-btn'),
            $dropMailList = $dropMailBox.find('.list'),
            $emptyListText = $dropMailBox.find('.mail-readed-msg');
        $loader.show();
        $seeAllBtn.hide();
        $markAllBtn.hide();
        $dropMailList.hide();
        $emptyListText.hide();
    }

    function hideLoaderMail(isListEmpty) {
        var $dropMailBox = jq('#drop-mail-box'),
          $loader = $dropMailBox.find('.loader-text-block'),
          $seeAllBtn = $dropMailBox.find('.see-all-btn'),
          $markAllBtn = $dropMailBox.find('.mark-all-btn'),
          $dropMailList = $dropMailBox.find('.list'),
          $emptyListText = $dropMailBox.find('.mail-readed-msg');
        $loader.hide();
        $seeAllBtn.show();
        if (isListEmpty) {
            $dropMailList.hide();
            $emptyListText.show();
        } else {
            $markAllBtn.show();
            $dropMailList.show();
            $emptyListText.hide();
        }        
    }

    function onGetDropMail(params, response) {
        var $dropMailBox = jq('#drop-mail-box'),
            $markAllBtn = $dropMailBox.find('.mark-all-btn'),
            dropMailList = $dropMailBox.find('.list');

        if (response) {
            var mails = response,
                ln = mails.length < 10 ? mails.length : 10;
                        
            dropMailList.empty();             

            if (ln) {
                for (var i = 0; i < ln; i++) {
                    try {
                        var template = getMailTemplate(mails[i]);
                        jq.tmpl('dropMailTmpl', template).appendTo(dropMailList);
                    } catch (e) {
                        toastr.error(e);
                    }
                }
            } else {
                jq(".mailActiveBox").removeClass("has-led");
            }
            hideLoaderMail(!ln);
            dropMailList.scrollTop(0);
            return;
        }
    }

    function getMailTemplate(mail) {
        var tmpl = mail;
        tmpl.itemUrl = "../../addons/mail/#conversation/" + mail.id;
        tmpl.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(mail.receivedDate));
        tmpl.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(mail.receivedDate));

        return tmpl;
    }

    function markAsReadedLetters() {
        var $items = jq("#drop-mail-box").find(".item"),
            ids = [];

        for (var j = 0, length = $items.length; j < length; j++) {
            ids.push(jq($items[j]).attr("data-id"));
        }

        Teamlab.markMailConversations({}, ids, "read", {
            success: function () {
                if (Modernizr && Modernizr.localstorage) {
                    var stored_count = window.localStorage.getItem("TPUnreadMessagesCount");
                        $unreadMes = jq("#TPUnreadMessagesCount"),
                        unreadMails = 0;

                    unreadMails = stored_count - ids.length;
                    window.localStorage.setItem("TPUnreadMessagesCount", unreadMails);
                    if (unreadMails > 0) {
                        unreadMails > 100 ? $unreadMes.html(">100") : $unreadMes.html(unreadMails);
                    } else {
                        if (location.pathname == "/addons/mail/" && (location.hash == "" || location.hash == "#inbox")) {
                            location.reload();
                        } else {
                            jq("#TPUnreadMessagesCount").remove();
                        }
                    }
                    jq("#studio_dropMailPopupPanel").hide();
                }
            }
        });
    }
}
