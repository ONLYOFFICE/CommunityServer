DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade105 DLM00

CREATE PROCEDURE upgrade105()
BEGIN
	CREATE TABLE  IF NOT EXISTS `projects_status` (
		`id` INT(11) NOT NULL AUTO_INCREMENT,
		`title` VARCHAR(255) NOT NULL,
		`description` VARCHAR(255) NOT NULL,
		`statusType` TINYINT(2) NOT NULL,
		`image` TEXT NOT NULL,
		`imageType` VARCHAR(50) NOT NULL,
		`color` CHAR(7) NOT NULL,
		`order` TINYINT(3) UNSIGNED NOT NULL,
		`isDefault` TINYINT(1) NOT NULL,
	    `available` TINYINT(1) NOT NULL,
		`tenant_id` INT(11) NOT NULL,
		PRIMARY KEY (`id`),
		INDEX `tenant` (`tenant_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'projects_tasks' AND COLUMN_NAME = 'status_id') THEN
		ALTER TABLE `projects_tasks` ADD COLUMN `status_id` SMALLINT NULL DEFAULT NULL AFTER `status`;
	END IF;

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_TunisianDinar', 'TND', 'DT', 'TN', 1, 0);
	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_QatariRiyal', 'QAR', 'ر.ق.‏', 'QA', 1, 0);

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'files_file' AND COLUMN_NAME = 'forcesave') THEN
        ALTER TABLE `files_file` ADD COLUMN `forcesave` INT(1) NOT NULL DEFAULT '0' AFTER `encrypted`;
    END IF;

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_events' AND COLUMN_NAME = 'rrule') <> 'text' THEN
        ALTER TABLE `calendar_events` CHANGE COLUMN `rrule` `rrule` text NOT NULL AFTER `alert_type`;
    END IF;

CREATE TABLE IF NOT EXISTS `encrypted_data` (
	`public_key` CHAR(64) NOT NULL,
	`file_hash` CHAR(66) NOT NULL,
	`data` CHAR(112) NOT NULL,
	`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	`tenant_id` INT(10) NOT NULL,
	PRIMARY KEY (`public_key`, `file_hash`),
	INDEX `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `dbip_location` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`addr_type` ENUM('ipv4','ipv6') NOT NULL,
	`ip_start` VARCHAR(39) NOT NULL,
	`ip_end` VARCHAR(39) NOT NULL,
	`country` VARCHAR(2) NOT NULL,
	`stateprov` VARCHAR(255) NOT NULL,
	`district` VARCHAR(255) NULL DEFAULT NULL,
	`city` VARCHAR(255) NOT NULL,
	`zipcode` VARCHAR(255) NULL DEFAULT NULL,
	`latitude` FLOAT NULL DEFAULT NULL,
	`longitude` FLOAT NULL DEFAULT NULL,
	`geoname_id` INT(11) NULL DEFAULT NULL,
	`timezone_offset` INT(10) NULL DEFAULT NULL,
	`timezone_name` VARCHAR(255) NULL DEFAULT NULL,
	`processed` INT(11) NOT NULL DEFAULT '1',
	PRIMARY KEY (`id`),
	INDEX `ip_start` (`ip_start`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB;

END DLM00

CALL upgrade105() DLM00

DELIMITER ;
