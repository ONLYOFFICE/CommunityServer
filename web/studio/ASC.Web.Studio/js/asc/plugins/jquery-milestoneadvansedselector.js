
(function ($, win, doc, body) {
    var milestoneadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.milestoneadvancedSelector.defaults, options);

        this.init();
    };
    milestoneadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: milestoneadvancedSelector,
        initCreationBlock: function (data) {
            var that = this,
                opts = {},
                itemsSimpleSelect = [];

            opts.newoptions = [
            { title: ASC.Resources.Master.Resource.SelectorMilestone, type: "input", tag: "title" },
            { title: ASC.Resources.Master.Resource.SelectorProject, type: "select", tag: "project" },
            { title: ASC.Resources.Master.Resource.SelectorResponsible, type: "select", tag: "manager" },
            { title: ASC.Resources.Master.Resource.SelectorDueDate, type: "date", tag: "duedate" }
            ];
            opts.newbtn = ASC.Resources.Master.Resource.CreateButton;

            that.displayAddItemBlock.call(that, opts);

            var $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                newDate = $addPanel.find(".duedate input");

            newDate.mask(ASC.Resources.Master.DatePatternJQ);

            newDate.datepicker({ selectDefaultDate: true });
            var date = new Date();
            date.setDate(date.getDate() + 7);
            newDate.datepicker('setDate', date);

            for (var i = 0, length = that.groups.length; i < length; i++) {
                itemsSimpleSelect.push(
                    {
                        title: that.groups[i].title,
                        id: that.groups[i].id.toString()
                    }
                );
            }
            $addPanel.find(".advanced-selector-field-wrapper.manager input").attr("disabled", "disabled");
            that.initDataSimpleSelector.call(that, { tag: "project", items: itemsSimpleSelect });
            that.hideLoaderSimpleSelector.call(that, "project");
        },
        initAdvSelectorData: function () {
            var that = this;

            Teamlab.getPrjMilestones({}, {
                before: function () {
                    that.showLoaderListAdvSelector.call(that, 'items');
                },
                after: function () {
                    that.hideLoaderListAdvSelector.call(that, 'items');
                },
                success: function (params, data) {
                    that.rewriteObjectItem.call(that, data);
                },
                error: function (params, errors) {
                    toastr.error(errors);
                }
            });
        },
        initAdvSelectorGroupsData: function(){
            var that = this;
            Teamlab.getPrjProjects({}, {
                before: function () {
                    that.showLoaderListAdvSelector.call(that, 'groups');
                },
                after: function () {
                    that.hideLoaderListAdvSelector.call(that, 'groups');
                },
                success: function (params, data) {
                    that.rewriteObjectGroup.call(that, data);
                },
                error: function (params, errors) {
                    toastr.error(errors);
                }
            });
        },
        createNewItemFn: function () {
            var that = this,
                $addPanel = that.$advancedSelector.find(".advanced-selector-add-new-block"),
                $btn = $addPanel.find(".advanced-selector-btn-add"),
                isError;
            var newMilestone = {
                title: $addPanel.find(".title input").val().trim(),
                projectId: $addPanel.find(".project input").attr("data-id"),
                responsible: $addPanel.find(".manager input").attr("data-id"),
                deadline: $addPanel.find(".duedate input").datepicker('getDate')
            };

            if (!newMilestone.title) {
                that.showErrorField.call(that, { field: $addPanel.find(".title"), error: ASC.Resources.Master.Resource.MilestoneSelectorEmptyTitleError });
                isError = true;
            }
            if (!newMilestone.projectId) {
                that.showErrorField.call(that, { field: $addPanel.find(".project"), error: ASC.Resources.Master.Resource.MilestoneSelectorEmptyProjectError });
                isError = true;
            }
            if (!newMilestone.responsible) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: ASC.Resources.Master.Resource.MilestoneSelectorEmptyResponsibleError });
                isError = true;
            }
            if (!newMilestone.deadline) {
                that.showErrorField.call(that, { field: $addPanel.find(".duedate"), error: ASC.Resources.Master.Resource.MilestoneSelectorEmptyDueDateError });
                isError = true;
            }
            if (!newMilestone.projectId && $addPanel.find(".project input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".project"), error: ASC.Resources.Master.Resource.MilestoneSelectorProjectNotFoundError });
                isError = true;
            }
            if (!newMilestone.responsible && $addPanel.find(".manager input").val()) {
                that.showErrorField.call(that, { field: $addPanel.find(".manager"), error: ASC.Resources.Master.Resource.MilestoneSelectorPersonNotFoundError });
                isError = true;
            }

            if (isError) {
                $addPanel.find(".error input").first().focus();
                return;
            }
            var params = { projectId: newMilestone.projectId };

            Teamlab.addPrjMilestone(params, newMilestone.projectId, newMilestone, {
                before: function() {that.displayLoadingBtn.call(that, { btn: $btn, text: ASC.Resources.Master.Resource.LoadingProcessing });},
                success: function (params, milestone) {

                    var newmilestone = {
                        id: milestone.id.toString(),
                        title: milestone.title,
                        groups: [{ id: milestone.projectId.toString() }]
                    }

                    that.actionsAfterCreateItem.call(that, { newitem: newmilestone, response: milestone, nameProperty: "projectId" });
                },
                error: function () {
                    that.showServerError.call(that, { field: $btn, error: errors });
                },
                after: function () { that.hideLoadingBtn.call(that, $btn); }
            });

        },
        onChooseItemSimpleSelector: function (data) {
            var that = this,
                itemsSimpleSelect = [],
                $field = that.$advancedSelector.find(".advanced-selector-add-new-block .advanced-selector-field-wrapper.manager input");

            Teamlab.getPrjTeam({}, data.id, {
                success: function (params, data) {
                    for (var i = 0, length = data.length; i < length; i++) {
                        if (data[i].status == 1 && !(data[i].isVisitor)) {
                            if (data[i].id == Teamlab.profile.id) {
                                itemsSimpleSelect.unshift(
                                    {
                                        title: ASC.Resources.Master.Resource.MeLabel,
                                        id: data[i].id.toString()
                                    }
                                );
                            } else {
                                itemsSimpleSelect.push(
                                    {
                                        title: data[i].displayName,
                                        id: data[i].id.toString()
                                    }
                                );
                            }
                        }
                    }
                    that.initDataSimpleSelector.call(that, { tag: "manager", items: itemsSimpleSelect });
                    $field.removeAttr("disabled");
                },
                error: function (params, errors) {
                    toastr.error(errors);
                }
            })
        }
    });

    $.fn.milestoneadvancedSelector = function (option, val) {
        return this.each(function () {
            var $this = $(this),
                data = $this.data('milestoneadvancedSelector'),
                options = $.extend({},
                        $.fn.milestoneadvancedSelector.defaults,
                        $this.data(),
                        typeof option == 'object' && option);
            if (!data) $this.data('milestoneadvancedSelector', (data = new milestoneadvancedSelector(this, options)));
            if (typeof option == 'string') data[option](val);
        });
    }
    $.fn.milestoneadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        addtext: ASC.Resources.Master.Resource.MilestoneSelectorAddText,
        noresults: ASC.Resources.Master.Resource.MilestoneSelectorNoResult,
        noitems: ASC.Resources.Master.Resource.MilestoneSelectorNoItems,
        emptylist: ASC.Resources.Master.Resource.MilestoneSelectorEmptyList
    });

})(jQuery, window, document, document.body);