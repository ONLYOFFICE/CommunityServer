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


NamingPeopleManager = new function() {


    this.SaveSchema = function() {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_namingPeople");
            else
                LoadingBanner.hideLoaderBtn("#studio_namingPeople");
        };

        var namingContentManager = new NamingPeopleContentManager();
        namingContentManager.SaveSchema(NamingPeopleManager.SaveSchemaCallback);
    }
    this.SaveSchemaCallback = function (result) {
        LoadingBanner.showMesInfoBtn("#studio_namingPeople", result.Message, result.Status == 1 ? "success" : "error");
    }
};

jq(function() {
    var namingContentManager = new NamingPeopleContentManager();
    jq('#saveNamingPeopleBtn').click(NamingPeopleManager.SaveSchema);   
});