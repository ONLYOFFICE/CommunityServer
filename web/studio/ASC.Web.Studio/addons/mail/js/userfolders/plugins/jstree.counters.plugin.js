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