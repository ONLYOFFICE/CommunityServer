DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade81 DLM00

CREATE PROCEDURE upgrade81()
BEGIN
	IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'audit_events' AND `INDEX_NAME` = 'tenant_id') THEN
		ALTER TABLE `audit_events` DROP INDEX `tenant_id`;
	END IF;
	IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'audit_events' AND `INDEX_NAME` = 'date') THEN
		ALTER TABLE `audit_events` DROP INDEX `date`, ADD INDEX `date` (`tenant_id`, `date`);
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'backup_backup' AND `INDEX_NAME` = 'expires_on') THEN
		ALTER TABLE `backup_backup` ADD INDEX `expires_on` (`expires_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'backup_backup' AND `INDEX_NAME` = 'is_scheduled') THEN
		ALTER TABLE `backup_backup` ADD INDEX `is_scheduled` (`is_scheduled`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'backup_schedule' AND COLUMN_NAME = 'backup_mail') THEN
		ALTER TABLE `backup_schedule` ADD `backup_mail` INT(11) NOT NULL DEFAULT 0;
	END IF;

	ALTER TABLE `crm_organisation_logo` CHANGE COLUMN `content` `content` MEDIUMTEXT NULL DEFAULT NULL;
	ALTER TABLE `crm_tag` ALTER `title` DROP DEFAULT;
	ALTER TABLE `crm_tag` CHANGE COLUMN `title` `title` VARCHAR(255) CHARACTER SET 'utf8' COLLATE 'utf8_bin' NOT NULL;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_voip_calls_history' AND COLUMN_NAME = 'price') THEN
		ALTER TABLE `crm_voip_calls_history`
			CHANGE `dial_date` `end_dial_date` DATETIME DEFAULT NULL,
			ADD `record_url` TEXT AFTER `end_dial_date`,
			ADD `record_duration` int(11) DEFAULT NULL AFTER `record_url`,
			ADD `price` decimal(10,4) DEFAULT NULL AFTER `record_duration`;
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'files_security' AND `INDEX_NAME` = 'tenant_id') THEN
		ALTER TABLE `files_security`
			CHANGE `entry_id` `entry_id` VARCHAR(50) NOT NULL,
			ADD INDEX `tenant_id` (`tenant_id`, `entry_type`, `entry_id`, `owner`);
	END IF;

	IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_alerts' AND `INDEX_NAME` = 'tenant_id_user') THEN
		ALTER TABLE `mail_alerts` DROP INDEX `tenant_id_user`;
	END IF;

	CREATE TABLE IF NOT EXISTS `mail_server_dns` (
		`id` int(11) unsigned NOT NULL AUTO_INCREMENT,
		`tenant` int(11) NOT NULL,
		`id_user` varchar(255) NOT NULL,
		`id_domain` int(11) NOT NULL DEFAULT '-1',
		`dkim_selector` varchar(63) NOT NULL DEFAULT 'dkim',
		`dkim_private_key` text,
		`dkim_public_key` text,
		`domain_check` text,
		`spf` text,
		`time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY (`id`),
		KEY `id_domain_tenant_id_user` (`id_domain`,`tenant`,`id_user`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `mail_server_mail_group` (
		`id` INT(11) NOT NULL AUTO_INCREMENT,
		`id_tenant` INT(11) NOT NULL,
		`id_address` INT(11) NOT NULL,
		`date_created` DATETIME NOT NULL,
		`address` VARCHAR(320) NOT NULL,
		PRIMARY KEY (`id`),
		INDEX `mail_server_address_fk_id` (`id_address`),
		INDEX `tenant` (`id_tenant`)
	) COLLATE='utf8_general_ci' ENGINE=InnoDB;
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_server_mail_group' AND COLUMN_NAME = 'address') THEN
		ALTER TABLE `mail_server_mail_group` ADD `address` varchar(320) NOT NULL;
	END IF;

	ALTER TABLE `projects_tasks`
		DROP INDEX `milestone_id`,
		DROP INDEX `tenant_id`,
		ADD INDEX `milestone_id` (`tenant_id`,`milestone_id`),
		ADD INDEX `tenant_id` (`tenant_id`,`project_id`);

	CREATE TABLE IF NOT EXISTS `webstudio_index` (
		`index_name` varchar(50) NOT NULL,
		`last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY (`index_name`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;
END DLM00

CALL upgrade81() DLM00

DELIMITER ;