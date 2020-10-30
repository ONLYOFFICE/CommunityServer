Summary: %{package_header_tag_summary}
Name: %{package_header_tag_name}
Version: %{package_header_tag_version}
Release: %{package_header_tag_release}
License: Apache-2.0
Group: Applications/Internet
URL: %{package_header_tag_url}
Vendor: %{package_header_tag_vendor}
Packager: %{package_header_tag_packager}
Requires: %{package_header_tag_requires}
BuildArch: noarch
AutoReq: no
AutoProv: no

%description
%{package_section_description}

%prep
rm -rf "$RPM_BUILD_ROOT"

%build

%install

#install files
mkdir -p "$RPM_BUILD_ROOT/var/www/%{package_sysname}/"
mkdir -p "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Tools/"
mkdir -p "$RPM_BUILD_ROOT/usr/bin/"
cp -r ../../Files/bin/*.sh "$RPM_BUILD_ROOT/usr/bin/"
cp -r ../../Files/onlyoffice/. "$RPM_BUILD_ROOT/var/www/%{package_sysname}/"
cp -r ../../Files/Tools/. "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Tools/"
mkdir -p "$RPM_BUILD_ROOT/var/log/%{package_sysname}/"

if [ -d ../../../../../dev_tools/RadicalePlugins ]; then
	mkdir -p "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Tools/radicale/plugins/"
	cp -Rf ../../../../../dev_tools/RadicalePlugins/* "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Tools/radicale/plugins/"
fi

#install systemd scripts
mkdir -p "$RPM_BUILD_ROOT/usr/lib/systemd/system/"
cp ../../Files/systemd/*.service "$RPM_BUILD_ROOT/usr/lib/systemd/system/"

#install god configs
mkdir -p "$RPM_BUILD_ROOT/etc/god/"
cp -r ../../Files/god/* "$RPM_BUILD_ROOT/etc/god/"

#install nginx config
mkdir -p "$RPM_BUILD_ROOT%{nginx_conf_d}/"
mkdir -p "$RPM_BUILD_ROOT/etc/nginx/includes/"
cp ../../Files/nginx/onlyoffice.conf "$RPM_BUILD_ROOT%{nginx_conf_d}/"
cp ../../Files/nginx/includes/* "$RPM_BUILD_ROOT/etc/nginx/includes/"

#install hyperfastcgi config
mkdir -p "$RPM_BUILD_ROOT/etc/hyperfastcgi/"
cp -r ../../Files/hyperfastcgi/* "$RPM_BUILD_ROOT/etc/hyperfastcgi/"

# rename
find "$RPM_BUILD_ROOT/usr/bin/" \
	 "$RPM_BUILD_ROOT/etc/hyperfastcgi/" \
 	 "$RPM_BUILD_ROOT/etc/god/" \
	 "$RPM_BUILD_ROOT/usr/lib/systemd/system/" \
	 "$RPM_BUILD_ROOT%{nginx_conf_d}/" \
	 "$RPM_BUILD_ROOT/etc/nginx/includes/" \
	 "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Tools/" \
-depth -type f \
-exec sed -i 's/onlyoffice/%{package_sysname}/g' {} \; \
-exec sed -i 's/ONLYOFFICE/%{package_sysname}/g' {} \; \
-exec sed -i 's/{{NGINX_USER}}/%{nginx_user}/g' {} \; \
-exec sed -i 's!{{NGINX_CONF_DIR}}!%{nginx_conf_d}!g' {} \; \
-exec rename -f -v 's/onlyoffice([^\/]*)$/%{package_sysname}$1/g' {} \;

find "$RPM_BUILD_ROOT/var/www/%{package_sysname}/Sql" -depth -type f -exec rename -f -v 's/onlyoffice([^\/]*)$/%{package_sysname}$1/g' {} \;

#list files
OLD_IFS="$IFS"
IFS="
"
for FILE in `find "$RPM_BUILD_ROOT/var/www/%{package_sysname}/"`; do
	RELFILE="`echo "$FILE" | sed s?"$RPM_BUILD_ROOT"??`"
	if [ -d "$FILE" ]; then
		echo "%%attr(-, %{package_sysname}, %{package_sysname}) %%dir \"$RELFILE\"" >>onlyoffice.list
	else
		case "$FILE" in
			*/WebStudio/web.connections.config )
				echo "%%attr(-, %{package_sysname}, %{package_sysname}) %%config(noreplace) \"$RELFILE\"" >>onlyoffice.list
			;;

			*/WebStudio/[Ww]eb*.config | */ApiSystem/[Ww]eb*.config | */TeamLabSvc.exe.config | */ASC.Mail.StorageCleaner.exe.config | */ASC.Mail.Aggregator.CollectionService.exe.config | */ASC.Mail.Watchdog.Service.exe.config | */ASC.Mail.EmlDownloader.exe.config )
				echo "%%attr(-, %{package_sysname}, %{package_sysname}) %%config \"$RELFILE\"" >>onlyoffice.list
			;;

			* )
				echo "%%attr(-, %{package_sysname}, %{package_sysname}) \"$RELFILE\"" >>onlyoffice.list
			;;
		esac
	fi
