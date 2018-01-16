Summary: Office suite and business productivity tools
Name: onlyoffice-communityserver
Version: 8.8.1
Release: 0
License: GPLv3
Group: Applications/Internet
URL: http://onlyoffice.com/
Vendor: ONLYOFFICE (Online documents editor)
Packager: ONLYOFFICE (Online documents editor) <support@onlyoffice.com>
Requires: mono-complete >= 5.0.0, nginx >= 0.8.21, mysql-server >= 5.7.0, wget, mono-webserver-hyperfastcgi, nodejs >= 6.11.4
BuildArch: noarch
AutoReq: no
AutoProv: no

%description
Free open source office suite and business productivity tools in one solution:
online document editors, file and project management, CRM

%prep
rm -rf "$RPM_BUILD_ROOT"

%build

%install
#install onlyoffice files
mkdir -p "$RPM_BUILD_ROOT/var/www/onlyoffice/"
mkdir -p "$RPM_BUILD_ROOT/var/www/onlyoffice/Tools/"
mkdir -p "$RPM_BUILD_ROOT/usr/bin/"
cp -r ../../Files/bin/*.sh "$RPM_BUILD_ROOT/usr/bin/"
cp -r ../../Files/onlyoffice/. "$RPM_BUILD_ROOT/var/www/onlyoffice/"
cp -r ../../Files/tools/. "$RPM_BUILD_ROOT/var/www/onlyoffice/Tools/"
mkdir -p "$RPM_BUILD_ROOT/var/log/onlyoffice/"

#install init scripts
mkdir -p "$RPM_BUILD_ROOT/usr/lib/systemd/system/"
cp ../../Files/init/*.service "$RPM_BUILD_ROOT/usr/lib/systemd/system/"

#install nginx config
mkdir -p "$RPM_BUILD_ROOT/etc/nginx/conf.d/"
mkdir -p "$RPM_BUILD_ROOT/etc/nginx/includes/"
cp ../../Files/nginx/onlyoffice.conf "$RPM_BUILD_ROOT/etc/nginx/conf.d/"
cp ../../Files/nginx/includes/* "$RPM_BUILD_ROOT/etc/nginx/includes/"

#install hyperfastcgi config
mkdir -p "$RPM_BUILD_ROOT/etc/hyperfastcgi/"
cp ../../Files/hyperfastcgi/onlyoffice "$RPM_BUILD_ROOT/etc/hyperfastcgi/"
cp ../../Files/hyperfastcgi/onlyofficeApiSystem "$RPM_BUILD_ROOT/etc/hyperfastcgi/"


#list onlyoffice files
OLD_IFS="$IFS"
IFS="
"
for FILE in `find "$RPM_BUILD_ROOT/var/www/onlyoffice/"`; do
	RELFILE="`echo "$FILE" | sed s?"$RPM_BUILD_ROOT"??`"
	if [ -d "$FILE" ]; then
		echo "%%attr(-, onlyoffice, onlyoffice) %%dir \"$RELFILE\"" >>onlyoffice.list
	else
		case "$FILE" in
			*/WebStudio?\(2\)/web.connections.config )
				echo "%%attr(-, onlyoffice, onlyoffice) %%config(noreplace) \"$RELFILE\"" >>onlyoffice.list
			;;

			*/WebStudio?\(2\)/[Ww]eb*.config | */TeamLabSvc.exe.Config | */ASC.Mail.Aggregator.CollectionService.exe.config | */ASC.Mail.Watchdog.Service.exe.config )
				echo "%%attr(-, onlyoffice, onlyoffice) %%config \"$RELFILE\"" >>onlyoffice.list
			;;

			* )
				echo "%%attr(-, onlyoffice, onlyoffice) \"$RELFILE\"" >>onlyoffice.list
			;;
		esac
	fi
done
IFS="$OLD_IFS"

%clean
rm -rf "$RPM_BUILD_ROOT"

