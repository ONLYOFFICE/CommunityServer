#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="/var/www/onlyoffice/Data/certs";
LETSENCRYPT_ROOT_DIR="/etc/letsencrypt";

certbot certonly --expand --webroot -w ${ROOT_DIR} --noninteractive --agree-tos --email support@$1 $1;

mkdir -p ${ROOT_DIR}

cat > ${LETSENCRYPT_ROOT_DIR}/renewal-hooks/deploy/mailserver.sh <<END
#!/bin/bash

cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/fullchain.pem ${ROOT_DIR}/mail.onlyoffice.crt
cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/privkey.pem ${ROOT_DIR}/mail.onlyoffice.key

service nginx reload

END

chmod a+x ${LETSENCRYPT_ROOT_DIR}/renewal-hooks/deploy/mailserver.sh
