ALTER TABLE `mail_mailbox` ADD COLUMN `ssl_outgoing` TINYINT(1) NOT NULL DEFAULT '0' AFTER `ssl_enabled`;
UPDATE mail_mailbox mb SET mb.ssl_outgoing = TRUE
WHERE mb.ssl_enabled = TRUE;