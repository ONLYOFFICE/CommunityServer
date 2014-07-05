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
ROW_FORMAT=COMPACT
AUTO_INCREMENT=9533;
 
 -- Перенос данных из mail_attachment в mail_attachment_copy 
INSERT INTO `mail_attachment_copy` (`id_mail`, `name`, `type`, `size`, `need_remove`, `file_number`, `tenant`)
SELECT `id_mail`, `name`, `type`, `size`, `need_remove`, `file_number`, `tenant` FROM `mail_attachment`;
 
-- Переименование mail_mail в mail_mail_old
RENAME TABLE `mail_attachment` TO `mail_attachment_old`;

-- Переименование mail_mail_copy в mail_mail
RENAME TABLE `mail_attachment_copy` TO `mail_attachment`;

-- Удаление mail_attachment_old
-- DROP TABLE `mail_attachment_old`
