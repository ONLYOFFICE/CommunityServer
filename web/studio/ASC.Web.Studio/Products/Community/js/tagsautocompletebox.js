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


/*
-- global variables

LoaderPath - loader image path
ContainerElementID - container id (div) where will be hint

--local

ajaxClassHandler - server Ajax handler class
inputID - id of input
hsItemClass - CSS item class
hsItemSelectClass - CSS selected item class
code - js after select item
selectCode - after select js code
serverFunc - sercer ajax method name
serverParams - server ajax method params
isAppend - just one tag or many
isVisibleHelp
*/

var LoaderPath = StudioManager.GetImage('loader_16.gif');
var ContainerElementID = 'TagsAutocompleteContainer';

var tagsMaxCount = 10;

var tagsAutocompleteImageID = 'TagsAutocompleteServerRequestImage';

var popupWindowTopAlign = 7;
var popupWindowWidth = 205;

HelpItem = function (accord, value, rank, help) {
    this.Accord = accord;
    this.Value = value;
    this.Help = help;
    this.Rank = rank;
    this.Helper = null;
    this.ID = 'ish_0';
};

var hv = new Array(0);
var isDocumentLoad = false;
jq(document).ready(function () {
    isDocumentLoad = true;
    try {
        //Appends div tag for autocomplete box display
        jq('#studioPageContent').append('<div id="' + ContainerElementID + '" style="position: absolute; top: 0px; left: 0px; margin-top: -3px;">&nbsp;</div>');
        createSearchHelper();
    } catch (err) {
    }
});


