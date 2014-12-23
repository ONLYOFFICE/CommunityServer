-- ---------------------------------------------------------------------------------------
-- Добавляем новое поле 'has_parse_error' в mail_mail
-- ---------------------------------------------------------------------------------------
-- Создание таблицы mail_mail_copy (на основе mail_mail) без данных с новым полем 'has_parse_error'

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
	`has_parse_error` TINYINT(1) NOT NULL DEFAULT '0',
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
COLLATE='utf8_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPACT;
 
 -- Перенос данных из mail_mail в mail_mail_copy 
 
INSERT INTO `mail_mail_copy` (`id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date`)
 SELECT `id`, `id_mailbox`, `id_user`, `tenant`, `uidl`, `md5`, `address`, `from_text`, `to_text`, `reply_to`, `cc`, `bcc`, `subject`, `introduction`, `importance`, `date_received`, `date_sent`, `size`, `attachments_count`, `unread`, `is_answered`, `is_forwarded`, `is_from_crm`, `is_from_tl`, `is_text_body_only`, `stream`, `folder`, `folder_restore`, `spam`, `time_modified`, `is_removed`, `mime_message_id`, `mime_in_reply_to`, `chain_id`, `chain_date` FROM `mail_mail`;
 
-- Переименование mail_mail в mail_mail_old

RENAME TABLE `mail_mail` TO `mail_mail_old`;

-- Переименование mail_mail_copy в mail_mail

RENAME TABLE `mail_mail_copy` TO `mail_mail`;

-- Удаление mail_mail_old
-- DROP TABLE `mail_mail_old`