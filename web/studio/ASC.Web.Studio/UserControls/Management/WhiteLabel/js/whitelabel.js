/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


var WhiteLabelManager = new function () {
    var _successMsgTest = "";
    var _uploadWhiteLabelLogoComplete = function (file, response, logotype) {
        //jq.unblockUI();

        var result = eval("(" + response + ")");
        if (result.Success) {
            jq('#studio_whiteLabelSettings .logo_' + logotype).attr('src', result.Message);
            jq("canvas[id^=canvas_logo_" + logotype + "]").hide();
            jq('#logoPath_' + logotype).val(result.Message);
        } else {
            toastr.error(result.Message);
        }
        LoadingBanner.hideLoaderBtn("#studio_whiteLabelSettings");
    };

    var _updateWhiteLabelLogosSrc = function (whiteLabelLogos) {
        for (var logo in whiteLabelLogos) {
            if (whiteLabelLogos.hasOwnProperty(logo)) {
                var now = new Date();
                jq('#studio_whiteLabelSettings .logo_' + logo).attr('src', whiteLabelLogos[logo] + '?' + now.getTime());
            }
        }
    }

    this.SaveWhiteLabelOptions = function () {
        var logoList = [],

            $logoPaths = jq('[id^=logoPath_]'),
            needToSave = false;

        for (var i = 0, n = $logoPaths.length; i < n; i++) {
            var logotype = jq($logoPaths[i]).attr('id').split('_')[1],
                logoPath = jq.trim(jq($logoPaths[i]).val());

            logoList.push({ key: logotype, value: logoPath });

            if (logoPath != "") { needToSave = true; }
        }

        if (jq("#studio_whiteLabelLogoText").val() !== jq("#studio_whiteLabelLogoText").attr("data-value")) { needToSave = true; }

        if (needToSave) {
            Teamlab.saveWhiteLabelSettings({},
             {
                 logoText: jq("#studio_whiteLabelLogoText").val(),
                 logo: logoList
             },
             {
                 before: function (params) { LoadingBanner.showLoaderBtn("#studio_whiteLabelSettings"); },
                 after: function (params) { LoadingBanner.hideLoaderBtn("#studio_whiteLabelSettings"); },
                 success: function (params, response) {
                     //clean logo path input
                     jq('[id^=logoPath_]').val('');

                     window.location.reload(true);
                     LoadingBanner.showMesInfoBtn("#studio_whiteLabelSettings", _successMsgTest, "success");
                 },
                 error: function (params, errors) {
                     var err = errors[0];
                     LoadingBanner.showMesInfoBtn("#studio_whiteLabelSettings", err, "error");

                 }
             });
        }
    };

    this.RestoreWhiteLabelOptions = function () {

        Teamlab.restoreWhiteLabelSettings({},
             {
                 before: function (params) { LoadingBanner.showLoaderBtn("#studio_whiteLabelSettings"); },
                 after: function (params) { LoadingBanner.hideLoaderBtn("#studio_whiteLabelSettings"); },
                 success: function (params, response) {
                     //clean logo path input
                     jq('[id^=logoPath_]').val('');

                     window.location.reload(true);
                     LoadingBanner.showMesInfoBtn("#studio_whiteLabelSettings", _successMsgTest, "success");
                 },
                 error: function (params, errors) {
                     var err = errors[0];
                     LoadingBanner.showMesInfoBtn("#studio_whiteLabelSettings", err, "error");
                 }
             });
    };

    this.UseTextAsLogo = function () {

        var $canvas = jq("[id^=canvas_logo_]"),
            text = jq("#studio_whiteLabelLogoText").val();
        
        for (var i = 0, n = $canvas.length; i < n; i++) {
            var cnv = $canvas[i],
                $c = jq(cnv),
                fontsize = $c.attr("data-fontsize"),
                fontcolor = $c.attr("data-fontcolor"),
                logotype = $c.attr("id").replace("canvas_logo_", ""),
                x = logotype == 3 ? cnv.width / 2 : 0,
                firstChar = jq.trim(text).charAt(0),
                firstCharCode = firstChar.charCodeAt(0),
                ctx = cnv.getContext("2d");

            if (logotype.indexOf('_') !== -1) logotype = logotype.split('_')[0]; // for docs editor

            if (firstCharCode >= 0xD800 && firstCharCode <= 0xDBFF) firstChar = jq.trim(text).substr(0, 2); // Note: for surrogates pairs only
            
            ctx.fillStyle = "transparent";
            ctx.clearRect(0, 0, cnv.width, cnv.height);
            ctx.fillStyle = fontcolor;
            ctx.textAlign = logotype == 3 ? "center" : "start";
            ctx.textBaseline = "top";

            ctx.font = fontsize + "px Arial";

            ctx.fillText(logotype == 3 ? firstChar : text, x, (cnv.height - parseInt(fontsize)) / 2);

            var img = cnv.toDataURL("image/png", 1.0);
            jq('#logoPath_' + logotype).val(img);

            $c.show();
        }
    };

    this.Init = function (successMsgTest) {
        _successMsgTest = successMsgTest;
        var isRetina = jq.cookies.get("is_retina");

        WhiteLabelManager.IsRetina = isRetina != null && isRetina == true;

        jq('#saveWhiteLabelSettingsBtn:not(.disable)').on("click", function () { WhiteLabelManager.SaveWhiteLabelOptions(); });
        jq('#restoreWhiteLabelSettingsBtn:not(.disable)').on("click", function () { WhiteLabelManager.RestoreWhiteLabelOptions(); });
        jq("#useAsLogoBtn").on("click", function () { WhiteLabelManager.UseTextAsLogo(); });

        var $uploaderBtns = jq('[id^=logoUploader_]');

        for (var i = 0, n = $uploaderBtns.length; i < n; i++) {
            var logotype = jq($uploaderBtns[i]).attr('id').split('_')[1];

            new AjaxUpload($uploaderBtns[i], {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.WhiteLabel.LogoUploader,ASC.Web.Studio',
                data: { logotype: logotype },
                onChange: function (file, ext) {
                    LoadingBanner.showLoaderBtn("#studio_whiteLabelSettings");
                },
                onComplete: function (file, response) {
                    _uploadWhiteLabelLogoComplete(file, response, this._settings.data.logotype);
                }
            });
        }
     
    };

};