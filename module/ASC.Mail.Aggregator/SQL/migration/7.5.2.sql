-----------------------------------------------------------------------------------------
-- Удаляем таблицы для сервера -> созданы по ошибке
-----------------------------------------------------------------------------------------
drop table if exists mail_server_domain;
drop table if exists mail_server_address;
drop table if exists mail_server_server;
drop table if exists mail_server_server_x_tenant;
drop table if exists mail_server_server_type;
drop table if exists mail_server_mail_group;
drop table if exists mail_server_mail_group_x_mail_server_address;

-----------------------------------------------------------------------------------------
-- Создание таблицы для подписей 
-----------------------------------------------------------------------------------------
create table if not exists mail_mailbox_signature(
tenant int not null,
id_mailbox int not null,
html text,
is_active tinyint not null default 0,
primary key(id_mailbox, tenant)
);

-----------------------------------------------------------------------------------------
-- Убираем устаревшие поля
-----------------------------------------------------------------------------------------
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

-- Удаление mail_folder_old
-- DROP TABLE `mail_folder_old`

-----------------------------------------------------------------------------------------
-- Добавляем колону stored_name в mail_attachment
-----------------------------------------------------------------------------------------
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
 
-- Переименование mail_mail в mail_mail_old
RENAME TABLE `mail_attachment` TO `mail_attachment_old`;

-- Переименование mail_mail_copy в mail_mail
RENAME TABLE `mail_attachment_copy` TO `mail_attachment`;

-- Удаление mail_attachment_old
-- DROP TABLE `mail_attachment_old`

-----------------------------------------------------------------------------------------
-- Меняем типы данных c TIMESTAMP на DATETIME
-----------------------------------------------------------------------------------------
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

-- Удаление mail_mail_old
-- DROP TABLE `mail_mail_old`
