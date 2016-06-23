-- adds mail_mailbox column
ALTER TABLE `mail_mailbox` ADD COLUMN `imap_folders` TEXT NULL DEFAULT NULL AFTER `refresh_token`;

-- table columns creation
ALTER TABLE `mail_mailbox` ADD COLUMN `incoming_encryption_type` INT(11) NULL DEFAULT NULL, ADD COLUMN `outcoming_encryption_type` INT(11) NULL DEFAULT NULL;
UPDATE `mail_mailbox` SET `incoming_encryption_type` = 1
WHERE `ssl_enabled` = 1;
UPDATE `mail_mailbox` SET `incoming_encryption_type` = 0
WHERE `ssl_enabled` = 0;
UPDATE `mail_mailbox` SET `outcoming_encryption_type` = 1
WHERE `ssl_outgoing` = 1;
UPDATE `mail_mailbox` SET `outcoming_encryption_type` = 0
WHERE `ssl_outgoing` = 0;

-- comments for encryptyon fields
ALTER TABLE `mail_mailbox` CHANGE COLUMN `incoming_encryption_type` `incoming_encryption_type` INT(11) NULL DEFAULT NULL COMMENT '0 - none, 1 - SSL, 2 - STARTTLS' AFTER `imap_folders`, CHANGE COLUMN `outcoming_encryption_type` `outcoming_encryption_type` INT(11) NULL DEFAULT NULL COMMENT '0 - none, 1 - SSL, 2 - STARTTLS' AFTER `incoming_encryption_type`;


-- fields for advanced authentication
ALTER TABLE `mail_mailbox` ADD COLUMN `auth_type_in` INT(11) NULL DEFAULT NULL COMMENT '1 - login, 4 - CremdMD5, 5 - oauh2', ADD COLUMN `auth_type_smtp` INT(11) NULL DEFAULT NULL COMMENT '1 - login, 4 - CremdMD5, 5 - oauh2';



-- set auth type for in mail - imap or pop3
UPDATE `mail_mailbox` SET `auth_type_in` = 1
WHERE `refresh_token` IS NULL;
UPDATE `mail_mailbox` SET `auth_type_in` = 5
WHERE `refresh_token` IS NOT NULL;


-- set auth type for out mail - smtp
UPDATE `mail_mailbox` SET `auth_type_smtp` = 0
WHERE `smtp_auth` = 0 AND `refresh_token` IS NULL;
UPDATE `mail_mailbox` SET `auth_type_smtp` = 1
WHERE `smtp_auth` = 1 AND `refresh_token` IS NULL;
UPDATE `mail_mailbox` SET `auth_type_smtp` = 5
WHERE `smtp_auth` = 1 AND `refresh_token` IS NOT NULL;


-- Dumping structure for table test.mail_garbage
DROP TABLE IF EXISTS `mail_garbage`;
CREATE TABLE IF NOT EXISTS `mail_garbage` (
 `id` INT(11) NOT NULL AUTO_INCREMENT,
 `tenant` INT(11) NOT NULL,
 `path` TEXT NOT NULL,
 `is_processed` VARCHAR(36) NOT NULL DEFAULT '',
 `time_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON
UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`id`)
);


-- Dumping structure for table test.mail_imap_flags
DROP TABLE IF EXISTS `mail_imap_flags`;
CREATE TABLE IF NOT EXISTS `mail_imap_flags` (
 `name` VARCHAR(50) NOT NULL,
 `folder_id` INT(11) NOT NULL,
 `skip` INT(11) NOT NULL DEFAULT '0', PRIMARY KEY (`name`)
);

-- Dumping data for table test.mail_imap_flags: ~12 rows (approximately)
DELETE
FROM `mail_imap_flags`;
INSERT INTO `mail_imap_flags` (`name`, `folder_id`, `skip`) VALUES
	('all', 0, 1),
	('archive', 0, 1),
	('bulk', 5, 0),
	('drafts', 0, 1),
	('flagged', 0, 1),
	('important', 0, 1),
	('inbox', 1, 0),
	('junk', 5, 0),
	('noselect', 0, 1),
	('sent', 2, 0),
	('spam', 5, 0),
	('trash', 0, 1);


-- Dumping structure for table test.mail_imap_skip_mailbox
DROP TABLE IF EXISTS `mail_imap_skip_mailbox`;
CREATE TABLE IF NOT EXISTS `mail_imap_skip_mailbox` (
 `server` VARCHAR(255) NOT NULL,
 `name` VARCHAR(255) NOT NULL, PRIMARY KEY (`server`,`name`)
);

-- Dumping data for table test.mail_imap_skip_mailbox: ~1 rows (approximately)
DELETE
FROM `mail_imap_skip_mailbox`;
INSERT INTO `mail_imap_skip_mailbox` (`server`, `name`) VALUES
	('imap.yandex.ru', 'исходящие');


-- Dumping structure for table test.mail_imap_special_mailbox
DROP TABLE IF EXISTS `mail_imap_special_mailbox`;
CREATE TABLE IF NOT EXISTS `mail_imap_special_mailbox` (
 `server` VARCHAR(255) NOT NULL,
 `name` VARCHAR(255) NOT NULL,
 `folder_id` TINYINT(3) NOT NULL,
 `skip` TINYINT(1) NOT NULL, PRIMARY KEY (`server`,`name`)
) COMMENT='Special mailboxes cases for some mail domains';

-- Dumping data for table test.mail_imap_special_mailbox: ~5 rows (approximately)
DELETE
FROM `mail_imap_special_mailbox`;
INSERT INTO `mail_imap_special_mailbox` (`server`, `name`, `folder_id`, `skip`) VALUES
	('imap.mail.yahoo.com', 'Bulk Mail', 5, 0),
	('imap.mail.yahoo.com', 'Draft', 3, 1),
	('imap.mail.yahoo.com', 'Sent', 2, 0),
	('imap.mail.yahoo.com', 'Trash', 4, 1),
	('imap.yandex.ru', 'исходящие', 0, 1);


-- Dumping structure for table test.mail_pop_unordered_domain
DROP TABLE IF EXISTS `mail_pop_unordered_domain`;
CREATE TABLE IF NOT EXISTS `mail_pop_unordered_domain` (
 `server` VARCHAR(255) NOT NULL, PRIMARY KEY (`server`)
);


-- Dumping data for table test.mail_pop_unordered_domain: ~1 rows (approximately)
DELETE
FROM `mail_pop_unordered_domain`;
INSERT INTO `mail_pop_unordered_domain` (`server`) VALUES
	('pop.yandex.ru');