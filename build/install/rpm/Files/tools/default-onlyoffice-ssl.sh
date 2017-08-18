#!/bin/bash

if [ ! -f /var/www/onlyoffice/Data/certs/dhparam.pem ]; then
        sudo openssl dhparam -out dhparam.pem 2048

        mv dhparam.pem /var/www/onlyoffice/Data/certs/dhparam.pem;
fi

DOCKER_ONLYOFFICE_SUBNET=$(ip -o -f inet addr show | awk '/scope global/ {print $4}');

cp /etc/nginx/includes/onlyoffice-communityserver-common-ssl.conf.template default-onlyoffice-ssl.conf;

SSL_CERTIFICATE_PATH="/var/www/onlyoffice/Data/certs/onlyoffice.crt"
SSL_KEY_PATH="/var/www/onlyoffice/Data/certs/onlyoffice.key"
ONLYOFFICE_SERVICES_DIR="/var/www/onlyoffice/Services"

sed "s,{{SSL_CERTIFICATE_PATH}},${SSL_CERTIFICATE_PATH}," -i default-onlyoffice-ssl.conf;
sed "s,{{SSL_KEY_PATH}},${SSL_KEY_PATH}," -i default-onlyoffice-ssl.conf;
sed 's,{{SSL_DHPARAM_PATH}},/var/www/onlyoffice/Data/certs/dhparam.pem,' -i default-onlyoffice-ssl.conf;
sed 's,{{SSL_VERIFY_CLIENT}},off,' -i default-onlyoffice-ssl.conf;
sed '/{{CA_CERTIFICATES_PATH}}/d' -i default-onlyoffice-ssl.conf;
sed 's/{{ONLYOFFICE_HTTPS_HSTS_MAXAGE}}/63072000/' -i default-onlyoffice-ssl.conf;
sed 's,{{DOCKER_ONLYOFFICE_SUBNET}},'"${DOCKER_ONLYOFFICE_SUBNET}"',' -i default-onlyoffice-ssl.conf;
sed 's/{{ONLYOFFICE_NIGNX_KEEPLIVE}}/64/g' -i default-onlyoffice-ssl.conf;

SSL_OCSP_CERTIFICATE_PATH="/var/www/onlyoffice/Data/certs/stapling.trusted.crt"

# if dhparam path is valid, add to the config, otherwise remove the option
if [ -r "${SSL_OCSP_CERTIFICATE_PATH}" ]; then
        sed 's,{{SSL_OCSP_CERTIFICATE_PATH}},'"${SSL_OCSP_CERTIFICATE_PATH}"',' -i default-onlyoffice-ssl.conf;
else
        sed '/ssl_stapling/d' -i default-onlyoffice-ssl.conf;
        sed '/ssl_stapling_verify/d' -i default-onlyoffice-ssl.conf;
        sed '/ssl_trusted_certificate/d' -i default-onlyoffice-ssl.conf;
        sed '/resolver/d' -i default-onlyoffice-ssl.conf;
        sed '/resolver_timeout/d' -i default-onlyoffice-ssl.conf;
fi

# sed '/certificate/s/\(value\s*=\s*\"\).*\"/\1${SSL_CERTIFICATE_PATH}"\"/' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config
# sed '/certificatePrivateKey/s/\(value\s*=\s*\"\).*\"/\1${SSL_KEY_PATH}"\"/' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config;
# sed '/startTls/s/\(value\s*=\s*\"\).*\"/\1optional"\"/' -i ${ONLYOFFICE_SERVICES_DIR}/TeamLabSvc/TeamLabSvc.exe.Config;

sed '/mail\.default-api-scheme/s/\(value\s*=\s*\"\).*\"/\1https\"/' -i /var/www/onlyoffice/Services/MailAggregator/ASC.Mail.Aggregator.CollectionService.exe.config;

mv default-onlyoffice-ssl.conf /etc/nginx/sites-enabled/onlyoffice

service onlyofficeMailAggregator restart
# service onlyofficeJabber restart
service nginx reload
