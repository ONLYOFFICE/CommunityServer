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


CustomNavigationManager = new function () {

    var renderItem = function (item) {
        var container = jq("#customNavigationItems tr.display-none").clone().removeClass("display-none");
        container.find(".item-img img").attr("src", item.smallImg);
        container.find(".item-label div").text(item.label);
        container.find(".item-action a").attr("id", item.id);

        var exist = jq("#" + item.id);

        if (exist.length) {
            exist.parents("tr:first").replaceWith(container);
        } else {
            container.appendTo("#customNavigationItems");
        }

        jq("#addBtn").parent().addClass("middle-button-container");

        exist = jq("#topNavCustomItem_" + item.id);

        if (!item.showInMenu) {
            exist.remove();
            return;
        }

        var topNavItemLi = jq("<li/>").attr("id", "topNavCustomItem_" + item.id);
        var topNavItemA = jq("<a/>").attr("href", item.url).attr("target", "_blank").addClass("dropdown-item menu-products-item").text(item.label);
        var topNavItemSpan = jq("<span/>").addClass("dropdown-item-icon").attr("style", "background: url('" + item.smallImg + "')");

        topNavItemSpan.prependTo(topNavItemA);
        topNavItemLi.append(topNavItemA);

        if (exist.length) {
            exist.replaceWith(topNavItemLi);
        } else {
            topNavItemLi.appendTo("#studio_productListPopupPanel .custom-nav-items ul");
        }
    };

    var bindUploader = function (uploader, img, size) {
        return new AjaxUpload(uploader, {
            action: "ajaxupload.ashx?type=ASC.Web.Studio.UserControls.CustomNavigation.LogoUploader,ASC.Web.Studio",
            autoSubmit: false,
            data: { size: size },
            onChange: function () {
                var fileUploader = this,
                    file = fileUploader._input.files[0],
                    reader = new FileReader();

                reader.onload = function (event) {
                    var dataUri = event.target.result,
                        image = new Image();

                    if (dataUri.indexOf("data:image") == -1) {
                        toastr.error(jq("#notImageError").text());
                        return;
                    }

                    image.onload = function () {
                        if (image.width == size && image.height == size) {
                            LoadingBanner.showLoaderBtn("#studio_customNavigation");
                            fileUploader.submit();
                        } else {
                            toastr.error(jq("#sizeError").text());
                        }
                    };

                    image.src = reader.result;
                };

                reader.onerror = function (event) {
                    console.error("The file can not be read! error code " + event.target.error.code);
                };

                reader.readAsDataURL(file);
            },
            onComplete: function (file, response) {
                var result = eval("(" + response + ")");
                if (result.Success) {
                    img.attr("src", result.Message);
                } else {
                    toastr.error(result.Message);
                }
                LoadingBanner.hideLoaderBtn("#studio_customNavigation");
            }
        });
    };

    var getItemData = function() {
        var item = {};
        var valid = true;

        item.id = jq("#itemId").val().trim();

        var obj = jq("#labelText");
        var value = obj.val().trim();
        item.label = value;
        value ? obj.removeClass("with-error") : (obj.addClass("with-error") && (valid = false));

        obj = jq("#urlText");
        value = obj.val().trim();
        item.url = value;

        if (value) {
            obj.removeClass("with-error");
            var urlpattern = /^(ftp|http|https):\/\//;
            if (!urlpattern.test(value)) {
                var trimpattern = /^\/+/g;
                item.url = "http://" + value.replace(trimpattern, "");
            }
        } else {
            obj.addClass("with-error");
            valid = false;
        }

        item.showInMenu = jq("#showInMenuCbx").is(":checked");

        item.showOnHomePage = jq("#showOnHomePageCbx").is(":checked");
        
        item.smallImg = jq("#smallImg").attr("src").trim();

        item.bigImg = jq("#bigImg").attr("src").trim();

        return valid ? item : null;
    };

    var setItemData = function (item) {
        jq("#itemId").val(item.id);
        jq("#labelText").val(item.label).removeClass("with-error");
        jq("#urlText").val(item.url).removeClass("with-error");
        jq("#showInMenuCbx").prop("checked", item.showInMenu);
        jq("#showOnHomePageCbx").prop("checked", item.showOnHomePage);
        jq("#smallImg").attr("src", item.smallImg);
        jq("#bigImg").attr("src", item.bigImg);
    };

    this.Init = function () {

        jq("#addBtn").click(function () {
            Teamlab.getCustomNavigationItemSample({},
            {
                before: function () {
                    LoadingBanner.showLoaderBtn("#studio_customNavigation");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#studio_customNavigation");
                },
                success: function (params, response) {
                    setItemData(response);
                    var dialog = jq("#customNavigationItemDialog").removeClass("settings-mode");
                    window.StudioBlockUIManager.blockUI(dialog, 400);
                },
                error: function (params, errors) {
                    LoadingBanner.showMesInfoBtn("#studio_customNavigation", errors[0], "error");
                }
            });
        });

        jq("#customNavigationItems").on("click", ".item-action a", function () {
            Teamlab.getCustomNavigationItem({}, jq(this).attr("id"),
            {
                before: function () {
                    LoadingBanner.showLoaderBtn("#studio_customNavigation");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#studio_customNavigation");
                },
                success: function (params, response) {
                    if (!response.id) return;

                    setItemData(response);
                    var dialog = jq("#customNavigationItemDialog").addClass("settings-mode");
                    window.StudioBlockUIManager.blockUI(dialog, 400);
                },
                error: function (params, errors) {
                    LoadingBanner.showMesInfoBtn("#studio_customNavigation", errors[0], "error");
                }
            });
        });

        jq("#customNavigationItemDialog .cancel-btn").click(function () {
            PopupKeyUpActionProvider.CloseDialog();
        });

        jq("#saveBtn").click(function () {
            var data = getItemData();

            if (!data) return;

            Teamlab.createCustomNavigationItem({}, { item: data },
            {
                before: function () {
                    LoadingBanner.showLoaderBtn("#customNavigationItemDialog");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#customNavigationItemDialog");
                },
                success: function (params, response) {
                    renderItem(response);
                    PopupKeyUpActionProvider.CloseDialog();
                    toastr.success(ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
                },
                error: function (params, errors) {
                    LoadingBanner.showMesInfoBtn("#customNavigationItemDialog", errors[0], "error");
                }
            });
        });

        jq("#removeBtn").click(function () {
            var id = jq("#customNavigationItemDialog #itemId").val();

            Teamlab.deleteCustomNavigationItem({}, id,
            {
                before: function () {
                    LoadingBanner.showLoaderBtn("#customNavigationItemDialog");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#customNavigationItemDialog");
                },
                success: function () {
                    jq("#" + id).parents("tr:first").remove();
                    jq("#topNavCustomItem_" + id).remove();

                    if (!jq("#customNavigationItems tr:visible").length)
                        jq("#addBtn").parent().removeClass("middle-button-container");

                    PopupKeyUpActionProvider.CloseDialog();
                },
                error: function (params, errors) {
                    LoadingBanner.showMesInfoBtn("#customNavigationItemDialog", errors[0], "error");
                }
            });
        });

        bindUploader(jq("#smallUploader"), jq("#smallImg"), 16);

        bindUploader(jq("#bigUploader"), jq("#bigImg"), 100);
    };
};

jq(document).ready(function () {
    CustomNavigationManager.Init();
});