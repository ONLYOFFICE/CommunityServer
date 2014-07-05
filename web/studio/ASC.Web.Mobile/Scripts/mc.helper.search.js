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


;window.SrchHelper = (function () {
  var
    isInit = false,
    customEvents = {
      found : 'onfound'
    },
    eventManager = new CustomEvent(customEvents);

  var init = function () {
    if (isInit) {
      return undefined;
    }
    isInit = true;

    //TODO:
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  function onFound (params, blogs, topics, events, bookmarks, profiles, projects, contacts, documents) {
    var data = [
      {title : ASC.Resources.LblCommunityTitle, type : 'commitems', items : [].concat(blogs, topics, events, bookmarks)},
      {title : ASC.Resources.LblPeopleTitle, type : 'peopitems', items : [].concat(profiles)},
      {title : ASC.Resources.LblProjectsTitle, type : 'projitems', items : [].concat(Teamlab.getProjectsSearchEntries(projects))},
      {title : ASC.Resources.LblCrmTitle, type : 'crmitems', items : [].concat(contacts)}
      //{title : ASC.Resources.LblDocumentsTitle, type : 'docsitems', items : [].concat(documents)}
    ];

    params = (params && params.length ? params[params.length - 1] : {});

    if (params.hasOwnProperty('query')) {
      eventManager.call(customEvents.found, window, [data, params.query, params]);
    }
  }

  var search = function (query) {
    var
      fn = null,
      container = null,
      filter = {filterValue: query};

    Teamlab.joint()
      .getCmtBlogs(null, {query : query})
      .getCmtForumTopics(null, {query : query})
      .getCmtEvents(null, {query : query})
      .getCmtBookmarks(null, {query : query})
      .getProfiles(null, {query : query})
      .getPrjProjects(null, {query : query})
      .getCrmContacts(null,{filter: filter})
      .start({query : query}, onFound);
  };

  return {
    events  : customEvents,

    init  : init,

    bind    : bind,
    unbind  : unbind,

    search  : search
  };
})();
