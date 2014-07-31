-- mail_aggregators
CREATE TABLE IF NOT EXISTS `mail_aggregators` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ip` varchar(50) NOT NULL,
  `start_work` datetime NOT NULL,
  `end_work` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ip_index` (`ip`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_alerts
CREATE TABLE IF NOT EXISTS `mail_alerts` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `data` mediumtext NOT NULL,
  PRIMARY KEY (`id`),
  KEY `USER` (`tenant`,`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_attachment
CREATE TABLE IF NOT EXISTS `mail_attachment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_mail` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `stored_name` varchar(255) DEFAULT NULL,
  `type` varchar(255) DEFAULT NULL,
  `size` bigint(20) NOT NULL DEFAULT '0',
  `need_remove` int(11) NOT NULL DEFAULT '0',
  `file_number` int(11) NOT NULL DEFAULT '0',
  `content_id` varchar(255) DEFAULT NULL,
  `tenant` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `quota_index` (`id_mail`,`need_remove`,`size`),
  KEY `main` (`id_mail`,`content_id`,`need_remove`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_chain
CREATE TABLE IF NOT EXISTS `mail_chain` (
  `id` varchar(255) NOT NULL,
  `id_mailbox` int(10) unsigned NOT NULL,
  `tenant` int(10) unsigned NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `folder` int(10) unsigned NOT NULL,
  `length` int(10) unsigned NOT NULL,
  `unread` tinyint(1) unsigned NOT NULL,
  `has_attachments` tinyint(1) unsigned NOT NULL,
  `importance` tinyint(1) unsigned NOT NULL,
  `tags` text NOT NULL,
  `is_crm_chain` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`id_user`,`id`,`id_mailbox`,`folder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_chain_x_crm_entity
CREATE TABLE IF NOT EXISTS `mail_chain_x_crm_entity` (
  `id_tenant` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `id_chain` varchar(255) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  PRIMARY KEY (`id_tenant`,`id_mailbox`,`id_chain`,`entity_id`,`entity_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_contacts
CREATE TABLE IF NOT EXISTS `mail_contacts` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `id_user` varchar(255) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `address` varchar(255) NOT NULL,
  `last_modified` datetime DEFAULT NULL,
  `tenant` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_user_name_address` (`id_user`,`address`),
  KEY `last_modified` (`tenant`,`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_display_images
CREATE TABLE IF NOT EXISTS `mail_display_images` (
  `tenant` int(10) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  PRIMARY KEY (`tenant`,`id_user`,`address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_folder
CREATE TABLE IF NOT EXISTS `mail_folder` (
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `folder` smallint(5) unsigned NOT NULL,
  `time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `unread_messages_count` int(10) unsigned NOT NULL DEFAULT '0',
  `total_messages_count` int(10) unsigned NOT NULL DEFAULT '0',
  `unread_conversations_count` int(10) unsigned NOT NULL DEFAULT '0',
  `total_conversations_count` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`id_user`,`folder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_garbage
CREATE TABLE IF NOT EXISTS `mail_garbage` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `path` text NOT NULL,
  `is_processed` varchar(36) NOT NULL DEFAULT '',
  `time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_imap_flags
CREATE TABLE IF NOT EXISTS `mail_imap_flags` (
  `name` varchar(50) NOT NULL,
  `folder_id` int(11) NOT NULL,
  `skip` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_imap_special_mailbox
CREATE TABLE IF NOT EXISTS `mail_imap_special_mailbox` (
  `server` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `folder_id` int(11) NOT NULL,
  `skip` int(11) NOT NULL,
  PRIMARY KEY (`server`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_log
CREATE TABLE IF NOT EXISTS `mail_log` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_aggregator` int(11) NOT NULL,
  `id_thread` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `processing_start_time` datetime NOT NULL,
  `processing_end_time` datetime DEFAULT NULL,
  `processed_mails_count` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_mailbox` (`id_mailbox`),
  KEY `id_aggregator` (`id_aggregator`,`processing_start_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mail
CREATE TABLE IF NOT EXISTS `mail_mail` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_mailbox` int(11) NOT NULL DEFAULT '0',
  `id_user` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  `uidl` varchar(255) DEFAULT NULL,
  `md5` varchar(255) DEFAULT NULL,
  `address` varchar(255) NOT NULL,
  `from_text` text,
  `to_text` text,
  `reply_to` text,
  `cc` text,
  `bcc` text,
  `subject` text,
  `introduction` varchar(255) NOT NULL DEFAULT '',
  `importance` tinyint(1) NOT NULL DEFAULT '0',
  `date_received` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
  `date_sent` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
  `size` int(11) NOT NULL DEFAULT '0',
  `attachments_count` int(11) NOT NULL DEFAULT '0',
  `unread` int(11) NOT NULL DEFAULT '0',
  `is_answered` int(11) NOT NULL DEFAULT '0',
  `is_forwarded` int(11) NOT NULL DEFAULT '0',
  `is_from_crm` int(11) NOT NULL DEFAULT '0',
  `is_from_tl` int(11) NOT NULL DEFAULT '0',
  `is_text_body_only` int(11) NOT NULL DEFAULT '0',
  `has_parse_error` TINYINT(1) NOT NULL DEFAULT '0',
  `stream` varchar(38) NOT NULL,
  `folder` int(11) NOT NULL DEFAULT '1',
  `folder_restore` int(11) NOT NULL DEFAULT '1',
  `spam` int(11) NOT NULL DEFAULT '0',
  `time_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `is_removed` tinyint(1) NOT NULL DEFAULT '0',
  `mime_message_id` varchar(255) DEFAULT NULL,
  `mime_in_reply_to` varchar(255) DEFAULT NULL,
  `chain_id` varchar(255) DEFAULT NULL,
  `chain_date` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
  PRIMARY KEY (`id`),
  KEY `chain_index_folders` (`chain_id`,`id_mailbox`,`folder`),
  KEY `id_mailbox` (`id_mailbox`),
  KEY `time_modified` (`tenant`,`time_modified`),
  KEY `main` (`tenant`,`id_user`,`is_removed`,`folder`,`chain_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mailbox
CREATE TABLE IF NOT EXISTS `mail_mailbox` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `id_user` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  `pop3_password` varchar(255) DEFAULT NULL,
  `enabled` int(11) NOT NULL DEFAULT '1',
  `msg_count_last` int(11) NOT NULL DEFAULT '0',
  `size_last` int(11) NOT NULL DEFAULT '0',
  `smtp_password` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `login_delay` int(11) unsigned NOT NULL DEFAULT '30',
  `time_checked` bigint(20) unsigned NOT NULL DEFAULT '0',
  `is_processed` int(11) unsigned NOT NULL DEFAULT '0',
  `user_time_checked` bigint(20) unsigned NOT NULL DEFAULT '0',
  `login_delay_expires` bigint(20) unsigned NOT NULL DEFAULT '0',
  `is_removed` tinyint(1) NOT NULL DEFAULT '0',
  `quota_error` tinyint(1) NOT NULL DEFAULT '0',
  `auth_error` bigint(20) unsigned DEFAULT NULL,
  `imap` tinyint(1) NOT NULL DEFAULT '0',
  `begin_date` timestamp NOT NULL DEFAULT '1975-01-01 00:00:00',
  `service_type` tinyint(4) NOT NULL DEFAULT '0',
  `refresh_token` varchar(255) DEFAULT NULL,
  `imap_folders` text,
  `id_smtp_server` int(11) NOT NULL,
  `id_in_server` int(11) NOT NULL,
  `email_in_folder` text,
  PRIMARY KEY (`id`),
  KEY `address_index` (`address`),
  KEY `user_id_index` (`id_user`,`tenant`),
  KEY `login_delay_expires` (`time_checked`,`login_delay_expires`),
  KEY `main_mailbox_id_smtp_server_mail_mailbox_server_id` (`id_smtp_server`),
  KEY `main_mailbox_id_in_server_mail_mailbox_server_id` (`id_in_server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mailbox_domain
CREATE TABLE IF NOT EXISTS `mail_mailbox_domain` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_provider` int(11) NOT NULL DEFAULT '0',
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_provider` (`name`,`id_provider`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mailbox_provider
CREATE TABLE IF NOT EXISTS `mail_mailbox_provider` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `display_name` varchar(255) DEFAULT NULL,
  `display_short_name` varchar(255) DEFAULT NULL,
  `documentation` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mailbox_server
CREATE TABLE IF NOT EXISTS `mail_mailbox_server` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_provider` int(11) NOT NULL DEFAULT '0',
  `type` enum('pop3','imap','smtp') NOT NULL,
  `hostname` varchar(255) NOT NULL,
  `port` int(11) NOT NULL DEFAULT '0',
  `socket_type` enum('plain','SSL','STARTTLS') NOT NULL DEFAULT 'plain',
  `username` varchar(255) DEFAULT NULL,
  `authentication` varchar(255) DEFAULT NULL,
  `is_user_data` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `id_provider` (`id_provider`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_mailbox_signature
CREATE TABLE IF NOT EXISTS `mail_mailbox_signature` (
  `tenant` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `html` text,
  `is_active` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id_mailbox`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_pop_unordered_domain
CREATE TABLE IF NOT EXISTS `mail_pop_unordered_domain` (
  `server` varchar(255) NOT NULL,
  PRIMARY KEY (`server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_tag
CREATE TABLE IF NOT EXISTS `mail_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `style` varchar(20) DEFAULT NULL,
  `addresses` text NOT NULL,
  `count` int(10) NOT NULL DEFAULT '0',
  `crm_id` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `username` (`tenant`,`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_tag_addresses
CREATE TABLE IF NOT EXISTS `mail_tag_addresses` (
  `id_tag` int(11) unsigned NOT NULL,
  `address` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  PRIMARY KEY (`id_tag`,`address`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- mail_tag_mail
CREATE TABLE IF NOT EXISTS `mail_tag_mail` (
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `id_mail` int(11) NOT NULL,
  `id_tag` int(11) NOT NULL,
  `time_created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`id_user`,`id_mail`,`id_tag`),
  KEY `id_mail` (`id_mail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

