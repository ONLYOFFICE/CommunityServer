/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
        value ? obj.removeClass("with-error") : (obj.addClass("with-error") && (valid = false));

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
                    window.StudioBlockUIManager.blockUI(dialog, 400, 500, 0);
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
                    window.StudioBlockUIManager.blockUI(dialog, 400, 500, 0);
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