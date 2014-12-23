<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.FederatedLogin.Web._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>

    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery.templates/beta1/jquery.tmpl.min.js"></script>

    <script>
        $(function() {
            $('.popup').click(function() {
                var link = $(this).attr('href');
                window.open(link, 'login', 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no')
                return false;
            });
        });

        function loginCallback(profile) {
            $('#profileTmpl').tmpl(profile).appendTo('#profile');
        }
    </script>

    <script id="profileTmpl" type="text/x-jquery-tmpl">
        <ul>
            <li>Id=${Id}</li>
            <li>UId=${UniqueId}</li>
            <li>HId=${HashId}</li>
            <li>Email=${EMail}</li>
            <li>Name=${Name}</li>
            <li>DisplayName=${DisplayName}</li>
            <li><img src="${Avatar}" alt="Avatar" /></li>
            <li>Gender=${Gender}</li>
            <li>FirstName=${FirstName}</li>
            <li>LastName=${LastName}</li>
            <li>Locale=${Locale}</li>
            <li>TimeZone=${TimeZone}</li>
            <li>AuthorizationError=${AuthorizationError}</li>
        </ul>        
    </script>

    <script type="text/javascript" language="javascript">
        $(function() {

        $("#shareLink ul li a.twitter").click(function(parameters) {
                  window.open(this.href, '', 'menubar=no,toolbar=no,resizable=yes,scrollbars=yes,height=600,width=600'); 
                 return false;
             });

             $("#shareLink ul li a.facebook").click(function(parameters) {
                 window.open(this.href, '', 'menubar=no,toolbar=no,resizable=yes,scrollbars=yes,height=600,width=600');
                 return false;
             });

             $("#shareLink ul li a.google-plus").click(function(parameters) {
                 window.open(this.href, '', 'menubar=no,toolbar=no,resizable=yes,scrollbars=yes,height=600,width=600');
                 return false;
             });

        });
    </script>

<style type="text/css">
	#shareLink {

		padding-top: 20px;
		
	}
	#shareLink ul {
		list-style: none;
		clear: both;
		padding: 0px;
		margin: 0px;
	}
	#shareLink ul li {
		float: left;
		padding-right: 3px;
	}
	
	#shareLink ul li a {
		display: block;
		height: 34px;
		width: 32px;	
		background: url('images/ess-icons.png') no-repeat scroll 0 0 transparent;
	}
	#shareLink ul li a.twitter 	{
		background-position: -102px 0px !important;	
	}
	
	#shareLink ul li  a.facebook 	
	{
	    background-position: -68px 0px !important;	
	}
	
	#shareLink ul li  a.google-plus
	{
	   background-position: 0px 0px !important;	

	}
</style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <a href="login.ashx?auth=twitter&mode=popup&callback=loginCallback" class="popup">Sign
            in with twitter</a> <a href="login.ashx?auth=facebook&mode=popup" class="popup">Sign
                in with facebook</a> <a href="login.ashx?auth=linkedin&mode=popup" class="popup">Sign
                    in with linkedin</a> <a href="login.ashx?auth=openid&mode=popup&oid=<%=HttpUtility.UrlEncode(@"https://www.google.com/accounts/o8/id")%>"
                        class="popup">Sign in with google</a>
    </div>
    <div id="shareLink">
      <span>Пример публикации ссылки в соц. сети:  </span>  
        <br/>
         <br/>
        <ul>
            <li ><a class="twitter"  href="https://twitter.com/intent/tweet?text=<%= HttpUtility.UrlEncode("Сообщение для публикации")%>"
                >
            </a></li>
            <li ><a class="facebook" href="http://www.facebook.com/sharer.php?s=100&p[title]=<%=HttpUtility.UrlEncode("Электронная таблица OpenDocument.ods")%>&p[url]=<%= HttpUtility.UrlEncode("http://nct.teamlab.com/products/files/doceditor.aspx?doc=Y1VlbURSbHFjKzk0aExmRm1nQ1lXQT09fCIxMDMyMCI1")%>&p[images][0]=<%= HttpUtility.UrlEncode("http://www.theage.com.au/ffximage/2006/05/12/i_word12_narrowweb__300x359,2.jpg") %>&p[summary]=<%= HttpUtility.UrlEncode("Краткое описание для файла") %>"></a></li>
            <li><a  class="google-plus" href="https://plus.google.com/share?url=<%= HttpUtility.UrlEncode("http://nct.teamlab.com/products/files/doceditor.aspx?doc=Y1VlbURSbHFjKzk0aExmRm1nQ1lXQT09fCIxMDMyMCI1")%>"></a></li>
        </ul>
    </div>
    <div id="profile">
    </div>
    </form>
</body>
</html>
