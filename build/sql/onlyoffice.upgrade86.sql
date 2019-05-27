DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade86 DLM00

CREATE PROCEDURE upgrade86()
BEGIN
	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'jabber_archive' AND `TABLE_COLLATION` = 'utf8mb4_general_ci') THEN
		RENAME TABLE jabber_archive TO jabber_archive_old;
		CREATE TABLE `jabber_archive` (
			`id` INT(11) NOT NULL AUTO_INCREMENT,
			`jid` VARCHAR(255) NOT NULL,
			`stamp` DATETIME NOT NULL,
			`message` MEDIUMTEXT NULL DEFAULT NULL,
			PRIMARY KEY (`id`),
			INDEX `jabber_archive_jid` (`jid`(190))
		)
		COLLATE='utf8mb4_general_ci';
		INSERT INTO jabber_archive SELECT * FROM jabber_archive_old;
		DROP TABLE jabber_archive_old;
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'jabber_offmessage' AND `TABLE_COLLATION` = 'utf8mb4_general_ci') THEN
		RENAME TABLE jabber_offmessage TO jabber_offmessage_old;
		CREATE TABLE `jabber_offmessage` (
			`id` INT(11) NOT NULL AUTO_INCREMENT,
			`jid` VARCHAR(255) NOT NULL,
			`message` MEDIUMTEXT NULL DEFAULT NULL,
			PRIMARY KEY (`id`),
			INDEX `jabber_offmessage_jid` (`jid`(190))
		)
		COLLATE='utf8mb4_general_ci';
		INSERT INTO jabber_offmessage SELECT * FROM jabber_offmessage_old;
		DROP TABLE jabber_offmessage_old;
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'login_events' AND `INDEX_NAME` = 'tenant_id' AND `COLUMN_NAME` = 'user_id') THEN
		ALTER TABLE `login_events`
			DROP INDEX `tenant_id`,
			ADD INDEX `tenant_id` (`tenant_id`, `user_id`);
	END IF;

	IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'res_files' AND `INDEX_NAME` = 'index1') THEN
		ALTER TABLE `res_files`
			DROP INDEX `index1`,
			ADD UNIQUE INDEX `resname` (`resName`);
	END IF;

	ALTER TABLE `mail_server_address`
		CHANGE COLUMN `is_mail_group` `is_mail_group` INT(10) NOT NULL DEFAULT '0' AFTER `id_mailbox`,
		CHANGE COLUMN `is_alias` `is_alias` INT(10) NOT NULL DEFAULT '0' AFTER `is_mail_group`;

	ALTER TABLE `mail_server_domain`
		CHANGE COLUMN `is_verified` `is_verified` INT(10) NOT NULL DEFAULT '0' AFTER `name`;

	ALTER TABLE `mail_server_server`
		CHANGE COLUMN `mx_record` `mx_record` VARCHAR(128) NOT NULL DEFAULT '' AFTER `id`,
		CHANGE COLUMN `connection_string` `connection_string` TEXT NOT NULL AFTER `mx_record`;

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_ColombianPeso', 'COP', '$', 'CO', 1, 0);
	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_CroatianKuna', 'HRK', 'kn', 'HR', 1, 0);
	UPDATE `crm_currency_info` SET `is_convertable` = 1 WHERE `abbreviation` IN ('MGA','BDT') AND `is_convertable` <> 1;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-20;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-21;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-22;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-23;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-25;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-32;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,sms,whitelabel,migration' WHERE `tenant`=-51;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-55;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-56;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-57;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-58;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-59;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-60;
	UPDATE `tenants_quota` SET `features`='backup,domain,docs,year,sms,whitelabel,migration' WHERE `tenant`=-61;
END DLM00

CALL upgrade86() DLM00

DELIMITER ;