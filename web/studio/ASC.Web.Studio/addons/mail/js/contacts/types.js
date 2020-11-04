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


window.contactStages = (function($) {
    var initFlag = false;
    var stages = [];
    var events = $({});
    var init = function() {
        if (!initFlag) {
            initFlag = true;
            update();
        }
    };

    var onGetStages = function(params, contactStages) {
        stages = [{ id: 0, title: MailScriptResource.FilterContactStageNotSpecified, color: '#ffffff' }];
        $.each(contactStages, function(index, value) {
            stages.push({ id: value.id, title: value.title, color: value.color });
        });
        events.trigger('update');
    };

    var update = function() {
        serviceManager.getCrmContactStatus({}, { success: onGetStages });
    };

    var getStages = function() {
        return stages;
    };

    return {
        init: init,
        update: update,
        getStages: getStages,
        events: events
    };
})(jQuery);