var SearchHelper = function(inputID, hsItemClass, hsItemSelectClass, code, selectCode, ajaxClassHandler, serverFunc, serverParams, isAppend, isVisibleHelp) {
    var varName = 'hv[' + hv.length + ']';
    var helperID = 'sh_hv_' + hv.length + '_';
    if (isAppend != 'undefined' && isAppend != undefined) {
        this.IsAppend = isAppend;
    } else {
        this.IsAppend = false;
    }

    if (isVisibleHelp != 'undefined' && isVisibleHelp != undefined) {
        this.IsVisibleHelp = isVisibleHelp;
    } else {
        this.IsVisibleHelp = true;
    }

    this.AjaxClassHandler = ajaxClassHandler;
    this.SelectedItem = null;
    this.VarName = varName;
    var inputElement = jq('[id$="' + inputID + '"]');
    this.InputID = inputElement.attr('id');
    this.ExecCode = code;
    this.IsExecCode = false;
    this.SelectCode = selectCode;
    this.ServerFunction = serverFunc;
    this.ServerParams = serverParams;
    this.ItemClass = hsItemClass;
    this.ItemSelectClass = hsItemSelectClass;
    this.Items = new Array();
    this.IsCache = false;
    this.TempCache = new Array();
    this.CacheTitle = '';
    this.TimerHandler = null;
    this.Cache = new Object();
    this.ID = helperID;
    this.CurrentItemID = null;
    this.IDLoader = helperID + '_loader';
    var el = jq('#' + this.InputID).get(0);

    var z_index = jq(el).css('z-index');

    while (el.tagName.toLowerCase() != 'body' && (z_index == '' || z_index == null || z_index == 'undefined')) {
        el = jq(el).parent().get(0);
        z_index = jq(el).css('z-index');
    }

    if (z_index == null || z_index == 'undefined') {
        z_index = '';
    } else {
        z_index = 'z-index:' + z_index + ';';
    }

    hv.push(this);

    if (isDocumentLoad) {
        try {
            eval(varName + '=this;');

            var bodySH = '<div id=' + helperID + ' style="' + z_index + ' background-color:white; padding:0px; margin:0px; width: 100%; position:absolute; display:none;"></div>';
            var bodySHLoader = '<div id=' + helperID + '_loader style="' + z_index + ' padding:0px; margin:0px; position:absolute; display:none;"><img src="' + LoaderPath + '" alt="" id="' + tagsAutocompleteImageID + '"></div>';
            if (jq('#' + helperID).length > 0) {
                jq('#' + helperID).remove();
                jq('#' + helperID + '_loader').remove();
            }

            jq('#' + ContainerElementID).append(bodySH + bodySHLoader);
            eval('jq("#' + this.InputID + '").keyup(function(event){' + varName + '.Handler(event);})');
        }
        catch (e) { };
    } else {
        jq(document).ready(function() {
            try {
                eval('isDocumentLoad=true;');
                var bodySH = '<div id=' + helperID + ' style="' + z_index + ' background-color:white; padding:0px; margin:0px; width: 100%; position:absolute; display:none;"></div>';
                var bodySHLoader = '<div id=' + helperID + '_loader style="' + z_index + 'padding:0px; margin:0px; position:absolute; display:none;"><img src="' + LoaderPath + '" alt="" id="' + tagsAutocompleteImageID + '"></div>';

                jq('#' + ContainerElementID).append(bodySH + bodySHLoader);
                eval('jq("#' + this.InputID + '").keyup(function(event){' + varName + '.Handler(event);})');
            }
            catch (e) { };
        });
    }

    this.Handler = function(e) {
        var code;
        if (!e) var e = window.event;
        if (e.keyCode) code = e.keyCode;
        else if (e.which) code = e.which;

        var inputWidth = jq('#' + this.InputID).width();
        if (inputWidth != 0) {
            jq('#' + ContainerElementID).width(inputWidth + 2);
        }

        var isVisible = jq('#' + this.ID).is(':visible');
        var text = jq.trim(jq('#' + this.InputID).val());

        if (code == 38 && isVisible)//up
        {
            this.PrevItem();
        }
        else if (code == 40 && isVisible)//down
        {
            this.NextItem();
        }
        else if (code == 13 && isVisible && this.IsPreSelected()) {
            this.SelectItem();
        }
        else if ((code <= 57 && code >= 48)
                || (code >= 96 && code <= 111)
                || (code >= 65 && code <= 90)
                || code == 61 || code == 220 || code == 192 || code == 219
                || code == 221 || code == 59 || code == 222
                || code == 188 || code == 190 || code == 191 || code == 32
                || (code == 8 && isVisible)
                || (code == 8 && text != '')
                || (code == 46 && text != '')
                || (code == 46 && isVisible)) {
            if (text == '') {
                this.Close();
                return;
            }

            this.IsCache = false;
            if (this.Cache != null) {
                for (property in this.Cache) {
                    if (text == property) {
                        this.Items = new Array().concat(this.Cache[property]);
                        this.IsCache = true;
                        this.CacheTitle = 'from cache key=' + property;
                        break;
                    }
                }
            }
            if (!this.IsCache) {
                var pos = jq('#' + this.InputID).offset();
                var inputWidth = jq('#' + this.InputID).width();
                var loaderWidth = jq('#' + this.IDLoader).width();

                jq('#' + this.IDLoader).css({
                    'left': pos.left + inputWidth - loaderWidth + 'px',
                    'top': (pos.top + popupWindowTopAlign) + 'px'
                });

                var var_name = this.VarName;


                AjaxPro.onLoading = function (b) {
                    if (b) {
                        eval('jq("#"+' + var_name + '.IDLoader).show();');
                    }
                }
                if (this.TimerHandler != null)
                    clearTimeout(this.TimerHandler);
                this.TimerHandler = setTimeout(


                    this.AjaxClassHandler + '.' + this.ServerFunction +
                        '(' + this.ServerParams + '"' + text + '","' + this.VarName + '",' + tagsMaxCount + ',' + this.VarName + '.SearchCallback);'
                    , 200);
            }
            else {
                if (this.Items != null && this.Items.length > 0) {
                    this.Show();
                } else {
                    this.Close();
                }
            }
        }
        else if (code == 27 && isVisible) {
            this.Close();
        }
        else if ((code == 13 && !isVisible)
                || (code == 13 && !this.IsPreSelected())) {
            this.Close();
            this.IsExecCode = true;
            eval(this.ExecCode);
        }
    };

    this.ClearCache = function() {
        this.Cache = new Object();
    };

    this.SearchCallback = function(result) {
        var res = result.value;
        //---conflict zone (variable names)
        var res = result.value;
        var text = res.rs3;
        var var_name = res.rs4;
        var values = res.rs1.split('$');
        var helps = res.rs2.split('$');
        //-----------

        eval(var_name + '.TempCache.clear();');
        eval('jq("#"+' + var_name + '.IDLoader).hide();');
        eval('if(jq("#"+' + var_name + '.InputID).val()=="' + text + '") ' + var_name + '.DeleteItems();');
        for (var i = 0; i < values.length; i++) {
            if (values[i] != '' && values[i] != null)
                eval(var_name + '.AddHelpItemToTempCache(new HelpItem("","' + values[i] + '",' + i + ',"' + helps[i] + '"));');
        }
        eval(var_name + '.Cache["' + text + '"]=' + var_name + '.TempCache.concat(new Array());');
        eval('if(jq("#"+' + var_name + '.InputID).val()=="' + text + '"){ ' + var_name + '.Items = ' + var_name + '.TempCache.concat(new Array()); if(' + var_name + '.Items!=null && ' + var_name + '.Items.length>0) ' + var_name + '.Show(); else ' + var_name + '.Close(); }');
    };

    this.IsPreSelected = function() {
        for (var i = 0; i < this.Items.length; i++) {
            if (jq('#' + this.Items[i].ID).attr('class') == this.ItemSelectClass)
                return true;
        }
        return false;
    };

    this.AddHelpItem = function(helpItem) {
        helpItem.Helper = this;
        helpItem.ID = this.ID + '_ish_' + (this.Items.length + 1);
        this.Items.push(helpItem);
        this.Items.sort(this._compareItems);

    };

    this.AddHelpItemToTempCache = function(helpItem) {
        helpItem.Helper = this;
        helpItem.ID = this.ID + '_ish_' + (this.TempCache.length + 1);
        this.TempCache.push(helpItem);
        this.TempCache.sort(this._compareItems);
    };

    this._compareItems = function(a, b) {
        if (a.Rank < b.Rank)
            return -1;
        if (a.Rank > b.Rank)
            return 1;
        return 0;
    };

    this.DeleteItems = function() {
        this.Items.clear();
    };

    this.MouseClick = function(event) {
        var elt = (event.target) ? event.target : event.srcElement;
        if (elt != jq('#' + this.InputID).get(0)) {
            this.Close();
        }
    };

    this.Close = function() {
        jq('#' + this.ID).hide();
        this.DeleteItems();
        jq("body").unbind("click");
    };

    this.GetItemById = function(idItem) {
        for (var i = 0; i < this.Items.length; i++) {
            var item = this.Items[i];
            if (item.ID == idItem) {
                return item;
            }
        }
        return null;
    };

    this.ClearItems = function() {
        for (var i = 0; i < this.Items.length; i++)
            jq('#' + this.Items[i].ID).attr('class', this.ItemClass);
    };

    this.NextItem = function() {
        if (this.Items.length > 0) {
            var nextItemId = 0;
            for (var i = 0; i < this.Items.length; i++) {
                if (jq('#' + this.Items[i].ID).attr('class') == this.ItemSelectClass) {
                    if (i == this.Items.length - 1)
                        nextItemId = 0;
                    else
                        nextItemId = i + 1;
                    break;
                }
            }
            this.ClearItems();
            jq('#' + this.Items[nextItemId].ID).attr('class', this.ItemSelectClass);
        }
    };

    this.PrevItem = function() {
        if (this.Items.length > 0) {
            var nextItemId = this.Items.length - 1;
            for (var i = 0; i < this.Items.length; i++) {
                if (jq('#' + this.Items[i].ID).attr('class') == this.ItemSelectClass) {
                    if (i == 0)
                        nextItemId = this.Items.length - 1;
                    else
                        nextItemId = i - 1;
                    break;
                }
            }
            this.ClearItems();
            jq('#' + this.Items[nextItemId].ID).attr('class', this.ItemSelectClass);
        }
    };

    this.PreSelectItem = function(idItem) {
        this.ClearItems();
        var item = this.GetItemById(idItem);
        jq('#' + item.ID).attr('class', this.ItemSelectClass);
    };

    this.SelectItem = function() {
        for (var i = 0; i < this.Items.length; i++) {
            var item = this.Items[i];
            if (jq('#' + item.ID).attr('class') == this.ItemSelectClass) {
                this.SelectedItem = item;
                if (this.IsAppend) {
                    var vals = jq('#' + this.InputID).val().split(',');
                    var result = new String();
                    var isFirst = 1;
                    for (var i = 0; i < vals.length - 1; i++) {
                        if (jq.trim(vals[i]) != '') {
                            if (isFirst == 1) {
                                result += jq.trim(vals[i]);
                                isFirst = 0;
                            }
                            else {
                                result += ',' + jq.trim(vals[i]);
                            }
                        }
                    }
                    if (isFirst == 1) {
                        result += item.Value;
                        isFirst = 0;
                    }
                    else {
                        result += ',' + item.Value;
                    }
                    jq('#' + this.InputID).val(result + ',');
                }
                else
                    jq('#' + this.InputID).val(item.Value);

                eval(this.SelectCode);
                this.Close();
                break;
            }
        }
    };

    this.LeaveItem = function(idItem) {
        this.ClearItems();
        var item = this.GetItemById(idItem);
        jq('#' + item.ID).attr('class', this.ItemClass);
    };

    this.Show = function() {
        //if(this.IsExecCode)
        //  return;

        this.SelectedItem = null;
        var pos = jq('#' + this.InputID).offset();
        var inputHeight = jq('#' + this.InputID).height();



        jq('#' + this.ID).css({
            'left': pos.left + 'px',
            'top': (pos.top + inputHeight + popupWindowTopAlign) + 'px'
        });
        var items_context = '';
        for (var i = 0; i < this.Items.length; i++) {
            var item = this.Items[i];
            items_context += '<div class=\'' + this.ItemClass + '\' id=\'' + item.ID +
                            '\' onmouseover=\'javascript:' + this.VarName + '.PreSelectItem("' + item.ID + '");\'' +
                            ' onclick=\'javascript:' + this.VarName + '.SelectItem();\'><table width=100% cellspacing=0 cellpadding=3><tr><td>' + item.Value + '</td><td style="text-align:right; color:gray;">' +
                            '<div style="float:right;' + (this.IsVisibleHelp ? '' : ' display:none;') + '">' + item.Help + '</div></td></tr></table></div>';

        }
        jq('#' + this.ID).html(items_context);
        jq('#' + this.ID).show();

        jq("body").unbind("click");

        var varName = this.VarName;
        jq('body').click(function(event) {
            eval(varName + '.Close();');
        });
    };
}