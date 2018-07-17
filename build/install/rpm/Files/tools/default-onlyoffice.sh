#!/bin/bash

ONLYOFFICE_SERVICES_DIR="/var/www/onlyoffice/Services"
NGINX_CONF_DIR="/etc/nginx/conf.d";

cp /etc/nginx/includes/onlyoffice-communityserver-common.conf.template default-onlyoffice.conf;

sed 's/{{ONLYOFFICE_NIGNX_KEEPLIVE}}/64/g' -i default-onlyoffice.conf;
sed '/mail\.default-api-scheme/s/\(value\s*=\s*\"\).*\"/\1http\"/' -i /var/www/onlyoffice/Services/MailAggregator/ASC.Mail.Aggregator.CollectionService.exe.config;

mv default-onlyoffice.conf ${NGINX_CONF_DIR}/onlyoffice

sed '/certificate"/s!\(value\s*=\s*\"\).*\"!\1\"!' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config
sed '/certificatePassword/s/\(value\s*=\s*\"\).*\"/\1\"/' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config
sed '/startTls/s/\(value\s*=\s*\"\).*\"/\1none\"/' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config;

service onlyofficeJabber restart
service onlyofficeMailAggregator restart
service nginx reload
