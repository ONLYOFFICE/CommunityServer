-- account_links
CREATE TABLE IF NOT EXISTS `account_links` (
	`id` VARCHAR(200) NOT NULL,
	`uid` VARCHAR(200) NOT NULL,
	`provider` CHAR(32) NULL DEFAULT NULL,
	`profile` TEXT NOT NULL,
	`linked` DATETIME NOT NULL,
	PRIMARY KEY (`id`, `uid`),
	INDEX `uid` (`uid`)
);
-- avangate_coupons
CREATE TABLE IF NOT EXISTS `avangate_coupons` (
	`id` VARCHAR(100) NOT NULL,
	`title` VARCHAR(100) NOT NULL,
	`description` VARCHAR(100) NULL DEFAULT NULL,
	PRIMARY KEY (`id`)
);
-- callback_request
CREATE TABLE IF NOT EXISTS `callback_request` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`first_name` VARCHAR(255) NOT NULL,
	`last_name` VARCHAR(255) NOT NULL,
	`phone` VARCHAR(255) NOT NULL,
	`calltheme` VARCHAR(255) NOT NULL,
	`desireddatetime` VARCHAR(255) NOT NULL,
	`timezone` VARCHAR(255) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`)
);
-- data_for_terms
CREATE TABLE IF NOT EXISTS `data_for_terms` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`portal_url` VARCHAR(255) NOT NULL,
	`company_name` VARCHAR(255) NOT NULL,
	`company_address` VARCHAR(255) NOT NULL,
	`vat` VARCHAR(255) NOT NULL,
	`kpp` VARCHAR(255) NOT NULL,
	`settlement_account` VARCHAR(255) NOT NULL,
	`correspondent_account` VARCHAR(255) NOT NULL,
	`bank_id_code` VARCHAR(255) NOT NULL,
	`client_name_nominative` VARCHAR(255) NOT NULL,
	`client_name_genitive` VARCHAR(255) NOT NULL,
	`email` VARCHAR(255) NOT NULL,
	`tariff` VARCHAR(255) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`),
	INDEX `portal_url` (`portal_url`)
);
-- dbip_location
CREATE TABLE IF NOT EXISTS `dbip_location` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`ip_start` VARCHAR(39) NOT NULL,
	`ip_end` VARCHAR(39) NOT NULL,
	`country` VARCHAR(2) NOT NULL,
	`stateprov` VARCHAR(255) NOT NULL,
	`city` VARCHAR(255) NOT NULL,
	`timezone_offset` INT(10) NOT NULL,
	`timezone_name` VARCHAR(255) NOT NULL,
	`processed` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `ip_start` (`ip_start`)
);
-- hybrid_cloud_request
CREATE TABLE IF NOT EXISTS `hybrid_cloud_request` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`first_name` VARCHAR(255) NOT NULL,
	`last_name` VARCHAR(255) NOT NULL,
	`email` VARCHAR(255) NOT NULL,
	`phone` VARCHAR(255) NOT NULL,
	`industry` VARCHAR(255) NOT NULL,
	`employee_count` INT(10) UNSIGNED NOT NULL,
	`winserver_version` VARCHAR(255) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`)
);
-- non_profit_request
CREATE TABLE IF NOT EXISTS `non_profit_request` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`email` VARCHAR(255) NOT NULL,
	`first_name` VARCHAR(255) NOT NULL,
	`last_name` VARCHAR(255) NOT NULL,
	`url` VARCHAR(255) NOT NULL,
	`url_hash` VARCHAR(32) NOT NULL,
	`status` INT(1) UNSIGNED ZEROFILL NOT NULL DEFAULT '0',
	`approved_on` DATETIME NULL DEFAULT NULL,
	`responsible_id` CHAR(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	`culture` VARCHAR(20) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`last_pinged_on` DATETIME NULL DEFAULT NULL,
	`comment` TEXT NULL,
	`disable_auto_check` TINYINT(1) NOT NULL DEFAULT '0',
	`note` TEXT NULL,
	`tenant_alias` VARCHAR(255) NOT NULL,
	`tenant_id` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `url` (`url_hash`),
	INDEX `create_on` (`create_on`),
	INDEX `tenant_alias` (`tenant_alias`)
);
-- non_profit_settings
CREATE TABLE IF NOT EXISTS `non_profit_settings` (
	`last_key` VARCHAR(128) NOT NULL,
	`last_date` DATETIME NOT NULL,
	PRIMARY KEY (`last_key`)
);
-- non_profit_tag
CREATE TABLE IF NOT EXISTS `non_profit_tag` (
	`tag_id` INT(11) NOT NULL,
	`non_profit_id` INT(11) NOT NULL,
	PRIMARY KEY (`tag_id`, `non_profit_id`)
);
-- non_profit_tag_description
CREATE TABLE IF NOT EXISTS `non_profit_tag_description` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `title` (`title`)
);
-- payments_system
CREATE TABLE IF NOT EXISTS `payments_system` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(50) NULL DEFAULT NULL,
	PRIMARY KEY (`id`)
);
-- payments_tenant
CREATE TABLE IF NOT EXISTS `payments_tenant` (
	`id` INT(10) NOT NULL,
	`tenant_id` INT(10) NOT NULL DEFAULT '0',
	`tenant_region` VARCHAR(50) NOT NULL DEFAULT '',
	`sum` DOUBLE NOT NULL DEFAULT '0',
	`payment_date` DATETIME NOT NULL,
	`is_year` TINYINT(4) NULL DEFAULT NULL,
	`status` INT(10) NULL DEFAULT NULL,
	`payment_type` INT(10) NULL DEFAULT '1',
	`active_users` INT(10) NULL DEFAULT '0',
	`affiliate_transferred` TINYINT(4) NOT NULL DEFAULT '-1',
	`cart_id` VARCHAR(50) NOT NULL DEFAULT '',
	`country` VARCHAR(50) NULL DEFAULT '',
	`payment_id` VARCHAR(10) NULL DEFAULT '',
	PRIMARY KEY (`id`),
	INDEX `tenant_id` (`tenant_id`),
	INDEX `tenant_id_tenant_region` (`tenant_region`, `tenant_id`),
	INDEX `payment_date` (`payment_date`),
	INDEX `payment_id` (`payment_id`, `payment_date`)
);
-- payments_tenant_transfer
CREATE TABLE IF NOT EXISTS `payments_tenant_transfer` (
	`payment_id` INT(10) NOT NULL,
	`status` TINYINT(4) NOT NULL DEFAULT '-1',
	`status_changed` DATETIME NULL DEFAULT NULL,
	`pay_key` VARCHAR(50) NULL DEFAULT NULL,
	`sender_transaction_id` VARCHAR(50) NULL DEFAULT NULL,
	`transaction_id` VARCHAR(50) NULL DEFAULT NULL,
	PRIMARY KEY (`payment_id`)
);
-- regions
CREATE TABLE IF NOT EXISTS `regions` (
	`region` VARCHAR(32) NOT NULL,
	`connection_string` VARCHAR(1024) NOT NULL,
	`provider` VARCHAR(64) NOT NULL,
	PRIMARY KEY (`region`)
);
-- remove_portal_feedback
CREATE TABLE IF NOT EXISTS `remove_portal_feedback` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`tenant_alias` VARCHAR(255) NULL DEFAULT NULL,
	`email` VARCHAR(255) NULL DEFAULT NULL,
	`first_name` VARCHAR(255) NULL DEFAULT NULL,
	`last_name` VARCHAR(255) NULL DEFAULT NULL,
	`culture` VARCHAR(20) NOT NULL,
	`reason` VARCHAR(255) NOT NULL,
	`effort` VARCHAR(255) NOT NULL,
	`note` TEXT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`),
	INDEX `tenant_alias` (`tenant_alias`)
);
-- request_params
CREATE TABLE IF NOT EXISTS `request_params` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(50) NULL DEFAULT NULL,
	`value` VARCHAR(100) NULL DEFAULT NULL,
	PRIMARY KEY (`id`)
);
-- session_params
CREATE TABLE IF NOT EXISTS `session_params` (
	`session_id` VARCHAR(50) NOT NULL DEFAULT '',
	`params_id` INT(11) NOT NULL DEFAULT '0',
	PRIMARY KEY (`session_id`, `params_id`)
);
-- session_session
CREATE TABLE IF NOT EXISTS `session_session` (
	`id` CHAR(40) NOT NULL DEFAULT '',
	`start_date` DATETIME NOT NULL,
	`affiliate_page_id` INT(11) NOT NULL DEFAULT '0',
	`affiliate_id` VARCHAR(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	`affiliate_site_id` INT(10) NULL DEFAULT NULL,
	`ip_address` VARCHAR(50) NULL DEFAULT NULL,
	PRIMARY KEY (`id`, `affiliate_page_id`),
	INDEX `FK_session_affiliate_page` (`affiliate_page_id`),
	INDEX `affiliate_site_id` (`affiliate_site_id`),
	INDEX `start_date` (`affiliate_site_id`, `start_date`, `affiliate_id`),
	INDEX `Index 5` (`affiliate_id`, `start_date`)
);
-- stat_enterprise_data
CREATE TABLE IF NOT EXISTS `stat_enterprise_data` (
	`id` VARCHAR(36) NOT NULL,
	`email` VARCHAR(255) NULL DEFAULT NULL,
	`clientname` VARCHAR(255) NULL DEFAULT NULL,
	`access` VARCHAR(255) NULL DEFAULT NULL,
	`portaladdress` VARCHAR(255) NULL DEFAULT NULL,
	`history` TEXT NULL,
	PRIMARY KEY (`id`)
);
-- support_requests
CREATE TABLE IF NOT EXISTS `support_requests` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`email` VARCHAR(100) NULL DEFAULT NULL,
	`subject` VARCHAR(100) NOT NULL,
	`message` VARCHAR(1000) NOT NULL,
	`date` DATETIME NULL DEFAULT NULL,
	`supporter_id` INT(10) NULL DEFAULT NULL,
	PRIMARY KEY (`id`)
);
-- support_users
CREATE TABLE IF NOT EXISTS `support_users` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(50) NOT NULL,
	`email` VARCHAR(100) NOT NULL,
	`role` INT(11) NOT NULL,
	`password` VARCHAR(200) NOT NULL,
	PRIMARY KEY (`id`)
);
-- teamlab_apps_request
CREATE TABLE IF NOT EXISTS `teamlab_apps_request` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`first_name` VARCHAR(255) NOT NULL,
	`last_name` VARCHAR(255) NOT NULL,
	`phone` VARCHAR(255) NOT NULL,
	`industry` VARCHAR(255) NOT NULL,
	`employees_count` VARCHAR(255) NOT NULL,
	`email` VARCHAR(255) NOT NULL,
	`company_name` VARCHAR(255) NOT NULL,
	`company_address` VARCHAR(255) NOT NULL,
	`category` VARCHAR(255) NOT NULL,
	`request_type` VARCHAR(255) NOT NULL,
	`culture` VARCHAR(20) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`notified` TINYINT(1) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`)
);
-- tenants_cache
CREATE TABLE IF NOT EXISTS `tenants_cache` (
	`tenant_alias` VARCHAR(100) NOT NULL,
	UNIQUE INDEX `tenat_alias` (`tenant_alias`)
);


