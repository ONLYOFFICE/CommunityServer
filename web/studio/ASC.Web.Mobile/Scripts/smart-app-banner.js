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

var origHtmlMargin = parseFloat($('html').css('margin-top'));

$(function () {
    if (iOSversion() && iOSversion()[0] < 6) {
        var iPad = navigator.userAgent.match(/iPad/i) != null; // Check if using an iPad
        var iPhone = navigator.userAgent.match(/iPhone/i) != null; // Check if using an iPhone
        var safari = navigator.userAgent.match(/Safari/i) != null; // Check if using Safari

        var standalone = navigator.standalone;
        var appBannerID = $('meta[name=apple-itunes-app]').attr("content"); //Check if using smart app banners
        if (!standalone && safari) { safari = false }; //Chrome is just a re-skinning of iOS WebKit UIWebView
        if (appBannerID != null) {
            appBannerID = appBannerID.replace('app-id=', '');
            if (true) {
                $.getJSON('http://itunes.apple.com/lookup?id=' + appBannerID + '&callback=?', function (json) {
                    if (json != null) {
                        var artistName, artistViewUrl, artworkUrl60, averageUserRating, formattedPrice, trackCensoredName, averageUserRatingForCurrentVersion;
                        artistName = json.results[0].artistName;
                        artistViewUrl = json.results[0].artistViewUrl;
                        artworkUrl60 = json.results[0].artworkUrl60;
                        averageUserRating = json.results[0].averageUserRating;
                        formattedPrice = json.results[0].formattedPrice;
                        averageUserRatingForCurrentVersion = json.results[0].averageUserRatingForCurrentVersion;
                        trackCensoredName = json.results[0].trackCensoredName;

                        //make sure rating is not null. 
                        if (averageUserRating == null) { averageUserRating = 0; }
                        if (averageUserRatingForCurrentVersion == null) { averageUserRatingForCurrentVersion = 0; }

                        var banner = '<div class="smart-banner">';
                        banner += '<a id="swb-close" onclick="CloseSmartBanner()">X</a>';
                        banner += '<img src="' + artworkUrl60 + '" alt="" class="smart-glossy-icon" />';
                        banner += '<div id="swb-info"><strong>' + trackCensoredName + '</strong>';
                        banner += '<span>' + artistName + '</span>';
                        banner += '<span class="rating-static rating-' + averageUserRating.toString().replace(".", "") + '"></span>';
                        banner += '<span>' + formattedPrice + '</span></div>';
                        banner += '<a href="' + artistViewUrl + '" id="swb-save">VIEW</a></div>';

                        $('body').append(banner);
                        $('html').animate({ marginTop: origHtmlMargin + 78 }, 300);
                    }
                });
            }
       }
    }
 
}); 
      
function CloseSmartBanner() {
  $('.smart-banner').stop().animate({top:-82},300);
  $('html').animate({marginTop:origHtmlMargin},300);
} 

function iOSversion() {
    if (/iP(hone|od|ad)/.test(navigator.platform)) {
        // supports iOS 2.0 and later: <http://bit.ly/TJjs1V>
        var v = (navigator.appVersion).match(/OS (\d+)_(\d+)_?(\d+)?/);
        return [parseInt(v[1], 10), parseInt(v[2], 10), parseInt(v[3] || 0, 10)];
    }
}
