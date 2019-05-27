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


(function ($, undefined) {
    "use strict";
    var counterElm = document.createElement('div');
    counterElm.className = "lattersCount counter pull-right";
    var counterSpan = document.createElement("span");
    counterSpan.className = "unread new-label-menu nohover";
    counterElm.appendChild(counterSpan);
    /*
     <div class="lattersCount counter pull-right">
        <span class="unread new-label-menu nohover">1</span>                                            
     </div>
     */

    $.jstree.plugins.counters = function (options, parent) {
        // own function
        /*this.sample_function = function (arg) {
            // you can chain this method if needed and available
            if (parent.sample_function) { parent.sample_function.call(this, arg); }
        };*/

        // *SPECIAL* FUNCTIONS
        this.init = function (el, options) {
            // do not forget parent
            parent.init.call(this, el, options);
        };
        // bind events if needed
        this.bind = function () {
            // call parent function first
            parent.bind.call(this);
        };
        // unbind events if needed (all in jquery namespace are taken care of by the core)
        this.unbind = function () {
            // do(stuff);
            // call parent function last
            parent.unbind.call(this);
        };
        this.teardown = function () {
            // do not forget parent
            parent.teardown.call(this);
        };

        this.redraw_node = function (obj, deep, callback, force_draw) {
            obj = parent.redraw_node.call(this, obj, deep, callback, force_draw);
            if (obj) {
                var tmp = counterElm.cloneNode(true);
                var span = tmp.getElementsByTagName('span')[0];

                var unread = commonSettingsPage.isConversationsEnabled() ? obj.getAttribute("unread_chains") : obj.getAttribute("unread_messages");
                if (unread && unread > 0) {
                    span.classList.toggle("hidden", false);
                    span.textContent = unread;
                } else {
                    span.classList.toggle("hidden", true);
                }

                obj.insertBefore(tmp, obj.childNodes[1]);
            }
            return obj;
        };
    };

    // you can include the sample plugin in all instances by default
    $.jstree.defaults.plugins.push("counters");
})(jQuery);