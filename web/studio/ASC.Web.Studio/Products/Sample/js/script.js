/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


if (typeof ASC === "undefined") {
    ASC = {};
}
if (typeof ASC.Sample === "undefined") {
    ASC.Sample = (function () { return {}; })();
}

ASC.Sample.PageScript = (function () {
    var isInit = false,
        userId = null;

    var initAdvansedFilter = function () {

        jq("#peopleFilter").advansedFilter({
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
            renderFilterContent(filters);
        })
        .bind("resetfilter", function (evt, $container, filter, filters) {
            renderFilterContent(filters);
        })
        .bind("resetallfilters", function () {
            jq("#peopleFilterContent").html("");
        });
    };

    function renderFilterContent(filters) {
        jq("#peopleFilterContent").html("");

        var asc = null;
        var text = null;
        var status = null;
        var group = null;

        filters.forEach(function (item) {
            switch (item.id) {
                case "sorter":
                    asc = item.params.sortOrder == "ascending";
                    break;
                case "text":
                    text = item.params.value.toLowerCase();
                    break;
                case "admin":
                    status = item.params.value;
                    break;
                case "user":
                    status = item.params.value;
                    break;
                case "user-group":
                    group = item.params.value;
                    break;
            }
        });

        var activeUsers = ASC.Resources.Master.ApiResponses_ActiveProfiles.response;

        var users = jq.grep(activeUsers, function (user) {

            if (group != null) {
                var groups = jq.map(user.groups, function (item) {
                    return item.id;
                });
                if (!groups.includes(group))
                    return false;
            }

            if (status != null) {
                if (user.isAdmin != status)
                    return false;
            }

            if (text != null) {
                if (user.displayName.toLowerCase().indexOf(text) == -1)
                    return false;
            }

            return true;
        });

        users.sort(function (a, b) {
            var res = ((a.displayName < b.displayName) ? -1 : ((a.displayName > b.displayName) ? 1 : 0));
            return asc ? res : res * (-1);
        });

        users.forEach(function (user) {
            jq("#peopleFilterContent").append(
                jq("<a/>").attr("href", user.profileUrl).append(
                    jq("<img/>").attr("src", user.avatarSmall).attr("title", user.displayName)
                ));
        });
    }

    function initElementsPage() {
        if (!jq("#select").length) return;
        
        jq("#select").tlCombobox();

        jq.dropdownToggle({
            switcherSelector: "#dropdownBtn",
            dropdownID: "dropdownPanel",
            addTop: 10,
            addLeft: 0,
            rightPos: false
        });

        jq("#dropdownPanel").on("click", ".dropdown-item", function () {
            jq("#dropdownBtn span").text(jq(this).text());
            jq("#dropdownPanel").hide();
        });

        jq("#popupBtn").on("click", function () {
            window.StudioBlockUIManager.blockUI("#popupDialog", 500);
        });

        jq("#popupDialog").on("click", ".close-popup", function () {
            window.LoadingBanner.hideLoaderBtn("#popupDialog");
            window.PopupKeyUpActionProvider.CloseDialog();
        });

        jq("#popupDialog").on("click", ".show-loading", function () {
            window.LoadingBanner.showLoaderBtn("#popupDialog");
        });

        jq("#popupDialog").on("click", ".show-success", function () {
            window.LoadingBanner.hideLoaderBtn("#popupDialog");
            window.LoadingBanner.showMesInfoBtn("#popupDialog", "success message", "success");
        });

        jq("#popupDialog").on("click", ".show-error", function () {
            window.LoadingBanner.hideLoaderBtn("#popupDialog");
            window.LoadingBanner.showMesInfoBtn("#popupDialog", "error message", "error");
        });
    }
    
    function initControlsPage() {
        if (!jq("#userSelector").length) return;
        
        var userSelector = jq("#userSelector");

        userSelector.useradvancedSelector({
            itemsChoose: [],
            itemsDisabledIds: [],
            canadd: false,
            showGroups: true,
            onechosen: true,
            withGuests: false
        }).on("showList", function (event, item) {
            jq(this).html(item.title + " (id: " + item.id + ")");
        });

        jq("#emailSelector").AdvancedEmailSelector("init", {
            isInPopup: false,
            items: [
                '"test.ru" <test@test.ru>;',
                '"BROKEN" <net@net.>;',
                '<test@test.org>;',
                'NOT VALID',
                { name: "test.com", email: "test@test.com", isValid: true }
            ],
            maxCount: 20,
            onChangeCallback: function () {
                console.log("changed");
            }
        });

        initAdvansedFilter(); 
    }
    
    function initApiPage() {
        if (!jq("#addBtn").length) return;
        
        jq("#addBtn").on("click", function () {
            jq("#itemValue").val("");
            jq("#itemId").val("");
            window.StudioBlockUIManager.blockUI("#itemDialog", 230);
        });

        jq("#itemsTbl").on("click", ".editBtn", function () {
            var id = jq(this).attr("data-id");
            jq("#itemValue").val(jq("#itemValue_" + id).text().trim());
            jq("#itemId").val(id);
            window.StudioBlockUIManager.blockUI("#itemDialog", 230);
        });

        jq("#itemsTbl").on("click", ".deleteBtn", function () {
            var btn = jq(this);
            jq.ajax({
                url: "/api/2.0/sample/delete/" + btn.attr("data-id") + ".json",
                type: "DELETE"
            }).done(function () {
                btn.parents("tr:first").remove();
            });
        });

        window.LoadingBanner.displayLoading(true);
        jq.get("/api/2.0/sample/read.json", function (data) {
            if (data && data.response && data.response.length) {
                jq.each(data.response, function (index, item) {
                    jq("#itemTmpl").tmpl(item).appendTo("#itemsTbl tbody");
                });
            }
            
        }).always(function () {
            window.LoadingBanner.hideLoading();
        });

        jq("#itemDialog").on("click", ".close-popup", function () {
            window.PopupKeyUpActionProvider.CloseDialog();
        });

        jq("#itemDialog").on("click", ".save-popup", function () {
            var id = jq("#itemId").val();
            var value = jq("#itemValue").val();

            if (id) {
                jq.ajax({
                    type: "PUT",
                    url: "/api/2.0/sample/update.json",
                    data: { id: id, value: value }
                }).done(function () {
                    jq("#itemValue_" + id).text(value);
                    window.PopupKeyUpActionProvider.CloseDialog();
                });
            } else {
                jq.ajax({
                    type: "POST",
                    url: "/api/2.0/sample/create.json",
                    data: { value: value }
                }).done(function (data) {
                    jq("#itemTmpl").tmpl({ id: data.response.id, value: data.response.value }).appendTo("#itemsTbl tbody");
                    window.PopupKeyUpActionProvider.CloseDialog();
                });
            }
        });
    }

    var init = function(id) {
        if (isInit) {
            return;
        }

        isInit = true;
        userId = id;

        initElementsPage();

        initControlsPage();

        initApiPage();
    };

    return {
        init: init
    };
})(jQuery);