done
IFS="$OLD_IFS"

%clean
rm -rf "$RPM_BUILD_ROOT"

%files -f onlyoffice.list
%attr(-, root, root) /usr/bin/*.sh
%attr(-, %{package_sysname}, %{package_sysname}) %dir /var/log/%{package_sysname}/
%attr(-, root, root) /usr/lib/systemd/system/*.service
%attr(-, root, root) %{nginx_conf_d}/%{package_sysname}.conf
%attr(-, root, root) /etc/nginx/includes/%{package_sysname}-communityserver-*
%attr(-, root, root) /etc/hyperfastcgi/*
%attr(-, root, root) /etc/god/

%pre
getent group %{package_sysname} >/dev/null || groupadd -r %{package_sysname}
getent passwd %{package_sysname} >/dev/null || useradd -r -g %{package_sysname} -d /var/www/%{package_sysname}/ -s /sbin/nologin %{package_sysname}

if [ $1 -ge 2 ]; then
	for SVC in %{package_services}; do
		if [ -e /usr/lib/systemd/system/$SVC.service ]; then
			systemctl stop $SVC
		fi
	done
fi

%triggerin -- %{package_sysname}-controlpanel

DIR=/var/www/%{package_sysname}
NGINX_ROOT_DIR="/etc/nginx";
APP_ROOT_DIR="$DIR/WebStudio";
APP_SERVICES_ROOT_DIR="$DIR/Services";
CONTROL_PANEL_PORT_80_TCP_ADDR="localhost:8082";
SERVICE_SSO_AUTH_HOST_ADDR="localhost";

cp ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf.template ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed 's,{{CONTROL_PANEL_HOST_ADDR}},'"${CONTROL_PANEL_PORT_80_TCP_ADDR}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed 's,{{SERVICE_SSO_AUTH_HOST_ADDR}},'"${SERVICE_SSO_AUTH_HOST_ADDR}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed '/web\.controlpanel\.url/s/\(value\s*=\s*\"\)[^\"]*\"/\1\/controlpanel\/\"/' -i  ${APP_ROOT_DIR}/web.appsettings.config;
sed '/web\.controlpanel\.url/s/\(value\s*=\s*\"\)[^\"]*\"/\1\/controlpanel\/\"/' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;

if systemctl is-active monoserve | grep -q "active"; then
	systemctl restart monoserve
fi

%triggerin -- python3, python36

if ! which python3; then
	if rpm -q python36; then
		update-alternatives --install /usr/bin/python3 python3 /usr/bin/python3.6 1
	fi
fi

DIR="/var/www/%{package_sysname}"

python3 -m pip install --upgrade pip
python3 -m pip install --upgrade requests
python3 -m pip install --upgrade radicale
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_auth_plugin/.
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_store_plugin/.
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_rights_plugin/.

%triggerin -- %{package_sysname}-xmppserver

DIR=/var/www/%{package_sysname}
APP_ROOT_DIR="$DIR/WebStudio";

sed '/web\.talk/s/value=\"\S*\"/value=\"true\"/g' -i  ${APP_ROOT_DIR}/web.appsettings.config
sed '/web\.chat/s/value=\"\S*\"/value=\"true\"/g' -i  ${APP_ROOT_DIR}/web.appsettings.config

if systemctl is-active monoserve | grep -q "active"; then
	systemctl restart monoserve
fi

%triggerin -- %{package_sysname}-documentserver, %{package_sysname}-documentserver-ee

DIR=/var/www/%{package_sysname}
APP_DATA_DIR="${DIR}/Data"
NGINX_ROOT_DIR="/etc/nginx";
APP_ROOT_DIR="$DIR/WebStudio";
APP_SERVICES_ROOT_DIR="$DIR/Services";

SERVER_HOST="localhost";
LICENSE_FILE_PATH="/var/www/%{package_sysname}/Data/license.lic";

DOCUMENT_SERVER_PROTOCOL="http";
DOCUMENT_SERVER_HOST="localhost:8083";
DOCUMENT_SERVER_HOST_PROXY="localhost\/ds-vpath";
DOCUMENT_SERVER_API_URL="\/ds-vpath\/";

DOCUMENT_SERVER_JWT_SECRET_FROM_DS=$(cat  /etc/onlyoffice/documentserver/local.json | jq -r '.services.CoAuthoring.secret.inbox.string')
DOCUMENT_SERVER_JWT_HEADER_FROM_DS=$(cat  /etc/onlyoffice/documentserver/local.json | jq -r '.services.CoAuthoring.token.inbox.header')
DOCUMENT_SERVER_JWT_SECRET=${JWT_SECRET:-${DOCUMENT_SERVER_JWT_SECRET_FROM_DS:-"%{package_sysname}"}};
DOCUMENT_SERVER_JWT_HEADER=${JWT_HEADER:-${DOCUMENT_SERVER_JWT_HEADER_FROM_DS:-"AuthorizationJwt"}};

cp ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf.template ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf;

sed 's,{{DOCUMENT_SERVER_HOST_ADDR}},'"${DOCUMENT_SERVER_PROTOCOL}:\/\/${DOCUMENT_SERVER_HOST}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf;

sed '/files\.docservice\.url\.internal/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_PROTOCOL}':\/\/'${DOCUMENT_SERVER_HOST}'\"!' -i  ${APP_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.url\.public/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_API_URL}'\"!' -i ${APP_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.url\.portal/s!\(value\s*=\s*\"\)[^\"]*\"!\1http:\/\/'${SERVER_HOST}'\"!' -i ${APP_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.secret/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_SECRET}'\"!' -i ${APP_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.secret.header/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_HEADER}'\"!' -i ${APP_ROOT_DIR}/web.appsettings.config;

sed '/files\.docservice\.url\.internal/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_PROTOCOL}':\/\/'${DOCUMENT_SERVER_HOST}'\"!' -i  ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.url\.public/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_API_URL}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.url\.portal/s!\(value\s*=\s*\"\)[^\"]*\"!\1http:\/\/'${SERVER_HOST}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.secret/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_SECRET}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.secret.header/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_HEADER}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;

if rpm -q %{package_sysname}-documentserver-ee; then
	sed '/license\.file\.path/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${LICENSE_FILE_PATH}'\"!' -i ${APP_ROOT_DIR}/web.appsettings.config
	sed '/license\.file\.path/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${LICENSE_FILE_PATH}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config	

	if [ ! -f ${LICENSE_FILE_PATH} ]; then
		MYSQL_SERVER_HOST=$(grep -oP "Server=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_DB_NAME=$(grep -oP "Database=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_USER=$(grep -oP "User ID=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_PASS=$(grep -oP "Password=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
	
		mysql --silent -h ${MYSQL_SERVER_HOST} -u ${MYSQL_SERVER_USER} --password=${MYSQL_SERVER_PASS} -D "$MYSQL_SERVER_DB_NAME" <<EOF || true
INSERT IGNORE INTO tenants_quota (tenant, name, max_file_size, max_total_size, active_users, features) \
SELECT -1000, 'start_trial', max_file_size, max_total_size, active_users, CONCAT(features, ',trial')
FROM tenants_quota
WHERE tenant = -1;
INSERT IGNORE INTO tenants_tariff (id, tenant, tariff, stamp) VALUES ('1000','-1', '-1000', NOW() + INTERVAL 30 DAY);
EOF

	fi
fi


if systemctl is-active monoserve | grep -q "active"; then
	systemctl restart monoserve
fi

%post
set -e

DIR=/var/www/%{package_sysname}
SERVICES_DIR="${DIR}/Services"
APP_DATA_DIR="${DIR}/Data"

mkdir -p "$APP_DATA_DIR"

if [ "$(ls -alhd ${APP_DATA_DIR} | awk '{ print $3 }')" != "%{package_sysname}" ]; then
	  chown %{package_sysname}:%{package_sysname} ${APP_DATA_DIR}
fi

ELASTIC_SEARCH_VERSION=$(rpm -qi elasticsearch | grep Version | tail -n1 | awk -F': ' '/Version/ {print $2}');
ELASTIC_SEARCH_CONF_PATH="/etc/elasticsearch/elasticsearch.yml"
ELASTIC_SEARCH_JAVA_CONF_PATH="/etc/elasticsearch/jvm.options";

APP_INDEX_DIR="${APP_DATA_DIR}/Index/v${ELASTIC_SEARCH_VERSION}"
LOG_DIR="/var/log/%{package_sysname}"

#import common ssl certificates
mozroots --import --sync --machine --quiet
mkdir -p /etc/mono/registry/LocalMachine
mkdir -p /usr/share/.mono/keypairs
mkdir -p /var/cache/nginx/%{package_sysname}
mkdir -p /var/run/%{package_sysname}
mkdir -p -m 700 /var/run/%{package_sysname}/.config/.mono/keypairs
chown %{nginx_user}:%{nginx_user} /var/cache/nginx/%{package_sysname}
chown %{package_sysname}:%{nginx_user} /var/run/%{package_sysname}
chmod g+s+w /var/run/%{package_sysname}
chmod +x /usr/bin/communityserver-configure.sh

if [ $1 -ge 2 ]; then
	if [ -d "$DIR" ]; then
		CONN_STR=$(grep -oP "Server=[^\"]*(?=\")" $DIR/WebStudio/web.connections.config | head -1)
		
		if [ -f $DIR/WebStudio/web.appsettings.config.rpmsave ]; then
			CORE_MACHINEKEY="$(sudo sed -n '/"core.machinekey"/s!.*value\s*=\s*"\([^"]*\)".*!\1!p' ${DIR}/WebStudio/web.appsettings.config.rpmsave)";
		fi		

		binDirs=("WebStudio" "ApiSystem" "Services/TeamLabSvc" "Services/MailAggregator" "Services/MailCleaner" "Services/MailWatchdog")

		for i in "${!binDirs[@]}";
		do
			find "$DIR/${binDirs[$i]}" -type f -name "*.[cC]onfig" -exec sed -i "s/connectionString=.*/connectionString=\"$CONN_STR\" providerName=\"MySql.Data.MySqlClient\"\/>/" {} \;

			if [ ! -z $CORE_MACHINEKEY ]; then
				find "$DIR/${binDirs[$i]}" -type f -name "*.[cC]onfig" -exec sed -i '/core.\machinekey/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${CORE_MACHINEKEY}'\"!' {} \;			
			fi				
		done				

		MYSQL_SERVER_HOST=$(grep -oP "Server=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_DB_NAME=$(grep -oP "Database=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_USER=$(grep -oP "User ID=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		MYSQL_SERVER_PASS=$(grep -oP "Password=[^\";]*" $DIR/WebStudio/web.connections.config | head -1 | cut -d'=' -f2);
		
		sed "s!\"host\":.*,!\"host\":\"${MYSQL_SERVER_HOST}\",!" -i ${SERVICES_DIR}/ASC.UrlShortener/config/config.json
		sed "s!\"user\":.*,!\"user\":\"${MYSQL_SERVER_USER}\",!" -i ${SERVICES_DIR}/ASC.UrlShortener/config/config.json
		sed "s!\"password\":.*,!\"password\":\"${MYSQL_SERVER_PASS//!/\\!}\",!" -i ${SERVICES_DIR}/ASC.UrlShortener/config/config.json
		sed "s!\"database\":.*!\"database\":\"${MYSQL_SERVER_DB_NAME}\"!" -i ${SERVICES_DIR}/ASC.UrlShortener/config/config.json		

		if [ ! -z $CORE_MACHINEKEY ]; then
			sed "s!\"core\.machinekey\":.*!\"core\.machinekey\":\"${CORE_MACHINEKEY}\"!" -i ${SERVICES_DIR}/ASC.UrlShortener/config/config.json		
		fi						

		if ! mysqladmin ping -h ${MYSQL_SERVER_HOST} -u ${MYSQL_SERVER_USER} --password=${MYSQL_SERVER_PASS} --silent; then
			echo "ERROR: mysql connection refused";
			exit 1
		fi
		
		if [ ! -d "$APP_INDEX_DIR" ]; then		
			mysql --silent -h ${MYSQL_SERVER_HOST} -u ${MYSQL_SERVER_USER} --password=${MYSQL_SERVER_PASS} -D "$MYSQL_SERVER_DB_NAME" -e "TRUNCATE webstudio_index";
		fi
				
		for i in $(ls $DIR/Sql/onlyoffice.upgrade*); do
				mysql --silent -h ${MYSQL_SERVER_HOST} -u ${MYSQL_SERVER_USER} --password=${MYSQL_SERVER_PASS} -D "$MYSQL_SERVER_DB_NAME" < ${i};
		done
	fi
fi

if [ $1 -eq 1 ]; then

	if /usr/share/elasticsearch/bin/elasticsearch-plugin list | grep -q "ingest-attachment"; then
		/usr/share/elasticsearch/bin/elasticsearch-plugin remove ingest-attachment
	fi

	/usr/share/elasticsearch/bin/elasticsearch-plugin install -s -b ingest-attachment	
fi

if [ -f ${ELASTIC_SEARCH_CONF_PATH}.rpmnew ]; then
   cp -rf ${ELASTIC_SEARCH_CONF_PATH}.rpmnew ${ELASTIC_SEARCH_CONF_PATH};   
fi

if [ -f ${ELASTIC_SEARCH_JAVA_CONF_PATH}.rpmnew ]; then
   cp -rf ${ELASTIC_SEARCH_JAVA_CONF_PATH}.rpmnew ${ELASTIC_SEARCH_JAVA_CONF_PATH};   
fi

mkdir -p "$LOG_DIR/Index"
mkdir -p "$APP_INDEX_DIR"

if [ "$(ls -alhd ${APP_INDEX_DIR} | awk '{ print $3 }')" != "elasticsearch" ]; then
	chown -R elasticsearch:elasticsearch "$APP_INDEX_DIR"
fi

chown -R elasticsearch:elasticsearch "$LOG_DIR/Index"

sed "/path\.data/s!path\.data.*!path\.data: $APP_INDEX_DIR!" -i ${ELASTIC_SEARCH_CONF_PATH}
sed "/path\.logs/s!path\.logs.*!path\.logs: ${LOG_DIR}/Index/!" -i ${ELASTIC_SEARCH_CONF_PATH}

if ! grep -q "indices.fielddata.cache.size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "indices.fielddata.cache.size: 30%" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/indices.fielddata.cache.size.*/indices.fielddata.cache.size: 30%/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

if ! grep -q "indices.memory.index_buffer_size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "indices.memory.index_buffer_size: 30%" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/indices.memory.index_buffer_size.*/indices.memory.index_buffer_size: 30%/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

if grep -q "HeapDumpOnOutOfMemoryError" ${ELASTIC_SEARCH_JAVA_CONF_PATH}; then
	sed "/-XX:+HeapDumpOnOutOfMemoryError/d" -i ${ELASTIC_SEARCH_JAVA_CONF_PATH}
fi

TOTAL_MEMORY=$(free -m | grep -oP '\d+' | head -n 1);
MEMORY_REQUIREMENTS=12228; #RAM ~4*3Gb

if [ ${TOTAL_MEMORY} -gt ${MEMORY_REQUIREMENTS} ]; then
	if ! grep -q "[-]Xms1g" ${ELASTIC_SEARCH_JAVA_CONF_PATH}; then
		echo "-Xms4g" >> ${ELASTIC_SEARCH_JAVA_CONF_PATH}
	else
		sed -i "s/-Xms1g/-Xms4g/" ${ELASTIC_SEARCH_JAVA_CONF_PATH} 
	fi

	if ! grep -q "[-]Xmx1g" ${ELASTIC_SEARCH_JAVA_CONF_PATH}; then
		echo "-Xmx4g" >> ${ELASTIC_SEARCH_JAVA_CONF_PATH}
	else
		sed -i "s/-Xmx1g/-Xmx4g/" ${ELASTIC_SEARCH_JAVA_CONF_PATH} 
	fi
fi

if [ -d /etc/elasticsearch/ ]; then 
	chmod g+ws /etc/elasticsearch/
fi

if systemctl is-active elasticsearch | grep -q "active"; then
	systemctl restart elasticsearch.service	
fi

if [ ! -f /proc/net/if_inet6 ]; then
	sed '/listen\s*\[::\]:80/d' -i /etc/nginx/includes/%{package_sysname}-communityserver-common-ssl.conf.template	
	sed '/listen\s*\[::\]:443/d' -i /etc/nginx/includes/%{package_sysname}-communityserver-common-ssl.conf.template	
fi

if %{getenforce} >/dev/null 2>&1; then
	# check whethere enabled
	shopt -s nocasematch
	PORTS=()
	case $(%{getenforce}) in
	enforcing|permissive)
		PORTS+=('9865') # teamlabJabber
		PORTS+=('9871') # teamlabNotify
		PORTS+=('9882') # teamlabBackup
		PORTS+=('9866') # teamlabSearcher    
		PORTS+=('9899') # SocketIO 
		PORTS+=('5280') # Jabber http-pool  
	;;
	disabled)
		:
	;;
	esac

	# allow http port SELinux
	for PORT in ${PORTS[@]}; do
	%{semanage} port -a -t http_port_t -p tcp $PORT >/dev/null 2>&1 || \
		%{semanage} port -m -t http_port_t -p tcp $PORT >/dev/null 2>&1 || \
		true
	done
