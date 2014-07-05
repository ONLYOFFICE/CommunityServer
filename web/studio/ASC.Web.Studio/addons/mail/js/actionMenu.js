/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
(function($) {

    var methods = {

        init: function(dropdownItemId, items, pretreatment) {
            var $this = $(this);
            if (0 === $this.length)
                return;

            var am_data = $this.data('actionMenu');

            // If the plugin hasn't been initialized yet
            if (!am_data) {
                $(this).data('actionMenu', {
                    target: $this,
                    dropdownItemId: dropdownItemId,
                    items: items,
                    pretreatment: pretreatment
                });
                am_data = $this.data('actionMenu');
                am_data['hide'] = function() {
                    $("#" + dropdownItemId).hide();
                    $this.find(".menu.active").removeClass("active");
                    dropdown.unregHide(am_data['hide']);
                };
            }
            else {
                am_data['dropdownItemId'] = dropdownItemId;
                am_data['items'] = items;
                am_data['pretreatment'] = pretreatment;
            }

            var $dropdownItem = $("#" + dropdownItemId);
            $dropdownItem.show();
            $dropdownItem.hide();

            $this.off("contextmenu.actionMenu").on("contextmenu.actionMenu", function (e) {
                methods._onClick(e, $dropdownItem, am_data);
            });

            $this.find('.menu').off('click').on('click', function (e) {
                var $this = $(this);
                if (!$this.is('.active')) {
                    $this.addClass('active');
                    methods._onClick(e, $dropdownItem, am_data);
                } else {
                    $dropdownItem.hide();
                    $this.removeClass("active");
                }

            });

            $this.on("remove.actionMenu", methods._destroy);
        },

        _onClick: function (e, $dropdownItem, am_data) {
                // handle click event if it exists
                e && dropdown.onClick(e);

                if (e.pageX == null && e.clientX != null) {
                    var html = document.documentElement;
                    var body = document.body;
                    e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0);
                    e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0);
                }

                var target = $(e.srcElement || e.target);

                var id = target.closest(".row").attr("data_id");
                if (!id || target.closest(".row").hasClass('inactive')) {
                    $dropdownItem.hide();
                    return;
                }

                _showActionMenu(am_data.dropdownItemId, am_data.items, id);
                $("menu.active").removeClass("active");

                $dropdownItem.show();

                var left = $dropdownItem.children(".corner-top").position().left;

                if (am_data.pretreatment) {
                    am_data.pretreatment(id, am_data.dropdownItemId);
                    left = $dropdownItem.children(".corner-top").position().left;
                }

                if (target.is(".menu")) {
                    target.addClass("active");
                    $dropdownItem.css({
                        "top": target.offset().top + target.outerHeight() - 2,
                        "left": target.offset().left - left + 7,
                        "right": "auto"
                    });
                } else {
                    $dropdownItem.css({
                        "top": e.pageY + 3,
                        "left": e.pageX - left - 5,
                        "right": "auto"
                    });
                }

                dropdown.regHide(am_data['hide']);
        },

        _destroy: function() {

            var $this = $(this),
                    am_data = $this.data('actionMenu');

            am_data['hide']();
            $this.removeData('actionMenu');
        }
    };

    var _showActionMenu = function(dropdownItemId, items, id) {

        items.forEach(function(item) {
            $(item.selector).unbind("click").bind("click", function() {
                $("#" + dropdownItemId).hide();
                $(".menu.active").removeClass("active");
                item.handler(id);
            });
        });
    };

    $.fn.actionMenu = function() {
        return methods.init.apply(this, arguments);
    };

})(jQuery);