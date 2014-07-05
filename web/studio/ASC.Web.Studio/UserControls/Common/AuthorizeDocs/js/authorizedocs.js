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

jq(function() {
    jq(".auth-form-with_buttons_text").hover(
        function (event) {
            jq(event.target).animate({ left: "-=10" }, 300);
            jq(event.target).animate({ left: "+=10" }, 300);
        }, function () {
        });

    function GetQuoteList() {
        var lng = jq(".auth-form-with_slogan_carousel .carousel-inner").attr("data-lng");
        lng = lng ? lng.toLowerCase() : "en";
        
        jq.getJSON("/UserControls/Common/AuthorizeDocs/js/quotes.json", function (data) {
            var quotes = data.en.quotes,
                items = "";

            jq.each(data, function (key, val) {
                if (key == lng) {
                    quotes = val.quotes;
                }
            });
            quotes.forEach(function (quote) {
                items += ("<a class=\"item\" target=\"_blank\" href=\"" + quote.link
                    + "\"><div class=\"personal-quote\">" + quote.value
                    + "</div><div class=\"personal-author\">" + quote.author + "</div></a>");
            });
            jq(".auth-form-with_slogan_carousel .carousel-inner").html(items);
            jq(".auth-form-with_slogan_carousel .carousel-inner .item").first().addClass("active");
        });
    }

    GetQuoteList();
    jq(".auth-form-with_slogan_carousel").carousel({
        interval: 5000
    });

})