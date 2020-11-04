#!/bin/bash

set -e

LETSENCRYPT_ROOT_DIR="/etc/letsencrypt";
ROOT_DIR="/var/www/onlyoffice/Data/certs";

_domains="";

for arg; do
    _domains="$_domains -d $arg";
done

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

mkdir -p ${ROOT_DIR}

certbot certonly --expand --webroot -w ${ROOT_DIR} --noninteractive --agree-tos --email support@$1 $_domains;

cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/fullchain.pem ${ROOT_DIR}/onlyoffice.crt
cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/privkey.pem ${ROOT_DIR}/onlyoffice.key
cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/chain.pem ${ROOT_DIR}/stapling.trusted.crt

cat > ${LETSENCRYPT_ROOT_DIR}/renewal-hooks/deploy/communityserver.sh <<END
#!/bin/bash

cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/fullchain.pem ${ROOT_DIR}/onlyoffice.crt
cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/privkey.pem ${ROOT_DIR}/onlyoffice.key
cp -f ${LETSENCRYPT_ROOT_DIR}/live/$1/chain.pem ${ROOT_DIR}/stapling.trusted.crt

service nginx reload

END

chmod a+x ${LETSENCRYPT_ROOT_DIR}/renewal-hooks/deploy/communityserver.sh

source $DIR/default-onlyoffice-ssl.sh

