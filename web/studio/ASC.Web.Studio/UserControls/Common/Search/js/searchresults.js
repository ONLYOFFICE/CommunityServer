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


var SearchResults = new function () {
    this._containerId = null;
    this._target = null;
    this.ShowAll = function (obj, control, product, item) {
        jq(obj).html('<img src=' + StudioManager.GetImage('loader_12.gif') + ' />');
        jq(obj).css('border-bottom', '0px');
        this._containerId = control;
        this._target = item;
        SearchController.GetAllData(product, jq("#searchTextHidden").val() || jq('#studio_search').val(), function (result) {
            SearchResults.ShowAllCallback(result.value);
        });
    };

    this.ShowAllCallback = function (result) {
        jq("#oper_" + SearchResults._target + " > span").remove();
        jq('#' + SearchResults._containerId).html(result);
    };

    this.Toggle = function (element, th) {
        var elem = jq('#' + element);
        if (elem.css('display') == 'none') {
            elem.css('display', 'block');
            jq('#' + th).removeClass("closed");
        } else {
            elem.css('display', 'none');
            jq('#' + th).addClass("closed");
        }
    };
};