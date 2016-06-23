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


(function($) {

    var methods = {
        init: function(dropdownItemId, items, pretreatment) {
            var $this = $(this);
            if (0 === $this.length) {
                return;
            }

            var amData = $this.data('actionMenu');

            // If the plugin hasn't been initialized yet
            if (!amData) {
                $(this).data('actionMenu', {
                    target: $this,
                    dropdownItemId: dropdownItemId,
                    items: items,
                    pretreatment: pretreatment
                });
                amData = $this.data('actionMenu');
                amData['hide'] = function(event) {
                    var e = $.fixEvent(event);
                    if (e.button == 2) {
                        return;
                    }

                    $("#" + dropdownItemId).hide();
                    $this.find(".entity-menu.active").removeClass("active");
                    dropdown.unregHide(amData['hide']);
                };
            } else {
                amData['dropdownItemId'] = dropdownItemId;
                amData['items'] = items;
                amData['pretreatment'] = pretreatment;
            }

            var $dropdownItem = $("#" + dropdownItemId);
            $dropdownItem.show();
            $dropdownItem.hide();

            $this.off("contextmenu.actionMenu").on("contextmenu.actionMenu", function(e) {
                return methods._onClick(e, $dropdownItem, amData);
            });

            $this.find('.entity-menu').off('click').on('click', function (e) {
                var $thisEl = $(this);
                if (!$thisEl.is('.active')) {
                    $thisEl.addClass('active');
                    methods._onClick(e, $dropdownItem, amData);
                } else {
                    $dropdownItem.hide();
                    $thisEl.removeClass("active");
                }

            });

            $this.on("remove.actionMenu", methods._destroy);
        },

        _onClick: function(event, $dropdownItem, amData) {
            var e = $.fixEvent(event);

            if (typeof e == "undefined" || !e) {
                return true;
            }
            $(window).click(); // initiate global event for other dropdowns close

            var target = $(e.srcElement || e.target),
                id = target.is(".entity-menu") ? target.attr("data_id") : target.closest(".row").attr("data_id");

            if (!id || target.closest(".row").hasClass('inactive')) {
                $dropdownItem.hide();
                return true;
            }

            showActionMenu(amData.dropdownItemId, amData.items, id);
            $("menu.active").removeClass("active");

            if (amData.pretreatment) {
                amData.pretreatment(id, amData.dropdownItemId);
            }

            jq.showDropDownByContext(e, target, $dropdownItem);

            dropdown.regHide(amData['hide']);
            return false;
        },

        _destroy: function() {

            var $this = $(this),
                amData = $this.data('actionMenu');

            amData['hide']();
            $this.removeData('actionMenu');
        }
    };

    var showActionMenu = function(dropdownItemId, items, id) {

        items.forEach(function(item) {
            $(item.selector).unbind("click").bind("click", function(event) {
                if ($(this).hasClass('disable')) {
                    event.stopPropagation();
                    return;
                }

                $("#" + dropdownItemId).hide();
                $(".entity-menu.active").removeClass("active");
                item.handler(id);

            });
        });
    };

    $.fn.actionMenu = function() {
        return methods.init.apply(this, arguments);
    };

})(jQuery);