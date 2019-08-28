<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserControls.aspx.cs" MasterPageFile="Masters/BasicTemplate.Master" Inherits="ASC.Web.Sample.UserControls" %>

<%@ MasterType TypeName="ASC.Web.Sample.Masters.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div>
        <h1>User selector</h1>
        
        <p>
            <span class="link dotline" id="userSelector">Choose User</span>
        </p>

<pre><code>jq("#userSelector").useradvancedSelector({
    itemsChoose: [],
    itemsDisabledIds: [],
    canadd: false,
    showGroups: true,
    onechosen: true,
    withGuests: false
}).on("showList", function (event, item) {
    jq(this).html(item.title + " (id: " + item.id + ")");
});

jq("#userSelector").useradvancedSelector("unselect", [Teamlab.profile.id]);
jq("#userSelector").useradvancedSelector("reset");
jq("#userSelector").useradvancedSelector("select", [Teamlab.profile.id]);
jq("#userSelector").useradvancedSelector("disable", [Teamlab.profile.id]);</code></pre>

        <h1>Email selector</h1>

        <div id="emailSelector" class="emailselector">
            <input type="text" class="emailselector-input" autocomplete="off">
            <pre class="emailSelector-input-buffer"></pre>
        </div>

<pre><code>var itemsString = '"test" &lt;test@test.ru&gt;, NOT VALID, test@test.com, &lt;test@test.org&gt;';
var itemsArray = [
    '"test.ru" &lt;test@test.ru&gt;',
    '"BROKEN" &lt;net@net.&gt;',
    '&lt;test@test.org&gt;',
    'NOT VALID',
    { name: "test.com", email: "test@test.com", isValid: true }
];
jq("#emailSelector").AdvancedEmailSelector("init", {
    isInPopup: false,
    items: itemsArray || itemsString,
    maxCount: 20,
    onChangeCallback: function () {
        console.log("changed");
    }
});
var selectedJson = jq("#emailSelector").AdvancedEmailSelector("get");
var selectedString = jq("#emailSelector").AdvancedEmailSelector("getString");
jq("#emailSelector").AdvancedEmailSelector("clear");</code></pre>

        <h1>Filter</h1>

        <div id="peopleFilter"></div>

        <div id="peopleFilterContent"></div>

<pre><code>jq("#peopleFilter").advansedFilter({
    maxfilters: -1,
    anykey: false,
    store: false,
    hintDefaultDisable: true,
    sorters:
    [
        {
            id: "by-name",
            title: "Name",
            dsc: false,
            def: true
        }
    ],
    filters: 
    [
        {
            type: "combobox",
            id: "admin",
            title: "Admin",
            filtertitle: "Type label",
            group: "User Type Title",
            groupby: "user-type",
            options: [
                { value: 1, classname: "admin", title: "Admin", def: true },
                { value: 0, classname: "user", title: "User or Guest" }
            ],
            hashmask: "type/{0}"
        },
        {
            type: "combobox",
            id: "user",
            title: "User or Guest",
            filtertitle: "Type label",
            group: "User Type Title",
            groupby: "user-type",
            options: [
                { value: 1, classname: "admin", title: "Admin" },
                { value: 0, classname: "user", title: "User or Guest", def: true }
            ],
            hashmask: "type/{0}"
        },
        {
            type: "group",
            id: "user-group",
            title: "Department label",
            group: "Department Title",
            hashmask: "group/{0}"
        }
    ]
})
.bind("setfilter", function (evt, $container, filter, filterparams, filters) {
    renderContent(filters);
})
.bind("resetfilter", function (evt, $container, filter, filters) {
    renderContent(filters);
})
.bind("resetallfilters", function () {
    jq("#peopleFilterContent").html("");
});</code></pre>

    </div>
</asp:Content>
