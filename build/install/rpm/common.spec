Summary: %{package_header_tag_summary}
Name: %{package_header_tag_name}
Version: %{package_header_tag_version}
Release: %{package_header_tag_release}
License: GPLv3
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

#install init scripts
mkdir -p "$RPM_BUILD_ROOT/usr/lib/systemd/system/"
cp ../../Files/init/*.service "$RPM_BUILD_ROOT/usr/lib/systemd/system/"

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
%attr(-, %{package_sysname}, %{package_sysname}) /var/log/%{package_sysname}/
%config %attr(-, root, root) /usr/lib/systemd/system/*.service
%config %attr(-, root, root) %{nginx_conf_d}/%{package_sysname}.conf
%attr(-, root, root) /etc/nginx/includes/%{package_sysname}-communityserver-*
%config %attr(-, root, root) /etc/hyperfastcgi/*

%pre
#add group and user for app
getent group %{package_sysname} >/dev/null || groupadd -r %{package_sysname}
getent passwd %{package_sysname} >/dev/null || useradd -r -g %{package_sysname} -d /var/www/%{package_sysname}/ -s /sbin/nologin %{package_sysname}
exit 0

%triggerin -- %{package_sysname}-controlpanel

DIR=/var/www/%{package_sysname}
NGINX_ROOT_DIR="/etc/nginx";
APP_WEB_ROOT_DIR="$DIR/WebStudio";
APP_SERVICES_ROOT_DIR="$DIR/Services";
CONTROL_PANEL_PORT_80_TCP_ADDR="localhost:8082";
SERVICE_SSO_AUTH_HOST_ADDR="localhost";

cp ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf.template ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed 's,{{CONTROL_PANEL_HOST_ADDR}},'"${CONTROL_PANEL_PORT_80_TCP_ADDR}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed 's,{{SERVICE_SSO_AUTH_HOST_ADDR}},'"${SERVICE_SSO_AUTH_HOST_ADDR}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-controlpanel.conf;
sed '/web\.controlpanel\.url/s/\(value\s*=\s*\"\)[^\"]*\"/\1\/controlpanel\/\"/' -i  ${APP_WEB_ROOT_DIR}/web.appsettings.config;
sed '/web\.controlpanel\.url/s/\(value\s*=\s*\"\)[^\"]*\"/\1\/controlpanel\/\"/' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;

%triggerin -- elasticsearch

ELASTIC_SEARCH_CONF_PATH="/etc/elasticsearch/elasticsearch.yml"
DIR=/var/www/%{package_sysname}
APP_DATA_DIR="${DIR}/Data"
LOG_DIR="/var/log/%{package_sysname}/"
	
systemctl stop elasticsearch.service

if ! /usr/share/elasticsearch/bin/elasticsearch-plugin list | grep -q "ingest-attachment"; then
	/usr/share/elasticsearch/bin/elasticsearch-plugin install -s -b ingest-attachment
fi

mkdir -p "$LOG_DIR/Index"
mkdir -p "$APP_DATA_DIR/Index"
chown -R elasticsearch:elasticsearch "$APP_DATA_DIR/Index"
chown -R elasticsearch:elasticsearch "$LOG_DIR/Index"
sed "/path\.data/s!path\.data.*!path\.data: ${APP_DATA_DIR}/Index/!" -i ${ELASTIC_SEARCH_CONF_PATH}
sed "/path\.logs/s!path\.logs.*!path\.logs: ${LOG_DIR}/Index/!" -i ${ELASTIC_SEARCH_CONF_PATH}

if ! grep -q "http.max_content_length" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "http.max_content_length: 2g" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/http.max_content_length.*/http.max_content_length: 2g/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

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

if ! grep -q "thread_pool.index.queue_size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "thread_pool.index.queue_size: 250" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/thread_pool.index.queue_size.*/thread_pool.index.queue_size: 250/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

if ! grep -q "thread_pool.write.queue_size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "thread_pool.write.queue_size: 250" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/thread_pool.write.queue_size.*/thread_pool.write.queue_size: 250/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

export LC_ALL=C

CORE_COUNT=$(( $(lscpu | awk '/^Socket\(s\)/{ print $2 }') * $(lscpu | awk '/^Core\(s\) per socket/{ print $4 }') ));

unset LC_ALL

if [ "$CORE_COUNT" -eq "0" ]; then
	CORE_COUNT=1;
fi

if ! grep -q "thread_pool.index.size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "thread_pool.index.size: $CORE_COUNT" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/thread_pool.index.size.*/thread_pool.index.size: $CORE_COUNT/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

if ! grep -q "thread_pool.write.size" ${ELASTIC_SEARCH_CONF_PATH}; then
	echo "thread_pool.write.size: $CORE_COUNT" >> ${ELASTIC_SEARCH_CONF_PATH}
else
	sed -i "s/thread_pool.write.size.*/thread_pool.write.size: $CORE_COUNT/" ${ELASTIC_SEARCH_CONF_PATH} 
