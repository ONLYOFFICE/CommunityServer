#!/bin/sh

DIR=/var/www/onlyoffice
CONF=$DIR/WebStudio/web.connections.config
DB_HOST=""
DB_NAME=""
DB_USER=""
DB_PWD=""
MYSQL=""

[ -e $CONF ] || { echo "Configuration file not found at path $CONF"; exit 1; }
[ $(id -u) -ne 0 ] && { echo "Root privileges required"; exit 1; }

restart_services() {
	[ -a /etc/nginx/conf.d/default.conf ] && mv /etc/nginx/conf.d/default.conf /etc/nginx/conf.d/default.conf.old
	echo -n "Restarting services... "
	for SVC in redis.service monoserve monoserve2 nginx onlyofficeBackup onlyofficeFeed onlyofficeJabber onlyofficeIndex onlyofficeNotify onlyofficeMailAggregator onlyofficeMailWatchdog
	do
		systemctl stop $SVC 
		systemctl start $SVC
	done
	echo "OK"
}

read_db_params() {
	CONN_STR=$(grep -o "Server=.*;Database=.*;User ID=.*;Password=.*;" $CONF)

	OLD_IFS="$IFS"
	IFS=";="
	VALUES=( $CONN_STR )
	IFS="$OLD_IFS"

	DB_HOST="${VALUES[1]}"
	DB_NAME="${VALUES[3]}"
	DB_USER="${VALUES[5]}"

	echo "Configuring MySQL access... "
	read -e -p "Host: " -i "$DB_HOST" DB_HOST
	read -e -p "Database name: " -i "$DB_NAME" DB_NAME
	read -e -p "User: " -i "$DB_USER" DB_USER 
	read -e -p "Password: " -s DB_PWD
	echo
}

save_db_params() {
	CONN_STR="Server=$DB_HOST;Database=$DB_NAME;User ID=$DB_USER;Password=$DB_PWD;Pooling=true;Character Set=utf8;AutoEnlist=false"
	if [ -d "$DIR" ]; then
		find "$DIR/" -type f -name "*.[cC]onfig" -exec sed -i "s/connectionString=.*/connectionString=\"$CONN_STR\" providerName=\"MySql.Data.MySqlClient\"\/>/" {} \;
	fi
}

establish_db_conn() {
	echo -n "Trying to establish MySQL connection... "

	command -v mysql >/dev/null 2>&1 || { echo "MySQL client not found"; exit 1; }

	MYSQL="mysql -h$DB_HOST -u$DB_USER"
	if [ -n "$DB_PWD" ]; then
		MYSQL="$MYSQL -p$DB_PWD"
	fi

	$MYSQL -e ";" >/dev/null 2>&1
	ERRCODE=$?
	if [ $ERRCODE -ne 0 ]; then
		systemctl mysqld start >/dev/null 2>&1
		$MYSQL -e ";" >/dev/null 2>&1 || { echo "FAILURE"; exit 1; }
	fi

	echo "OK"
}

execute_db_scripts() {
	echo -n "Installing MySQL database... "
	$MYSQL -e "CREATE DATABASE IF NOT EXISTS $DB_NAME CHARACTER SET utf8 COLLATE 'utf8_general_ci';"
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.sql
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.data.sql
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.resources.sql
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.upgrade85.sql
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.upgrade86.sql
	$MYSQL "$DB_NAME" < $DIR/Sql/onlyoffice.upgrade87.sql
	echo "OK"
}

install_sphinx() {
	if [ -x /usr/bin/searchd ]; then
		echo "Sphinx already installed."
	else
		echo -n "Installing sphinx... "
		yum -y install postgresql-libs unixODBC
		if [ ! -f /tmp/sphinx-2.2.8-1.rhel7.x86_64.rpm ]; then
			wget http://sphinxsearch.com/files/sphinx-2.2.8-1.rhel7.x86_64.rpm -O /tmp/sphinx-2.2.8-1.rhel7.x86_64.rpm
		fi
		rpm -Uhv /tmp/sphinx-2.2.8-1.rhel7.x86_64.rpm
		echo "OK"
	fi
}

