DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade95 DLM00

CREATE PROCEDURE upgrade95()
BEGIN

    UPDATE `tenants_quotarow` SET `tag`='BF88953E-3C43-4850-A3FB-B1E43AD53A3E' WHERE `path`='/talk/';

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.xlsx');


    UPDATE `core_user` SET `sid` = REPLACE(`sid`, '-000', '-') WHERE `sid` like 'S-_-000_-%';

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'audit_events' AND COLUMN_NAME = 'target') THEN
        ALTER TABLE `audit_events` ADD COLUMN `target` TEXT NULL DEFAULT NULL AFTER `description`;
    END IF;

    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_PacificFranc', 'XPF', 'franc', 'NC', 1, 0);

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_settings' AND COLUMN_NAME = 'last_modified') THEN
        ALTER TABLE `core_settings` ADD COLUMN `last_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER `value`;
        UPDATE `core_settings` SET `last_modified`=CURRENT_TIMESTAMP();
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'files_thirdparty_app' AND COLUMN_NAME = 'modified_on' AND DATA_TYPE= 'datetime') THEN
        ALTER TABLE `files_thirdparty_app` CHANGE COLUMN `modified_on` `modified_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER `tenant_id`;
    END IF;

END DLM00

CALL upgrade95() DLM00

DELIMITER ;