/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            mobphone: { name: 'mobphone', type: 1, title: 'Mobile' }
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
            { handler: 'cmt-blog', re: /blog\.json/, method: 'post' },
            { handler: 'cmt-blog', re: /blog\/[\w\d-]+\.json/ },
            { handler: 'cmt-topic', re: /forum\/topic\/[\w\d-]+\.json/ },
            { handler: 'cmt-event', re: /event\/[\w\d-]+\.json/ },
            { handler: 'cmt-event', re: /event\.json/, method: 'post' },
            { handler: 'cmt-bookmark', re: /bookmark\/[\w\d-]+\.json/ },
            { handler: 'cmt-bookmark', re: /bookmark\.json/, method: 'post' },
            { handler: 'cmt-blogs', re: /blog\.json/, method: 'get' },
            { handler: 'cmt-blogs', re: /blog\/@search\/.+\.json/, method: 'get' },
            { handler: 'cmt-topics', re: /forum\/topic\/recent\.json/ },
            { handler: 'cmt-topics', re: /forum\/@search\/.+\.json/, method: 'get' },
            { handler: 'cmt-categories', re: /forum\.json/ },
            { handler: 'cmt-events', re: /event\.json/, method: 'get' },
            { handler: 'cmt-events', re: /event\/@search\/.+\.json/, method: 'get' },
            { handler: 'cmt-bookmarks', re: /bookmark\/top\/recent\.json/, method: 'get' },
            { handler: 'cmt-bookmarks', re: /bookmark\/@search\/.+\.json/, method: 'get' },
            { handler: 'prj-task', re: /project\/[\w\d-]+\/task\.json/, method: 'post' },
            { handler: 'prj-task', re: /project\/task\/[\w\d-]+\.json/ },
            { handler: 'prj-task', re: /project\/task\/[\w\d-]+\/[\w\d-]+\.json/ },
            { handler: 'prj-task', re: /project\/task\/[\w\d-]+\/status\.json/ },
            { handler: 'prj-task', re: /project\/task\/[\w\d-]+\/[\w\d-]+\/status\.json/ },
            { handler: 'prj-tasks', re: /project\/[\w\d-]+\/task\.json/, method: 'get' },
            { handler: 'prj-tasks', re: /project\/[\w\d-]+\/task\/@self\.json/ },
            { handler: 'prj-tasks', re: /project\/[\w\d-]+\/task\/filter\.json/ },
            { handler: 'prj-tasks', re: /project\/task\/filter\.json/ },
            { handler: 'prj-tasks', re: /project\/task\.json\?taskid/ },
            { handler: 'prj-simpletasks', re: /project\/task\/filter\/simple\.json/, method: 'get' },
            { handler: 'prj-tasks', re: /project\/task\/@self\.json/ },
            { handler: 'prj-milestone', re: /project\/milestone\/[\w\d-]+\.json/ },
            { handler: 'prj-milestone', re: /project\/[\w\d-]+\/milestone\.json/, method: 'post' },
            { handler: 'prj-milestones', re: /project\/[\w\d-]+\/milestone\.json/, method: 'get' },
            { handler: 'prj-milestones', re: /project\/milestone\/[\w\d-]+\/[\w\d-]+\/[\w\d-]+\.json/ },
            { handler: 'prj-milestones', re: /project\/milestone\/[\w\d-]+\/[\w\d-]+\.json/ },
            { handler: 'prj-milestones', re: /project\/milestone\/late\.json/ },
            { handler: 'prj-milestones', re: /project\/milestone\.json/ },
            { handler: 'prj-milestones', re: /project\/milestone\/filter\.json/ },
            { handler: 'prj-milestone', re: /project\/milestone\/[\w\d-]+\/status\.json/ },
            { handler: 'prj-discussion', re: /project\/message\/[\w\d-]+\.json/ },
            { handler: 'prj-discussion', re: /project\/[\w\d-]+\/message\.json/, method: 'post' },
            { handler: 'prj-discussions', re: /project\/message\.json/ },
            { handler: 'prj-discussions', re: /project\/[\w\d-]+\/message\.json/, method: 'get' },
            { handler: 'prj-discussions', re: /project\/message\/filter\.json/ },
            { handler: 'prj-project', re: /project\.json/, method: 'post' },
            { handler: 'prj-project', re: /project\/[\w\d-]+\/contact\.json/, method: 'post' },
            { handler: 'prj-project', re: /project\/[\w\d-]+\/contact\.json/, method: 'delete' },
            { handler: 'prj-projects', re: /project[\/]*[@self|@follow]*\.json/ },
            { handler: 'prj-projects', re: /project\/filter\.json/ },
            { handler: 'prj-projects', re: /project\/contact\/[\w\d-]+\.json/ },
            { handler: 'prj-searchentries', re: /project\/@search\/.+\.json/ },
            { handler: 'prj-projectperson', re: /project\/[\w\d-]+\/team\.json/, method: 'post' },
            { handler: 'prj-projectperson', re: /project\/[\w\d-]+\/team\.json/, method: 'delete' },
            { handler: 'prj-projectpersons', re: /project\/[\w\d-]+\/team\.json/, method: 'get' },
            { handler: 'prj-timespend', re: /project\/time\/[\w\d-]+\.json/ },
            { handler: 'prj-timespends', re: /project\/time\/filter\.json/, method: 'get' },
            { handler: 'prj-timespends', re: /project\/task\/[\w\d-]+\/time\.json/, method: 'get' },
            { handler: 'prj-activities', re: /project\/activities\/filter\.json/ },
            { handler: 'doc-miss', re: /files\/fileops.json/ },
            { handler: 'doc-folder', re: /files\/[@\w\d-]+\.json/ },
            { handler: 'doc-folder', re: /files\/[\w\d-]+\/[text|html|file]+\.json/ },
            { handler: 'doc-folder', re: /project\/[\w\d-]+\/files\.json/ },
            { handler: 'doc-files', re: /project\/task\/[\w\d-]+\/files\.json/ },
            { handler: 'doc-files', re: /project\/[\w\d-]+\/entityfiles\.json/ },
            { handler: 'doc-files', re: /crm\/[\w\d-]+\/[\w\d-]+\/files\.json/, method: 'get' },
            { handler: 'doc-file', re: /files\/file\/[\w\d-]+\.json/ },
            { handler: 'doc-file', re: /files\/[\w\d-]+\/upload\.xml/ },
            { handler: 'doc-file', re: /files\/[\w\d-]+\/upload\.json/ },
            { handler: 'doc-session', re: /files\/[\w\d-]+\/upload\/create_session\.json/ },
            { handler: 'doc-file', re: /crm\/files\/[\w\d-]+\.json/ },
            { handler: 'doc-file', re: /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/upload\.xml/ },
            { handler: 'doc-file', re: /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/upload\.json/ },
            { handler: 'doc-file', re: /crm\/[case|contact|opportunity]+\/[\w\d-]+\/files\/text\.json/ },
            { handler: 'doc-miss', re: /files\/fileops.json/ },
            { handler: 'crm-addresses', re: /crm\/contact\/[\w\d-]+\/data\.json/, method: 'get' },
            { handler: 'crm-address', re: /crm\/contact\/[\w\d-]+\/data\/[\w\d-]+\.json/ },
            { handler: 'crm-address', re: /crm\/contact\/[\w\d-]+\/data\.json/, method: 'post' },
            { handler: 'crm-address', re: /crm\/contact\/[\w\d-]+\/batch\.json/ },
            { handler: 'crm-contact', re: /crm\/contact\/[\w\d-]+\.json/ },
            { handler: 'crm-contact', re: /crm\/contact\/company\/[\w\d-]+\/type\.json/, method: 'post' },
            { handler: 'crm-contact', re: /crm\/contact\/company\/[\w\d-]+\/person\.json/, method: 'post' },
            { handler: 'crm-contact', re: /crm\/contact\/company\/[\w\d-]+\/person\.json/, method: 'delete' },
            { handler: 'crm-contact', re: /crm\/[case|opportunity]+\/[\w\d-]+\/contact\.json/, method: 'post' },
            { handler: 'crm-contact', re: /crm\/[case|opportunity]+\/[\w\d-]+\/contact\/[\w\d-]+\.json/, method: 'post' },
            { handler: 'crm-contact', re: /crm\/[case|opportunity]+\/[\w\d-]+\/contact\/[\w\d-]+\.json/ },
            { handler: 'crm-contacts', re: /crm\/contact\/bycontactinfo\.json/, method: 'get' },
            { handler: 'crm-contact', re: /crm\/contact\/merge\.json/ },
            { handler: 'crm-socialmediaavatars', re: /crm\/contact\/socialmediaavatar\.json/ },
            { handler: 'crm-tweets', re: /crm\/contact\/[\w\d-]+\/tweets\.json/ },
            { handler: 'crm-twitterprofiles', re: /crm\/contact\/twitterprofile\.json/ },
            { handler: 'crm-facebookprofiles', re: /crm\/contact\/facebookprofile\.json/ },
            { handler: 'crm-linkedinprofiles', re: /crm\/contact\/linkedinprofile\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/mailsmtp\/send\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/mailsmtp\/status\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/mailsmtp\/cancel\.json/ },
            { handler: 'crm-progressitem', re: /crm\/[contact|person|company|opportunity|case]+\/import\/status\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/export\/status\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/export\/cancel\.json/ },
            { handler: 'crm-progressitem', re: /crm\/contact\/export\/start\.json/ },
            { handler: 'crm-fileuploadresult', re: /crm\/import\/uploadfake\.json/ },
            { handler: 'crm-task', re: /crm\/task\.json/ },
            { handler: 'crm-task', re: /crm\/task\/[\w\d-]+\.json/ },
            { handler: 'crm-task', re: /crm\/task\/[\w\d-]+\/close\.json/ },
            { handler: 'crm-task', re: /crm\/task\/[\w\d-]+\/reopen\.json/ },
            { handler: 'crm-opportunity', re: /crm\/opportunity\/[\w\d-]+\.json/ },
            { handler: 'crm-opportunity', re: /crm\/opportunity\/[\w\d-]+\/stage\/[\w\d-]+\.json/ },
            { handler: 'crm-opportunity', re: /crm\/contact\/[\w\d-]+\/opportunity\/[\w\d-]+\.json/ },
            { handler: 'crm-dealmilestone', re: /crm\/opportunity\/stage\/[\w\d-]+\.json/ },
            { handler: 'crm-dealmilestone', re: /crm\/opportunity\/stage\/[\w\d-]+\/color\.json/ },
            { handler: 'crm-dealmilestone', re: /crm\/opportunity\/stage\.json/, method: 'post' },
            { handler: 'crm-dealmilestones', re: /crm\/opportunity\/stage\/reorder\.json/ },
            { handler: 'crm-dealmilestones', re: /crm\/opportunity\/stage\.json/, method: 'get' },
            { handler: 'crm-contactstatus', re: /crm\/contact\/status\/[\w\d-]+\/color\.json/ },
            { handler: 'crm-contactstatus', re: /crm\/contact\/status\/[\w\d-]+\.json/ },
            { handler: 'crm-contactstatus', re: /crm\/contact\/status\.json/, method: 'post' },
            { handler: 'crm-contactstatuses', re: /crm\/contact\/status\/reorder\.json/ },
            { handler: 'crm-contactstatuses', re: /crm\/contact\/status\.json/, method: 'get' },
            { handler: 'crm-contacttypekind', re: /crm\/contact\/type\/[\w\d-]+\.json/ },
            { handler: 'crm-contacttypekind', re: /crm\/contact\/type\.json/, method: 'post' },
            { handler: 'crm-contacttypekinds', re: /crm\/contact\/type\/reorder\.json/ },
            { handler: 'crm-contacttypekinds', re: /crm\/contact\/type\.json/, method: 'get' },
            { handler: 'crm-customfield', re: /crm\/[contact|person|company|opportunity|case]+\/customfield\/[\w\d-]+\.json/ },
            { handler: 'crm-customfield', re: /crm\/[contact|person|company|opportunity|case]+\/customfield\.json/ },
            { handler: 'crm-customfields', re: /crm\/[contact|person|company|opportunity|case]+\/customfield\/definitions\.json/ },
            { handler: 'crm-customfields', re: /crm\/[contact|person|company|opportunity|case]+\/customfield\/reorder\.json/ },
            { handler: 'crm-tag', re: /crm\/[case|contact|opportunity]+\/tag\.json/ },
            { handler: 'crm-tag', re: /crm\/[case|contact|opportunity]+\/taglist\.json/ },
            { handler: 'crm-tag', re: /crm\/[case|contact|opportunity]+\/[\w\d-]+\/tag\.json/ },
            { handler: 'crm-tags', re: /crm\/[case|contact|opportunity]+\/tag\/unused\.json/ },
            { handler: 'crm-tags', re: /crm\/[case|contact|opportunity]+\/tag\/[\w\d-]+\.json/ },
            { handler: 'crm-fulltags', re: /crm\/[case|contact|opportunity]+\/tag\.json/, method: 'get' },
            { handler: 'crm-caseitem', re: /crm\/case\/[\w\d-]+\/close\.json/ },
            { handler: 'crm-caseitem', re: /crm\/case\/[\w\d-]+\/reopen\.json/ },
            { handler: 'crm-cases', re: /crm\/cases\.json/ },
            { handler: 'crm-cases', re: /crm\/case\/filter\.json/ },
            { handler: 'crm-cases', re: /crm\/case\/byprefix\.json/ },
            { handler: 'crm-contacts', re: /crm\/contact\.json/ },
            { handler: 'crm-basecontacts', re: /crm\/contact\/byprefix\.json/ },
            { handler: 'crm-contacts', re: /crm\/contact\/filter\.json/ },
            { handler: 'crm-simplecontacts', re: /crm\/contact\/simple\/filter\.json/ },
            { handler: 'crm-basecontacts', re: /crm\/contact\/mail\.json/ },
            { handler: 'crm-contacts', re: /crm\/contact\/company\/[\w\d-]+\/person\.json/, method: 'get' },
            { handler: 'crm-contacts', re: /crm\/[case|opportunity]+\/[\w\d-]+\/contact\.json/, method: 'get' },
            { handler: 'crm-contacts', re: /crm\/contact\/access\.json/ },
            { handler: 'crm-contacts', re: /crm\/contact\/[\w\d-]+\/access\.json/ },
            { handler: 'crm-crunchbaseinfo', re: /crm\/contact\/crunchbase\.json/ },
            { handler: 'crm-cases', re: /crm\/case\/access\.json/ },
            { handler: 'crm-cases', re: /crm\/case\/[\w\d-]+\/access\.json/ },
            { handler: 'crm-opportunities', re: /crm\/opportunity\/access\.json/ },
            { handler: 'crm-opportunities', re: /crm\/opportunity\/[\w\d-]+\/access\.json/ },
            //{handler : 'crm-customfields', re : /crm\/contact\/[\w\d-]+\/access\.json/},
            { handler: 'crm-tasks', re: /crm\/task\/filter\.json/ },
            { handler: 'crm-opportunities', re: /crm\/opportunity\/filter\.json/ },
            { handler: 'crm-opportunities', re: /crm\/opportunity\/bycontact\/[\w\d-]+\.json/ },
            { handler: 'crm-opportunities', re: /crm\/opportunity\/byprefix\.json/ },
            { handler: 'crm-taskcategory', re: /crm\/task\/category\/[\w\d-]+\.json/ },
            { handler: 'crm-taskcategory', re: /crm\/task\/category\/[\w\d-]+\/icon\.json/ },
            { handler: 'crm-taskcategory', re: /crm\/task\/category\.json/, method: 'post' },
            { handler: 'crm-taskcategories', re: /crm\/task\/category\/reorder\.json/ },
            { handler: 'crm-taskcategories', re: /crm\/task\/category\.json/, method: 'get' },
            { handler: 'crm-historyevent', re: /crm\/history\.json/ },
            { handler: 'crm-historyevent', re: /crm\/history\/[\w\d-]+.json/ },
            { handler: 'crm-historyevent', re: /crm\/[contact|opportunity|case]+\/[\w\d-]+\/files\.json/, method: 'post' },
            { handler: 'crm-historyevents', re: /crm\/history\/filter\.json/ },
            { handler: 'crm-historycategory', re: /crm\/history\/category\/[\w\d-]+\.json/ },
            { handler: 'crm-historycategory', re: /crm\/history\/category\/[\w\d-]+\/icon\.json/ },
            { handler: 'crm-historycategory', re: /crm\/history\/category\.json/, method: 'post' },
            { handler: 'crm-historycategories', re: /crm\/history\/category\/reorder\.json/ },
            { handler: 'crm-historycategories', re: /crm\/history\/category\.json/, method: 'get' },
            { handler: 'crm-currency', re: /crm\/settings\/currency\.json/ },
            { handler: 'crm-currencies', re: /crm\/settings\/currency\.json/, method: 'get' },
            { handler: 'crm-smtpsettings', re: /crm\/settings\/smtp\.json/ },
            { handler: 'crm-rootfolder', re: /crm\/files\/root\.json/ },
            { handler: 'crm-tasktemplatecontainer', re: /crm\/[contact|person|company|opportunity|case]+\/tasktemplatecontainer\.json/ },
            { handler: 'crm-tasktemplatecontainer', re: /crm\/tasktemplatecontainer\/[\w\d-]+\.json/ },
            { handler: 'crm-tasktemplate', re: /crm\/tasktemplatecontainer\/[\w\d-]+\/tasktemplate\.json/ },
            { handler: 'crm-tasktemplate', re: /crm\/tasktemplatecontainer\/tasktemplate\/[\w\d-]+\.json/ },
            { handler: 'crm-invoiceline', re: /crm\/invoiceline\.json/ },
            { handler: 'crm-invoiceline', re: /crm\/invoice\/[\w\d-]+\.json/ },
            { handler: 'crm-invoice', re: /crm\/invoice\.json/, method: 'post' },
            { handler: 'crm-invoice', re: /crm\/invoice\/[\w\d-]+\.json/ },
            { handler: 'crm-invoice', re: /crm\/invoice\/sample\.json/ },
            { handler: 'crm-invoice', re: /crm\/invoice\/sample\.json/ },
            { handler: 'crm-invoice', re: /crm\/invoice\/bynumber\.json/ },
            { handler: 'crm-invoiceJsonData', re: /crm\/invoice\/jsondata\/[\w\d-]+\.json/ },
            { handler: 'crm-invoices', re: /crm\/invoice\.json/, method: 'delete' },
            { handler: 'crm-invoicesAndItems', re: /crm\/invoice\/status\/[\w\d-]+\.json/ },
            { handler: 'crm-invoices', re: /crm\/invoice\/filter\.json/ },
            { handler: 'crm-invoices', re: /crm\/[contact|person|company|opportunity]+\/invoicelist\/[\w\d-]+\.json/ },
            { handler: 'crm-invoiceItem', re: /crm\/invoiceitem\.json/, method: 'post' },
            { handler: 'crm-invoiceItems', re: /crm\/invoiceitem\.json/, method: 'delete' },
            { handler: 'crm-invoiceItems', re: /crm\/invoiceitem\/filter\.json/ },
            { handler: 'crm-invoiceTax', re: /crm\/invoice\/tax\/[\w\d-]+\.json/ },
            { handler: 'crm-invoiceTax', re: /crm\/invoice\/tax\.json/, method: 'post' },
            { handler: 'crm-invoiceTaxes', re: /crm\/invoice\/tax\.json/, method: 'get' },
            { handler: 'crm-invoiceSettings', re: /crm\/invoice\/settings\.json/ },
            { handler: 'crm-invoiceSettings', re: /crm\/invoice\/settings\/name\.json/ },
            { handler: 'crm-invoiceSettings', re: /crm\/invoice\/settings\/terms\.json/ },
            { handler: 'doc-file', re: /crm\/invoice\/[\w\d-]+\/pdf\.json/ },
            { handler: 'crm-converterData', re: /crm\/invoice\/converter\/data\.json/ },            
            { handler: 'crm-currencyRates', re: /crm\/currency\/rates\.json/, method: 'get' },
            { handler: 'crm-currencyRate', re: /crm\/currency\/rates\/[\w\d-]+\.json/ },
            { handler: 'crm-currencyRate', re: /crm\/currency\/rates\/\w{3}\/\w{3}\.json/, method: 'get' },
            { handler: 'crm-currencyRate', re: /crm\/currency\/rates\.json/, method: 'post' },            
            { handler: 'crm-voipNumbers', re: /crm\/voip\/numbers\/available\.json/ },
            { handler: 'crm-voipNumbers', re: /crm\/voip\/numbers\/existing\.json/ },
            { handler: 'crm-voipNumber', re: /crm\/voip\/numbers\.json/, method: 'post' },
            { handler: 'crm-voipNumber', re: /crm\/voip\/numbers\/current\.json/ },
            { handler: 'crm-voipCall', re: /crm\/voip\/call\.json/, method: 'post' },
            { handler: 'crm-voipCalls', re: /crm\/voip\/call\.json/, method: 'get' },
            { handler: 'crm-voipCall', re: /crm\/voip\/call\/[\w\d-]+\.json/, method: 'get' },
            { handler: 'crm-voipCalls', re: /crm\/voip\/call\/missed\.json/, method: 'get' },
            { handler: 'crm-voipUploads', re: /crm\/voip\/uploads\.json/ },
            { handler: 'authentication', re: /authentication\.json/ },
            { handler: 'settings', re: /settings\.json/ },
            { handler: 'security', re: /settings\/security\.json/ },
            { handler: 'access', re: /settings\/security\/access\.json/ },
            { handler: 'administrator', re: /settings\/security\/administrator\.json/ },
            { handler: 'quotas', re: /settings\/quota\.json/ },
            { handler: 'profile', re: /people\/[\w\d-]+\.json/ },
            { handler: 'isme', re: /people\/@self\.json/ },
            { handler: 'profiles', re: /people\/status\/[\w\d-]+\.json/ },
            { handler: 'profiles', re: /people\/status\/[\w\d-]+\/@search\/.+\.json/ },
            { handler: 'profiles', re: /people\/status\/[\w\d-]+\/search\.json/ },
            { handler: 'profiles', re: /people\.json/ },
            { handler: 'profiles', re: /people\/@search\/.+\.json/ },
            { handler: 'profiles', re: /people\/search\.json/ },
            { handler: 'profiles', re: /people\/filter\.json/ },
            { handler: 'profiles', re: /people\/type\/[\w\d-]+\.json/ },
            { handler: 'profiles', re: /people\/invite\.json/ },
            { handler: 'group', re: /group\/[\w\d-]+\.json/ },
            { handler: 'groups', re: /group\.json/ },
            { handler: 'crm-tasks', re: /crm\/contact\/task\/group\.json/ },
            { handler: 'crm-tag', re: /crm\/[company|person]+\/[\w\d-]+\/tag\/group\.json/ },
            { handler: 'crm-voipNumber', re: /crm\/voip\/numbers\/[\w]+\/settings.json/, method: 'put' },
            { handler: 'crm-voipSettings', re: /crm\/voip\/numbers\/settings.json/ },
            { handler: 'comment', re: /comment\.json/, method: 'post' },
            { handler: 'comments', re: /comment\.json/, method: 'get' },
            { handler: 'feed-feeds', re: /feed\/filter\.json/ },
            { handler: 'text', re: /crm\/[contact|opportunity|case|relationshipevent]+\/[\w\d-]+\/files\/hidden\.json/ }
        ];

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
                        create(fld, 'get', response);
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
        if (new Date(Date.parse('1970-01-01T00:00:00.000Z')).getTime() === new Date(Date.parse('1970-01-01T00:00:00.000Z')).getTime() && false) {
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
        return timestamp;

        var timestamp = d instanceof Date ? '' + [d.getFullYear(), leftPad((d.getMonth() + 1)), leftPad(d.getDate())].join('-') + 'T' + [leftPad(d.getHours()), leftPad(d.getMinutes()), leftPad(d.getSeconds())].join(':') + '.' + leftPad(d.getMilliseconds(), 7) + portalUtcOffset : '';
        //return safeurl === true ? timestamp.replace(/:/g, '-') : timestamp;
        return timestamp;

        return d instanceof Date ? '' + [d.getFullYear(), leftPad((d.getUTCMonth() + 1)), leftPad(d.getUTCDate())].join('-') + 'T' + [leftPad(d.getUTCHours()), leftPad(d.getUTCMinutes()), leftPad(d.getUTCSeconds())].join(':') + '.' + leftPad(d.getMilliseconds(), 7) + 'Z' : '';
        return JSON.stringify(d);
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
                return dayshortnames[date.getMonth()];
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

        var hours = date.getHours(),
            amhours = hours > 12 ? hours - 12 : hours === 0 ? 12 : hours,
            output = '',
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
        var displaydate = date ? date.toLocaleTimeString() : '';
        if (date && formatTime) {
            displaydate = formattingDate(date, formatTime, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return displaydate;
    };

    var getDisplayDate = function(date) {
        var displaydate = date ? date.toLocaleDateString() : '';
        if (date && formatDate) {
            displaydate = formattingDate(date, formatDate, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return displaydate;
    };

    var getDisplayDatetime = function(date) {
        var displaydate = date ? date.toLocaleTimeString() + ' ' + date.toLocaleDateString() : '';
        if (date && formatDatetime) {
            displaydate = formattingDate(date, formatDatetime, dayShortNames, dayNames, monthShortNames, monthNames);
        }
        return displaydate;
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
            userName: o.userName || '',
            firstName: o.firstName || '',
            lastName: o.lastName || '',
            displayName: displayname || '',
            email: o.email || '',
            tel: contacts.telephones.length > 0 ? contacts.telephones[0].val : '',
            contacts: contacts,
            avatar: o.avatar || o.avatarSmall || defaultAvatar,
            avatarSmall: o.avatarSmall || defaultAvatarSmall,
            group: createGroup(o.groups && o.groups.length > 0 ? o.groups[0] || null : null),
            groups: createGroups(o.groups || []),
            status: o.status || 0,
            activationStatus: o.activationStatus || 0,
            isActivated: activationStatuses.activated.id === (o.activationStatus || 0),
            isOnline: typeof(o.isOnline) != "undefined" ? o.isOnline : '',
            isPending: activationStatuses.pending.id === (o.activationStatus || 0),
            isTerminated: profileStatuses.terminated.id === (o.status || 0),
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
            profileUrl: o.profileUrl || ''
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

        return {
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
                availableUsersCount: response.availableUsersCount
            };

            return portalQuotas;
        },

        comment: function(response) {
            return createComment(response);
        },

        comments: function(response) {
            return createCommentsTree(response);
        },

        searchentryitems: function(response) {
            return collection(response, null, function(response) {
                var type = getSearchEntityTypeById(response.entityType);
                switch (type) {
                    case 'file':
                        return factories.doc.file(response);
                    case 'task':
                    case 'subtask':
                        return factories.prj.task(response);
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
        item: function(response) {
            var createdBy = createPerson(response.createdBy || response.author),
                responsible = createPerson(response.responsible ? response.responsible : (response.responsibles && response.responsibles.length > 0 ? response.responsibles[0] : null)),
                crtdate = serializeDate(response.created),
                uptdate = serializeDate(response.updated);

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
                createdBy: createdBy,
                updatedBy: createPerson(response.updatedBy),
                responsible: responsible,
                responsibles: createPersons(response.responsibles || response.responsible),
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
                tomorrowdate = new Date();

            todaydate = new Date(todaydate.getFullYear(), todaydate.getMonth(), todaydate.getDate(), 0, 0, 0, 0);
            tomorrowdate = new Date(tomorrowdate.getFullYear(), tomorrowdate.getMonth(), tomorrowdate.getDate() + 1, 0, 0, 0, 0);


            return extend(this.item(response), {
                type: 'task',
                projectId: response.hasOwnProperty('projectOwner') ? response.projectOwner.id : -1,
                projectTitle: response.hasOwnProperty('projectOwner') ? response.projectOwner.title : '',
                projectOwner: response.projectOwner,
                canCreateSubtask: response.canCreateSubtask,
                canCreateTimeSpend: response.canCreateTimeSpend,
                canDelete: response.canDelete,
                deadline: dlndate,
                displayDeadline: getDisplayDatetime(dlndate),
                displayDateDeadline: getDisplayDate(dlndate),
                displayTimeDeadline: getDisplayTime(dlndate),
                startDate: startdate,
                displayStartDate: getDisplayDatetime(startdate),
                displayDateStart: getDisplayDate(startdate),
                displayTimeStart: getDisplayTime(startdate),
                status: response.status,
                statusname: getTaskStatusName(response.status),
                priority: response.priority,
                subtasks: factories.prj.subtasks(response.subtasks),
                progress: response.hasOwnProperty('progress') ? response.progress : 0,
                milestoneId: response.milestoneId,
                milestoneTitle: response.hasOwnProperty('milestone') && response.milestone ? response.milestone.title : '',
                milestone: response.hasOwnProperty('milestone') ? factories.prj.milestone(response.milestone) : null,
                deadlineToday: dlndate ? dlndate.getTime() >= todaydate.getTime() && dlndate.getTime() < tomorrowdate.getTime() : false,
                isOpened: response.status == taskStatuses.open.id,
                isExpired: response.isExpired || false,
                links: response.hasOwnProperty('links') ? factories.prj.links(response.links) : null
            });
        },

        simpletask: function(response) {
            var dlndate = serializeDate(response.deadline, false),
                startdate = serializeDate(response.startDate, false),
                todaydate = new Date(),
                tomorrowdate = new Date();

            todaydate = new Date(todaydate.getFullYear(), todaydate.getMonth(), todaydate.getDate(), 0, 0, 0, 0);
            tomorrowdate = new Date(tomorrowdate.getFullYear(), tomorrowdate.getMonth(), tomorrowdate.getDate() + 1, 0, 0, 0, 0);


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
                status: response.status,
                statusname: getTaskStatusName(response.status),
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
                tomorrowdate = new Date();

            todaydate = new Date(todaydate.getFullYear(), todaydate.getMonth(), todaydate.getDate(), 0, 0, 0, 0);
            tomorrowdate = new Date(tomorrowdate.getFullYear(), tomorrowdate.getMonth(), tomorrowdate.getDate() + 1, 0, 0, 0, 0);

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

        discussion: function(response) {
            return extend(this.item(response), {
                type: 'discussion',
                projectId: response.hasOwnProperty('projectOwner') ? response.projectOwner.id : -1,
                projectTitle: response.hasOwnProperty('projectOwner') ? response.projectOwner.title : '',
                parentId: null,
                comments: null,
                text: response.text || '',
                status: response.status
            });
        },

        project: function(response) {
            return extend(this.item(response), {
                type: 'project',
                responsibleId: response.responsible.id ? response.responsible.id : response.responsible,
                status: response.status,
                taskCount: response.taskCount,
                milestoneCount: response.milestoneCount,
                participantCount: response.participantCount,
                canCreateMessage: response.hasOwnProperty('security') ? response.security.canCreateMessage : '',
                canCreateMilestone: response.hasOwnProperty('security') ? response.security.canCreateMilestone : '',
                canCreateTask: response.hasOwnProperty('security') ? response.security.canCreateTask : '',
                isInTeam: response.hasOwnProperty('security') ? response.security.isInTeam : '',
                canLinkContact: response.hasOwnProperty('security') ? response.security.canLinkContact : null
            });
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

        subtasks: function(response) {
            return collection(response, this.item, function(response) {
                return {
                    type: 'subtask',
                    status: response.status
                };
            });
        },

        tasks: function(response) {
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
                return {
                    type: 'discussion',
                    projectId: response.hasOwnProperty('projectOwner') ? response.projectOwner.id : -1,
                    projectTitle: response.hasOwnProperty('projectOwner') ? response.projectOwner.title : '',
                    text: response.text || '',
                    commentsCount: response.commentsCount
                };
            });
        },

        projects: function(response) {
            return collection(response, this.item, function(response) {
                return factories.prj.project(response);
            });
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
            person.department = response.department || "";
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
                paymentStatus: response.paymentStatus,
                statusChanged: getDisplayDate(statusChangedDate),
                canEditPaymentStatus: response.canEditPaymentStatus,
                relatedProject: response.relatedProject,
                relatedTask: response.relatedTask,
                relatedTaskTitle: response.relatedTaskTitle,
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
                var commentDateStr = comment.Date.slice(0, -1) + ".0+" + ASC.Resources.Master.CurrentTenantUtcOffset;
                var commentDate = serializeDate(commentDateStr);

                var offsetHours = parseInt(ASC.Resources.Master.CurrentTenantUtcHoursOffset);
                var offsetMinutes = parseInt(ASC.Resources.Master.CurrentTenantUtcMinutesOffset);
                if (!isNaN(offsetHours)) {
                    commentDate.setHours(commentDate.getHours() + offsetHours);
                    commentDate.setMinutes(commentDate.getMinutes() + offsetMinutes);
                }

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

        file: function(response) {
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
                viewUrl: fixUrl(response.viewUri || response.viewUrl || ''),
                fileUrl: fixUrl(response.fileUri || response.fileUrl || ''),
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

        facebookprofile: function(response) {
            var postedOn = serializeDate(response.postedOn);
            return {
                type: "facebookprofile",
                userID: response.userID,
                userName: response.userName,
                smallImageUrl: response.smallImageUrl
            };
        },

        facebookprofiles: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.facebookprofile(response);
            });
        },

        linkedinprofile: function(response) {
            var postedOn = serializeDate(response.postedOn);
            return {
                type: "linkedinprofile",
                userID: response.userID,
                userName: response.userName,
                firstName: response.firstName,
                lastName: response.lastName,
                companyName: response.companyName,
                imageUrl: response.imageUrl,
                position: response.position,
                publicProfileUrl: response.publicProfileUrl
            };
        },

        linkedinprofiles: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.linkedinprofile(response);
            });
        },

        progressitem: function(response) {
            return response;
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
                tmpDate = new Date(),
                today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),
                debtor = response.status.id == 2 && dueDate < today;

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
                quantity: response.quantity,
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

        crunchbaseinfo: function(response) {
            return response;
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
                recordUrl: "",
                recordDuration: 0,
                contact: response.hasOwnProperty('contact') && response.contact ? factories.crm.contact(response.contact) : null,
                history: response.history
            };
        },

        voipUploads: function(response) {
            return collection(response, null, function(response) {
                return factories.crm.voipUpload(response);
            });
        },

        voipUpload: function(response) {
            return {
                path: response.path,
                name: response.name,
                audioType: response.audioType,
            };
        },

        converterData: function(response) {
            return {
                type: 'converterData',
                converterUrl: response.converterUrl,
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