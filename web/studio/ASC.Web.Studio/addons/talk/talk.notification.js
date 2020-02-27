/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


var port;

self.addEventListener('install', function (event) {
    event.waitUntil(self.skipWaiting()); 
});

self.addEventListener('activate', function (event) {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('push', function (event) {

    var obj = JSON.parse(event.data.text());
    fireNotification(obj.data, event);
	
});
self.onmessage = function (e) {
    port = e.ports[0];
};

self.addEventListener('notificationclick', function (event) {
 
    event.notification.close();
    
    event.waitUntil(
      self.clients.matchAll({
          type: 'window',
          includeUncontrolled: true
      })
      .then(function (clientList) {
          var url = "jabberclient.aspx";
           for (var i = 0; i < clientList.length; i++) {
               var client = clientList[i];
               var clientUrl = client.url.split("/");
               if (clientUrl[clientUrl.length-1] == url && 'focus' in client)
                   return client.focus();
             }
           if (clients.openWindow) {
               return clients.openWindow(url);
           }
      })
    );
});

function fireNotification(obj, event) {
    var title = unescape(obj.fromFullName);
    var body = obj.msg;
    var tag = 'simple-push-notification-tag';
    var photo;
    if ((obj.photoPath.indexOf('http://') != -1 || obj.photoPath.indexOf('https://') != -1) && obj.photoPath != '') {
        photo = obj.photoPath;
    } else if (obj.photoPath != '') {
        photo = event.target.origin != '' ? event.target.origin + obj.photoPath : "http://download.onlyoffice.com/assets/logo/emptyuser-48.png";
    } else {
        photo = "http://download.onlyoffice.com/assets/logo/emptyuser-48.png";
    }
    event.waitUntil(self.registration.showNotification(title, {
        icon: photo,
		title:title,
		body: body,
        tag: tag
	}));
}