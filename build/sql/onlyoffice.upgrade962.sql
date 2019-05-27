DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade962 DLM00

CREATE PROCEDURE upgrade962()
BEGIN

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_tenants' AND COLUMN_NAME = 'spam') THEN
		ALTER TABLE `tenants_tenants` ADD COLUMN `spam` INT(10) NOT NULL DEFAULT '1' AFTER `last_modified`;
		ALTER TABLE `tenants_tenants` ADD COLUMN `calls` INT(10) NOT NULL DEFAULT '1' AFTER `spam`;
    END IF;

	INSERT IGNORE INTO `core_subscription` (`tenant`, `source`, `action`, `recipient`, `object`) VALUES (-1, 'asc.web.studio', 'periodic_notify', 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '');
	INSERT IGNORE INTO `core_subscriptionmethod` (`tenant`, `source`, `action`, `recipient`, `sender`) values(-1, 'asc.web.studio', 'periodic_notify', 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'email.sender');

    UPDATE `tenants_quota` SET `max_total_size`='10995116277760' WHERE `tenant`=-1 AND `features` LIKE "%controlpanel%";

END DLM00

CALL upgrade962() DLM00

DELIMITER ;