#!/bin/bash

APP_SERVICES_ROOT_DIR="/var/www/onlyoffice/Services"
APP_CONFIG_DIR="/etc/onlyoffice/communityserver"
NGINX_ROOT_DIR="/etc/nginx";
NGINX_CONF_DIR="/etc/nginx/sites-enabled";

cp $NGINX_ROOT_DIR/includes/onlyoffice-communityserver-common.conf.template default-onlyoffice.conf;

sed 's/{{APP_NIGNX_KEEPLIVE}}/64/g' -i default-onlyoffice.conf;
sed 's_\(\"DefaultApiSchema":\).*,_\1 "http",_' -i ${APP_CONFIG_DIR}/mail.production.json

mv -f default-onlyoffice.conf ${NGINX_CONF_DIR}/onlyoffice

sed '/certificate"/s!\(value\s*=\s*\"\).*\"!\1\"!' -i ${APP_SERVICES_ROOT_DIR}/Jabber/ASC.Xmpp.Server.Launcher.exe.config
sed '/certificatePassword/s/\(value\s*=\s*\"\).*\"/\1\"/' -i ${APP_SERVICES_ROOT_DIR}/Jabber/ASC.Xmpp.Server.Launcher.exe.config
sed '/startTls/s/\(value\s*=\s*\"\).*\"/\1none\"/' -i ${APP_SERVICES_ROOT_DIR}/Jabber/ASC.Xmpp.Server.Launcher.exe.config

service onlyofficeJabber restart
service onlyofficeMailAggregator restart
service nginx reload
