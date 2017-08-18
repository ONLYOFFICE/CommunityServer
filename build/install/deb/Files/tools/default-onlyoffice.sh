#!/bin/bash

cp /etc/nginx/includes/onlyoffice-communityserver-common.conf.template default-onlyoffice.conf;

sed 's/{{ONLYOFFICE_NIGNX_KEEPLIVE}}/64/g' -i default-onlyoffice.conf;
sed '/mail\.default-api-scheme/s/\(value\s*=\s*\"\).*\"/\1http\"/' -i /var/www/onlyoffice/Services/MailAggregator/ASC.Mail.Aggregator.CollectionService.exe.config;

mv default-onlyoffice.conf /etc/nginx/sites-enabled/onlyoffice

service onlyofficeMailAggregator restart
service nginx reload

