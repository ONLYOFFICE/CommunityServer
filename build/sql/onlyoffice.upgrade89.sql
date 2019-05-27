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
    
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mailbox' AND COLUMN_NAME = 'token') THEN
        ALTER TABLE `mail_mailbox`
            ADD COLUMN `token_type` TINYINT(4) NOT NULL DEFAULT '0' AFTER `refresh_token`,
            ADD COLUMN `token` TEXT NULL AFTER `token_type`;
        
        UPDATE mail_mailbox SET token = refresh_token, token_type = service_type WHERE refresh_token IS NOT NULL;        
    END IF;
    
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'res_files' AND COLUMN_NAME = 'creationDate') THEN
        ALTER TABLE `res_files`
            ADD COLUMN `creationDate` TIMESTAMP NOT NULL DEFAULT 0 AFTER `lastUpdate`;             
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'res_cultures' AND COLUMN_NAME = 'creationDate') THEN
        ALTER TABLE `res_cultures`
            ADD COLUMN `creationDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `available`;             
    END IF;	
	
	IF NOT EXISTS(SELECT * FROM core_settings WHERE tenant = -1 AND id = 'CustomMode') THEN	
		replace into core_settings(tenant, id, value) values (-1, 'SmtpSettings', 0xF052E090A1A3750DADCD4E9961DA04AA51EF0197E2C0623CF12C5838BFA40A9B48BAEFCBE371587731D7E3DC9E7C6009742F9E415D56DB0F0AE08E32F8904B2C441CC657C64543EAEE262044A28B4335DCB0F0C4E9401D891FA06369F984CA2D475C86C237917961C5827769831585230A66AC7787E6FB56FD3E37389267A46A);
    END IF;	
	
END DLM00

CALL upgrade89() DLM00

DELIMITER ;