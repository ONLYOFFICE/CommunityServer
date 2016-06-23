<%@ Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl" %>

<div class="treeheader">Get Started</div>
<ul class="treeview root">
    <li>
        <%= Html.MenuActionLink("Basic concepts", "basic", "editors", "selected") %>
    </li>
    <li>
        <%= Html.MenuActionLink("Integration Examples", "demopreview", "editors", "selected") %>
        <ul class="treeview">
            <li>
                <%= Html.MenuActionLink(".Net (C#) Example", "example/csharp", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Node.js Example", "example/nodejs", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Java Example", "example/java", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("PHP Example", "example/php", "editors", "selected") %>
            </li>
            <li>
                <%= Html.MenuActionLink("Ruby Example", "example/ruby", "editors", "selected") %>
            </li>
        </ul>
    </li>
    <li>
        <%= Html.MenuActionLink("Integration Plugins", "plugins", "editors", "selected") %>
        <ul class="treeview">
            <li>
                <%= Html.MenuActionLink("Alfresco integration", "alfresco", "editors", "selected") %>
            </li>
            <%--<li>
                <%= Html.MenuActionLink("Confluence integration", "confluence", "editors", "selected") %>
            </li>--%>
        </ul>
    </li>
    <li>
        <%= Html.MenuActionLink("How It Works", "howitworks", "editors", "selected") %>
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
                <%= Html.MenuActionLink("Hardware Requirements", "hardware", "editors", "selected") %>
            </li>
        </ul>
    </li>
</ul>

<div class="treeheader">Documentation</div>
<ul class="treeview root">
    <li>
        <%= Html.MenuActionLink("Advanced parameters", "advanced", "editors", "selected") %>
    </li>
    <li>
        <%= Html.MenuActionLink("Config", "config/", "editors", "selected") %>
        <ul class="treeview">
            <li>
                <%= Html.MenuActionLink("Document", "config/document", "editors", "selected") %>
                <ul class="treeview">
                    <li>
                        <%= Html.MenuActionLink("Info", "config/document/info", "editors", "selected") %>
                    </li>
                    <li>
                        <%= Html.MenuActionLink("Permissions", "config/document/permissions", "editors", "selected") %>
                    </li>
                </ul>
            </li>
            <li>
                <%= Html.MenuActionLink("Editor", "config/editor", "editors", "selected") %>
                <ul class="treeview">
                    <li>
                        <%= Html.MenuActionLink("Customization", "config/editor/customization", "editors", "selected") %>
                    </li>
                    <li>
                        <%= Html.MenuActionLink("Embedded", "config/editor/embedded", "editors", "selected") %>
                    </li>
                </ul>
            </li>
            <li>
                <%= Html.MenuActionLink("Events", "config/events", "editors", "selected") %>
            </li>
        </ul>
    </li>
    <li>
        <%= Html.MenuActionLink("Methods", "methods", "editors", "selected") %>
    </li>
    <li>
        <%= Html.MenuActionLink("Callback handler", "callback", "editors", "selected") %>
    </li>
    <li>
        <%= Html.MenuActionLink("Conversion API", "conversionapi", "editors", "selected") %>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/viewforum.php?f=9" target="_blank">Forum</a>