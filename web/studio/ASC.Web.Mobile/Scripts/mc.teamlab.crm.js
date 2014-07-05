/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

;window.TeamlabMobile = (function(TeamlabMobile) {
    if (!TeamlabMobile) {
        console.log('Teamlab.default: has no TeamlabMobile');
        return undefined;
    }

    var 
    
    loadedCrmItems = { all: 0, persons: 0, companies: 0 },
    loadedCrmTasks = { all: 0 },
    loadedCrmHistoryEvents = { all: 0 },
    loadedContactPersones = { all: 0 },
    loadedContactFiles = { all: 0 },
    loadedCrmContactTasks = { all: 0 },

    templateIds = {
        lbcrmtaskstimeline: 'template-crm-tasks-timeline',
        lbcrmtimeline: 'template-crm-timeline',
        lbcrmpersonstimeline: 'template-crm-persones-timeline',
        pgcrm: 'template-page-crm',
        pgcrmtasks: 'template-page-crm-tasks',
        pgcrmtask: 'template-page-crm-task',
        pgcrmaddtask: 'template-page-crm-addtask',
        pgcrmcontacthistory: 'template-page-crm-contacthistory',
        pgcrmcontacttasks: 'template-page-crm-contacttasks',
        pgcrmcontactpersones: 'template-page-crm-contactpersones',
        pgcrmcontactfiles: 'template-page-crm-contactfiles',
        pgcrmaddcompany: 'template-page-crm-addcompany',
        pgcrmaddpersone: 'template-page-crm-addpersone',
        pgcrmaddhistoryevent: 'template-crm-addhistoryevent',
        pgcrmaddnote: 'template-crm-addnote',
        pgcrmperson: 'template-page-crm-person',
        pgcrmcompany: 'template-page-crm-company',
        dgdocsadditem: 'template-dialog-documents-additem',
        dgcrmadditem: 'template-dialog-crm-additem',
        dgcrmnavigate: 'template-dialog-crm-navigate',
        dgcrmaddtocontact: 'template-dialog-crm-addtocontact',
        dgcrmaddfiletocontact: 'template-dialog-crm-additem-file'
    },
    staticAnchors = {
        
        crm: 'crm',
        crm_search: 'crm/search/',
        crm_contact: 'crm/contact/',
        contact: 'crm/contact/',
        tasks_search: 'crm/tasks/search/'
    },
    anchorRegExp = {
        crm: /^crm[\/]*$/,
        crmsearch: /^crm\/search\/(.+)$/,
        taskssearch: /^crm\/tasks\/search\/(.+)$/,
        taskstoday: /^crm\/tasks\/today[\/]*$/,
        tasksnextdays: /^crm\/tasks\/nextdays[\/]*$/,
        taskslate: /^crm\/tasks\/late[\/]*$/,
        tasksclosed: /^crm\/tasks\/closed[\/]*$/,
        crmaddtask: /^crm(\/contact\/([\d-]+))*\/add\/task[\/]*$/,
        crmtask: /^crm\/task\/([\d-]+)$/,
        company: /^crm\/company\/([\w\d-]+)$/,
        contact: /^crm\/contact\/([\w\d-]+)$/,
        contacthistory: /^crm\/contact\/([\w\d-]+)\/history[\/]*$/,
        contactfiles: /^crm\/contact\/([\w\d-]+)\/files[\/]*$/,
        contacttasks: /^crm\/contact\/([\d-]+)\/tasks[\/]*$/,
        contacttask: /^crm\/contact\/([\d-]+)\/task\/([\d-]+)$/,
        contactpersones: /^crm\/contact\/([\d-]+)\/persones[\/]*$/,
        addcompany: /^crm\/company\/add[\/]*$/,
        editcompany: /^crm\/company\/([\d-]+)\/edit[\/]*$/,
        addpersone: /^crm(\/contact\/([\d-]+))*\/persone\/add[\/]*$/,
        editperson: /^crm\/person\/([\d-]+)\/edit[\/]*$/,
        addhistoryevent: /^crm\/contact\/([\w\d-]+)\/add\/historyevent[\/]*$/,
        addnote: /^crm\/contact\/([\w\d-]+)\/add\/file[\/]*$/
    },
    customEvents = {
        changePage: 'onchangepage',
        crmPage: 'oncrmpage',
        crmTasksPage: 'oncrmtaskspage',
        addMoreCrmTasksToList: 'onaddmorecrmtaskstolist',
        addMoreCrmContactTasksToList: 'onaddmorecrmcontacttaskstolist',
        addMoreCrmItemsToList: 'onaddmorecrmitemstolist',
        crmTaskPage: 'oncrmtaskpage',
        crmAddTaskPage: 'oncrmaddtaskpage',
        crmAddCompanyPage: 'oncrmaddcompanypage',
        crmAddPersonePage: 'oncrmaddpersonepage',
        crmAddHistoryEventPage: 'oncrmaddhistoryeventpage',
        crmAddNotePage: 'oncrmaddnotepage',
        crmContactHistoryPage: 'oncrmcontacthistorypage',
        crmContactTasksPage: 'oncrmcontacttaskspage',
        crmContactPersonesPage: 'oncrmcontactpersonespage',
        crmContactFilesPage: 'oncrmcontactfilespage',
        getMoreCrmItems: 'ongetmorecrmitems',
        getMoreCrmTasks: 'ongetmorecrmtasks',
        getMoreCrmContactTasks: 'ongetmorecrmcontacttasks',
        getMoreCrmHistoryEvents: 'ongetmorecrmhistoryevents',
        getMoreCrmContactPersones: 'ongetmorecrmcontactpersones',
        getMoreCrmContactFiles: 'ongetmorecrmcontactfiles',
        personPage: 'onpersonpage',
        companyPage: 'oncompanypage',
        updateCrmTaskCheckbox: 'onupdatecrmtaskcheckbox',
        addCrmItemDialog: 'onaddcrmitemdialog',
        navigateCrmDialog: 'onnavigatecrmdialog',
        addToContactCrmDialog: 'onaddtocontactcrmdialog',
        addFileToContactCrmDialog: 'onaddfiletocontactcrmdialog'
    },
    eventManager = TeamlabMobile.extendEventManager(customEvents),
    dialogMarkCollection = [
      { regexp: /^crm\/customers\/add[\/]*$/, evt: customEvents.addCrmItemDialog },
      { regexp: /^crm\/navigate/, evt: customEvents.navigateCrmDialog },
      { regexp: /^crm\/contact[\/]*([\d-]*)\/add$/, evt: customEvents.addToContactCrmDialog },
    //{regexp : /^crm\/contact\/([\d-]+)\/add\/file/, evt : customEvents.addFileToContactCrmDialog}
    ];

    TeamlabMobile.extendModule(templateIds, anchorRegExp, staticAnchors, dialogMarkCollection);

    
    ASC.Controls.AnchorController.bind(anchorRegExp.crm, onCrmAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.crmsearch, onCrmSearchAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contact, onContactAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.taskssearch, onCrmSearchTasksAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.taskstoday, onCrmTasksTodayAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.tasksnextdays, onCrmTasksNextAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.taskslate, onCrmTasksLateAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.tasksclosed, onCrmTasksClosedAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.crmtask, onCrmTaskAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contacthistory, onCrmContactHistoryAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contacttasks, onCrmContactTasksAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contacttask, onCrmContactTaskAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contactpersones, onCrmContactPersonesAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.contactfiles, onCrmContactFilesAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.crmaddtask, onAddCrmTaskAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.addcompany, onAddCrmCompanyAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.editcompany, onEditCrmCompanyAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.addpersone, onAddCrmPersoneAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.editperson, onEditCrmPersoneAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.addhistoryevent, onAddCrmHistoryEventAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.addnote, onAddCrmNoteAnch);

    
    function onCrmAnch(params) {
        loadedCrmItems.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        Teamlab.getCrmContacts(null, {
            success: onGetCrmItems_,
            filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortby: 'displayname' }
        });
    }

    function onCrmSearchAnch(params, filtervalue) {
        filtervalue = decodeURI(Base64.decode(filtervalue));

        eventManager.call(customEvents.changePage, window, []);
        Teamlab.getCrmContacts({ filtervalue: filtervalue }, {
            success: onGetCrmItems_,
            filter: { filtervalue: filtervalue, sortby: 'displayname' }
        });
    }

    function onCrmSearchTasksAnch(params, filtervalue) {
        filtervalue = decodeURI(Base64.decode(filtervalue));

        eventManager.call(customEvents.changePage, window, []);
        Teamlab.getCrmTasks({ filtervalue: filtervalue }, {
            success: onGetTasks_,
            filter: { filtervalue: filtervalue, sortby: 'displayname' }
        });
    }

    function onCrmTasksTodayAnch(params) {
        loadedCrmTasks.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        var curDate = new Date();
        var today = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 0, 0, 0, 1);
        //var nextDay = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 23, 59, 59, 999);          
        Teamlab.getCrmTasks({ page: 'today' }, {
            success: onGetTasks_,
            filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortby: 'displayname', fromDate: today, toDate: today, isClosed: false }
        });
    }

    function onCrmTasksNextAnch(params) {
        loadedCrmTasks.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        var curDate = new Date();
        var nextDay = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 23, 59, 59, 999);
        Teamlab.getCrmTasks({ page: 'nextdays' }, {
            success: onGetTasks_,
            filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortBy: 'displayname', fromDate: nextDay, isClosed: false }
        });
    }

    function onCrmTasksLateAnch(params) {
        loadedCrmTasks.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        var curDate = new Date();
        var today = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 0, 0, 0, 1);
        Teamlab.getCrmTasks({ page: 'late' }, {
            success: onGetTasks_,
            filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortBy: 'displayname', toDate: today, isClosed: false }
        });
        //Teamlab.joint()
        //.getCrmTasks({page : 'late'}, {filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems+1, sortBy: 'displayname',  toDate: today, isClosed: false }})
        //.start(params, onGetTasks_)
    }

    function onCrmTasksClosedAnch(params) {
        loadedCrmTasks.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        Teamlab.getCrmTasks({ page: 'closed' }, {
            success: onGetTasks_,
            filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortBy: 'displayname', isClosed: true }
        });
    }

    function onCrmContactTasksAnch(params, id) {
        loadedCrmContactTasks.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.id = id;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmTasks(params, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, entitytype: 'contact', entityId: id} })
                .start(params, onGetContactTasks_)
        }
    }

    function onCrmContactHistoryAnch(params, id) {
        loadedCrmHistoryEvents.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.id = id;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmHistoryEvents(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, sortBy: 'createdDate', entitytype: 'contact', entityId: id} })
                .start(params, onGetCrmContactHistory_)
        }

    }

    function onCrmContactFilesAnch(params, id) {
        loadedContactFiles.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.id = id;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmEntityFiles(null, id, 'contact', { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, entitytype: 'contact', entityId: id} })
                .start(params, onGetContactFiles_)
        }
    }

    function onCrmContactPersonesAnch(params, id) {
        loadedContactPersones.all = 0;
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.id = id;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmEntityMembers(params, 'company', id, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1, entitytype: 'company', entityId: id} })
                .start(params, onGetContactPersones_)
        }
    }

    function onContactAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.contactTypes = ServiceFactory.contactTypes;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.getCrmContact(params, id, {
                success: TeamlabMobile.onGetContact_
            });
        }
    }

    function onCrmTaskAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.getCrmTask(params, id, {
                success: onGetTask_
            });
        }
    }

    function onCrmContactTaskAnch(params, contactId, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            params.contactId = contactId;
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.getCrmTask(params, id, {
                success: onGetTask_
            });
        }
    }

    function onAddCrmTaskAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }

        if (id != null) {
            var temp = id.split(/[\/]/);
            var correctid = temp[2];
        }
        Teamlab.joint()
            .getGroups(null)
            .getCrmTaskCategories(null)
            .start(null, function(params, groups, categories) {
                onAddCrmTask_(correctid, groups, categories);
            });
    }

    function onAddCrmCompanyAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        params.contactTypes = ServiceFactory.contactTypes;
        onAddCrmCompany_(params);
    }

    function onAddCrmPersoneAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }

        if (id != null) {
            var temp = id.split(/[\/]/);
            var correctid = temp[2];
        }
        params.id = correctid;
        params.contactTypes = ServiceFactory.contactTypes;
        /*Teamlab.getCrmContacts(null, {           
        success: function (params, companies) { 
        onAddCrmPersone_(params, companies);
        },
        filter: {sortby: 'displayname', isCompany: true}
        });*/
        onAddCrmPersone_(params);
    }

    function onEditCrmPersoneAnch(params, id) {
        TeamlabMobile.editCrmContact(id);
    }

    function onEditCrmCompanyAnch(params, id) {
        TeamlabMobile.editCrmContact(id);
    }

    function onAddCrmHistoryEventAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        Teamlab.getCrmListItem({}, 3, {
            success: function(params, items) {
                onAddCrmHistoryEvent_(params, id, items);
            }
        });
    }

    function onAddCrmNoteAnch(params, contactid) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(staticAnchors.auth);
            return undefined;
        }
        if (contactid) {
            Teamlab.getCrmContact(null, contactid, onAddCrmNote_);
        }
    }

    TeamlabMobile.onGetContact_ = function(params, item) {
        params.phone = null;
        params.email = null;
        for (var i = 0; i < item.commonData.length; i++) {
            for (var id in params.contactTypes) {
                if (item.commonData[i].infoType == params.contactTypes[id].id) {
                    for (var k in params.contactTypes[id].categories) {
                        if (item.commonData[i].category == params.contactTypes[id].categories[k].id) {
                            item.commonData[i].category_title = params.contactTypes[id].categories[k].title;
                            item.commonData[i].category_id = params.contactTypes[id].categories[k].id;
                        }
                    }
                }
            }
        }
        if (item.commonData.length > 0) {
            for (var i = 0; i < item.commonData.length; i++) {
                if (item.commonData[i].infoType == 0 && item.commonData[i].isPrimary) params.phone = item.commonData[i].data;
                if (item.commonData[i].infoType == 1 && item.commonData[i].isPrimary) params.email = item.commonData[i].data;
            }
        }
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contact)) {
            if (item.contactclass == "company") {
                eventManager.call(customEvents.companyPage, window, [item, params]);
            }
            if (item.contactclass == "person") {
                eventManager.call(customEvents.personPage, window, [item, params]);
            }
        }
    }

    function onGetTask_(params, item) {
        item.classname = item.type;
        item.description = TeamlabMobile.htmlEncodeLight(item.description);
        data = item.deadLineString.split(/[  ]/);
        item.deadlineTime = data[0];
        if (data[1] == "AM" || data[1] == "PM") item.deadlineDate = data[2];
        else item.deadlineDate = data[1];
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.crmtask) || ASC.Controls.AnchorController.testAnchor(anchorRegExp.contacttask)) {
            eventManager.call(customEvents.crmTaskPage, window, [item, params]);
        }
    }

    function onAddCrmTask_(params, groups, categories) {
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.crmaddtask)) {
            eventManager.call(customEvents.crmAddTaskPage, window, [params, groups, categories]);
        }
    }

    function onAddCrmCompany_(params) {
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.addcompany)) {
            eventManager.call(customEvents.crmAddCompanyPage, window, [null, params]);
        }
    }

    function onAddCrmPersone_(params, companies) {
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.addpersone)) {
            eventManager.call(customEvents.crmAddPersonePage, window, [null, companies, params]);
        }
    }

    function onAddCrmHistoryEvent_(params, id, items) {
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.addhistoryevent)) {
            eventManager.call(customEvents.crmAddHistoryEventPage, window, [params, id, items]);
        }
    }

    function onAddCrmNote_(params, contact) {

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.addnote)) {

            eventManager.call(customEvents.crmAddNotePage, window, [params, contact]);
        }
    }

    function checkCrmNextIndex(items, loadedIndex) {
        if (items.length == loadedIndex) {
            var nextIndex = true;
        } else {
            var nextIndex = false;
        }
        return nextIndex;
    }

    function preparationCrmItem(item) {
        item.classname = item.type;
        item.href = 'crm/' + item.type + '/' + item.id;
        return item;
    }

    function onGetCrmItems_(params, items) {
        params.nextIndex = checkCrmNextIndex(items, loadedCrmItems.all + TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        var itemsInd = items.length;
        while (itemsInd--) {
            items[itemsInd] = preparationCrmItem(items[itemsInd]);
        }
        loadedCrmItems.all = items.length;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.crm) || ASC.Controls.AnchorController.testAnchor(anchorRegExp.crmsearch)) {
            eventManager.call(customEvents.crmPage, window, [items, params]);
        }
    }

    function onGetMoreCrmItems_(params, items) {
        params.nextIndex = checkCrmNextIndex(items, TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        params.tmpl = 'template-crm-timeline';
        var itemsInd = items.length;
        while (itemsInd--) {
            items[itemsInd] = preparationCrmItem(items[itemsInd]);
        }
        loadedCrmItems.all = loadedCrmItems.all + items.length;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.crm) || ASC.Controls.AnchorController.testAnchor(anchorRegExp.crmsearch)) {
            eventManager.call(customEvents.addMoreCrmItemsToList, window, [items, params]);
        }
    }

    TeamlabMobile.preparationCrmTask = function(task) {
        var currentDate = new Date();
        var data = [];
        task.classname = task.type;
        task.href = 'crm/' + task.type + '/' + task.id;
        data = task.deadLineString.split(/[  ]/);
        task.deadlineTime = data[0];
        if (data[1] == "AM" || data[1] == "PM") task.deadlineDate = data[2];
        else task.deadlineDate = data[1];
        var curdata = new Date();
        if (curdata >= task.deadLine) {
            task.isOverdue = true;
            task.timeType = "late";
        }
        else {
            task.isOverdue = false;
            if (curdata <= task.deadLine) {
                task.timeType = "nextdays";
            }
            else {
                task.timeType = "today";
            }
        }     
        return task;
    }

    function onGetTasks_(params, items) {
        params.nextIndex = checkCrmNextIndex(items, loadedCrmTasks.all + TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        var itemsInd = items.length;
        while (itemsInd--) {
            items[itemsInd] = TeamlabMobile.preparationCrmTask(items[itemsInd]);
        }
        loadedCrmTasks.all = items.length;

        //if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.taskstoday)) {
        eventManager.call(customEvents.crmTasksPage, window, [items, params]);
        //}
    }

    function onGetMoreTasks_(params, items) {
        params.nextIndex = checkCrmNextIndex(items, TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        params.tmpl = 'template-crm-tasks-timeline';
        var itemsInd = items.length;
        while (itemsInd--) {
            items[itemsInd] = TeamlabMobile.preparationCrmTask(items[itemsInd]);
        }

        loadedCrmTasks.all = loadedCrmTasks.all + items.length;

        //if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.taskstoday)) {
        //eventManager.call(customEvents.addMoreCrmTasksToList, window, [items, params]);
        eventManager.call(customEvents.addMoreCrmItemsToList, window, [items, params]);
        //}
    }

    function onGetContactTasks_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        params.nextIndex = checkCrmNextIndex(items, loadedCrmContactTasks.all + TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        if (items) {
            var itemsInd = items.length;
            while (itemsInd--) {
                items[itemsInd] = TeamlabMobile.preparationCrmTask(items[itemsInd]);
            }
            loadedCrmContactTasks.all = items.length;
            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contacttasks)) {
                eventManager.call(customEvents.crmContactTasksPage, window, [contact, items, params]);
            }
        }
    }

    function onGetMoreContactTasks_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        params.nextIndex = checkCrmNextIndex(items, TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        params.tmpl = 'template-crm-tasks-timeline';
        if (items) {
            var itemsInd = items.length;
            while (itemsInd--) {
                items[itemsInd] = TeamlabMobile.preparationCrmTask(items[itemsInd]);
            }
            loadedCrmContactTasks.all = loadedCrmContactTasks.all + items.length;
            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contacttasks)) {
                eventManager.call(customEvents.addMoreCrmItemsToList, window, [items, params]);
            }
        }
    }

    function onGetCrmContactHistory_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        params.nextIndex = checkCrmNextIndex(items, loadedCrmHistoryEvents.all + TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        loadedCrmHistoryEvents.all = items.length;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contacthistory)) {
            eventManager.call(customEvents.crmContactHistoryPage, window, [contact, items, params]);
        }
    }

    function onGetCrmMoreContactHistory_(params, contact, items) {
        params.nextIndex = checkCrmNextIndex(items, TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        params.tmpl = 'template-page-crm-contacthistory';
        loadedCrmHistoryEvents.all = loadedCrmHistoryEvents.all + items.length;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contacthistory)) {
            eventManager.call(customEvents.addMoreCrmItemsToList, window, [items, params]);
        }
    }

    function onGetContactFiles_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        if (items.length == loadedContactFiles.all + TeamlabMobile.constants.pageItems + 1) {
            params.nextIndex = true;
            items.length = items.length - 1;
        }
        else params.nextIndex = false;
        if (items) {
            var item = null, itemsInd = items.length;
            while (itemsInd--) {
                item = items[itemsInd];
                item.classname = item.type;
                //item.href = 'crm/' + item.type + '/' + item.id;
                item.target = 'target-blank';
                item.href = TeamlabMobile.getViewUrl(item);
            }
            loadedContactFiles.all = items.length;

            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contactfiles)) {
                eventManager.call(customEvents.crmContactFilesPage, window, [contact, items, params]);
            }
        }
    }

    function onGetContactPersones_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        params.nextIndex = checkCrmNextIndex(items, loadedContactPersones.all + TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;

        if (items) {
            var item = null, itemsInd = items.length;
            while (itemsInd--) {
                item = items[itemsInd];
                item.classname = item.type;
                item.href = 'crm/' + item.type + '/' + item.id;
            }
            loadedContactPersones.all = items.length;

            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contactpersones)) {
                eventManager.call(customEvents.crmContactPersonesPage, window, [contact, items, params]);
            }
        }
    }

    function onGetMoreContactPersones_(params, contact, items) {
        params = (params && params.length ? params[params.length - 1] : {});
        params.nextIndex = checkCrmNextIndex(items, TeamlabMobile.constants.pageItems + 1);
        if (params.nextIndex) items.length = items.length - 1;
        params.tmpl = 'template-crm-persones-timeline';
        if (items) {
            var item = null, itemsInd = items.length;
            while (itemsInd--) {
                item = items[itemsInd];
                item.classname = item.type;
                item.href = 'crm/' + item.type + '/' + item.id;
            }
            loadedContactPersones.all = loadedContactPersones.all + items.length;
            if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.contactpersones)) {
                eventManager.call(customEvents.addMoreCrmItemsToList, window, [items, params]);
            }
        }
    }

    function onAddCrmNoteToContact_(params, file) {
        var contactid = params.hasOwnProperty('contactid') ? params.contactid || -1 : -1;
        if (contactid !== -1) {
            ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.crm_contact + contactid + '/files');
        }
    }

    TeamlabMobile.getMoreCrmItems = function(type) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedCrmItems.all;
                break;
            case 'contact':
                index = loadedCrmItems.contacts;
                break;
            default:
                return undefined;
        }

        if (index !== -1) {
            getMoreCrmItems_(type, index, TeamlabMobile.constants.pageItems);
        }
    };

    TeamlabMobile.getCrmContactsBySearchValue = function(searchvalue) {
        ASC.Controls.AnchorController.move(searchvalue ? staticAnchors.crm_search + Base64.encode(encodeURI(searchvalue)) : staticAnchors.crm);
    }

    TeamlabMobile.getCrmTasksBySearchValue = function(searchvalue) {
        ASC.Controls.AnchorController.move(staticAnchors.tasks_search + Base64.encode(encodeURI(searchvalue)));
    }
    function getMoreCrmItems_(type, index, count) {
        Teamlab.getCrmContacts(null, {
            success: onGetMoreCrmItems_,
            filter: { startIndex: index, count: count + 1, sortby: 'displayname' }
        });

    }

    TeamlabMobile.getMoreCrmTasks = function(type, page) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedCrmTasks.all;
                break;
            case 'task':
                index = loadedCrmItems.tasks;
                break;
            default:
                return undefined;
        }
        if (index !== -1) {
            getMoreCrmTasks_(type, index, TeamlabMobile.constants.pageItems, page);
        }
    }

    function getMoreCrmTasks_(type, index, count, page) {
        //all_count = count+index + 1;         
        var curDate = new Date();
        var today = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 0, 0, 0, 1);
        var nextDay = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate(), 23, 59, 59, 999);
        if (page == "today") {
            Teamlab.getCrmTasks({ page: 'today' }, {
                success: onGetMoreTasks_,
                filter: { startIndex: index, count: count + 1, sortby: 'displayname', fromDate: today, toDate: today, isClosed: false }
            });
        }
        if (page == "nextdays") {
            Teamlab.getCrmTasks({ page: 'nextdays' }, {
                success: onGetMoreTasks_,
                filter: { startIndex: index, count: count + 1, sortby: 'displayname', fromDate: nextDay, isClosed: false }
            });
        }
        if (page == "late") {
            Teamlab.getCrmTasks({ page: 'late' }, {
                success: onGetMoreTasks_,
                filter: { startIndex: index, count: count + 1, sortby: 'displayname', toDate: today, isClosed: false }
            });
        }
        if (page == "closed") {
            Teamlab.getCrmTasks({ page: 'closed' }, {
                success: onGetMoreTasks_,
                filter: { startIndex: index, count: count + 1, sortby: 'displayname', isClosed: true }
            });
        }
    }

    TeamlabMobile.getMoreCrmContactTasks = function(type, id) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedCrmContactTasks.all;
                break;
            case 'task':
                index = loadedCrmItems.tasks;
                break;
            default:
                return undefined;
        }
        if (index !== -1) {
            getMoreCrmContactTasks_(type, index, TeamlabMobile.constants.pageItems, id);
        }
    }

    function getMoreCrmContactTasks_(type, index, count, id) {
        var params = {};
        params.id = id;
        //all_count = count + index + 1;
        Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmTasks(params, { filter: { startIndex: index, count: count + 1, sortby: 'displayname', entitytype: 'contact', entityId: id} })
                .start(params, onGetMoreContactTasks_)
    }

    TeamlabMobile.getMoreCrmHistoryEvents = function(type, id) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedCrmHistoryEvents.all;
                break;
            default:
                return undefined;
        }

        if (index !== -1) {
            getMoreCrmHistoryEvents_(type, index, TeamlabMobile.constants.pageItems, id);
        }
    }

    function getMoreCrmHistoryEvents_(type, index, count, id) {
        var params = {};
        params.id = id;
        all_count = count + index + 1;
        Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmHistoryEvents(null, { filter: { startIndex: 0, count: all_count, entitytype: 'contact', entityId: id} })
                .start(params, onGetCrmContactHistory_)
    }

    TeamlabMobile.getMoreCrmContactPersones = function(type, id) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedContactPersones.all;
                break;
            default:
                return undefined;
        }
        if (index !== -1) {
            getMoreCrmContactPersones_(type, index, TeamlabMobile.constants.pageItems, id);
        }
    }

    function getMoreCrmContactPersones_(type, index, count, id) {
        var params = {};
        params.id = id;
        //all_count = count + index + 1;
        Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmEntityMembers(params, 'company', id, { filter: { startIndex: index, count: count + 1, entitytype: 'contact', entityId: id} })
                .start(params, onGetMoreContactPersones_)
    }

    TeamlabMobile.getMoreCrmContactFiles = function(type, id) {
        var index = -1;
        switch (type) {
            case null:
                index = loadedContactFiles.all;
                break;
            default:
                return undefined;
        }

        if (index !== -1) {
            getMoreCrmContactFiles_(type, index, TeamlabMobile.constants.pageItems, id);
        }
    }

    function getMoreCrmContactFiles_(type, index, count, id) {
        var params = {};
        params.id = id;
        all_count = count + index + 1;
        Teamlab.joint()
                .getCrmContact(null, id)
                .getCrmEntityFiles(null, id, 'contact', { filter: { startIndex: 0, count: all_count, entitytype: 'contact', entityId: id} })
                .start(params, onGetContactFiles_)
    }

    TeamlabMobile.updateCrmTaskStatus = function(param, id, closed) {
        var data = {};
        if (closed) {
            data.isClosed = true;
        }
        if (!closed) {
            data.isClosed = false;
        }
        Teamlab.updateCrmTask(null, id, data, {
            success: function(params, id, status) {
                updateCrtmTask_(params, id, status);
                if (param.contactId != null) {
                    data.contactId = param.contactId;
                    data.categoryId = -1;
                    if (data.isClosed) data.content = 'task ' + params.taskid.title + ' closed';
                    else data.content = 'task ' + params.taskid.title + ' reopened';
                    data.created = Teamlab.serializeTimestamp(new Date());
                    Teamlab.addCrmHistoryEvent(null, data);
                }
            }
        })
    }

    TeamlabMobile.addCrmNoteToContact = function(contactid, data) {
        return Teamlab.addCrmEntityNote({ contactid: contactid }, 'contact', contactid, data, onAddCrmNoteToContact_);
    };

    function updateCrtmTask_(params, id, status) {
        params.taskid = id;
        eventManager.call(customEvents.updateCrmTaskCheckbox, window, [params]);
    }

    TeamlabMobile.editCrmContact = function(id) {
        Teamlab.getCrmContact(null, id, {
            success: function(params, item) {
                params.contactTypes = ServiceFactory.contactTypes;
                if (item.isCompany) {
                    eventManager.call(customEvents.crmAddCompanyPage, window, [item, params]);
                }
                else {
                    eventManager.call(customEvents.crmAddPersonePage, window, [item, null, params]);
                }
            }
        });
    }

    return TeamlabMobile;
})(TeamlabMobile);
