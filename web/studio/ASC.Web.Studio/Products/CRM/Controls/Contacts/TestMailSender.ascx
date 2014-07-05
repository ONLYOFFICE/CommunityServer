<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestMailSender.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Contacts.TestMailSender" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="aswscc" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<script type="text/javascript">
	jq(function() {


       setInterval("getstatus()",3000);

		AjaxPro.TestMailSender.Start(
			function(res) {
				if (res.error != null) {
					console.log(res.error.Massage);
					
					return;
				}

				console.log(res.value);
				
			}
		);
			
     });


	function cancel() {
		AjaxPro.TestMailSender.Cancel(
    		function(res) {
				if (res.error != null) {
					console.log(res.error.Massage);
					
					return;
				}
    			
                console.log(res.value);
			}
		);
	}
	
	function testTemplate() {
		
		
		AjaxPro.TestMailSender.TestTemplate(
    		function(res) {
				if (res.error != null) {
					console.log(res.error.Massage);
					
					return;
				}
    			
                console.log(res.value);
			}
		);
		
		
	}


	function getstatus() {
		AjaxPro.TestMailSender.GetStatus(
    		function(res) {
				if (res.error != null) {
					console.log(res.error.Massage);
					
					return;
				}
    			
                console.log(res.value);
			}
		);
	}
	
</script>


<input type="button" value="GetList Status"  onclick="getstatus()" />
<br/>
<br />
<input type="button" value="Cancel"  onclick="cancel()" />
<br/>
<br />
<input type="button" value="Test Template"  onclick="testTemplate()" />




