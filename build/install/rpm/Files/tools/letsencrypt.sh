#!/bin/bash

_domains="";

for arg; do
    _domains="$_domains -d $arg";
done

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

certbot certonly --expand --webroot -w /var/www/onlyoffice/Data/certs --noninteractive --agree-tos --email support@$1 $_domains;

cp /etc/letsencrypt/live/$1/fullchain.pem /var/www/onlyoffice/Data/certs/onlyoffice.crt
cp /etc/letsencrypt/live/$1/privkey.pem /var/www/onlyoffice/Data/certs/onlyoffice.key
cp /etc/letsencrypt/live/$1/chain.pem /var/www/onlyoffice/Data/certs/stapling.trusted.crt

cat > /etc/cron.d/letsencrypt <<END
@weekly root certbot renew >> /var/log/le-renew.log
@weekly root cp /etc/letsencrypt/live/$1/fullchain.pem /var/www/onlyoffice/Data/certs/onlyoffice.crt
@weekly root cp /etc/letsencrypt/live/$1/privkey.pem /var/www/onlyoffice/Data/certs/onlyoffice.key
@weekly root cp /etc/letsencrypt/live/$1/chain.pem /var/www/onlyoffice/Data/certs/stapling.trusted.crt
@weekly root nginx reload
END

source $DIR/default-onlyoffice-ssl.sh
