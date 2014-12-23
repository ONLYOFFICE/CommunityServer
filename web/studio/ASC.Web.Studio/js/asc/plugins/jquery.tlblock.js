/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        return this;
    };
})(jQuery, window, document, document.body);
