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


window.TMContainers = (function($) {

    function idMap() {

        var map = {};

        this.AddId = function(messageId, what) {
            map[messageId] = what;
        };

        this.RemoveId = function(messageId) {
            delete map[messageId];
        };

        this.RemoveIds = function(messageIds) {
            for (var i = 0; i < messageIds.length; i++) {
                var id = messageIds[i];
                delete map[id];
            }
        };

        this.GetIds = function() {
            var result = [];
            for (var prop in map) {
                result.push(prop);
            }
            return result;
        };

        this.GetValues = function() {
            var result = [];
            for (var id in map) {
                result.push(map[id]);
            }
            return result;
        };

        this.Count = function() {
            var numProps = 0;
            for (var prop in map) {
                numProps++;
            }
            return numProps;
        };

        this.Clear = function() {
            map = {};
        };

        this.Each = function(callback) {
            if (callback === undefined) {
                return;
            }

            $.each(map, callback);
        };

        this.HasId = function(id) {
            return map.hasOwnProperty(id);
        };

    }

    function stringSet() {

        var values = [];

        this.Add = function(value) {
            if (!this.Has(value)) {
                values.push(value);
            }
        };

        this.GetValues = function() {
            return values;
        };

        this.Count = function() {
            return values.length;
        };

        this.Clear = function() {
            values = [];
        };

        this.Each = function(callback) {
            if (callback === undefined) {
                return;
            }

            var count = this.Count();
            for (var i = 0; i < count; i++) {
                var ret = callback(values[i]);
                if (ret === false) {
                    break;
                }
            }
        };

        this.Has = function(value) {
            var count = this.Count();
            for (var i = 0; i < count; i++) {
                if (values[i] === value) {
                    return true;
                }
            }
            return false;
        };

    }

    return {
        IdMap: idMap,
        StringSet: stringSet
    };

})(jQuery);