fi

for SVC in %{package_services}; do
	if [ -e /usr/lib/systemd/system/$SVC.service ]; then
		systemctl enable $SVC
		systemctl stop $SVC
		systemctl start $SVC || true
	fi
done

if systemctl is-active monoserve | grep -q "active"; then
	curl --silent --output /dev/null http://127.0.0.1/api/2.0/warmup/restart.json || true
fi		

%preun
#if it is deinstallation then we stop and deregister all services
if [ $1 -eq 0 ]; then
	for SVC in %{package_services}; do
		if [ -e /usr/lib/systemd/system/$SVC.service ]; then
			systemctl stop $SVC
			systemctl disable $SVC
		fi
	done
	systemctl daemon-reload
fi

%postun

# need for reinstall plugins for new version elasticsearch after delete prev. version
# not work in post/triggerin and other scriplets install
%triggerpostun -p /bin/bash -- elasticsearch
# elasticsearch is removed
if [ $2 -eq 0 ]; then
	exit 0;
fi

ELASTIC_SEARCH_VERSION=$(rpm -qi elasticsearch | grep Version | tail -n1 | awk -F': ' '/Version/ {print $2}');

if [ ! -z $ELASTIC_SEARCH_VERSION ]; then
	DIR="/usr/share/elasticsearch/bin";

	if $DIR/elasticsearch-plugin list | grep -q "ingest-attachment"; then
		$DIR/elasticsearch-plugin remove ingest-attachment
	fi

	$DIR/elasticsearch-plugin install -s -b ingest-attachment	
	
	if ! systemctl is-active elasticsearch | grep -q "inactive"; then
		systemctl restart elasticsearch 
	fi
	
	if ! systemctl is-active %{package_sysname}Index | grep -q "inactive"; then
		systemctl restart %{package_sysname}Index 
	fi
fi