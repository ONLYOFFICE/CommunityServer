DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade125 DLM00

CREATE PROCEDURE upgrade125()
BEGIN

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_iprestrictions' AND COLUMN_NAME = 'for_admin') THEN
        ALTER TABLE `tenants_iprestrictions` ADD COLUMN `for_admin` TINYINT(1) NOT NULL AFTER `ip`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'feed_users' AND COLUMN_NAME = 'aggregated_date') THEN

        ALTER TABLE `feed_users` ADD COLUMN `aggregated_date` DATETIME NULL AFTER `user_id`;
        ALTER TABLE `feed_users` ADD INDEX `aggregated_date` (`aggregated_date`);

        UPDATE feed_users fu
        INNER JOIN feed_aggregate fa ON fu.feed_id = fa.id
        SET fu.aggregated_date = fa.aggregated_date;

    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'files_security' AND COLUMN_NAME = 'subject_type') THEN
        ALTER TABLE `files_security` ADD COLUMN `subject_type` TINYINT UNSIGNED NOT NULL AFTER `subject`, ADD COLUMN `options` TEXT NULL AFTER `timestamp`;
        UPDATE files_security SET subject_type=1 WHERE subject='d77bd6af-828b-41f5-84ed-7ffe2565b13a';
        UPDATE files_security SET subject_type=2 WHERE subject='ee7a7468-cda5-4f8b-afdb-f4e42c318eb6' or subject='aafd9c26-9686-4996-9665-35ca72721c4c';
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_user' AND COLUMN_NAME = 'user_lead') THEN
        ALTER TABLE `core_user` ADD COLUMN `user_lead` VARCHAR(36) NULL AFTER `last_modified`;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_user' AND COLUMN_NAME = 'department') THEN
        ALTER TABLE `core_user` DROP COLUMN `department`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_quotarow' AND COLUMN_NAME = 'user_id') THEN
        ALTER TABLE `tenants_quotarow` ADD COLUMN `user_id` VARCHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000' AFTER `last_modified`, DROP PRIMARY KEY, ADD PRIMARY KEY (`tenant`, `path`, `user_id`);
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'jabber_offmessage' AND COLUMN_NAME = 'stamp') THEN
        ALTER TABLE `jabber_offmessage` ADD COLUMN `stamp` DATETIME NULL AFTER `message`, ADD INDEX `jabber_offmessage_stamp` (`stamp`);
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'backup_backup' AND COLUMN_NAME = 'removed') THEN
        ALTER TABLE `backup_backup` ADD COLUMN `removed` tinyint(1) NOT NULL DEFAULT '0' AFTER `hash`;
    END IF;

    UPDATE `crm_currency_info` SET `is_convertable` = 0 WHERE `abbreviation` IN ('LTL', 'LVL', 'VEF');
    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_AngolanKwanza', 'AOA', 'Kz', 'AO', 0, 0);
    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_VenezuelanBolivarSoberano', 'VES', 'Bs.S', 'VE', 1, 0);

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dps', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dpt', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.et', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ett', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.htm', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mhtml', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pdf', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.stw', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxc', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.odp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.ppsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.ppsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxi', '.pptx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.sxw', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wps', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.wpt', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.csv');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.ods');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.xlsx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsb', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xps', '.txt');

END DLM00

CALL upgrade125() DLM00

DELIMITER ;
