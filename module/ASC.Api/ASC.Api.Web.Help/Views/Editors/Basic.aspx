<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Basic concepts
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<h1>
	    <span class="hdr">Basic concepts</span>
	</h1>
	
    <p class="dscr">ONLYOFFICE™ Document Server API is used to let the developers integrate the ONLYOFFICE™ Document/Spreadsheet/Presentation Editors into their own web sites and setup and manage the editors.</p>
	<p>The API JavaScript file can normally be found in the following editors folder:</p>
	<p><b>/apps/api/documents/api.js</b></p>
	<p>The target HTML file where the editors are to be embedded need to have a placeholder <em>div</em> tag, where all the information about the editor parameters will be passed:</p>
    
<pre>&lt;div id=&quot;placeholder&quot;&gt;&lt;/div>
&lt;script type=&quot;text/javascript&quot; src=&quot;api.js&quot;&gt;&lt;/script&gt;</pre>
	
    <p>The page code containing the changeable parameters looks the following way:</p>
    
    <pre>var docEditor = new DocsAPI.DocEditor('placeholder', config)</pre>

	<p>Where <em>config</em> is an object:</p>

<pre>config = {
       type: 'desktop',
       width: '100%',
       height: '100%',
       documentType: 'text',
       document: {
          title: 'Example Document Title.doc',
          url: 'http://www.examplesite.com/url-to-example-document/',
          fileType: 'doc',
          key: 'key',
          vkey: 'vkey',
          info: {
             author: 'Jessie Jamieson',
             folder: 'Example Files',
             created: '12/12/2012 3:46 PM',
             sharingSettings: [
                   {
                      user: 'John Smith',
                      permissions: 'Full Access'
                   },
                   ...
               ]
           },
          permissions: {
                edit: true,
                download: false
          }
       },
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
       },
       events: {
          'onReady': onDocEditorReady,
          'onDocumentStateChange': onDocumentStateChange,
          'onRequestEditRights': onRequestEditRights,
          'onSave': onDocumentSave,
          'onError': onError,
          'onBack': onBack
       }
    };</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>