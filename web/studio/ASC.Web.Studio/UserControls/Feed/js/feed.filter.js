/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


var FeedFilter = (function() {
    var $ = jq;

    var firstLoad = true;
    function resolveFirstLoad() {
        if (firstLoad) {
            setFilterByCurrentHash();
            firstLoad = false;

            return true;
        }

        return false;
    }

    function changeHash(hashParam, hashValue) {
        var currentHash = getCurrentHash();
        var hash = hashValue ? changeHashParam(currentHash, hashParam, hashValue) : removeHashParam(hashParam, hashValue);

        ASC.Controls.AnchorController.move(hash);
        setFilterByCurrentHash();
    }

    function syncHashWithFilter($fltr) {
        var hash = getCurrentHash();
        var filterHash = getFilterData($fltr, "hash");

        if (hash != filterHash) {
            window.location.hash = filterHash;
            return true;
        }
        return false;
    }

    ASC.Controls.AnchorController.bind(/^(.+)*$/, onMovedHash);

    function onMovedHash() {
        syncNavWithFilter();
    }

    function setFilterByCurrentHash() {
        var product = getUrlParam(massNameFilters.product);

        var from = getUrlParam(massNameFilters.from);
        var to = getUrlParam(massNameFilters.to);

        var author = getUrlParam(massNameFilters.author);

        var text = decodeURIComponent(getUrlParam(massNameFilters.text));

        filters = [];

        // Product
        if (product.length > 0) {
            filters.push({ type: "combobox", id: product, isset: true, params: { value: product } });
        } else {
            filters.push({ type: "combobox", id: "community", reset: true });
            filters.push({ type: "combobox", id: "crm", reset: true });
            filters.push({ type: "combobox", id: "projects", reset: true });
            filters.push({ type: "combobox", id: "documents", reset: true });
        }

        //Distance
        if (from.length > 0 && to.length > 0) {
            filters.push({ type: "daterange", id: "distance", isset: true, params: { from: from, to: to } });
        } else {
            filters.push({ type: "daterange", id: "today", reset: true });
            filters.push({ type: "daterange", id: "currentweek", reset: true });
            filters.push({ type: "daterange", id: "currentmonth", reset: true });
        }

        //Author
        if (author.length > 0) {
            filters.push({ type: "person", id: massNameFilters.author, isset: true, params: { id: author } });
        } else {
            filters.push({ type: "person", id: massNameFilters.author, reset: true });
        }

        // Text
        if (text.length > 0) {
            filters.push({ type: "text", id: "text", isset: true, params: { value: text } });
        } else {
            filters.push({ type: "text", id: "text", reset: true, params: { value: null } });
        }
        
        $("#feed-filter").advansedFilter({ filters: filters, sorters: [] });
    }

    var massNameFilters = {
        product: "product",

        from: "from",
        to: "to",

        author: "author",

        text: "text"
    };

    function syncNavWithFilter() {
        var product = getUrlParam("product", getCurrentHash());
        if (!product) {
            $("#feed-menu-item .menu-sub-item").removeClass("active");
            $("#feed-menu-item").addClass("active");
        } else {
            var elem = $("#feed-" + product + "-product-nav").parent();
            elem.siblings().removeClass("active");
            elem.parents(".menu-item").removeClass("active");
            elem.addClass("active");
        }
    }

    function getUrlParam(name, str) {
        var regexS = "[#&]" + name + "=([^&]*)";
        var regex = new RegExp(regexS);
        var tmpUrl = "#";
        if (str) {
            tmpUrl += str;
        } else {
            tmpUrl += getCurrentHash();
        }
        var results = regex.exec(tmpUrl);
        if (results == null) {
            return "";
        } else {
            return results[1];
        }
    }


    function getFilter($container) {
        return getFilterData($container);
    }

    function getFilterData($container, type) {
        var data = {};
        var hash = "";
        var filters = $container.advansedFilter();

        for (var filterInd = 0; filterInd < filters.length; filterInd++) {
            switch (filters[filterInd].id) {
                case "community":
                case "crm":
                case "projects":
                case "documents":
                case "product":
                    var product = filters[filterInd].params.value;
                    data.product = product;
                    hash = changeHashParam(hash, "product", product);
                    break;
                case "today":
                case "currentweek":
                case "currentmonth":
                case "distance":
                    data.from = Teamlab.serializeTimestamp(new Date(filters[filterInd].params.from));
                    data.to = Teamlab.serializeTimestamp(new Date(filters[filterInd].params.to));
                    hash = changeHashParam(hash, "from", filters[filterInd].params.from);
                    hash = changeHashParam(hash, "to", filters[filterInd].params.to);
                    break;
                case "me":
                case "author":
                    data.author = filters[filterInd].params.id;
                    hash = changeHashParam(hash, "author", data.author);
                    break;
                case "text":
                    if (type == "hash") {
                        data.FilterValue = encodeURIComponent(filters[filterInd].params.value);
                    } else {
                        data.FilterValue = filters[filterInd].params.value;
                    }
                    hash = changeHashParam(hash, "text", data.FilterValue);
                    break;
            }
        }
        if (type == "hash") {
            return hash;
        } else {
            return data;
        }
    }

    function changeHashParam(paramsList, name, value) {
        if (hasParam(name, paramsList)) {
            var regex = new RegExp(name + "[=][0-9a-z\-]*");
            return paramsList.replace(regex, name + '=' + value);
        } else {
            return addParam(paramsList, name, value);
        }
    }

    function hasParam(paramName, url) {
        var regex = new RegExp('(\\#|&|^)' + paramName + '=', 'g');
        return regex.test(url);
    }

    function addParam(paramsList, name, value) {
        if (paramsList.length) {
            paramsList += '&';
        }
        paramsList = paramsList + name + '=' + value;
        return paramsList;
    }

    function removeHashParam(paramName, url) {
        var regex = new RegExp("[#&]" + paramName + "=([^&]*)");
        return url.replace(regex, '');
    }

    function getCurrentHash() {
        return ASC.Controls.AnchorController.getAnchor();
    }

    return {
        resolveFirstLoad: resolveFirstLoad,
        syncHashWithFilter: syncHashWithFilter,

        get: getFilter,

        changeHash: changeHash,

        onSetFilter: function() {
        },
        onResetFilter: function() {
        }
    };
});