fi

if grep -q "HeapDumpOnOutOfMemoryError" /etc/elasticsearch/jvm.options; then
	sed "/-XX:+HeapDumpOnOutOfMemoryError/d" -i /etc/elasticsearch/jvm.options
fi

if [ -d /etc/elasticsearch/ ]; then 
	chmod g+ws /etc/elasticsearch/
fi

systemctl start elasticsearch.service	

%triggerin -- python3, python36

if ! which python3; then
	if rpm -q python36; then
		update-alternatives --install /usr/bin/python3 python3 /usr/bin/python3.6 1
	fi
fi

DIR=/var/www/%{package_sysname}

python3 -m pip install --upgrade pip
python3 -m pip install --upgrade requests
python3 -m pip install --upgrade radicale
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_auth_plugin/.
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_store_plugin/.
python3 -m pip install --upgrade $DIR/Tools/radicale/plugins/app_rights_plugin/.

%triggerin -- %{package_sysname}-documentserver, %{package_sysname}-documentserver-ie

DIR=/var/www/%{package_sysname}
APP_DATA_DIR="${DIR}/Data"
NGINX_ROOT_DIR="/etc/nginx";
APP_WEB_ROOT_DIR="$DIR/WebStudio";
APP_SERVICES_ROOT_DIR="$DIR/Services";

SERVER_HOST="localhost";
LICENSE_FILE_PATH="/var/www/%{package_sysname}/Data/license.lic";

DOCUMENT_SERVER_PROTOCOL="http";
DOCUMENT_SERVER_HOST="localhost:8083";
DOCUMENT_SERVER_HOST_PROXY="localhost\/ds-vpath";
DOCUMENT_SERVER_API_URL="\/ds-vpath\/";
DOCUMENT_SERVER_JWT_SECRET=${JWT_SECRET:-"%{package_sysname}"};
DOCUMENT_SERVER_JWT_HEADER=${JWT_HEADER:-"AuthorizationJwt"};

cp ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf.template ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf;

sed 's,{{DOCUMENT_SERVER_HOST_ADDR}},'"${DOCUMENT_SERVER_PROTOCOL}:\/\/${DOCUMENT_SERVER_HOST}"',' -i ${NGINX_ROOT_DIR}/includes/%{package_sysname}-communityserver-proxy-to-documentserver.conf;

sed '/files\.docservice\.url\.internal/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_PROTOCOL}':\/\/'${DOCUMENT_SERVER_HOST}'\"!' -i  ${APP_WEB_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.url\.public/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_API_URL}'\"!' -i ${APP_WEB_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.url\.portal/s!\(value\s*=\s*\"\)[^\"]*\"!\1http:\/\/'${SERVER_HOST}'\"!' -i ${APP_WEB_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.secret/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_SECRET}'\"!' -i ${APP_WEB_ROOT_DIR}/web.appsettings.config;
sed '/files\.docservice\.secret.header/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_HEADER}'\"!' -i ${APP_WEB_ROOT_DIR}/web.appsettings.config;

sed '/files\.docservice\.url\.internal/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_PROTOCOL}':\/\/'${DOCUMENT_SERVER_HOST}'\"!' -i  ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.url\.public/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_API_URL}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.url\.portal/s!\(value\s*=\s*\"\)[^\"]*\"!\1http:\/\/'${SERVER_HOST}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.secret/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_SECRET}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;
sed '/files\.docservice\.secret.header/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${DOCUMENT_SERVER_JWT_HEADER}'\"!' -i ${APP_SERVICES_ROOT_DIR}/TeamLabSvc/TeamLabSvc.exe.config;

sed '/license\.file\.path/s!\(value\s*=\s*\"\)[^\"]*\"!\1'${LICENSE_FILE_PATH}'\"!' -i ${APP_WEB_ROOT_DIR}/web.appsettings.config

if [ "$(ls -alhd ${APP_DATA_DIR} | awk '{ print $3 }')" != "%{package_sysname}" ]; then
	  chown %{package_sysname}:%{package_sysname} ${APP_DATA_DIR}
fi

%post

DIR=/var/www/%{package_sysname}

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
	CONN_STR=$(grep -oP "Server=[^\"]*(?=\")" /var/www/%{package_sysname}/WebStudio/web.connections.config | head -1)
	find "/var/www/%{package_sysname}/ApiSystem/" -type f -name "*.[cC]onfig" -exec sed -i "s/connectionString=.*/connectionString=\"$CONN_STR\" providerName=\"MySql.Data.MySqlClient\"\/>/" {} \;
	find "/var/www/%{package_sysname}/Services/" -type f -name "*.[cC]onfig" -exec sed -i "s/connectionString=.*/connectionString=\"$CONN_STR\" providerName=\"MySql.Data.MySqlClient\"\/>/" {} \;
fi

systemctl daemon-reload

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
		systemctl start $SVC
	fi
done

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