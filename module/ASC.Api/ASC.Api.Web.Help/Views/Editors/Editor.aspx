<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Editor Config
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "config", null, new {@class = "up"}) %>
        <span class="hdr">Editor Config</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">The editorConfig section allows to change the following editor settings</p>

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
                <td>mode</td>
                <td>
                    defines the editor opening mode. Can be either <b>view</b> to open the document for viewing, or <b>edit</b> to open the document in the editing mode allowing to apply changes to the document data
                </td>
                <td>string</td>
                <td>"edit"</td>
            </tr>
            <tr class="tablerow">
                <td>lang</td>
                <td>
                    defines the editor interface language (if some other languages other than English are present). Is set using the two letter (<b>de</b>, <b>ru</b>, <b>it</b>, etc.) or four letter (<b>en-US</b>, <b>fr-FR</b>, etc.) language codes
				</td>
                <td>string</td>
                <td>"en-US"</td>
            </tr>
            <tr class="tablerow">
                <td>canCreateNew</td>
                <td>
                    defines the presence or absence of the <b>Create New...</b> menu option which will allow to create new document of the same type.
			    </td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>createUrl</td>
                <td>
                    defines the URL of the document where it will be created and available after creation. The editor will add <b>'?title={document title}&amp;action=create'</b>
				</td>
                <td>string</td>
                <td>"http://www.examplesite.com/..."</td>
            </tr>
            <tr class="tablerow">
                <td>user</td>
                <td>
                    the user currently viewing or editing the document:
                    <ul>
					    <li>
					        <b>id</b> - the identification of the user
					        <br/>
                            <b>type</b>: string
					    </li>
					    <li>
					        <b>name</b> - the full name of the user
						    <br/>
                            <b>type</b>: string
		    		    </li>
				    </ul>
                </td>
				<td>object</td>
                <td></td>
            </tr>
            <tr class="tablerow">
                <td>recent</td>
                <td>
                    defines the presence or absence of the documents in the <b>Open Recent...</b> menu option where the following document parameters can be set:
                    <ul>
					    <li>
					        <b>title</b> - the document title that will be displayed in the <b>Open Recent...</b> menu option
					        <br />
						    <b>type</b>: string
					    </li>
					    <li>
					        <b>url</b> - the URL to the document where it is stored
						    <br />
						    <b>type</b>: string
					    </li>
					    <li>
					        <b>folder</b> - the folder where the document is stored (can be empty in case the document is stored in the root folder)
						    <br />
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
            <tr class="tablerow">
                <td>embedded</td>
                <td>
                    defines the properties of the embedded document:
                    <ul>
					    <li>
					        <b>embedUrl</b> - the URL to the document serving as a source file for the document embedded into the web page
						    <br/>
                            <b>type</b>: string
		    		    </li>
					    <li>
					        <b>fullscreenUrl</b> - the URL to the document which will open in full screen mode
						    <br/>
                            <b>type</b>: string
					    </li>
					    <li>
					        <b>saveUrl</b> - the URL that will allow the document to be saved onto the user personal computer
						    <br/>
                            <b>type</b>: string
					    </li>
					    <li>
					        <b>shareUrl</b> - the URL that will allow other users to share this document
						    <br/>
                            <b>type</b>: string
					    </li>
					    <li>
					        <b>toolbarDocked</b> - the place for the embedded viewer toolbar, can be either <b>top</b> or <b>bottom</b>
						    <br/>
                            <b>type</b>: string
					    </li>
				    </ul>
                </td>
				<td>object</td>
                <td></td>
            </tr>
        </tbody>
    </table>
    
    <div class="note">The embedded section is for the <b>embedded</b> document type only (see the <a href="<%=Url.Action("config") %>" class="underline">config</a> section to find out how to define the <b>embedded</b> document type.</div>
    
    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
       editorConfig: {
          mode: 'edit',
          lang: 'en-US',
          canCreateNew: true,
          createUrl: 'http://www.examplesite.com/url-to-example-document/',
          user: {
                id: '78e1e841-8314-48465-8fc0-e7d6451b6475',
                name: 'John Smith'
             },
          recent: [
             {
                title: 'exampledocument1.doc',
                url: 'http://www.examplesite.com/files/exampledocument1.doc',
                folder: 'Example Files'
             },
             ...
          ],
          embedded: {
                embedUrl: 'http://www.examplesite.com/files/?action=embedded&doc=exampledocument1.doc',
                fullscreenUrl: 'http://www.examplesite.com/files/?action=embedded&doc=exampledocument1.doc#fullscreen',
                saveUrl: 'http://www.examplesite.com/files/?action=download&doc=exampledocument1.doc',
                shareUrl: 'http://www.examplesite.com/files/?action=view&doc=exampledocument1.doc',
                toolbarDocked: 'top'
                }

       }
    });
    </pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>