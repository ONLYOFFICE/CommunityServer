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

window.TMContainers = (function($) {

    function IdMap() {

        var _map = {};

        this.AddId = function(messageId, what) {
            _map[messageId] = what;
        }

        this.RemoveId = function(messageId) {
            delete _map[messageId];
        }

        this.RemoveIds = function(messageIds) {
            for (var i = 0; i < messageIds.length; i++) {
                var id = messageIds[i];
                delete _map[id];
            }
        }

        this.GetIds = function() {
            var result = [];
            for (var prop in _map) {
                result.push(prop);
            }
            return result;
        }

        this.GetValues = function() {
            var result = [];
            for (var id in _map) {
                result.push(_map[id]);
            }
            return result;
        }

        this.Count = function() {
            var num_props = 0;
            for (var prop in _map) {
                num_props++;
            }
            return num_props;
        };

        this.Clear = function() {
            _map = {};
        };

        this.Each = function(callback) {
            if (callback === undefined) return;

            $.each(_map, callback);
        };

        this.HasId = function(id) {
            return _map.hasOwnProperty(id);
        };

    }

    function StringSet() {

        var values = [];

        this.Add = function(value) {
            if (!this.Has(value))
                values.push(value);
        }

        this.GetValues = function() {
            return values;
        }

        this.Count = function() {
            return values.length;
        };

        this.Clear = function() {
            values = [];
        };

        this.Each = function(callback) {
            if (callback === undefined) return;

            var count = this.Count();
            for (var i = 0; i < count; i++) {
                var ret = callback(values[i]);
                if (ret === false) break;
            }
        };

        this.Has = function(value) {
            var count = this.Count();
            for (var i = 0; i < count; i++) {
                if (values[i] === value) return true;
            }
            return false;
        };

    }


    return {
        IdMap: IdMap,
        StringSet: StringSet
    };

})(jQuery);
