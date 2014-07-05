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


;window.TeamlabMobile = (function() {
    var 
    isInit = false,
    pageItems = 15,
    historykeyname = 'history_steps',
    localStorage = window.localStorage || {},
    historyManager = [],
    historyManagerSeparator = ' , ',
    maxDurationHistory = 24 * 60 * 60 * 1000,
    userHandlers = {},
    prevExtensions = [],
    systemConstants = {
        pageItems: 15,
        maxUploadSize: 1024 * 1024 * 10
    },
    templateIds = {
        lbcomments: 'template-comments',
        lbcommthreads: 'template-commthreads',
        lbprojmilestones: 'template-projmilestones',
        lbprojteam: 'template-projteam',
        lbtasks: 'template-proj-tasks',
        page: 'template-page',
        pgauth: 'template-page-auth',
        pgsearch: 'template-page-search',
        pgdefault: 'template-page-default',
        pgrewrite: 'template-page-rewrite',
        pgexception: 'template-exception',
        pgaddcomment: 'template-addcomment'
    },
    staticAnchors = {
        index: '#',
        auth: 'auth',
        rewrite: 'rewrite',
        search: 'search/'
    },
    anchorRegExp = {
        index: null,
        facebookRedirect: /^_=_$/,
        auth: /^auth[\/]*$/,
        rewrite: /^rewrite[\/]*(.+)/,
        search: /^search\/(.+)/,
        active: /^crm\/active[\/]*$/,
        cmt: /^community/,
        c_r_m: /^crm/,
        prj: /^projects/,
        files: /^docs/,
        profiles: /^people/
        
    },
    customEvents = {
        getException: 'ongetexception',
        changePage: 'onchangepage',
        authPage: 'onauthpage',
        rewritePage: 'onrewritepage',
        indexPage: 'onindexpage',
        searchPage: 'onsearchpage',
        addComment: 'onaddcomment',
        loadComments: 'onloadcomments'
    },
    eventManager = new CustomEvent(customEvents),
    dialogMarkCollection = [];

    var templateData = {
        index: {
            products: [
        { id: 'ea942538-e68e-4907-9394-035336ee0ba8', title: ASC.Resources.LblCommunityTitle, link: 'community', icon: 'images/blank.gif', classname: 'community', enabled: true },
        { id: 'f4d98afd-d336-4332-8778-3c6945c81ea0', title: ASC.Resources.LblPeopleTitle, link: 'people', icon: 'images/blank.gif', classname: 'people', enabled: true },
        { id: '1e044602-43b5-4d79-82f3-fd6208a11960', title: ASC.Resources.LblProjectsTitle, link: 'projects', icon: 'images/blank.gif', classname: 'projects', enabled: true },
        { id: 'e67be73d-f9ae-4ce1-8fec-1880cb518cb4', title: ASC.Resources.LblDocumentsTitle, link: 'docs', icon: 'images/blank.gif', classname: 'docs', enabled: true },
        { id: '6743007c-6f95-4d20-8c88-a8601ce5e76d', title: ASC.Resources.LblCrmTitle, link: 'crm', icon: 'images/blank.gif', classname: 'crm', enabled: true }
      ]
        },
        auth: {
            domains: ['avsmedia.net', 'avs4you.com', 'avsmedia.net', 'nctsoft.com']
        }
    };

    ServiceManager.bind('extention', onGetException);

    function validSession() {
        return true;
    };

    function fixUrl(url) {
        if (!url) {
            return '';
        }
        if (url.indexOf('://') === -1) {
            if (url.charAt(0) !== '/') {
                url = '/' + url;
            }
            url = [location.protocol, '//', location.hostname, location.port ? ':' + location.port : '', url].join('');
        }
        return url;
    }

    function trim(s) {
        if (typeof s != 'string' || s.length == 0) {
            return '';
        }
        return s.replace(/^[\n\s]+|[\n\s]+$/g, '');
    }

    function verificationDate(date) {
        try {
            if (date) {
                return Teamlab.serializeTimestamp(new Date(date));
            }
        } catch (err) {
            return null;
        }
        return date;
    }

    function verificationValue(val) {
        if (typeof val !== 'boolean' && typeof val !== 'number' && typeof val !== 'string') {
            return null;
        }
        if (val == -1 || typeof val === 'string' && trim(val).length === 0) {
            return null;
        }

        return val;
    }

    function getItemTarget(item) {
        var target = 'self';
        if (item.isSupported === true) {
            target = 'blank';
            switch (item.filetype) {
                case 'image':
                    target = 'self';
                    break;
            }
        }
        return target;
    }

    function getViewUrl(item) {
        var href = [userHandlers.fileview, encodeURIComponent(item.folderId), '/', Base64.encode("" + item.id), '/', item.version || 0, '/', Base64.encode(item.title || '')].join('');

        if (href) {

            switch (item.filetype) {
                case 'image':
                    href = item.viewUrl || '';
                    break;
            }
        }

        return href;
    }

    var renderScripts = function(type) {
        var 
      node = null,
      nodesInd = 0,
      nodes = $('script[type="' + type + '"]').filter('[async="true"]');
        nodesInd = nodes.length;
        while (nodesInd--) {
            node = nodes[nodesInd];
            node.textContent = node.textContent.replace(/\/\//g, '');
        }
    };

    var preInit = function(DefaultParams) {
        DefaultParams = DefaultParams || ASC.DefaultParams;

        if (!DefaultParams || typeof DefaultParams !== 'object') {
            return undefined;
        }

        if (DefaultParams.hasOwnProperty('srchhelper')) {
            SrchHelper.init.apply(null, DefaultParams.srchhelper);
        }

        if (DefaultParams.hasOwnProperty('teamlab')) {
            TeamlabMobile.init.apply(null, DefaultParams.teamlab);
        }

        ASC.Controls.AnchorController.init();
    };

    var init = function(producticons, handlers) {
        if (isInit) {
            return undefined;
        }
        isInit = true;

        historyManager = localStorage[historykeyname] || '';
        historyManager = historyManager && typeof historyManager ? historyManager.split(historyManagerSeparator) : [];
        localStorage[historykeyname] = historyManager.join(historyManagerSeparator);

        var products = templateData.index.products,
            productsInd = products.length,
            productsIds = [];
        while (productsInd--) {
            if (products[productsInd].id !== null) {
                productsIds.unshift(products[productsInd].id);
                continue;
            }
        }
        Teamlab.getWebItemSecurityInfo({}, productsIds, { success: onGetInfo });

        for (var fld in producticons) {
            if (producticons.hasOwnProperty(fld)) {
                switch (fld.toLowerCase()) {
                    case 'community':
                        if (templateData.index.products[0]) {
                            if (typeof producticons[fld] === 'string') {
                                templateData.index.products[0].icon = producticons[fld];
                                continue;
                            }
                            if (producticons[fld] && typeof producticons[fld] === 'object' && producticons[fld].length > 0) {
                                templateData.index.products[0].icon = producticons[fld][0];
                                if (templateData.index.products[0].unavailable === true) {
                                    templateData.index.products[0].icon = producticons[fld][1];
                                }
                            }
                        }
                        break;
                    case 'people':
                        if (templateData.index.products[1]) {
                            if (typeof producticons[fld] === 'string') {
                                templateData.index.products[1].icon = producticons[fld];
                                continue;
                            }
                            if (producticons[fld] && typeof producticons[fld] === 'object' && producticons[fld].length > 0) {
                                templateData.index.products[1].icon = producticons[fld][0];
                                if (templateData.index.products[1].unavailable === true) {
                                    templateData.index.products[1].icon = producticons[fld][1];
                                }
                            }
                        }
                        break;
                    case 'projects':
                        if (templateData.index.products[2]) {
                            if (typeof producticons[fld] === 'string') {
                                templateData.index.products[2].icon = producticons[fld];
                                continue;
                            }
                            if (producticons[fld] && typeof producticons[fld] === 'object' && producticons[fld].length > 0) {
                                templateData.index.products[2].icon = producticons[fld][0];
                                if (templateData.index.products[2].unavailable === true) {
                                    templateData.index.products[2].icon = producticons[fld][1];
                                }
                            }
                        }
                        break;
                    case 'documents':
                        if (templateData.index.products[3]) {
                            if (typeof producticons[fld] === 'string') {
                                templateData.index.products[3].icon = producticons[fld];
                                continue;
                            }
                            if (producticons[fld] && typeof producticons[fld] === 'object' && producticons[fld].length > 0) {
                                templateData.index.products[3].icon = producticons[fld][0];
                                if (templateData.index.products[3].unavailable === true) {
                                    templateData.index.products[3].icon = producticons[fld][1];
                                }
                            }
                        }
                        break;
                    case 'crm':
                        if (templateData.index.products[4]) {
                            if (typeof producticons[fld] === 'string') {
                                templateData.index.products[4].icon = producticons[fld];
                                continue;
                            }
                            if (producticons[fld] && typeof producticons[fld] === 'object' && producticons[fld].length > 0) {
                                templateData.index.products[4].icon = producticons[fld][0];
                                if (templateData.index.products[4].unavailable === true) {
                                    templateData.index.products[4].icon = producticons[fld][1];
                                }
                            }
                        }
                        break;
                }
            }
        }

        if (handlers) {
            var fileview = handlers.fileview;
            fileview = fileview.charAt(fileview.length - 1) === '/' ? fileview : fileview + '/';
            if (fileview.indexOf('://') === -1) {
                if (fileview.charAt(0) !== '/') {
                    fileview = '/' + fileview;
                }
                fileview = [location.protocol, '//', location.hostname, location.port ? ':' + location.port : '', fileview].join('');
            }
            userHandlers.fileview = fileview;
        }

        ASC.Controls.AnchorController.bind(onAnch);
        ASC.Controls.AnchorController.bind(anchorRegExp.facebookRedirect, function () { ASC.Controls.AnchorController.move(staticAnchors.index); });
        ASC.Controls.AnchorController.bind(anchorRegExp.index, onIndexAnch);
        ASC.Controls.AnchorController.bind(anchorRegExp.search, onSearchAnch);
        ASC.Controls.AnchorController.bind(anchorRegExp.auth, onAuthAnch);
        ASC.Controls.AnchorController.bind(anchorRegExp.rewrite, onRewriteAnch);

        SrchHelper.bind(SrchHelper.events.found, onFound);
    };

    var update = function(params) {
        if (!params) {
            var params = {}, item = null, search = '', searchInd = 0;
            search = (location.search.charAt(0) === '?' ? location.search.substring(1) : location.search).split('&');
            searchInd = search.length;
            while (searchInd--) {
                item = search[searchInd].split('=');
                if (item.length === 2) {
                    params[item[0]] = decodeURIComponent(item[1]);
                }
            }
        }
    };

    function onGetException(eventname, params, errors) {
        eventManager.call(customEvents.getException, window, [eventname, eventname, errors[0], errors[0], params]);
    }

    function onFound(items, query, params) {
        var itemsInd = items.length, el = null, els = null, elsInd = 0, elitem = null, elitems = null, elitemsInd = 0, parent = null, href = '', producttype = '';
        while (itemsInd--) {
            producttype = items[itemsInd].type;
            items[itemsInd].classname = items[itemsInd].type;
            els = items[itemsInd].items;
            elsInd = els ? els.length : 0;
            while (elsInd--) {
                el = els[elsInd];
                if (producttype === "crmitems") {
                    el.classname = el.contactclass;
                    el.additioninfo = el.title;
                    el.title = el.displayName;
                } else {
                    el.classname = el.classname || el.type;
                }
                el.producttype = producttype;
                el.href = staticAnchors.hasOwnProperty(el.itemtype || el.type) ? staticAnchors[el.itemtype || el.type] + el.id : null;
                if (el.items) {
                    elitems = el.items;
                    elitemsInd = elitems ? elitems.length : 0;
                    while (elitemsInd--) {
                        elitem = elitems[elitemsInd];
                        elitem.classname = elitem.type;
                        elitem.href = elitem.href ? elitem.href : (staticAnchors.hasOwnProperty(elitem.type) ? '#' + staticAnchors[elitem.type] + elitem.id : null);
                        elitem.entryTitle = el.title;
                    }
                }
            }
        }

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.search)) {
            eventManager.call(customEvents.searchPage, window, [items, query, params]);
        }
    }

    function onGetSecurityInfo(params, items) {
        var 
      itemsInd = 0,
      products = templateData.index.products,
      productsInd = 0,
      enabledProducts = [];

        productsInd = products.length;
        while (productsInd--) {
            if (products[productsInd].id === null) {
                enabledProducts.unshift(products[productsInd]);
                continue;
            }
            itemsInd = items.length;
            while (itemsInd--) {
                if (items[itemsInd].isSubItem === false) {
                    if (products[productsInd].id == items[itemsInd].webItemId && items[itemsInd].enabled === true) {
                        enabledProducts.unshift(products[productsInd]);
                    }
                }
            }
        }

        eventManager.call(customEvents.indexPage, window, [{ products: enabledProducts}]);
    }

    function onAnch(anchor) {
        if (!window.pageTracker && window._gat) {
            try { window.pageTracker = _gat._getTracker('UA-12442749-7') } catch (err) { }
        }

        if (window.pageTracker) {
            try { window.pageTracker._trackPageview(anchor) } catch (err) { }
        }
    }

    function onGetInfo(params, products) {
        var productsall = templateData.index.products,
            name,
            redirect;

        for (var i = 0; i < productsall.length; i++) {
            for (var j = 0; j < products.length; j++) {
                if (productsall[i].id == products[j].webItemId && products[j].enabled === false) {
                    name = productsall[i].classname;
                    switch (name) {
                        case "comunity":
                            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt)) {
                                redirect = true;
                            }
                            break;
                        case "crm":
                            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.c_r_m)) {
                                redirect = true;
                            }
                            break;
                        case "docs":
                            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.files)) {
                                redirect = true;
                            }
                            break;
                        case "people":
                            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.profiles)) {
                                redirect = true;
                            }
                            break;
                        case "projects":
                            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj)) {
                                redirect = true;
                            }
                            break;
                    }
                    if (redirect) {
                        document.location.href = "./";
                    }
                }
            }
        }
    }

    function onAuthAnch(params) {
        if (!validSession() && false) {
            ASC.Controls.AnchorController.move(staticAnchors.index);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.authPage, window, [templateData.auth]);
    }

    function onRewriteAnch(params, url) {
        if (!validSession() && false) {
            ASC.Controls.AnchorController.move(staticAnchors.index);
            return undefined;
        }

        url = typeof url === 'string' && url.length > 0 ? url : null;
        if (url) {
            var flag = 'asc_nomobile=1';
            //url = url.charAt(0) !== '/' ? '/' + url : url;
            url = url.indexOf('://') === -1 ? location.protocol + '//' + url : url;
            if (url.indexOf(flag) === -1) {
                url = url.indexOf('?') === -1 ? url + '?' + flag : url + '&' + flag;
            }
            url = url.replace('/mobile', '');
        }

        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.rewritePage, window, [{ url: url}]);
    }

    function onIndexAnch(params) {
        if (!validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        Teamlab.getWebItemSecurityInfo(null, null, onGetSecurityInfo);
    }

    function onSearchAnch(params, query) {
        if (!validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        query = query ? decodeURI(Base64.decode(query)) : '';
        if (typeof query === 'string' && query.length > 0) {
            eventManager.call(customEvents.changePage, window, []);
            SrchHelper.search(query);
        }
    }

    var bind = function(eventName, handler, params) {
        return eventManager.bind(eventName, handler, params);
    };

    var unbind = function(handlerId) {
        return eventManager.unbind(handlerId);
    };

    var call = function(eventName, self, args) {
        eventManager.call(eventName, self, args);
    };

    var extendEventManager = function(events) {
        for (var fld in events) {
            if (events.hasOwnProperty(fld)) {
                customEvents[fld] = events[fld];
            }
        }
        eventManager.extend(customEvents);
        return eventManager;
    };

    var extendModule = function(templates, anchors, staticanchors, dialogcollection) {
        if (templates && typeof templates === 'object') {
            for (var fld in templates) {
                if (templates.hasOwnProperty(fld)) {
                    templateIds[fld] = templates[fld];
                }
            }
        }

        if (anchors && typeof anchors === 'object') {
            for (var fld in anchors) {
                if (anchors.hasOwnProperty(fld)) {
                    anchorRegExp[fld] = anchors[fld];
                }
            }
        }

        if (staticanchors && typeof staticanchors === 'object') {
            for (var fld in staticanchors) {
                if (staticanchors.hasOwnProperty(fld)) {
                    staticAnchors[fld] = staticanchors[fld];
                }
            }
        }

        if (dialogcollection && typeof dialogcollection === 'object') {
            for (var i = 0, n = dialogcollection.length; i < n; i++) {
                dialogMarkCollection.push(dialogcollection[i]);
            }
        }
    };

    var dscSortByDate = function(a, b) {
        return b.timestamp - a.timestamp;
    };

    var ascSortById = function(a, b) {
        return a.id - b.id;
    };

    var dscSortById = function(a, b) {
        return b.id - a.id;
    };

    var ascSortByIndex = function(a, b) {
        if (!a.index) {
            return 1;
        }
        if (!b.index) {
            return -1;
        }
        return a.index === b.index ? 0 : a.index > b.index ? 1 : -1;
    };

    var ascSortByTitle = function(a, b) {
        if (!a.title) {
            return 1;
        }
        if (!b.title) {
            return -1;
        }
        return a.title === b.title ? 0 : a.title > b.title ? 1 : -1;
    };

    var ascSortByLowTitle = function(a, b) {
        if (!a.lowTitle) {
            return 1;
        }
        if (!b.lowTitle) {
            return -1;
        }
        return a.lowTitle === b.lowTitle ? 0 : a.lowTitle > b.lowTitle ? 1 : -1;
    };

    var search = function(query) {
        if (typeof query === 'string' && query.length > 0) {
            ASC.Controls.AnchorController.move(staticAnchors.search + Base64.encode(encodeURI(query)));
        }
    };

    var throwException = function(params, errors) {
        eventManager.call(customEvents.getException, window, ['none', 'none', errors[0], errors[0], params]);
    }

    var resetFocus = function() {
        var scrollTop = document.documentElement.scrollTop, o = document.createElement('input');
        o.style.border = '0';
        o.style.background = 'transparent';
        o.style.position = 'absolute';
        o.style.left = '-1000px';
        o.style.top = '-1000px';
        document.body.appendChild(o);
        o.setAttribute('type', 'text');
        o.focus();
        o.parentNode.removeChild(o);
        document.documentElement.scrollTop = scrollTop;
    };

    var showDialog = function(hash) {
        hash = hash.charAt(0) === '#' ? hash.substring(1) : hash;

        var params = [];
        for (var i = 0, n = dialogMarkCollection.length; i < n; i++) {
            if (dialogMarkCollection[i].regexp === null) {
                if (hash.length === 0) {
                    eventManager.call(dialogMarkCollection[i].evt, window, []);
                }
                continue;
            }
            if ((params = dialogMarkCollection[i].regexp.exec(hash)) !== null) {
                // TODO: place for do somthing with parameters string
                params = params.length > 1 ? params.slice(1) : [];
                eventManager.call(dialogMarkCollection[i].evt, window, params);
            }
        }
    };

    var saveHistoryPage = function() {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        if (anchor != historyManager[0]) {
            historyManager.unshift(anchor);
        }
        if (!anchor || anchor === '#') {
            historyManager = [];
        }
        localStorage[historykeyname] = historyManager.join(historyManagerSeparator);

        //anchor ? console.log('saveHistoryPage ', anchor) : console.log('reset history');
    };

    var getPrevHistoryStep = function() {
        return historyManager[1] || historyManager[0] || '#';
    };

    var goPrevHistoryStep = function(noneshift) {
        if (noneshift !== true) {
            historyManager.shift();
        }
        ASC.Controls.AnchorController.lazymove(historyManager[0] || '#');
    };

    var getFolderItem = function(item) {
        item.classname = item.type + (item.filetype ? ' ' + item.filetype : '') + (item.isThirdParty === true ? ' third-party' : '');
        var href = TeamlabMobile.getViewUrl(item);
        if (href) {
            item.target = TeamlabMobile.getItemTarget(item);
        }

        if (item.target === 'blank') {
            item.href = href;
        } else {
            item.href = item.type === 'file' ? '#file/' + item.id : '#folder/' + item.id;
        }

        return item;
    };

    var getFolderItems = function(entry) {
        var 
      href = '',
      item = null,
      itemsInd = 0,
      items = [].concat(entry.folders).concat(entry.files);

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            item = items[itemsInd];

            item = TeamlabMobile.getFolderItem(item);
        }
        return items || [];
    };
    
    var htmlEncodeLight =  function(string) {
        var newStr = string.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>').replace('&amp;', '&');
        return newStr;
    };

    return {
        constants: systemConstants,
        anchors: staticAnchors,
        events: customEvents,
        regexps: anchorRegExp,
        templates: templateIds,
        dateFormats: Teamlab.constants.dateFormats,
        nameCollections: Teamlab.constants.nameCollections,

        renderScripts: renderScripts,
        preInit: preInit,
        init: init,
        update: update,

        bind: bind,
        unbind: unbind,
        call: call,
        extendEventManager: extendEventManager,
        extendModule: extendModule,

        dscSortByDate: dscSortByDate,
        ascSortById: ascSortById,
        dscSortById: dscSortById,
        ascSortByIndex: ascSortByIndex,
        ascSortByTitle: ascSortByTitle,
        ascSortByLowTitle: ascSortByLowTitle,

        search: search,

        throwException: throwException,

        validSession: validSession,
        verificationDate: verificationDate,
        verificationValue: verificationValue,
        getItemTarget: getItemTarget,
        getViewUrl: getViewUrl,

        resetFocus: resetFocus,

        showDialog: showDialog,

        getFolderItem: getFolderItem,
        getFolderItems: getFolderItems,

        saveHistoryPage: saveHistoryPage,
        getPrevHistoryStep: getPrevHistoryStep,
        goPrevHistoryStep: goPrevHistoryStep,
        
        htmlEncodeLight: htmlEncodeLight
    };
})();
