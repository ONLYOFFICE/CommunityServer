DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade115 DLM00

CREATE PROCEDURE upgrade115()
BEGIN

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.txt');

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_events' AND COLUMN_NAME = 'time_zone') THEN
        ALTER TABLE `calendar_events` ADD COLUMN `time_zone` VARCHAR(255) NULL DEFAULT NULL AFTER `status`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'backup_backup' AND COLUMN_NAME = 'hash') THEN
        ALTER TABLE `backup_backup`	ADD COLUMN `hash` char(64) NOT NULL AFTER `storage_params`;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_tariff' AND COLUMN_NAME = 'tariff_key') THEN
        ALTER TABLE `tenants_tariff` DROP COLUMN `tariff_key`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_tariff' AND COLUMN_NAME = 'quantity') THEN
        ALTER TABLE `tenants_tariff` ADD COLUMN `quantity` int(10) NOT NULL DEFAULT 1 AFTER `stamp`;
    END IF;

    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_MongolianTugrik', 'MNT', '₮', 'MN', 0, 0);

    UPDATE `tenants_quota` SET `features` = 'domain,audit,controlpanel,healthcheck,ldap,sso,whitelabel,branding,ssbranding,update,support,portals:10000,discencryption,privacyroom,restore,contentsearch' WHERE `tenant` = -1 and `name` NOT LIKE '%saas%';

END DLM00

CALL upgrade115() DLM00

DELIMITER ;
