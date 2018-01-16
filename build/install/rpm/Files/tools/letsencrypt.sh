#!/bin/bash

set -e

_domains="";

for arg; do
    _domains="$_domains -d $arg";
done

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="/var/www/onlyoffice/Data/certs";

certbot certonly --expand --webroot -w ${ROOT_DIR} --noninteractive --agree-tos --email support@$1 $_domains;

cp /etc/letsencrypt/live/$1/fullchain.pem ${ROOT_DIR}/onlyoffice.crt
cp /etc/letsencrypt/live/$1/privkey.pem ${ROOT_DIR}/onlyoffice.key
cp /etc/letsencrypt/live/$1/chain.pem ${ROOT_DIR}/stapling.trusted.crt

cat > ${DIR}/letsencrypt_cron.sh <<END
certbot renew >> /var/log/le-renew.log
cp /etc/letsencrypt/live/$1/fullchain.pem ${ROOT_DIR}/onlyoffice.crt
cp /etc/letsencrypt/live/$1/privkey.pem ${ROOT_DIR}/onlyoffice.key
cp /etc/letsencrypt/live/$1/chain.pem ${ROOT_DIR}/stapling.trusted.crt
openssl pkcs12 -export -out ${ROOT_DIR}/onlyoffice.pfx -inkey ${ROOT_DIR}/onlyoffice.key -in ${ROOT_DIR}/onlyoffice.crt -password pass:onlyoffice
chown onlyoffice:onlyoffice ${ROOT_DIR}/onlyoffice.pfx
service nginx reload
END

chmod a+x ${DIR}/letsencrypt_cron.sh

cat > /etc/cron.d/letsencrypt <<END
@weekly root ${DIR}/letsencrypt_cron.sh
END

source $DIR/default-onlyoffice-ssl.sh
