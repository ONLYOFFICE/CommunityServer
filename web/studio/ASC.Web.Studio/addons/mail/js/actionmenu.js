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

            $("body").off("contextmenu.actionMenu." + dropdownItemId).on("contextmenu.actionMenu." + dropdownItemId, function (e) {
                var target = $(e.srcElement || e.target);
                if (!$.contains($this[0], target[0])) {
                    $dropdownItem.hide();
                }
            });

            $this.off("contextmenu.actionMenu." + dropdownItemId).on("contextmenu.actionMenu." + dropdownItemId, function (e) {
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