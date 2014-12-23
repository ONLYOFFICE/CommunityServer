<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Document Info
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "document", null, new {@class = "up"}) %>
        <span class="hdr">Document Info</span>
    </h1>
    
    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col style="width: 20%"/>
            <col/>
            <col style="width: 110px"/>
            <col style="width: 20%"/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>author</td>
                <td>the name of the document author/creator</td>
                <td>string</td>
                <td>"Joey Jordison"</td>
            </tr>
            <tr class="tablerow">
                <td>folder</td>
                <td>the folder where the document is stored (can be empty in case the document is stored in the root folder)</td>
                <td>string</td>
                <td>"Example Files"</td>
            </tr>
            <tr class="tablerow">
                <td>created</td>
                <td>defines the document creation date</td>
                <td>string</td>
                <td>"12/12/2012 3:46 PM"</td>
            </tr>
            <tr class="tablerow">
                <td>sharingSettings</td>
                <td>defines the settings which will allow to share the document with other users:
			        <ul>
				        <li>
				            <b>user</b> - the name of the user the document will be shared with
					        <br/>
                            <b>type</b>: string
			            </li>
				        <li>
				            <b>permissions</b> - the access rights for the user with the name above. Can be <b>Full Access</b>, <b>Read Only</b> or <b>Deny Access</b>
					        <br/>
                            <b>type</b>: string
			            </li>
			        </ul>
                </td>
                <td>
                    Collection of object
                    <div class="infotext">Collection</div>
                </td>
                <td></td>
            </tr>
        </tbody>
    </table>

    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
       document: {
          info: {
             author: 'Joey Jordison',
             folder: 'Example Files',
             created: '12/12/2012 3:46 PM',
             sharingSettings: [
                   {
                      user: 'John Smith',
                      permissions: 'Full Access'
                   },
                   ...
             ]
          }
       }
    });
    </pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>