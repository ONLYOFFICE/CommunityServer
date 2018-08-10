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


(function ($, win, doc, body) {
    var supportedClient = true,
        blockClassname = "tl-block",
        blocksCollection = [],
        $window = $(window),
        $body = $(body),
        setResizeCallback = false;

    // check client
    supportedClient = $.browser.msie && $.browser.version < 8 ? false : supportedClient;

    // methods
    function getFullOffsetTop (o) {
        var offsetTop = 0,
            fullOffsetTop = o.offsetTop;
        while (o = o.offsetParent) {
            offsetTop = o.offsetTop;
            offsetTop ? fullOffsetTop += offsetTop : null;
        }
        return fullOffsetTop;
    }

    function resizeBlock ($block) {
        var maxContentHeight,
            blockHeight,
            column = null,
            columnsInd;

        blockHeight = $block.css("overflow-y", "auto").height("auto").height();
        maxContentHeight = $window.height() - getFullOffsetTop($block[0]) - 1; // 1 - border-fix
        $block.height(maxContentHeight + 'px');
        if (blockHeight > maxContentHeight) {
            $block.css("overflow-y", "scroll");
        }

        //maxColumnHeight = 0;
        //columnsInd = $columns.length;
        //while (columnsInd--) {
        //    column = $columns[columnsInd];
        //    column.style.height = "auto";
        //    columnHeight = column.offsetHeight;
        //    maxColumnHeight = columnHeight > maxColumnHeight ? columnHeight : maxColumnHeight;
        //}

        //$columns.height(maxColumnHeight > windowHeight ? windowHeight : )
    }

    // callbacks
    function onWindowResize (evt) {
        var blocks = blocksCollection,
            blocksInd = 0;

        blocksInd = blocks.length;
        while (blocksInd--) {
            resizeBlock(blocks[blocksInd]);
        }
    }

    function initBlock ($block, opts) {
        $body.addClass(blockClassname);
        $block.addClass(blockClassname).css("overflow-x", "visible");

        resizeBlock($block);
        blocksCollection.push($block);
    }

    $.fn.tlBlock = function (opt) {

        if (opt && typeof opt === 'string') {
            switch (opt) {
                case 'resize':
                    return this.each(function () {
                        if (supportedClient) {
                            resizeBlock($(this));
                        }
                    });

            }
            return this;
        }

        opt = $.extend({
            title: "",
            classname: ""
        }, opt);

        if (!setResizeCallback) {
            setResizeCallback = true;
            $window.unbind("resize", onWindowResize).bind("resize", onWindowResize);
        }

        return this.each(function () {
            var $this = $(this);
            if (supportedClient) {
                initBlock($this, opt);
            }
        });

        //return this;
    };
})(jQuery, window, document, document.body);
