Update mail_tag t set t.addresses = '' WHERE t.addresses is null;

ALTER TABLE `mail_tag`
	CHANGE COLUMN `addresses` `addresses` TEXT NOT NULL AFTER `style`;