/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
;
jq(document).ready(function () {

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .product-menu",
        dropdownID: "studio_productListPopupPanel",
        addTop: 2,
        addLeft: 16,
        rightPos: true,
        toggleOnOver: true,
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .staff-profile-box",
        dropdownID: "studio_myStaffPopupPanel",
        addTop: 0,
        addLeft: 16,
        rightPos: true
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .searchActiveBox",
        dropdownID: "studio_searchPopupPanel",
        addTop: 8,
        addLeft: 0,
        afterShowFunction: function () {
            jq("#studio_search").focus();

            var w = jq(window),
                scrWidth = w.width(),
                leftPadding = w.scrollLeft(),
                elem = jq(".studio-top-panel .searchActiveBox"),
                dropElem = jq("#studio_searchPopupPanel"),
                tooth = dropElem.children(".corner-top");

            if ((elem.offset().left + dropElem.outerWidth()) > scrWidth + leftPadding) {
                dropElem.css("left", Math.max(0, elem.offset().left - dropElem.outerWidth() + elem.outerWidth()) + "px");
                tooth.removeClass("left").addClass("right");
            } else {
                tooth.removeClass("right").addClass("left");
            }
        }
    });

    jq("#studio_search").keydown(function (event) {
        var code;

        if (!e) {
            var e = event;
        }
        if (e.keyCode) {
            code = e.keyCode;
        } else if (e.which) {
            code = e.which;
        }

        if (code == 13) {
            Searcher.Search();
            return false;
        }

        if (code == 27) {
            jq("#studio_search").val("");
        }
    });

    jq("#studio_searchPopupPanel .search-btn").on("click", function () {
        Searcher.Search();
        return false;
    })

    VideoSaver.Init();
});

var Searcher = new function () {
    this.Search = function () {
        var text = encodeURIComponent(jq("#studio_search").val());

        if (text == "") {
            return false;
        }

        var url = jq("#studio_search").attr("data-url");

        var selectedProducts = [];
        jq("#studio_searchPopupPanel .search-options-box :checkbox:checked").each(function (i, val) {
            selectedProducts.push(jq(val).attr("data-product-id"));
        });
        var productIds = selectedProducts.join(",");

        var productsUriPart = (productIds ? "&products=" + productIds : "");
        url += "?search=" + text + productsUriPart;

        window.open(url, "_self");
    };
};

var VideoSaver = new function () {
    this.Init = function () {
        if (jq("#dropVideoList li a").length != 0) {
            jq(".top-item-box.video").addClass("has-led");
            jq(".top-item-box.video").find(".inner-label").text(jq("#dropVideoList li a").length);
            jq("a.videoActiveBox").removeAttr("href");

            jq.dropdownToggle({
                switcherSelector: ".studio-top-panel .has-led .videoActiveBox",
                dropdownID: "studio_videoPopupPanel",
                addTop: 5,
                addLeft: -300
            });

            jq("#dropVideoList li a").on("click", function () {
                AjaxPro.timeoutPeriod = 1800000;
                UserVideoGuideUsage.SaveWatchVideo([jq(this).attr("id")]);
            });

            jq("#markVideoRead").on("click", function () {
                var allVideoIds = new Array();
                jq("#dropVideoList li a").each(function () {
                    allVideoIds.push(jq(this).attr("id"));
                });
                AjaxPro.timeoutPeriod = 1800000;
                UserVideoGuideUsage.SaveWatchVideo(allVideoIds);

                jq("#studio_videoPopupPanel").hide();
                jq(".top-item-box.video").removeClass("has-led");
                var boxVideo = jq(".videoActiveBox");
                boxVideo.attr("href", boxVideo.attr("data-videourl"));
            });
        }
    };
};
