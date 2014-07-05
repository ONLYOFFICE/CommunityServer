ALTER TABLE `mail_mail`
	ADD COLUMN `has_parse_error` TINYINT(1) NOT NULL DEFAULT '0' AFTER `is_text_body_only`;