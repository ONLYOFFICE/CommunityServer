<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Thrdparty.Web._Default" %>
<%@ Assembly Name="ASC.Thrdparty.Web" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Import contacts</title>
    <meta name="google-site-verification" content="pC57JRzd8yl1ru71Mx-9wBgUrgY_BuTjsK5qBok3K40" />
    <link rel="stylesheet" href="http://www.w3.org/StyleSheets/Core/Swiss" type="text/css" />
    <link rel="stylesheet" href="<%=ResolveUrl("~/content/style.css") %>" type="text/css" />

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.6.2/jquery.min.js" type="text/javascript"></script>

    <script language="javascript" type="text/javascript">
        // implement JSON.parse de-serialization
        var JSON = JSON || {};
        JSON.parse = JSON.parse || function(str) {
            if (str === "") str = '""';
            eval("var p=" + str + ";");
            return p;
        };

        window.master = {
            hWnd: null,
            winName: 'ImportContacts',
            params: 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
            init: function() {
                if (window.addEventListener) {
                    window.addEventListener('message', master.listener, false);
                } else {
                    window.attachEvent('onmessage', master.listener);
                }
            },
            listener: function(e) {
                var data = JSON.parse(e.data);
                master.callback(data.message, data.error);
            },

            callback: function(data, error) {
                var ap = $('#resp');
                ap.empty();
                if (data && data != undefined) {
                    var tableStr = '<table><tr><th>Name</th><th>Family</th><th>Email</th></tr>';
                    var x;
                    for (x in data) {
                        tableStr += '<tr><td>' + data[x].FirstName + '</td><td>' + data[x].LastName + '</td><td>' + data[x].Email + '</td></tr>';
                    }
                    tableStr += '</table>';
                    ap.append(tableStr);
                }
                else {
                    ap.append('<h1 class="error">' + data.error + '</h1>');
                }
            },
            open: function(addr) {
                this.hWnd = window.open(addr, this.winName, this.params);
            }
        };
        master.init();
        
    </script>

</head>
<body>
    <form id="form1" runat="server">
    <h1>
        <iframe id="provider" src="import-contacts/importframe.aspx" frameborder="0"></iframe>
        <a href="javascript:master.open('import-contacts')">Same domain</a>
    </h1>
    <div id="resp">
    </div>
    </form>
</body>
</html>
