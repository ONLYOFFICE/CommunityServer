DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade88 DLM00

CREATE PROCEDURE upgrade88()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_contacts' AND COLUMN_NAME = 'description') THEN
        ALTER TABLE `mail_contacts` ADD COLUMN `description` VARCHAR(100) NULL DEFAULT NULL AFTER `address`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_contacts' AND COLUMN_NAME = 'type') THEN
        ALTER TABLE `mail_contacts` ADD COLUMN `type` INT(11) NOT NULL AFTER `description`;
    END IF;
    
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_contacts' AND COLUMN_NAME = 'has_photo') THEN
        ALTER TABLE `mail_contacts` ADD COLUMN `has_photo` TINYINT(1) NOT NULL DEFAULT '0' AFTER `type`,
        CHANGE COLUMN `last_modified` `last_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER `has_photo`;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_contacts' AND `INDEX_NAME` = 'id_user_name_address') THEN
        ALTER TABLE `mail_contacts` DROP INDEX `id_user_name_address`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_contacts' AND `INDEX_NAME` = 'tenant_id_user_name_address') THEN
        ALTER TABLE `mail_contacts` ADD INDEX `tenant_id_user_name_address` (`tenant`, `id_user`, `address`);
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_contacts' AND `INDEX_NAME` = 'tenant_id' AND `COLUMN_NAME` = 'tenant') THEN
        ALTER TABLE `mail_contacts` DROP INDEX `last_modified`, ADD INDEX `last_modified` (`last_modified`);
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_contact_info') THEN
        CREATE TABLE `mail_contact_info` (
            `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
            `tenant` INT(11) NOT NULL,
            `id_user` VARCHAR(255) NOT NULL,
            `id_contact` INT(11) UNSIGNED NOT NULL,
            `data` VARCHAR(255) NOT NULL,
            `type` INT(11) NOT NULL,
            `is_primary` TINYINT(1) NOT NULL DEFAULT '0',
            `last_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (`id`),
            INDEX `last_modified` (`last_modified`),
            INDEX `contact_id` (`id_contact`),
            INDEX `tenant_id_user_data` (`tenant`, `id_user`, `data`)
        )
        COLLATE='utf8_general_ci'
        ENGINE=InnoDB;

        INSERT IGNORE INTO `mail_contact_info` (`tenant`, `id_user`, `id_contact`, `data`, `type`, `is_primary`) SELECT `tenant`, `id_user`, `id`, `address`, 1, 1 FROM `mail_contacts`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mailbox_signature' AND `INDEX_NAME` = 'tenant') THEN
        ALTER TABLE `mail_mailbox_signature` DROP PRIMARY KEY, ADD PRIMARY KEY (`id_mailbox`), ADD INDEX `tenant` (`tenant`); 
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'calendar_events' AND COLUMN_NAME = 'uid') THEN
        ALTER TABLE `calendar_events` ADD COLUMN `uid` VARCHAR(255) NULL DEFAULT NULL, ADD COLUMN `status` SMALLINT NOT NULL DEFAULT 0;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mail' AND COLUMN_NAME = 'calendar_uid') THEN
        ALTER TABLE `mail_mail` ADD COLUMN `calendar_uid` VARCHAR(255) NULL AFTER `has_parse_error`;
    END IF;

    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_OmaniRial', 'OMR', 'ر.ع', 'OM', 1, 0);
    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_NigerianNaira', 'NGN', '₦', 'NG', 1, 0);
    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_CubanPeso', 'CUP', '$', 'CU', 0, 0);
    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_KenyanShilling', 'KES', 'KSh', 'KE', 1, 0);

    IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mailbox_autoreply') THEN
        CREATE TABLE IF NOT EXISTS `mail_mailbox_autoreply` (
          `id_mailbox` INT(11) NOT NULL,
          `tenant` INT(11) NOT NULL,
          `turn_on` TINYINT(1) NOT NULL,
          `only_contacts` TINYINT(1) NOT NULL,
          `turn_on_to_date` TINYINT(1) NOT NULL,
          `from_date` DATETIME NOT NULL,
          `to_date` DATETIME NOT NULL,
          `subject` TEXT NULL,
          `html` TEXT NULL,
          PRIMARY KEY (`id_mailbox`),
          INDEX `tenant` (`tenant`)
        )
        COLLATE='utf8_general_ci'
        ENGINE=InnoDB;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mailbox_autoreply_history') THEN
        CREATE TABLE IF NOT EXISTS `mail_mailbox_autoreply_history` (
            `id_mailbox` INT(11) NOT NULL,
            `tenant` INT(11) NOT NULL,
            `sending_email` VARCHAR(255) NOT NULL,
            `sending_date` DATETIME NOT NULL,
            PRIMARY KEY (`id_mailbox`, `sending_email`),
            INDEX `tenant` (`tenant`)
        )
        COLLATE='utf8_general_ci'
        ENGINE=InnoDB;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'calendar_event_history') THEN
        CREATE TABLE IF NOT EXISTS `calendar_event_history` (
          `tenant` int(11) NOT NULL,
          `calendar_id` int(11) NOT NULL,
          `event_uid` char(255) NOT NULL,
          `event_id` int(10) NOT NULL DEFAULT '0',
          `ics` text,
          PRIMARY KEY (`tenant`,`calendar_id`,`event_uid`),
          KEY `event_id` (`tenant`,`event_id`)
        )
        COLLATE='utf8_general_ci'
        ENGINE=InnoDB;
    END IF;

END DLM00

CALL upgrade88() DLM00

DELIMITER ;