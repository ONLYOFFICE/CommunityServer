#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="/var/www/onlyoffice/Data/certs";

certbot certonly --expand --webroot -w ${ROOT_DIR} --noninteractive --agree-tos --email support@$1 $1;

cp /etc/letsencrypt/live/$1/fullchain.pem ${ROOT_DIR}/mail.onlyoffice.crt
cp /etc/letsencrypt/live/$1/privkey.pem ${ROOT_DIR}/mail.onlyoffice.key

cat > ${DIR}/letsencrypt_mail_cron.sh <<END
certbot renew >> /var/log/le-renew.log
cp /etc/letsencrypt/live/$1/fullchain.pem ${ROOT_DIR}/mail.onlyoffice.crt
cp /etc/letsencrypt/live/$1/privkey.pem ${ROOT_DIR}/mail.onlyoffice.key
END

chmod a+x ${DIR}/letsencrypt_mail_cron.sh

cat > /etc/cron.d/letsencrypt_mail <<END
@weekly root ${DIR}/letsencrypt_mail_cron.sh
END
