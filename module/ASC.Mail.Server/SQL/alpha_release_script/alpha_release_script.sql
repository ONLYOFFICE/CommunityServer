ALTER TABLE mail_mailbox
	ADD `is_teamlab_mailbox` INT(11) NOT NULL DEFAULT '0',
	ADD `date_created` DATETIME NULL DEFAULT NULL

CREATE TABLE IF NOT EXISTS `mail_server_domain` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_tenant_index` (`id`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_address` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(64) NOT NULL COMMENT 'Maximum name length - 63',
  `id_domain` int(11) DEFAULT NULL,
  `id_mailbox` int(11) DEFAULT NULL,
  `is_mail_group` tinyint(4) DEFAULT '0',
  `is_alias` tinyint(4) DEFAULT '0',
  `date_created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_tenant_index` (`id`,`tenant`),
  KEY `id_domain_fk_index` (`id_domain`),
  KEY `id_mailbox_fk_index` (`id_mailbox`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `mx_record` varchar(128) DEFAULT NULL,
  `connection_string` varchar(256) NOT NULL,
  `server_type` int(11) NOT NULL,
  `smtp_settings_id` int(11) NOT NULL,
  `imap_settings_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `mail_mailbox_smtp_server_fk_id` (`smtp_settings_id`),
  KEY `mail_mailbox_imap_server_fk_id` (`imap_settings_id`),
  KEY `mail_server_server_type_server_type_fk_id` (`server_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server_x_tenant` (
  `id_tenant` int(11) NOT NULL,
  `id_server` int(11) NOT NULL,
  `cname` varchar(34) NOT NULL,
  PRIMARY KEY (`id_tenant`,`id_server`),
  KEY `mail_server_server_x_tenant_server_fk_id` (`id_server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server_type` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_mail_group` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_tenant` int(11) NOT NULL,
  `id_address` int(11) NOT NULL,
  `date_created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `mail_server_address_fk_id` (`id_address`),
  KEY `mail_server_mail_group_tenant_seatch_indx` (`id`,`id_tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_mail_group_x_mail_server_address` (
  `id_address` int(11) NOT NULL,
  `id_mail_group` int(11) NOT NULL,
  PRIMARY KEY (`id_address`,`id_mail_group`),
  KEY `mail_server_mail_group_fk_id` (`id_mail_group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_domain_x_cname` (
  `id_domain` int(11) NOT NULL,
  `cname` varchar(32) NOT NULL,
  `reference_url` varchar(256) NOT NULL,
  `verified` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`cname`),
  KEY `domain_id_x_mail_server_domain_id_fk_index` (`id_domain`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;