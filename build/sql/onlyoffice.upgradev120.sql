DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade120 DLM00

CREATE PROCEDURE upgrade120()
BEGIN

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_quota' AND COLUMN_NAME = 'price2') THEN
        ALTER TABLE `tenants_quota` DROP COLUMN `price2`;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_usersecurity' AND COLUMN_NAME = 'pwdhashsha512') THEN
        ALTER TABLE `core_usersecurity` DROP COLUMN `pwdhashsha512`;
    END IF;

    CREATE TABLE IF NOT EXISTS `core_userdav` (
    `tenant_id` int(11) NOT NULL,
    `user_id` varchar(255) NOT NULL,
    PRIMARY KEY (`user_id`),
    KEY `tenant_id` (`tenant_id`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'tenants_tenants' AND `INDEX_NAME` = 'status') THEN
        ALTER TABLE `tenants_tenants` ADD INDEX `status` (`status`);
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'files_thirdparty_account' AND `INDEX_NAME` = 'tenant_id') THEN
        ALTER TABLE `files_thirdparty_account` ADD INDEX `tenant_id` (`tenant_id`);
    END IF;

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.txt');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.epub');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.epub');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.fb2');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.fb2');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.html');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.html');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.dotx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.dotx');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.ott');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.ott');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.potx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.potx');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.otp');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.otp');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.csv', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ods', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xls', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsx', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.xltx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.xltx');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.csv', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ods', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xls', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsx', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.ots');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.ots');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oxps', '.pdf');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotm', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.docm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.docm');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docm', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dot', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.dotx', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fb2', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodt', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.dotm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xml', '.dotm');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.csv', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ods', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xls', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsx', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltm', '.xlsm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.xlsm');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.csv', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fods', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ods', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xls', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsm', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlsx', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xlt', '.xltm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xltx', '.xltm');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potm', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.pptm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.pptm');

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.fodp', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pot', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.potx', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsm', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppsx', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptm', '.potm');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pptx', '.potm');

    CREATE TABLE IF NOT EXISTS `files_properties` (
        `tenant_id` int(10) NOT NULL,
        `entry_id` varchar(32) NOT NULL,
        `data` MEDIUMTEXT NOT NULL,
        PRIMARY KEY (`tenant_id`, `entry_id`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_EthiopianBirr', 'ETB', 'Br', 'ET', 0, 0);

    UPDATE `crm_currency_info` SET `symbol`='₼' WHERE `abbreviation`='AZN';

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'login_events' AND COLUMN_NAME = 'active') THEN
        ALTER TABLE `login_events` ADD COLUMN `active` INT(10) NOT NULL DEFAULT '0' AFTER `description`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'login_events' AND `INDEX_NAME` = 'active') THEN
        ALTER TABLE `login_events` ADD INDEX `active` (`active`);
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_events' AND COLUMN_NAME = 'has_attachments') THEN
        ALTER TABLE `calendar_events` ADD COLUMN `has_attachments` TINYINT(1) NOT NULL DEFAULT '0' AFTER `time_zone`;
    END IF;

    ALTER TABLE `mail_mail` ROW_FORMAT=DYNAMIC;
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_mail' AND COLUMN_NAME = 'read_request_status') THEN
        ALTER TABLE `mail_mail` ADD COLUMN `read_request_status` TINYINT(1) NOT NULL DEFAULT '0' AFTER `chain_date`;
    END IF;

    IF NOT EXISTS(SELECT * FROM core_settings WHERE tenant = -1 AND id = 'CustomMode') THEN
        UPDATE `core_settings`
        SET `value`=0xF547048A4865171587D9CEBC8A496C601D96031F2C1C3E9160353942EE765DACD316F4B5F42892436FC4A21B9A6DF8FFB511FD7F2A41135A1ACECB919F9FF3691847BCA535553925BAFB396012A2DA500065FA6442611833F0D7D8A969640FFD1D53B6ECCB3544FB029695943A88542597525CE31E346F289A9B077E9564A9570A81E48AB1654D43B1D8BDC901D588D86BC6FDDD6AFFC611440E6E5AAEB644DEBC4D9D131A4456610F5118ABD672BFAF383830347D52FD714729C9050876A2BF63C430C6DF4FCCAE1F61EC14D5DA5522104AC4D1EB0E47D12083C3540B424A1373FF6345EBD2CC0F0D048F7F987DD45B
        WHERE `tenant`=-1 AND `id`='CompanyWhiteLabelSettings';
    END IF;

    IF EXISTS(SELECT * FROM core_settings WHERE tenant = -1 AND id = 'CustomMode') THEN
        UPDATE `core_settings`
        SET `value`=0xF547048A4865171587D9CEBC8A496C6058DF4F90020290E3BB59C0B7C027FBA70FB0863A8FBC33232A20DB226B8D5D4F7B363F0C4497E997383720E1B39BB95416F7227F55DEBADDF28FC81D15971C1E1417B9C652B36964BAC4820225F7127BD391ED7A57C1F65C0AEB3450AB23603D5D49EA55AA20A98198CA01741ED1D971FCE56FD6AA98FF466DC6330CE50C66461D08B1BAE7C87218A48504BBD846AA83CF1FF0DB6D377D39C972C61EFCD682088EE868FB230A032DAEF2AD037BD4E19209FAE102500553888219C39F3B404B7166EFDCDC6E64AB6D48116512C00CA720B34591D75672932931B3A0EFF63751B8CE77A275C2614A98056A431B74683F411536C17EBB200FABB222CA0E1BBE58AB84D94CC1179C57ED6723227DD8BCA296
        WHERE `tenant`=-1 AND `id`='CompanyWhiteLabelSettings';

        DELETE FROM `files_converts` WHERE `input` = '.docx' AND `output` = '.docxf';
        DELETE FROM `files_converts` WHERE `input` = '.docxf';
    END IF;

END DLM00

CALL upgrade120() DLM00

DELIMITER ;
