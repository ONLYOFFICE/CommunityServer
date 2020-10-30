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


window.ASC.Clipboard = (function () {
    var isInit = false;
    var enable = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        ASC.Clipboard.enable = (typeof Clipboard != "undefined" && Clipboard.isSupported());
    };

    var create = function (text, buttonId, options) {
        if (!ASC.Clipboard.enable) {
            return null;
        }

        var opt = {
            onComplete: null,
            textareaId: null,
        };

        jq.extend(opt, options);

        if (opt.textareaId) {
            var cfg = {
                target: function () {
                    return jq("#" + opt.textareaId)[0];
                }
            };
        } else {
            cfg = {
                text: function () {
                    return text;
                }
            };
        }

        var clip = new Clipboard("#" + buttonId, cfg);

        if (opt.onComplete) {
            clip.on("success", opt.onComplete);
        }

        return clip;
    };

    var destroy = function (clip) {
        if (clip) {
            clip.destroy();
        }

        return null;
    };

    return {
        init: init,

        enable: enable,

        create: create,
        destroy: destroy,
    };
})();

(function ($) {
    $(function () {
        ASC.Clipboard.init();
    });
})(jQuery);