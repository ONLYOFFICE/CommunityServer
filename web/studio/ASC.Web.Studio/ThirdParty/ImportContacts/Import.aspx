<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Import.aspx.cs" Inherits="ASC.Web.Studio.ThirdParty.ImportContacts.Import" %>

<%@ Import Namespace="Resources" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Import</title>
    <script type="text/javascript">
        window.client = {
            init: function () {
            },
            send: function (msg) {
                window.opener.master.callback(msg);
            },
            sendAndClose: function (msg, error) {
                try {
                    client.send(JSON.stringify({ "msg": msg, "error": error }), error);
                }
                catch (ex) {
                }
                window.close();
            }
        };

        client.init();
    </script>

    <style type="text/css">
        html, body, div {
            margin: 0;
            padding: 0;
        }
        .buttons {
            overflow: hidden;
        }

        .buttons div {
            border: 1px solid #d7d7d7;
            -moz-border-radius: 3px;
            -webkit-border-radius: 3px;
            border-radius: 3px;
            display: block;
            float: left;
            margin-left: 20px;
            width: 168px;
            height: 58px;
            cursor: pointer;
        }

        .buttons div.google {
            background: #ffffff url("image/google.png") no-repeat center center;
        }

        .buttons div.yahoo {
            background: #ffffff url("image/yahoo.png") no-repeat center center;
        }

    </style>
</head>
<body>
    <form enctype="multipart/form-data" id="form1" method="post" runat="server">
        <div class="clearFix buttons">
            <% if (ASC.Web.Studio.ThirdParty.ImportContacts.Google.Enable)
               { %>
            <div class="google" onclick="window.master.open('<%= ASC.Web.Studio.ThirdParty.ImportContacts.Google.Location %>');"
                title="<%= Resource.ImportFromGoogle %>">
            </div>
            <% } %>

            <% if (ASC.Web.Studio.ThirdParty.ImportContacts.Yahoo.Enable)
               { %>
            <div class="yahoo" onclick="window.master.open('<%= ASC.Web.Studio.ThirdParty.ImportContacts.Yahoo.Location %>');"
                title="<%= Resource.ImportFromYahoo %>">
            </div>
            <% } %>
        </div>

        <script type="text/javascript">
            window.master = {
                winName: 'ImportContacts',
                params: 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
                init: function () {
                    if (window.addEventListener) {
                        window.addEventListener('message', master.listener, false);
                    } else {
                        window.attachEvent('onmessage', master.listener);
                    }
                },
                callback: function (msg) {
                    var data = JSON.parse(msg);
                    var obj = { "Tpr": "Importer", "Data": data.msg, "error": data.error };
                    window.parent.postMessage(JSON.stringify(obj), '<%=Request.UrlReferrer%>');
                },

                open: function (url) {
                    window.open(url, this.winName, this.params);
                }
            };
            master.init();
        </script>
    </form>
</body>
</html>
