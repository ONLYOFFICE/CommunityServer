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
jq(function () {
    if (jq('#studio_logoUploader').length > 0) {
        new AjaxUpload('studio_logoUploader', {
            action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.LogoUploader,ASC.Web.Studio',
            onChange: function (file, ext) {
                LoadingBanner.showLoaderBtn("#studio_greetingSettings");
            },
            onComplete: GreetingSettingsUploadManager.SaveGreetingLogo
        });
    }
});

var GreetingSettingsUploadManager = new function() {
    this.BeforeEvent = null;
    this.SaveGreetingLogo = function (file, response) {
        var GreetManager1 = new GreetingSettingsContentManager();
        GreetManager1.SaveGreetingLogo(file, response);
    };
};

var GreetingSettingsContentManager = function() {

    this.OnAfterSaveData = null;

    this.SaveGreetingLogo = function(file, response) {
        LoadingBanner.hideLoaderBtn("#studio_greetingSettings");

        var result = eval("(" + response + ")");

        if (result.Success) {
            jq('#studio_greetingLogo').attr('src', result.Message);
            jq('#studio_greetingLogoPath').val(result.Message);
        } else {
            if (this.OnAfterSaveData != null) {
                this.OnAfterSaveData(result);
            } else {
                LoadingBanner.showMesInfoBtn("#studio_greetingSettings", result.Message, "error");
            }
        }
    };

    this.SaveGreetingOptions = function(parentCallback) {
        GreetingSettingsController.SaveGreetingSettings(jq('#studio_greetingLogoPath').val(),
                                                jq('#studio_greetingHeader').val(),
                                                function(result) {
                                                    //clean logo path input
                                                    jq('#studio_greetingLogoPath').val('');
                                                    if (parentCallback != null)
                                                        parentCallback(result.value);
                                                });
    };

    this.RestoreGreetingOptions = function(parentCallback) {
        GreetingSettingsController.RestoreGreetingSettings(function(result) {
            //clean logo path input
            jq('#studio_greetingLogoPath').val('');

            if (result.value.Status == 1) {
                jq('#studio_greetingHeader').val(result.value.CompanyName);
                jq('#studio_greetingLogo').attr('src', result.value.LogoPath);
            }

            if (parentCallback != null) {
                parentCallback(result.value);
            }
        });
    };
}