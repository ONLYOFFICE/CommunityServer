DROP TABLE `mail_alerts`;
CREATE TABLE `mail_alerts` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`tenant` INT(11) NOT NULL,
	`id_user` VARCHAR(255) NOT NULL,
	`id_mailbox` INT(11) NOT NULL DEFAULT '-1',
	`type` INT(11) UNSIGNED NOT NULL,
	`data` MEDIUMTEXT NULL,
   PRIMARY KEY (`id`),
	INDEX `tenant_id_user` (`tenant`, `id_user`),
	INDEX `tenant_id_user_id_mailbox_type` (`tenant`, `id_user`, `id_mailbox`, `type`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB
ROW_FORMAT=COMPACT;