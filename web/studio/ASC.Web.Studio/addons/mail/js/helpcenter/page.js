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


window.helpPage = (function($) {
    var isInit = false,
        page;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            page = $('#helpPanel');
        }
    };

    var show = function(helpId) {
        var params = { helpId: helpId };

        if (!page.text().trim().length) {
            params.update = true;
            serviceManager.getHelpCenterHtml(params, { success: onGetHelpCenterHtml }, ASC.Resources.Master.Resource.LoadingProcessing);
        } else {
            onGetHelpCenterHtml(params, null);
        }
    };

    var onGetHelpCenterHtml = function(params, html) {
        if (params.update) {
            page.html(html);
            messagePage.initImageZoom();
        }

        page.show();

        showHelpPage(params.helpId);
        mailBox.hideLoadingMask();
    };

    var hide = function() {
        page.hide();
    };

    return {
        init: init,
        show: show,
        hide: hide
    };
})(jQuery);