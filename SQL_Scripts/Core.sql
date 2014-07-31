-- audit_events
CREATE TABLE IF NOT EXISTS `audit_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `ip` varchar(50) DEFAULT NULL,
  `initiator` varchar(200) DEFAULT NULL,
  `browser` varchar(200) DEFAULT NULL,
  `mobile` tinyint(4) NOT NULL DEFAULT '0',
  `platform` varchar(200) DEFAULT NULL,
  `date` datetime NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `user_id` char(38) DEFAULT NULL,
  `page` varchar(100) DEFAULT NULL,
  `action` int(11) DEFAULT NULL,
  `description` varchar(20000) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- sso_links
CREATE TABLE IF NOT EXISTS `sso_links` (
  `id` varchar(200) NOT NULL,
  `uid` varchar(200) NOT NULL,
  `profile` varchar(200) NOT NULL,
  PRIMARY KEY (`id`,`uid`,`profile`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- sso_tokens
CREATE TABLE IF NOT EXISTS `sso_tokens` (
  `tokenType` varchar(50) NOT NULL,
  `tenant` int(11) NOT NULL,
  `tokenId` varchar(100) NOT NULL,
  `expirationDate` datetime NOT NULL,
  UNIQUE KEY `tokenId` (`tokenId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_acl
CREATE TABLE IF NOT EXISTS `core_acl` (
  `tenant` int(11) NOT NULL,
  `subject` varchar(38) NOT NULL,
  `action` varchar(38) NOT NULL,
  `object` varchar(255) NOT NULL DEFAULT '',
  `acetype` int(11) NOT NULL,
  PRIMARY KEY (`tenant`,`subject`,`action`,`object`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_group
CREATE TABLE IF NOT EXISTS `core_group` (
  `tenant` int(11) NOT NULL,
  `id` varchar(38) NOT NULL,
  `name` varchar(128) NOT NULL,
  `categoryid` varchar(38) DEFAULT NULL,
  `parentid` varchar(38) DEFAULT NULL,
  `sid` varchar(512) DEFAULT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `parentid` (`tenant`,`parentid`),
  KEY `last_modified` (`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_logging
CREATE TABLE IF NOT EXISTS `core_logging` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `user_id` varchar(38) NOT NULL,
  `user_email` varchar(50) DEFAULT NULL,
  `caller_ip` varchar(50) DEFAULT NULL,
  `tenant_id` int(10) NOT NULL,
  `action` text NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`,`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_settings
CREATE TABLE IF NOT EXISTS `core_settings` (
  `tenant` int(11) NOT NULL,
  `id` varchar(128) NOT NULL,
  `value` mediumblob NOT NULL,
  PRIMARY KEY (`tenant`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_subscription
CREATE TABLE IF NOT EXISTS `core_subscription` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `object` varchar(128) NOT NULL,
  `unsubscribed` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`,`object`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_subscriptionmethod
CREATE TABLE IF NOT EXISTS `core_subscriptionmethod` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `sender` varchar(1024) NOT NULL,
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_user
CREATE TABLE IF NOT EXISTS `core_user` (
  `tenant` int(11) NOT NULL,
  `id` varchar(38) NOT NULL,
  `username` varchar(255) NOT NULL,
  `firstname` varchar(64) NOT NULL,
  `lastname` varchar(64) NOT NULL,
  `sex` int(11) DEFAULT NULL,
  `bithdate` datetime DEFAULT NULL,
  `status` int(11) NOT NULL DEFAULT '1',
  `activation_status` int(11) NOT NULL DEFAULT '0',
  `email` varchar(255) DEFAULT NULL,
  `workfromdate` datetime DEFAULT NULL,
  `terminateddate` datetime DEFAULT NULL,
  `title` varchar(64) DEFAULT NULL,
  `department` varchar(128) DEFAULT NULL,
  `culture` varchar(20) DEFAULT NULL,
  `contacts` varchar(1024) DEFAULT NULL,
  `phone` varchar(255) DEFAULT NULL,
  `phone_activation` int(11) NOT NULL DEFAULT '0',
  `location` varchar(255) DEFAULT NULL,
  `notes` varchar(512) DEFAULT NULL,
  `sid` varchar(512) DEFAULT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `last_modified` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `last_modified` (`last_modified`),
  KEY `username` (`tenant`,`username`),
  KEY `email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_usergroup
CREATE TABLE IF NOT EXISTS `core_usergroup` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `groupid` varchar(38) NOT NULL,
  `ref_type` int(11) NOT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`userid`,`groupid`,`ref_type`),
  KEY `last_modified` (`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_userphoto
CREATE TABLE IF NOT EXISTS `core_userphoto` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `photo` mediumblob NOT NULL,
  PRIMARY KEY (`userid`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- core_usersecurity
CREATE TABLE IF NOT EXISTS `core_usersecurity` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `pwdhash` varchar(512) DEFAULT NULL,
  `pwdhashsha512` varchar(512) DEFAULT NULL,
  PRIMARY KEY (`userid`),
  KEY `pwdhash` (`pwdhash`(255)),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- feed_aggregate
CREATE TABLE IF NOT EXISTS `feed_aggregate` (
  `id` varchar(88) NOT NULL,
  `tenant` int(10) NOT NULL,
  `product` varchar(50) NOT NULL,
  `module` varchar(50) DEFAULT NULL,
  `author` char(38) DEFAULT NULL,
  `created_date` datetime DEFAULT NULL,
  `group_id` varchar(70) DEFAULT NULL,
  `json` mediumtext,
  `feed` mediumtext,
  `keywords` text,
  `aggregated_date` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `product` (`tenant`,`product`),
  KEY `aggregated_date` (`aggregated_date`,`tenant`),
  KEY `created_date` (`tenant`,`created_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- feed_last
CREATE TABLE IF NOT EXISTS `feed_last` (
  `last_key` varchar(128) NOT NULL,
  `last_date` datetime NOT NULL,
  PRIMARY KEY (`last_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- feed_readed
CREATE TABLE IF NOT EXISTS `feed_readed` (
  `user_id` varchar(38) NOT NULL,
  `timestamp` datetime NOT NULL,
  `module` varchar(50) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`tenant_id`,`user_id`,`module`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- feed_users
CREATE TABLE IF NOT EXISTS `feed_users` (
  `feed_id` varchar(88) NOT NULL,
  `user_id` char(38) NOT NULL,
  PRIMARY KEY (`feed_id`,`user_id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- login_events
CREATE TABLE IF NOT EXISTS `login_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `ip` varchar(50) DEFAULT NULL,
  `login` varchar(200) DEFAULT NULL,
  `browser` varchar(200) DEFAULT NULL,
  `mobile` tinyint(4) NOT NULL DEFAULT '0',
  `platform` varchar(200) DEFAULT NULL,
  `date` datetime NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `user_id` char(38) NOT NULL,
  `page` varchar(300) NOT NULL,
  `action` int(11) DEFAULT NULL,
  `description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- notify_info
CREATE TABLE IF NOT EXISTS `notify_info` (
  `notify_id` int(10) NOT NULL,
  `state` int(10) NOT NULL DEFAULT '0',
  `attempts` int(10) NOT NULL DEFAULT '0',
  `modify_date` datetime NOT NULL,
  `priority` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`notify_id`),
  KEY `state` (`state`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- notify_queue
CREATE TABLE IF NOT EXISTS `notify_queue` (
  `notify_id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant_id` int(11) NOT NULL,
  `sender` varchar(255) DEFAULT NULL,
  `reciever` varchar(255) DEFAULT NULL,
  `subject` varchar(1024) DEFAULT NULL,
  `content_type` varchar(64) DEFAULT NULL,
  `content` text,
  `sender_type` varchar(64) DEFAULT NULL,
  `reply_to` varchar(1024) DEFAULT NULL,
  `creation_date` datetime NOT NULL,
  PRIMARY KEY (`notify_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_forbiden
CREATE TABLE IF NOT EXISTS `tenants_forbiden` (
  `address` varchar(50) NOT NULL,
  PRIMARY KEY (`address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_partners
CREATE TABLE IF NOT EXISTS `tenants_partners` (
  `tenant_id` int(10) NOT NULL,
  `partner_id` varchar(36) NOT NULL,
  PRIMARY KEY (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_quota
CREATE TABLE IF NOT EXISTS `tenants_quota` (
  `tenant` int(10) NOT NULL,
  `name` varchar(128) DEFAULT NULL,
  `description` varchar(128) DEFAULT NULL,
  `max_file_size` bigint(20) NOT NULL DEFAULT '0',
  `max_total_size` bigint(20) NOT NULL DEFAULT '0',
  `active_users` int(10) NOT NULL DEFAULT '0',
  `features` text,
  `price` decimal(10,2) NOT NULL DEFAULT '0.00',
  `price2` decimal(10,2) NOT NULL DEFAULT '0.00',
  `avangate_id` varchar(128) DEFAULT NULL,
  `visible` int(10) NOT NULL DEFAULT '0',
  `https_enable` int(10) NOT NULL DEFAULT '0',
  `security_enable` int(10) NOT NULL DEFAULT '0',
  `sms_auth` int(10) NOT NULL DEFAULT '0',
  `branding` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_quotarow
CREATE TABLE IF NOT EXISTS `tenants_quotarow` (
  `tenant` int(11) NOT NULL,
  `path` varchar(255) NOT NULL,
  `counter` bigint(20) NOT NULL DEFAULT '0',
  `tag` varchar(1024) DEFAULT NULL,
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`path`),
  KEY `last_modified` (`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_tariff
CREATE TABLE IF NOT EXISTS `tenants_tariff` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `tenant` int(10) NOT NULL,
  `tariff` int(10) NOT NULL,
  `stamp` datetime NOT NULL,
  `tariff_key` varchar(64) DEFAULT NULL,
  `comment` varchar(255) DEFAULT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_tenants
CREATE TABLE IF NOT EXISTS `tenants_tenants` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `alias` varchar(100) NOT NULL,
  `mappeddomain` varchar(100) DEFAULT NULL,
  `version` int(10) NOT NULL DEFAULT '2',
  `version_changed` datetime DEFAULT NULL,
  `language` char(10) NOT NULL DEFAULT 'en-US',
  `timezone` varchar(50) DEFAULT NULL,
  `trusteddomains` varchar(1024) DEFAULT NULL,
  `trusteddomainsenabled` int(10) NOT NULL DEFAULT '1',
  `status` int(11) NOT NULL DEFAULT '0',
  `statuschanged` datetime DEFAULT NULL,
  `creationdatetime` datetime NOT NULL,
  `owner_id` varchar(38) DEFAULT NULL,
  `public` int(10) NOT NULL DEFAULT '0',
  `publicvisibleproducts` varchar(1024) DEFAULT NULL,
  `payment_id` varchar(38) DEFAULT NULL,
  `industry` INT(10) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `alias` (`alias`),
  KEY `last_modified` (`last_modified`),
  KEY `mappeddomain` (`mappeddomain`),
  KEY `version` (`version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- tenants_version
CREATE TABLE IF NOT EXISTS `tenants_version` (
  `id` int(10) NOT NULL,
  `version` varchar(64) NOT NULL,
  `url` varchar(64) NOT NULL,
  `default_version` int(11) NOT NULL DEFAULT '0',
  `visible` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

