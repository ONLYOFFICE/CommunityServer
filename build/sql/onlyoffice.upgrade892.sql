DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade892 DLM00

CREATE PROCEDURE upgrade892()
BEGIN

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_RomanianLeu', 'RON', 'lei', 'RO', 1, 0);

    UPDATE `tenants_quota` SET `max_total_size`=8192 WHERE `tenant`=-77;
    UPDATE `tenants_quota` SET `max_total_size`=8192 WHERE `tenant`=-78;
    UPDATE `tenants_quota` SET `max_total_size`=20480 WHERE `tenant`=-80;
    UPDATE `tenants_quota` SET `max_total_size`=20480 WHERE `tenant`=-81;
    UPDATE `tenants_quota` SET `max_total_size`=40960 WHERE `tenant`=-83;
    UPDATE `tenants_quota` SET `max_total_size`=40960 WHERE `tenant`=-84;
    UPDATE `tenants_quota` SET `max_total_size`=81920 WHERE `tenant`=-86;
    UPDATE `tenants_quota` SET `max_total_size`=81920 WHERE `tenant`=-87;
    UPDATE `tenants_quota` SET `max_total_size`=122880 WHERE `tenant`=-89;
    UPDATE `tenants_quota` SET `max_total_size`=122880 WHERE `tenant`=-90;
    UPDATE `tenants_quota` SET `max_total_size`=204800 WHERE `tenant`=-92;
    UPDATE `tenants_quota` SET `max_total_size`=204800 WHERE `tenant`=-93;
    UPDATE `tenants_quota` SET `max_total_size`=40960 WHERE `tenant`=-94;
    UPDATE `tenants_quota` SET `max_total_size`=81920 WHERE `tenant`=-95;
    UPDATE `tenants_quota` SET `max_total_size`=122880 WHERE `tenant`=-96;
    UPDATE `tenants_quota` SET `max_total_size`=204800 WHERE `tenant`=-97;

    UPDATE `tenants_quota` SET `features`=CONCAT("sso,", `features`) WHERE `tenant`=-1 AND `features` LIKE "%controlpanel%" AND `features` NOT LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=CONCAT("sso,", `features`) WHERE `tenant`=-1000 AND `features` NOT LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=REPLACE(`features`, "sso,", "") WHERE `tenant`=-1 AND `features` LIKE "%controlpanel%" AND `features` LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=REPLACE(`features`, ",sso", "") WHERE `tenant`=-1 AND `features` LIKE "%controlpanel%" AND `features` LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=REPLACE(`features`, "sso,", "") WHERE `tenant`=-1000 AND `features` LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=REPLACE(`features`, ",sso", "") WHERE `tenant`=-1000 AND `features` LIKE "%sso%";

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'notify_queue' AND COLUMN_NAME = 'attachments') THEN
        ALTER TABLE `notify_queue` ADD COLUMN `attachments` text NULL AFTER `creation_date`;
    END IF;

END DLM00

CALL upgrade892() DLM00

DELIMITER ;