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

var TimeAndLanguage = new function () {
    this.SaveLanguageTimeSettings = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_lngTimeSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_lngTimeSettings");
            }
        };
        var timeManager = new TimeAndLanguageContentManager();
        timeManager.SaveTimeLangSettings(function (res) {
            if (res.Status == "2") {
                LoadingBanner.showMesInfoBtn("#studio_lngTimeSettings", res.Message, "success");
            } else if (res.Status == "1") {
                window.location.reload(true);
            } else {
                LoadingBanner.showMesInfoBtn("#studio_lngTimeSettings", res.Message, "error");
            }
        });
    };
};

TimeAndLanguageContentManager = function () {
    this.SaveTimeLangSettings = function (parentCallback) {
        TimeAndLanguageSettingsController.SaveLanguageTimeSettings(jq("#studio_lng").val(), jq("#studio_timezone").val(), function (result) {
            if (parentCallback != null)
                parentCallback(result.value);

        });
    };
};