upgrade_storage(){
	echo -n "Upgrade data storage... "
	if [ -d $DIR/WebStudio/Products/Community/Modules/Blogs/data ]; then
		mkdir -p $DIR/Data/Products/Community/Modules/Blogs/Data
		cp -R -u $DIR/WebStudio/Products/Community/Modules/Blogs/data/. $DIR/Data/Products/Community/Modules/Blogs/Data
	fi
	if [ -d $DIR/WebStudio/Products/Community/Modules/Forum/data ]; then
		mkdir -p $DIR/Data/Products/Community/Modules/Forum/Data
		cp -R -u $DIR/WebStudio/Products/Community/Modules/Forum/data/. $DIR/Data/Products/Community/Modules/Forum/Data
	fi
	if [ -d $DIR/WebStudio/Products/Community/Modules/News/data ]; then
		mkdir -p $DIR/Data/Products/Community/Modules/News/Data
		cp -R -u $DIR/WebStudio/Products/Community/Modules/News/data/. $DIR/Data/Products/Community/Modules/News/Data
	fi
	if [ -d $DIR/WebStudio/Products/Community/Modules/Bookmarking/data ]; then
		mkdir -p $DIR/Data/Products/Community/Modules/Bookmarking/Data
		cp -R -u $DIR/WebStudio/Products/Community/Modules/Bookmarking/data/. $DIR/Data/Products/Community/Modules/Bookmarking/Data
	fi
	if [ -d $DIR/WebStudio/Products/Community/Modules/Wiki/data ]; then
		mkdir -p $DIR/Data/Products/Community/Modules/Wiki/Data
		cp -R -u $DIR/WebStudio/Products/Community/Modules/Wiki/data/. $DIR/Data/Products/Community/Modules/Wiki/Data
	fi
	if [ -d $DIR/Data/Files ]; then
		mkdir -p $DIR/Data/Products
		cp -R -u $DIR/Data/Files $DIR/Data/Products
	fi
	if [ -d $DIR/WebStudio/Products/CRM/data ]; then
		mkdir -p $DIR/Data/Products/CRM/Data
		cp -R -u $DIR/WebStudio/Products/CRM/data/. $DIR/Data/Products/CRM/Data
	fi
	if [ -d $DIR/WebStudio/Products/Projects/data ]; then
		mkdir -p $DIR/Data/Products/Projects/Data
		cp -R -u $DIR/WebStudio/Products/Projects/data/. $DIR/Data/Products/Projects/Data
	fi
	if [ -d $DIR/WebStudio/data ]; then
		mkdir -p $DIR/Data/Studio
		cp -R -u $DIR/WebStudio/data/. $DIR/Data/Studio
	fi
	if [ -d $DIR/WebStudio/addons/mail/data ]; then
		mkdir -p $DIR/Data/addons/mail/Data
		cp -R -u $DIR/WebStudio/addons/mail/data/. $DIR/Data/addons/mail/Data
	fi
	if [ -d $DIR/WebStudio/addons/mail/Data ]; then
		mkdir -p $DIR/Data/addons/mail/Data
		cp -R -u $DIR/WebStudio/addons/mail/Data/. $DIR/Data/addons/mail/Data
	fi
	if [ -d $DIR/Data/Mail/Aggregator ]; then
		mkdir -p $DIR/Data/addons/mail/Data/aggregator
		cp -R -u $DIR/Data/Mail/Aggregator/. $DIR/Data/addons/mail/Data/aggregator
	fi
	if [ -d $DIR/WebStudio/addons/talk/Data/upload ]; then
		mkdir -p $DIR/Data/addons/talk/Data
		cp -R -u $DIR/WebStudio/addons/talk/Data/upload/. $DIR/Data/addons/talk/Data
	fi
	chown -R -f onlyoffice:onlyoffice -R $DIR/Data
	echo "OK"
}


read_db_params
establish_db_conn || exit $?
execute_db_scripts || exit $?
save_db_params
upgrade_storage
install_sphinx
restart_services
