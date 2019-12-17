DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade10 DLM00

CREATE PROCEDURE upgrade10()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tenants_partners' AND COLUMN_NAME = 'campaign') THEN
        ALTER TABLE `tenants_partners`
            ADD COLUMN `campaign` VARCHAR(50) NULL DEFAULT NULL AFTER `affiliate_id`;
    END IF;

	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_events' AND COLUMN_NAME = 'update_date') THEN
		ALTER TABLE `calendar_events`
			ADD COLUMN `update_date` DATETIME NULL DEFAULT NULL AFTER `end_date`;
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_calendars' AND COLUMN_NAME = 'caldav_guid') THEN
		ALTER TABLE `calendar_calendars` ADD COLUMN `caldav_guid` CHAR(38) NULL DEFAULT NULL AFTER `ical_url`;
		ALTER TABLE `calendar_calendars` ADD COLUMN `is_todo` INT(11) NULL AFTER `caldav_guid`;
	END IF;

	UPDATE `crm_currency_info` SET `symbol`='₽' WHERE `abbreviation`='RUB'; 

	INSERT IGNORE INTO `crm_currency_info` (`resource_key`, `abbreviation`, `symbol`, `culture_name`, `is_convertable`, `is_basic`) values ('Currency_SerbianDinar', 'RSD', 'din.', 'SR', 1, 0);

	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.odp');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.pdf');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.otp', '.pptx');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.csv');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.ods');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.pdf');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ots', '.xlsx');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.docx');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.odt');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.pdf');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.rtf');
	INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ott', '.txt');

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_server_dns' AND COLUMN_NAME = 'mx') THEN
        ALTER TABLE `mail_server_dns`
            ADD COLUMN `dkim_ttl` INT NOT NULL DEFAULT '300' AFTER `dkim_public_key`,
            ADD COLUMN `dkim_verified` TINYINT(1) NOT NULL DEFAULT '0' AFTER `dkim_ttl`,
            ADD COLUMN `dkim_date_checked` DATETIME NULL AFTER `dkim_verified`,
            ADD COLUMN `spf_ttl` INT NOT NULL DEFAULT '300' AFTER `spf`,
            ADD COLUMN `spf_verified` TINYINT(1) NOT NULL DEFAULT '0' AFTER `spf_ttl`,
            ADD COLUMN `spf_date_checked` DATETIME NULL AFTER `spf_verified`,
            ADD COLUMN `mx` VARCHAR(255) NULL AFTER `spf_date_checked`,
            ADD COLUMN `mx_ttl` INT NOT NULL DEFAULT '300' AFTER `mx`,
            ADD COLUMN `mx_verified` TINYINT(1) NOT NULL DEFAULT '0' AFTER `mx_ttl`,
            ADD COLUMN `mx_date_checked` DATETIME NULL AFTER `mx_verified`;

        UPDATE mail_server_dns t
            INNER JOIN mail_server_domain d ON t.id_domain = d.id
            INNER JOIN mail_server_server_x_tenant st ON d.tenant = st.id_tenant
            INNER JOIN mail_server_server s ON st.id_server = s.id 
            SET t.mx = s.mx_record, t.dkim_verified = d.is_verified, t.spf_verified = d.is_verified, t.mx_verified = d.is_verified;

        UPDATE mail_server_dns t
            INNER JOIN mail_server_server_x_tenant st ON t.tenant = st.id_tenant
            INNER JOIN mail_server_server s ON st.id_server = s.id 
            SET t.mx = s.mx_record, t.dkim_verified = 1, t.spf_verified = 1, t.mx_verified = 1
            WHERE t.id_domain = -1;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mail_mailbox' AND COLUMN_NAME = 'login_delay_expires') THEN
        ALTER TABLE `mail_mailbox`
            CHANGE COLUMN `tenant` `tenant` INT(11) NOT NULL AFTER `id`,
            CHANGE COLUMN `id_user` `id_user` VARCHAR(38) NOT NULL AFTER `tenant`,
            CHANGE COLUMN `name` `name` VARCHAR(255) NULL DEFAULT NULL AFTER `address`,
            CHANGE COLUMN `enabled` `enabled` TINYINT(1) UNSIGNED NOT NULL DEFAULT '1' AFTER `name`,
            CHANGE COLUMN `is_removed` `is_removed` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `enabled`,
            CHANGE COLUMN `is_processed` `is_processed` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `is_removed`,
            CHANGE COLUMN `is_teamlab_mailbox` `is_server_mailbox` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `is_processed`,
            CHANGE COLUMN `imap` `imap` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `is_server_mailbox`,
            CHANGE COLUMN `user_online` `user_online` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `imap`,
            CHANGE COLUMN `is_default` `is_default` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `user_online`,
            CHANGE COLUMN `msg_count_last` `msg_count_last` INT(11) NOT NULL DEFAULT '0' AFTER `is_default`,
            CHANGE COLUMN `size_last` `size_last` INT(11) NOT NULL DEFAULT '0' AFTER `msg_count_last`,
            CHANGE COLUMN `login_delay` `login_delay` INT(11) UNSIGNED NOT NULL DEFAULT '30' AFTER `size_last`,
            CHANGE COLUMN `quota_error` `quota_error` TINYINT(1) NOT NULL DEFAULT '0' AFTER `login_delay`,
            CHANGE COLUMN `imap_intervals` `imap_intervals` MEDIUMTEXT NULL AFTER `quota_error`,
            CHANGE COLUMN `begin_date` `begin_date` TIMESTAMP NOT NULL DEFAULT '1975-01-01 00:00:00' AFTER `imap_intervals`,
            CHANGE COLUMN `email_in_folder` `email_in_folder` TEXT NULL AFTER `begin_date`,
            CHANGE COLUMN `date_checked` `date_checked` DATETIME NULL DEFAULT NULL AFTER `id_in_server`,
            CHANGE COLUMN `date_user_checked` `date_user_checked` DATETIME NULL DEFAULT NULL AFTER `date_checked`,
            CHANGE COLUMN `date_login_delay_expires` `date_login_delay_expires` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00' AFTER `date_user_checked`,
            CHANGE COLUMN `date_auth_error` `date_auth_error` DATETIME NULL DEFAULT NULL AFTER `date_login_delay_expires`,
            DROP COLUMN `time_checked`,
            DROP COLUMN `login_delay_expires`,
            DROP COLUMN `auth_error`,
            DROP COLUMN `service_type`,
            DROP COLUMN `refresh_token`,
            DROP COLUMN `imap_folders`,
            DROP COLUMN `user_time_checked`,
            DROP INDEX `login_delay_expires`;

        ALTER TABLE `mail_mailbox`
            ADD COLUMN `is_teamlab_mailbox` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `is_server_mailbox`;

        UPDATE `mail_mailbox` SET is_teamlab_mailbox = is_server_mailbox;

    END IF;
    
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'backup_backup' AND COLUMN_NAME = 'storage_params') THEN
		ALTER TABLE `backup_backup`
			ADD COLUMN `storage_params` TEXT NULL AFTER `expires_on`;
	END IF;
	
	IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'backup_schedule' AND COLUMN_NAME = 'storage_params') THEN
		ALTER TABLE `backup_schedule`
			ADD COLUMN `storage_params` TEXT NULL AFTER `last_backup_time`;
	END IF;
	
    CREATE TABLE IF NOT EXISTS `mail_user_folder` (
      `id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
      `parent_id` INT(11) NOT NULL DEFAULT '0',
      `tenant` INT(11) NOT NULL,
      `id_user` VARCHAR(38) NOT NULL,
      `name` VARCHAR(400) NOT NULL,
      `folders_count` INT(11) UNSIGNED NOT NULL,
      `unread_messages_count` INT(11) UNSIGNED NOT NULL DEFAULT '0',
      `total_messages_count` INT(11) UNSIGNED NOT NULL DEFAULT '0',
      `unread_conversations_count` INT(11) UNSIGNED NOT NULL DEFAULT '0',
      `total_conversations_count` INT(11) UNSIGNED NOT NULL DEFAULT '0',
      `modified_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
      PRIMARY KEY (`id`),
      INDEX `tenant_user_parent` (`tenant`, `id_user`, `parent_id`)
    )ENGINE=InnoDB DEFAULT CHARSET=utf8;

    CREATE TABLE IF NOT EXISTS `mail_folder_counters` (
      `tenant` int(11) NOT NULL,
      `id_user` varchar(255) NOT NULL,
      `folder` smallint(5) unsigned NOT NULL,
      `unread_messages_count` int(10) unsigned NOT NULL DEFAULT '0',
      `total_messages_count` int(10) unsigned NOT NULL DEFAULT '0',
      `unread_conversations_count` int(10) unsigned NOT NULL DEFAULT '0',
      `total_conversations_count` int(10) unsigned NOT NULL DEFAULT '0',
      `time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
      PRIMARY KEY (`tenant`,`id_user`,`folder`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    CREATE TABLE IF NOT EXISTS `mail_user_folder_tree` (
      `folder_id` INT(11) UNSIGNED  NOT NULL,
      `parent_id` INT(11) UNSIGNED  NOT NULL,
      `level` INT(11) UNSIGNED NOT NULL,
      PRIMARY KEY (`parent_id`, `folder_id`),
      INDEX `folder_id` (`folder_id`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    CREATE TABLE IF NOT EXISTS `mail_user_folder_x_mail` (
        `tenant` INT(11) NOT NULL,
        `id_user` VARCHAR(38) NOT NULL,
        `id_mail` INT(11) UNSIGNED NOT NULL,
        `id_folder` INT(11) UNSIGNED NOT NULL,
        `time_created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (`tenant`, `id_user`, `id_mail`, `id_folder`),
        INDEX `id_mail` (`id_mail`),
        INDEX `id_tag` (`id_folder`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    CREATE TABLE IF NOT EXISTS `mail_filter` (
        `id` INT(11) NOT NULL AUTO_INCREMENT,
        `tenant` INT(11) NOT NULL,
        `id_user` VARCHAR(38) NOT NULL,
        `enabled` TINYINT(1) NOT NULL DEFAULT '1',
        `filter` TEXT NOT NULL,
        `position` INT(11) NOT NULL DEFAULT '0',
        `date_created` TIMESTAMP NULL DEFAULT NULL,
        `date_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        PRIMARY KEY (`id`),
        INDEX `tenant_id_user` (`tenant`, `id_user`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `projects_reports` (
		`id` INT(11) NOT NULL AUTO_INCREMENT,
		`type` INT(11) NOT NULL,
		`name` VARCHAR(1024) NOT NULL,
		`fileId` INT(11) NOT NULL DEFAULT '0',
		`create_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
		`create_by` VARCHAR(38) NULL DEFAULT NULL,
		`tenant_id` INT(10) NOT NULL,
		PRIMARY KEY (`id`),
		INDEX `tenant_id` (`tenant_id`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `calendar_todos` (
		`id` INT(10) NOT NULL AUTO_INCREMENT,
		`tenant` INT(11) NOT NULL,
		`name` VARCHAR(255) NOT NULL,
		`description` TEXT NOT NULL,
		`calendar_id` INT(11) NOT NULL,
		`start_date` DATETIME NULL DEFAULT NULL,
		`completed` DATETIME NULL DEFAULT NULL,
		`owner_id` CHAR(38) NOT NULL,
		`uid` VARCHAR(255) NULL DEFAULT NULL,
		PRIMARY KEY (`id`),
		INDEX `calendar_id` (`tenant`, `calendar_id`)
	)ENGINE=InnoDB DEFAULT CHARSET=utf8;

	CREATE TABLE IF NOT EXISTS `short_links` (
		`id` INT(21) NOT NULL AUTO_INCREMENT,
		`short` VARCHAR(12) COLLATE utf8_bin NULL DEFAULT NULL,
		`link` TEXT NULL,
		PRIMARY KEY (`id`),
		UNIQUE INDEX `UNIQUE` (`short`)
	) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    UPDATE mail_mailbox_server s
    INNER JOIN mail_mailbox_domain d ON s.id_provider = d.id_provider 
    SET s.hostname = 'imap.rambler.ru'
    WHERE d.name = 'rambler.ru' AND s.hostname = 'mail.rambler.ru' AND s.`type` = 'imap';

    UPDATE mail_mailbox_server s
    INNER JOIN mail_mailbox_domain d ON s.id_provider = d.id_provider
    SET s.hostname = 'smtp.rambler.ru'
    WHERE d.name = 'rambler.ru' AND s.hostname = 'mail.rambler.ru' AND s.`type` = 'smtp';

    UPDATE mail_mailbox_server s
    INNER JOIN mail_mailbox_domain d ON s.id_provider = d.id_provider
    SET s.hostname = 'pop.rambler.ru'
    WHERE d.name = 'rambler.ru' AND s.hostname = 'mail.rambler.ru' AND s.`type` = 'pop3';

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calendar_event_history' AND COLUMN_NAME = 'ics') <> 'mediumtext' THEN
        ALTER TABLE `calendar_event_history` CHANGE COLUMN `ics` `ics` mediumtext NULL AFTER `event_id`;
    END IF;

    DELETE c
    FROM mail_chain c
    LEFT JOIN mail_mail m ON c.tenant = m.tenant AND c.id_user = m.id_user AND c.folder = m.folder AND c.id_mailbox = m.id_mailbox AND c.id = m.chain_id
    WHERE m.chain_id IS NULL;

    INSERT INTO `mail_folder_counters` (`tenant`, `id_user`, `folder`, `unread_messages_count`, `total_messages_count`, `unread_conversations_count`, `total_conversations_count`, `time_modified`)
    SELECT t1.tenant, t1.id_user, t1.folder, ifnull(t2.real_unread_mess_cnt, 0), ifnull(t1.real_total_mess_cnt, 0), ifnull(t4.real_unread_cnt, 0), ifnull(t3.real_total_cnt, 0), UTC_TIMESTAMP() AS UTC_NOW
    FROM 
    (
    SELECT ifnull(COUNT(*), 0) AS real_total_mess_cnt, m.tenant, m.id_user, m.folder
    FROM mail_mail m
    WHERE m.is_removed = 0
    GROUP BY m.tenant, m.id_user, m.folder) t1
    LEFT JOIN
    (
    SELECT ifnull(COUNT(*), 0) AS real_unread_mess_cnt, m.tenant, m.id_user, m.folder
    FROM mail_mail m
    WHERE m.is_removed = 0 AND m.unread = 1
    GROUP BY m.tenant, m.id_user, m.folder) t2 ON t1.tenant = t2.tenant AND t1.id_user = t2.id_user AND t1.folder = t2.folder
    LEFT JOIN
    (
    SELECT ifnull(COUNT(*), 0) AS real_total_cnt, c.tenant, c.id_user, c.folder
    FROM mail_chain c
    GROUP BY c.tenant, c.id_user, c.folder) t3 ON t1.tenant = t3.tenant AND t1.id_user = t3.id_user AND t1.folder = t3.folder
    LEFT JOIN
    (
    SELECT ifnull(COUNT(*), 0) AS real_unread_cnt, c.tenant, c.id_user, c.folder
    FROM mail_chain c
    WHERE c.unread = 1
    GROUP BY c.tenant, c.id_user, c.folder) t4 ON t1.tenant = t4.tenant AND t1.id_user = t4.id_user AND t1.folder = t4.folder
    ON DUPLICATE KEY
    UPDATE `unread_messages_count`=ifnull(t2.real_unread_mess_cnt, 0), 
    `total_messages_count`=ifnull(t1.real_total_mess_cnt, 0), 
    `unread_conversations_count`=ifnull(t4.real_unread_cnt, 0),
    `total_conversations_count`=ifnull(t3.real_total_cnt, 0), 
    `time_modified`= UTC_TIMESTAMP();

END DLM00

CALL upgrade10() DLM00

DELIMITER ;