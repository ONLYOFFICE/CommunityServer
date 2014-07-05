CREATE TABLE `mail_display_images` (
	`tenant` INT(10) NOT NULL,
	`id_user` VARCHAR(255) NOT NULL,
	`address` VARCHAR(255) NOT NULL COMMENT 'Always display images from this address',
	PRIMARY KEY (`tenant`, `id_user`, `address`),
	INDEX `user_index` (`tenant`, `id_user`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB;