%files -f onlyoffice.list
%attr(-, root, root) /usr/bin/communityserver-*.sh
%attr(-, onlyoffice, onlyoffice) /var/log/onlyoffice/
%config %attr(-, root, root) /usr/lib/systemd/system/*.service
%config %attr(-, root, root) /etc/nginx/conf.d/onlyoffice.conf
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-common.conf
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-common.conf.template
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-common-init.conf.template
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-common-ssl.conf.template
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-nginx.conf.template
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-services.conf
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-letsencrypt.conf
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-proxy-to-controlpanel.conf.template
%config %attr(-, root, root) /etc/nginx/includes/onlyoffice-communityserver-proxy-to-documentserver.conf.template
%config %attr(-, root, root) /etc/hyperfastcgi/onlyoffice
%config %attr(-, root, root) /etc/hyperfastcgi/onlyofficeApiSystem

%pre
#add group and user for onlyoffice app
getent group onlyoffice >/dev/null || groupadd -r onlyoffice
getent passwd onlyoffice >/dev/null || useradd -r -g onlyoffice -d /var/www/onlyoffice/ -s /sbin/nologin onlyoffice
exit 0

%post
#import common ssl certificates
mozroots --import --sync --machine --quiet

mkdir -p /etc/mono/registry/LocalMachine
mkdir -p /usr/share/.mono/keypairs
mkdir -p /var/cache/nginx/onlyoffice
mkdir -p /var/run/onlyoffice
mkdir -p -m 700 /var/run/onlyoffice/.config/.mono/keypairs
chown nginx:nginx /var/cache/nginx/onlyoffice
chown onlyoffice:nginx /var/run/onlyoffice
chmod g+s+w /var/run/onlyoffice

rm -f "/etc/nginx/conf.d/onlyoffice-apisystem.conf"
rm -f "/var/www/onlyoffice/configure.sh"

# fix for mono 4.4.2.11
# ln -s /usr/lib64/libMonoPosixHelper.so /usr/lib/libMonoPosixHelper.so

#register all services
systemctl daemon-reload
systemctl enable redis
systemctl enable nginx
systemctl start redis

for SVC in monoserve monoserveApiSystem onlyofficeSocketIO onlyofficeBackup onlyofficeFeed onlyofficeJabber onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog; do
	if [ -e /usr/lib/systemd/system/$SVC.service ]; then
		systemctl enable $SVC
	fi
done
systemctl daemon-reload

if [ $1 -ge 2 ]; then
	CONN_STR=$(grep -oP "Server=[^\"]*(?=\")" /var/www/onlyoffice/WebStudio/web.connections.config | head -1)
	find "/var/www/onlyoffice/" -type f -name "*.[cC]onfig" -exec sed -i "s/connectionString=.*/connectionString=\"$CONN_STR\" providerName=\"MySql.Data.MySqlClient\"\/>/" {} \;
fi

# allow http port SELinux http://stackoverflow.com/questions/23948527/13-permission-denied-while-connecting-to-upstreamnginx
semanage port --add --type http_port_t --proto tcp 8086-8087 || true

%preun
#if it is deinstallation then we stop and deregister all services
if [ $1 -eq 0 ]; then
	for SVC in monoserve monoserveApiSystem onlyofficeSocketIO onlyofficeBackup onlyofficeFeed onlyofficeJabber onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog; do
		if [ -e /usr/lib/systemd/system/$SVC.service ]; then
			systemctl stop $SVC
			systemctl disable $SVC
		fi
	done
	systemctl daemon-reload
fi

%postun
#if it was update then we can restart all services including nginx
if [ $1 -ge 1 ]; then
	for SVC in redis monoserve monoserveApiSystem onlyofficeSocketIO onlyofficeBackup onlyofficeFeed onlyofficeJabber onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog; do
		if [ -e /usr/lib/systemd/system/$SVC.service ]; then
			systemctl stop $SVC
			systemctl start $SVC
		fi
	done
	systemctl daemon-reload
fi

%changelog
* Mon Nov 16 2015 ONLYOFFICE (Online documents editor) <support@onlyoffice.com>
- We have updated ONLYOFFICE Community Server to ver. 8.8.0.
