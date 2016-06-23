DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade89 DLM00

CREATE PROCEDURE upgrade89()
BEGIN

    IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'stat_enterprise_data') THEN
        CREATE TABLE `stat_enterprise_data` (
			`id` VARCHAR(36) NOT NULL,
			`email` VARCHAR(255) NULL DEFAULT NULL,
			`clientname` VARCHAR(255) NULL DEFAULT NULL,
			`access` VARCHAR(255) NULL DEFAULT NULL,
			`portaladdress` VARCHAR(255) NULL DEFAULT NULL,
			`history` TEXT NULL,
			PRIMARY KEY (`id`)
		)
		COLLATE='utf8_general_ci'
		ENGINE=InnoDB;
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'stat_monitoring') THEN
		CREATE TABLE `stat_monitoring` (
			`url` VARCHAR(255) NOT NULL,
			`name` VARCHAR(255) NOT NULL,
			PRIMARY KEY (`name`)
		)
		COLLATE='utf8_general_ci'
		ENGINE=InnoDB;
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'stat_snapshot') THEN
		CREATE TABLE `stat_snapshot` (
			`Tenant` VARCHAR(100) NOT NULL,
			`UtcDate` DATETIME NOT NULL,
			`UsedSize` BIGINT(11) NOT NULL DEFAULT '0',
			`UsersCount` INT(11) NOT NULL DEFAULT '0',
			`HitsToday` INT(11) NOT NULL DEFAULT '0',
			`ActionsToday` INT(11) NOT NULL DEFAULT '0',
			`BusinessToday` INT(11) NOT NULL DEFAULT '0',
			PRIMARY KEY (`Tenant`, `UtcDate`),
			INDEX `UtcDate` (`UtcDate`)
		)
		COLLATE='utf8_general_ci'
		ENGINE=InnoDB
		;
    END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'regions') THEN
		CREATE TABLE `regions` (
			`region` VARCHAR(32) NOT NULL,
			`connection_string` VARCHAR(1024) NOT NULL,
			`provider` VARCHAR(64) NOT NULL,
			PRIMARY KEY (`region`)
		)
		COLLATE='utf8_general_ci'
		ENGINE=InnoDB;
    END IF;
        
    IF EXISTS(SELECT * FROM mail_mailbox_server t WHERE t.hostname LIKE '%aol.com' AND t.is_user_data = 0 AND t.username = '%EMAILLOCALPART%') THEN
        UPDATE mail_mailbox_server t SET t.username = '%EMAILADDRESS%' WHERE t.hostname LIKE '%aol.com' AND t.is_user_data = 0 AND t.username = '%EMAILLOCALPART%';
    END IF;
	
    INSERT IGNORE INTO `mail_imap_special_mailbox` VALUES ('imap.googlemail.com', 'Migrated', 1, 1);
    INSERT IGNORE INTO `mail_imap_special_mailbox` VALUES ('imap.googlemail.com', '[Gmail]', 1, 1);
    
END DLM00

CALL upgrade89() DLM00

DELIMITER ;