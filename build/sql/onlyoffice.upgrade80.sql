DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade80 DLM00

CREATE PROCEDURE upgrade80()
BEGIN
	CREATE TABLE IF NOT EXISTS `backup_backup` (
	  `id` char(38) NOT NULL,
	  `tenant_id` int(11) NOT NULL,
	  `is_scheduled` int(1) NOT NULL,
	  `name` varchar(255) NOT NULL,
	  `storage_type` int(11) NOT NULL,
	  `storage_base_path` varchar(255) DEFAULT NULL,
	  `storage_path` varchar(255) NOT NULL,
	  `created_on` datetime NOT NULL,
	  `expires_on` datetime NOT NULL DEFAULT '0001-01-01 00:00:00',
	  PRIMARY KEY (`id`),
	  KEY `tenant_id` (`tenant_id`),
	  KEY `expires_on` (`expires_on`),
	  KEY `is_scheduled` (`is_scheduled`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `backup_schedule` (
	  `tenant_id` int(11) NOT NULL,
	  `cron` varchar(255) NOT NULL,
	  `backups_stored` int(11) NOT NULL,
	  `storage_type` int(11) NOT NULL,
	  `storage_base_path` varchar(255) DEFAULT NULL,
	  `last_backup_time` datetime NOT NULL,
	  PRIMARY KEY (`tenant_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `crm_voip_calls` (
	  `id` varchar(50) NOT NULL,
	  `number_from` varchar(50) NOT NULL,
	  `number_to` varchar(50) NOT NULL,
	  `status` int(10) DEFAULT NULL,
	  `answered_by` varchar(50) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	  `dial_date` datetime DEFAULT NULL,
	  `dial_duration` int(11) DEFAULT NULL,
	  `record_url` text,
	  `record_duration` int(11) DEFAULT NULL,
	  `contact_id` int(10) DEFAULT NULL,
	  `price` decimal(10,4) DEFAULT NULL,
	  `tenant_id` int(10) NOT NULL,
	  PRIMARY KEY (`id`),
	  KEY `tenant_id` (`tenant_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `crm_voip_calls_history` (
	  `id` varchar(50) NOT NULL,
	  `parent_call_id` varchar(50) NOT NULL,
	  `answered_by` varchar(50) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	  `queue_date` datetime DEFAULT NULL,
	  `answer_date` datetime DEFAULT NULL,
	  `end_dial_date` datetime DEFAULT NULL,
	  `record_url` text,
	  `record_duration` int(11) DEFAULT NULL,
	  `price` decimal(10,4) DEFAULT NULL,
	  `tenant_id` int(10) NOT NULL,
	  PRIMARY KEY (`id`),
	  KEY `tenant_id` (`tenant_id`,`parent_call_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

	CREATE TABLE IF NOT EXISTS `crm_voip_number` (
	  `id` varchar(50) NOT NULL,
	  `number` varchar(50) NOT NULL,
	  `alias` varchar(255) DEFAULT NULL,
	  `settings` text,
	  `tenant_id` int(10) NOT NULL,
	  PRIMARY KEY (`id`),
	  KEY `tenant_id` (`tenant_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	ALTER TABLE `files_security` ALTER `entry_id` DROP DEFAULT;
	ALTER TABLE `files_security` CHANGE COLUMN `entry_id` `entry_id` VARCHAR(50) NOT NULL;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'files_security' AND `INDEX_NAME` = 'tenant_id') THEN
		ALTER TABLE `files_security` ADD INDEX `tenant_id` (`tenant_id`, `entry_type`, `entry_id`, `owner`);
	END IF;

	ALTER TABLE `login_events` CHANGE COLUMN `page` `page` VARCHAR(300) NULL DEFAULT NULL;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'projects_messages' AND COLUMN_NAME = 'status') THEN
		ALTER TABLE `projects_messages` ADD COLUMN `status` INT NOT NULL DEFAULT 0 AFTER `title`;
	END IF;

	ALTER TABLE `projects_tasks`
		DROP INDEX `tenant_id`,
		ADD INDEX `tenant_id` (`tenant_id`, `project_id`);

	CREATE TABLE IF NOT EXISTS `tenants_iprestrictions` (
	  `id` int(38) NOT NULL AUTO_INCREMENT,
	  `tenant` int(10) NOT NULL,
	  `ip` varchar(50) NOT NULL,
	  PRIMARY KEY (`id`),
	  KEY `tenant` (`tenant`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `webstudio_index` (
	  `index_name` varchar(50) NOT NULL,
	  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	  PRIMARY KEY (`index_name`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_CentralAfricanFranc', 'XAF', 'franc', 'CF', 1, 0);
	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_WestAfricanFranc', 'XOF', 'franc', 'ML', 1, 0);
	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_DominicanPeso', 'DOP', 'RD$', 'DO', 1, 0);

END DLM00

CALL upgrade80() DLM00

DELIMITER ;