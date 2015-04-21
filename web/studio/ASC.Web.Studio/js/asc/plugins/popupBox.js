/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
PopupBoxManager = new function() {
    this.Collection = new Array();
    this.TimerCollection = new Array();

    this.RegistryPopupBox = function(popupBox) {
        this.Collection[popupBox.ID] = popupBox;
    };

    this.GetPopupBoxByID = function(id) {
        return this.Collection[id];
    };

    this.AjaxCallback = function(result) { };
};

var PopupBoxContainerElementID = 'container';

PopupBox = function(id, width, height, backgroundCSSClass, borderCSSClass, serverMethodFullName, params) {
    this.Height = height;
    this.Width = width;

    this.BorderCSSClass = borderCSSClass;
    this.BackgroundCSSClass = backgroundCSSClass;

    this.ID = id;
    this.ContentID = '_' + id;
    this.IsVisible = false;

    this.TimerHandler = null;
    this.CurrentElementID = '';
    this.PreviousElementID = '';

    this.VisibleWidth = -1;
    this.VisibleHeight = -1;

    this.ServerMethodFullName = serverMethodFullName;
    if (typeof (params) === "object") {
        if (params.hasOwnProperty("apiMethodName")) {
            this.ApiMethodName = params.apiMethodName;
        }
        if (params.hasOwnProperty("tmplName")) {
            this.TmplName = params.tmplName;
        }
        if (params.hasOwnProperty("customFactory")) {
            this.CustomFactory = params.customFactory;
        }
    }

    this.BeginBox = '<div class="' + this.BorderCSSClass + ' ' + this.BackgroundCSSClass + '" style="z-index:9999999; position:absolute; display:none; width:' + this.Whidth + 'px;" id=' + this.ID + '>';
    this.EndBox = '</div>';
    this.Content = '<div id=' + this.ContentID + '></div>';

    this.Init = function() {
        jq('#' + PopupBoxContainerElementID).append(this.BeginBox + this.Content + this.EndBox);
    };


    PopupBoxManager.RegistryPopupBox(this);
    if (jq('#' + PopupBoxContainerElementID).length > 0) {
        this.Init();
    } else {
        var popupBoxID = this.ID;
        jq(document).ready(function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).Init();
        });
    };

    this.SetContent = function(content) {
        jq('#' + this.ContentID).html(content);
        jq('#' + this.ID).css({
            'width': this.Width + 'px'
        });

    };

    this.CheckVisible = function() {
        if (jq('#' + this.ID).is(':visible') && this.IsVisible == false) {
            jq("#" + this.ID).fadeOut("slow");
            clearInterval(this.checkInterval);
        }
    };

    this.RegistryElement = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        jq('#' + idElement).mouseover(function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).ExecAndShow(idElement, methodParams);
        });

        jq('#' + idElement).mouseout(function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
        });

        jq('#' + idElement).click(function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
        });
    };

    this.RegistryElementOnLive = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        jq(document).on('mouseover', '#' + idElement, function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).ExecAndShow(idElement, methodParams);
        });

        jq(document).on('mouseout', '#' + idElement, function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
        });

        jq(document).on('click', '#' + idElement, function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
        });
    };

    this.ExecAndShow = function(idElement, methodParams) {
        this.CurrentElementID = idElement;
        if (this.CurrentElementID == this.PreviousElementID && 1 == 2) {
            setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').Show('" + idElement + "')", 850);
            return;
        } else {
            if (this.TimerHandler)
                clearInterval(this.TimerHandler);

            this.PreviousElementID = this.CurrentElementID;

            if (typeof (this.ApiMethodName) != "undefined" && this.ApiMethodName != "") {
                this.TimerHandler = setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').ExecApiCode('" + idElement + "','" + methodParams + "')", 850);
            } else {
                this.TimerHandler = setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').ExecAjaxCode('" + idElement + "','" + methodParams + "')", 850);
            }
        }
    };

    this.ExecApiCode = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        eval(this.ApiMethodName + "({popupBoxID: \"" + this.ID + "\"}," + methodParams + "," +
        "{" +
        "success:" + this.ApiCallback + "," +
        "before:" + function() {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).SetContent('<img class="popupbox-loader" src="' + StudioManager.GetImage('loader_32.gif') + '" alt="">');
                PopupBoxManager.GetPopupBoxByID(popupBoxID).Show(idElement);
            }
        +"})");
    };

    this.ExecAjaxCode = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        AjaxPro.onLoading = function(b) {
            if (b) {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).SetContent('<img class="popupbox-loader" src="' + StudioManager.GetImage('loader_32.gif') + '" alt="">');
                PopupBoxManager.GetPopupBoxByID(popupBoxID).Show(idElement);
            }
        };
        eval(this.ServerMethodFullName + "(" + methodParams + ",\"" + this.ID + "\",this.AjaxCallback);");
    };

    this.AjaxCallback = function(result) {
        AjaxPro.onLoading = function(b) { };
        var ppb = PopupBoxManager.GetPopupBoxByID(result.value.rs1);
        ppb.SetContent(result.value.rs2);
        ppb.SetPopupPosition(ppb.CurrentElementID);
    };

    this.ApiCallback = function(params, data) {
        var ppb = PopupBoxManager.GetPopupBoxByID(params.popupBoxID);
        if (typeof (ppb.CustomFactory) === "function") {
            ppb.CustomFactory.apply(this, [data]);
        }
        data.isPrivateForMe = data.isPrivate;
        if (data.isPrivate && data.accessList) {
            for (var i = 0; i < data.accessList.length; i++) {
                if (data.accessList[i].id == Teamlab.profile.id) {
                    data.isPrivateForMe = false;
                }
            }
        }
        var $newContent = jq.tmpl(ppb.TmplName, data);
        ppb.SetContent($newContent);
        ppb.SetPopupPosition(ppb.CurrentElementID);
    };

    this.SetPopupPosition = function (idElement) {
        if (jq('#' + idElement).length == 0) return;
        var ppbHeight = jq('#' + this.ID).height(),

            w = jq(window),
            TopPadding = w.scrollTop(),
            LeftPadding = w.scrollLeft(),
            ScrWidth = w.width(),
            ScrHeight = w.height(),

            idBox = this.ID,
            pos = jq('#' + idElement).offset(),
            elHeight = jq('#' + idElement).height(),

            left = "",
            top = "";

        if ((pos.left + this.Width) > (LeftPadding + ScrWidth)) {
            left = pos.left - this.Width + 'px';
        } else {
            left = pos.left + 'px';
        }

        if ((pos.top + ppbHeight + elHeight) > (TopPadding + ScrHeight)) {
            top = pos.top - 5 - ppbHeight + 'px';
        } else {
            top = pos.top + 3 + elHeight + 'px'
        }
        jq('#' + idBox).css({
            'left': left,
            'top': top
        });
    };

    this.Show = function(idElement) {
        this.SetPopupPosition(idElement);

        var idBox = this.ID;
        this.VisibleWidth = -1;
        this.VisibleHeight = -1;

        if (jq('#' + idBox).is(':visible') == false) {
            this.IsVisible = true;
            jq("#" + idBox).fadeIn("slow");

            jq('#' + idBox).mouseout(function() {
                PopupBoxManager.GetPopupBoxByID(idBox).SetVisible(false);
            });

            jq('#' + idBox).mouseover(function() {
                PopupBoxManager.GetPopupBoxByID(idBox).SetVisible(true);
            });
        }
        var self = this;
        self.checkInterval = setInterval(function () {
            self.CheckVisible();
        }, 1000);
    };

    this.SetVisible = function(state) {
        if (jq('#' + this.ID).is(':visible'))
            this.IsVisible = state;
    };

    this.StopTimer = function(id) {
        try {
            this.IsVisible = false;
            clearInterval(this.TimerHandler);
            this.TimerHandler = null;
        }
        catch (e) { };
    };
};