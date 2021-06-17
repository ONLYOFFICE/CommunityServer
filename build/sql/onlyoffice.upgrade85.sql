DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade85 DLM00

CREATE PROCEDURE upgrade85()
BEGIN
	IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'audit_events' AND COLUMN_NAME = 'mobile') THEN
		ALTER TABLE `audit_events` DROP COLUMN `mobile`;
	END IF;
	IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'login_events' AND COLUMN_NAME = 'mobile') THEN
		ALTER TABLE `login_events` DROP COLUMN `mobile`;
	END IF;

	drop table if exists core_logging;
	drop table if exists mail_aggregators;
	drop table if exists mail_garbage;
	drop table if exists mail_log;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'feed_aggregate' AND COLUMN_NAME = 'modified_date') THEN
		TRUNCATE TABLE feed_last; 
		TRUNCATE TABLE feed_users;
		DROP TABLE IF EXISTS feed_aggregate;
		CREATE TABLE IF NOT EXISTS `feed_aggregate` (
			`id` VARCHAR(88) NOT NULL,
			`tenant` INT(10) NOT NULL,
			`product` VARCHAR(50) NOT NULL,
			`module` VARCHAR(50) NOT NULL,
			`author` CHAR(38) NOT NULL,
			`modified_by` CHAR(38) NOT NULL,
			`created_date` DATETIME NOT NULL,
			`modified_date` DATETIME NOT NULL,
			`group_id` VARCHAR(70) NULL DEFAULT NULL,
			`json` MEDIUMTEXT NOT NULL,
			`keywords` TEXT NULL,
			`aggregated_date` DATETIME NOT NULL,
			PRIMARY KEY (`id`),
			KEY `product` (`tenant`, `product`),
			KEY `aggregated_date` (`tenant`, `aggregated_date`),
			KEY `modified_date` (`tenant`, `modified_date`)
		);
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_attachment' AND COLUMN_NAME = 'id_mailbox') THEN
		ALTER TABLE `mail_attachment` ADD COLUMN `id_mailbox` INT NOT NULL DEFAULT '0' AFTER `tenant`, ADD INDEX `id_mailbox` (`id_mailbox`, `tenant`);
		UPDATE mail_attachment t INNER JOIN mail_mail m ON t.id_mail = m.id SET t.id_mailbox = m.id_mailbox;
	END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_mailbox' AND COLUMN_NAME = 'date_created') THEN
		ALTER TABLE `mail_mailbox` ADD COLUMN `date_created` DATETIME NULL DEFAULT NULL AFTER `email_in_folder`;
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_mailbox' AND COLUMN_NAME = 'date_modified') THEN
		ALTER TABLE `mail_mailbox`
			CHANGE COLUMN `date_created` `date_created` DATETIME NULL DEFAULT NULL AFTER `email_in_folder`,
			ADD COLUMN `date_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP AFTER `date_created`,
			ADD COLUMN `date_checked` DATETIME NULL DEFAULT NULL AFTER `date_created`,
			ADD COLUMN `date_user_checked` DATETIME NULL DEFAULT NULL AFTER `date_checked`,
			ADD COLUMN `user_online` TINYINT(1) NOT NULL DEFAULT '0' AFTER `date_user_checked`,
			ADD COLUMN `date_login_delay_expires` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00' AFTER `date_user_checked`,
			ADD COLUMN `date_auth_error` DATETIME NULL DEFAULT NULL AFTER `date_login_delay_expires`,
			ADD COLUMN `is_default` TINYINT(1) NOT NULL DEFAULT '0' AFTER `is_removed`,
			ADD COLUMN `imap_intervals` mediumtext NULL DEFAULT NULL,
			ADD INDEX `date_login_delay_expires` (`date_checked`, `date_login_delay_expires`);
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mail' AND `INDEX_NAME` = 'uidl') THEN
		DROP TABLE IF EXISTS `mail_mail_copy`;
		CREATE TABLE IF NOT EXISTS `mail_mail_copy` (
		  `id` int(11) NOT NULL AUTO_INCREMENT,
		  `id_mailbox` int(11) NOT NULL DEFAULT '0',
		  `id_user` varchar(255) NOT NULL,
		  `tenant` int(11) NOT NULL,
		  `uidl` varchar(255) DEFAULT NULL,
		  `md5` varchar(255) DEFAULT NULL,
		  `address` varchar(255) NOT NULL,
		  `from_text` text,
		  `to_text` text,
		  `reply_to` text,
		  `cc` text,
		  `bcc` text,
		  `subject` text,
		  `introduction` varchar(255) NOT NULL DEFAULT '',
		  `importance` tinyint(1) NOT NULL DEFAULT '0',
		  `date_received` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
		  `date_sent` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
		  `size` int(11) NOT NULL DEFAULT '0',
		  `attachments_count` int(11) NOT NULL DEFAULT '0',
		  `unread` int(11) NOT NULL DEFAULT '0',
		  `is_answered` int(11) NOT NULL DEFAULT '0',
		  `is_forwarded` int(11) NOT NULL DEFAULT '0',
		  `is_from_crm` int(11) NOT NULL DEFAULT '0',
		  `is_from_tl` int(11) NOT NULL DEFAULT '0',
		  `is_text_body_only` int(11) NOT NULL DEFAULT '0',
		  `has_parse_error` tinyint(1) NOT NULL DEFAULT '0',
		  `stream` varchar(38) NOT NULL,
		  `folder` int(11) NOT NULL DEFAULT '1',
		  `folder_restore` int(11) NOT NULL DEFAULT '1',
		  `spam` int(11) NOT NULL DEFAULT '0',
		  `time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
		  `is_removed` tinyint(1) NOT NULL DEFAULT '0',
		  `mime_message_id` varchar(255) DEFAULT NULL,
		  `mime_in_reply_to` varchar(255) DEFAULT NULL,
		  `chain_id` varchar(255) DEFAULT NULL,
		  `chain_date` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
		  PRIMARY KEY (`id`),
		  KEY `chain_index_folders` (`chain_id`,`id_mailbox`,`folder`),
		  KEY `uidl` (`uidl`,`id_mailbox`),
		  KEY `mime_message_id` (`id_mailbox`,`mime_message_id`),
		  KEY `md5` (`md5`,`id_mailbox`),
		  KEY `main` (`tenant`,`id_user`,`folder`,`chain_date`),
		  KEY `time_modified` (`time_modified`)
		);
		INSERT INTO `mail_mail_copy` (`id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `has_parse_error`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date`)
							   SELECT `id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `has_parse_error`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date` FROM `mail_mail`;
		DROP TABLE mail_mail;
		RENAME TABLE `mail_mail_copy` TO `mail_mail`;
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_field_value' AND COLUMN_NAME = 'id') THEN
		ALTER TABLE `crm_field_value`
			ADD COLUMN `id` INT NOT NULL AUTO_INCREMENT FIRST,
			DROP PRIMARY KEY,
			ADD PRIMARY KEY (`id`),
			ADD INDEX `tenant_id` (`tenant_id`, `entity_id`, `entity_type`, `field_id`);
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'wiki_pages' AND COLUMN_NAME = 'id') THEN
		ALTER TABLE `wiki_pages`
			ADD COLUMN `id` INT NOT NULL AUTO_INCREMENT FIRST,
			DROP PRIMARY KEY,
			ADD PRIMARY KEY (`id`, `tenant`, `pagename`);
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'blogs_posts' AND COLUMN_NAME = 'post_id') THEN
		ALTER TABLE `blogs_posts`
			ADD COLUMN `post_id` INT NOT NULL AUTO_INCREMENT FIRST,
			DROP PRIMARY KEY,
			ADD INDEX `Tenant` (`Tenant`, `id`),
			ADD PRIMARY KEY (`post_id`);
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'projects_comments' AND COLUMN_NAME = 'comment_id') THEN
		ALTER TABLE `projects_comments`
			ADD COLUMN `comment_id` INT NOT NULL AUTO_INCREMENT FIRST,
			DROP PRIMARY KEY,
			ADD PRIMARY KEY (`comment_id`, `id`);
	END IF;


	IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'jabber_private' AND COLUMN_NAME = 'element') <> 'mediumtext' THEN
		ALTER TABLE `jabber_private` CHANGE COLUMN `element` `element` MEDIUMTEXT NULL;
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'files_file' AND COLUMN_NAME = 'changes') THEN
		ALTER TABLE `files_file` ADD COLUMN `changes` MEDIUMTEXT NULL DEFAULT NULL;
	END IF;
	

	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'projects_projects' AND `INDEX_NAME` = 'last_modified_on') THEN
		ALTER TABLE `projects_projects` ADD INDEX `last_modified_on` (`last_modified_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'projects_tasks' AND `INDEX_NAME` = 'last_modified_on') THEN
		ALTER TABLE `projects_tasks` ADD INDEX `last_modified_on` (`last_modified_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'projects_messages' AND `INDEX_NAME` = 'last_modified_on') THEN
		ALTER TABLE `projects_messages` ADD INDEX `last_modified_on` (`last_modified_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'projects_milestones' AND `INDEX_NAME` = 'last_modified_on') THEN
		ALTER TABLE `projects_milestones` ADD INDEX `last_modified_on` (`last_modified_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_deal' AND `INDEX_NAME` = 'last_modifed_on') THEN
		ALTER TABLE `crm_deal` ADD INDEX `last_modifed_on` (`last_modifed_on`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'crm_case' AND `INDEX_NAME` = 'last_modifed_on') THEN
		ALTER TABLE `crm_case` ADD INDEX `last_modifed_on` (`last_modifed_on`);
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'events_feed' AND `INDEX_NAME` = 'Tenant') THEN
		ALTER TABLE `events_feed`
			DROP INDEX `LastModified`,
			ADD INDEX `LastModified` (`LastModified`),
			ADD INDEX `Tenant` (`Tenant`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'bookmarking_userbookmark' AND `INDEX_NAME` = 'Tenant') THEN
		ALTER TABLE `bookmarking_userbookmark`
			DROP INDEX `LastModified`,
			ADD INDEX `LastModified` (`LastModified`),
			ADD INDEX `Tenant` (`Tenant`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'forum_topic' AND `INDEX_NAME` = 'TenantID') THEN
		ALTER TABLE `forum_topic` 
			DROP INDEX `LastModified`,
			ADD INDEX `LastModified` (`LastModified`),
			ADD INDEX `TenantID` (`TenantID`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'forum_post' AND `INDEX_NAME` = 'TenantID') THEN
		ALTER TABLE `forum_post`
			DROP INDEX `LastModified`,
			ADD INDEX `LastModified` (`LastModified`),
			ADD INDEX `TenantID` (`TenantID`);
	END IF;
	IF NOT EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'wiki_pages' AND `INDEX_NAME` = 'tenant') THEN
		ALTER TABLE `wiki_pages`
			DROP INDEX `modified_on`,
			ADD INDEX `modified_on` (`modified_on`),
			ADD INDEX `tenant` (`tenant`);
	END IF;
	
	CREATE TABLE IF NOT EXISTS `mobile_app_install` (
		`user_email` varchar(255) NOT NULL,
		`app_type` int(11) NOT NULL,
		`registered_on` datetime NOT NULL,
		PRIMARY KEY (`user_email`,`app_type`)
	);

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) VALUES ('Currency_MoldovanLeu', 'MDL', 'lei', 'MD', 1, 0);

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_folder') THEN
		TRUNCATE mail_folder;
	END IF;


	ALTER TABLE `res_data`
		DROP PRIMARY KEY,
		ADD PRIMARY KEY (`fileid`, `cultureTitle`, `title`);
END DLM00

CALL upgrade85() DLM00

DELIMITER ;