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

window.ASC.Controls.StartSteps = (function () {
    var projVideo;
    var peopleVideo;
    var docsVideo;
    var crmVideo;
    var arVideo;

    var init = function () {
        var culture = jq("#curCulture").text();
        var protocol = location.protocol;

        switch (culture) {
            case "de-DE":
                projVideo = protocol + "//www.youtube.com/embed/FtGwZPaRY6k?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/JuiojlK2e58?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/ZWUpl9DBCco?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/OPf8wV2m6lg?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/cgmLl9ZxSL4?autoplay=1";
                break;
            case "es-ES":
                projVideo = protocol + "//www.youtube.com/embed/UPVb9LrctA0?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/8_J9y9EEc5s?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/c4VZUxe1dcA?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/OrcRr-Wmfes?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/NjSu1APX6Io?autoplay=1";
                break;
            case "fr-FR":
                projVideo = protocol + "//www.youtube.com/embed/Qn64XoOoZxM?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/xbvpN0Jv8uM?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/WUOAcJPVF-g?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/1EF7BSibv2o?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/cpWpIHFipdM?autoplay=1";
                break;
            case "it-IT":
                projVideo = protocol + "//www.youtube.com/embed/brDilp1I02k?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/YptsfQoTqu8?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/A3nWJAk1o6s?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/Yyt8MOrslHU?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/SwJ9PnjPi1Y?autoplay=1";
                break;
            case "ru-RU":
                projVideo = protocol + "//www.youtube.com/embed/utTZDwXyyoE?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/lI3dsjZd5e0?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/qJY3BaIHgyQ?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/Pme06OhkvGk?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/CeutfrndjrE?autoplay=1";
                break;
            default:
                projVideo = protocol + "//www.youtube.com/embed/X9_8z-Y0uZM?autoplay=1";
                peopleVideo = protocol + "//www.youtube.com/embed/2fPVa1A93Pg?autoplay=1";
                docsVideo = protocol + "//www.youtube.com/embed/a2w-KmqbAsE?autoplay=1";
                crmVideo = protocol + "//www.youtube.com/embed/5vqW_1WWfzE?autoplay=1";
                arVideo = protocol + "//www.youtube.com/embed/YemscFLqgIo?autoplay=1";
                break;
        }

        var moduleValue = getModule();

        if (moduleValue) {
            var choice = jq(".item-module." + moduleValue).children(".default-module");
            jq(choice).addClass("choosed");

            setTimeout(unlockButton, 60000);

            var module = jq(choice).attr("data-name");
            var link, relate;

            switch (module) {
                case "documents":
                    link = docsVideo;
                    relate = peopleVideo;
                    break;
                case "projects":
                    link = projVideo;
                    relate = arVideo;
                    break;
                case "crm":
                    link = crmVideo;
                    relate = arVideo;
                    break;
                default:
                    return;
            }
            jq("#relatedVideo").on("click", function () {
                matchVideo();
                if (jq(".choose-module-video iframe").attr("src") == arVideo) {
                    relate = peopleVideo;
                }
                jq(".choose-module-video iframe").attr("src", relate);
            });

            onloadYoutube();

            jq(".choose-module-video iframe").attr("src", link);
            jq("#chooseDefaultModule").addClass("display-none");
            jq("#chooseVideoModule").removeClass("display-none");
            matchVideo();
        }

        jq(".item-module").on("click", function () {
            var moduleName = jq(this).children(".default-module").attr("data-name");
            window.location.replace("welcome.aspx?module=" + moduleName);
        });
    };

    var unlockButton = function () {
        var moduleUrl = jq(".default-module.choosed").attr("data-url");
        if (moduleUrl == null) {
            moduleUrl = jq(".item-module.documents").parent().attr("data-url");
        }
        jq("#continueVideoModule").attr("href", moduleUrl);

        jq("#continueVideoModule").removeClass("disable");
        jq("#continueVideoModule").text(jq("#continueVideoModule").attr("data-value"));
    };

    var iframeLoad = function () {
        setTimeout(unlockButton, 60000);
        var frame = "<iframe id=\"docsFrame\" width=1 height=1 style=\"position: absolute; visibility: hidden;\" ></iframe>";
        jq("#listScriptStyle").append(frame);
        jq("#docsFrame").attr("src", jq("#listScriptStyle").attr("data-docs"));

        document.getElementsByTagName("iframe")[2].onload = function () {
            unlockButton();
        };
    };

    var iframeCommonLoad = function () {
        var frame = "<iframe id=\"commonFrame\" width=1 height=1 style=\"position: absolute; visibility: hidden;\" ></iframe>";
        var module = getModule();
        jq("#listScriptStyle").append(frame);
        jq("#commonFrame").attr("src", jq("#listScriptStyle").attr("data-common") + "?module=" + module);

        document.getElementsByTagName("iframe")[1].onload = function () {
            unlockButton();
        };
    };

    var onloadYoutube = function () {
        document.getElementsByTagName("iframe")[0].onload = function () {
            setTimeout(iframeCommonLoad, 5000);
            setTimeout(iframeLoad, 6000);
        };
    };

    var getModule = function () {
        var regex = new RegExp("[\\?&]module=([^&#]*)", "i");
        var results = regex.exec(window.location.search);
        if (results == null) {
            return "";
        } else {
            return decodeURIComponent(results[1].replace(/\+/g, " ")).toLowerCase();
        }
    };

    var matchVideo = function () {
        var id;
        switch (jq(".choose-module-video iframe").attr("src")) {
            case projVideo:
                id = "video-guides-3";
                break;
            case peopleVideo:
                id = "video-guides-5";
                break;
            case docsVideo:
                id = "video-guides-2";
                break;
            case crmVideo:
                id = "video-guides-4";
                break;
            case arVideo:
                id = "video-guides-1";
                break;
            default:
                return;
        }
        AjaxPro.timeoutPeriod = 1800000;
        UserVideoGuideUsage.SaveWatchVideo([id]);
    };

    return {
        init: init
    };
})();

jq(document).ready(function () {
    ASC.Controls.StartSteps.init();
});