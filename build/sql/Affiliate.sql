-- affiliate_page
CREATE TABLE IF NOT EXISTS `affiliate_page` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`url` VARCHAR(2600) NULL DEFAULT NULL,
	`affiliate_site_id` INT(11) NOT NULL,
	`affiliate_id` VARCHAR(38) NULL DEFAULT NULL,
	`verified` TINYINT(1) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `FK_affiliate_page_affiliate_site` (`affiliate_site_id`),
	INDEX `affiliate_id` (`affiliate_id`),
	INDEX `url` (`url`(255))
);
-- affiliate_payments
CREATE TABLE IF NOT EXISTS `affiliate_payments` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`affiliate_id` VARCHAR(38) NOT NULL DEFAULT '0',
	`payments_system_id` INT(10) NOT NULL DEFAULT '0',
	`payments_email` VARCHAR(200) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `affiliate_id` (`affiliate_id`)
);
-- affiliate_plans
CREATE TABLE IF NOT EXISTS `affiliate_plans` (
	`affiliate_id` VARCHAR(38) NOT NULL DEFAULT '',
	`plan_id` INT(10) NOT NULL DEFAULT '0',
	`date` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`affiliate_id`, `plan_id`),
	INDEX `affiliate_id` (`affiliate_id`)
);
-- affiliate_promo
CREATE TABLE IF NOT EXISTS `affiliate_promo` (
	`affiliate_id` VARCHAR(38) NOT NULL DEFAULT '',
	`coupon_id` VARCHAR(100) NOT NULL DEFAULT '',
	PRIMARY KEY (`affiliate_id`, `coupon_id`)
);
-- affiliate_site
CREATE TABLE IF NOT EXISTS `affiliate_site` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`url` VARCHAR(1450) NULL DEFAULT NULL,
	`affiliate_id` VARCHAR(38) NULL DEFAULT NULL,
	`verified` TINYINT(1) NOT NULL DEFAULT '0',
	`common` TINYINT(1) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	INDEX `affiliate_id` (`affiliate_id`),
	INDEX `url` (`url`(255))
);
-- affiliate_tenant
CREATE TABLE IF NOT EXISTS `affiliate_tenant` (
	`affiliate_id` VARCHAR(38) NOT NULL DEFAULT '0',
	`tenant_id` INT(10) NOT NULL DEFAULT '0',
	`tenant_owner_email` VARCHAR(100) NOT NULL DEFAULT '',
	`date` DATETIME NULL DEFAULT NULL,
	`affiliate_page_id` INT(10) NULL DEFAULT NULL,
	`by_promo` TINYINT(4) NOT NULL DEFAULT '0',
	`affiliate_site_id` INT(10) NULL DEFAULT '0',
	`tenant_alias` VARCHAR(100) NOT NULL DEFAULT '',
	`tenant_active_users` INT(10) NULL DEFAULT '1',
	`tenant_region` VARCHAR(50) NOT NULL DEFAULT '',
	`stamp` DATETIME NULL DEFAULT NULL,
	`session_id` VARCHAR(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	`tenant_status` INT(10) NULL DEFAULT '0',
	`payment_id` VARCHAR(10) NOT NULL,
	PRIMARY KEY (`tenant_alias`),
	INDEX `affiliate_id` (`affiliate_id`),
	INDEX `tenant_id` (`tenant_id`),
	INDEX `session_id` (`session_id`),
	INDEX `payment_id` (`payment_id`),
	INDEX `affiliate_page` (`affiliate_page_id`),
	INDEX `affiliate_site_id` (`date`, `affiliate_site_id`, `affiliate_id`, `affiliate_page_id`),
	INDEX `tenant_status` (`tenant_status`)
);
-- affiliate_tenant
CREATE TABLE IF NOT EXISTS `affiliate_user` (
	`id` VARCHAR(38) NOT NULL,
	`name` VARCHAR(50) NULL DEFAULT NULL,
	`surname` VARCHAR(50) NULL DEFAULT NULL,
	`email` VARCHAR(100) NULL DEFAULT NULL,
	`website` VARCHAR(100) NULL DEFAULT NULL,
	`phone` VARCHAR(100) NULL DEFAULT NULL,
	`country` VARCHAR(100) NULL DEFAULT NULL,
	`password` VARCHAR(100) NULL DEFAULT NULL,
	`culture` VARCHAR(20) NULL DEFAULT 'en-US',
	`registration_date` DATETIME NOT NULL,
	`isAdmin` TINYINT(4) NOT NULL DEFAULT '0',
	`status` INT(10) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id`),
	UNIQUE INDEX `email` (`email`)
);

