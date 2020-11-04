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


(function ($, win, doc, body) {
    var
      defaultAnykeyTimeout = 500,
      currentHash = '',
      cmplClassName = 'advansed-filter-complete',
      templates = {
          filterContainer: 'template-filter-container',
          filterItem: 'template-filter-item'
      },
      isFirstUpdateLocaleStorage = true,
      lazyTrigger = false,
      filterContainer = null,
      filterInputKeyupHandler = 0,
      filterInputKeyupObject = null,
      filterInputKeyupTimeout = 0,
      Resources = {},
      filterValues = [
        { type: 'sorter', id: 'sorter', hashmask: 'sorter/{0}' },
        { type: 'text', id: 'text', hashmask: 'text/{0}' }
      ],
      sorterValues = [];

    var
      path = '/',
      parts = location.pathname.split('/');
    parts.splice(parts.length - 1, 1);
    path = parts.join('/');

    function isArray(o) {
        return o ? o.constructor.toString().indexOf("Array") != -1 : false;
    }

    function converText(str, toText) {
        str = typeof str === 'string' ? str : '';
        if (!str) {
            return '';
        }

        if (toText === true) {
            var
              symbols = [
                ['&lt;', '<'],
                ['&gt;', '>'],
                ['&and;', '\\^'],
                ['&sim;', '~'],
                ['&amp;', '&']
              ];

            var symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
            }
            return str;
        }

        var o = document.createElement('textarea');
        o.innerHTML = str;
        return o.value;
    }

    function format() {
        if (arguments.length === 0) {
            return '';
        }

        var pos = -1, str = arguments[0] || '', cnd = '', ind = -1, cnds = str.match(/{(\d+)}/g), cndsInd = cnds ? cnds.length : 0;
        while (cndsInd--) {
            pos = -1;
            cnd = cnds[cndsInd];
            ind = cnd.replace(/[{}]+/g, '');
            while ((pos = str.indexOf(cnd, pos + 1)) !== -1) {
                str = str.substring(0, pos) + (arguments[+ind + 1] || '') + str.substring(pos + cnd.length);
            }
        }
        return str;
    };

    function getFiltersHash(filtervalues) {
        var
          hash = [],
          filtervalue = null,
          filtervaluehash = '';

        for (var i = 0, n = filtervalues ? filtervalues.length : 0; i < n; i++) {
            filtervalue = filtervalues[i];
            
            if (!filtervalue.params) continue;

            switch (filtervalue.type) {
                case 'sorter':
                    filtervaluehash = {
                        id: filtervalue.id,
                        type: filtervalue.type,
                        params: {
                            id: filtervalue.params.id,
                            def: filtervalue.params.def,
                            dsc: filtervalue.params.dsc,
                            sortOrder: filtervalue.params.sortOrder
                        }
                    };
                    break;
                case 'daterange':
                    filtervaluehash = {
                        id: filtervalue.id,
                        type: filtervalue.type,
                        params: {
                            from: Teamlab.serializeTimestamp(new Date(filtervalue.params.from)),
                            to: Teamlab.serializeTimestamp(new Date(filtervalue.params.to))
                        }
                    };
                    break;
                default:
                    filtervaluehash = {
                        id: filtervalue.id,
                        type: filtervalue.type,
                        params: filtervalue.params
                    };
                    break;
            }

            filtervaluehash.params = $.base64.encode($.toJSON(filtervaluehash.params));
            hash.push($.toJSON(filtervaluehash));
        }

        return $.base64.encode(hash.join(';'));
    }

    function getkey() {
        var key = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + location.pathname + location.search;
        return encodeURIComponent(key.charAt(key.length - 1) === '/' ? key + 'Default.aspx' : key);
    }

    function toggleHasFilters(selectedfilters, $container) {
        if (selectedfilters.some(function (item) { return item.type != "sorter";})) {
            $container.addClass('has-filters');
        } else {
            $container.removeClass('has-filters');
        }
    }

    function updateLocalStorageFilters(opts, filtervalues) {
        if (isFirstUpdateLocaleStorage === true && (filtervalues.length === 0 || (filtervalues.length === 1 && filtervalues[0].id === 'sorter'))) {
            isFirstUpdateLocaleStorage = false;
            return undefined;
        }

        opts = opts && typeof opts === 'object' ? opts : {};
        isFirstUpdateLocaleStorage = false;

        var newhash = getFiltersHash(filtervalues);
        //currentHash = newhash;

        if (opts.inhash === true) {
            try { ASC.Controls.AnchorController.safemove(newhash) } catch (err) { }
        }

        // quota exceeded error workaround
        var key = getkey();
        localStorageManager.setItem(key, newhash);
    }

    function getLocalStorageFilters(opts) {
        var values = null;
        
        var key = getkey();
        if (localStorageManager.getItem(key)) {
            values = $.base64.decode(localStorageManager.getItem(key));
        }
        if (opts && typeof opts === 'object' && opts.inhash === true) {
            currentHash = location.hash;
            currentHash = currentHash && typeof currentHash === 'string' && currentHash.charAt(0) === '#' ? currentHash.substring(1) : currentHash;
            if (currentHash.length === 0) {
                currentHash = $.cookies.get(key);
                currentHash = decodeURIComponent(currentHash);
                currentHash = currentHash && typeof currentHash === 'string' && currentHash.charAt(0) === '#' ? currentHash.substring(1) : currentHash;
            }
            values = $.base64.decode(currentHash);
            if (currentHash && currentHash !== 'null') {
                try { ASC.Controls.AnchorController.safemove(currentHash) } catch (err) { }
                $.cookies.set(key, currentHash, { path: path });
            }
        }

        var
          params = null,
          filtervalue = null,
          filtervalues = [];

        values = typeof values === 'string' && values.length > 0 ? values.split(';') : [];
        for (var i = 0, n = values.length; i < n; i++) {
            try {
                filtervalue = $.parseJSON(values[i]);
                filtervalue.params = $.parseJSON($.base64.decode(filtervalue.params));
            } catch (err) {
                filtervalue = null;
            }
            if (filtervalue) {
                switch (filtervalue.type) {
                    case 'sorter':
                        filtervalues.push({ id: filtervalue.id, selected: true, params: filtervalue.params });
                        break;
                    case 'daterange':
                        filtervalues.push({ id: filtervalue.id, params: { from: Teamlab.serializeDate(filtervalue.params.from), to: Teamlab.serializeDate(filtervalue.params.to) } });
                        break;
                    default:
                        filtervalues.push({ id: filtervalue.id, params: filtervalue.params });
                        break;
                }
            }
        }

        return filtervalues || [];
    }

    function getContainerHash($container) {
        return getFiltersHash($container.data('filtervalues') || []);
    }

    function getStorageHash($container) {
        var key = getkey();
        return localStorageManager.getItem(key) || getContainerHash($container);
    }

    function getContainerFilters($container) {
        return $container.data('filtervalues') || [];
    }

    function getFilterValue(filtervalues, type) {
        var filtervaluesInd = filtervalues.length;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].type === type) {
                return filtervalues[filtervaluesInd];
            }
        }
        return null;
    }

    function extendItemValue(dst, src) {
        for (var srcInd in src) {
            if (src.hasOwnProperty(srcInd) && !dst.hasOwnProperty(srcInd)) {
                dst[srcInd] = src[srcInd];
            }
        }
    }

    function extendItemValues(dst, src) {
        var
          rslt = [],
          dstInd = 0, srcInd = 0;

        dstInd = dst.length;
        while (dstInd--) {
            srcInd = src.length;
            while (srcInd--) {
                if (dst[dstInd] && src[srcInd] && dst[dstInd].id === src[srcInd].id) {
                    extendItemValue(dst[dstInd], src[srcInd]);
                    break;
                }
            }
            rslt.unshift(dst[dstInd]);
        }

        srcInd = src.length;
        while (srcInd--) {
            dstInd = dst.length;
            while (dstInd--) {
                if (src[srcInd].id === dst[dstInd].id) {
                    break;
                }
            }
            rslt.unshift(src[srcInd]);
        }

        return rslt;
    }

    function getGroupSelectorName($container, id) {
        var
          groups = $container.find(".filter-item.filter-item-group > .selector-wrapper:first").data('items'),
          groupsInd = 0;
        groups = groups ? groups : [];
        groupsInd = groups.length;

        while (groupsInd--) {
            if (groups[groupsInd].id == id) {
                return groups[groupsInd].title;
            }
        }
        return id;
    }

    function getUserSelectorName($container, id, name) {
        var
          users = $container.find(".filter-item.filter-item-person > .selector-wrapper:first").data('items'),
          usersInd = 0;

        users = users ? users : [];
        usersInd = users.length;

        while (usersInd--) {
            if (users[usersInd].id == id) {
                return users[usersInd].title;
            }
        }

        return typeof name === 'string' && name ? name : id;
    }

    function addFilterToGroup(groups, filtervalue) {
        if (!filtervalue.title) {
            return undefined;
        }

        var groupsInd = 0;
        groupsInd = groups ? groups.length : 0;
        while (groupsInd--) {
            if (filtervalue.groupid) {
                if (groups[groupsInd].id === filtervalue.groupid) {
                    groups[groupsInd].items.push(filtervalue);
                    break;
                } else {
                    continue;
                }
            }

            if (groups[groupsInd].title === filtervalue.group) {
                groups[groupsInd].items.push(filtervalue);
                break;
            }
        }
        if (groupsInd === -1) {
            groups.push({ title: filtervalue.group || '', items: [filtervalue], id: filtervalue.groupid || '' });
        }
    }

    function createDatepicker($o, $container, $filteritem, filtervalue, defaultDate) {
        var $datepicker =
          $o
          .datepicker({
              onSelect: (function ($container, $filteritem, filtervalue, callback) {
                  return function (datetext, inst) {
                      callback(this, $container, $filteritem, filtervalue, datetext, inst);
                  }
              })($container, $filteritem, filtervalue, onUserFilterDateSelectValue)
          })
          .click(function (evt) {
              evt.stopPropagation();
          });

        $datepicker.datepicker('setDate', defaultDate);
        return $datepicker;
    }

    function createComboboxOptions(options) {
        var option = null, html = [];

        for (var i = 0, n = options.length; i < n; i++) {
            option = options[i];
            html = html.concat(['<option', ' class="' + option.classname + '"' + ' value="' + option.value + '"', option.def === true ? ' selected="selected"' : '', '>', converText(option.title, true), '</option>']);
        }

        return html.join('');
    }

    function createFilterItem(filtervalue) {
        var
          html = [],
          o = null, $o = null;

        $o = jQuery.tmpl(templates.filterItem, filtervalue);
        return $o;
    }

    function createAdvansedFilterGroup(opts, itemgroups, type) {
        var
          colcount = 1,
          html = [],
          items = null,
          itemvalues = null,
          title = '',
          itemgroupsInd = 0, itemgroupsLen = 0,
          itemvaluesInd = 0, itemvaluesLen = 0;

        colcount = opts && typeof opts === 'object' && opts.hasOwnProperty('colcount') && isFinite(+opts.colcount) ? +opts.colcount : colcount;
        for (itemgroupsInd = 0, itemgroupsLen = itemgroups.length; itemgroupsInd < itemgroupsLen; itemgroupsInd++) {
            if (type === 'filter' && colcount >= 1) {
                if (itemgroupsInd < colcount) {
                    html.push('<li class="item-group-col"><ul class="group-items">');
                    for (var i = itemgroupsInd; i < itemgroupsLen; i += colcount) {
                        itemvalues = itemgroups[i].items;
                        items = [];
                        for (itemvaluesInd = 0, itemvaluesLen = itemvalues.length; itemvaluesInd < itemvaluesLen; itemvaluesInd++) {
                            items.push(createAdvansedFilterItem(itemvalues[itemvaluesInd], type));
                        }
                        title = converText(itemgroups[i].title, true);

                        html = html.concat([
                          '<li',
                            ' class="item-group ' + type + '-group',
                              //itemgroups[i].title ? '' : ' none-title',
                            '"',
                          '>',
                            '<span class="title" title="' + title + '">',
                              title,
                            '</span>',
                            '<ul class="filter-items">',
                              items.join(''),
                            '</ul>',
                          '</li>'
                        ]);
                    }
                    html.push('</ul><div class="clear"></div></li>');
                }
                //html = html.concat([
                //  '<li',
                //    ' class="item-group ' + type + '-group',
                //      //itemgroupsInd === 0 ? ' first-group' : '',
                //      itemgroups[itemgroupsInd].title ? '' : ' none-title',
                //    '"',
                //  '>',
                //    '<span class="title" title="' + title + '">',
                //      title,
                //    '</span>',
                //    '<ul class="filter-items">',
                //      items.join(''),
                //    '</ul>',
                //  '</li>'
                //]);
            } else {
                itemvalues = itemgroups[itemgroupsInd].items;
                items = [];
                for (itemvaluesInd = 0, itemvaluesLen = itemvalues.length; itemvaluesInd < itemvaluesLen; itemvaluesInd++) {
                    items.push(createAdvansedFilterItem(itemvalues[itemvaluesInd], type));
                }

                title = converText(itemgroups[itemgroupsInd].title, true);
                html = html.concat([
                  '<li',
                    ' class="item-group ' + type + '-group',
                      //itemgroupsInd === 0 ? ' first-group' : '',
                      type === 'sorter' && !itemgroups[itemgroupsInd].title ? ' none-title' : '',
                    '"',
                  '>',
                    '<span class="title" title="' + title + '">',
                      title,
                    '</span>',
                  '</li>',
                  items.join('')
                ]
                );
            }
            //if (itemgroupsInd > 0 && itemgroupsLen > colcount && itemgroupsInd !== (itemgroupsLen - 1) && (itemgroupsInd + 1) % colcount === 0) {
            //  html.push('<li class="item-list-separator ' + type + '-list-separator"></li>')
            //}
        }
        html.push('<li class="item-list-separator ' + type + '-list-separator"></li>');
        return html.join('');
    }

    function createAdvansedFilterItem(itemvalue, type) {
        return [
          '<li',
            ' class="item-item ' + type + '-item',
              itemvalue.def === true ? ' selected' : '',
              itemvalue.def === true ? (itemvalue.dsc = !!itemvalue.dsc) === true ? ' dsc-sort' : ' asc-sort' : '',
              itemvalue.dsc === true || itemvalue.sortOrder === 'descending' ? ' dsc-sort-default' : '',
            '"',
            ' title="' + converText(itemvalue.title, true) + '"',
            ' data-id="' + itemvalue.id + '"',
            itemvalue.type ? ' data-type="' + itemvalue.type + '"' : '',
            itemvalue.hashmask ? ' data-anchor="' + itemvalue.hashmask + '"' : '',
          '>',
            '<span class="inner-text">',
              converText(itemvalue.title, true),
              //itemvalue.title,
            '</span>',
          '</li>'
        ].join('');
    };

    function createAdvansedFilter(filtervalues, sortervalues, opts) {
        var
          html = null,
          filters = [],
          sorters = [],
          o = null, $o = null;

        $o = jQuery.tmpl(templates.filterContainer, { filtervalues: filtervalues, sortervalues: sortervalues });
        if (!$o || $o.length != 1) return null;
        $o = $o[0];
        o = doc.createElement('div');
        o.className = 'clearFix';
        o.innerHTML = $o.outerHTML;
        return $o;
    }

    function updateAdvansedFilter(opts, $container, filtervalues, sortervalues) {
        var
          o = null,
          wasAdded = false,
          groups = [],
          groupsInd = 0,
          filtervalues = [].concat(filtervalues),
          sortervalues = [].concat(sortervalues),
          filtervaluesInd = 0, filtervaluesLen = 0,
          filterid = '',
          $filter = null,
          $filters = $container.find('li.filter-item'),
          filtersInd = 0, filtersLen = 0;

        for (filtersInd = 0, filtersLen = $filters.length; filtersInd < filtersLen; filtersInd++) {
            $filter = $($filters[filtersInd]);
            filterid = $filter.attr('data-id');
            wasAdded = false;
            for (filtervaluesInd = 0, filtervaluesLen = filtervalues ? filtervalues.length : 0; filtervaluesInd < filtervaluesLen; filtervaluesInd++) {
                if (filtervalues[filtervaluesInd].id === filterid) {
                    addFilterToGroup(groups, filtervalues[filtervaluesInd]);
                    filtervalues.splice(filtervaluesInd, 1);
                    wasAdded = true;
                    break;
                }
            }
            if (wasAdded === false) {
                addFilterToGroup(groups, { type: $filter.attr('data-type'), title: $filter.text(), group: '', hashmask: $filter.attr('data-anchor') });
            }
        }

        for (filtervaluesInd = 0, filtervaluesLen = filtervalues ? filtervalues.length : 0; filtervaluesInd < filtervaluesLen; filtervaluesInd++) {
            addFilterToGroup(groups, filtervalues[filtervaluesInd]);
        }

        groupsInd = groups.length;
        while (groupsInd--) {
            if (groups[groupsInd].title === '') {
                groups.push(groups[groupsInd]);
                groups.splice(groupsInd, 1);
                break;
            }
        }

        o = $container.find('ul.filter-list:first').removeClass('multi-column').empty()[0] || null;
        if (o) {
            if (opts.colcount >= 1) {
                o.className += ' multi-column multi-column-' + opts.colcount;
            }
            o.innerHTML = createAdvansedFilterGroup(opts, groups, 'filter');
        }

        o = $container.find('ul.sorter-list:first').removeClass('multi-column').empty()[0] || null;
        if (o) {
            o.innerHTML = createAdvansedFilterGroup(opts, [{ title: '', items: sortervalues }], 'sorter');
        }

        return o;
    }

    function callSetFilterTrigger($container, filtervalue, needResetAllEvent) {
        var
          filtervalue = filtervalue || null,
          opts = $container.data('filteroptions'),
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = filtervalues ? filtervalues.length : 0,
          selectedfilters = [];
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].isset === true) {
                selectedfilters.unshift(filtervalues[filtervaluesInd]);
            }
            if (filtervalue === null && filtervalues[filtervaluesInd].id === 'sorter') {
                filtervalue = filtervalues[filtervaluesInd];
            }
        }

        toggleHasFilters(selectedfilters, $container);

        updateLocalStorageFilters(opts, selectedfilters);
        if (needResetAllEvent === true) {
            $container.trigger('resetallfilters', [$container, filtervalue.id, selectedfilters]);
        }

        $container.trigger('setfilter', [$container, filtervalue, filtervalue != null ? filtervalue.params : null, selectedfilters]);
        $container.trigger('updatefilter', [$container, filtervalue != null ?  filtervalue.id : null, selectedfilters]);
    }

    function callResetFilterTrigger($container, filtervalue, needResetAllEvent) {
        var
          filtervalue = filtervalue || null,
          opts = $container.data('filteroptions'),
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = filtervalues ? filtervalues.length : 0,
          selectedfilters = [];
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].isset === true) {
                selectedfilters.unshift(filtervalues[filtervaluesInd]);
            }
            if (filtervalue === null && filtervalues[filtervaluesInd].id === 'sorter') {
                filtervalue = filtervalues[filtervaluesInd];
            }
        }
        
        toggleHasFilters(selectedfilters, $container);
        
        updateLocalStorageFilters(opts, selectedfilters);
        if (needResetAllEvent === true) {
            $container.trigger('resetallfilters', [$container, filtervalue.id, selectedfilters]);
        }
        $container.trigger('resetfilter', [$container, filtervalue, selectedfilters]);
        $container.trigger('updatefilter', [$container, filtervalue.id, selectedfilters]);
    }


    function setFilterItem($container, $filteritem, filtervalue, params, nonetrigger) {
        $container.trigger('adv-setFilterItem', [$container, $filteritem, filtervalue, params, nonetrigger]);
        onBodyClick(true);

        updateTextFilter($container, true);

        if ($container.length > 0 && filtervalue && filtervalue.id === 'sorter') {
            var $sorter = $container.find('li.sorter-item[data-id="' + params.id + '"]:first');

            resizeUserSorterContainer($container, params.title);

            var
              $sortercontainer = $container.find('div.advansed-filter-sort-container:first');

            var classname = params.dsc === true ? 'dsc-sort' : 'asc-sort';
            $sortercontainer
              .removeClass('asc-sort')
              .removeClass('dsc-sort')
              .addClass(classname);
            $sorter
              .addClass('selected')
              .removeClass('asc-sort')
              .removeClass('dsc-sort')
              .addClass(classname)
              .siblings().removeClass('selected').removeClass('asc-sort').removeClass('dsc-sort');
        }

        if ($container.length > 0 && filtervalue) {
            var paramsitem = null;
            var paramsid = $filteritem && $filteritem.length > 0 ? $filteritem.attr('data-paramsid') || null : null;
            if (!filtervalue.params) {
                filtervalue.params = filtervalue.multiselect === true ? [] : {};
            }

            if (filtervalue.multiselect === true && isArray(filtervalue.params) && paramsid) {
                var
                  items = filtervalue.params,
                  itemsInd = items ? items.length : 0;
                while (itemsInd--) {
                    if (paramsid == items[itemsInd].__id) {
                        paramsitem = items[itemsInd];
                        break;
                    }
                }
                if (!paramsitem) {
                    paramsitem = { __id: paramsid };
                    filtervalue.params.push(paramsitem);
                }
            } else {
                paramsitem = filtervalue.params;
            }

            for (var fld in params) {
                if (params.hasOwnProperty(fld)) {
                    paramsitem[fld] = params[fld];
                }
            }

            filtervalue.isset = true;
            resizeUserFilterContainer($container);
            if (nonetrigger !== true && lazyTrigger === false) {
                callSetFilterTrigger($container, filtervalue);
            }
        }
    }

    function unsetFilterItem($container, $filteritem, filtervalue, nonetrigger) {
        $container.trigger('adv-unsetFilterItem', [$container, $filteritem, filtervalue, nonetrigger]);
        onBodyClick(true);

        updateTextFilter($container, true);

        if ($container.length > 0 && filtervalue) {
            var noParams = false;
            var paramsid = $filteritem && $filteritem.length > 0 ? $filteritem.attr('data-paramsid') || null : null;
            if (filtervalue.multiselect === true && isArray(filtervalue.params) && paramsid) {
                var
                  items = filtervalue.params,
                  itemsInd = items ? items.length : 0;
                while (itemsInd--) {
                    if (paramsid == items[itemsInd].__id) {
                        items.splice(itemsInd, 1);
                        break;
                    }
                }
                if (items.length === 0) {
                    noParams = true;
                }
            } else {
                noParams = true;
            }
            if (noParams === true) {
                filtervalue.params = null;
                filtervalue.isset = false;
            }
            if (nonetrigger !== true && lazyTrigger === false) {
                callResetFilterTrigger($container, filtervalue);
            }
        }
    }

    function updateFiltersList($container) {
        var $groups = $container.find("li.item-group.filter-group");

        for (var i = 0, j = $groups.length; i < j; i++) {
            var $group = $($groups[i]);
            $group.removeClass('hidden-item');

            if ($group.find('.item-item:not(.hidden-item)').length === 0) {
                $group.addClass('hidden-item');
            }
        }

        //$items.filter('.item-group').removeClass('first-group').not('.disabled-item').filter(':first').addClass('first-group');
    }

    function updateTextFilter($container, nonetrigger) {
        var
          $this = $container.find('input.advansed-filter-input:first'),
          value = $this.val(),
          filtervalue = null,
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = 0;

        if (!filtervalues || filtervalues.length === 0) {
            return undefined;
        }

        filtervaluesInd = filtervalues.length;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === 'text') {
                break;
            }
        }

        if (filtervaluesInd !== -1) {
            filtervalue = filtervalues[filtervaluesInd];
            if (typeof value === 'string' && value.length > 0) {
                var params = { value: value };
                if (!filtervalue.params) {
                    filtervalue.params = {};
                }
                for (var fld in params) {
                    if (params.hasOwnProperty(fld)) {
                        filtervalue.params[fld] = params[fld];
                    }
                }
                filtervalue.isset = true;
            } else {
                filtervalue.params = null;
                filtervalue.isset = false;
            }
        }
    }

    function resetTextFilter($container) {
        var
            $this = $container.find('input.advansed-filter-input:first'),
            filtervalue = null,
            filtervalues = $container.data('filtervalues'),
            filtervaluesInd = 0;

        if (!filtervalues || filtervalues.length === 0) {
            return undefined;
        }

        filtervaluesInd = filtervalues.length;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === 'text') {
                break;
            }
        }

        $this.val('');

        if (filtervaluesInd !== -1) {
            filtervalue = filtervalues[filtervaluesInd];
            filtervalue.params = null;
            filtervalue.isset = false; 
        }
        $container.data('filtervalues', filtervalues);
    }
    /* <flag> */

    function onUserFilterFlagSelectValue($container, $filteritem, filtervalue, nonetrigger) {
        if ($container.length > 0 && filtervalue) {
            setFilterItem($container, $filteritem, filtervalue, {}, nonetrigger);
        }
    }

    function compareUserFilterParamsFlag($container, containerfiltervalue, filtervalue) {
        return true;
    }

    function customizeUserFilterFlag($container, $filteritem, filtervalue) {
        return filtervalue.hasOwnProperty('defaultparams') ? filtervalue.defaultparams : {};
    }

    function destroyUserFilterFlag($container, $filteritem, filtervalue) {

    }

    /* </flag> */
    /* <group> */

    function setUserFilterGroupValue($container, $filteritem, params) {
        if (params && params.hasOwnProperty('name')) {
            $filteritem.removeClass('default-value').find('span.group-selector:first span.custom-value:first').attr('title', params.name).find('span.value:first').text(params.name);
        }
    }

    function onUserFilterGroupSelectValue($container, $filteritem, filtervalue, group, nonetrigger) {
        if ($container.length > 0 && $filteritem.length > 0) {
            setUserFilterGroupValue($container, $filteritem, { id: group.id, name: converText(group.title) });
        }

        if ($container.length > 0 && filtervalue) {
            setFilterItem($container, $filteritem, filtervalue, { id: group.id, value: group.id, name: converText(group.title) }, nonetrigger);
        }
    }

    function showUserFilterGroup($container, $filteritem) {
        if ($container.hasClass('showed-groupselector')) {
            return undefined;
        }

        if (jQuery($container).find(".filter-item.filter-item-group > .advansed-filter-groupselector").length != 0){
            try {
                $container.addClass("showed-groupselector").find(".filter-item.filter-item-group > .selector-wrapper:first").groupadvancedSelector('reset', true);
            } catch (err) { }
        }

        //onBodyClick();
        jQuery(document.body).unbind('click', onBodyClick);

        resizeControlContainer($container, $filteritem, $container.find('div.advansed-filter-groupselector-container:first'));

        setTimeout(function () {
            jQuery(document.body).one('click', onBodyClick);
        }, 1);
    }

    function destroyUserFilterGroup($container, $filteritem, filtervalue) {

    }

    function compareUserFilterParamsGroup($container, containerfiltervalue, filtervalue) {
        return containerfiltervalue.id === filtervalue.id;
    }

    function customizeUserFilterGroup($container, $filteritem, filtervalue) {
        if (jQuery($filteritem).children(".advanced-selector-container").length == 0) {
            try {
                $container.addClass("showed-groupselector");
                $filteritem.children(".selector-wrapper:first").groupadvancedSelector(
                {
                    inPopup: true,
                    onechosen: true
                });
            } catch (err) { }

            try {
                jQuery($filteritem).children(".selector-wrapper:first")
                    .on("showList", 
                        (function ($container, $filteritem, filtervalue, callback) {
                            return function (event, group) {
                                callback($container, $filteritem, filtervalue, group);
                            }
                        })($container, $filteritem, filtervalue, onUserFilterGroupSelectValue)
                    );

                if (!filtervalue.isset && !filtervalue.bydefault) {
                    setTimeout(function() {
                        if ($filteritem.hasClass("default-value")) {
                            $filteritem.find(".selector-wrapper").click();
                        }
                    }, 0);
                }

            } catch (err) { }
        }
    }

    /* </group> */
    /* <person> */

    function setUserFilterPersonValue($container, $filteritem, params) {
        if (params && params.hasOwnProperty('name')) {
            $filteritem.removeClass('default-value').find('span.person-selector:first span.custom-value:first').attr('title', params.name).find('span.value:first').text(params.name);
        }
    }

    function onUserFilterPersonSelectValue($container, $filteritem, filtervalue, userid, username, nonetrigger) {
        if ($container.length > 0 && $filteritem.length > 0) {
            setUserFilterPersonValue($container, $filteritem, { id: userid, name: converText(username) });
        }

        if ($container.length > 0 && filtervalue) {
            setFilterItem($container, $filteritem, filtervalue, { id: userid, value: userid, name: converText(username) }, nonetrigger);
        }
    }

    function compareUserFilterParamsPerson($container, containerfiltervalue, filtervalue) {
        return containerfiltervalue.id === filtervalue.id;
    }

    function customizeUserFilterPerson($container, $filteritem, filtervalue) {
        if (jQuery($filteritem).children(".advansed-filter-userselector").length == 0) {
            try {
                var showme = filtervalue.hasOwnProperty('showme') ? Boolean(filtervalue.showme) : true;
                $container.addClass("showed-userselector");
                $filteritem.children(".selector-wrapper:first").useradvancedSelector(
                {
                    showme: showme,
                    inPopup: true,
                    onechosen: true,
                    showGroups: true,
                    itemsDisabledIds: showme ? [] : [window.Teamlab.profile.id],
                    showDisabled: window.Teamlab.profile.isAdmin
                });
            } catch (err) { }
            
            try {
                jQuery($filteritem).children(".selector-wrapper:first")
                    .on("showList", 
                        (function ($container, $filteritem, filtervalue, callback) {
                            return function (event, item) {
                                callback($container, $filteritem, filtervalue, item.id, item.title);
                            }
                        })($container, $filteritem, filtervalue, onUserFilterPersonSelectValue)
                    );

                if (!filtervalue.isset && !filtervalue.bydefault) {
                    setTimeout(function() {
                        if ($filteritem.hasClass("default-value")) {
                            $filteritem.find(".selector-wrapper").click();
                        }
                    }, 0);
                }

            } catch (err) { }
        }
    }

    function showUserFilterPerson($container, $filteritem) {
        if ($container.hasClass('showed-userselector')) {
            return undefined;
        }

        if (jQuery($container).find(".filter-item.filter-item-person > .advansed-filter-userselector").length != 0){
            try {
                $container.addClass("showed-userselector").find(".filter-item.filter-item-person > .selector-wrapper:first").useradvancedSelector('reset', true);
            } catch (err) { }
        }

        //onBodyClick();
        jQuery(document.body).unbind('click', onBodyClick);

        resizeControlContainer($container, $filteritem, $container.find('div.advansed-filter-userselector-container:first'));
        setTimeout(function () {
            jQuery(document.body).one('click', onBodyClick);
        }, 1);
    }

    function destroyUserFilterPerson($container, $filteritem, filtervalue) {

    }

    /* </person> */
    /* <date> */

    function onUserFilterDateSelectValue(target, $container, $filteritem, filtervalue, datetext, inst, nonetrigger, instDate) {
        var
          $target = jQuery(target),
          date = inst ? inst.input.datepicker('getDate') : instDate,
          $dateselector = $target.parents('span.advansed-filter-dateselector-date:first'),
          $dependentDatepicker = null,
          isFromDateDatepicker = false;

        if ($dateselector.hasClass("dateselector-from-date")) {
            isFromDateDatepicker = true;
            $dependentDatepicker = $dateselector.parents(".filter-item-daterange:first").find('span.to-daterange-selector:first span.datepicker-container:first');
        } else {
            isFromDateDatepicker = false;
            $dependentDatepicker = $dateselector.parents(".filter-item-daterange:first").find('span.from-daterange-selector:first span.datepicker-container:first');
        }

        if ($container.length > 0 && filtervalue) {
            $dateselector.find('span.btn-show-datepicker-title:first').text(datetext);
            if (date) {
                setFilterItem($container, $filteritem, filtervalue, isFromDateDatepicker ? { from: date.getTime() } : { to: date.getTime() }, nonetrigger);
                $dependentDatepicker.datepicker("option", isFromDateDatepicker ? "minDate" : "maxDate", date);
            }
        }
    }

    function compareUserFilterParamsDate($container, containerfiltervalue, filtervalue) {
        return containerfiltervalue.from == filtervalue.from && containerfiltervalue.to == filtervalue.to;
    }

    function customizeUserFilterDate($container, $filteritem, filtervalue) {
        var
          $datepicker = null,
          $fromDateContainer = null,
          $toDateContainer = null,
          tmpDate = new Date(),
          defaultFromDate,
          defaultToDate;

        defaultFromDate = new Date(tmpDate.getFullYear(), tmpDate.getMonth() - 6, tmpDate.getDate(), 0, 0, 0, 0);
        defaultToDate = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0);

        $fromDateContainer = $filteritem.find('span.from-daterange-selector:first span.datepicker-container:first');
        $toDateContainer = $filteritem.find('span.to-daterange-selector:first span.datepicker-container:first')

        $datepicker = createDatepicker($fromDateContainer, $container, $filteritem, filtervalue, defaultFromDate);
        $datepicker.datepicker("option", "maxDate", defaultToDate);
        $filteritem.find('span.from-daterange-selector:first span.btn-show-datepicker-title:first').text($.datepicker.formatDate($datepicker.datepicker("option", "dateFormat"), $datepicker.datepicker("getDate")));

        $datepicker = createDatepicker($toDateContainer, $container, $filteritem, filtervalue, defaultToDate);
        $datepicker.datepicker("option", "minDate", defaultFromDate);
        $filteritem.find('span.to-daterange-selector:first span.btn-show-datepicker-title:first').text($.datepicker.formatDate($datepicker.datepicker("option", "dateFormat"), $datepicker.datepicker("getDate")));

        return { from: defaultFromDate.getTime(), to: defaultToDate.getTime() };
    }

    function showUserFilterDate(evt, $container, $filteritem, $dateselector) {
        if ($dateselector.hasClass('showed-datepicker')) {
            return undefined;
        }

        //onBodyClick(evt);
        jQuery(document.body).unbind('click', onBodyClick);

        var $datepicker = $dateselector.addClass('showed-datepicker').find('span.advansed-filter-datepicker-container:first').css('display', 'block');

        $datepicker.removeClass('reverse-position');
        if ($filteritem.parents('div.hidden-filters-container:first').length === 0) {
            if ($container.width() - $filteritem[0].offsetLeft - $filteritem.parent()[0].offsetLeft - $filteritem.width() < $datepicker.width()) {
                $datepicker.addClass('reverse-position');
            }
        }

        setTimeout(function () {
            jQuery(document.body).one('click', onBodyClick);
        }, 1);
    }

    function destroyUserFilterDate($container, $filteritem, filtervalue) {

    }

    /* </date> */
    /* <combobox> */

    function onUserFilterComboboxSelectValue(target, $container, $filteritem, filtervalue, nonetrigger) {
        var
          $target = jQuery(target),
          value = $target.val();

        if ($container.length > 0 && filtervalue) {
            if (isArray(value)) {
                var
                  values = value,
                  valuesInd = value.length;
                while (valuesInd--) {
                    if (values[valuesInd] == -1) {
                        values.splice(valuesInd, 1);
                        break;
                    }
                }
                value = values;
            }

            $filteritem.removeClass('default-value');

            setFilterItem($container, $filteritem, filtervalue, { value: value, title: $target.find('option[value="' + ("" + value).replace(/\"/g, '\\"') + '"]:first').text() }, nonetrigger);
        }
    }

    function compareUserFilterParamsCombobox($container, containerfiltervalue, filtervalue) {
        if (containerfiltervalue.value == filtervalue.value) {
            return true;
        }
        if (isArray(containerfiltervalue.value) && isArray(filtervalue.value) && containerfiltervalue.value.length === filtervalue.value.length) {
            return [].concat(containerfiltervalue.value).sort().join('') == [].concat(filtervalue.value).sort().join('');
        }
        return false;
    }

    function customizeUserFilterCombobox($container, $filteritem, filtervalue) {
        var
          $select = $filteritem.find('select.advansed-filter-combobox:first'),
          value = $select.val(),
          defaultvalue = filtervalue.hasOwnProperty('defaultvalue') ? filtervalue.defaultvalue : '-1';

        if (isArray(value)) {
            var
              values = value,
              valuesInd = value.length;
            while (valuesInd--) {
                if (values[valuesInd] == -1) {
                    values.splice(valuesInd, 1);
                    break;
                }
            }
            value = values;
        }

        value = value == defaultvalue ? null : value;
        value = isArray(value) && (value.length === 0 || (value.length === 1 && value[0] == '-1')) ? null : value;

        $select
          .advansedFilterCustomCombobox()
          .change((function ($container, $filteritem, filtervalue, callback) {
              return function (evt) {
                  callback(this, $container, $filteritem, filtervalue);
              }
          })($container, $filteritem, filtervalue, onUserFilterComboboxSelectValue));

        if (!filtervalue.isset && value === null) {
            setTimeout(function () {
                if ($filteritem.hasClass("default-value")) {
                    $filteritem.find(".selector-wrapper .combobox-selector .combobox-title").click();
                }
            }, 0);
        }

        // d'ohhh
        return !value ? null : { value: value, title: $select.find('option[value="' + ("" + value).replace(/\"/g, '\\"') + '"]:first').text() };
    }

    function destroyUserFilterCombobox($container, $filteritem, filtervalue) {

    }

    /* </combobox> */

    function compareUserFilterParams($container, filtervalue, params, nonetrigger) {
        var fn = null;
        switch (filtervalue.type) {
            case 'flag': fn = compareUserFilterParamsFlag; break;
            case 'group': fn = compareUserFilterParamsGroup; break;
            case 'person': fn = compareUserFilterParamsPerson; break;
            case 'daterange': fn = compareUserFilterParamsDate; break;
            case 'combobox': fn = compareUserFilterParamsCombobox; break;
        }

        if (fn !== null) {
            if (filtervalue.hasOwnProperty('params')) {
                return fn($container, filtervalue.params, params, nonetrigger);
            }
        }
        return false;
    }

    function customizeUserFilter($container, $filteritem, filtervalue, nonetrigger) {
        var fn = null;
        switch (filtervalue.type) {
            case 'flag': fn = customizeUserFilterFlag; break;
            case 'group': fn = customizeUserFilterGroup; break;
            case 'person': fn = customizeUserFilterPerson; break;
            case 'daterange': fn = customizeUserFilterDate; break;
            case 'combobox': fn = customizeUserFilterCombobox; break;
        }

        if (fn !== null) {
            var o = fn($container, $filteritem, filtervalue, nonetrigger);
            return filtervalue.hasOwnProperty('bydefault') && filtervalue.bydefault && typeof filtervalue.bydefault === 'object' ? filtervalue.bydefault : o;
        }
    }

    function destroyUserFilter($container, $filteritem, filtervalue) {
        var fn = null;
        switch (filtervalue.type) {
            case 'flag': fn = destroyUserFilterFlag; break;
            case 'group': fn = destroyUserFilterGroup; break;
            case 'person': fn = destroyUserFilterPerson; break;
            case 'daterange': fn = destroyUserFilterDate; break;
            case 'combobox': fn = destroyUserFilterCombobox; break;
        }

        if (fn !== null) {
            fn($container, $filteritem, filtervalue);
        }
    }

    function processUserFilter($container, $filteritem, filtervalue, params, nonetrigger) {
        switch (filtervalue.type) {
            case 'flag':
                onUserFilterFlagSelectValue($container, $filteritem, filtervalue, nonetrigger);
                break;
            case 'group':
                onUserFilterGroupSelectValue($container, $filteritem, filtervalue, { id: params.id, title: getGroupSelectorName($container, params.id) }, nonetrigger);
                break;
            case 'person':
                onUserFilterPersonSelectValue($container, $filteritem, filtervalue, params.id, getUserSelectorName($container, params.id, params.name), nonetrigger);
                break;
            case 'daterange':
                var date = null, instDate = null, $datepicker = null;

                if (params.hasOwnProperty('from')) {
                    $datepicker = $filteritem.find('span.from-daterange-selector:first span.datepicker-container:first');
                    date = new Date(+params.from);
                    instDate = $datepicker.datepicker('getDate');

                    if ($datepicker.length > 0 && instDate && date instanceof Date) {
                        $datepicker.datepicker('setDate', date);
                        instDate = $datepicker.datepicker('getDate');
                        onUserFilterDateSelectValue($datepicker, $container, $filteritem, filtervalue, $.datepicker.formatDate($datepicker.datepicker("option", "dateFormat"), instDate), null, nonetrigger, instDate);
                    }
                }

                if (params.hasOwnProperty('to')) {
                    $datepicker = $filteritem.find('span.to-daterange-selector:first span.datepicker-container:first');
                    date = new Date(+params.to);
                    instDate = $datepicker.datepicker('getDate');

                    if ($datepicker.length > 0 && instDate && date instanceof Date) {
                        $datepicker.datepicker('setDate', date);
                        instDate = $datepicker.datepicker('getDate');
                        onUserFilterDateSelectValue($datepicker, $container, $filteritem, filtervalue, $.datepicker.formatDate($datepicker.datepicker("option", "dateFormat"), instDate), null, nonetrigger, instDate);
                    }
                }
                break;
            case 'combobox':
                $filteritem.find('select').val(params.value).change();
                break;
            default:
                filtervalue.hasOwnProperty('process') && typeof filtervalue.process === 'function' ? filtervalue.process($container, $filteritem, filtervalue, params, nonetrigger) : null;
                break;
        }
    }

    function showUserFilterByOption($filterItem) {
        if (!$filterItem.hasClass('hidden-item')) return;

        $filterItem.removeClass('hidden-item');
    }

    function hideUserFilterByOption($container, $selectedfilterItem, $filterItem, nonetrigger) {
        if ($filterItem.hasClass('hidden-item')) return false;

        $filterItem.addClass('hidden-item');

        if ($selectedfilterItem.length > 0) {
            removeUserFilterByObject($container, $selectedfilterItem, nonetrigger);
            return true;
        }
    }

    function enableUserFilterByOption($filterItem) {
        if (!$filterItem.hasClass('disabled-item')) return;

        $filterItem.removeClass('disabled-item');
    }

    function disableUserFilterByOption($container, $selectedfilterItem, $filterItem, nonetrigger) {
        if ($filterItem.hasClass('disabled-item')) return false;

        $filterItem.addClass('disabled-item');

        if ($selectedfilterItem.length > 0) {
            removeUserFilterByObject($container, $selectedfilterItem, nonetrigger);
            return true;
        }
    }

    function resetUserFilterByOption($container, $selectedfilterItem, nonetrigger) {
        if ($selectedfilterItem.length > 0) {
            removeUserFilterByObject($container, $selectedfilterItem, nonetrigger);
            return true;
        }
    }

    function addUserFilter($container, filtervalue, params, nonetrigger) {
        if (typeof(nonetrigger) == "undefined" && typeof(filtervalue.nonetrigger) != "undefined") {
            nonetrigger = filtervalue.nonetrigger;
        }
        var
          $filteritem = null,
          $filters = $container.find('div.advansed-filter-filters:first');
        if ($filters.length === 0) {
            return undefined;
        }

        if (filtervalue.id === 'text') {
            if (params && typeof params === 'object' && params.hasOwnProperty('value')) {
                var $input = $container.find('input.advansed-filter-input:first');
                if ($input.val() != params.value) {
                    $input.val(params.value);
                    updateTextFilter($container, nonetrigger);
                    return true;
                }
            };
            return false;
        }

        if (($filteritem = $filters.find('div.filter-item[data-id="' + filtervalue.id + '"]:first')).length > 0 && filtervalue.multiselect !== true) {
            if (!filtervalue.hasOwnProperty('groupby') && (!params || compareUserFilterParams($container, filtervalue, params, nonetrigger))) {
                return false;
            }
            removeUserFilterByObject($container, $filteritem, true);
        }

        var groupby = filtervalue.hasOwnProperty('groupby') && typeof filtervalue.groupby === 'string' && filtervalue.groupby.length > 0 ? filtervalue.groupby : null;
        if (groupby !== null) {
            var $groupfilter = $filters.find('div.filter-item[data-group="' + groupby + '"]:first');
            if ($container.length > 0 && $groupfilter.length > 0) {
                removeUserFilterByObject($container, $groupfilter, true);
            }
        }

        var customdata = $container.data('customdata');
        if (customdata) {
            var maxfilters = customdata.hasOwnProperty('maxfilters') ? customdata.maxfilters : -1;
            if (maxfilters !== -1 && $filters.find('div.filter-item').length >= maxfilters) {
                var $firstfilter = $filters.find('div.filter-item:first');
                if ($container.length > 0 && $firstfilter.length > 0) {
                    removeUserFilterByObject($container, $firstfilter, true);
                }
            }
        }

        var paramsitems = isArray(params) ? params : [params];
        for (var i = 0, n = paramsitems.length; i < n; i++) {
            params = paramsitems[i];

            $filteritem = $((filtervalue.hasOwnProperty('create') && typeof filtervalue.create === 'function' ? filtervalue.create : createFilterItem)(filtervalue));

            if ($filteritem.length > 0) {
                $filters.append($filteritem);
                if (filtervalue) {
                    var paramsid = params && typeof params === 'object' && params.hasOwnProperty('__id') ? params.__id : Math.floor(Math.random() * 1000000);
                    $filteritem.attr('data-id', filtervalue.id).attr('data-paramsid', paramsid).attr('data-type', filtervalue.type).addClass('filter-item').addClass('filter-item-' + filtervalue.type);
                    if (groupby !== null) {
                        $filteritem.attr('data-group', groupby);
                    }

                    var defaultparams = (filtervalue.hasOwnProperty('customize') && typeof filtervalue.customize === 'function' ? filtervalue.customize : customizeUserFilter)($container, $filteritem, filtervalue, nonetrigger);
                    defaultparams = defaultparams && typeof defaultparams === 'object' ? defaultparams : null;
                    defaultparams = filtervalue.hasOwnProperty('value') ? filtervalue.value : defaultparams;
                    defaultparams ? defaultparams.__id = paramsid : null;
                    params = params && typeof params === 'object' ? params : null;
                    params ? params.__id = paramsid : null;
                    if (defaultparams && !params) {
                        setFilterItem($container, $filteritem, filtervalue, defaultparams, nonetrigger);
                        processUserFilter($container, $filteritem, filtervalue, defaultparams, true);
                    }
                    if (params) {
                        setFilterItem($container, $filteritem, filtervalue, params, nonetrigger);
                        processUserFilter($container, $filteritem, filtervalue, params, nonetrigger);
                    }
                }
            }
        }

        resizeUserFilterContainer($container);
        return true;
    }

    function removeUserFilter($container, filtervalue, $filteritem, nonetrigger) {
        var $filters = $container.find('div.advansed-filter-filters:first');
        if ($filters.length === 0) {
            return undefined;
        }

        if ($filteritem.length > 0) {
            if (filtervalue) {
                (filtervalue.hasOwnProperty('destroy') && typeof filtervalue.destroy === 'function' ? filtervalue.destroy : destroyUserFilter)($container, $filteritem, filtervalue);
                unsetFilterItem($container, $filteritem, filtervalue, nonetrigger);
            }
            $filteritem.find('div.advansed-filter-control').appendTo($container);
            $filteritem.remove();
        }

        resizeUserFilterContainer($container);
    }

    function removeUserFilterByObject($container, $filteritem, nonetrigger) {
        var
          id = $filteritem.attr('data-id'),
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = 0;
        filtervaluesInd = filtervalues ? filtervalues.length : 0;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === id) {
                break;
            }
        }
        removeUserFilter($container, filtervaluesInd !== -1 ? filtervalues[filtervaluesInd] : null, $filteritem, nonetrigger);
    }

    function enableUserSorter($container, sorterid, nonetrigger) {
        var
          $sorteritems = $container.find('li.item-item.sorter-item');

        $sorteritems.filter('[data-id="' + sorterid + '"]').removeClass('hidden-item');
    }

    function disableUserSorter($container, sorterid, nonetrigger) {
        var
          $sorteritems = $container.find('li.item-item.sorter-item'),
          $sorter = null;

        $sorter = $sorteritems.filter('[data-id="' + sorterid + '"]').addClass('hidden-item');

        if ($sorter.hasClass('selected')) {
            $sorter = $sorteritems.not('.hidden-item').filter(':first');
            if ($sorter.length > 0) {
                sorterid = $sorter.attr('data-id');
                if (sorterid) {
                    setUserSorter($container, sorterid, { dsc: false }, nonetrigger);
                    return true;
                }
            }
        }
    }

    function setUserSorter($container, sorterid, params, nonetrigger) {
        var
          filtervalues = $container.data('filtervalues') || [],
          filtervaluesInd = -1,
          sortervalues = $container.data('sortervalues') || [],
          sortervaluesInd = -1;

        if (sortervalues.length > 0) {
            sortervaluesInd = sortervalues.length;
            while (sortervaluesInd--) {
                if (sortervalues[sortervaluesInd].id === sorterid) {
                    break;
                }
            }
        }

        if (filtervalues.length > 0) {
            filtervaluesInd = filtervalues.length;
            while (filtervaluesInd--) {
                if (filtervalues[filtervaluesInd].id === 'sorter') {
                    break;
                }
            }
        }

        if (filtervaluesInd !== -1 && sortervaluesInd !== -1) {
            if (
              filtervalues[filtervaluesInd].params &&
              filtervalues[filtervaluesInd].params.id === sortervalues[sortervaluesInd].id &&
              filtervalues[filtervaluesInd].params.dsc === params.dsc
            ) {
                return false;
            }
            sortervalues[sortervaluesInd].dsc = params.dsc;
            sortervalues[sortervaluesInd].sortOrder = params.dsc === true ? 'descending' : 'ascending';
            setFilterItem($container, null, filtervalues[filtervaluesInd], sortervalues[sortervaluesInd], nonetrigger);
            return true;
        }
        return false;
    }

    function needHideFilters($container, $filters, $firstfilter) {
        var
          minInputWidth = parseInt($container.find("div.advansed-filter-input input:first").css("min-width") || 100) || 100,
          resetBtnWidth = parseInt($container.find("div.advansed-filter-input").css("margin-right") || 0),
          $lastfilter = $filters.children("div.filter-item:visible:last"),
          $btn = $container.find(".advansed-filter-button.btn-show-filters:first"),
          btnWidth = $btn.length == 1 ? $btn.width() : 0,
          visibleBlocksWidth = 0;

        $filters.children("div:visible").each(function(){
            visibleBlocksWidth += jq(this).outerWidth();
        });

        //console.log("$container[0].offsetWidth, $filters[0].offsetWidth, visibleBlocksWidth, visibleBlocksWidth + $filters[0].offsetLeft + btnWidth + minInputWidth + resetBtnWidth + 10);
        //console.log($firstfilter[0].offsetTop, $lastfilter[0].offsetTop);


        return $firstfilter.length === 0 || $lastfilter.length === 0 ? false :
          ($firstfilter[0].offsetTop !== $lastfilter[0].offsetTop) ||
          ($firstfilter[0].offsetHeight !== $lastfilter[0].offsetHeight) ||
          ($container[0].offsetWidth < $filters[0].offsetWidth + $filters[0].offsetLeft + btnWidth + minInputWidth) ||
          ($firstfilter.find('span.selector-wrapper:first')[0].offsetHeight !== $lastfilter.find('span.selector-wrapper:first')[0].offsetHeight) ||
          $container[0].offsetWidth < visibleBlocksWidth + $filters[0].offsetLeft + btnWidth + minInputWidth + resetBtnWidth + 10;
    }

    function resizeFilterGroupByHeight(opts, $container) {
        if (opts && typeof opts === 'object' && opts.colcount > 1) {
            var rowcount = 0, maxitemmetric = 0, itemmetric = 0,
                $grouplist = $container.find('ul.item-list.filter-list.multi-column:first'),
                $group = null,
                $cols = $grouplist.find('li.item-group-col'),
                $colsInd = 0,
                $groups = null;

            rowcount = Math.ceil($grouplist.find('li.item-group.filter-group').length / opts.colcount);
            while (rowcount--) {
                maxitemmetric = 0;
                $groups = $();
                $colsInd = $cols.length;
                while ($colsInd--) {
                    $group = $($($cols[$colsInd]).find('li.item-group.filter-group')[rowcount]);
                    if ($group.length > 0) {
                        $groups = $groups.add($group);
                        itemmetric = $group.height();
                        maxitemmetric < itemmetric ? maxitemmetric = itemmetric : null;
                    }
                }
                $groups.height(maxitemmetric + 'px');
            }
        }
    }

    function resizeUserSorterContainer($container, title) {
        var
          sortercontainerWidth = 0,
          $filtercontainer = $container.find('div.advansed-filter-container:first'),
          $sortercontainer = $container.find('div.advansed-filter-sort-container:first');
        if ($sortercontainer.length > 0) {
            $sortercontainer.addClass('sorter-isset');
            if (title) {
                $sortercontainer.find('span.value:first').text(title).attr("title", title);
            }
            sortercontainerWidth = $container.hasClass('disable-sorter-block') ? 0 : $sortercontainer.width();
            $filtercontainer.css('margin-right', (sortercontainerWidth > 0 ? sortercontainerWidth + 38 : 0) + 22 + 'px');
            $container.find('div.advansed-filter-helper:first').css('margin-right', (sortercontainerWidth > 0 ? sortercontainerWidth + 38 : 0) + 22 + 'px');
            //$container.find('label.advansed-filter-state:first').css('left', $filtercontainer.width() + 8 + 'px');
            $container.find('label.advansed-filter-state:first').css('left', 'auto').css('right', (sortercontainerWidth > 0 ? sortercontainerWidth + 38 : 0) + 'px');

            // if filter width change
            resizeUserFilterContainer($container);
        }
    }

    function resizeUserFilterContainer($container) {
        var
          containerWidth = $container.width(),
          $input = $container.find('div.advansed-filter-input:first'),
          $button = $container.find('div.advansed-filter-button:first'),
          $filters = $container.find('div.advansed-filter-filters:first'),
          $hiddenfilterscontainer = $filters.find('div.hidden-filters-container:first'),
          $hiddenfilteritems = $hiddenfilterscontainer.find('div.filter-item');

        if ($input.length === 0 || $filters.length === 0 || containerWidth === 0) {
            return undefined;
        }

        if ($filters.find('div.filter-item').length === 0) {
            $filters.addClass('empty-list');
            $container.addClass('empty-filter-list');
        } else {
            $filters.removeClass('empty-list');
            $container.removeClass('empty-filter-list');
        }

        //$filters.removeClass('has-hidden-filters').find('div.filter-item').removeClass('hidden-filter').appendTo($filters);
        $filters.removeClass('has-hidden-filters');
        $hiddenfilteritems.removeClass('hidden-filter').insertAfter($hiddenfilterscontainer);

        var
          titlewidth = 0,
          maxwidth = 0,
          $selectorwrapper = null,
          $el = null,
          ind = 0,
          opts = $container.data('filteroptions'),
          $advansedcontainer = $container.find('div.advansed-filter-container:first'),
          $firstfilter = $filters.find('div.filter-item:first'),

          //$selectedfilters = $filters.children('div.filter-item').not('.is-rendered'),
          //selectedfiltersInd = $selectedfilters.length,
          $selectedfilters = $filters.find('div.filter-item'),
          needhidefilters = false;


        ind = $selectedfilters.length;
        while (ind--) {
            $el = jQuery($selectedfilters[ind]);
            $selectorwrapper = $el.find('span.selector-wrapper:first');
            titlewidth = $el.find('span.title:first').width() + 8; //8 - padding
            $el.width(titlewidth + $selectorwrapper.width());
            $selectorwrapper.css('left', titlewidth + 'px');
        }
        $input[0].style.marginLeft = 'auto';
        needhidefilters = needHideFilters($advansedcontainer, $filters, $firstfilter);

        if ($firstfilter.length && needhidefilters) {
            $filters.addClass('has-hidden-filters');

            $firstfilter = $filters.find('div.filter-item:first:visible');
            while ($firstfilter.length > 0 && needHideFilters($advansedcontainer, $filters, $firstfilter)) {
                $el = $firstfilter;

                $firstfilter = $firstfilter.next("div.filter-item");

                $el.addClass('hidden-filter').appendTo($hiddenfilterscontainer);
            }
        }

        if ($filters.find('div.hidden-filter').length > 0) {
            $filters.addClass('has-hidden-filters');
        } else {
            $filters.removeClass('has-hidden-filters');
        }

        var
          $hiddenfilters = $hiddenfilterscontainer.find('div.filter-item'),
          hiddenfiltersInd = $hiddenfilters.length,
          zindex = 0;
        while (hiddenfiltersInd--) {
            $hiddenfilters[hiddenfiltersInd].style.zIndex = ++zindex;
        }

        if (opts && typeof opts === 'object' && opts.colcount > 1) {
            var $advansedfilterlist = $container.find('ul.item-list.filter-list.multi-column:first');

            if ($advansedfilterlist.addClass('show-item-list').height() > 0 && !$advansedfilterlist.hasClass('is-render')) {
                //resizeFilterGroupByHeight(opts, $container);

                var colwidth = 0, colswidth = 0,
                    $advansedfilterlistcols = $advansedfilterlist.find('li.item-group-col');

                $advansedfilterlist.width('auto')
                var $advansedfilterlistcolsInd = $advansedfilterlistcols.length;
                while ($advansedfilterlistcolsInd--) {
                    colwidth = getClientWidth($advansedfilterlistcols[$advansedfilterlistcolsInd]);
                    $advansedfilterlistcols[$advansedfilterlistcolsInd].style.width = colwidth + 'px';
                    colswidth += colwidth;
                }
                $advansedfilterlist.addClass('is-render').width(colswidth + 20 + 'px'); // 20 - padding
            }
            $advansedfilterlist.removeClass('show-item-list');
        }

        var offsetLeft = $filters.width() + 2;
        //if (/* !$filters.hasClass('is-render') && */$button.length > 0) {
        //  //$filters.addClass('is-render');
        //  //$filters.css('left', $button.width() + 1 + 3 + 'px');
        //  $button.css('left', offsetLeft + 'px');
        //  $container.find('div.advansed-filter-list:first').css('left', offsetLeft + 'px');
        //}
        $button.css('left', offsetLeft + 'px');
        $container.find('div.advansed-filter-list:first').css('left', offsetLeft + 'px');
        $input[0].style.marginLeft = $button[0].offsetWidth + offsetLeft + 'px';
    }

    function getClientWidth(elem) {
        var rect = elem.getBoundingClientRect();
        return Math.ceil(rect.width);
    }

    function resizeControlContainer($container, $filteritem, $control) {
        $control
          .addClass('reset-position')
          .parents('div.advansed-filter-control:first')
          .appendTo($filteritem.parents('div.hidden-filters-container:first').length === 0 ? $filteritem : $filteritem.parents('div.advansed-filter-filters:first').find('div.btn-show-hidden-filters:first'));

        if ($filteritem.parents('div.hidden-filters-container:first').length > 0) {
            return undefined;
        }

        if ($container.width() - $filteritem[0].offsetLeft - $filteritem.parent()[0].offsetLeft - $filteritem.width() < $control.width()) {
            var
              offset = $control.width() - ($container.width() - $filteritem[0].offsetLeft - $filteritem.parent()[0].offsetLeft - $filteritem.width()) + 40,
              offsetcontroltop = $control.find('div.control-top:first')[0].offsetLeft || 0,
              margincontainer = parseFloat($control.css('margin-left'));

            $control.removeClass('reset-position').css('margin-left', -offset + 'px')
              .find('div.control-top:first').css('left', offsetcontroltop + offset - (isFinite(margincontainer) ? Math.abs(margincontainer) : 0) + 'px');
        }
    }

    function resizeContainer($container) {
        var
          $support = $container.find('div.advansed-filter-support:first'),
          $label = $container.find('label.advansed-filter-label:first'),
          $input = $container.find('input.advansed-filter-input:first'),
          $filtercontainer = $container.find('div.advansed-filter-container:first'),
          $filterlist = $container.find('div.advansed-filter-list:first'),
          id = $input.attr('id') || Math.floor(Math.random() * 1000000);

        $input.attr('id', id);
        $label.attr('for', id);

        var supportwidth = $support.innerWidth();
        $filtercontainer.css('margin-left', supportwidth + 'px');
        $filterlist.css('margin-left', supportwidth + 'px');
    }

    function toggleSorterBlock($container, value) {
        value === true ? $container.removeClass('disable-sorter-block') : $container.addClass('disable-sorter-block');
    }

    function addReadyEvent(fn, args) {
        if (typeof fn === 'function') {
            jQuery((function (fn, args) { return function () { setTimeout(function () { fn.apply(window, args) }, 1) } })(fn, args));
        }
    }

    function setEvents($container, opts) {
        $container = $container.hasClass('advansed-filter') ? $container : $container.find('div.advansed-filter:first');
        if ($container.length === 0) {
            return undefined;
        }

        if (opts.hasOwnProperty('anykey') && opts.anykey === true) {
            var timeout = opts.hasOwnProperty('anykeytimeout') ? opts.anykeytimeout : defaultAnykeyTimeout;
            timeout = isFinite(+timeout) ? +timeout : defaultAnykeyTimeout;
            if (timeout > 0) {
                filterInputKeyupTimeout = timeout;
                $container.find('input.advansed-filter-input:first').unbind('keyup paste', onFilterInputKeyupHelper).bind('keyup paste', onFilterInputKeyupHelper);
            } else {
                $container.find('input.advansed-filter-input:first').unbind('keyup paste', onFilterInputKeyup).bind('keyup paste', onFilterInputKeyup);
            }
        } else {
            $container.find('input.advansed-filter-input:first').unbind('keyup paste', onFilterInputEnter).bind('keyup paste', onFilterInputEnter);
        }
        $container.find('input.advansed-filter-input:first').unbind('keyup paste input', onKeyUp).bind('keyup paste input', onKeyUp);

        $container.find('.btn-start-filter:first').unbind('click', onStartFilter).bind('click', onStartFilter);
        $container.find('label.btn-reset-filter:first').unbind('click', onResetFilter).bind('click', onResetFilter);
        $container.find('ul.filter-list:first').unbind('click', onSelectFilter).bind('click', onSelectFilter);
        $container.find('ul.sorter-list:first').unbind('click', onSelectSorter).bind('click', onSelectSorter);
        $container.find('div.btn-show-filters:first').unbind('click', onShowFilters).bind('click', onShowFilters);

        $container.find('span.btn-toggle-sorter:first').unbind('click', onToggleSorter).bind('click', onToggleSorter);
        $container.find('div.advansed-filter-sort-container:first span.title:first').unbind('click', onShowSorters).bind('click', onShowSorters);

        $container.find('div.advansed-filter-filters:first').unbind('click', onUserFilterClick).bind('click', onUserFilterClick);
        $container.find('div.advansed-filter-userselector:first').unbind('click', onUserSelectorClick).bind('click', onUserSelectorClick);
        $container.find('div.advansed-filter-groupselector:first').unbind('click', onGroupSelectorClick).bind('click', onGroupSelectorClick);
    }

    /* <callbacks> */

    function onBodyClick(p1) {
        var $target = p1 && typeof p1 === 'object' ? jQuery(p1.target) : null;

        if (
          ($target && $target.is('span.btn-show-datepicker') && $target.parents('div.hidden-filters-container:first').length > 0) ||
          ($target && $target.is('span.combobox-title') && $target.parents('div.hidden-filters-container:first').length > 0) ||
          ($target && $target.is('span.combobox-title-inner-text') && $target.parents('div.hidden-filters-container:first').length > 0)
        ) {
            jQuery(document.body).unbind('click', arguments.callee);
            jQuery(document.body).one('click', arguments.callee);

            jQuery('div.advansed-filter').find('span.advansed-filter-dateselector-date').removeClass('showed-datepicker').find('span.advansed-filter-datepicker-container').hide();
            return undefined;
        }

        if (p1 === true) {
            jQuery(document.body).trigger('click');
        }

        jQuery('div.advansed-filter').removeClass('showed-filters').find('ul.filter-list:first').hide();
        jQuery('div.advansed-filter').removeClass('showed-sorters').find('ul.sorter-list:first').hide();
        jQuery('div.advansed-filter').removeClass('showed-hidden-filters').find('div.hidden-filters-container:first').hide();
        jQuery('div.advansed-filter').removeClass('showed-userselector').find('div.advansed-filter-userselector-container:first').hide();
        jQuery('div.advansed-filter').removeClass('showed-groupselector').find('div.advansed-filter-groupselector-container:first').hide();

        jQuery('div.advansed-filter').find('span.advansed-filter-dateselector-date').removeClass('showed-datepicker').find('span.advansed-filter-datepicker-container').hide();
    }

    function onKeyUp() {
        if (this.value) {
            this.parentNode.classList.add('has-value');
        } else {
            this.parentNode.classList.remove('has-value');
        }
    }

    function onShowFilters(evt) {
        var $filter = jQuery(this).parents('div.advansed-filter:first');
        if ($filter.hasClass('showed-filters')) {
            return undefined;
        }

        onBodyClick(evt);
        jQuery(document.body).unbind('click', onBodyClick);
        $filter.addClass('showed-filters').find('ul.filter-list:first').show();
        setTimeout(function () {
            jQuery(document.body).one('click', onBodyClick);
        }, 1);
    }

    function onShowSorters(evt) {
        var $container = jQuery(this).parents('div.advansed-filter:first');
        if ($container.hasClass('disable-sorter-block') || $container.find('div.advansed-filter-sort-container:first').hasClass('sorter-nochange') || $container.hasClass('showed-sorters')) {
            return undefined;
        }

        onBodyClick(evt);
        jQuery(document.body).unbind('click', onBodyClick);
        $container.addClass('showed-sorters').find('ul.sorter-list:first').show();
        setTimeout(function () {
            jQuery(document.body).one('click', onBodyClick);
        }, 1);
    }

    function onToggleSorter(evt) {
        var $container = jQuery(this).parents('div.advansed-filter:first');
        if ($container.hasClass('disable-sorter-block')) {
            return undefined;
        }

        var $selsorter = $container.find('li.sorter-item.selected:first');

        if ($container.length === 0 || $selsorter.length === 0) {
            return undefined;
        }

        var sorterid = $selsorter.attr('data-id') || null;
        if (sorterid) {
            setUserSorter($container, sorterid, { dsc: $selsorter.hasClass('asc-sort') });
        }
    }

    function onSelectFilter(evt) {
        var $selfilter = jQuery(evt.target);
        if (!$selfilter.hasClass('filter-item')) {
            $selfilter = $selfilter.parents('li.filter-item:first');
        }
        var $container = $selfilter.parents('div.advansed-filter:first');
        if ($container.length === 0 || $selfilter.length === 0 || $selfilter.hasClass('hidden-item') || $selfilter.hasClass('disabled-item')) {
            return undefined;
        }

        var
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = 0,
          filterid = $selfilter.attr('data-id') || '',
          filtervalue = null;

        filtervaluesInd = filtervalues ? filtervalues.length : 0;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === filterid) {
                filtervalue = filtervalues[filtervaluesInd];
                break;
            }
        }

        if (!filtervalue.params) {
            filtervalue.params = filtervalue.multiselect === true ? [] : {};
        }

        if (filtervalue != null) {
            addUserFilter($container, filtervalue);
        }
    }

    function onSelectSorter(evt) {
        var $selsorter = jQuery(evt.target);
        if (!$selsorter.hasClass('sorter-item')) {
            $selsorter = $selsorter.parents('li.sorter-item:first');
        }
        var $container = $selsorter.parents('div.advansed-filter:first');
        if ($container.length === 0 || $selsorter.length === 0) {
            return undefined;
        }

        var sorterid = $selsorter.attr('data-id') || null;
        if (sorterid) {
            var dsc = $selsorter.hasClass('asc-sort');
            if (!$selsorter.hasClass('asc-sort') && !$selsorter.hasClass('dsc-sort') && $selsorter.hasClass('dsc-sort-default')) {
                dsc = true;
            }
            setUserSorter($container, sorterid, { dsc: dsc });
        }
    }

    function onUserFilterClick(evt) {
        var
          $container = null,
          $filteritem = null,
          $target = jQuery(evt.target);

        $container = $target.parents('div.advansed-filter:first');
        $filteritem = $target.hasClass('filter-item') ? $target : $target.parents('div.filter-item:first');

        if ($target.hasClass('btn-show-hidden-filters')) {
            var $filter = jQuery(this).parents('div.advansed-filter:first');
            if ($filter.hasClass('showed-hidden-filters')) {
                return undefined;
            }

            onBodyClick(evt);
            jQuery(document.body).unbind('click', onBodyClick);
            $filter.addClass('showed-hidden-filters').find('div.hidden-filters-container:first').show();
            setTimeout(function () {
                jQuery(document.body).one('click', onBodyClick);
            }, 1);
            return undefined;
        }

        if ($target.hasClass('btn-show-datepicker')) {
            var $dateselector = $target.parents('span.advansed-filter-dateselector-date:first');
            showUserFilterDate(evt, $container, $filteritem, $dateselector);
            return undefined;
        }

        if ($container.length === 0 || $filteritem.length === 0) {
            return undefined;
        }

        if ($target.hasClass('btn-delete')) {
            var
              id = $filteritem.attr('data-id'),
              filtervalues = $container.data('filtervalues'),
              filtervaluesInd = 0;
            filtervaluesInd = filtervalues ? filtervalues.length : 0;
            while (filtervaluesInd--) {
                if (filtervalues[filtervaluesInd].id === id) {
                    break;
                }
            }
            removeUserFilter($container, filtervaluesInd !== -1 ? filtervalues[filtervaluesInd] : null, $filteritem);
            return undefined;
        }

        if ($target.hasClass('group-selector') || $target.parents('span.group-selector:first').length > 0) {
            showUserFilterGroup($container, $filteritem);
        }

        if ($target.hasClass('person-selector') || $target.parents('span.person-selector:first').length > 0) {
            showUserFilterPerson($container, $filteritem);
        }
    }

    function onUserSelectorClick(evt) {
        var
          $container = null,
          $filteritem = null,
          $target = jQuery(evt.target);

        $container = $target.parents('div.advansed-filter:first');

        if ($container.length === 0) {
            return undefined;
        }

        evt.stopPropagation();
        //if ($target.is('#userSelector') || $target.hasClass('adv-userselector-deps') || $target.parents('div.adv-userselector-deps:first').length > 0) {
        //  evt.stopPropagation();
        //}
    }

    function onGroupSelectorClick(evt) {
        var
          $container = null,
          $filteritem = null,
          $target = jQuery(evt.target);

        $container = $target.parents('div.advansed-filter:first');

        if ($container.length === 0) {
            return undefined;
        }

        evt.stopPropagation();
        //if ($target.is('.groupSelectorContainer', $container) || $target.is('.filterBox') || $target.parents('div.filterBox:first').length > 0) {
        //  evt.stopPropagation();
        //}
    }

    function onFilterInputKeyup(evt) {
        if (!checkKeyCode(evt.keyCode)) {
            return;
        }

        var
          $this = jQuery(this),
          value = $this.val(),
          $container = $this.parents('div.advansed-filter:first'),
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd;

        if (!filtervalues || filtervalues.length === 0) {
            return;
        }

        filtervaluesInd = filtervalues.length;

        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === 'text') {
                break;
            }
        }

        if (filtervaluesInd === -1 || (!filtervalues[filtervaluesInd].params && !value)) {
            return;
        }

        if (value) {
            setFilterItem($container, null, filtervalues[filtervaluesInd], { value: value });
        } else {
            unsetFilterItem($container, null, filtervalues[filtervaluesInd]);
        }
    }

    function onFilterInputKeyupHelper(evt) {
        clearTimeout(filterInputKeyupHandler);

        filterInputKeyupHandler = setTimeout(function() {
            onFilterInputKeyup.call(evt.target, evt);
        }, filterInputKeyupTimeout);
    }

    function onFilterInputEnter(evt) {
        switch (evt.keyCode) {
            case 13:
                onFilterInputKeyup.call(evt.target, evt);
        }
    }

    function checkKeyCode(keyCode) {
        var excluded = [
            9, //tab,
            16, //shift
            17, //ctrl	
            18, //alt
            19, //pause/break
            20, //caps lock
            27, //escape
            33, //page up
            34, //page down
            35, //end
            36, //home
            37, //left arrow
            38, //up arrow
            39, //right arrow
            40, //down arrow
            44, //print
            45, //insert
            91, //left window key
            92, //right window key
            93, //select key
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, //f1-f12
            144, //num lock
            145 //scroll lock
        ];

        return !excluded.includes(keyCode);
    }

    function onStartFilter(evt) {
        var
          $this = jQuery(this),
          $container = $this.parents('div.advansed-filter:first'),
          value = $container.find('input.advansed-filter-input:first').val(),
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = 0;

        if (!filtervalues || filtervalues.length === 0) {
            return undefined;
        }

        filtervaluesInd = filtervalues.length;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].id === 'text') {
                break;
            }
        }

        if (filtervaluesInd !== -1) {
            if (typeof value === 'string' && value.length > 0) {
                setFilterItem($container, null, filtervalues[filtervaluesInd], { value: value });
            } else {
                unsetFilterItem($container, null, filtervalues[filtervaluesInd]);
            }
        }
    }

    function onResetFilter(evt) {
        var
          $container = jQuery(this).parents('div.advansed-filter:first'),
          filtervalues = $container.data('filtervalues'),
          sortervalues = $container.data('sortervalues'),
          filtervaluesInd = 0,
          filtervalue = null,
          sorterfilter = null,
          $filters = $container.find('div.filter-item'),
          $filter = null,
          filtersInd = 0,
          wasRemover = false;

        lazyTrigger = true;
        $container.removeClass('has-filters')
          .find('div.advansed-filter-input:first').removeClass('has-value')
            .find('input.advansed-filter-input:first').val('');
        filtervaluesInd = filtervalues ? filtervalues.length : 0;
        while (filtervaluesInd--) {
            filtervalue = filtervalues[filtervaluesInd];
            if (filtervalue.id === 'text' && filtervalue.isset === true) {
                wasRemover = true;
                unsetFilterItem($container, null, filtervalue);
            }
            if (filtervalue.id === 'sorter') {
                sorterfilter = filtervalue;
            }
        }

        filtersInd = $filters.length;
        while (filtersInd--) {
            wasRemover = true;
            $filter = jQuery($filters[filtersInd]);
            removeUserFilterByObject($container, $filter, true);
        }

        // TODO: add reset sorter

        lazyTrigger = false;
        if (wasRemover === true) {
            callSetFilterTrigger($container, null, true);
        }
    }

    function onChangeHash(opts, $container, hash) {
        if (currentHash !== hash) {
            readLastState(opts, $container, undefined, true);
        }
    }
    /* </callbacks> */

    function readLastState(opts, $container, nonetrigger, clearAndRestore) {
        var
          containerfiltervalues = $container.length > 0 ? $container.data('filtervalues') || [] : [],
          filtervalues = getLocalStorageFilters(opts, $container);

        if (clearAndRestore === true) {
            var
              containerfiltervalueId = null,
              containerfiltervalue = null,
              containerfiltervaluesInd = 0,
              filtervaluesInd = 0;
            containerfiltervaluesInd = containerfiltervalues.length;
            while (containerfiltervaluesInd--) {
                containerfiltervalue = containerfiltervalues[containerfiltervaluesInd];
                containerfiltervalueId = containerfiltervalue.id;
                filtervaluesInd = filtervalues.length;
                while (filtervaluesInd--) {
                    if (filtervalues[filtervaluesInd].id == containerfiltervalueId) {
                        break;
                    }
                }
                if (filtervaluesInd === -1) {
                    filtervalues.push({
                        id: containerfiltervalue.id,
                        type: containerfiltervalue.type,
                        reset: true
                    });
                }
            }
        }

        if (filtervalues.length > 0) {
            initAdvansedFilter(null, $container, filtervalues, null, nonetrigger);
        }
    }

    function initAdvansedFilter(opts, $this, filtervalues, sortervalues, nonetrigger) {
        var
          wasCallTrigger = false,
          changeSorter = false,
          changeOptions = false,
          wasAdded = false,
          $container = $this.filter(':first'),
          containerfiltervalues = $container.length > 0 ? $container.data('filtervalues') || [] : [],
          containersortervalues = $container.length > 0 ? $container.data('sortervalues') || [] : [],
          containerfiltervaluesInd = 0,
          containersortervaluesInd = 0,
          filtervalue = null,
          sortervalue = null;

        changeOptions = false;
        if (opts) {
            if (!$container.hasClass('no-button') && opts.hasButton === false || $container.hasClass('no-button') && (typeof (opts.hasButton) == 'undefined' || opts.hasButton === true)) {
                changeOptions = true;
            }
            $container[opts.hasButton === false ? 'addClass' : 'removeClass']('no-button');
        }

        // d'ohhh
        if (sortervalues === null) {
            sortervalues = [];
            var filtervaluesInd = filtervalues.length;
            while (filtervaluesInd--) {
                if (filtervalues[filtervaluesInd].id === 'sorter' && filtervalues[filtervaluesInd].params) {
                    filtervalue = filtervalues[filtervaluesInd];
                    sortervalue = { id: filtervalue.params.id, title: filtervalue.params.title, dsc: filtervalue.params.dsc, sortOrder: filtervalue.params.sortOrder, selected: filtervalue.selected === true };
                    sortervalues.push(sortervalue);
                    break;
                }
            }
        }

        opts = opts || {};
        lazyTrigger = true;
        wasCallTrigger = false;

        changeSorter = false;
        var selItem = sortervalues.find(function(item) { return item.selected });
        for (var i = 0, n = sortervalues.length; i < n; i++) {
            sortervalue = sortervalues[i];
            if (sortervalue.visible === true) {
                enableUserSorter($container, sortervalue.id, true);
            }
            if (sortervalue.visible === false) {
                if (disableUserSorter($container, sortervalue.id, true)) {
                    changeSorter = true;
                }
            }
            if (sortervalue.selected === true) {
                //containersortervaluesInd = containersortervalues.length;
                //while (containersortervaluesInd--) {
                //  if (sortervalue.id == containersortervalues[containersortervaluesInd].id) {
                //    break;
                //  }
                //}
                //if (containersortervaluesInd !== -1) {
                //  extendItemValue(sortervalue, containersortervalues[containerfiltervaluesInd]);
                //}
                if (setUserSorter($container, sortervalue.id, { dsc: sortervalue.dsc === true || sortervalue.sortOrder === 'descending' }, true)) {
                    changeSorter = true;
                    break;
                }
            }
            if (!selItem && sortervalue.def === true) {
                setUserSorter($container, sortervalue.id, { dsc: sortervalue.dsc === true || sortervalue.sortOrder === 'descending' }, true);
                //break;
            }
        }

        var $availablesorteritems = $container.find('li.item-item.sorter-item').not('.hidden-item');
        if ($availablesorteritems.length === 1) {
            $container.find('.advansed-filter-sort-container:first').addClass('sorter-nochange');
        }

        wasAdded = false;
        
        var $filterItems = $container.find('li.item-item.filter-item');
        var $selectedFilters = $container.find('div.advansed-filter-filters:first div.filter-item');
        for (var i = 0, n = filtervalues.length; i < n; i++) {
            filtervalue = filtervalues[i];

            var $filterItem, $selectedfilterItem;
            if (filtervalue.hasOwnProperty("visible") || filtervalue.hasOwnProperty("enable") ||
                filtervalue.hasOwnProperty("reset") || filtervalue.hasOwnProperty('params')) {
                $filterItem = $filterItems.filter('[data-id="' + filtervalue.id + '"]');
                $selectedfilterItem = $selectedFilters.filter('[data-id="' + filtervalue.id + '"]');
            }

            if (filtervalue.visible === true) {
                showUserFilterByOption($filterItem);
            } else if (filtervalue.visible === false) {
                if (hideUserFilterByOption($container, $selectedfilterItem, $filterItem, true)) {
                    wasAdded = true;
                }
            }

            if (filtervalue.enable === true) {
                enableUserFilterByOption($filterItem);
            } else if (filtervalue.enable === false) {
                if (disableUserFilterByOption($container, $selectedfilterItem, $filterItem, true)) {
                    wasAdded = true;
                }
            }

            if (filtervalue.type === 'combobox' && filtervalue.hasOwnProperty('options')) {
                containerfiltervaluesInd = containerfiltervalues.length;
                while (containerfiltervaluesInd--) {
                    if (filtervalue.id === containerfiltervalues[containerfiltervaluesInd].id) {
                        break;
                    }
                }
                if (containerfiltervaluesInd !== -1) {
                    var containerfiltervalue = containerfiltervalues[containerfiltervaluesInd];
                    containerfiltervalue.options = filtervalue.options.slice(0);
                    // d'ohhh
                    var defaultvalue = '-1',
                        defaulttitle = null;

                    defaultvalue = typeof filtervalue.defaultvalue === 'string' && filtervalue.defaultvalue.length > 0 ? filtervalue.defaultvalue : defaultvalue;
                    defaulttitle = typeof filtervalue.defaulttitle === 'string' && filtervalue.defaulttitle.length > 0 ? filtervalue.defaulttitle : defaulttitle;
                    defaulttitle = typeof containerfiltervalue.defaulttitle === 'string' && containerfiltervalue.defaulttitle.length > 0 ? containerfiltervalue.defaulttitle : defaulttitle;
                    if (defaulttitle) {
                        containerfiltervalue.options.unshift({ value: defaultvalue, classname: 'default-value', title: defaulttitle, def: true });
                    }
                }
            }

            if (filtervalue.visible !== false && filtervalue.hasOwnProperty('params') && filtervalue.params) {
                containerfiltervaluesInd = containerfiltervalues.length;
                while (containerfiltervaluesInd--) {
                    if (filtervalue.id !== 'sorter' && filtervalue.id == containerfiltervalues[containerfiltervaluesInd].id) {
                        break;
                    }
                }
                if (containerfiltervaluesInd !== -1) {
                    if (addUserFilter($container, containerfiltervalues[containerfiltervaluesInd], filtervalue.params, true)) {
                        wasAdded = true;
                    }
                }
            }

            if (filtervalue.reset === true || (filtervalue.hasOwnProperty('params') && filtervalue.params === null)) {
                if (resetUserFilterByOption($container, $selectedfilterItem, true)) {
                    wasAdded = true;
                }
            }
        }

        if (filtervalue.hasOwnProperty("visible") || filtervalue.hasOwnProperty("enable")) {
            updateFiltersList($container);
        }

        if (opts.store === true) {
            if (!$container.hasClass('is-init')) {
                readLastState(opts, $container, true);
                if (opts.inhash === true) {
                    try {
                        ASC.Controls.AnchorController.bind((function (opts, $container) {
                            return function (hash) {
                                onChangeHash(opts, $container, hash);
                            };
                        })(opts, $container));
                    } catch (err) { }
                }
            }
        }
        lazyTrigger = false;

        if (!$container.hasClass('is-init')) {

            if (nonetrigger !== true) {
                resizeContainer($container);
                $container.addClass('is-init');
                callSetFilterTrigger($container);
                wasCallTrigger = true;
            }
        }

        if (changeOptions == true) {
            onBodyClick(true);
            resizeUserFilterContainer($container);
        }
        if (nonetrigger !== true && wasCallTrigger !== true && (wasAdded === true || changeSorter === true)) {
            wasCallTrigger = true;
            callSetFilterTrigger($container);
        }
        $container.trigger('adv-ready', [$container, opts, nonetrigger]);
    }

    function setAdvansedFilter($this, id, params) {
        var
          $container = $this.filter(':first'),
          containerfiltervalues = $container.length > 0 ? $container.data('filtervalues') || [] : [],
          containerfiltervaluesInd = 0;

        containerfiltervaluesInd = containerfiltervalues.length;
        while (containerfiltervaluesInd--) {
            if (id == containerfiltervalues[containerfiltervaluesInd].id) {
                break;
            }
        }
        if (containerfiltervaluesInd !== -1) {
            setFilterItem($container, null, containerfiltervalues[containerfiltervaluesInd], params);
        }
    }

    function getAdvansedFilter($this) {
        var
          $container = $this.filter(':first'),
          $filters = null,
          filterid = null,
          filtervalue = null,
          filtervalues = $container.data('filtervalues'),
          filtervaluesInd = 0,
          selectedfilters = [];

        if (!filtervalues || filtervalues.length === 0) {
            return selectedfilters;
        }

        updateTextFilter($container, true);

        filtervaluesInd = filtervalues.length;
        while (filtervaluesInd--) {
            if (filtervalues[filtervaluesInd].isset === true) {
                selectedfilters.unshift(filtervalues[filtervaluesInd]);
            }
        }

        return selectedfilters;
    };

    function clearAdvansedFilter($this) {
        var
          wasRemover = false,
          $container = $this.filter(':first'),
          filtervalues = $container.length > 0 ? $container.data('filtervalues') || [] : [],
          $filters = $container.find('div.advansed-filter-filters:first div.filter-item'),
          $filter = null,
          filtersInd = 0,
          filtervaluesInd = 0;

        lazyTrigger = true;
        $container.removeClass('has-filters')
          .find('div.advansed-filter-input:first').removeClass('has-value')
            .find('input.advansed-filter-input:first').val('');
        filtervaluesInd = filtervalues ? filtervalues.length : 0;
        while (filtervaluesInd--) {
            filtervalue = filtervalues[filtervaluesInd];
            if (filtervalue.id === 'text' && filtervalue.isset === true) {
                wasRemover = true;
                $container.find('input.advansed-filter-input:first').val('');
                unsetFilterItem($container, null, filtervalue);
            }
        }
        filtersInd = $filters.length;
        while (filtersInd--) {
            wasRemover = true;
            $filter = jQuery($filters[filtersInd]);
            removeUserFilterByObject($container, $filter, true);
        }
        lazyTrigger = false;
        if (wasRemover === true) {
            callSetFilterTrigger($container);
        }
    };


    function initHint($container, hintHtml) {
        var popupId = Math.floor(Math.random() * 1000000);
        $container.find('div.advansed-filter-hint-popup:first').attr('id', popupId).html(hintHtml);

        $container.addClass('has-hint').find('label.advansed-filter-hint:first').attr('data-popupid', popupId).click(function () {
            jQuery(this).helper({ BlockHelperID: jQuery(this).attr('data-popupid') });
            // check position
            var $hint = jQuery('#' + jQuery(this).attr('data-popupid')),
            top = $hint.offset().top;
            if ($hint.offset().top > $hint.outerHeight() + 20) {
                $hint.removeClass('valign-bottom').addClass('valign-top')
                .parents('div.advansed-filter-hint-popup-helper:first').removeClass('valign-bottom').addClass('valign-top')
                .find('div.cornerHelpBlock:first').removeClass('pos_bottom').addClass('pos_top');
            } else {
                $hint.removeClass('valign-top').addClass('valign-bottom')
                .parents('div.advansed-filter-hint-popup-helper:first').removeClass('valign-top').addClass('valign-bottom')
                .find('div.cornerHelpBlock:first').removeClass('pos_top').addClass('pos_bottom');
            }
        });
    };


    function updateEmptyInputs($els, filtervalues, sortervalues, opts, customdata) {
        var
          needIndex = opts.hasOwnProperty('zindex') ? opts.zindex === true : false,
          $template = null,
          $containers = $(),
          $container = null,
          elsInd = 0,
          $el = null;

        $template = $(createAdvansedFilter(filtervalues, sortervalues, opts));
        updateAdvansedFilter(opts, $template, filtervalues, sortervalues);
        setEvents($template, opts);

        customdata = customdata && typeof customdata === 'object' ? customdata : null;
        elsInd = $els.length;
        while (elsInd--) {
            $el = $($els[elsInd]).val('');
            if (($container = $el.is('div.advansed-filter') ? $el : $el.parents('div.advansed-filter:first')).length > 0) {
                if (needIndex === true) {
                    $container.css('z-index', 100 - elsInd);
                }
                continue;
            }
            $container = $template.clone(true);
            //$container.insertBefore($el).find('input.advansed-filter-complete:first').attr('id', $el.attr('id') || null);
            $container.insertBefore($el).attr('id', $el.attr('id') || null);
            $el.remove();
            $container = $container.hasClass('advansed-filter') ? $container : $container.find('div.advansed-filter:first');

            if (needIndex === true) {
                $container.css('z-index', 100 - elsInd);
            }

            $container.addClass('has-events').data('filtervalues', filtervalues).data('sortervalues', sortervalues).data('customdata', customdata).data('filteroptions', opts);

            if (opts.hasOwnProperty('help')) {
                $container.addClass('has-help').find('div.advansed-filter-helper:first')
                    .html([
                            '<label class="advansed-filter-helper-label"></label>',
                            opts.help
                    ]
                    .join(''));
            }

            if (!opts.hintDefaultDisable && opts.hasOwnProperty('hint')) {
                initHint($container, opts.hint);
            }

            $containers = $containers.add($container);
        }

        return $containers;
    }

    function updateControlInputs($els, filtervalues, sortervalues, opts, customdata) {
        var
          needIndex = opts.hasOwnProperty('zindex') ? opts.zindex === true : false,
          $containers = $(),
          $container = null,
          elsInd = 0,
          $el = null;

        customdata = customdata && typeof customdata === 'object' ? customdata : null;
        elsInd = $els.length;
        while (elsInd--) {
            $el = $($els[elsInd]).val('');
            if (($container = $el.is('div.advansed-filter') ? $el : $el.parents('div.advansed-filter:first')).length === 0) {
                continue;
            }

            if (!$container.hasClass('has-events')) {
                $container.find('input.advansed-filter:first').val('').attr('maxlength', opts.hasOwnProperty('maxlength') ? opts.maxlength : null);

                updateAdvansedFilter(opts, $container, filtervalues, sortervalues);
                setEvents($container, opts);

                if (needIndex === true) {
                    $container.css('z-index', 100 - elsInd);
                }

                $container.addClass('has-events').data('filtervalues', filtervalues).data('sortervalues', sortervalues).data('customdata', customdata).data('filteroptions', opts);
                if (opts.hasOwnProperty('help')) {
                    $container.addClass('has-help').find('div.advansed-filter-helper:first')
                    .html([
                            '<label class="advansed-filter-helper-label"></label>',
                            opts.help
                    ]
                    .join(''));
                }

                if (!opts.hintDefaultDisable && opts.hasOwnProperty('hint')) {
                    initHint($container, opts.hint);
                }
            }
            $containers = $containers.add($container);
        }
        return $containers;
    }

    $.fn.advansedFilter = $.fn.advansedfilter = function (opts) {
        if (arguments.length === 0) {
            return getAdvansedFilter($(this));
        }

        //if (arguments.length === 2) {
        //  return setAdvansedFilter($(this), arguments[0], arguments[1]);
        //}

        if (opts === null) {
            return clearAdvansedFilter($(this));
        }

        if (opts && typeof opts === 'string') {
            var
              cmd = opts,
              $container = $(this).filter(':first');

            switch (cmd) {
                case 'sort':
                    toggleSorterBlock($container, arguments.length > 1 ? arguments[1] === true : true);

                    resizeUserSorterContainer($container);
                    resizeContainer($container);
                    resizeUserFilterContainer($container);
                    return undefined;
                case 'filters':
                    return getContainerFilters($container);
                case 'hash':
                    return getStorageHash($container);
                case 'storage':
                    return getLocalStorageFilters();
                case 'resetText':
                    return resetTextFilter($container);
                case 'resize':
                    if ($container.is(":visible")) {
                        onBodyClick();
                        resizeUserSorterContainer($container);
                        resizeContainer($container);
                        resizeUserFilterContainer($container);
                     }
                    return undefined;
                default:
                    return setAdvansedFilter($container, arguments[0], arguments[1]);
            }
            return undefined;
        }

        var
          customdata = null,
          colcount = 1,
          opts = opts && typeof opts === 'object' ? opts : {},
          filtervalues = opts.hasOwnProperty('filters') ? opts.filters : [],
          sortervalues = opts.hasOwnProperty('sorters') ? opts.sorters : [],
          resources = opts.hasOwnProperty('resources') ? opts.resources : {},
          maxfilters = opts.hasOwnProperty('maxfilters') ? isFinite(+opts.maxfilters) && +opts.maxfilters >= -1 ? +opts.maxfilters : -1 : -1,
          $containers = null,
          $this = $(this);

        if ($this.length === 0) {
            return $this;
        }

        for (var fld in resources) {
            if (resources.hasOwnProperty(fld)) {
                Resources[fld] = resources[fld];
            }
        }

        opts.colcount = opts.hasOwnProperty('colcount') && isFinite(+opts.colcount) ? +opts.colcount : colcount;
        opts.colcount = opts.colcount > 4 ? 4 : opts.colcount;

        if (!opts.hintDefaultDisable && ASC.Resources.Master.FilterHelpCenterLink) {
        opts.hint = opts.hasOwnProperty('hint') ? opts.hint : ASC.Resources.Master.Resource.AdvansedFilterInfoText.format(
                                '<b>',
                                '</b>',
                                '<br/><br/><a href="' + ASC.Resources.Master.FilterHelpCenterLink + '" target="_blank">',
                                '</a>');
        }

        customdata = { maxfilters: maxfilters };
        filtervalues = extendItemValues(filtervalues, jQuery.parseJSON(jQuery.toJSON(filterValues)));
        sortervalues = extendItemValues(sortervalues, jQuery.parseJSON(jQuery.toJSON(sorterValues)));

        if (($containers = updateControlInputs($this, filtervalues, sortervalues, opts, customdata)).length === $this.length) {
            addReadyEvent(initAdvansedFilter, [opts, $containers, filtervalues, sortervalues, opts.nonetrigger]);
            return $containers;
        }
        $containers = updateEmptyInputs($this, filtervalues, sortervalues, opts, customdata);
        addReadyEvent(initAdvansedFilter, [opts, $containers, filtervalues, sortervalues, opts.nonetrigger]);
        return $containers;
    };
})(jQuery, window, document, document.body);
