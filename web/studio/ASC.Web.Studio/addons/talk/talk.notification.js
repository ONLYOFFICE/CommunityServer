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