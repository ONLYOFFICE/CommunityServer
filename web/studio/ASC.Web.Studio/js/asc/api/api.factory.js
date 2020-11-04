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


;
window.ServiceFactory = (function() {
    var isInit = false,
        onlyFactory = false,
        defaultAvatar = '',
        defaultAvatarMedium = '',
        defaultAvatarSmall = '',
        formatDatetime = null,
        formatDate = null,
        formatTime = null,
        formats = { datetime: formatDatetime, date: formatDate, time: formatTime },
        monthNames = [],
        monthShortNames = [],
        dayNames = [],
        dayShortNames = [],
        nameCollections = { days: dayNames, shortdays: dayShortNames, months: monthNames, shortmonths: monthShortNames },
        portalUtcOffsetTotalMinutes = 0,
        portalUtcOffset = '',
        portalTimeZoneName = '',
        myProfile = {},
        portalSettings = {},
        portalQuotas = {},
        post = 'post',
        get = 'get',
        dlt = 'delete',
        put = 'put',
        supportedImgs = [],
        supportedDocs = [],
        supportedTypes = [],
        searchEntityTypes = [
            { id: -1, label: 'unknown' },
            { id: 0, label: 'project' },
            { id: 1, label: 'milestone' },
            { id: 2, label: 'task' },
            { id: 3, label: 'subtask' },
            { id: 4, label: 'team' },
            { id: 5, label: 'comment' },
            { id: 6, label: 'discussion' },
            { id: 7, label: 'file' },
            { id: 8, label: 'timespend' },
            { id: 9, label: 'activity' }
        ],
        folderTypes = [
            { id: 0, name: 'DEFAULT', label: 'folder-default' },
            { id: 1, name: 'COMMON', label: 'folder-common' },
            { id: 2, name: 'BUNCH', label: 'folder-bunch' },
            { id: 3, name: 'TRASH', label: 'folder-trash' },
            { id: 5, name: 'USER', label: 'folder-user' },
            { id: 6, name: 'SHARE', label: 'folder-shared' }
        ],
        // mail - 0, tel - 1, link - 2
        contactTitles = {
            mail: { name: 'mail', type: 0, title: 'Email' },
            facebook: { name: 'facebook', type: 2, title: 'FaceBook' },
            myspace: { name: 'myspace', type: 2, title: 'MySpace' },
            livejournal: { name: 'livejournal', type: 2, title: 'LiveJournal' },
            twitter: { name: 'twitter', type: 2, title: 'twitter' },
            yahoo: { name: 'yahoo', type: 2, title: 'YAHOO' },
            jabber: { name: 'jabber', type: 2, title: 'Jabber' },
            blogger: { name: 'blogger', type: 2, title: 'blogger' },
            skype: { name: 'skype', type: 2, title: 'skype' },
            msn: { name: 'msn', type: 2, title: 'msn' },
            aim: { name: 'aim', type: 2, title: 'aim' },
            icq: { name: 'icq', type: 2, title: 'icq' },
            gmail: { name: 'gmail', type: 0, title: 'Google Mail' },
            gbuzz: { name: 'gbuzz', type: 2, title: 'Google Buzz' },
            gtalk: { name: 'gtalk', type: 2, title: 'Google Talk' },
            phone: { name: 'phone', type: 1, title: 'Tel' },
            mobphone: { name: 'mobphone', type: 1, title: 'Mobile' },
            extmobphone: { name: 'extmobphone', type: 1, title: 'Mobile' },
            extmail: { name: 'extmail', type: 0, title: 'Email' }
        },
        contactTypes = {
            phone: { id: 0, title: 'Phone', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, mobile: { id: 2, title: 'Mobile' }, fax: { id: 3, title: 'Fax' }, direct: { id: 4, title: 'Direct' }, other: { id: 5, title: 'Other' } } },
            email: { id: 1, title: 'Email', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            website: { id: 2, title: 'Website', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            skype: { id: 3, title: 'Skype', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            twitter: { id: 4, title: 'Twitter', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            linkedin: { id: 5, title: 'LinkedIn', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            facebook: { id: 6, title: 'Facebook', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            address: { id: 7, title: 'Address', categories: { home: { id: 0, title: 'Home' }, postal: { id: 1, title: 'Postal' }, office: { id: 2, title: 'Office' }, billing: { id: 3, title: 'Billing' }, other: { id: 4, title: 'Other' }, work: { id: 5, title: 'Work' } } },
            livejournal: { id: 8, title: 'LiveJournal', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            myspace: { id: 9, title: 'MySpace', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            gmail: { id: 10, title: 'GMail', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            blogger: { id: 11, title: 'Blogger', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            yahoo: { id: 12, title: 'Yahoo', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            msn: { id: 13, title: 'MSN', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            icq: { id: 14, title: 'ICQ', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            jabber: { id: 15, title: 'Jabber', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } },
            aim: { id: 16, title: 'AIM', categories: { home: { id: 0, title: 'Home' }, work: { id: 1, title: 'Work' }, other: { id: 2, title: 'Other' } } }
        },
        extTypes = [
            { name: 'archive', exts: ['.zip', '.rar', '.ace', '.arc', '.arj', '.cab', '.enc', '.jar', '.lha', '.lzh', '.pak', '.pk3', '.tar', '.tgz', '.uue', '.xxe', '.zoo', '.bh', '.gz', '.ha'] },
            { name: 'image', exts: ['.bmp', '.cod', '.gif', '.ief', '.jpe', '.jpg', '.tif', '.cmx', '.ico', '.pnm', '.pbm', '.ppm', '.psd', '.rgb', '.xbm', '.xpm', '.xwd', '.png', '.ai', '.jpeg'] },
            { name: 'sound', exts: ['.mp3', '.wav', '.pcm', '.3gp', '.fla', '.cda', '.ogg', '.aiff', '.flac'] },
            { name: 'ebook', exts: ['.fb2', '.ibk', '.prc', '.epub'] },
            { name: 'html', exts: ['.htm', '.mht', '.html'] },
            { name: 'djvu', exts: ['.djvu'] },
            { name: 'svg', exts: ['.svg'] },
            { name: 'svgt', exts: ['.svgt'] },
            { name: 'doc', exts: ['.doc', '.docx'] },
            { name: 'doct', exts: ['.doct'] },
            { name: 'xls', exts: ['.xls', '.xlsx'] },
            { name: 'xlst', exts: ['.xlst'] },
            { name: 'pps', exts: ['.pps', '.ppsx'] },
            { name: 'ppt', exts: ['.ppt', '.pptx'] },
            { name: 'pptt', exts: ['.pptt'] },
            { name: 'odp', exts: ['.odp'] },
            { name: 'ods', exts: ['.ods'] },
            { name: 'odt', exts: ['.odt'] },
            { name: 'pdf', exts: ['.pdf'] },
            { name: 'rtf', exts: ['.rtf'] },
            { name: 'txt', exts: ['.txt'] },
            { name: 'iaf', exts: ['.iaf'] },
            { name: 'csv', exts: ['.csv'] },
            { name: 'xml', exts: ['.xml'] },
            { name: 'xps', exts: ['.xps'] },
            { name: 'avi', exts: ['.avi'] },
            { name: 'flv', exts: ['.flv', '.fla'] },
            { name: 'm2ts', exts: ['.m2ts'] },
            { name: 'mkv', exts: ['.mkv'] },
            { name: 'mov', exts: ['.mov'] },
            { name: 'mp4', exts: ['.mp4'] },
            { name: 'mpg', exts: ['.mpg'] },
            { name: 'vob', exts: ['.vob'] }
        ],
        profileStatuses = {
            active: { id: 1, name: 'active' },
            terminated: { id: 2, name: 'terminated' },
            leaveofabsence: { id: 4, name: 'leaveofabsence' }
        },
        activationStatuses = {
            notactivated: { id: 0, name: 'notactivated' },
            activated: { id: 1, name: 'activated' },
            pending: { id: 2, name: 'pending' }
        },
        taskStatuses = {
            notaccept: { id: 0, name: 'notaccept' },
            open: { id: 1, name: 'open' },
            closed: { id: 2, name: 'closed' },
            disable: { id: 3, name: 'disable' },
            unclassified: { id: 4, name: 'unclassified' },
            notinmilestone: { id: 5, name: 'notinmilestone' }
        },
        fileShareTypes = {
            None: '0',
            ReadWrite: '1',
            Read: '2',
            Restrict: '3'
        },
        apiAnchors = [
            apiHandler('cmt-blog', /blog\.json/, post),
            apiHandler('cmt-blog', /blog\/[\w\d-]+\.json/),
            apiHandler('cmt-topic', /forum\/topic\/[\w\d-]+\.json/),
            apiHandler('cmt-event', /event\/[\w\d-]+\.json/),
            apiHandler('cmt-event', /event\.json/, post),
            apiHandler('cmt-bookmark', /bookmark\/[\w\d-]+\.json/),
            apiHandler('cmt-bookmark', /bookmark\.json/, post),
            apiHandler('cmt-blogs', /blog\.json/, get),
            apiHandler('cmt-blogs', /blog\/@search\/.+\.json/, get),
            apiHandler('cmt-topics', /forum\/topic\/recent\.json/),
            apiHandler('cmt-topics', /forum\/@search\/.+\.json/, get),
            apiHandler('cmt-categories', /forum\.json/),
            apiHandler('cmt-events', /event\.json/, get),
            apiHandler('cmt-events', /event\/@search\/.+\.json/, get),
            apiHandler('cmt-bookmarks', /bookmark\/top\/recent\.json/, get),
            apiHandler('cmt-bookmarks', /bookmark\/@search\/.+\.json/, get),
            apiHandler('prj-task', /project\/[\w\d-]+\/task\.json/, post),
            apiHandler('prj-task', /project\/task\/[\w\d-]+\.json/),
            apiHandler('prj-task', /project\/task\/[\w\d-]+\/status\.json/),
            apiHandler('prj-task', /project\/task\/[\w\d-]+\/copy\.json/),
            apiHandler('prj-task', /project\/task\/[\w\d-]+\/milestone\.json/),
            apiHandler('prj-tasks', /project\/[\w\d-]+\/task\.json/, get),
            apiHandler('prj-tasks', /project\/[\w\d-]+\/task\/@self\.json/),
            apiHandler('prj-tasks', /project\/[\w\d-]+\/task\/filter\.json/),
            apiHandler('prj-tasks', /project\/task\/filter\.json/),
            apiHandler('prj-tasks', /project\/task\.json\?taskid/),
            apiHandler('prj-tasks', /project\/task\/status\.json/),
            apiHandler('prj-tasks', /project\/task\/milestone\.json/),
            apiHandler('prj-tasks', /project\/task\.json/, dlt),
            apiHandler('prj-simpletasks', /project\/task\/filter\/simple\.json/, get),
            apiHandler('prj-tasks', /project\/task\/@self\.json/),
            apiHandler('prj-subtask', /project\/task\/[\d]+\.json/, post),
            apiHandler('prj-subtask', /project\/task\/[\d]+\/[\d]+\/copy\.json/),
            apiHandler('prj-subtask', /project\/task\/[\d]+\/[\d]+\.json/),
            apiHandler('prj-subtask', /project\/task\/[\d]+\/[\d]+\/status\.json/),
            apiHandler('prj-milestone', /project\/milestone\/[\w\d-]+\.json/),
            apiHandler('prj-milestone', /project\/[\w\d-]+\/milestone\.json/, post),
            apiHandler('prj-milestones', /project\/[\w\d-]+\/milestone\.json/, get),
            apiHandler('prj-milestones', /project\/milestone\/[\w\d-]+\/[\w\d-]+\/[\w\d-]+\.json/),
            apiHandler('prj-milestones', /project\/milestone\/[\w\d-]+\/[\w\d-]+\.json/),
            apiHandler('prj-milestones', /project\/milestone\/late\.json/),
            apiHandler('prj-milestones', /project\/milestone\.json/),
            apiHandler('prj-milestones', /project\/milestone\/filter\.json/),
            apiHandler('prj-milestone', /project\/milestone\/[\w\d-]+\/status\.json/),
            apiHandler('prj-discussion', /project\/message\/[\w\d-]+\.json/),
            apiHandler('prj-discussion', /project\/[\w\d-]+\/message\.json/, post),
            apiHandler('prj-discussions', /project\/message\.json/),
            apiHandler('prj-discussions', /project\/[\w\d-]+\/message\.json/, get),
            apiHandler('prj-discussions', /project\/message\/filter\.json/),

            apiHandler('prj-project', /project\/[\d-]+\.json/, get),
            apiHandler('prj-project', /project\/[\d-]+\/status\.json/, put),
            apiHandler('prj-project', /project\/[\w\d-]+\/contact\.json/, post),
            apiHandler('prj-project', /project\/[\w\d-]+\/contact\.json/, dlt),
            apiHandler('prj-projects', /project[\/]*[@self|@follow]*\.json/),
            apiHandler('prj-projects', /project\/filter\.json/),
            apiHandler('prj-projects', /project\/contact\/[\w\d-]+\.json/),
            apiHandler('prj-searchentries', /project\/@search\/.+\.json/),
            apiHandler('prj-projectperson', /project\/[\w\d-]+\/team\.json/, post),
            apiHandler('prj-projectperson', /project\/[\w\d-]+\/team\.json/, dlt),
            apiHandler('prj-projectpersons', /project\/[\w\d-]+\/team\.json/, get),
            apiHandler('prj-projectpersons', /project\/[\w\d-]+\/teamExcluded\.json/, get),
            apiHandler('prj-timespend', /project\/time\/[\w\d-]+\.json/),
            apiHandler('prj-timespends', /project\/time\/times\/status\.json/, put),
            apiHandler('prj-timespends', /project\/time\/filter\.json/, get),
            apiHandler('prj-timespends', /project\/task\/[\w\d-]+\/time\.json/, get),
            apiHandler('prj-activities', /project\/activities\/filter\.json/),
            apiHandler('prj-project', /project\.json/, post),
            apiHandler('doc-folder', /files\/[@\w\d-]+\.json/),
            apiHandler('doc-folder', /files\/[\w\d-]+\/[text|html|file]+\.json/),
            apiHandler('doc-folder', /project\/[\w\d-]+\/files\.json/),
            apiHandler('doc-files', /project\/task\/[\w\d-]+\/files\.json/),
            apiHandler('doc-files', /project\/[\w\d-]+\/entityfiles\.json/),
            apiHandler('doc-files', /crm\/[\w\d-]+\/[\w\d-]+\/files\.json/, get),
            apiHandler('doc-file', /files\/file\/[\w\d-]+\.json/),
            apiHandler('doc-file', /files\/[\w\d-]+\/upload\.xml/),
            apiHandler('doc-file', /files\/[\w\d-]+\/upload\.json/),
            apiHandler('doc-session', /files\/[\w\d-]+\/upload\/create_session\.json/),
            apiHandler('doc-file', /crm\/files\/[\w\d-]+\.json/),
            apiHandler('doc-file', /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/upload\.xml/),
            apiHandler('doc-file', /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/upload\.json/),
            apiHandler('doc-file', /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/text\.json/),
            apiHandler('doc-miss', /files\/fileops.json/),
            apiHandler('doc-miss', /files\/storeoriginal.json/),
            apiHandler('doc-miss', /files\/hideconfirmconvert.json/),
            apiHandler('doc-miss', /files\/displayfavorite.json/),
            apiHandler('doc-miss', /files\/displayrecent.json/),
            apiHandler('doc-miss', /files\/displaytemplates.json/),
            apiHandler('doc-check', /files\/docservice.json/),
            apiHandler('crm-addresses', /crm\/contact\/[\w\d-]+\/data\.json/, get),
            apiHandler('crm-address', /crm\/contact\/[\w\d-]+\/data\/[\w\d-]+\.json/),
            apiHandler('crm-address', /crm\/contact\/[\w\d-]+\/data\.json/, post),
            apiHandler('crm-address', /crm\/contact\/[\w\d-]+\/batch\.json/),
            apiHandler('crm-contact', /crm\/contact\/[\w\d-]+\.json/),
            apiHandler('crm-contact', /crm\/contact\/company\/[\w\d-]+\/type\.json/, post),
            apiHandler('crm-contact', /crm\/contact\/company\/[\w\d-]+\/person\.json/, post),
            apiHandler('crm-contact', /crm\/contact\/company\/[\w\d-]+\/person\.json/, dlt),
            apiHandler('crm-contact', /crm\/[case|opportunity]+\/[\w\d-]+\/contact\.json/, post),
            apiHandler('crm-contact', /crm\/[case|opportunity]+\/[\w\d-]+\/contact\/[\w\d-]+\.json/, post),
            apiHandler('crm-contact', /crm\/[case|opportunity]+\/[\w\d-]+\/contact\/[\w\d-]+\.json/),
            apiHandler('crm-contacts', /crm\/contact\/bycontactinfo\.json/, get),
            apiHandler('crm-contact', /crm\/contact\/merge\.json/),
            apiHandler('crm-socialmediaavatars', /crm\/contact\/socialmediaavatar\.json/),
            apiHandler('crm-tweets', /crm\/contact\/[\w\d-]+\/tweets\.json/),
            apiHandler('crm-twitterprofiles', /crm\/contact\/twitterprofile\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/mailsmtp\/send\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/mailsmtp\/status\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/mailsmtp\/cancel\.json/),
            apiHandler('crm-progressitem', /crm\/[contact|person|company|opportunity|case]+\/import\/status\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/export\/status\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/export\/cancel\.json/),
            apiHandler('crm-progressitem', /crm\/contact\/export\/start\.json/),
            apiHandler('crm-exportitem', /crm\/export\/partial\/status\.json/),
            apiHandler('crm-exportitem', /crm\/export\/partial\/cancel\.json/),
            apiHandler('crm-exportitem', /crm\/export\/partial\/[\w\d-]+\/start\.json/),
            apiHandler('crm-fileuploadresult', /crm\/import\/uploadfake\.json/),
            apiHandler('crm-task', /crm\/task\.json/),
            apiHandler('crm-task', /crm\/task\/[\w\d-]+\.json/),
            apiHandler('crm-task', /crm\/task\/[\w\d-]+\/close\.json/),
            apiHandler('crm-task', /crm\/task\/[\w\d-]+\/reopen\.json/),
            apiHandler('crm-opportunity', /crm\/opportunity\/[\w\d-]+\.json/),
            apiHandler('crm-opportunity', /crm\/opportunity\/[\w\d-]+\/stage\/[\w\d-]+\.json/),
            apiHandler('crm-opportunity', /crm\/contact\/[\w\d-]+\/opportunity\/[\w\d-]+\.json/),
            apiHandler('crm-dealmilestone', /crm\/opportunity\/stage\/[\w\d-]+\.json/),
            apiHandler('crm-dealmilestone', /crm\/opportunity\/stage\/[\w\d-]+\/color\.json/),
            apiHandler('crm-dealmilestone', /crm\/opportunity\/stage\.json/, post),
            apiHandler('crm-dealmilestones', /crm\/opportunity\/stage\/reorder\.json/),
            apiHandler('crm-dealmilestones', /crm\/opportunity\/stage\.json/, get),
            apiHandler('crm-contactstatus', /crm\/contact\/status\/[\w\d-]+\/color\.json/),
            apiHandler('crm-contactstatus', /crm\/contact\/status\/[\w\d-]+\.json/),
            apiHandler('crm-contactstatus', /crm\/contact\/status\.json/, post),
            apiHandler('crm-contactstatuses', /crm\/contact\/status\/reorder\.json/),
            apiHandler('crm-contactstatuses', /crm\/contact\/status\.json/, get),
            apiHandler('crm-contacttypekind', /crm\/contact\/type\/[\w\d-]+\.json/),
            apiHandler('crm-contacttypekind', /crm\/contact\/type\.json/, post),
            apiHandler('crm-contacttypekinds', /crm\/contact\/type\/reorder\.json/),
            apiHandler('crm-contacttypekinds', /crm\/contact\/type\.json/, get),
            apiHandler('crm-customfield', /crm\/[contact|person|company|opportunity|case]+\/customfield\/[\w\d-]+\.json/),
            apiHandler('crm-customfield', /crm\/[contact|person|company|opportunity|case]+\/customfield\.json/),
            apiHandler('crm-customfields', /crm\/[contact|person|company|opportunity|case]+\/customfield\/definitions\.json/),
            apiHandler('crm-customfields', /crm\/[contact|person|company|opportunity|case]+\/customfield\/reorder\.json/),
            apiHandler('crm-tag', /crm\/[case|contact|opportunity]+\/tag\.json/),
            apiHandler('crm-tag', /crm\/[case|contact|opportunity]+\/taglist\.json/),
            apiHandler('crm-tag', /crm\/[case|contact|opportunity]+\/[\w\d-]+\/tag\.json/),
            apiHandler('crm-tags', /crm\/[case|contact|opportunity]+\/tag\/unused\.json/),
            apiHandler('crm-tags', /crm\/[case|contact|opportunity]+\/tag\/[\w\d-]+\.json/),
            apiHandler('crm-fulltags', /crm\/[case|contact|opportunity]+\/tag\.json/, get),
            apiHandler('crm-caseitem', /crm\/case\/[\w\d-]+\/close\.json/),
            apiHandler('crm-caseitem', /crm\/case\/[\w\d-]+\/reopen\.json/),
            apiHandler('crm-cases', /crm\/cases\.json/),
            apiHandler('crm-cases', /crm\/case\/filter\.json/),
            apiHandler('crm-cases', /crm\/case\/byprefix\.json/),
            apiHandler('crm-contacts', /crm\/contact\.json/),
            apiHandler('crm-contacts', /crm\/contact\/project\/[\w\d-]+\.json/),
            apiHandler('crm-basecontacts', /crm\/contact\/byprefix\.json/),
            apiHandler('crm-contacts', /crm\/contact\/filter\.json/),
            apiHandler('crm-simplecontacts', /crm\/contact\/simple\/filter\.json/),
            apiHandler('crm-basecontacts', /crm\/contact\/mail\.json/),
            apiHandler('crm-contacts', /crm\/contact\/company\/[\w\d-]+\/person\.json/, get),
            apiHandler('crm-contacts', /crm\/[case|opportunity]+\/[\w\d-]+\/contact\.json/, get),
            apiHandler('crm-contacts', /crm\/contact\/access\.json/),
            apiHandler('crm-contacts', /crm\/contact\/[\w\d-]+\/access\.json/, put),
            apiHandler('crm-cases', /crm\/case\/access\.json/),
            apiHandler('crm-cases', /crm\/case\/[\w\d-]+\/access\.json/),
            apiHandler('crm-opportunities', /crm\/opportunity\/access\.json/),
            apiHandler('crm-opportunities', /crm\/opportunity\/[\w\d-]+\/access\.json/),
            //ApiHandler : 'crm-customfields', re : /crm\/contact\/[\w\d-]+\/access\.json/},
            apiHandler('crm-tasks', /crm\/task\/filter\.json/),
            apiHandler('crm-opportunities', /crm\/opportunity\/filter\.json/),
            apiHandler('crm-opportunities', /crm\/opportunity\/bycontact\/[\w\d-]+\.json/),
            apiHandler('crm-opportunities', /crm\/opportunity\/byprefix\.json/),
            apiHandler('crm-taskcategory', /crm\/task\/category\/[\w\d-]+\.json/),
            apiHandler('crm-taskcategory', /crm\/task\/category\/[\w\d-]+\/icon\.json/),
            apiHandler('crm-taskcategory', /crm\/task\/category\.json/, post),
            apiHandler('crm-taskcategories', /crm\/task\/category\/reorder\.json/),
            apiHandler('crm-taskcategories', /crm\/task\/category\.json/, get),
            apiHandler('crm-historyevent', /crm\/history\.json/),
            apiHandler('crm-historyevent', /crm\/history\/[\w\d-]+.json/),
            apiHandler('crm-historyevent', /crm\/[contact|opportunity|case]+\/[\w\d-]+\/files\.json/, post),
            apiHandler('crm-historyevents', /crm\/history\/filter\.json/),
            apiHandler('crm-historycategory', /crm\/history\/category\/[\w\d-]+\.json/),
            apiHandler('crm-historycategory', /crm\/history\/category\/[\w\d-]+\/icon\.json/),
            apiHandler('crm-historycategory', /crm\/history\/category\.json/, post),
            apiHandler('crm-historycategories', /crm\/history\/category\/reorder\.json/),
            apiHandler('crm-historycategories', /crm\/history\/category\.json/, get),
            apiHandler('crm-currency', /crm\/settings\/currency\.json/),
            apiHandler('crm-currencies', /crm\/settings\/currency\.json/, get),
            apiHandler('crm-smtpsettings', /crm\/settings\/smtp\.json/),
            apiHandler('crm-rootfolder', /crm\/files\/root\.json/),
            apiHandler('crm-tasktemplatecontainer', /crm\/[contact|person|company|opportunity|case]+\/tasktemplatecontainer\.json/),
            apiHandler('crm-tasktemplatecontainer', /crm\/tasktemplatecontainer\/[\w\d-]+\.json/),
            apiHandler('crm-tasktemplate', /crm\/tasktemplatecontainer\/[\w\d-]+\/tasktemplate\.json/),
            apiHandler('crm-tasktemplate', /crm\/tasktemplatecontainer\/tasktemplate\/[\w\d-]+\.json/),
            apiHandler('crm-invoiceline', /crm\/invoiceline\.json/),
            apiHandler('crm-invoiceline', /crm\/invoice\/[\w\d-]+\.json/),
            apiHandler('crm-invoice', /crm\/invoice\.json/, post),
            apiHandler('crm-invoice', /crm\/invoice\/[\w\d-]+\.json/),
            apiHandler('crm-invoice', /crm\/invoice\/sample\.json/),
            apiHandler('crm-invoice', /crm\/invoice\/sample\.json/),
            apiHandler('crm-invoice', /crm\/invoice\/bynumber\.json/),
            apiHandler('crm-invoiceJsonData', /crm\/invoice\/jsondata\/[\w\d-]+\.json/),
            apiHandler('crm-invoices', /crm\/invoice\.json/, dlt),
            apiHandler('crm-invoicesAndItems', /crm\/invoice\/status\/[\w\d-]+\.json/),
            apiHandler('crm-invoices', /crm\/invoice\/filter\.json/),
            apiHandler('crm-invoices', /crm\/[contact|person|company|opportunity]+\/invoicelist\/[\w\d-]+\.json/),
            apiHandler('crm-invoiceItem', /crm\/invoiceitem\.json/, post),
            apiHandler('crm-invoiceItems', /crm\/invoiceitem\.json/, dlt),
            apiHandler('crm-invoiceItems', /crm\/invoiceitem\/filter\.json/),
            apiHandler('crm-invoiceTax', /crm\/invoice\/tax\/[\w\d-]+\.json/),
            apiHandler('crm-invoiceTax', /crm\/invoice\/tax\.json/, post),
            apiHandler('crm-invoiceTaxes', /crm\/invoice\/tax\.json/, get),
            apiHandler('crm-invoiceSettings', /crm\/invoice\/settings\.json/),
            apiHandler('crm-invoiceSettings', /crm\/invoice\/settings\/name\.json/),
            apiHandler('crm-invoiceSettings', /crm\/invoice\/settings\/terms\.json/),
            apiHandler('doc-file', /crm\/invoice\/[\w\d-]+\/pdf\.json/),
            apiHandler('crm-converterData', /crm\/invoice\/converter\/data\.json/),            
            apiHandler('crm-currencyRates', /crm\/currency\/rates\.json/, get),
            apiHandler('crm-currencyRate', /crm\/currency\/rates\/[\w\d-]+\.json/),
            apiHandler('crm-currencyRate', /crm\/currency\/rates\/\w{3}\/\w{3}\.json/, get),
            apiHandler('crm-currencyRate', /crm\/currency\/rates\.json/, post),            
            apiHandler('crm-voipNumbers', /crm\/voip\/numbers\/available\.json/),
            apiHandler('crm-voipNumbers', /crm\/voip\/numbers\/existing\.json/),
            apiHandler('crm-voipNumber', /crm\/voip\/numbers\.json/, post),
            apiHandler('crm-voipNumber', /crm\/voip\/numbers\/current\.json/),
            apiHandler('crm-voipCall', /crm\/voip\/call\.json/, post),
            apiHandler('crm-voipCalls', /crm\/voip\/call\.json/, get),
            apiHandler('crm-voipCall', /crm\/voip\/call\/[\w\d-]+\.json/, get),
            apiHandler('crm-voipCalls', /crm\/voip\/call\/missed\.json/, get),
            apiHandler('authentication', /authentication\.json/),
            apiHandler('settings', /settings\.json/),
            apiHandler('security', /settings\/security\.json/),
            apiHandler('access', /settings\/security\/access\.json/),
            apiHandler('administrator', /settings\/security\/administrator\.json/),
            apiHandler('quotas', /settings\/quota\.json/),
            apiHandler('profile', /people\/[\w\d-]+\.json/),
            apiHandler('isme', /people\/@self\.json/),
            apiHandler('profiles', /people\/status\/[\w\d-]+\.json/),
            apiHandler('profiles', /people\/status\/[\w\d-]+\/@search\/.+\.json/),
            apiHandler('profiles', /people\/status\/[\w\d-]+\/search\.json/),
            apiHandler('profiles', /people\.json/),
            apiHandler('profiles', /people\/@search\/.+\.json/),
            apiHandler('profiles', /people\/search\.json/),
            apiHandler('profiles', /people\/filter\.json/),
            apiHandler('profiles', /people\/type\/[\w\d-]+\.json/),
            apiHandler('profiles', /people\/invite\.json/),
            apiHandler('text', /people\/password\.json/),
            apiHandler('text', /people\/thirdparty\/linkaccount\.json/),
            apiHandler('text', /people\/thirdparty\/unlinkaccount\.json/),
            apiHandler('group', /group\/[\w\d-]+\.json/),
            apiHandler('groups', /group\.json/),
            apiHandler('crm-tasks', /crm\/contact\/task\/group\.json/),
            apiHandler('crm-tag', /crm\/[company|person]+\/[\w\d-]+\/tag\/group\.json/),
            apiHandler('crm-voipNumber', /crm\/voip\/numbers\/[\w]+\/settings.json/, put),
            apiHandler('crm-voipSettings', /crm\/voip\/numbers\/settings.json/),
            apiHandler('comment', /comment\.json/, post),
            apiHandler('comments', /comment\.json/, get),

            apiHandler('prj-settings', /project\/settings\.json/),
            apiHandler('prj-report', /project\/report\/files\.json/),
            
            apiHandler('text', /project\/comment\/[\w\d-]+\.json/, dlt),
            apiHandler('text', /community\/wiki\/comment\/[\w\d-]+\.json/, dlt),
            apiHandler('text', /community\/event\/comment\/[\w\d-]+\.json/, dlt),
            apiHandler('text', /community\/bookmark\/comment\/[\w\d-]+\.json/, dlt),
            apiHandler('text', /community\/blog\/comment\/[\w\d-]+\.json/, dlt),
            
            apiHandler('text', /project\/comment\/[\w\d-]+\.json/, put),
            apiHandler('text', /community\/wiki\/comment\/[\w\d-]+\.json/, put),
            apiHandler('text', /community\/event\/comment\/[\w\d-]+\.json/, put),
            apiHandler('text', /community\/bookmark\/comment\/[\w\d-]+\.json/, put),
            apiHandler('text', /community\/blog\/comment\/[\w\d-]+\.json/, put),
            
            apiHandler('text', /project\/message\/discussion\/preview\.json/),
            apiHandler('commentinlist', /project\/message\/preview\.json/),
            apiHandler('commentinlist', /community\/wiki\/comment\/preview\.json/),
            apiHandler('commentinlist', /community\/event\/comment\/preview\.json/),
            apiHandler('commentinlist', /community\/bookmark\/comment\/preview\.json/),
            apiHandler('commentinlist', /community\/blog\/comment\/preview\.json/),
            
            apiHandler('commentinlist', /project\/comment\.json/, post),
            apiHandler('commentinlist', /community\/wiki\/comment\.json/, post),
            apiHandler('commentinlist', /community\/event\/comment\.json/, post),
            apiHandler('commentinlist', /community\/bookmark\/comment\.json/, post),
            apiHandler('commentinlist', /community\/blog\/comment\.json/, post),
            
            
            
            apiHandler('feed-feeds', /feed\/filter\.json/),
            apiHandler('text', /crm\/[contact|opportunity|case|relationshipevent]+\/[\w\d-]+\/files\/hidden\.json/)
        ];

    function apiHandler(handler, re, method) {
        var result = { handler: handler, re: re };
        if (typeof method != "undefined") {
            result.method = method;
        }
        return result;
    }

    function isArray(o) {
        return o ? o.constructor.toString().indexOf("Array") != -1 : false;
    }

    function converText(str, toText) {
        if (toText === true) {
            var symbols = [
                ['&lt;', '<'],
                ['&gt;', '>'],
                ['&and;', '\\^'],
                ['&sim;', '~'],
                ['&amp;', '&']
            ];

            var symInd = symbols.length;
            while (symInd--) {
                str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
            }
            return str;
        }

        var o = document.createElement('textarea');
        o.innerHTML = str;
        return o.value;
    }

    function extend(src, dsc) {
        for (var fld in dsc) {
            if (dsc.hasOwnProperty(fld)) {
                src[fld] = dsc[fld];
            }
        }
        return src;
    }

    ;

    function clone(o) {
        if (!o || typeof o !== 'object') {
            return o;
        }

        var p, v, c = typeof o.pop === 'function' ? [] : {};
        for (p in o) {
            if (o.hasOwnProperty(p)) {
                v = o[p];
                if (v && typeof v === 'object') {
                    c[p] = clone(v);
                } else {
                    c[p] = v;
                }
            }
        }
        return c;
    }

    function collection(items, hCreate, hExtend) {
        var collection = [],
            itemsInd = items ? items.length : 0;

        while (itemsInd--) {
            collection.unshift(extend(hCreate ? hCreate(items[itemsInd]) : {}, hExtend(items[itemsInd])));
        }

        return collection;
    }

    function leftPad(n, m) {
        var p = '000000000';
        n = '' + n;
        if (!m) {
            return n.length === 1 ? '0' + n : n;
        }
        return n.length === m ? n : p.substring(0, m - n.length) + n;
    }

    function getResponse(response) {
        var o = null;
        if (typeof response === 'string') {
            try {
                o = jQuery.parseJSON(converText(response));
            } catch(err) {
                o = null;
            }
            if (!o || typeof o !== 'object') {
                try {
                    o = jQuery.parseJSON(converText(jQuery.base64.decode(response)));
                } catch(err) {
                    o = null;
                }
            }
        }
        if (typeof response === 'object') {
            try {
                o = response.hasOwnProperty('response') ? response.response : response;
            } catch(err) {
                o = null;
            }
        }
        return o;
    }

    function fixUrl(url) {
        if (!url) {
            return '';
        }
        if (url.indexOf('://') === -1) {
            if (url.charAt(0) !== '/') {
                url = '/' + url;
            }
            url = [location.protocol, '//', location.hostname, location.port ? ':' + location.port : '', url].join('');
        }
        return url;
    }

    function getFileType(ext) {
        var exts = null,
            extsInd = 0,
            types = extTypes,
            typesInd = 0;

        typesInd = types.length;
        while (typesInd--) {
            exts = types[typesInd].exts;
            extsInd = exts.length;
            while (extsInd--) {
                if (exts[extsInd] == ext) {
                    return types[typesInd].name;
                }
            }
        }
        return 'unknown';
    }

    function isSupportedFileType(ext) {
        var types = supportedTypes,
            typesInd = 0;

        typesInd = types.length;
        while (typesInd--) {
            if (ext == types[typesInd]) {
                return true;
            }
        }
        return false;
    }

    function getRootFolderTypeById(id) {
        var types = folderTypes,
            typesInd = 0;

        typesInd = types.length;
        while (typesInd--) {
            if (types[typesInd].id == id) {
                return types[typesInd].label;
            }
        }
        return types.length > 0 ? types[0].label : '';
    }


    function getSearchEntityTypeById(id) {
        var types = searchEntityTypes,
            typesInd = 0;

        typesInd = types.length;
        while (typesInd--) {
            if (types[typesInd].id == id) {
                return types[typesInd].label;
            }
        }
        return types.length > 0 ? types[0].label : id;
    }

    function getTaskStatusName(id) {
        var statuses = taskStatuses,
            statusesInd = 0;

        for (statusesInd in statuses) {
            if (statuses.hasOwnProperty(statusesInd)) {
                if (statuses[statusesInd].id == id) {
                    return statuses[statusesInd].name;
                }
            }
        }
        return '';
    }

    function getContacts(items) {
        var contact = null, item = null, strcontacts = { mailboxes: [], telephones: [], links: [] };
        if (!items) {
            return strcontacts;
        }

        for (var i = 0, n = items.length; i < n; i++) {
            item = items[i];
            contact = {
                type: contactTitles.hasOwnProperty(item.type) ? contactTitles[item.type].type : -1,
                name: item.type,
                title: item.value,
                label: contactTitles.hasOwnProperty(item.type) ? contactTitles[item.type].title : item.type,
                istop: false
            };

            switch (contact.name) {
                case contactTitles.twitter.name:
                    contact.val = contact.title.indexOf('twitter.com') === -1 ? 'http://twitter.com/' + contact.title : contact.title;
                    contact.title = contact.title.indexOf('twitter.com') !== -1 ? contact.title.substring(contact.title.lastIndexOf('/')) : contact.title;
                    break;
                case contactTitles.facebook.name:
                    contact.val = contact.title.indexOf('facebook.com') === -1 ? 'http://facebook.com/' + contact.title : contact.title;
                    contact.title = contact.title.indexOf('facebook.com') !== -1 ? contact.title.substring(contact.title.lastIndexOf('/') + 1) : contact.title;
                    break;
                case contactTitles.skype.name:
                    contact.istop = true;
                    contact.val = 'skype:' + contact.title + '?call';
                case contactTitles.jabber.name:
                case contactTitles.msn.name:
                case contactTitles.aim.name:
                    break;
                case contactTitles.icq.name:
                    contact.val = 'http://www.icq.com/people/' + contact.title;
                    break;
                case contactTitles.yahoo.name:
                case contactTitles.gmail.name:
                case contactTitles.gtalk.name:
                    contact.istop = true;
                    contact.val = 'mailto:' + contact.title;
                    break;
                case contactTitles.blogger.name:
                    contact.val = contact.title.indexOf('blogger.com') === -1 ? 'http://' + contact.title + '.blogger.com/' : contact.title;
                    break;
                case contactTitles.myspace.name:
                    contact.val = contact.title.indexOf('myspace.com') === -1 ? 'http://myspace.com/' + contact.title : contact.title;
                    contact.title = contact.title.indexOf('myspace.com') !== -1 ? contact.title.substring(contact.title.lastIndexOf('/') + 1) : contact.title;
                    break;
                case contactTitles.livejournal.name:
                    contact.val = contact.title.indexOf('livejournal.com') === -1 ? 'http://' + contact.title + '.livejournal.com/' : contact.title;
                    break;
                default:
                    contact.val = contact.title;
                    break;
            }

            switch (contact.type) {
                // mails
                case 0:
                    strcontacts.mailboxes.push(contact);
                    break;
                // tels
                case 1:
                    strcontacts.telephones.push(contact);
                    break;
                // links
                case 2:
                    strcontacts.links.push(contact);
                    break;
                // other
                case -1:
                    strcontacts.links.push(contact);
                    break;
            }
        }

        return strcontacts;
    }

    var init = function(opts) {
        if (isInit === true) {
            return undefined;
        }
        isInit = true;

        opts = opts || {};
        if (opts.hasOwnProperty('portaldatetime') && opts.portaldatetime && typeof opts.portaldatetime === 'object') {
            portalUtcOffsetTotalMinutes = opts.portaldatetime.hasOwnProperty('utcoffsettotalminutes') ? opts.portaldatetime.utcoffsettotalminutes : portalUtcOffsetTotalMinutes;
            portalUtcOffset = portalUtcOffsetTotalMinutes != 0 ? (portalUtcOffsetTotalMinutes > 0 ? '+' : '-') + leftPad(Math.floor(Math.abs(portalUtcOffsetTotalMinutes) / 60)) + ':' + leftPad(Math.abs(portalUtcOffsetTotalMinutes) % 60) : portalUtcOffset;
            portalTimeZoneName = opts.portaldatetime.hasOwnProperty('displayname') ? opts.portaldatetime.displayname : portalTimeZoneName;
        }
        if (opts.hasOwnProperty('names') && opts.names && typeof opts.names === 'object') {
            monthNames = opts.names.hasOwnProperty('months') ? typeof opts.names.months === 'object' ? opts.names.months : opts.names.months.split(',') : monthNames;
            monthShortNames = opts.names.hasOwnProperty('shortmonths') ? typeof opts.names.shortmonths === 'object' ? opts.names.shortmonths : opts.names.shortmonths.split(',') : monthShortNames;
            dayNames = opts.names.hasOwnProperty('days') ? typeof opts.names.days === 'object' ? opts.names.days : opts.names.days.split(',') : dayNames;
            dayShortNames = opts.names.hasOwnProperty('shortdays') ? typeof opts.names.shortdays === 'object' ? opts.names.shortdays : opts.names.shortdays.split(',') : dayShortNames;

            nameCollections.days = dayNames;
            nameCollections.shortdays = dayShortNames;
            nameCollections.months = monthNames;
            nameCollections.shortmonths = monthShortNames;
        }

        if (opts.hasOwnProperty('avatars') && opts.avatars && typeof opts.avatars === 'object') {
            defaultAvatar = opts.avatars.hasOwnProperty('large') ? opts.avatars.large : defaultAvatar;
            defaultAvatarMedium = opts.avatars.hasOwnProperty('medium') ? opts.avatars.medium : defaultAvatarMedium;
            defaultAvatarSmall = opts.avatars.hasOwnProperty('small') ? opts.avatars.small : defaultAvatarSmall;
        }

        if (opts.hasOwnProperty('formats') && opts.formats && typeof opts.formats === 'object') {
            formatDatetime = opts.formats.hasOwnProperty('datetime') ? opts.formats.datetime : formatDatetime;
            formatDate = opts.formats.hasOwnProperty('date') ? opts.formats.date : formatDate;
            formatTime = opts.formats.hasOwnProperty('time') ? opts.formats.time : formatTime;
            if (formatTime && formatDate) {
                formatDatetime = formatTime + ' ' + formatDate;
            }

            formats.datetime = formatDatetime;
            formats.date = formatDate;
            formats.time = formatTime;
        }

        if (opts.hasOwnProperty('supportedfiles') && opts.supportedfiles && typeof opts.supportedfiles === 'object') {
            var imgs = opts.supportedfiles.hasOwnProperty('imgs') ? opts.supportedfiles.imgs || [] : [];
            supportedImgs = imgs && typeof imgs === 'string' ? imgs.split('|') : [];
            var docs = opts.supportedfiles.hasOwnProperty('docs') ? opts.supportedfiles.docs || [] : [];
            supportedDocs = docs && typeof docs === 'string' ? docs.split('|') : [];
            supportedTypes = [].concat(supportedImgs, supportedDocs);
        }

        if (opts.hasOwnProperty('responses') && opts.responses && typeof opts.responses === 'object') {
            var response = null,
                responses = opts.responses;
            for (var fld in responses) {
                response = responses[fld];
                if (response && (typeof response === 'object' || typeof response === 'string')) {
                    response = getResponse(response);
                    if (response) {
                        response = response.hasOwnProperty('response') ? response.response : response;
                        create(fld, get, response);
                    }
                }
            }
        }

        if (opts.hasOwnProperty('contacttitles') && opts.contacttitles && typeof opts.contacttitles === 'object') {
            var contacttitles = opts.contacttitles;
            for (var fld in contacttitles) {
                if (contacttitles.hasOwnProperty(fld)) {
                    if (contactTitles.hasOwnProperty(fld)) {
                        contactTitles[fld].title = contacttitles[fld];
                    }
                }
            }

        }
    };

    var fixData = function(data) {
        if (!data || typeof data !== 'object') {
            return data;
        }

        var value = null;
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                value = data[fld];
                switch (typeof value) {
                    case 'string':
                        switch (fld.toLowerCase()) {
                            case 'infotype':
                                if (contactTypes.hasOwnProperty(value)) {
                                    if (data.hasOwnProperty('category') && typeof data.category === 'string' && contactTypes[value].categories.hasOwnProperty(data.category)) {
                                        data.category = contactTypes[value].categories[data.category].id;
                                    }
                                    value = contactTypes[value].id;
                                }
                                break;
                        }
                        break;
                }
                data[fld] = value;
            }
        }

        return data;
    };

    var serializeDate = (function() {
        if (false && new Date(Date.parse('1970-01-01T00:00:00.000Z')).getTime() === new Date(Date.parse('1970-01-01T00:00:00.000Z')).getTime()) {
            return function(d, toLocalTime) {
                if (!d) {
                    return null;
                }
                var date = null, timestamp = d && typeof d === 'object' && d.hasOwnProperty('utc') && d.utc || d;
                var offset = d && typeof d === 'object' && d.hasOwnProperty('offset') && isFinite(+d.offset) && +d.offset || 0;
                if (typeof timestamp !== 'string') {
                    return null;
                }

                date = new Date(Date.parse(timestamp));

                if (toLocalTime !== true && date instanceof Date) {
                    date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
                }

                return date instanceof Date ? date : null;
            };
        }
        return function(d, toUTC) {
            if (!d) {
                return null;
            }
            var date = null, timestamp = d && typeof d === 'object' && d.hasOwnProperty('utc') && d.utc || d,
                offset = d && typeof d === 'object' && d.hasOwnProperty('offset') && isFinite(+d.offset) && +d.offset || 0;

            if (typeof timestamp !== 'string') {
                return null;
            }

            offset = 0;
            if (timestamp.indexOf('Z') === -1) {
                offset = timestamp.substring(timestamp.length - 5).split(':');
                offset = (+offset[0] * 60 + +offset[1]) * (timestamp.charAt(timestamp.length - 6, 1) === '+' ? 1 : -1);
            }
            date = timestamp.split('.')[0].split('T');
            date[0] = date[0].split('-');
            date[1] = date[1].split(':');

            if (typeof(toUTC) !== "undefined" && toUTC !== true) {
                date = new Date(date[0][0], date[0][1] - 1, date[0][2], date[1][0], date[1][1], date[1][2], 0);
            } else {
                //Fix Bug 27729
                date = new Date(
                    Date.UTC(
                        +date[0][0],
                        +date[0][1] - 1,
                        +date[0][2],
                        +date[1][0],
                        +date[1][1],
                        +date[1][2],
                        0
                    )
                );
                date.setMinutes(date.getMinutes() + date.getTimezoneOffset());
            }

            if (toUTC !== true && date instanceof Date) {
                //date.setMinutes(date.getMinutes() + new Date().getTimezoneOffset());
                //date.setMinutes(date.getMinutes() + portalUtcOffsetTotalMinutes);
                //date.setMinutes(date.getMinutes() + offset);
            }

            return date instanceof Date ? date : null;
        };
    })();

    var serializeTimestamp = function(d, safeurl) {
        var timestamp = d instanceof Date ? '' + [d.getFullYear(), leftPad((d.getMonth() + 1)), leftPad(d.getDate())].join('-') + 'T' + [leftPad(d.getHours()), leftPad(d.getMinutes()), leftPad(d.getSeconds())].join(':') : '';
        return safeurl === true ? timestamp.replace(/:/g, '-') : timestamp;

        //var timestamp = d instanceof Date ? '' + [d.getFullYear(), leftPad((d.getMonth() + 1)), leftPad(d.getDate())].join('-') + 'T' + [leftPad(d.getHours()), leftPad(d.getMinutes()), leftPad(d.getSeconds())].join(':') + '.' + leftPad(d.getMilliseconds(), 7) + portalUtcOffset : '';
        ////return safeurl === true ? timestamp.replace(/:/g, '-') : timestamp;
        //return timestamp;

        //return d instanceof Date ? '' + [d.getFullYear(), leftPad((d.getUTCMonth() + 1)), leftPad(d.getUTCDate())].join('-') + 'T' + [leftPad(d.getUTCHours()), leftPad(d.getUTCMinutes()), leftPad(d.getUTCSeconds())].join(':') + '.' + leftPad(d.getMilliseconds(), 7) + 'Z' : '';
        //return JSON.stringify(d);
    };

    function formatingDateTerm(date, term, dayshortnames, daynames, monthshortnames, monthnames) {
        switch (term) {
            case 'd':
                return date.getDate();
            case 'dd':
                return leftPad(date.getDate());
            case 'ddd':
                return dayshortnames[date.getDay()];
            case 'dddd':
                return daynames[date.getDay()];
            case 'h':
                var hours = date.getHours();
                return hours > 12 ? hours - 12 : hours === 0 ? 12 : hours;
            case 'hh':
                var hours = date.getHours();
                return leftPad(hours > 12 ? hours - 12 : hours === 0 ? 12 : hours);
                break;
            case 'H':
                return date.getHours();
            case 'HH':
                return leftPad(date.getHours());
            case 'm':
                return date.getMinutes();
            case 'mm':
                return leftPad(date.getMinutes());
            case 'M':
                return date.getMonth() + 1;
            case 'MM':
                return leftPad(date.getMonth() + 1);
            case 'MMM':
                return monthshortnames[date.getMonth()];
            case 'MMMM':
                return monthnames[date.getMonth()];
            case 's':
                return date.getSeconds();
            case 'ss':
                return leftPad(date.getSeconds());
            case 't':
                return date.getHours() < 12 ? 'A' : 'P';
            case 'tt':
                return date.getHours() < 12 ? 'AM' : 'PM';
            case 'y':
                return date.getYear() - 100;
            case 'yy':
                return (date.getYear() % 100 < 10 ? '0' : '') + date.getYear() % 100;
            case 'yyy':
                return (date.getYear() % 100 < 10 ? '0' : '') + date.getYear() % 100;
            case 'yyyy':
                return date.getFullYear();
        }
        return '';
    }

    var formattingDate = function(date, format, dayshortnames, daynames, monthshortnames, monthnames) {
        if (!(date instanceof Date) || date.getTime() < 0) {
            return '';
        }
        if (typeof format !== 'string' || format.length === 0) {
            return '';
        }

        var output = '',
            term = '',
            islit = false;
        format = format.split('');
        for (var i = 0, n = format.length; i < n; i++) {
            islit = false;
            switch (format[i]) {
                case 'd':
                case 'h':
                case 'H':
                case 'm':
                case 'M':
                case 's':
                case 't':
                case 'y':
                    term += format[i];
                    break;
                default:
                    islit = true;
            }
            if (islit) {
                output += formatingDateTerm(date, term, dayshortnames, daynames, monthshortnames, monthnames);
                output += format[i];
                term = '';
            }
        }
        if (term) {
            output += formatingDateTerm(date, term, dayshortnames, daynames, monthshortnames, monthnames);
        }
        return output;
    };

    var getDisplayTime = function(date) {
        if (date && formatTime) {
            return formattingDate(date, formatTime, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return date ? date.toLocaleTimeString() : '';
    };

    var getDisplayDate = function(date) {
        if (date && formatDate) {
            return formattingDate(date, formatDate, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return date ? date.toLocaleDateString() : '';
    };

    var getDisplayDatetime = function(date) {
        if (date && formatDatetime) {
            return formattingDate(date, formatDatetime, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return date ? date.toLocaleTimeString() + ' ' + date.toLocaleDateString() : '';
    };

    function createGroup(o) {
        if (!o) {
            return null;
        }

        var group = {
            type: 'group',
            id: o.id,
            name: o.name,
            manager: o.manager || '',
            title: o.title || o.name,
            members: o.hasOwnProperty('members') ? createPersons(o.members) : null
        };

        return group;
    }

    function createGroups(o) {
        if (!o) {
            return [];
        }

        var group = null,
            groups = [],
            items = isArray(o) ? o : [o],
            itemsInd = 0;

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            group = createGroup(items[itemsInd]);
            group ? groups.unshift(group) : null;
        }
        return groups;
    }

    function createPerson(o) {
        if (!o) {
            return null;
        }
        if (o.hasOwnProperty('email') && o.hasOwnProperty('contacts')) {
            var email = { type: 'mail', value: o.email };
            o.contacts = o.contacts == null ? [email] : [email].concat(o.contacts);
        }

        var person = null,
            crtdate = serializeDate(o.created || o.workFrom || o.workFromDate),
            trtdate = serializeDate(o.terminated || o.terminatedDate),
            bthdate = serializeDate(o.birthday),
            displayname = o.displayName || o.firstName + ' ' + o.lastName,
            contacts = getContacts(o.contacts);

        person = {
            index: displayname.charAt(0).toLowerCase(),
            type: 'person',
            id: o.id,
            timestamp: crtdate ? crtdate.getTime() : 0,
            crtdate: crtdate,
            displayCrtdate: getDisplayDatetime(crtdate),
            displayDateCrtdate: getDisplayDate(crtdate),
            displayTimeCrtdate: getDisplayTime(crtdate),
            trtdate: trtdate,
            displayTrtdate: getDisplayDatetime(trtdate),
            displayDateTrtdate: getDisplayDate(trtdate),
            displayTimeTrtdate: getDisplayTime(trtdate),
            birthday: bthdate,
            birthdayApiString: o.birthday || '',
            userName: o.userName || '',
            firstName: o.firstName || '',
            lastName: o.lastName || '',
            displayName: displayname || '',
            email: o.email || '',
            tel: contacts.telephones.length > 0 ? contacts.telephones[0].val : '',
            contacts: contacts,
            avatar: o.avatar || o.avatarSmall || defaultAvatar,
            avatarBig: o.avatarBig,
            avatarSmall: o.avatarSmall || defaultAvatarSmall,
            groups: createGroups(o.groups || []),
            status: o.status || 0,
            activationStatus: o.activationStatus || 0,
            isActivated: activationStatuses.activated.id === (o.activationStatus || 0),
            isPending: activationStatuses.pending.id === (o.activationStatus || 0),
            isTerminated: typeof (o.isTerminated) !== "undefined" ? o.isTerminated : profileStatuses.terminated.id === (o.status || 0),
            isMe: myProfile ? myProfile.id === o.id : false,
            isManager: false,
            isPortalOwner: typeof(o.isOwner) != "undefined" ? o.isOwner : null,
            isAdmin: typeof(o.isAdmin) != "undefined" ? o.isAdmin : null,
            listAdminModules: typeof(o.listAdminModules) != "undefined" ? o.listAdminModules : [],
            isVisitor: typeof (o.isVisitor) != "undefined" ? o.isVisitor : null,
            isOutsider: typeof (o.isOutsider) != "undefined" ? o.isOutsider : null,
            sex: o.sex || '',
            location: o.location || '',
            title: o.title || '',
            notes: o.notes || '',
            culture: o.cultureName || '',
            profileUrl: o.profileUrl || '',
            isLDAP: typeof (o.isLDAP) != "undefined" ? o.isLDAP : typeof (o.isldap) != "undefined" ? o.isldap : false,
            isSSO: typeof (o.isSSO) != "undefined" ? o.isSSO : typeof (o.issso) != "undefined" ? o.issso : false
        };

        return person;
    }

    function createPersons(o) {
        if (!o) {
            return [];
        }

        var person = null,
            persons = [],
            items = isArray(o) ? o : [o],
            itemsInd = 0;

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            person = createPerson(items[itemsInd]);
            person ? persons.unshift(person) : null;
        }
        return persons;
    }

    var sortCommentsByTree = function(comments) {
        if (comments.length === 0) {
            return comments;
        }
        var tree = clone(comments), commentParentId = null, commentsInd = 0, ind = 0;

        commentsInd = tree ? tree.length : 0;
        while (commentsInd--) {
            commentParentId = tree[commentsInd].parentId;
            if (!tree[commentsInd].comments) {
                tree[commentsInd].comments = [];
            }
            if (commentParentId === null) {
                continue;
            }

            ind = tree.length;
            while (ind--) {
                if (tree[ind].id == commentParentId) {
                    if (!tree[ind].comments) {
                        tree[ind].comments = [];
                    }
                    tree[ind].comments.unshift(tree[commentsInd]);
                    break;
                }
            }
        }

        commentsInd = tree ? tree.length : 0;
        while (commentsInd--) {
            if (tree[commentsInd].parentId !== null) {
                tree.splice(commentsInd, 1);
            }
        }

        return tree ? tree : comments;
    };

    var createComment = function(o) {
        var crtdate = serializeDate(o.created);
        uptdate = serializeDate(o.updated);
        createdBy = createPerson(o.createdBy);

        var result = {
            type: 'comment',
            id: o.id,
            inactive: o.hasOwnProperty('inactive') ? o.inactive : false,
            parentId: o.parentId === '00000000-0000-0000-0000-000000000000' ? null : o.parentId || null,
            timestamp: crtdate ? crtdate.getTime() : 0,
            crtdate: crtdate,
            displayCrtdate: getDisplayDatetime(crtdate),
            displayDateCrtdate: getDisplayDate(crtdate),
            displayTimeCrtdate: getDisplayTime(crtdate),
            displayDatetimeCrtdate: getDisplayDatetime(crtdate),
            uptdate: uptdate,
            displayUptdate: getDisplayDatetime(uptdate),
            displayDateUptdate: getDisplayDate(uptdate),
            displayTimeUptdate: getDisplayTime(uptdate),
            displayDatetimeUptdate: getDisplayDatetime(uptdate),
            createdBy: createdBy,
            updatedBy: createPerson(o.updatedBy || o.createdBy),
            isMine: myProfile && createdBy && myProfile.id == createdBy.id || false,
            comments: [],
            text: o.text || ''
        }

        if (o.hasOwnProperty('canEdit')) {
            result.canEdit = o.canEdit;
        }

        return result;
    };
    
    

    var createCommentsTree = function(o) {
        var comments = [],
            items = isArray(o) ? o : [o],
            itemsInd = 0;

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            comments.unshift(createComment(items[itemsInd]));
        }

        return sortCommentsByTree(comments);
    };

    var createPoll = function(o) {
        if (!o) {
            return null;
        }

        var max = 0, all = 0, val = 0,
            votes = [],
            items = o.votes,
            itemsInd = 0;

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            val = items[itemsInd].votes;
            all += val
            if (max < val) {
                max = val;
            }
        }

        itemsInd = items ? items.length : 0;
        while (itemsInd--) {
            val = items[itemsInd].votes;
            votes.unshift({
                title: items[itemsInd].name,
                count: val,
                percent: all !== 0 ? Math.round((val * 100) / all) : 0,
                leader: max === val
            });
        }

        return { votes: votes, voted: o.voted, fullCount: all };
    };

    var factories = {
        storageusage: function(response) {
            return response;
        },

        text: function(response) {
            return response;
        },

        authentication: function(response) {
            return {
                token: response.token
            };
        },

        isme: function(response) {
            var profile = createPerson(response);
            for (var fld in profile) {
                if (profile.hasOwnProperty(fld)) {
                    myProfile[fld] = profile[fld];
                }
            }
            myProfile.isMe = true;

            return myProfile;
        },

        profile: function(response) {
            return createPerson(response);
        },

        profiles: function(response) {
            return collection(response, createPerson, function(response) {
                return {};
            });
        },

        group: function(response) {
            return createGroup(response);
        },

        groups: function(response) {
            return collection(response, createGroup, function(response) {
                return {};
            });
        },

        settings: function(response) {
            portalSettings = {
                type: 'settings',
                culture: response.culture,
                timezone: response.timezone,
                trustedDomains: response.trustedDomains,
                trustedDomainsType: response.trustedDomainsType,
                utcHoursOffset: response.utcHoursOffset,
                utcOffset: response.utcOffset
            };

            return portalSettings;
        },

        quotas: function(response) {
            portalQuotas = {
                type: 'quotas',
                storageSize: response.storageSize,
                maxFileSize: response.maxFileSize,
                usedSize: response.usedSize,
                availableSize: response.availableSize,
                storageUsage: factories.storageusage(response.storageUsage),
                maxUsersCount: response.maxUsersCount,
                usersCount: response.usersCount,
                availableUsersCount: response.availableUsersCount,
                userStorageSize: response.userStorageSize,
                userUsedSize: response.userUsedSize,
                userAvailableSize: response.userAvailableSize
            };

            return portalQuotas;
        },

        comment: function(response) {
            return createComment(response);
        },
        
        comments: function(response) {
            return createCommentsTree(response);
        },
        
        commentinlist: function(response){
            return response;
        },

        searchentryitems: function(response) {
            return collection(response, null, function(response) {
                var type = getSearchEntityTypeById(response.entityType);
                switch (type) {
                    case 'file':
                        return factories.doc.file(response);
                    case 'task':
                    case 'subtask':
                        return factories.prj.subtask(response);
                    case 'discussion':
                        return factories.prj.discussion(response);
                    case 'milestone':
                        return factories.prj.milestone(response);
                    default:
                        return {
                            type: type
                        };
                }
            });
        }
    };

    /* community */
    factories.cmt = {
        item: function(response) {
            var crtdate = serializeDate(response.created),
                uptdate = serializeDate(response.updated);

            return {
                id: response.id,
                timestamp: crtdate ? crtdate.getTime() : 0,
                crtdate: crtdate,
                displayCrtdate: getDisplayDatetime(crtdate),
                displayDateCrtdate: getDisplayDate(crtdate),
                displayTimeCrtdate: getDisplayTime(crtdate),
                displayDatetimeCrtdate: getDisplayDatetime(crtdate),
                uptdate: uptdate,
                displayUptdate: getDisplayDatetime(uptdate),
                displayDateUptdate: getDisplayDate(uptdate),
                displayTimeUptdate: getDisplayTime(uptdate),
                displayDatetimeUptdate: getDisplayDatetime(uptdate),
                createdBy: createPerson(response.createdBy || response.author),
                updatedBy: createPerson(response.updatedBy || response.createdBy || response.author),
                title: response.title || '',
                text: response.preview || response.text || response.description || ''
            };
        },

        tags: function(response) {
            return response || [];
        },

        blog: function(response) {
            return extend(this.item(response), {
                type: 'blog',
                tags: factories.cmt.tags(response.tags),
                comments: null
            });
        },

        poll: function(response) {
            return extend(this.item(response), {
                type: 'poll'
                //blah-blah-blah
            });
        },

        topic: function(response) {
            var firstPost = null;
            if (response.posts && response.posts.length > 0) {
                firstPost = response.posts[0];
                response.createdBy = firstPost.createdBy;
                response.text = firstPost.text;
                response.posts = response.posts.slice(1);
            }

            return extend(this.item(response), {
                type: 'forum',
                typeCode: response.type,
                statusCode: response.status,
                tags: factories.cmt.tags(response.tags),
                threadTitle: response.threadTitle || response.threadTitile,
                posts: createCommentsTree(response.posts || [])
            });
        },

        thread: function(response) {
            return extend(this.item(response), {
                type: 'thread'
            });
        },

        category: function(response) {
            return extend(this.item(response), {
                type: 'category',
                threads: factories.cmt.threads(response.threads)
            });
        },

        event: function(response) {
            var item = this.item(response);

            return extend(item, {
                type: 'event',
                typeCode: response.type,
                poll: createPoll(response.poll),
                comments: null
            });
        },

        bookmark: function(response) {
            return extend(this.item(response), {
                type: 'bookmark',
                url: response.url,
                thumbnail: response.thumbnail,
                comments: null
            });
        },

        blogs: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.blog(response);
            });
        },

        polls: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.poll(response);
            });
        },

        topics: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.topic(response);
            });
        },

        threads: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.thread(response);
            });
        },

        categories: function(response) {
            response = response.hasOwnProperty('categories') ? response.categories : response;

            return collection(response, this.item, function(response) {
                return factories.cmt.category(response);
            });
        },

        events: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.event(response);
            });
        },

        bookmarks: function(response) {
            return collection(response, this.item, function(response) {
                return factories.cmt.bookmark(response);
            });
        }
    };

    /* projects */
    factories.prj = {
        item: function (response) {
            var createdBy, responsible, updatedBy, responsibles  = [];

            if (typeof response.createdBy === "object") {
                createdBy = createPerson(response.createdBy);
            } else if (typeof response.createdById === "string") {
                createdBy = UserManager.getPerson(response.createdById, createPerson);
            } else {
                createdBy = createPerson(response.author);
            }

            if (typeof response.updatedBy === "object") {
                updatedBy = createPerson(response.updatedBy);
            } else if (typeof response.updatedById === "string") {
                updatedBy = UserManager.getPerson(response.updatedById, createPerson);
            }

            if (typeof response.responsible === "object") {
                responsible = createPerson(response.responsible);
            } else if (typeof response.responsibleId === "string") {
                responsible = UserManager.getPerson(response.responsibleId, createPerson);
            }

            if (response.responsibles) {
                responsibles = createPersons(response.responsibles || response.responsible);
            } else if (response.responsibleIds) {
                responsibles = response.responsibleIds.map(function(item) {
                    return createPerson(UserManager.getUser(item) || UserManager.getRemovedProfile());
                });
            }

            var
                crtdate = serializeDate(response.created),
                uptdate = serializeDate(response.updated);

            return {
                id: response.id,
                timestamp: crtdate ? crtdate.getTime() : 0,
                crtdate: crtdate,
                displayDateCrtdate: getDisplayDate(crtdate),
                uptdate: uptdate,
                displayUptdate: getDisplayDatetime(uptdate),
                displayDateUptdate: getDisplayDate(uptdate),
                createdBy: createdBy,
                updatedBy: updatedBy,
                responsible: responsible || responsibles[0],
                responsibles: responsibles,
                canEdit: response.canEdit || false,
                isPrivate: response.isPrivate || false,
                isShared: response.isShared || false,
                isMy: createdBy && myProfile ? createdBy.id === myProfile.id : false,
                forMe: responsible && myProfile ? responsible.id === myProfile.id : false,
                title: response.title || '',
                lowTitle: (response.title || '').toLowerCase(),
                description: response.description || '',
                status: response.status
            };
        },

        task: function(response) {
            var dlndate = serializeDate(response.deadline, false),
                startdate = serializeDate(response.startDate, false),
                todaydate = new Date(),
                year = todaydate.getFullYear(),
                month = todaydate.getMonth(),
                day = todaydate.getDate();

            todaydate = new Date(year, month, day, 0, 0, 0, 0);
            var tomorrowdate = new Date(year, month, day + 1, 0, 0, 0, 0);
            var hasProjectOwner = response.hasOwnProperty('projectOwner');

            return extend(this.item(response), {
                type: 'task',
                projectId: hasProjectOwner ? response.projectOwner.id : -1,
                projectTitle: hasProjectOwner ? response.projectOwner.title : '',
                projectOwner: response.projectOwner,
                canCreateSubtask: response.canCreateSubtask,
                canCreateTimeSpend: response.canCreateTimeSpend,
                canReadFiles: response.canReadFiles,
                canEditFiles: response.canEditFiles,
                canDelete: response.canDelete,
                deadline: dlndate,
                displayDeadline: getDisplayDatetime(dlndate),
                displayDateDeadline: getDisplayDate(dlndate),
                displayTimeDeadline: getDisplayTime(dlndate),
                startDate: startdate,
                displayStartDate: getDisplayDatetime(startdate),
                displayDateStart: getDisplayDate(startdate),
                displayTimeStart: getDisplayTime(startdate),
                status: response.status > 2 ? 1 : response.status,
                statusname: getTaskStatusName(response.status > 2 ? 1 : response.status),
                priority: response.priority,
                subtasks: factories.prj.subtasks(response.subtasks),
                progress: response.hasOwnProperty('progress') ? response.progress : 0,
                milestoneId: response.milestoneId,
                milestoneTitle: response.hasOwnProperty('milestone') && response.milestone ? response.milestone.title : '',
                milestone: response.hasOwnProperty('milestone') ? factories.prj.milestone(response.milestone) : null,
                deadlineToday: dlndate ? dlndate.getTime() >= todaydate.getTime() && dlndate.getTime() < tomorrowdate.getTime() : false,
                isOpened: response.status == taskStatuses.open.id,
                isExpired: response.isExpired || false,
                links: response.hasOwnProperty('links') ? factories.prj.links(response.links) : [],
                files: response.files,
                commentsCount: response.commentsCount,
                isSubscribed: response.isSubscribed,
                project: response.project,
                timeSpend: response.timeSpend,
                comments: response.comments ? response.comments : [],
                canCreateComment: response.canCreateComment,
                customTaskStatus: response.customTaskStatus
            });
        },

        simpletask: function(response) {
            var dlndate = serializeDate(response.deadline, false),
                startdate = serializeDate(response.startDate, false),
                todaydate = new Date(),
                year = todaydate.getFullYear(),
                month = todaydate.getMonth(),
                day = todaydate.getDate();

            todaydate = new Date(year, month, day, 0, 0, 0, 0);
            var tomorrowdate = new Date(year, month, day + 1, 0, 0, 0, 0);

            return extend(this.item(response), {
                type: 'task',
                projectOwner: response.projectOwner,
                canCreateSubtask: response.canCreateSubtask,
                canCreateTimeSpend: response.canCreateTimeSpend,
                canDelete: response.canDelete,
                createdBy: response.createdBy,
                updatedBy: response.updatedBy,
                deadline: dlndate,
                displayDeadline: getDisplayDatetime(dlndate),
                displayDateDeadline: getDisplayDate(dlndate),
                startDate: startdate,
                displayStartDate: getDisplayDatetime(startdate),
                displayDateStart: getDisplayDate(startdate),
                responsibles: response.responsibles,
                status: response.status > 2 ? 1 : response.status,
                customTaskStatus: response.customTaskStatus,
                statusname: getTaskStatusName(response.status > 2 ? 1 : response.status),
                priority: response.priority,
                subtasksCount: response.subtasksCount,
                progress: response.hasOwnProperty('progress') ? response.progress : 0,
                milestoneId: response.milestoneId,
                deadlineToday: dlndate ? dlndate.getTime() >= todaydate.getTime() && dlndate.getTime() < tomorrowdate.getTime() : false,
                isOpened: response.status == taskStatuses.open.id,
                isExpired: response.isExpired || false,
                links: response.hasOwnProperty('links') ? factories.prj.links(response.links) : null
            });
        },

        milestone: function(response) {
            var dlndate = serializeDate(response.deadline, false),
                todaydate = new Date(),
                year = todaydate.getFullYear(),
                month = todaydate.getMonth(),
                day = todaydate.getDate();

            todaydate = new Date(year, month, day, 0, 0, 0, 0);
            var tomorrowdate = new Date(year, month, day + 1, 0, 0, 0, 0);

            return extend(this.item(response), {
                type: 'milestone',
                projectId: response.hasOwnProperty('projectOwner') ? response.projectOwner.id : -1,
                projectTitle: response.hasOwnProperty('projectOwner') ? response.projectOwner.title : '',
                deadline: dlndate,
                displayDeadline: getDisplayDatetime(dlndate),
                displayDateDeadline: getDisplayDate(dlndate),
                displayTimeDeadline: getDisplayTime(dlndate),
                status: response.status,
                deadlineToday: dlndate ? dlndate.getTime() >= todaydate.getTime() && dlndate.getTime() < tomorrowdate.getTime() : false,
                isKey: response.isKey,
                isNotify: response.isNotify,
                isExpired: response.isExpired || false,
                activeTaskCount: response.activeTaskCount,
                closedTaskCount: response.closedTaskCount,
                canDelete: response.canDelete
            });
        },

        discussion: function (response) {
            var crtdate = serializeDate(response.created);
            return extend(this.item(response), {
                type: 'discussion',
                projectId: response.hasOwnProperty('projectOwner') ? response.projectOwner.id : -1,
                projectTitle: response.hasOwnProperty('projectOwner') ? response.projectOwner.title : '',
                projectOwner: response.projectOwner,
                parentId: null,
                comments: response.comments ? response.comments : [],
                text: response.text || '',
                status: response.status,
                displayTimeCrtdate: getDisplayTime(crtdate),
                displayDateTimeCrtdate: getDisplayDatetime(crtdate),
                canReadFiles: response.canReadFiles,
                canEditFiles: response.canEditFiles,
                canCreateComment: response.canCreateComment,
                commentsCount: response.commentsCount,
                subscribers: response.subscribers,
                files: response.files,
                project: response.project
            });
        },

        project: function (response) {
            var createdBy;
            var responsible;

            if (typeof response.createdBy === "object") {
                createdBy = createPerson(response.createdBy);
            } else if (typeof response.createdById === "string") {
                createdBy = UserManager.getPerson(response.createdById, createPerson);
            }

            if (typeof response.responsible === "object") {
                responsible = createPerson(response.responsible);
            } else if (typeof response.responsibleId === "string") {
                responsible = UserManager.getPerson(response.responsibleId, createPerson);
            } else if (typeof response.responsible === "string") {
                responsible = UserManager.getPerson(response.responsible, createPerson);
            }

            return {
                id: response.id,
                title: response.title || '',
                description: response.description || '',
                status: response.status,

                canCreateMessage: response.hasOwnProperty('security') ? response.security.canCreateMessage : '',
                canCreateMilestone: response.hasOwnProperty('security') ? response.security.canCreateMilestone : '',
                canCreateTask: response.hasOwnProperty('security') ? response.security.canCreateTask : '',
                canCreateTimeSpend: response.hasOwnProperty('security') ? response.security.canCreateTimeSpend : '',
                canEditTeam: response.hasOwnProperty('security') ? response.security.canEditTeam : '',
                security: response.security,

                isPrivate: response.isPrivate || false,
                isInTeam: response.hasOwnProperty('security') ? response.security.isInTeam : '',
                canLinkContact: response.hasOwnProperty('security') ? response.security.canLinkContact : null,

                displayDateCrtdate: getDisplayDate(serializeDate(response.created)),
                taskCount: response.taskCount,
                taskCountTotal: response.taskCountTotal,
                milestoneCount: response.milestoneCount,
                participantCount: response.participantCount,
                discussionCount: response.discussionCount,
                documentsCount: response.documentsCount,
                responsibleId: responsible != null && responsible.id ? responsible.id : responsible,
                canEdit: response.canEdit || false,
                canDelete: response.canDelete,

                createdBy: createdBy,
                responsible: responsible,
                timeTrackingTotal: response.timeTrackingTotal,
                isFollow: response.isFollow,
                tags: response.hasOwnProperty('tags') ? response.tags : []
            };
        },

        link: function(response) {
            return {
                type: 'link',
                parentTaskId: response.parentTaskId,
                dependenceTaskId: response.dependenceTaskId,
                linkType: response.linkType
            };
        },

        projectrequest: function(response) {
            return extend(this.item(response), {
                type: 'request'
            });
        },

        activity: function(response) {
            var date = new Date(response.date);
            return extend(this.item(response), {
                type: 'activity',
                projectId: response.projectId,
                projectTitle: response.projectTitle,
                title: response.title,
                url: response.url,
                actionText: response.actionText,
                displayDatetime: getDisplayDatetime(date),
                displayDate: getDisplayDate(date),
                displayTime: getDisplayTime(date),
                user: response.user,
                entityType: response.entityType,
                entityTitle: response.entityTitle
            });
        },

        subtask: function(response) {
            return extend(this.item(response), {
                type: 'subtask',
                status: response.status,
                taskid: response.taskId
            });
        },

        subtasks: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.subtask(response);
            });
        },

        tasks: function (response) {
            return collection(response, this.item, function(response) {
                return factories.prj.task(response);
            });
        },

        simpletasks: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.simpletask(response);
            });
        },

        milestones: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.milestone(response);
            });
        },

        discussions: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.discussion(response);
            });
        },

        projects: function (response) {
            var func = function(prj) {
                return factories.prj.project(prj);
            };
            return response.map(func);
        },

        links: function(response) {
            return collection(response, null, function(response) {
                return factories.prj.link(response);
            });
        },

        projectperson: function(response) {
            var person = createPerson(response);
            person.canReadFiles = response.canReadFiles;
            person.canReadMessages = response.canReadMessages;
            person.canReadMilestones = response.canReadMilestones;
            person.canReadTasks = response.canReadTasks;
            person.canReadContacts = response.canReadContacts;
            person.isAdministrator = response.isAdministrator;
            person.isManager = response.isManager;
            person.department = response.department || "";
            person.isRemovedFromTeam = response.isRemovedFromTeam;
            return person;
        },

        projectpersons: function(response) {
            var persons = [];
            for (var i = 0; i < response.length; i++) {
                persons.push(factories.prj.projectperson(response[i]));
            }
            return persons;
        },

        searchentries: function(response) {
            return collection(response, this.item, function(response) {
                var projectOwner = response.projectOwner;
                return {
                    id: projectOwner ? projectOwner.id : response.id || -1,
                    type: 'project',
                    title: projectOwner ? projectOwner.title : response.title || '',
                    status: projectOwner ? projectOwner.status : response.status || '',
                    items: response.items ? factories.searchentryitems(response.items) : []
                };
            });
        },

        timespend: function(response) {
            var creationdate = serializeDate(response.date);
            var statusChangedDate = serializeDate(response.statusChanged);
            return extend(this.item(response), {
                type: 'timespend',
                date: creationdate,
                displayCreationDate: getDisplayDatetime(creationdate),
                displayDateCreation: getDisplayDate(creationdate),
                displayTimeCreation: getDisplayTime(creationdate),
                hours: response.hours,
                note: response.note,
                status: response.paymentStatus,
                paymentStatus: response.paymentStatus,
                statusChanged: getDisplayDate(statusChangedDate),
                canEditPaymentStatus: response.canEditPaymentStatus,
                relatedProject: response.relatedProject,
                relatedTask: response.relatedTask,
                relatedTaskTitle: response.relatedTaskTitle,
                task: factories.prj.task(response.task),
                person: response.hasOwnProperty('person') ? createPerson(response.person) : ""
            });
        },

        timespends: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.timespend(response);
            });
        },
        activities: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.activity(response);
            });
        },
        settings: function(response) {
            return response;
        },

        report: function(response) {
            return response;
        }
    };

    /* feed */
    factories.feed = {
        item: function(response) {
            var createdDate = serializeDate(response.CreatedDate);
            var modifiedDate = serializeDate(response.ModifiedDate);
            var aggregatedDate = serializeDate(response.AggregatedDate);

            var feed = {
                item: response.Item,
                itemId: response.ItemId,
                itemUrl: response.ItemUrl,
                id: response.Id,
                authorId: response.AuthorId,
                createdDate: createdDate,
                displayCreatedDatetime: getDisplayDatetime(createdDate),
                displayCreatedDate: getDisplayDate(createdDate),
                displayCreatedTime: getDisplayTime(createdDate),
                modifiedDate: modifiedDate,
                displayModifiedDatetime: getDisplayDatetime(modifiedDate),
                displayModifiedDate: getDisplayDate(modifiedDate),
                displayModifiedTime: getDisplayTime(modifiedDate),
                aggregatedDate: aggregatedDate,
                product: response.Product,
                module: response.Module,
                action: response.Action,
                title: response.Title,
                isToday: response.IsToday,
                isYesterday: response.IsYesterday,
                description: response.Description,
                extraLocation: response.ExtraLocation,
                extraLocationUrl: response.ExtraLocationUrl,
                additionalInfo: response.AdditionalInfo,
                additionalInfo2: response.AdditionalInfo2,
                additionalInfo3: response.AdditionalInfo3,
                additionalInfo4: response.AdditionalInfo4,
                hasPreview: response.HasPreview,
                canComment: response.CanComment,
                commentApiUrl: response.CommentApiUrl,
                groupedFeeds: response.GroupedFeeds,
                comments: getComments(response.Comments)
            };

            function getComments(comments) {
                if (!comments) {
                    return null;
                }

                var templates = [];
                for (var i = 0; i < comments.length; i++) {
                    templates.push(getComment(comments[i]));
                }
                return templates;
            }

            function getComment(comment) {
                var commentDateStr = comment.Date.slice(0, -1);
                var commentDate = serializeDate(commentDateStr);

                commentDate.setMinutes(commentDate.getMinutes() + ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset);

                return {
                    id: comment.Id,
                    description: comment.Description,
                    authorId: comment.AuthorId,
                    date: commentDate,
                    formattedDate: getDisplayDatetime(commentDate)
                };
            }

            return feed;
        },

        feeds: function(response) {
            var obj = [];
            for (var i = 0; i < response.feeds.length; i++) {
                var resp = response.feeds[i];
                var feed = makeFeedItem(resp);
                feed.GroupedFeeds = [];
                for (var j = 0; j < resp.groupedFeeds.length; j++) {
                    feed.GroupedFeeds.push(makeFeedItem(resp.groupedFeeds[j]));
                }
                obj = obj.concat(feed);
            }
            var feeds = collection(obj, this.item, function(r) {
                return factories.feed.item(r);
            });

            return { feeds: feeds, readedDate: serializeDate(response.readedDate) };

            function makeFeedItem(responseItem) {
                var item = jq.parseJSON(responseItem.feed);
                item.IsToday = responseItem.isToday;
                item.IsYesterday = responseItem.isYesterday;
                item.LastModifiedBy = responseItem.lastModifiedBy;
                item.CreatedDate = responseItem.createdDate;
                item.ModifiedDate = responseItem.modifiedDate;
                item.AggregatedDate = responseItem.aggregatedDate;

                return item;
            }
        }
    };

    /* documents */
    factories.doc = {
        miss: function(response) {
            return response;
        },

        item: function(response) {
            var crtdate = serializeDate(response.created),
                uptdate = serializeDate(response.updated);

            return {
                id: response.id,
                parentId: response.parentId || response.folderId || -1,
                folderId: response.folderId || response.parentId || -1,
                access: response.access,
                sharedByMe: response.sharedByMe,
                rootFolderType: response.rootFolderType,
                rootType: getRootFolderTypeById(response.rootFolderType),
                timestamp: crtdate ? crtdate.getTime() : 0,
                crtdate: crtdate,
                displayCrtdate: getDisplayDatetime(crtdate),
                displayDateCrtdate: getDisplayDate(crtdate),
                displayTimeCrtdate: getDisplayTime(crtdate),
                uptdate: uptdate,
                displayUptdate: getDisplayDatetime(uptdate),
                displayDateUptdate: getDisplayDate(uptdate),
                displayTimeUptdate: getDisplayTime(uptdate),
                createdBy: createPerson(response.createdBy || response.author),
                updatedBy: createPerson(response.updatedBy),
                canEdit: response.canEdit || false,
                title: response.title || '',
                description: response.description || ''
            };
        },

        rootFolder: function(response) {
            if (!response || response.length <= 1) {
                return null;
            }
            var first = response[0] || {};
            return {
                type: 'folder',
                id: first.key,
                title: first.path
            };
        },

        parentFolder: function(response) {
            if (!response || response.length <= 1) {
                return null;
            }
            var last = response[response.length - 2] || null;
            return !last ? null : {
                type: 'folder',
                id: last.key,
                title: last.path,
                path: last.path
            };
        },

        files: function(response) {
            return collection(response, this.item, function(response) {
                return factories.doc.file(response);
            });
        },

        folders: function(response) {
            return collection(response, this.item, function(response) {
                return factories.doc.folder(response);
            });
        },

        file: function (response) {
            if (!response) return undefined;

            var title = response.title ? response.title : '';

            var filename = title.substring(0, title.lastIndexOf('.'));
            filename = filename ? filename : response.title;

            var extension = (title.substring(title.lastIndexOf('.')) || '').toLowerCase();

            return extend(this.item(response), {
                type: 'file',
                extension: extension,
                filename: filename,
                filetype: getFileType(extension),
                version: response.version,
                versionGroup: response.versionGroup,
                fileStatus: response.fileStatus,
                contentLength: response.contentLength,
                pureContentLength: response.pureContentLength,
                webUrl: fixUrl(response.webUrl || ''),
                viewUrl: fixUrl(response.viewUrl || ''),
                fileUrl: fixUrl(response.fileUrl || ''),
                isSupported: isSupportedFileType(extension),
                isUploaded: false
            });
        },

        folder: function(response) {
            var uploadedfilenames = response.hasOwnProperty('__filenames') ? response.__filenames : null,
                folders = factories.doc.folders(response.folders),
                files = factories.doc.files(response.files),
                pathParts = response.pathParts,
                response = response.current || response;

            if (uploadedfilenames) {
                var filesInd = 0,
                    uploadedfilename = '',
                    uploadedfilenamesInd = uploadedfilenames.length;
                while (uploadedfilenamesInd--) {
                    uploadedfilename = uploadedfilenames[uploadedfilenamesInd];
                    filesInd = files.length;
                    while (filesInd--) {
                        if (files[filesInd].title === uploadedfilename) {
                            files[filesInd].isUploaded = true;
                        }
                    }
                }
            }

            return extend(this.item(response), {
                type: 'folder',
                folders: folders,
                files: files,
                pathParts: pathParts,
                filesCount: response.filesCount,
                foldersCount: response.foldersCount,
                isShareable: response.isShareable || false,
                canAddItems: response.access == fileShareTypes.None || response.access == fileShareTypes.ReadWrite,
                rootFolder: factories.doc.rootFolder(pathParts),
                parentFolder: factories.doc.parentFolder(pathParts)
            });
        },

        session: function(response) {
            return response;
        }
    };

    /* crm */
    factories.crm = {
        item: function(response) {
            var crtdate = serializeDate(response.created),
                uptdate = serializeDate(response.updated),
                expdate = serializeDate(response.expected || response.expectedCloseDate);

            return {
                id: response.id,
                timestamp: crtdate ? crtdate.getTime() : 0,
                crtdate: crtdate,
                displayCrtdate: getDisplayDatetime(crtdate),
                displayDateCrtdate: getDisplayDate(crtdate),
                displayTimeCrtdate: getDisplayTime(crtdate),
                uptdate: uptdate,
                displayUptdate: getDisplayDatetime(uptdate),
                displayDateUptdate: getDisplayDate(uptdate),
                displayTimeUptdate: getDisplayTime(uptdate),
                expdate: expdate,
                displayExpdate: getDisplayDatetime(expdate),
                displayDateExpdate: getDisplayDate(expdate),
                displayTimeExpdate: getDisplayTime(expdate),
                createdBy: createPerson(response.createdBy || response.createBy || response.author),
                updatedBy: createPerson(response.updatedBy),
                responsible: createPerson(response.responsible),
                title: response.title || '',
                description: response.description || ''
            };
        },

        bidCurrency: function(response) {
            return response;
        },

        customfield: function(response) {
            return response;
        },

        dealmilestone: function(response) {
            return response;
        },

        category: function(response) {
            return response;
        },

        contactstatus: function(response) {
            return response;
        },

        contacttypekind: function(response) {
            return response;
        },
        historycategory: function(response) {
            return response;
        },

        entity: function(response) {
            return response;
        },

        company: function(response) {
            return response;
        },

        address: function(response) {
            var categories = ['Home', 'Postal', 'Office', 'Billing', 'Other', 'Work'],
                infoTypes = ['Phone', 'Email', 'Website', 'Skype', 'Twitter', 'LinkedIn', 'Facebook', 'Address', 'LiveJournal', 'MySpace', 'GMail', 'Blogger', 'Yahoo', 'MSN', 'ICQ', 'Jabber', 'AIM'],
                category = response.category,
                infoType = response.infoType;

            return {
                id: response.id,
                infoType: infoType,
                infoTypeName: isFinite(+infoType) && +infoType >= 0 && +infoType < infoTypes.length ? infoTypes[+infoType] : infoType,
                category: category,
                categoryName: isFinite(+category) && +category >= 0 && +category < categories.length ? categories[+category] : category,
                data: response.data,
                isPrimary: response.isPrimary
            };
        },

        addresses: function(response) {
            return collection(response, this.address, function(response) {
                return factories.crm.address(response);
            });
        },

        contactbase: function(response) {
            return extend(this.item(response), {
                type: 'contactbase',
                displayName: response.displayName,
                isCompany: response.isCompany,
                isPrivate: response.isPrivate,
                accessList: response.accessList,
                isShared: response.isShared,
                shareType: response.shareType,
                smallFotoUrl: response.smallFotoUrl,
                mediumFotoUrl: response.mediumFotoUrl,
                company: response.company ? factories.crm.company(response.company) : null,
                email: response.email ? response.email : null,
                currency: response.currency ? factories.crm.currency(response.currency) : null,
                canEdit: response.canEdit,
                canDelete: response.canDelete
            });
        },

        contact: function(response) {
            return extend(this.item(response), {
                type: 'contact',
                contactclass: response.firstName ? 'person' : 'company',
                displayName: response.displayName,
                firstName: response.firstName,
                lastName: response.lastName,
                isCompany: response.isCompany,
                isPrivate: response.isPrivate,
                isShared: response.isShared,
                shareType: response.shareType,
                smallFotoUrl: response.smallFotoUrl,
                mediumFotoUrl: response.mediumFotoUrl,
                company: response.company ? factories.crm.company(response.company) : null,
                personsCount: response.personsCount,
                customFields: factories.crm.customfields(response.customFields),
                about: response.about,
                industry: response.industry,
                currency: response.currency ? factories.crm.currency(response.currency) : null,
                accessList: response.accessList,
                addresses: response.addresses,
                commonData: response.commonData,
                haveLateTasks: response.haveLateTasks || false,
                taskCount: response.taskCount,
                contactStatus: response.contactStatus,
                canEdit: response.canEdit,
                canDelete: response.canDelete,
                tags: response.tags
            });
        },

        simplecontact: function(response) {
            return extend(this.item(response), {
                type: 'contact',
                contactclass: response.firstName ? 'person' : 'company',
                displayName: response.displayName,
                firstName: response.firstName,
                lastName: response.lastName,
                isCompany: response.isCompany,
                isPrivate: response.isPrivate,
                isShared: response.isShared,
                shareType: response.shareType,
                smallFotoUrl: response.smallFotoUrl,
                mediumFotoUrl: response.mediumFotoUrl,
                company: response.company ? factories.crm.company(response.company) : null,
                personsCount: response.personsCount,
                customFields: factories.crm.customfields(response.customFields),
                about: response.about,
                industry: response.industry,
                accessList: response.accessList,
                addresses: response.addresses,
                commonData: response.commonData,
                haveLateTasks: response.haveLateTasks || false,
                taskCount: response.taskCount,
                contactStatus: response.contactStatus,
                currency: response.currency ? factories.crm.currency(response.currency) : null,
                canEdit: response.canEdit,
                canDelete: response.canDelete,
                tags: response.tags,
                nearTask: response.nearTask || null
            });
        },

        basecontact: function(response) {
            return extend(this.item(response), {
                type: 'contact',
                contactclass: response.isCompany ? 'company' : 'person',
                displayName: response.displayName,
                email: response.email,//may be undefined if ContactBaseWithPhoneWrapper
                phone: response.phone,//may be undefined if ContactBaseWithEmailWrapper
                isCompany: response.isCompany,
                currency: response.currency ? factories.crm.currency(response.currency) : null,
                isPrivate: response.isPrivate,
                isShared: response.isShared,
                shareType: response.shareType,
                accessList: response.accessList,
                smallFotoUrl: response.smallFotoUrl,
                mediumFotoUrl: response.mediumFotoUrl,
                canEdit: response.canEdit,
                canDelete: response.canDelete
            });
        },

        socialmediaavatar: function(response) {
            return {
                type: "socialmediaavatar",
                socialNetwork: response.socialNetwork,
                imageUrl: response.imageUrl,
                identity: response.identity
            };
        },

        socialmediaavatars: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.socialmediaavatar(response);
            });
        },

        tweet: function(response) {
            var postedOn = serializeDate(response.postedOn);
            return {
                type: "tweet",
                userImageUrl: response.userImageUrl,
                userName: response.userName,
                userId: response.userId,
                text: response.text,
                postedOn: postedOn,
                postedOnDisplay: getDisplayDatetime(postedOn),
                source: response.source
            };
        },

        tweets: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.tweet(response);
            });
        },

        twitterprofile: function(response) {
            var postedOn = serializeDate(response.postedOn);
            return {
                type: "twitterprofile",
                userID: response.userID,
                screenName: response.screenName,
                userName: response.userName,
                smallImageUrl: response.smallImageUrl,
                description: response.description
            };
        },

        twitterprofiles: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.twitterprofile(response);
            });
        },

        progressitem: function(response) {
            return response;
        },

        exportitem: function (response) {
            if (!response || jq.isEmptyObject(response)) return response;

            return {
                id: response.id,
                status: response.status,
                percentage: response.percentage,
                isCompleted: response.isCompleted,
                exception: response.error || response.errorText,
                fileId: response.fileId,
                fileUrl: response.fileUrl,
                fileName: response.fileName
            };
        },

        fileuploadresult:  function(response) {
            return response;
        },

        file: function(response) {
            return extend(this.item(response), {
                type: 'file',
                title: response.title,
                folderId: response.folderId,
                version: response.version,
                fileStatus: response.fileStatus,
                contentLength: response.contentLength,
                pureContentLength: response.pureContentLength,
                viewUrl: response.viewUrl,
                webUrl: response.webUrl,
                access: response.access,
                sharedByMe: response.sharedByMe,
                rootFolderType: response.rootFolderType,
                createdDate: serializeDate(response.created)
            });
        },

        files: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.file(response);
            });
        },

        tag: function(response) {
            return response;
        },

        task: function(response) {
            var deadLine = serializeDate(response.deadLine);
            var deadLineString = deadLine && (deadLine.getHours() == 0 && deadLine.getMinutes() == 0 && deadLine.getSeconds() == 0) ? getDisplayDate(deadLine) : getDisplayDatetime(deadLine);

            return extend(this.item(response), {
                type: 'task',
                category: factories.crm.category(response.category),
                canEdit: response.canEdit,
                canWork: response.canWork,
                contact: response.contact ? factories.crm.contactbase(response.contact) : null,
                deadLine: deadLine,
                deadLineString: deadLineString,
                entity: response.entity ? factories.crm.entity(response.entity) : null,
                isClosed: response.isClosed,
                alertValue: typeof(response.alertValue) != "undefined" ? response.alertValue : 0
            });
        },

        opportunity: function(response) {
            var expectedCloseDate = serializeDate(response.expectedCloseDate);
            var actualCloseDate = serializeDate(response.actualCloseDate);

            return extend(this.item(response), {
                type: 'opportunity',
                accessList: response.accessList,
                actualCloseDate: actualCloseDate,
                actualCloseDateString: getDisplayDate(actualCloseDate),
                bidCurrency: factories.crm.bidCurrency(response.bidCurrency),
                bidType: response.bidType,
                bidValue: response.bidValue,
                contact: response.contact ? factories.crm.contact(response.contact) : null,
                customFields: factories.crm.customfields(response.customFields),
                stage: factories.crm.dealmilestone(response.stage),
                successProbability: response.successProbability,
                expectedCloseDate: expectedCloseDate,
                expectedCloseDateString: getDisplayDate(expectedCloseDate),
                isPrivate: response.isPrivate,
                canEdit: response.canEdit,
                perPeriodValue: response.perPeriodValue
            });
        },

        taskcategory: function(response) {
            return extend(this.item(response), {
                type: 'taskcategory',
                imagePath: response.imagePath,
                relativeItemsCount: response.relativeItemsCount,
                sortOrder: response.sortOrder
            });
        },

        caseitem: function(response) {
            return extend(this.item(response), {
                type: 'case',
                accessList: response.accessList,
                isClosed: response.isClosed,
                isPrivate: response.isPrivate,
                canEdit: response.canEdit
            });
        },

        cases: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.caseitem(response);
            });
        },

        tags: function(response) {
            return response;
        },

        fulltag: function(response) {
            return {
                type: 'fulltag',
                title: response.title,
                relativeItemsCount: response.relativeItemsCount || 0,
            };
        },

        fulltags: function(response) {
            var items = [];
            for (var i = 0, n = response.length; i < n; i++) {
                items.push(factories.crm.fulltag(response[i]));
            }
            return items;
        },

        contacts: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.contact(response);
            });
        },

        simplecontacts: function(response) {
            var items = [];
            for (var i = 0; i < response.length; i++) {
                var item = factories.crm.simplecontact(response[i].contact);
                if (response[i].hasOwnProperty('task')) {
                    item.nearTask = factories.crm.task(response[i].task);
                }
                items.push(item);
            }
            return items;
        },

        basecontacts: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.basecontact(response);
            });
        },

        customfields: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.customfield(response);
            });
        },

        dealmilestones: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.dealmilestone(response);
            });
        },

        contactstatuses: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.contactstatus(response);
            });
        },

        contacttypekinds: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.contacttypekind(response);
            });
        },

        historycategories: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.historycategory(response);
            });
        },

        tasks: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.task(response);
            });
        },

        opportunities: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.opportunity(response);
            });
        },

        taskcategories: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.taskcategory(response);
            });
        },

        historyevent: function(response) {
            return extend(this.item(response), {
                type: 'event',
                createdDate: getDisplayDate(serializeDate(response.created)),
                canEdit: response.canEdit,
                category: factories.crm.category(response.category),
                contact: response.contact ? factories.crm.contact(response.contact) : null,
                content: response.content,
                entity: response.entity ? factories.crm.entity(response.entity) : null,
                files: response.files && response.files.length != 0 ? factories.crm.files(response.files) : null
            });
        },

        historyevents: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.historyevent(response);
            });
        },

        currency: function(response) {
            return {
                type: 'currency',
                abbreviation: response.abbreviation,
                cultureName: response.cultureName,
                symbol: response.symbol,
                title: response.title,
                isConvertable: response.isConvertable,
                isBasic: response.isBasic
            };
        },

        currencies: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.currency(response);
            });
        },

        smtpsettings: function(response){
            return {
                type: 'smtpsettings',
                enableSSL: response.enableSSL,
                host: response.host,
                hostLogin: response.hostLogin,
                hostPassword: response.hostPassword,
                port: response.port,
                requiredHostAuthentication: response.requiredHostAuthentication,
                senderDisplayName: response.senderDisplayName,
                senderEmailAddress: response.senderEmailAddress
            };
        },

        rootfolder: function(response) {
            return {
                id: response
            };
        },

        invoice: function(response) {
            var issueDate = serializeDate(response.issueDate),
                issueDateString = getDisplayDate(issueDate),
                dueDate = serializeDate(response.dueDate),
                dueDateString = dueDate && (dueDate.getHours() == 0 && dueDate.getMinutes() == 0 && dueDate.getSeconds() == 0)
                    ? getDisplayDate(dueDate)
                    : getDisplayDatetime(dueDate),
                crtdate = response.createOn ? serializeDate(response.createOn) : null,
                debtor = response.status.id == 2 && dueDate < new Date();

            return extend(this.item(response), {
                type: 'invoice',
                number: response.number,
                status: response.status,//int
                debtor: debtor,
                issueDate: issueDate,
                issueDateString: issueDateString,
                dueDate: dueDate,
                dueDateString: dueDateString,
                templateType: response.templateType,
                contact: response.contact ? factories.crm.basecontact(response.contact) : null,
                consignee: response.consignee ? factories.crm.basecontact(response.consignee) : null,
                entity: response.entity ? factories.crm.entity(response.entity) : null,
                language: response.language,
                currency: response.currency ? factories.crm.currency(response.currency) : null,
                cost: response.cost,
                purchaseOrderNumber: response.purchaseOrderNumber,
                terms: response.terms,
                description: response.description,
                fileId: response.fileID,
                crtdate: crtdate,
                displayCrtdate: getDisplayDatetime(crtdate),
                displayDateCrtdate: getDisplayDate(crtdate),
                displayTimeCrtdate: getDisplayTime(crtdate),
                invoiceLines: response.invoiceLines && response.invoiceLines.length != 0 ? factories.crm.invoiceLines(response.invoiceLines) : null,
                canEdit: response.canEdit,
                canDelete: response.canDelete
            });
        },

        invoices: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.invoice(response);
            });
        },

        invoiceJsonData: function(response) {
            return jq.parseJSON(response || null);
        },

        invoiceLines: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.invoiceLine(response);
            });
        },

        invoiceLine: function(response) {
            return {
                type: 'invoiceLine',
                id: response.id,
                invoiceID: response.invoiceID,
                invoiceItemID: response.invoiceItemID,
                invoiceTax1ID: response.invoiceTax1ID,
                invoiceTax2ID: response.invoiceTax2ID,
                sortOrder: response.sortOrder,
                description: response.description,
                quantity: response.quantity,
                price: response.price,
                discount: response.discount,
                canEdit: response.canEdit
            };
        },

        invoiceItems: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.invoiceItem(response);
            });
        },

        invoiceItem: function(response) {
            return {
                type: 'invoiceItem',
                id: response.id,
                title: response.title,
                stockKeepingUnit: response.stockKeepingUnit,
                description: response.description,
                price: response.price,
                currency: response.currency,
                stockQuantity: response.stockQuantity,
                trackInvenory: response.trackInvenory,
                invoiceTax1: response.invoiceTax1 ? factories.crm.invoiceTax(response.invoiceTax1) : null,
                invoiceTax2: response.invoiceTax2 ? factories.crm.invoiceTax(response.invoiceTax2) : null,
                sortOrder: response.sortOrder,
                canEdit: response.canEdit,
                canDelete: response.canDelete
            };
        },

        invoicesAndItems: function(response) {
            return {
                invoices: response.key && response.key.length != 0 ? factories.crm.invoices(response.key) : null,
                invoiceItems: response.value && response.value.length != 0 ? factories.crm.invoiceItems(response.value) : null
            };
        },

        invoiceTaxes: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.invoiceTax(response);
            });
        },

        invoiceTax: function(response) {
            return {
                type: 'invoiceTax',
                id: response.id,
                name: response.name,
                description: response.description,
                rate: response.rate,
                canEdit: response.canEdit,
                canDelete: response.canDelete
            };
        },

        invoiceSettings: function(response) {
            return {
                type: 'invoiceSettings',
                autogenerated: response.autogenerated,
                prefix: response.prefix,
                number: response.number,
                terms: response.terms
            };
        },

        currencyRates: function(response) {
            return collection(response, this.item, function(response) {
                return factories.crm.currencyRate(response);
            });
        },

        currencyRate: function(response) {
            return {
                type: 'currencyRate',
                id: response.id,
                fromCurrency: response.fromCurrency,
                toCurrency: response.toCurrency,
                rate: response.rate
            };
        },

        voipNumbers: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.voipNumber(response);
            });
        },

        voipNumber: function(response) {
            return {
                id: response.id,
                number: response.number,
                alias: response.alias,
                settings: response.settings
            };
        },

        voipSettings: function(response) {
            return response;
        },

        voipCalls: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.voipCall(response);
            });
        },

        voipCall: function (response) {
            var serializedDate = serializeDate(response.dialDate);

            return {
                id: response.id,
                from: response.from,
                to: response.to,
                status: response.status,
                answeredBy: createPerson(response.answeredBy),
                dateObj: serializedDate,
                datetime: getDisplayDatetime(serializedDate),
                time: getDisplayTime(serializedDate),
                dialDuration: response.dialDuration,
                cost: response.cost,
                recordUrl: response.recordUrl,
                recordDuration: response.recordDuration,
                contact: response.hasOwnProperty('contact') && response.contact ? factories.crm.contact(response.contact) : null,
                history: response.history
            };
        },

        converterData: function(response) {
            return {
                type: 'converterData',
                storageUrl: response.storageUrl,
                revisionId: response.revisionId,
                urlToFile: response.urlToFile,
                invoiceId: response.invoiceId,
                fileId: response.fileId
            };
        },
    };

    var create = function(apiurl, method, response, responses) {
        if (typeof response === 'undefined') {
            return null;
        }

        responses = responses || [response];
        method = typeof method === 'string' ? method.toLowerCase() : '';
        var apiAnchorsInd = apiAnchors.length;
        while (apiAnchorsInd--) {
            if (apiAnchors[apiAnchorsInd].re.test(apiurl)) {
                //console.log(apiurl, ' # ', apiAnchors[apiAnchorsInd].handler);
                if (!apiAnchors[apiAnchorsInd].hasOwnProperty('method')) {
                    break;
                }
                if (apiAnchors[apiAnchorsInd].method === method) {
                    break;
                }
            }
        }

        var handlername = apiAnchorsInd !== -1 ? apiAnchors[apiAnchorsInd].handler : apiurl,
            namespaces = handlername.split('-');

        if (namespaces.length > 1) {
            if (factories.hasOwnProperty(namespaces[0]) && typeof factories[namespaces[0]][namespaces[1]] === 'function') {
                return factories[namespaces[0]][namespaces[1]].apply(factories[namespaces[0]], [response, responses]);
            }
        }

        if (typeof factories[handlername] === 'function') {
            return factories[handlername].apply(factories, [response, responses]);
        }

        return onlyFactory === true ? null : response;
    };

    return {
        profile: myProfile,
        dateFormats: formats,
        contactTypes: contactTypes,
        nameCollections: nameCollections,

        init: init,

        create: create,

        fixData: fixData,
        formattingDate: formattingDate,
        serializeDate: serializeDate,
        serializeTimestamp: serializeTimestamp,
        getDisplayTime: getDisplayTime,
        getDisplayDate: getDisplayDate,
        getDisplayDatetime: getDisplayDatetime
    };
})();