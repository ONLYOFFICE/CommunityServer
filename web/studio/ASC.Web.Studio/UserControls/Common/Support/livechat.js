/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


if (typeof (ASC) === 'undefined') {
    ASC = {};
}

if (typeof (ASC.ZopimLiveChat) === 'undefined') {
    ASC.ZopimLiveChat = {};
}

ASC.ZopimLiveChat = (function () {

    var keyChat;
    var onloadTimeout = null;
    var counter = 0;
    var counterMax = 100;
    var $switcher = jq('#liveChatSwitch .switch-btn');

    var init = function (key) {

        if (!$switcher.length) return;

        keyChat = key;

        jq('#liveChatSwitch').on("click", function () {
            if (jq(this).hasClass('disabled')) return;

            if (window.$zopim === undefined || window.$zopim.livechat === undefined) {
                jq(this).addClass('disabled');
                loadLiveChat();
                return;
            }
            toggleSwitcher(localStorage.getItem('livechat') === 'off');
        });

        if (localStorage.getItem('livechat') === 'on') {
            jq('#liveChatSwitch').addClass('disabled');
            loadLiveChat();
        }
    };

    var loadLiveChat = function () {
        window.$zopim || (function (d, s) {
            var z = $zopim = function (c) { z._.push(c) },
                $ = z.s = d.createElement(s),
                e = d.getElementsByTagName(s)[0];
            z.set = function (o) {
                z.set._.push(o)
            };
            z._ = [];
            z.set._ = [];
            $.async = !0;
            $.setAttribute("charset", "utf-8");
            $.src = "https://v2.zopim.com/?" + keyChat;
            z.t = +new Date;
            $.type = "text/javascript";
            $.onload = onloadLiveChat;
            e.parentNode.insertBefore($, e)
        })(document, "script");
    };

    var onloadLiveChat = function () {
        clearTimeout(onloadTimeout);

        if (counter < counterMax) {

            if (window.$zopim.livechat === undefined) {
                console.log('livechat undefined');
                onloadTimeout = setTimeout(onloadLiveChat, 200);
                counter += 1;
                return;
            }

            toggleSwitcher(true);
        }
    };

    var toggleSwitcher = function (toggle) {
        jq('#liveChatSwitch').removeClass('disabled');
        if (toggle) {
            $switcher.addClass('switch-on');
            $zopim.livechat.setDisableSound(false);
            $zopim.livechat.window.hide();
            localStorage.setItem('livechat', 'on');
        } else {
            $switcher.removeClass('switch-on');
            $zopim.livechat.setDisableSound(true);
            $zopim.livechat.hideAll();
            localStorage.setItem('livechat', 'off');
        }
    }

    return {
        init: init
    }
})();