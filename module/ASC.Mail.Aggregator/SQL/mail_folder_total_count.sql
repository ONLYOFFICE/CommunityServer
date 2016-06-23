ALTER TABLE `mail_folder`
	ADD COLUMN `total_count` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `unread`;
	
ALTER TABLE `mail_folder`
	ADD COLUMN `unread_messages_count` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `total_count`,
	ADD COLUMN `total_messages_count` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `unread_messages_count`,
	ADD COLUMN `unread_conversations_count` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `total_messages_count`,
	ADD COLUMN `total_conversations_count` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `unread_conversations_count`;