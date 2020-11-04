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


StudioConfirm = new function() {
    this.OpenDialog = function(additionalID, callback) {
        jq("#studio_confirmMessage").html('');
        StudioBlockUIManager.blockUI("#studio_confirmDialog" + additionalID, 400);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = StudioConfirm.Select.bind(null, additionalID, callback);
        if (callback) {
            var conf = jq("#studio_confirmOk");
            conf.removeAttr("onclick");
            conf.off("click");
            conf.on("click", function () {
                callback();
            });
        }
    };
    this.Cancel = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };
    this.SelectCallback = function() {
        alert('empty callback');
    };
    this.Select = function(additionalID, callback) {
        callback(jq("#studio_confirmInput" + additionalID).val());
    };
}