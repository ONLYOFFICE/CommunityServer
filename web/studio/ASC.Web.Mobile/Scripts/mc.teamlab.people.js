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


;window.TeamlabMobile = (function (TeamlabMobile) {
  if (!TeamlabMobile) {
    console.log('Teamlab.people: has no TeamlabMobile');
    return undefined;
  }

  var
    templateIds = {
      
      pgpeople : 'template-page-people',
      pgpeopitem : 'template-page-person'
    },
    staticAnchors = {
      
      my : 'my',
      profile : 'people/'
    },
    anchorRegExp = {
      
      my : /^my$/,
      people : /^people[\/]*$/,
      profile : /^people\/([\w\d\.-]+)$/
    },
    customEvents = {
      changePage : 'onchangepage',
      addComment: 'onaddcomment',
      loadComments: 'onloadcomments',
      
      myPage : 'onmypage',
      peoplePage : 'onpeoplepage',
      profilePage : 'onprofilepage'
    },
    eventManager = TeamlabMobile.extendEventManager(customEvents),
    dialogMarkCollection = [
      
    ];

  
  TeamlabMobile.extendModule(templateIds, anchorRegExp, staticAnchors, dialogMarkCollection);

  
  ASC.Controls.AnchorController.bind(anchorRegExp.people, onPeopleAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.profile, onProfileAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.my, onMyAnch);

  
  function onMyAnch (params) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getProfile(null, '@self', onGetProfile);
  }

  function onProfileAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getProfile(null, id, onGetProfile);
  }

  function onPeopleAnch (params) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getProfiles(null, onGetPeople);
  }

  function onGetProfile (params, item) {
    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.my) || ASC.Controls.AnchorController.testAnchor(anchorRegExp.profile)) {
      eventManager.call(customEvents.profilePage, window, [item]);
    }
  }

  function onGetPeople (params, items) {
    var
      o = {},
      indexes = [],
      item = null,
      itemsInd = items.length;

    while (itemsInd--) {
      item = items[itemsInd];
      if (!o.hasOwnProperty(item.index)) {
        o[item.index] = [];
      }
      o[item.index].unshift(item);
    }

    var indexes = [];
    for (var fld in o) {
      if (o.hasOwnProperty(fld)) {
        indexes.push({index : fld, items : o[fld]});
      }
    }
    indexes.sort(TeamlabMobile.ascSortByIndex);

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.people)) {
      eventManager.call(customEvents.peoplePage, window, [indexes]);
    }
  }

  return TeamlabMobile;
})(TeamlabMobile);
