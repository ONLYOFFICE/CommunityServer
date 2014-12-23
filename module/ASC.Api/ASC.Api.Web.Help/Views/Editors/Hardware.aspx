<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Hardware Requirements
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
	    <span class="hdr">Hardware Requirements</span>
	</h1>
	<p class="dscr">See the table below for the minimal hardware configuration requirements to the server necessary for the Teamlab Office Apps installation.</p>
	<table class="table">
		<colgroup>
		    <col style="width: 200px;"/>
            <col/>
		</colgroup>
        <thead>
            <tr class="tablerow">
			    <td>Number of concurrent active users</td>
			    <td>Minimal hardware server configuration</td>
		    </tr>
        </thead>
        <tbody>
		    <tr class="tablerow">
			    <td>less than 100</td>
			    <td>Single core Intel Sandy Bridge or better processor running at 2.8 GHz, 2 GB RAM, 40 GB of free hard disk drive space</td>
		    </tr>
		    <tr class="tablerow">
			    <td>100 - 200</td>
			    <td>Dual core Intel Sandy Bridge or better processor running at 2.8 GHz, 2 GB RAM, 80 GB of free hard disk drive space</td>
		    </tr>
		    <tr class="tablerow">
			    <td>200 - 400</td>
			    <td>Quad core Intel Sandy Bridge or better processor running at 2.8 GHz, 4 GB RAM, 160 GB of free hard disk drive space</td>
		    </tr>
        </tbody>
	</table>
	<p>Hardware configuration is given for reference use only. The real number of the server users depend on the number, type and size of the documents that the users work with.</p>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>