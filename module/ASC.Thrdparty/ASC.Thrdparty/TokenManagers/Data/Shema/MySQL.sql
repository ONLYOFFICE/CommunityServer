DROP TABLE IF EXISTS `auth_tokens`;
CREATE TABLE `auth_tokens` (
						`token` VARCHAR(255) NOT NULL,
						`token_secret` VARCHAR(255),
						`associate_id` VARCHAR(255),
						`request_token` BIT,
						PRIMARY KEY (`token`)
					);

