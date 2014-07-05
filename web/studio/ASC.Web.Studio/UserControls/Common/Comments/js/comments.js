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

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
var CommentsManagerObj = new function() {
    this.obj = null;
    this.iFCKEditor = null;

    this.javaScriptAddCommentFunctionName = "";
    this.javaScriptLoadBBcodeCommentFunctionName = "";
    this.javaScriptUpdateCommentFunctionName = "";
    this.javaScriptCallBackAddComment = "";
    this.javaScriptPreviewCommentFunctionName = "";

    this.OnEditedCommentJS = "";
    this.OnRemovedCommentJS = "";
    this.OnCanceledCommentJS = "";
    this.FckDomainName = "";
    this.currentCommentID = "";

    this.FckUploadHandlerPath = "";

    this.isSimple = false;
    this.inactiveMessage = "";
    this.RemoveAttachButton = "";
    this.CurrentAttachID = 0;

    this.mainLoader = "";

    this.maxLevel = 8;

    this._jsObjName = "";
    this.PID = "";

    this.isDisableCtrlEnter = false;
    this.EnableAttachmets = false;

    this.HandlerTypeName = "";

    this.InitEditor = function(FCKBasePath, FCKToolbar, FCKHeight, FCKWidth, FCKEditorAreaCss) {
        __FCKeditorNS = null;
        FCKeditorAPI = null;

        CKEDITOR.replace('commentEditor', { toolbar: 'Comment', extraPlugins: 'teamlabquote', filebrowserUploadUrl: CommentsManagerObj.FckUploadHandlerPath, height: "200" });
        this.oFCKeditor = CKEDITOR.instances.commentEditor;
    };

    this.Redraw = function() {
        oddcnt = jq('#mainContainer div[id^=comment_]:even')
                    .removeClass('')
                    .addClass('tintMedium')
                    .css('border-top', '1px solid #DDD')
                    .css('border-bottom', '1px solid #DDD')
                    .length;
        evencnt = jq('#mainContainer div[id^=comment_]:odd')
                    .removeClass('tintMedium')
                    .css('border-top', '')
                    .css('border-bottom', '')
                    .addClass('').length;

        if (oddcnt == evencnt)
            jq('#mainContainer').css('border-bottom', '1px solid #DDD');
        else
            jq('#mainContainer').css('border-bottom', '');
    };

    this.ResponseToComment = function(obj, id) {
        this.obj = obj.id.replace("response_", "");
        this.SetParentComment(obj.id.replace("response_", ""));
        this.SetAction("add", null);
        this.ShowCommentBox("", obj);
    };

    this.AddNewComment = function() {
        this.obj = null;
        this.SetParentComment("");
        this.SetAction("add", null);
        this.ShowCommentBox("", null);

        jq("#comment_attachments").html("");
        this.CurrentAttachID = 0;
        this.currentCommentID = "";
        jq('#hdnCommentID').val('');
    };

    this.EditComment = function(obj, id) {
        this.obj = obj.id.replace("edit_", "");
        this.SetParentComment("");
        this.SetAction("update", obj.id.replace("edit_", ""));
        this.ShowCommentBox(obj.id.replace("edit_", ""));

        jq("#comment_attachments").html("");
        this.CurrentAttachID = 0;
        this.FillAttachments(id);
        this.currentCommentID = id;
    };

    this.ShowCommentBox = function(id) {
        CommentsManagerObj.hideAddButton();

        if (!this.isSimple) {
            var ContentDiv = document.getElementById('content_' + id);
            var iFCKEditor = CKEDITOR.instances.commentEditor;

            if (ContentDiv != null) {
                iFCKEditor.setData(ContentDiv.innerHTML);
            }
            else {
                iFCKEditor.setData('');
            }

            jq('#commentBox').show();

            jq(window).scrollTop(jq('#commentBox').position().top, { speed: 500 });
            jq('#previewBox').hide("slow");

            setTimeout(function () { CKEDITOR.instances.commentEditor.focus(); }, 500); 
        }
        else {
            var ContentDiv = document.getElementById('content_' + id);
            if (ContentDiv != null) {
                var text = TextHelper.Html2FormattedText(ContentDiv);
                jq('#simpleTextArea').val(text);
            }
            else {
                jq('#simpleTextArea').val('');
            }

            AjaxPro.onLoading = function(b) {
                if (b) {
                    CommentsManagerObj.BlockCommentsBox();
                }
                else {
                    CommentsManagerObj.UnblockCommentsBox();
                };
            }

            jq('#commentBox').show();
            jq('#simpleTextArea').focus();
            jq(window).scrollTop(jq('#commentBox').position().top, { speed: 500 });
            jq('#previewBox').hide("slow");
        }
    };

    this.callBackLoadComment = function(result) {
        if (result != null && result.value != '') {
            jq('#simpleTextArea').val(result.value);
        }
        else {
            jq('#simpleTextArea').val('');
        }

        jq('#simpleTextArea').focus();
        jq('#simpleTextArea').scrollTo();
        jq('#commentBox').show();
        jq('#previewBox').hide();
    };

    this.SetParentComment = function(value) {
        jq('#hdnParentComment').val(value);
    };

    this.SetAction = function(action, comment_id) {
        jq('#hdnAction').val(action);

        if (comment_id != null)
            jq('#hdnCommentID').val(comment_id);
    };

    this.callbackRemove = function(result) {
        if (result.value != null) {

            var html = "<div style='padding:10px;'>" + CommentsManagerObj.inactiveMessage + "</div>";
            jq('#comment_' + result.value).html(html);
            CommentsManagerObj.currentCommentID = result.value;
            CommentsManagerObj.CallActionHandlerJS('remove', 'CommentsManagerObj.UnblockCommentsBox');

        }
        CommentsManagerObj.Redraw();
        CommentsManagerObj.Cancel();

    };

    this.Cancel = function() {
        CommentsManagerObj.CallActionHandlerJS('cancel', 'CommentsManagerObj.UnblockCommentsBox');

        jq('#commentBox').hide();
        CommentsManagerObj.showAddButton();
    };

    this.CallActionHandlerJS = function(action, callBack) {
        switch (action) {
            case "add":
                if (CommentsManagerObj.OnEditedCommentJS != "" && !CommentsManagerObj.isSimple) {
                    eval(CommentsManagerObj.OnEditedCommentJS + "('" + CommentsManagerObj.currentCommentID + "', CKEDITOR.instances.commentEditor.getData() , '" + CommentsManagerObj.FckDomainName + "', false, '" + callBack + "')");
                }
                return;

            case "edit":
                if (CommentsManagerObj.OnEditedCommentJS != "") {
                    var text = '';

                    if (CommentsManagerObj.isSimple)
                        text = jq('#simpleTextArea').val();
                    else {
                        text = CKEDITOR.instances.commentEditor.getData();
                    }

                    eval(CommentsManagerObj.OnEditedCommentJS + "('" + CommentsManagerObj.currentCommentID + "', text, '" + CommentsManagerObj.FckDomainName + "', true, '" + callBack + "')");
                }
                return;

            case "remove":
                if (CommentsManagerObj.OnRemovedCommentJS != "")
                    eval(CommentsManagerObj.OnRemovedCommentJS + "('" + CommentsManagerObj.currentCommentID + "', '" + CommentsManagerObj.FckDomainName + "', '" + callBack + "')");
                return;

            case "cancel":
                if (CommentsManagerObj.OnCanceledCommentJS != "" && !CommentsManagerObj.isSimple)
                    eval(CommentsManagerObj.OnCanceledCommentJS + "('" + CommentsManagerObj.currentCommentID + "', '" + CommentsManagerObj.FckDomainName + "', (CommentsManagerObj.currentCommentID != ''), '" + callBack + "')");
                return;
        }

        eval(callBack + "()");
    };


    this.CallFCKComplete = function() {
        if (jq('#hdnAction').val() == "update") {
            CommentsManagerObj.CallActionHandlerJS('edit', 'CommentsManagerObj.UnblockCommentsBox');
        }
        else {
            CommentsManagerObj.CallActionHandlerJS('add', 'CommentsManagerObj.UnblockCommentsBox');
        }
    };

    this.AddComment_Click = function() {
        if (this.isSimple) {

            if (jq('#simpleTextArea').val() != "") {
                AjaxPro.onLoading = function(b) {
                    if (b) {
                        CommentsManagerObj.BlockCommentsBox();
                    }
                    else {
                        CommentsManagerObj.UnblockCommentsBox();
                    };
                }

                var postText = TextHelper.Text2EncodedHtml(jq('#simpleTextArea').val());

                if (jq('#hdnAction').val() == "add") {

                    var strMethod = this.javaScriptAddCommentFunctionName + "(jq('#hdnParentComment').val(), jq('#hdnObjectID').val(), postText, ";

                    if (this.PID != "") {
                        strMethod += "CommentsManagerObj.PID, ";
                    }

                    if (this.EnableAttachmets) {
                        strMethod += "CommentsManagerObj.GetAttachments(), ";
                    }

                    strMethod += "CommentsManagerObj.callBackAddComment)";

                    eval(strMethod);
                }
                else if (jq('#hdnAction').val() == "update") {
                    var strMethod = this.javaScriptUpdateCommentFunctionName + "(jq('#hdnCommentID').val(), postText, ";

                    if (this.PID != "") {
                        strMethod += "CommentsManagerObj.PID, ";
                    }

                    if (this.EnableAttachmets) {
                        strMethod += "CommentsManagerObj.GetAttachments(), ";
                    }

                    strMethod += "CommentsManagerObj.callBackUpdateComment)";

                    eval(strMethod);
                }
            }
            else {
                alert(jq('#EmptyCommentErrorMessage').val());
                return false;
            }
        }
        else {
            CommentsManagerObj.iFCKEditor = CKEDITOR.instances.commentEditor;
            var text = CommentsManagerObj.iFCKEditor.getData();
            if (text.trim().length == 0) {
                alert(jq('#EmptyCommentErrorMessage').val());
                return false;
            }

            if (text != "") {
                AjaxPro.onLoading = function(b) {
                    if (b) {
                        CommentsManagerObj.BlockCommentsBox();
                    }
                }

                if (jq('#hdnAction').val() == "add") {
                    var strMethod = this.javaScriptAddCommentFunctionName + "(jq('#hdnParentComment').val(), jq('#hdnObjectID').val(), CKEDITOR.instances.commentEditor.getData(), "

                    if (this.PID != "") {
                        strMethod += "CommentsManagerObj.PID, ";
                    }

                    if (this.EnableAttachmets) {
                        strMethod += "CommentsManagerObj.GetAttachments(), ";
                    }

                    strMethod += "CommentsManagerObj.callBackAddComment)";

                    eval(strMethod);
                }
                else if (jq('#hdnAction').val() == "update") {
                    var strMethod = this.javaScriptUpdateCommentFunctionName + "(jq('#hdnCommentID').val(), CKEDITOR.instances.commentEditor.getData(), "

                    if (this.PID != "") {
                        strMethod += "CommentsManagerObj.PID, ";
                    }

                    if (this.EnableAttachmets) {
                        strMethod += "CommentsManagerObj.GetAttachments(), ";
                    }

                    strMethod += "CommentsManagerObj.callBackUpdateComment)";

                    eval(strMethod);
                }
            }
        }
          jq('#noComments').hide();
    };

    this.BlockCommentsBox = function () {
        LoadingBanner.showLoaderBtn("#commentBoxContainer");
        this.mainLoader = jq.blockUI.defaults.message;
        jq.blockUI.defaults.message = '';
        jq.blockUI();
    };

    this.UnblockCommentsBox = function() {
        LoadingBanner.hideLoaderBtn("#commentBoxContainer");
        jq.blockUI.defaults.message = CommentsManagerObj.mainLoader;
        jq.unblockUI();
    };

    this.hideAddButton = function () {
        jq('#add_comment_btn').addClass("display-none");
    };

    this.showAddButton = function () {
        setTimeout(function () { jq('#add_comment_btn').removeClass("display-none"); }, 500);
    };

    this.callBackAddComment = function(result) {
        var res = result.value;
        if (res == null) return;

        if (res.rs10 == "postDeleted") {
            window.location = res.rs11;
            return;
        }

        if (res.rs1 == "") {
            jq('#mainContainer').append(res.rs2);
        }
        else {
            var level = false;

            var obj = jq('#container_' + res.rs1).parent();

            for (var i = CommentsManagerObj.maxLevel; i > 1; i--) {
                if (obj.attr("id") == "mainContainer") {
                    level = true;
                    break;
                }
                obj = obj.parent();
            }
            var html = res.rs2;
            if (level == false)
                html = html.replace("margin-left: 35px;", "");

            jq('#container_' + res.rs1).append(html);
        }

        if (this.javaScriptCallBackAddComment != "")
            eval(this.javaScriptCallBackAddComment);

        jq('#commentBox').hide();

        CommentsManagerObj.Redraw();

        jq('#mainContainer').show();
        jq('#commentsTitle').show();
        CommentsManagerObj.showAddButton();

        var obj;

        if (res.rs1 == "") {
            obj = jq('#mainContainer > div:last-child').children();
        }
        else {
            obj = jq('#container_' + res.rs1 + ' > div:last-child').children();
        }

        CommentsManagerObj.currentCommentID = obj.attr("id").replace("comment_", "");


        jq(window).scrollTop(obj.position().top, { speed: 500 });

        var endbackgroundColor;
        if (obj.hasClass('tintMedium')) {
            endbackgroundColor = '#fff';
            obj.removeClass('tintMedium');
        }
        else
            endbackgroundColor = '#ffffff';


        obj.css({ "background-color": "#ffffcc" });
        obj.animate({ backgroundColor: endbackgroundColor }, 1000);

        CommentsManagerObj.CallFCKComplete();
    };

    this.callBackUpdateComment = function(result) {
        var res = result.value;
        if (res == null) return;

        jq('#content_' + res.rs1).html(res.rs2);

        jq('#commentBox').hide("slow");
        CommentsManagerObj.showAddButton();

        var obj = jq('#comment_' + res.rs1);

        CommentsManagerObj.currentCommentID = obj.attr("id").replace("comment_", "");

        jq(window).scrollTop(obj.position().top, { speed: 500 });
        
        if (obj.hasClass('tintMedium')) {
            endbackgroundColor = '#fff';
            obj.removeClass('tintMedium');
        }
        else
            endbackgroundColor = '#ffffff';


        obj.css({ "background-color": "#ffffcc" });
        obj.animate({ backgroundColor: endbackgroundColor }, 1000);

        CommentsManagerObj.CallFCKComplete();
    };

    this.Preview_Click = function() {
        if (this.isSimple) {
            var html = TextHelper.Text2EncodedHtml(jq('#simpleTextArea').val());
            jq('#previewBoxBody').html(html);
            jq('#previewBox').show();
            jq(window).scrollTop(jq('#previewBox').position().top, { speed: 500 });
        }
        else {
            AjaxPro.onLoading = function(b) {
                if (b) {
                    CommentsManagerObj.BlockCommentsBox();
                }
                else {
                    CommentsManagerObj.UnblockCommentsBox();
                };
            };
            eval(this.javaScriptPreviewCommentFunctionName + "(CKEDITOR.instances.commentEditor.getData(), jq('#hdnCommentID').val(), CommentsManagerObj.callBackPreview)");
        }
    };

    this.callBackPreview = function(result) {
        jq('#previewBoxBody').html(result.value);
        jq('#previewBox').show();
        jq(window).scrollTop(jq('#previewBox').position().top, { speed: 500 });
    };

    this.HidePreview = function() {
        jq(window).scrollTop(jq('#commentBox').position().top, { speed: 500 });
        jq('#previewBox').hide("slow");
    };

    this.UploadCallBack = function(file, response) {
        var result = eval("(" + response + ")");

        if (result.Success) {
            var html = jq("#comment_attachments").html();

            html += "<span id='attach_" + CommentsManagerObj.CurrentAttachID + "'>";

            if (CommentsManagerObj.CurrentAttachID > 0)
                html += ", ";

            html += file + " <a class='linkDescribe' href='javascript:CommentsManagerObj.RemoveAttach(" + CommentsManagerObj.CurrentAttachID + ");'>" + CommentsManagerObj.RemoveAttachButton + "<input name='attach_comments' type='hidden' value='" + result.Data + "' /></a></span>";

            CommentsManagerObj.CurrentAttachID++;
            jq("#comment_attachments").html(html);
        }
    };

    this.FillAttachments = function(commentID) {
        var listNames = jq("input[name^='attacment_name_" + commentID + "']");
        var listPaths = jq("input[name^='attacment_path_" + commentID + "']");
        var html = "";

        for (var i = 0; i < listNames.length; i++) {
            html += "<span id='attach_" + CommentsManagerObj.CurrentAttachID + "'>";

            if (CommentsManagerObj.CurrentAttachID > 0)
                html += ", ";

            html += listNames[i].value + " <a class='linkDescribe' href='javascript:CommentsManagerObj.RemoveAttach(" + CommentsManagerObj.CurrentAttachID + ");'>" + CommentsManagerObj.RemoveAttachButton + "<input name='attach_comments' type='hidden' value='" + listPaths[i].value + "' /></a></span>";
            CommentsManagerObj.CurrentAttachID++;

        }
        jq("#comment_attachments").html(html);

    };

    this.GetAttachments = function() {
        var list = jq("input[name^='attach_comments']");
        var result = "";

        for (var i = 0; i < list.length; i++) {
            if (i != 0)
                result += ";"

            result += list[i].value;
        }

        return result;
    };

    this.RemoveAttach = function(id) {
        jq('#attach_' + id).remove();
    };
}
