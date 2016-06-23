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


window.ASC.Clipboard = (function () {
    var isInit = false;
    var enable = false;
    var viaFlash = false;
    var viaDesktop = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        if (ASC.Clipboard.enable = ASC.Clipboard.viaFlash = !jq.browser.mobile && jq.browser.flashEnabled()
            && typeof ZeroClipboard != 'undefined' && ZeroClipboard.moviePath === 'ZeroClipboard.swf') {
            ZeroClipboard.setMoviePath(ASC.Resources.Master.ZeroClipboardMoviePath);
        } else {
            ASC.Clipboard.enable = ASC.Clipboard.viaDesktop = !!window["AscDesktopEditor"];
        }
    };

    var create = function (text, buttonId, options) {
        if (!ASC.Clipboard.enable) {
            return null;
        }

        var opt = {
            zIndex: 670,
            onComplete: null,
            onMouseOver: null,
            onMouseOut: null,
            textareaId: null,
            panelId: buttonId,
        };

        jq.extend(opt, options);

        if (ASC.Clipboard.viaFlash) {
            return createViaFlash(text, buttonId, opt);
        }

        if (ASC.Clipboard.viaDesktop) {
            return createViaDesktop(text, buttonId, opt);
        }

        return null;
    };

    var createViaFlash = function (text, buttonId, opt) {
        var clip = new ZeroClipboard.Client();

        if (!text && opt.textareaId) {
            text = jq("#" + textareaId).val();
        }

        clip.setText(text);
        clip.glue(buttonId, opt.panelId, opt);

        if (opt.onComplete) {
            clip.addEventListener("onComplete", opt.onComplete);
        }

        if (opt.onMouseOver) {
            clip.addEventListener("onMouseOver", opt.onMouseOver);
        }

        if (opt.onMouseOut) {
            clip.addEventListener("onMouseOut", opt.onMouseOut);
        }

        return clip;
    };

    var createViaDesktop = function (text, buttonId, opt) {
        var clip = jq("#" + buttonId)
            .on("click.clipboard", function () {
                if (opt.textareaId) {
                    jq("#" + opt.textareaId).select();
                } else {
                    var textareaId = buttonId + new Date().getTime();

                    var tempInput = document.createElement("input");
                    tempInput.id = textareaId;
                    tempInput.type = "text";
                    tempInput.style.height = "1px";
                    tempInput.style.width = "1px";
                    tempInput.style.left = "-10px";
                    tempInput.style.right = "-10px";
                    tempInput.style.position = "absolute";
                    document.body.appendChild(tempInput);

                    tempInput = jq("#" + textareaId);
                    tempInput.val(text);
                    tempInput.select();
                }

                window["AscDesktopEditor"].Copy();

                if (tempInput) {
                    tempInput.remove();
                }

                jq(this).trigger("mouseout.clipboard");

                if (opt.onComplete) {
                    opt.onComplete();
                }
            })
            .on("mouseover.clipboard", function () {
                if (opt.onMouseOver) {
                    opt.onMouseOver();
                }
            })
            .on("mouseout.clipboard", function () {
                if (opt.onMouseOut) {
                    opt.onMouseOut();
                }
            });

        return clip;
    };

    var destroy = function (clip) {
        if (ASC.Clipboard.viaFlash && clip && clip.destroy) {
            clip.destroy();
            delete window.ZeroClipboard.clients[clip.id];
        }

        if (ASC.Clipboard.viaDesktop && clip) {
            clip.off(".clipboard");
        }
    };

    return {
        init: init,

        enable: enable,
        viaFlash: viaFlash,
        viaDesktop: viaDesktop,

        create: create,
        destroy: destroy,
    };
})();

(function ($) {
    $(function () {
        ASC.Clipboard.init();
    });
})(jQuery);