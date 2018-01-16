DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade90 DLM00

CREATE PROCEDURE upgrade90()
BEGIN
	IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_tax' AND COLUMN_NAME = 'rate') <> 'DECIMAL(10,2)' THEN
		ALTER TABLE `crm_invoice_tax` CHANGE COLUMN `rate` `rate` DECIMAL(10,2) NOT NULL DEFAULT '0.00' AFTER `description`;
	END IF;
	
	IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mail' AND `INDEX_NAME` = 'main') THEN
		DROP TABLE IF EXISTS `mail_mail_copy`;
		CREATE TABLE IF NOT EXISTS `mail_mail_copy` (
			`id` INT(11) NOT NULL AUTO_INCREMENT,
			`id_mailbox` INT(11) NOT NULL DEFAULT '0',
			`id_user` VARCHAR(255) NOT NULL,
			`tenant` INT(11) NOT NULL,
			`uidl` VARCHAR(255) NULL DEFAULT NULL,
			`md5` VARCHAR(255) NULL DEFAULT NULL,
			`address` VARCHAR(255) NOT NULL,
			`from_text` TEXT NULL,
			`to_text` TEXT NULL,
			`reply_to` TEXT NULL,
			`cc` TEXT NULL,
			`bcc` TEXT NULL,
			`subject` TEXT NULL,
			`introduction` VARCHAR(255) NOT NULL DEFAULT '',
			`importance` TINYINT(1) NOT NULL DEFAULT '0',
			`date_received` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00',
			`date_sent` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00',
			`size` INT(11) NOT NULL DEFAULT '0',
			`attachments_count` INT(11) NOT NULL DEFAULT '0',
			`unread` INT(11) NOT NULL DEFAULT '0',
			`is_answered` INT(11) NOT NULL DEFAULT '0',
			`is_forwarded` INT(11) NOT NULL DEFAULT '0',
			`is_from_crm` INT(11) NOT NULL DEFAULT '0',
			`is_from_tl` INT(11) NOT NULL DEFAULT '0',
			`is_text_body_only` INT(11) NOT NULL DEFAULT '0',
			`has_parse_error` TINYINT(1) NOT NULL DEFAULT '0',
			`calendar_uid` VARCHAR(255) NULL DEFAULT NULL,
			`stream` VARCHAR(38) NOT NULL,
			`folder` INT(11) NOT NULL DEFAULT '1',
			`folder_restore` INT(11) NOT NULL DEFAULT '1',
			`spam` INT(11) NOT NULL DEFAULT '0',
			`time_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
			`is_removed` TINYINT(1) NOT NULL DEFAULT '0',
			`mime_message_id` VARCHAR(255) NULL DEFAULT NULL,
			`mime_in_reply_to` VARCHAR(255) NULL DEFAULT NULL,
			`chain_id` VARCHAR(255) NULL DEFAULT NULL,
			`chain_date` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00',
			PRIMARY KEY (`id`),
			INDEX `chain_index_folders` (`chain_id`, `id_mailbox`, `folder`),
			INDEX `uidl` (`uidl`, `id_mailbox`),
			INDEX `mime_message_id` (`id_mailbox`, `mime_message_id`),
			INDEX `md5` (`md5`, `id_mailbox`),
			INDEX `list_conversations` (`tenant`, `id_user`, `folder`, `chain_date`),
			INDEX `list_messages` (`tenant`, `id_user`, `folder`, `date_sent`),
			INDEX `time_modified` (`time_modified`)
		);
		INSERT INTO `mail_mail_copy` (`id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `has_parse_error`, `calendar_uid`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date`)
							   SELECT `id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `has_parse_error`, `calendar_uid`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date` FROM `mail_mail`;

		DROP TABLE mail_mail;
		RENAME TABLE `mail_mail_copy` TO `mail_mail`;
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_voip_calls' AND COLUMN_NAME = 'record_price') THEN
		ALTER TABLE `crm_voip_calls`
			ADD COLUMN `record_price` DECIMAL(10,4) NULL AFTER `record_duration`;			
    	END IF;
    
    	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_voip_calls' AND COLUMN_NAME = 'record_sid') THEN
		ALTER TABLE `crm_voip_calls`
			ADD COLUMN `record_sid` VARCHAR(50) NULL AFTER `dial_duration`;		
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_voip_calls' AND COLUMN_NAME = 'parent_call_id') THEN
		ALTER TABLE `crm_voip_calls`
			ADD COLUMN `parent_call_id` VARCHAR(50) NULL AFTER `id`,
         ADD INDEX `parent_call_id` (`parent_call_id`, `tenant_id`);			
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_chain' AND `INDEX_NAME` = 'clean_folder') THEN
		ALTER TABLE `mail_chain`
			ADD INDEX `clean_folder` (`tenant`, `id_user`, `folder`);
	END IF;
	
	replace into core_acl (tenant, subject, action, object, acetype) values (-1, "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "77777777-32ae-425f-99b5-83176061d1ae", "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|46cfa73af32046cf8d5bcd82e1d67f26", 0);

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_NewBelarusianRuble', 'BYN', 'Br', 'BY', 0, 0);

	IF NOT EXISTS(SELECT * FROM information_schema.`TABLES` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_report_file') THEN
		CREATE TABLE `crm_report_file` (
			`file_id` int(11) NOT NULL,
			`report_type` int(11) NOT NULL,
			`create_on` datetime NOT NULL,
			`create_by` char(38) NOT NULL,
			`tenant_id` int(11) NOT NULL,
			PRIMARY KEY (`file_id`),
			KEY `tenant_id` (`tenant_id`),
			KEY `create_by` (`create_by`),
			KEY `create_on` (`create_on`)
		)
		COLLATE='utf8_general_ci'
		ENGINE=InnoDB;
    END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_currency_rate' AND `INDEX_NAME` = 'from_currency') THEN
		ALTER TABLE `crm_currency_rate`
			ADD INDEX `from_currency` (`from_currency`);
    END IF;
	
END DLM00

CALL upgrade90() DLM00

DELIMITER ;