ALTER TABLE `audit_events`
	DROP INDEX `tenant_id`,
	DROP INDEX `date`,
	ADD INDEX `date` (`tenant_id`, `date`);

ALTER TABLE `backup_backup`
	ADD INDEX `expires_on` (`expires_on`),
	ADD INDEX `is_scheduled` (`is_scheduled`);

ALTER TABLE `backup_schedule`
	ADD `backup_mail` INT(11) NOT NULL DEFAULT '0';

ALTER TABLE `crm_organisation_logo`
	CHANGE `content` `content` MEDIUMTEXT NOT NULL;

ALTER TABLE `crm_tag`
	CHANGE `title` `title` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL;

ALTER TABLE `crm_task`
	CHANGE `exec_alert` `exec_alert` int(10) NOT NULL DEFAULT '0';

ALTER TABLE `crm_voip_calls_history`
	CHANGE `dial_date` `end_dial_date` DATETIME DEFAULT NULL,
	ADD `record_url` TEXT AFTER `end_dial_date`,
	ADD `record_duration` int(11) DEFAULT NULL AFTER `record_url`,
	ADD `price` decimal(10,4) DEFAULT NULL AFTER `record_duration`;

ALTER TABLE `files_security`
	CHANGE `entry_id` `entry_id` VARCHAR(50) NOT NULL,
	ADD INDEX `tenant_id` (`tenant_id`, `entry_type`, `entry_id`, `owner`);

ALTER TABLE `mail_alerts`
	CHANGE `type` `type` INT NOT NULL DEFAULT '0',
	DROP INDEX `tenant_id_user`;

CREATE TABLE IF NOT EXISTS `mail_server_dns` (
	`id` int(11) unsigned NOT NULL AUTO_INCREMENT,
	`tenant` int(11) NOT NULL,
	`id_user` varchar(255) NOT NULL,
	`id_domain` int(11) NOT NULL DEFAULT '-1',
	`dkim_selector` varchar(63) NOT NULL DEFAULT 'dkim',
	`dkim_private_key` text,
	`dkim_public_key` text,
	`domain_check` text,
	`spf` text,
	`time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`),
	KEY `id_domain_tenant_id_user` (`id_domain`,`tenant`,`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `mail_server_mail_group`
	ADD `address` varchar(320) NOT NULL;

ALTER TABLE `projects_messages`
	CHANGE `status` `status` int(11) NOT NULL DEFAULT '0';

ALTER TABLE `projects_tasks`
	DROP INDEX `milestone_id`,
	DROP INDEX `tenant_id`,
	ADD INDEX `milestone_id` (`tenant_id`,`milestone_id`),
	ADD INDEX `tenant_id` (`tenant_id`,`project_id`);

ALTER TABLE `sso_tokens`
	DROP INDEX `tokenId`,
	ADD PRIMARY KEY (`tokenId`);

ALTER TABLE `tenants_quota`
	DROP COLUMN `https_enable`,
	DROP COLUMN `security_enable`;

ALTER TABLE `tenants_tenants`
	CHANGE `industry` `industry` int(11) NOT NULL DEFAULT '0';

CREATE TABLE IF NOT EXISTS `webstudio_index` (
	`index_name` varchar(50) NOT NULL,
	`last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
	PRIMARY KEY (`index_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
