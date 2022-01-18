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

jq(function () {
    var time = 60000;

    var waitForZopim = setInterval(function () {
        if (time < 0) {
            clearInterval(waitForZopim);
        };

        if (window.$zopim === undefined || window.$zopim.livechat === undefined) {
            time -= 100;
            return ;
        };

        var $switcher = jq('#liveChatSwitch .switch-btn');

        jq('#liveChatSwitch').click(function () {
            $switcher.toggleClass('switch-on');
            if ($switcher.hasClass('switch-on')) {
                $zopim.livechat.window.hide();
                localStorage.setItem('livechat', 'on');
            } else {
                $zopim.livechat.hideAll();
                localStorage.setItem('livechat', 'off');
            }
        });

        if (localStorage.getItem("livechat") !== 'on') {
            $zopim.livechat.hideAll();
        } else {
            $switcher.click();
        };

        clearInterval(waitForZopim);
    }, 100);
});
