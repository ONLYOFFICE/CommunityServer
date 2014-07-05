<%@ Page Language="C#" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
    <title>Quote Properties</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta content="noindex, nofollow" name="robots">

    <script src="../../dialog/common/fck_dialog_common.js" type="text/javascript"></script>

    <script type="text/javascript">



        var dialog = window.parent;
        var oEditor = dialog.InnerDialogLoaded();

        var FCK = oEditor.FCK;
        var FCKLang = oEditor.FCKLang;
        var FCKConfig = oEditor.FCKConfig;
        var FCKRegexLib = oEditor.FCKRegexLib;
        var FCKTools = oEditor.FCKTools;
        var FCKSelection = oEditor.FCKSelection;
        var FCKBrowserInfo = oEditor.FCKBrowserInfo;


        var oUser = dialog.Selection.GetSelection().MoveToAncestorNode('DIV');
        if (oUser)
            FCK.Selection.SelectNode(oUser);

        function OnDialogTabChange(tabCode) {
            ShowE('divMain', true);

            dialog.SetAutoSize(true);
        }



        window.onload = function() {
            // Translate the dialog box texts.
            oEditor.FCKLanguageManager.TranslatePage(document);

            // Load the selected element information (if any).
            LoadSelection();

            GetE('divMain').style.display = '';

            dialog.SetOkButton(true);
            dialog.SetAutoSize(true);

            SelectField('txtName');
        }


        function LoadSelection() {

            if (oUser && oUser.getAttribute('__ascuser')) {
                GetE('itemSelected').value = oUser.getAttribute('__ascuser');
                var selectedDiv = ItemSelected();
                var scroll = selectedDiv.offsetTop - GetE('listUserSelect').offsetHeight;
                if (scroll < 0)
                    scroll = 0;
                GetE('listUserSelect').scrollTop = scroll;

                }
            }


        


        //#### The OK button was hit.
        function Ok() {
            if (GetE('itemSelected').value == '') {
                GetE('txtName').focus();
                alert(FCKLang.AscUserOwnerAlert);
                return false;
            }

            oEditor.FCKUndo.SaveUndoStep();


            
            var html = "<div class='fckAscUser' __ascuser='";
            html += GetE('itemSelected').value + "'>"
            html += GetE(GetE('itemSelected').value).innerHTML;
            html += "</div><span>&nbsp;</span>";
            if (FCKBrowserInfo.IsSafari) {
                html = "<span>&nbsp;</span>" + html;
            }
            
            //oUser = FCK.EditorDocument.createElement('DIV');
            //oUser.setAttribute('__ascuser', GetE('itemSelected').value);
            //oUser.setAttribute('class', 'fckAscUser');
            //oUser.innerHTML = GetE(GetE('itemSelected').value).innerHTML;

            FCK.InsertHtml(html);
            FCK.OnAfterSetHTML();

            return true;
        }


        function ItemSelected(item) {
            if (item)
                document.getElementById('itemSelected').value = item.id;
            var result = null;
            var itemSelectedId = document.getElementById('itemSelected').value;
            var employeeList = document.getElementById('listUserSelect');
            for (var i in employeeList.childNodes) {
                var it = employeeList.childNodes[i];
                if (it.nodeName != 'DIV')
                    continue;
                if (it.id == itemSelectedId) {
                    it.style.backgroundColor = '#00AEFC';
                    result = it;
                }
                else {
                    it.style.backgroundColor = '';
                }

            }

            return result;

        }

        function Trim(str) { return str.replace(/^\s+|\s+$/g, ''); }

        function Employee_SearchKeyPress(obj) {
            var searchText = Trim(obj.value).toUpperCase();
            var element, firstName, lastName;
            var elementList, offsetWidth;
            var employeeList = document.getElementById('listUserSelect');

            for (var i = 0; i < employeeList.childNodes.length; i++) {
                var child = employeeList.childNodes[i];
                if (child.nodeName != 'DIV')
                    continue;

                var childText = Trim(child.innerHTML);

                if (childText.indexOf(' ') > 0) {
                    firstName = Trim(childText.substring(0, childText.indexOf(' ')).toUpperCase());
                    lastName = Trim(childText.substring(childText.indexOf(' ')).toUpperCase());
                }
                else {
                    firstName = Trim(childText.toUpperCase());
                    lastName = '';
                }
                if (searchText == '' || firstName.indexOf(searchText) == 0 || lastName.indexOf(searchText) == 0) {
                    child.style.display = '';
                } else {
                    if (child.id == document.getElementById('itemSelected').value) {
                        document.getElementById('itemSelected').value = '';
                    }
                    child.style.display = 'none';
                }
            }
            ItemSelected();
            //            if (FCKBrowserInfo.IsIE) {
            //                employeeList.style.width = offsetWidth + 'px';
            //            }

        };

        //        function HideOption(opt, hide) {
        //            if (FCKBrowserInfo.IsIE || FCKBrowserInfo.IsSafari) {
        //                if (hide) {
        //                    if (opt.nodeName != '#comment') {
        //                        alert(opt.nodeName);
        //                        var comment = document.createComment();
        //                        comment.nodeValue = opt.outerHTML;
        //                        opt.parentNode.replaceChild(comment, opt);
        //                    }
        //                    
        //                    
        //                }
        //                else {
        //                    if (opt.nodeName == '#comment') {
        //                        var newNode = document.createElement('OPTION');
        //                        var regText = new RegExp('</?(.|\\n)*?>', 'g');
        //                        var regValue = new RegExp('<OPTION.*value=([\"\']?((?:.(?![\"\']?s+(?:S+)=|[>\"\']))+.)[\"\']?).*>', 'gi');
        //                        newNode.value = opt.nodeValue.replace(regValue, '$1');
        //                        newNode.innerText = opt.nodeValue.replace(regText, '');
        //                        opt.parentNode.replaceChild(newNode, opt);
        //                    }
        //                }



        //            }
        //            else {
        //                opt.style.display = (hide ? 'none' : '');
        //            }

        //        }
        
        
		
    </script>

</head>
<body scroll="no" style="overflow: hidden">
    <div id="divMain">
        <div class="Caption">
            <span fcklang="AscUserSearchLnkName">Search Name</span><br />
            <input id="txtName" style="width: 100%" onkeyup="javascript:Employee_SearchKeyPress(this);"
                type="text" />
        </div>
        <div class="Caption">
            <span fcklang="AscUserList">User List</span><input type="hidden" id="itemSelected" /><br />
            <div id="listUserSelect" size="12" style="cursor: default; overflow-x: hidden; overflow-y: auto;
                height: 170px; background-color: #fff; border: 1px solid #333; padding: 3px; font-weight:normal;">
                <% 
                    List<UserInfo> list = new List<UserInfo>();
                    list.AddRange(UserInfoExtension.SortByUserName(CoreContext.UserManager.GetUsers(EmployeeStatus.All)));
                    foreach (UserInfo user in list)
                    { %>
                <div id="<%=user.ID%>" onclick="javascript:ItemSelected(this)" name="option">
                    <%=UserInfoExtension.DisplayUserName(user) %></div>
                <%} %>
            </div>
        </div>
    </div>
</body>
</html>
