#!/bin/bash

ROOT_DIR="/var/www/onlyoffice/Data/certs";
NGINX_CONF_DIR="{{NGINX_CONF_DIR}}";
NGINX_ROOT_DIR="/etc/nginx";
APP_SERVICES_ROOT_DIR="/var/www/onlyoffice/Services"
SSL_OCSP_CERTIFICATE_PATH="${ROOT_DIR}/stapling.trusted.crt";

SSL_DHPARAM_PATH="${ROOT_DIR}/dhparam.pem";

mkdir -p ${ROOT_DIR}

if [ ! -f ${SSL_DHPARAM_PATH} ]; then
        sudo openssl dhparam -out dhparam.pem 2048

        mv dhparam.pem ${SSL_DHPARAM_PATH};
fi

SSL_CERTIFICATE_PATH="${ROOT_DIR}/onlyoffice.crt"
SSL_KEY_PATH="${ROOT_DIR}/onlyoffice.key"
SSL_CERTIFICATE_PATH_PFX="${ROOT_DIR}/onlyoffice.pfx"
SSL_CERTIFICATE_PATH_PFX_PWD="onlyoffice";

openssl pkcs12 -export -out ${SSL_CERTIFICATE_PATH_PFX} -inkey ${SSL_KEY_PATH} -in ${SSL_CERTIFICATE_PATH} -password pass:${SSL_CERTIFICATE_PATH_PFX_PWD};

chown -R onlyoffice:onlyoffice ${ROOT_DIR}

DOCKER_ENABLED=false;

if cat /proc/1/cgroup | grep -qE "docker|lxc"; then
	DOCKER_ENABLED=true;
fi

DOCKER_APP_SUBNET=$(ip -o -f inet addr show | awk '/scope global/ {print $4}' | head -1);

cp ${NGINX_ROOT_DIR}/includes/onlyoffice-communityserver-common-ssl.conf.template default-onlyoffice-ssl.conf;

sed "s,{{SSL_CERTIFICATE_PATH}},${SSL_CERTIFICATE_PATH}," -i default-onlyoffice-ssl.conf;
sed "s,{{SSL_KEY_PATH}},${SSL_KEY_PATH}," -i default-onlyoffice-ssl.conf;
sed "s,{{SSL_DHPARAM_PATH}},${SSL_DHPARAM_PATH}," -i default-onlyoffice-ssl.conf;
sed 's,{{SSL_VERIFY_CLIENT}},off,' -i default-onlyoffice-ssl.conf;
sed '/{{CA_CERTIFICATES_PATH}}/d' -i default-onlyoffice-ssl.conf;
sed 's/{{APP_HTTPS_HSTS_MAXAGE}}/63072000/' -i default-onlyoffice-ssl.conf;

if [ "${DOCKER_ENABLED}" = "true" ] && [ ${DOCKER_APP_SUBNET} ]; then
	sed 's,{{DOCKER_APP_SUBNET}},'"${DOCKER_APP_SUBNET}"',' -i default-onlyoffice-ssl.conf;
else
	sed '/{{DOCKER_APP_SUBNET}}/d' -i default-onlyoffice-ssl.conf;
fi

sed 's/{{APP_NIGNX_KEEPLIVE}}/64/g' -i default-onlyoffice-ssl.conf;


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

sed '/certificate"/s!\(value\s*=\s*\"\).*\"!\1'${SSL_CERTIFICATE_PATH_PFX}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.Config
sed '/certificatePassword/s/\(value\s*=\s*\"\).*\"/\1'${SSL_CERTIFICATE_PATH_PFX_PWD}'\"/' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.Config
sed '/startTls/s/\(value\s*=\s*\"\).*\"/\1optional\"/' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.Config;

sed '/mail\.default-api-scheme/s/\(value\s*=\s*\"\).*\"/\1https\"/' -i ${APP_SERVICES_ROOT_DIR}/MailAggregator/ASC.Mail.Aggregator.CollectionService.exe.config;

mv -f default-onlyoffice-ssl.conf ${NGINX_CONF_DIR}/onlyoffice.conf

service onlyofficeMailAggregator restart
service onlyofficeJabber restart
service nginx restart
