-- ---------------------------------------------------------------------------------------
-- Создание таблицы для подписей 
-- --------------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS `mail_mailbox_signature` (
	`tenant` int(11) NOT NULL,
	`id_mailbox` int(11) NOT NULL,
	`html` text,
	`is_active` tinyint(4) NOT NULL DEFAULT '0',
	PRIMARY KEY (`id_mailbox`,`tenant`)
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ---------------------------------------------------------------------------------------
-- Убираем устаревшие поля
-- ---------------------------------------------------------------------------------------

CREATE TABLE `mail_folder_copy` (
	`tenant` INT(11) NOT NULL,
	`id_user` VARCHAR(255) NOT NULL,
	`folder` SMALLINT(5) UNSIGNED NOT NULL,
	`time_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	`unread_messages_count` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`total_messages_count` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`unread_conversations_count` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	`total_conversations_count` INT(10) UNSIGNED NOT NULL DEFAULT '0',
	PRIMARY KEY (`tenant`, `id_user`, `folder`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB;

-- Переименование mail_folder в mail_folder_old
RENAME TABLE `mail_folder` TO `mail_folder_old`;

-- Переименование mail_folder_copy в mail_folder
RENAME TABLE `mail_folder_copy` TO `mail_folder`;

-- ---------------------------------------------------------------------------------------
-- Добавляем колону stored_name в mail_attachment
-- ---------------------------------------------------------------------------------------
-- Создание таблицы mail_attachment_copy (на основе mail_attachment) без данных, где добалена колонка stored_name

CREATE TABLE `mail_attachment_copy` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`id_mail` INT(11) NOT NULL,
	`name` VARCHAR(255) NOT NULL,
	`stored_name` VARCHAR(255) NULL DEFAULT NULL,
	`type` VARCHAR(255) NULL DEFAULT NULL,
	`size` BIGINT(20) NOT NULL DEFAULT '0',
	`need_remove` INT(11) NOT NULL DEFAULT '0',
	`file_number` INT(11) NOT NULL DEFAULT '0',
	`content_id` VARCHAR(255) NULL DEFAULT NULL,
	`tenant` INT(11) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `quota_index` (`id_mail`, `need_remove`, `size`),
	INDEX `main` (`id_mail`, `content_id`, `need_remove`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPACT;
 
-- Перенос данных из mail_attachment в mail_attachment_copy 
INSERT INTO `mail_attachment_copy` (`id_mail`, `name`, `type`, `size`, `need_remove`, `file_number`, `tenant`)
SELECT `id_mail`, `name`, `type`, `size`, `need_remove`, `file_number`, `tenant` FROM `mail_attachment`;
 
-- Переименование mail_attachment в mail_attachment_old
RENAME TABLE `mail_attachment` TO `mail_attachment_old`;

-- Переименование mail_attachment_copy в mail_attachment
RENAME TABLE `mail_attachment_copy` TO `mail_attachment`;

-- ---------------------------------------------------------------------------------------
-- Зачищаем устаревшие данные mail_log
-- ---------------------------------------------------------------------------------------

TRUNCATE `mail_log`;

-- Добавляем поле в index `id_aggregator`
ALTER TABLE `mail_log`
	DROP INDEX `id_aggregator`,
	ADD INDEX `id_aggregator` (`id_aggregator`, `processing_start_time`);

-- ---------------------------------------------------------------------------------------
-- Меняем типы данных c TIMESTAMP на DATETIME
-- ---------------------------------------------------------------------------------------
-- Создание таблицы mail_mail_copy (на основе mail_mail) без данных, где тип колонок date_sent, date_received, chain_date поменян на DATETIME

CREATE TABLE `mail_mail_copy` (
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
	INDEX `id_mailbox` (`id_mailbox`),
	INDEX `time_modified` (`tenant`, `time_modified`),
	INDEX `main` (`tenant`, `id_user`, `is_removed`, `folder`, `chain_date`)
)
 COLLATE 'utf8_general_ci' ENGINE=InnoDB ROW_FORMAT=Compact;
 
-- Перенос данных из mail_mail в mail_mail_copy 
INSERT INTO `mail_mail_copy` (`id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date`) 
SELECT `id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date` FROM `mail_mail`;
 
-- Переименование mail_mail в mail_mail_old
RENAME TABLE `mail_mail` TO `mail_mail_old`;

-- Переименование mail_mail_copy в mail_mail
RENAME TABLE `mail_mail_copy` TO `mail_mail`;

-- ---------------------------------------------------------------------------------------
-- Добавляем колону email_in_folder в mail_mailbox
-- ---------------------------------------------------------------------------------------
-- Создание таблицы mail_mailbox_copy (на основе mail_mailbox) без данных, где добалена колонка email_in_folder

CREATE TABLE `mail_mailbox_copy` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `id_user` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  `pop3_password` varchar(255) DEFAULT NULL,
  `enabled` int(11) NOT NULL DEFAULT '1',
  `msg_count_last` int(11) NOT NULL DEFAULT '0',
  `size_last` int(11) NOT NULL DEFAULT '0',
  `smtp_password` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `login_delay` int(11) unsigned NOT NULL DEFAULT '30',
  `time_checked` bigint(20) unsigned NOT NULL DEFAULT '0',
  `is_processed` int(11) unsigned NOT NULL DEFAULT '0',
  `user_time_checked` bigint(20) unsigned NOT NULL DEFAULT '0',
  `login_delay_expires` bigint(20) unsigned NOT NULL DEFAULT '0',
  `is_removed` tinyint(1) NOT NULL DEFAULT '0',
  `quota_error` tinyint(1) NOT NULL DEFAULT '0',
  `auth_error` bigint(20) unsigned DEFAULT NULL,
  `imap` tinyint(1) NOT NULL DEFAULT '0',
  `begin_date` timestamp NOT NULL DEFAULT '1975-01-01 00:00:00',
  `service_type` tinyint(4) NOT NULL DEFAULT '0',
  `refresh_token` varchar(255) DEFAULT NULL,
  `imap_folders` text,
  `id_smtp_server` int(11) NOT NULL,
  `id_in_server` int(11) NOT NULL,
  `email_in_folder` text,
  PRIMARY KEY (`id`),
  KEY `address_index` (`address`),
  KEY `user_id_index` (`id_user`,`tenant`),
  KEY `login_delay_expires` (`time_checked`,`login_delay_expires`),
  KEY `main_mailbox_id_smtp_server_mail_mailbox_server_id` (`id_smtp_server`),
  KEY `main_mailbox_id_in_server_mail_mailbox_server_id` (`id_in_server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Перенос данных из mail_mailbox в mail_mailbox_copy 
INSERT INTO `mail_mailbox_copy` (`id`,`id_user`,`address`,`tenant`,`pop3_password`,`enabled`,`msg_count_last`,`size_last`,`smtp_password`,`name`,`login_delay`,`time_checked`,`is_processed`,`user_time_checked`,`login_delay_expires`,`is_removed`,`quota_error`,`auth_error`,`imap`,`begin_date`,`service_type`,`refresh_token`,`imap_folders`,`id_smtp_server`,`id_in_server`)
SELECT `id`,`id_user`,`address`,`tenant`,`pop3_password`,`enabled`,`msg_count_last`,`size_last`,`smtp_password`,`name`,`login_delay`,`time_checked`,`is_processed`,`user_time_checked`,`login_delay_expires`,`is_removed`,`quota_error`,`auth_error`,`imap`,`begin_date`,`service_type`,`refresh_token`,`imap_folders`,`id_smtp_server`,`id_in_server` FROM `mail_mailbox`;
 
-- Переименование mail_mailbox в mail_mailbox_old
RENAME TABLE `mail_mailbox` TO `mail_mailbox_old`;

-- Переименование mail_mailbox_copy в mail_mailbox
RENAME TABLE `mail_mailbox_copy` TO `mail_mailbox`;

-- ---------------------------------------------------------------------------------------
-- Удаляем таблицы для сервера -> созданы по ошибке
-- ---------------------------------------------------------------------------------------

DROP TABLE IF EXISTS mail_server_domain;
DROP TABLE IF EXISTS mail_server_address;
DROP TABLE IF EXISTS mail_server_server;
DROP TABLE IF EXISTS mail_server_server_x_tenant;
DROP TABLE IF EXISTS mail_server_server_type;
DROP TABLE IF EXISTS mail_server_mail_group;
DROP TABLE IF EXISTS mail_server_mail_group_x_mail_server_address;

-- ---------------------------------------------------------------------------------------
-- Удаляем устаревшие таблицы
-- ---------------------------------------------------------------------------------------

DROP TABLE IF EXISTS mail_imap_skip_mailbox;

-- ---------------------------------------------------------------------------------------
-- Удаляем временные таблицы
-- ---------------------------------------------------------------------------------------

DROP TABLE IF EXISTS `mail_folder_old`;
DROP TABLE IF EXISTS `mail_attachment_old`;
DROP TABLE IF EXISTS `mail_mail_old`;
DROP TABLE IF EXISTS `mail_mailbox_old`;
