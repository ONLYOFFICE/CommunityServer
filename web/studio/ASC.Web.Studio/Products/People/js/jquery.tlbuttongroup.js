(function ($, win, doc, body) {
    var supportedClient = true,
        jqTemplateId = "jqTemplateButtonGroup",
        openedClassname = "tl-btngroup-opened",
        disabledClassname = "disabled",
        $body = null;

    // check client
    supportedClient = true;

    function callTriggerShow ($btngroup) {
        var handler = $btngroup.data("opt").onShow;
        if (typeof handler === "function") {
            handler.apply($btngroup, []);
        }
    }

    function callTriggerHide ($btngroup) {
        var handler = $btngroup.data("opt").onHide;
        if (typeof handler === "function") {
            handler.apply($btngroup, []);
        }
    }

    function tmpl (data) {
        var o = document.createElement("span");
        o.setAttribute("class", "btn-group" + (data.classname ? " " + data.classname : ""));
        o.innerHTML = [
          "<span class='menu-helper'>",
              "<span class='menu-container'>",
                  "<span class='menu-list'></span>",
              "</span>",
          "</span>"
        ].join("");

        return $(o);
    }

    // callbacks
    function onBodyClick (evt) {
        var $btngroup = null,
            $btngroups = $body.unbind("click", onBodyClick).find("span." + openedClassname).removeClass(openedClassname),
            btngroupsInd = 0;
        btngroupsInd = $btngroups.length;
        while (btngroupsInd--) {
            $btngroup = $($btngroups[btngroupsInd]);
            $btngroup.find("span.menu-container").css("display", "none");
            callTriggerHide($btngroup);
        }
    }

    function onTitleClick(evt) {        
        var $btngroup = $(this).parents('span.btn-group:first');
        if ($btngroup.hasClass(openedClassname) || $btngroup.hasClass(disabledClassname)) {
          return undefined;
        }
        onBodyClick();
        $btngroup.addClass(openedClassname).find("span.menu-container:first").css("display", "block");
        callTriggerShow($btngroup);
        
        setTimeout(bindBodyEvents, 0);
    }

    function bindBodyEvents () {
        $body.bind("click", onBodyClick);
    }

    function bindEvents ($btngroup) {
        $btngroup.children(".btn:first")
          .bind("click", onTitleClick);
    }

    function initButtonGroup ($btnTitle, $btns, opt) {
        var $btngroup,
            classname = "",
            btnTitleClasses = ($btnTitle.attr("class") || "").split(/\s+/),
            btnTitleClassesInd = 0,
            incorrectClasses = ["btn", "border-radius"],
            incorrectClassesInd = 0,
            data = {
                classname: ""
            };

        incorrectClassesInd = incorrectClasses.length;
        while (incorrectClassesInd--) {
            classname = incorrectClasses[incorrectClassesInd];
            btnTitleClassesInd = btnTitleClasses.length;
            while (btnTitleClassesInd--) {
                if (classname == btnTitleClasses[btnTitleClassesInd]) {
                    btnTitleClasses.splice(btnTitleClassesInd, 1);
                    break;
                }
            }
        }
        data.classname = btnTitleClasses.join(" ");

        //$btngroup = $.tlTmpl(jqTemplateId, data);
        $btngroup = tmpl(data);
        $btnTitle.before($btngroup);
        $btngroup.append($btnTitle);
        $btngroup.find("span.menu-list:first").append($btns);

        $btngroup.data("opt", opt);
        bindEvents($btngroup);
    }

    $.fn.tlButtonGroup = function (selector, opt) {
        opt = $.extend({
            onShow: null,
            onHide: null
        }, opt);

        $body = $body || $(document.body);
        if (supportedClient) {
            initButtonGroup(this.filter(":first"), $(selector), opt);
        }

        return this;
    };
})(jQuery, window, document, document.body);