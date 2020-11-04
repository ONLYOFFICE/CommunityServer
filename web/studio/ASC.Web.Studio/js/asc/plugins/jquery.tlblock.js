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
