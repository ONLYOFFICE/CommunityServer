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


$(function() {
    function initDemo () {
        if (location.href.indexOf("demopreview") > -1 && $("#embeddedEditor").length) {
            $(".layout-content").css("max-width", "1085px");
            var hash = location.hash.replace("#", "") || "text";
            $(".layout-side .treeview a").removeClass("selected");
            $("#" + hash + "Demo").addClass("selected");
            $("#embeddedEditor").html("<div id=\"placeholder\"></div>");
            window.docKey = null;
            initEditor(null, null, "edit", "desktop");
        }
    }

    $(window).on("hashchange", function () {
        initDemo();
    });

    $(".search-box .btn").click(function () {
        $(this).parent().submit();
    });

    $(".search-box input").bind("change paste keyup", function () {
        $(this).val() ? $(".search-box .search-clear").show() : $(".search-box .search-clear").hide();
    });

    $(".search-box .search-clear").click(function () {
        $(".search-box input").val("");
        $(this).hide();
    });

    $("#sideNav").treeview({
        collapsed: window.Config.TreeviewCollapsed,
        animated: "medium",
        unique: false,
        persist: "location"
    });

    $("pre").each(function (i, block) {
        hljs.highlightBlock(block);
    });

    if ($("#clipLink").length) {
        ZeroClipboard.setMoviePath(window.Config.ZeroclipboardSwfUrl);
        var clip = new window.ZeroClipboard.Client();
        clip.setText(location.href + "#returns");
        clip.addEventListener("oncomplete", function () { alert("Link was copied to clipboard"); });
        clip.glue("clipLink");
    }

    initDemo();
});