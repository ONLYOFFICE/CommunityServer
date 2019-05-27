DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade91 DLM00

CREATE PROCEDURE upgrade91()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_user' AND COLUMN_NAME = 'sso_name_id') THEN	
        ALTER TABLE `core_user` ADD COLUMN `sso_name_id` VARCHAR(512) NULL DEFAULT NULL AFTER `sid`;
        ALTER TABLE `core_user` ADD COLUMN `sso_session_id` VARCHAR(512) NULL DEFAULT NULL AFTER `sso_name_id`;
    END IF;

    UPDATE `tenants_quota` SET `features`=CONCAT("sso,", `features`) WHERE `tenant`=-1 AND `features` LIKE "%controlpanel%" AND `features` NOT LIKE "%sso%";
    UPDATE `tenants_quota` SET `features`=CONCAT("sso,", `features`) WHERE `tenant`=-1000 AND `features` NOT LIKE "%sso%";

END DLM00

CALL upgrade91() DLM00

DELIMITER ;