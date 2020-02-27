/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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