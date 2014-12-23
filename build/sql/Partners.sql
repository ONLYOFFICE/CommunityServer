-- partners_clients
CREATE TABLE IF NOT EXISTS `partners_clients` (
	`id` VARCHAR(36) NOT NULL,
	`partner_id` VARCHAR(36) NULL DEFAULT NULL,
	`email` VARCHAR(255) NULL DEFAULT NULL,
	`first_name` VARCHAR(255) NULL DEFAULT NULL,
	`last_name` VARCHAR(255) NULL DEFAULT NULL,
	`portal_alias` VARCHAR(255) NOT NULL,
	`portal_domain` VARCHAR(255) NOT NULL,
	`phone` VARCHAR(255) NULL DEFAULT NULL,
	`website` VARCHAR(255) NULL DEFAULT NULL,
	`comment` VARCHAR(255) NULL DEFAULT NULL,
	`registration_date` DATETIME NOT NULL,
	`trial_date` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	INDEX `portal` (`portal_alias`),
	INDEX `partner_id` (`partner_id`)
);
-- partners_hosted
CREATE TABLE IF NOT EXISTS `partners_hosted` (
	`partner_id` VARCHAR(36) NOT NULL,
	`api_url` VARCHAR(255) NOT NULL,
	`token` VARCHAR(255) NOT NULL,
	`skey` VARCHAR(255) NOT NULL,
	`json_data` TEXT NOT NULL,
	`synchronize_date` DATETIME NOT NULL,
	`admin_id` VARCHAR(36) NOT NULL,
	PRIMARY KEY (`partner_id`)
);
-- partners_hosted
CREATE TABLE IF NOT EXISTS `partners_hosted_quotas` (
	`id` INT(10) NOT NULL,
	`name` VARCHAR(255) NULL DEFAULT NULL,
	`level` INT(10) NOT NULL DEFAULT '0',
	`features` TEXT NULL,
	`price` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
	`percentage` INT(10) NOT NULL DEFAULT '0',
	`visible` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`)
);
-- partners_hosted_servers
CREATE TABLE IF NOT EXISTS `partners_hosted_servers` (
	`id` VARCHAR(255) NOT NULL,
	`ip` VARCHAR(255) NULL DEFAULT NULL,
	`alias` VARCHAR(255) NOT NULL,
	`status` INT(10) NOT NULL DEFAULT '0',
	`creation_date` DATETIME NOT NULL,
	`first_name` VARCHAR(255) NULL DEFAULT NULL,
	`last_name` VARCHAR(255) NULL DEFAULT NULL,
	`email` VARCHAR(255) NULL DEFAULT NULL,
	`phone` VARCHAR(255) NULL DEFAULT NULL,
	`company` VARCHAR(255) NULL DEFAULT NULL,
	`website` VARCHAR(255) NULL DEFAULT NULL,
	`password` VARCHAR(255) NULL DEFAULT NULL,
	`host_type` TINYINT(4) NULL DEFAULT NULL,
	`region` VARCHAR(30) NULL DEFAULT NULL,
	`instance_type` VARCHAR(30) NULL DEFAULT NULL,
	PRIMARY KEY (`id`)
);
-- partners_invoices
CREATE TABLE IF NOT EXISTS `partners_invoices` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`paypal_id` VARCHAR(36) NULL DEFAULT NULL,
	`partner_id` VARCHAR(36) NULL DEFAULT NULL,
	`amount` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
	`exchange_rate` DECIMAL(8,4) NOT NULL DEFAULT '0.0000',
	`discount` INT(10) NOT NULL DEFAULT '0',
	`status` INT(10) NOT NULL DEFAULT '0',
	`status_date` DATETIME NOT NULL,
	`creation_date` DATETIME NOT NULL,
	`tax` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	UNIQUE INDEX `paypal_id` (`paypal_id`)
);
-- partners_invoices_quotas
CREATE TABLE IF NOT EXISTS `partners_invoices_quotas` (
	`invoice_id` INT(10) NOT NULL DEFAULT '0',
	`quota_id` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`invoice_id`, `quota_id`)
);
-- partners_keys
CREATE TABLE IF NOT EXISTS `partners_keys` (
	`id` CHAR(36) NOT NULL,
	`partner_id` CHAR(36) NOT NULL,
	`code` VARCHAR(50) NOT NULL,
	`tariff` INT(11) NOT NULL,
	`price` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
	`discount` INT(10) NOT NULL DEFAULT '0',
	`payment_method` INT(10) NOT NULL DEFAULT '0',
	`status` INT(11) NOT NULL DEFAULT '0',
	`creation_date` DATETIME NULL DEFAULT NULL,
	`activation_date` DATETIME NULL DEFAULT NULL,
	`payment_date` DATETIME NULL DEFAULT NULL,
	`due_date` DATETIME NULL DEFAULT NULL,
	`comment` TEXT NULL,
	`client_id` VARCHAR(36) NULL DEFAULT NULL,
	`invoice_id` INT(11) NULL DEFAULT NULL,
	`transaction_id` VARCHAR(128) NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	UNIQUE INDEX `code` (`code`),
	INDEX `client_id` (`client_id`),
	INDEX `partner_id` (`partner_id`, `status`)
);
-- partners_log
CREATE TABLE IF NOT EXISTS `partners_log` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`action` TEXT NOT NULL,
	`timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`)
);
-- partners_partners
CREATE TABLE IF NOT EXISTS `partners_partners` (
	`id` VARCHAR(36) NOT NULL,
	`email` VARCHAR(255) NOT NULL,
	`password` VARCHAR(255) NULL DEFAULT NULL,
	`domain` VARCHAR(255) NULL DEFAULT NULL,
	`partner_type` INT(11) NOT NULL DEFAULT '0',
	`first_name` VARCHAR(255) NULL DEFAULT NULL,
	`last_name` VARCHAR(255) NULL DEFAULT NULL,
	`url` VARCHAR(255) NULL DEFAULT NULL,
	`phone` VARCHAR(32) NULL DEFAULT NULL,
	`language` VARCHAR(32) NULL DEFAULT 'en-US',
	`company` VARCHAR(255) NULL DEFAULT NULL,
	`country` VARCHAR(255) NULL DEFAULT NULL,
	`country_code` VARCHAR(10) NULL DEFAULT NULL,
	`country_vat` INT(11) NOT NULL DEFAULT '0',
	`address` VARCHAR(1024) NULL DEFAULT NULL,
	`vat_id` VARCHAR(255) NULL DEFAULT NULL,
	`comment` TEXT NULL,
	`creation_date` DATETIME NOT NULL,
	`status` INT(11) NOT NULL DEFAULT '0',
	`portal` VARCHAR(255) NULL DEFAULT NULL,
	`portal_confirmed` INT(11) NOT NULL DEFAULT '0',
	`partner_limit` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
	`discount` INT(11) NOT NULL DEFAULT '0',
	`deposit` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
	`paypal_account` VARCHAR(64) NULL DEFAULT NULL,
	`paypal_login` VARCHAR(150) NULL DEFAULT NULL,
	`paypal_token` VARCHAR(150) NULL DEFAULT NULL,
	`paypal_stoken` VARCHAR(150) NULL DEFAULT NULL,
	`payment_method` INT(11) NOT NULL DEFAULT '0',
	`payment_url` VARCHAR(2048) NULL DEFAULT NULL,
	`currency` VARCHAR(16) NULL DEFAULT NULL,
	`logo_url` VARCHAR(1024) NULL DEFAULT NULL,
	`display_name` VARCHAR(255) NULL DEFAULT NULL,
	`display_url` VARCHAR(255) NULL DEFAULT NULL,
	`display_email` VARCHAR(255) NULL DEFAULT NULL,
	`display_phone` VARCHAR(32) NULL DEFAULT NULL,
	`display_address` VARCHAR(1024) NULL DEFAULT NULL,
	`support_phone` VARCHAR(255) NULL DEFAULT NULL,
	`support_email` VARCHAR(255) NULL DEFAULT NULL,
	`sales_email` VARCHAR(255) NULL DEFAULT NULL,
	`terms_url` VARCHAR(255) NULL DEFAULT NULL,
	`theme` VARCHAR(32) NULL DEFAULT 'default',
	`ru_account` VARCHAR(64) NULL DEFAULT NULL,
	`ru_bank` VARCHAR(255) NULL DEFAULT NULL,
	`ru_ks` VARCHAR(64) NULL DEFAULT NULL,
	`ru_kpp` VARCHAR(64) NULL DEFAULT NULL,
	`ru_bik` VARCHAR(64) NULL DEFAULT NULL,
	`ru_inn` VARCHAR(64) NULL DEFAULT NULL,
	`skey` VARCHAR(64) NULL DEFAULT NULL,
	`admin_id` VARCHAR(36) NULL DEFAULT NULL,
	`activation_date` DATETIME NULL DEFAULT NULL,
	`removed` INT(11) NOT NULL DEFAULT '0',
	`time_zone` VARCHAR(255) NULL DEFAULT NULL,
	`contract_number` VARCHAR(255) NULL DEFAULT NULL,
	`tax_number` VARCHAR(64) NULL DEFAULT NULL,
	`tax_file_url` VARCHAR(1024) NULL DEFAULT NULL,
	`helptour_status` INT(10) NULL DEFAULT '0',
	`custom_email_signature` TINYINT(4) NOT NULL DEFAULT '0',
	`level` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `email` (`email`)
);
-- partners_quotas
CREATE TABLE IF NOT EXISTS `partners_quotas` (
	`partner_id` VARCHAR(38) NOT NULL,
	`quota_id` INT(11) NOT NULL,
	`price` DECIMAL(15,2) NOT NULL DEFAULT '0.00',
	`hosted_service` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`partner_id`, `quota_id`)
);


