<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<div class="treeheader">Get Started</div>
<ul class="treeview root">
    <li>
        <%=Html.MenuActionLink("Basic concepts", "basic", "editors", "selected")%>
    </li>
    <li>
        <%= Html.ActionLink("Demo preview", "demopreview", "editors") %>
        <ul class="treeview">
            <li>
                <%= Html.ActionLink("Document editor", "demopreview", "editors", null, null, "text", null, new { id = "textDemo" }) %>
            </li>
            <li>
                <%= Html.ActionLink("Spreadsheet editor", "demopreview", "editors", null, null, "spreadsheet", null, new { id = "spreadsheetDemo" }) %>
            </li>
            <li>
                <%= Html.ActionLink("Presentation editor", "demopreview", "editors", null, null, "presentation", null, new { id = "presentationDemo" }) %>
            </li>
        </ul>
    </li>
    <li>
        <%=Html.MenuActionLink("How It Works", "howitworks", "editors", "selected")%>
        <ul class="treeview">
            <li>
                <%= Html.MenuActionLink("Opening File", "open", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Saving File", "save", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Converting and Downloading File", "conversion", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Conversion API", "conversionapi", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Hardware Requirements", "hardware", "editors", "selected") %>
            </li>
        </ul>
    </li>
</ul>

<div class="treeheader">Documentation</div>
<ul class="treeview root">
    <li>
        <%= Html.MenuActionLink("Config", "config", "editors", "selected") %>
        <ul class="treeview">
            <li>
                <%= Html.MenuActionLink("Document", "document", "editors", "selected") %>
                <ul class="treeview">
                    <li>
                        <%= Html.MenuActionLink("Info", "docinfo", "editors", "selected") %>
                    </li>
                    <li>
                        <%= Html.MenuActionLink("Permissions", "docpermissions", "editors", "selected") %>
                    </li>
                </ul>
            </li>
            <li>
                <%= Html.MenuActionLink("Editor", "editor", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Events", "events", "editors", "selected") %>
            </li>
        </ul>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/" target="_blank">Forum</a>