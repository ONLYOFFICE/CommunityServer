CREATE TABLE IF NOT EXISTS `account_links` (
  `id` varchar(200) NOT NULL,
  `uid` varchar(200) NOT NULL,
  `provider` char(60) DEFAULT NULL,
  `profile` text NOT NULL,
  `linked` datetime NOT NULL,
  PRIMARY KEY (`id`,`uid`),
  KEY `uid` (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `audit_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `ip` varchar(50) DEFAULT NULL,
  `initiator` varchar(200) DEFAULT NULL,
  `browser` varchar(200) DEFAULT NULL,
  `platform` varchar(200) DEFAULT NULL,
  `date` datetime NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `user_id` char(38) DEFAULT NULL,
  `page` varchar(300) DEFAULT NULL,
  `action` int(11) DEFAULT NULL,
  `description` varchar(20000) DEFAULT NULL,
  `target` text,
  PRIMARY KEY (`id`),
  KEY `date` (`tenant_id`,`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `backup_backup` (
  `id` char(38) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `is_scheduled` int(1) NOT NULL,
  `name` varchar(255) NOT NULL,
  `storage_type` int(11) NOT NULL,
  `storage_base_path` varchar(255) DEFAULT NULL,
  `storage_path` varchar(255) NOT NULL,
  `created_on` datetime NOT NULL,
  `expires_on` datetime NOT NULL DEFAULT '0001-01-01 00:00:00',
  `storage_params` TEXT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `expires_on` (`expires_on`),
  KEY `is_scheduled` (`is_scheduled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `backup_schedule` (
  `tenant_id` int(11) NOT NULL,
  `backup_mail` int(11) NOT NULL DEFAULT '0',
  `cron` varchar(255) NOT NULL,
  `backups_stored` int(11) NOT NULL,
  `storage_type` int(11) NOT NULL,
  `storage_base_path` varchar(255) DEFAULT NULL,
  `last_backup_time` datetime NOT NULL,
  `storage_params` TEXT NULL,
  PRIMARY KEY (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `blogs_comments` (
  `tenant` int(11) NOT NULL,
  `id` char(38) NOT NULL,
  `post_id` char(38) NOT NULL,
  `content` text,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `parent_id` char(38) DEFAULT NULL,
  `inactive` int(11) DEFAULT NULL,
  PRIMARY KEY (`tenant`,`id`),
  KEY `ixComments_PostId` (`tenant`,`post_id`),
  KEY `ixComments_Created` (`created_when`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `blogs_posts` (
  `post_id` int(11) NOT NULL AUTO_INCREMENT,
  `id` char(38) NOT NULL,
  `title` varchar(255) NOT NULL,
  `content` mediumtext NOT NULL,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `blog_id` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastCommentId` char(38) DEFAULT NULL,
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`post_id`),
  KEY `ixPosts_CreatedBy` (`Tenant`,`created_by`),
  KEY `ixPosts_LastCommentId` (`Tenant`,`LastCommentId`),
  KEY `ixPosts_CreatedWhen` (`created_when`,`Tenant`),
  KEY `LastModified` (`LastModified`),
  KEY `Tenant` (`Tenant`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `blogs_reviewposts` (
  `post_id` char(38) NOT NULL,
  `reviewed_by` char(38) NOT NULL,
  `timestamp` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`post_id`,`reviewed_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `blogs_tags` (
  `post_id` varchar(38) NOT NULL,
  `name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`post_id`,`name`),
  KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_bookmark` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `URL` text,
  `Date` datetime DEFAULT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` text,
  `UserCreatorID` char(38) DEFAULT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `Tenant` (`Tenant`),
  KEY `DateIndex` (`Date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_bookmarktag` (
  `BookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`BookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_comment` (
  `ID` char(38) NOT NULL,
  `UserID` char(38) DEFAULT NULL,
  `Content` text,
  `Datetime` datetime DEFAULT NULL,
  `Parent` char(38) DEFAULT NULL,
  `BookmarkID` int(11) DEFAULT NULL,
  `Inactive` int(11) DEFAULT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`ID`),
  KEY `IndexCommentBookmarkID` (`Tenant`,`BookmarkID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_tag` (
  `TagID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`TagID`),
  KEY `Name` (`Tenant`,`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_userbookmark` (
  `UserBookmarkID` int(11) NOT NULL AUTO_INCREMENT,
  `UserID` char(38) DEFAULT NULL,
  `DateAdded` datetime DEFAULT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` text,
  `BookmarkID` int(11) NOT NULL,
  `Raiting` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL,
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserBookmarkID`),
  KEY `BookmarkID` (`BookmarkID`),
  KEY `LastModified` (`LastModified`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `bookmarking_userbookmarktag` (
  `UserBookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`UserBookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_calendars` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `owner_id` char(38) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  `tenant` int(10) NOT NULL,
  `text_color` varchar(50) NOT NULL DEFAULT '#000000',
  `background_color` varchar(50) NOT NULL DEFAULT '#fa9191',
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `time_zone` varchar(255) NOT NULL DEFAULT 'UTC',
  `ical_url` mediumtext,
  `caldav_guid` char(38) DEFAULT NULL,
  `is_todo` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `owner_id` (`tenant`,`owner_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_calendar_item` (
  `calendar_id` int(10) NOT NULL,
  `item_id` char(38) NOT NULL,
  `is_group` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`calendar_id`,`item_id`,`is_group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_calendar_user` (
  `calendar_id` int(10) NOT NULL DEFAULT '0',
  `ext_calendar_id` varchar(50) NOT NULL DEFAULT '',
  `user_id` char(38) NOT NULL,
  `hide_events` smallint(2) NOT NULL DEFAULT '0',
  `is_accepted` smallint(2) NOT NULL DEFAULT '0',
  `text_color` varchar(50) NOT NULL,
  `background_color` varchar(50) NOT NULL,
  `is_new` smallint(2) NOT NULL DEFAULT '0',
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `name` varchar(255) DEFAULT NULL,
  `time_zone` varchar(255) DEFAULT 'UTC',
  PRIMARY KEY (`calendar_id`,`ext_calendar_id`,`user_id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `calendar_id` int(11) NOT NULL,
  `start_date` datetime NOT NULL,
  `end_date` datetime NOT NULL,
  `update_date` datetime NULL DEFAULT NULL,
  `all_day_long` smallint(6) NOT NULL DEFAULT '0',
  `repeat_type` smallint(6) NOT NULL DEFAULT '0',
  `owner_id` char(38) NOT NULL,
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `rrule` text NOT NULL,
  `uid` varchar(255) DEFAULT NULL,
  `status` smallint(6) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `calendar_id` (`tenant`,`calendar_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_event_history` (
  `tenant` int(11) NOT NULL,
  `calendar_id` int(11) NOT NULL,
  `event_uid` char(255) NOT NULL,
  `event_id` int(10) NOT NULL DEFAULT '0',
  `ics` mediumtext,
  PRIMARY KEY (`tenant`,`calendar_id`,`event_uid`),
  KEY `event_id` (`tenant`,`event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_event_item` (
  `event_id` int(10) NOT NULL,
  `item_id` char(38) NOT NULL,
  `is_group` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`event_id`,`item_id`,`is_group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_event_user` (
  `event_id` int(10) NOT NULL,
  `user_id` char(38) NOT NULL,
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `is_unsubscribe` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`event_id`,`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `calendar_notifications` (
  `user_id` char(38) NOT NULL,
  `event_id` int(10) NOT NULL,
  `notify_date` datetime NOT NULL,
  `tenant` int(10) NOT NULL,
  `alert_type` smallint(2) NOT NULL,
  `repeat_type` smallint(2) NOT NULL DEFAULT '0',
  `time_zone` varchar(255) NOT NULL DEFAULT 'UTC',
  `rrule` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`user_id`,`event_id`),
  KEY `event_id` (`event_id`),
  KEY `notify_date` (`notify_date`)
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

CREATE TABLE IF NOT EXISTS `core_acl` (
  `tenant` int(11) NOT NULL,
  `subject` varchar(38) NOT NULL,
  `action` varchar(38) NOT NULL,
  `object` varchar(255) NOT NULL DEFAULT '',
  `acetype` int(11) NOT NULL,
  PRIMARY KEY (`tenant`,`subject`,`action`,`object`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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

CREATE TABLE IF NOT EXISTS `core_settings` (
  `tenant` int(11) NOT NULL,
  `id` varchar(128) NOT NULL,
  `value` mediumblob NOT NULL,
  `last_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `core_subscription` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `object` varchar(128) NOT NULL,
  `unsubscribed` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`,`object`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

CREATE TABLE IF NOT EXISTS `core_subscriptionmethod` (
  `tenant` int(11) NOT NULL,
  `source` varchar(38) NOT NULL,
  `action` varchar(128) NOT NULL,
  `recipient` varchar(38) NOT NULL,
  `sender` varchar(1024) NOT NULL,
  PRIMARY KEY (`tenant`,`source`,`action`,`recipient`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  `sso_name_id` varchar(512) DEFAULT NULL,
  `sso_session_id` varchar(512) DEFAULT NULL,
  `removed` int(11) NOT NULL DEFAULT '0',
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `last_modified` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `last_modified` (`last_modified`),
  KEY `username` (`tenant`,`username`),
  KEY `email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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

CREATE TABLE IF NOT EXISTS `core_userphoto` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `photo` mediumblob NOT NULL,
  PRIMARY KEY (`userid`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `core_usersecurity` (
  `tenant` int(11) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `pwdhash` varchar(512) DEFAULT NULL,
  `pwdhashsha512` varchar(512) DEFAULT NULL,
  `LastModified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`userid`),
  KEY `pwdhash` (`pwdhash`(255)),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_case` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `is_closed` tinyint(1) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `create_on` (`create_on`),
  KEY `last_modifed_on` (`last_modifed_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_contact` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant_id` int(11) NOT NULL,
  `is_company` tinyint(1) NOT NULL,
  `notes` text,
  `title` varchar(255) DEFAULT NULL,
  `first_name` varchar(255) DEFAULT NULL,
  `last_name` varchar(255) DEFAULT NULL,
  `company_name` varchar(255) DEFAULT NULL,
  `industry` varchar(255) DEFAULT NULL,
  `status_id` int(11) NOT NULL DEFAULT '0',
  `company_id` int(11) NOT NULL,
  `contact_type_id` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `display_name` varchar(255) DEFAULT NULL,
  `is_shared` tinyint(4) DEFAULT NULL,
  `currency` varchar(3) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `company_id` (`tenant_id`,`company_id`),
  KEY `display_name` (`tenant_id`,`display_name`),
  KEY `create_on` (`create_on`),
  KEY `last_modifed_on` (`last_modifed_on`,`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_contact_info` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `data` text NOT NULL,
  `category` int(255) NOT NULL,
  `tenant_id` int(255) NOT NULL,
  `is_primary` tinyint(4) NOT NULL,
  `contact_id` int(11) NOT NULL,
  `type` int(255) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_Contact` (`tenant_id`,`contact_id`),
  KEY `last_modifed_on` (`last_modifed_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_currency_info` (
  `resource_key` varchar(255) NOT NULL,
  `abbreviation` varchar(255) NOT NULL,
  `symbol` varchar(255) NOT NULL,
  `culture_name` varchar(255) NOT NULL,
  `is_convertable` tinyint(4) NOT NULL,
  `is_basic` tinyint(4) NOT NULL,
  PRIMARY KEY (`abbreviation`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_currency_rate` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `from_currency` varchar(255) NOT NULL,
  `to_currency` varchar(255) NOT NULL,
  `rate` decimal(10,2) NOT NULL DEFAULT '0.00',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `from_currency` (`from_currency`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_deal` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `description` text,
  `responsible_id` char(38) NOT NULL,
  `contact_id` int(11) NOT NULL,
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `bid_currency` varchar(255) DEFAULT NULL,
  `bid_value` decimal(50,9) NOT NULL DEFAULT '0.000000000',
  `bid_type` int(11) NOT NULL DEFAULT '0',
  `deal_milestone_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `expected_close_date` datetime NOT NULL,
  `per_period_value` int(11) NOT NULL DEFAULT '0',
  `deal_milestone_probability` int(11) DEFAULT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `actual_close_date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `contact_id` (`tenant_id`,`contact_id`),
  KEY `create_on` (`create_on`),
  KEY `deal_milestone_id` (`deal_milestone_id`),
  KEY `last_modifed_on` (`last_modifed_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_deal_milestone` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `color` varchar(50) NOT NULL DEFAULT '0',
  `sort_order` int(10) NOT NULL DEFAULT '0',
  `title` varchar(250) NOT NULL,
  `description` text,
  `probability` int(10) NOT NULL DEFAULT '0',
  `status` int(10) NOT NULL DEFAULT '0',
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_entity_contact` (
  `entity_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  `contact_id` int(11) NOT NULL,
  PRIMARY KEY (`entity_id`,`entity_type`,`contact_id`),
  KEY `IX_Contact` (`contact_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_entity_tag` (
  `tag_id` int(11) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `entity_type` int(10) NOT NULL,
  PRIMARY KEY (`entity_id`,`entity_type`,`tag_id`),
  KEY `tag_id` (`tag_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_field_description` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant_id` int(11) NOT NULL,
  `label` varchar(255) NOT NULL,
  `type` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `mask` text,
  `entity_type` int(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `entity_type` (`tenant_id`,`entity_type`,`sort_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_field_value` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `value` text,
  `entity_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `field_id` int(11) NOT NULL,
  `entity_type` int(10) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `field_id` (`field_id`),
  KEY `last_modifed_on` (`last_modifed_on`),
  KEY `tenant_id` (`tenant_id`,`entity_id`,`entity_type`,`field_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_invoice` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `status` int(11) NOT NULL DEFAULT '1',
  `number` varchar(255) NOT NULL,
  `issue_date` datetime NOT NULL,
  `template_type` int(11) NOT NULL DEFAULT '0',
  `contact_id` int(11) NOT NULL DEFAULT '-1',
  `consignee_id` int(11) NOT NULL DEFAULT '-1',
  `entity_type` int(11) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `due_date` datetime NOT NULL,
  `language` varchar(255) NOT NULL,
  `currency` varchar(255) NOT NULL,
  `exchange_rate` decimal(10,2) NOT NULL DEFAULT '1.00',
  `purchase_order_number` varchar(255) NOT NULL,
  `terms` text,
  `description` text,
  `json_data` text,
  `file_id` int(11) NOT NULL DEFAULT '-1',
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_invoice_item` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `stock_keeping_unit` varchar(255) NOT NULL,
  `price` decimal(10,2) NOT NULL DEFAULT '0.00',
  `stock_quantity` decimal(10,2) NOT NULL DEFAULT '0.00',
  `track_inventory` tinyint(4) NOT NULL DEFAULT '0',
  `invoice_tax1_id` int(11) NOT NULL DEFAULT '0',
  `invoice_tax2_id` int(11) NOT NULL DEFAULT '0',
  `currency` varchar(255) NOT NULL,
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_invoice_line` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `invoice_id` int(11) NOT NULL,
  `invoice_item_id` int(11) NOT NULL,
  `invoice_tax1_id` int(11) NOT NULL,
  `invoice_tax2_id` int(11) NOT NULL,
  `description` text NOT NULL,
  `quantity` decimal(10,2) NOT NULL DEFAULT '0.00',
  `price` decimal(10,2) NOT NULL DEFAULT '0.00',
  `discount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `tenant_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_invoice_tax` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `rate` decimal(10,2) NOT NULL DEFAULT '0.00',
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_list_item` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `color` varchar(255) DEFAULT NULL,
  `additional_params` varchar(255) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `list_type` int(255) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `list_type` (`tenant_id`,`list_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_organisation_logo` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `content` mediumtext NOT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_projects` (
  `project_id` int(10) NOT NULL,
  `contact_id` int(10) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`tenant_id`,`contact_id`,`project_id`),
  KEY `project_id` (`tenant_id`,`project_id`),
  KEY `contact_id` (`tenant_id`,`contact_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_relationship_event` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `contact_id` int(11) NOT NULL,
  `content` text,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `category_id` int(11) NOT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `have_files` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `IX_Contact` (`contact_id`),
  KEY `IX_Entity` (`entity_id`,`entity_type`),
  KEY `last_modifed_on` (`last_modifed_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_report_file` (
  `file_id` int(11) NOT NULL,
  `report_type` int(11) NOT NULL,
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`file_id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `create_by` (`create_by`),
  KEY `create_on` (`create_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_task` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `description` text,
  `deadline` datetime NOT NULL,
  `responsible_id` char(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `contact_id` int(11) NOT NULL DEFAULT '-1',
  `is_closed` int(1) NOT NULL DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `category_id` int(11) NOT NULL DEFAULT '0',
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL,
  `last_modifed_on` datetime DEFAULT NULL,
  `last_modifed_by` char(38) DEFAULT NULL,
  `alert_value` int(10) NOT NULL DEFAULT '0',
  `exec_alert` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `responsible_id` (`tenant_id`,`responsible_id`),
  KEY `IX_Contact` (`tenant_id`,`contact_id`),
  KEY `IX_Entity` (`tenant_id`,`entity_id`,`entity_type`),
  KEY `create_on` (`create_on`),
  KEY `deadline` (`deadline`),
  KEY `last_modifed_on` (`last_modifed_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_task_template` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `last_modifed_on` datetime NOT NULL,
  `last_modifed_by` char(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `title` varchar(255) NOT NULL,
  `category_id` int(10) NOT NULL,
  `description` tinytext,
  `responsible_id` char(38) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `is_notify` tinyint(4) NOT NULL,
  `offset` bigint(20) NOT NULL DEFAULT '0',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `deadLine_is_fixed` tinyint(4) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `container_id` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `template_id` (`tenant_id`,`container_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_task_template_container` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `title` varchar(256) NOT NULL,
  `entity_type` int(10) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `create_on` datetime NOT NULL,
  `create_by` char(38) NOT NULL DEFAULT '0',
  `last_modifed_on` datetime NOT NULL,
  `last_modifed_by` char(38) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `entity_type` (`tenant_id`,`entity_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_task_template_task` (
  `task_id` int(10) NOT NULL,
  `task_template_id` int(10) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`tenant_id`,`task_id`,`task_template_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_voip_calls` (
  `id` varchar(50) NOT NULL,
  `parent_call_id` varchar(50) NOT NULL,
  `number_from` varchar(50) NOT NULL,
  `number_to` varchar(50) NOT NULL,
  `status` int(10) DEFAULT NULL,
  `answered_by` varchar(50) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `dial_date` datetime DEFAULT NULL,
  `dial_duration` int(11) DEFAULT NULL,
  `record_sid` VARCHAR(50) NULL DEFAULT NULL,
  `record_url` text,
  `record_duration` int(11) DEFAULT NULL,
  `record_price` DECIMAL(10,4) NOT NULL,
  `contact_id` int(10) DEFAULT NULL,
  `price` decimal(10,4) DEFAULT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `parent_call_id` (`parent_call_id`, `tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `crm_voip_number` (
  `id` varchar(50) NOT NULL,
  `number` varchar(50) NOT NULL,
  `alias` varchar(255) DEFAULT NULL,
  `settings` text,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `dbip_location` (
	`id` INT(10) NOT NULL AUTO_INCREMENT,
	`addr_type` ENUM('ipv4','ipv6') NOT NULL,
	`ip_start` VARCHAR(39) NOT NULL,
	`ip_end` VARCHAR(39) NOT NULL,
	`country` VARCHAR(2) NOT NULL,
	`stateprov` VARCHAR(255) NOT NULL,
	`district` VARCHAR(255) NULL DEFAULT NULL,
	`city` VARCHAR(255) NOT NULL,
	`zipcode` VARCHAR(255) NULL DEFAULT NULL,
	`latitude` FLOAT NULL DEFAULT NULL,
	`longitude` FLOAT NULL DEFAULT NULL,
	`geoname_id` INT(11) NULL DEFAULT NULL,
	`timezone_offset` INT(10) NULL DEFAULT NULL,
	`timezone_name` VARCHAR(255) NULL DEFAULT NULL,
	`processed` INT(11) NOT NULL DEFAULT '1',
	PRIMARY KEY (`id`),
	INDEX `ip_start` (`ip_start`)
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `dbsync_last` (
  `last_key` varchar(128) NOT NULL,
  `last_date` datetime NOT NULL,
  PRIMARY KEY (`last_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_comment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Feed` int(11) NOT NULL,
  `Comment` text NOT NULL,
  `Parent` int(11) NOT NULL DEFAULT '0',
  `Date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Creator` varchar(38) DEFAULT NULL,
  `Inactive` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_feed` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FeedType` int(11) NOT NULL DEFAULT '1',
  `Caption` text NOT NULL,
  `Text` text,
  `Date` datetime NOT NULL,
  `Creator` varchar(38) DEFAULT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `Date` (`Date`),
  KEY `LastModified` (`LastModified`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_poll` (
  `Id` int(11) NOT NULL,
  `PollType` int(11) NOT NULL DEFAULT '0',
  `StartDate` datetime NOT NULL,
  `EndDate` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_pollanswer` (
  `Variant` int(11) NOT NULL,
  `User` varchar(64) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Variant`,`User`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_pollvariant` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Poll` int(11) NOT NULL,
  `Name` varchar(1024) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Poll` (`Tenant`,`Poll`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `events_reader` (
  `Feed` int(11) NOT NULL,
  `Reader` varchar(38) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Feed`,`Reader`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `feed_aggregate` (
  `id` varchar(88) NOT NULL,
  `tenant` int(10) NOT NULL,
  `product` varchar(50) NOT NULL,
  `module` varchar(50) NOT NULL,
  `author` char(38) NOT NULL,
  `modified_by` char(38) NOT NULL,
  `created_date` datetime NOT NULL,
  `modified_date` datetime NOT NULL,
  `group_id` varchar(70) DEFAULT NULL,
  `json` mediumtext NOT NULL,
  `keywords` text,
  `aggregated_date` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `product` (`tenant`,`product`),
  KEY `aggregated_date` (`tenant`,`aggregated_date`),
  KEY `modified_date` (`tenant`,`modified_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `feed_last` (
  `last_key` varchar(128) NOT NULL,
  `last_date` datetime NOT NULL,
  PRIMARY KEY (`last_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `feed_readed` (
  `user_id` varchar(38) NOT NULL,
  `timestamp` datetime NOT NULL,
  `module` varchar(50) NOT NULL,
  `tenant_id` int(10) NOT NULL,
  PRIMARY KEY (`tenant_id`,`user_id`,`module`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `feed_users` (
  `feed_id` varchar(88) NOT NULL,
  `user_id` char(38) NOT NULL,
  PRIMARY KEY (`feed_id`,`user_id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_bunch_objects` (
  `tenant_id` int(10) NOT NULL,
  `right_node` varchar(255) NOT NULL,
  `left_node` varchar(255) NOT NULL,
  PRIMARY KEY (`tenant_id`,`right_node`),
  KEY `left_node` (`left_node`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_converts` (
  `input` varchar(50) NOT NULL,
  `output` varchar(50) NOT NULL,
  PRIMARY KEY (`input`,`output`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_file` (
  `id` int(11) NOT NULL,
  `version` int(11) NOT NULL,
  `version_group` int(11) NOT NULL DEFAULT '1',
  `current_version` int(11) NOT NULL DEFAULT '0',
  `folder_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(400) NOT NULL,
  `content_length` bigint(25) NOT NULL DEFAULT '0',
  `file_status` int(11) NOT NULL DEFAULT '0',
  `category` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `converted_type` varchar(10) DEFAULT NULL,
  `comment` varchar(255) DEFAULT NULL,
  `changes` mediumtext,
  `encrypted` int(1) NOT NULL DEFAULT '0',
  `forcesave` int(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant_id`,`id`,`version`),
  KEY `modified_on` (`modified_on`),
  KEY `folder_id` (`folder_id`),
  KEY `id` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_folder` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parent_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(400) NOT NULL,
  `folder_type` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `foldersCount` int(10) NOT NULL DEFAULT '0',
  `filesCount` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `parent_id` (`tenant_id`,`parent_id`),
  KEY `modified_on` (`modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_folder_tree` (
  `folder_id` int(11) NOT NULL,
  `parent_id` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  PRIMARY KEY (`parent_id`,`folder_id`),
  KEY `folder_id` (`folder_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_security` (
  `tenant_id` int(10) NOT NULL,
  `entry_id` varchar(50) NOT NULL,
  `entry_type` int(10) NOT NULL,
  `subject` char(38) NOT NULL,
  `owner` char(38) NOT NULL,
  `security` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant_id`,`entry_id`,`entry_type`,`subject`),
  KEY `owner` (`owner`),
  KEY `tenant_id` (`tenant_id`,`entry_type`,`entry_id`,`owner`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `owner` varchar(38) NOT NULL,
  `flag` int(11) NOT NULL DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`tenant_id`,`owner`,`name`,`flag`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_tag_link` (
  `tenant_id` int(10) NOT NULL,
  `tag_id` int(10) NOT NULL,
  `entry_type` int(10) NOT NULL,
  `entry_id` varchar(32) NOT NULL,
  `create_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `tag_count` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant_id`,`tag_id`,`entry_id`,`entry_type`),
  KEY `entry_id` (`tenant_id`,`entry_id`,`entry_type`),
  KEY `create_on` (`create_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_thirdparty_account` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `provider` varchar(50) NOT NULL DEFAULT '0',
  `customer_title` varchar(400) NOT NULL,
  `user_name` varchar(100) NOT NULL,
  `password` varchar(100) NOT NULL,
  `token` text,
  `user_id` varchar(38) NOT NULL,
  `folder_type` int(11) NOT NULL DEFAULT '0',
  `create_on` datetime NOT NULL,
  `url` text,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_thirdparty_app` (
  `user_id` varchar(38) NOT NULL,
  `app` varchar(50) NOT NULL,
  `token` text,
  `tenant_id` int(11) NOT NULL,
  `modified_on` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`,`app`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `files_thirdparty_id_mapping` (
  `hash_id` char(32) NOT NULL,
  `id` text NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`hash_id`),
  KEY `index_1` (`tenant_id`,`hash_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_answer` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `question_id` int(11) NOT NULL,
  `create_date` datetime DEFAULT NULL,
  `user_id` char(38) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_answer_variant` (
  `answer_id` int(11) NOT NULL,
  `variant_id` int(11) NOT NULL,
  PRIMARY KEY (`answer_id`,`variant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_attachment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(500) NOT NULL,
  `post_id` int(11) NOT NULL,
  `size` int(11) NOT NULL DEFAULT '0',
  `download_count` int(11) NOT NULL DEFAULT '0',
  `content_type` int(11) NOT NULL DEFAULT '0',
  `mime_content_type` varchar(100) DEFAULT NULL,
  `create_date` datetime DEFAULT NULL,
  `path` varchar(1000) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `post_id` (`TenantID`,`post_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(500) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `create_date` datetime NOT NULL,
  `poster_id` char(38) NOT NULL,
  `tenantid` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `TenantID` (`tenantid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_lastvisit` (
  `tenantid` int(11) NOT NULL,
  `user_id` char(38) NOT NULL,
  `thread_id` int(11) NOT NULL,
  `last_visit` datetime NOT NULL,
  PRIMARY KEY (`tenantid`,`user_id`,`thread_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_post` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `topic_id` int(11) NOT NULL,
  `poster_id` char(38) NOT NULL,
  `create_date` datetime NOT NULL,
  `subject` varchar(500) NOT NULL DEFAULT '',
  `text` mediumtext NOT NULL,
  `edit_date` datetime DEFAULT NULL,
  `edit_count` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `parent_post_id` int(11) NOT NULL DEFAULT '0',
  `formatter` int(11) NOT NULL DEFAULT '0',
  `editor_id` char(38) DEFAULT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `topic_id` (`TenantID`,`topic_id`),
  KEY `create_date` (`create_date`),
  KEY `LastModified` (`LastModified`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_question` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `topic_id` int(11) NOT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  `name` varchar(500) NOT NULL,
  `create_date` datetime NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `topic_id` (`TenantID`,`topic_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_thread` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(500) NOT NULL,
  `description` varchar(500) NOT NULL DEFAULT '',
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `category_id` int(11) NOT NULL,
  `topic_count` int(11) NOT NULL DEFAULT '0',
  `post_count` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `recent_post_id` int(11) NOT NULL DEFAULT '0',
  `recent_topic_id` int(11) NOT NULL DEFAULT '0',
  `recent_post_date` datetime DEFAULT NULL,
  `recent_poster_id` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenantid` (`TenantID`,`category_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_topic` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `thread_id` int(11) NOT NULL,
  `title` varchar(500) NOT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  `create_date` datetime NOT NULL,
  `view_count` int(11) NOT NULL DEFAULT '0',
  `post_count` int(11) NOT NULL DEFAULT '0',
  `recent_post_id` int(11) NOT NULL DEFAULT '0',
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `poster_id` char(38) DEFAULT NULL,
  `sticky` int(11) NOT NULL DEFAULT '0',
  `closed` int(11) DEFAULT '0',
  `question_id` varchar(45) DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `thread_id` (`thread_id`),
  KEY `LastModified` (`LastModified`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_topicwatch` (
  `TenantID` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `TopicID` int(11) NOT NULL,
  `ThreadID` int(11) NOT NULL,
  PRIMARY KEY (`TenantID`,`UserID`,`TopicID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_topic_tag` (
  `topic_id` int(11) NOT NULL,
  `tag_id` int(11) NOT NULL,
  PRIMARY KEY (`topic_id`,`tag_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `forum_variant` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `question_id` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `install_registration` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `timestamp` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `email` varchar(500) NOT NULL,
  `version` varchar(500) NOT NULL,
  `ip` varchar(50) NOT NULL,
  `tenant` varchar(36) DEFAULT NULL,
  `alias` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_archive` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jid` varchar(255) NOT NULL,
  `stamp` datetime NOT NULL,
  `message` mediumtext,
  PRIMARY KEY (`id`),
  KEY `jabber_archive_jid` (`jid`(190))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `jabber_archive_switch` (
  `id` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_clear` (
  `lastdate` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_offactivity` (
  `jid` varchar(255) NOT NULL,
  `logout` datetime DEFAULT NULL,
  `status` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_offmessage` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jid` varchar(255) NOT NULL,
  `message` mediumtext,
  PRIMARY KEY (`id`),
  KEY `jabber_offmessage_jid` (`jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_offpresence` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jid_to` varchar(255) NOT NULL,
  `jid_from` varchar(255) DEFAULT NULL,
  `type` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `jabber_offpresence_to` (`jid_to`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_private` (
  `jid` varchar(255) NOT NULL,
  `tag` varchar(255) NOT NULL,
  `namespace` varchar(255) NOT NULL,
  `element` mediumtext,
  PRIMARY KEY (`jid`,`tag`,`namespace`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_room` (
  `jid` varchar(255) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `subject` varchar(255) DEFAULT NULL,
  `instructions` varchar(255) DEFAULT NULL,
  `pwd` varchar(255) DEFAULT NULL,
  `pwdprotect` int(11) DEFAULT NULL,
  `visible` int(11) DEFAULT NULL,
  `members` text,
  `maxoccupant` int(11) DEFAULT NULL,
  `historycountonenter` int(11) DEFAULT NULL,
  `anonymous` int(11) DEFAULT NULL,
  `logging` int(11) DEFAULT NULL,
  `membersonly` int(11) DEFAULT NULL,
  `usernamesonly` int(11) DEFAULT NULL,
  `moderated` int(11) DEFAULT NULL,
  `persistent` int(11) DEFAULT NULL,
  `presencebroadcastedfrom` int(11) DEFAULT NULL,
  `canchangesubject` int(11) DEFAULT NULL,
  `caninvite` int(11) DEFAULT NULL,
  `canseememberlist` int(11) DEFAULT NULL,
  PRIMARY KEY (`jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_room_history` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jid` varchar(255) NOT NULL,
  `stamp` datetime NOT NULL,
  `message` mediumtext NOT NULL,
  PRIMARY KEY (`id`),
  KEY `jabber_room_history_jid` (`jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_roster` (
  `jid` varchar(255) NOT NULL,
  `item_jid` varchar(255) NOT NULL,
  `name` varchar(512) DEFAULT NULL,
  `subscription` int(11) NOT NULL DEFAULT '0',
  `ask` int(11) NOT NULL DEFAULT '0',
  `groups` text,
  PRIMARY KEY (`jid`,`item_jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `jabber_vcard` (
  `jid` varchar(255) NOT NULL,
  `vcard` text NOT NULL,
  PRIMARY KEY (`jid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `login_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `ip` varchar(50) DEFAULT NULL,
  `login` varchar(200) DEFAULT NULL,
  `browser` varchar(200) DEFAULT NULL,
  `platform` varchar(200) DEFAULT NULL,
  `date` datetime NOT NULL,
  `tenant_id` int(10) NOT NULL,
  `user_id` char(38) NOT NULL,
  `page` varchar(300) DEFAULT NULL,
  `action` int(11) DEFAULT NULL,
  `description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `date` (`date`),
  KEY `tenant_id` (`tenant_id`,`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_alerts` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `id_mailbox` int(11) NOT NULL DEFAULT '-1',
  `type` int(11) NOT NULL DEFAULT '0',
  `data` mediumtext,
  PRIMARY KEY (`id`),
  KEY `tenant_id_user_id_mailbox_type` (`tenant`,`id_user`,`id_mailbox`,`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

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
  `id_mailbox` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant` (`tenant`,`id_mail`),
  KEY `id_mail` (`id_mail`,`content_id`),
  KEY `id_mailbox` (`id_mailbox`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

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

CREATE TABLE IF NOT EXISTS `mail_chain_x_crm_entity` (
  `id_tenant` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `id_chain` varchar(255) NOT NULL,
  `entity_id` int(11) NOT NULL,
  `entity_type` int(11) NOT NULL,
  PRIMARY KEY (`id_tenant`,`id_mailbox`,`id_chain`,`entity_id`,`entity_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_contacts` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `id_user` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `address` varchar(255) NOT NULL,
  `description` varchar(100) DEFAULT NULL,
  `type` int(11) NOT NULL,
  `has_photo` tinyint(1) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `tenant_id_user_name_address` (`tenant`,`id_user`,`address`),
  KEY `last_modified` (`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_contact_info` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `id_contact` int(11) unsigned NOT NULL,
  `data` varchar(255) NOT NULL,
  `type` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `last_modified` (`last_modified`),
  KEY `contact_id` (`id_contact`),
  KEY `tenant_id_user_data` (`tenant`,`id_user`,`data`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_display_images` (
  `tenant` int(10) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  PRIMARY KEY (`tenant`,`id_user`,`address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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

CREATE TABLE IF NOT EXISTS `mail_imap_flags` (
  `name` varchar(50) NOT NULL,
  `folder_id` int(11) NOT NULL,
  `skip` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_imap_special_mailbox` (
  `server` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `folder_id` int(11) NOT NULL,
  `skip` int(11) NOT NULL,
  PRIMARY KEY (`server`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  `has_parse_error` tinyint(1) NOT NULL DEFAULT '0',
  `calendar_uid` varchar(255) DEFAULT NULL,
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
  KEY `uidl` (`uidl`,`id_mailbox`),
  KEY `mime_message_id` (`id_mailbox`,`mime_message_id`),
  KEY `md5` (`md5`,`id_mailbox`),
  KEY `list_conversations` (`tenant`, `id_user`, `folder`, `chain_date`),
  KEY `list_messages` (`tenant`, `id_user`, `folder`, `date_sent`),
  KEY `time_modified` (`time_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

CREATE TABLE IF NOT EXISTS `mail_mailbox` (
  `id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  `tenant` INT(11) NOT NULL,
  `id_user` VARCHAR(38) NOT NULL,
  `address` VARCHAR(255) NOT NULL,
  `name` VARCHAR(255) NULL DEFAULT NULL,
  `enabled` TINYINT(1) UNSIGNED NOT NULL DEFAULT '1',
  `is_removed` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `is_processed` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `is_server_mailbox` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `imap` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `user_online` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `is_default` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0',
  `msg_count_last` INT(11) NOT NULL DEFAULT '0',
  `size_last` INT(11) NOT NULL DEFAULT '0',
  `login_delay` INT(11) UNSIGNED NOT NULL DEFAULT '30',
  `quota_error` TINYINT(1) NOT NULL DEFAULT '0',
  `imap_intervals` MEDIUMTEXT NULL,
  `begin_date` TIMESTAMP NOT NULL DEFAULT '1975-01-01 00:00:00',
  `email_in_folder` TEXT NULL,
  `pop3_password` VARCHAR(255) NULL DEFAULT NULL,
  `smtp_password` VARCHAR(255) NULL DEFAULT NULL,
  `token_type` TINYINT(4) NOT NULL DEFAULT '0',
  `token` TEXT NULL,
  `id_smtp_server` INT(11) NOT NULL,
  `id_in_server` INT(11) NOT NULL,
  `date_checked` DATETIME NULL DEFAULT NULL,
  `date_user_checked` DATETIME NULL DEFAULT NULL,
  `date_login_delay_expires` DATETIME NOT NULL DEFAULT '1975-01-01 00:00:00',
  `date_auth_error` DATETIME NULL DEFAULT NULL,
  `date_created` DATETIME NULL DEFAULT NULL,
  `date_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `address_index` (`address`),
  INDEX `main_mailbox_id_smtp_server_mail_mailbox_server_id` (`id_smtp_server`),
  INDEX `main_mailbox_id_in_server_mail_mailbox_server_id` (`id_in_server`),
  INDEX `date_login_delay_expires` (`date_checked`, `date_login_delay_expires`),
  INDEX `user_id_index` (`tenant`, `id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_mailbox_autoreply` (
  `id_mailbox` int(11) NOT NULL,
  `tenant` int(11) NOT NULL,
  `turn_on` tinyint(1) NOT NULL,
  `only_contacts` tinyint(1) NOT NULL,
  `turn_on_to_date` tinyint(1) NOT NULL,
  `from_date` datetime NOT NULL,
  `to_date` datetime NOT NULL,
  `subject` text,
  `html` text,
  PRIMARY KEY (`id_mailbox`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_mailbox_autoreply_history` (
  `id_mailbox` int(11) NOT NULL,
  `tenant` int(11) NOT NULL,
  `sending_email` varchar(255) NOT NULL,
  `sending_date` datetime NOT NULL,
  PRIMARY KEY (`id_mailbox`,`sending_email`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_mailbox_domain` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_provider` int(11) NOT NULL DEFAULT '0',
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_provider` (`name`,`id_provider`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

CREATE TABLE IF NOT EXISTS `mail_mailbox_provider` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `display_name` varchar(255) DEFAULT NULL,
  `display_short_name` varchar(255) DEFAULT NULL,
  `documentation` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

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

CREATE TABLE IF NOT EXISTS `mail_mailbox_signature` (
  `tenant` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `html` text,
  `is_active` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id_mailbox`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_pop_unordered_domain` (
  `server` varchar(255) NOT NULL,
  PRIMARY KEY (`server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_address` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `id_domain` int(11) NOT NULL,
  `id_mailbox` int(11) NOT NULL,
  `is_mail_group` int(10) NOT NULL DEFAULT '0',
  `is_alias` int(10) NOT NULL DEFAULT '0',
  `date_created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_mailbox_fk_index` (`id_mailbox`),
  KEY `domain_index` (`id_domain`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_dns` (
  `id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  `tenant` INT(11) NOT NULL,
  `id_user` VARCHAR(255) NOT NULL,
  `id_domain` INT(11) NOT NULL DEFAULT '-1',
  `dkim_selector` VARCHAR(63) NOT NULL DEFAULT 'dkim',
  `dkim_private_key` TEXT NULL,
  `dkim_public_key` TEXT NULL,
  `dkim_ttl` INT(11) NOT NULL DEFAULT '300',
  `dkim_verified` TINYINT(1) NOT NULL DEFAULT '0',
  `dkim_date_checked` DATETIME NULL DEFAULT NULL,
  `domain_check` TEXT NULL,
  `spf` TEXT NULL,
  `spf_ttl` INT(11) NOT NULL DEFAULT '300',
  `spf_verified` TINYINT(1) NOT NULL DEFAULT '0',
  `spf_date_checked` DATETIME NULL DEFAULT NULL,
  `mx` VARCHAR(255) NULL DEFAULT NULL,
  `mx_ttl` INT(11) NOT NULL DEFAULT '300',
  `mx_verified` TINYINT(1) NOT NULL DEFAULT '0',
  `mx_date_checked` DATETIME NULL DEFAULT NULL,
  `time_modified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `id_domain_tenant_id_user` (`id_domain`, `tenant`, `id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_domain` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `is_verified` int(10) NOT NULL DEFAULT '0',
  `date_added` datetime NOT NULL,
  `date_checked` datetime NOT NULL DEFAULT '1975-01-01 00:00:00',
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`),
  KEY `tenant` (`tenant`),
  KEY `date_checked` (`date_checked`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_mail_group` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_tenant` int(11) NOT NULL,
  `id_address` int(11) NOT NULL,
  `date_created` datetime NOT NULL,
  `address` varchar(320) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `mail_server_address_fk_id` (`id_address`),
  KEY `tenant` (`id_tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_mail_group_x_mail_server_address` (
  `id_address` int(11) NOT NULL,
  `id_mail_group` int(11) NOT NULL,
  PRIMARY KEY (`id_address`,`id_mail_group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `mx_record` varchar(128) NOT NULL DEFAULT '',
  `connection_string` text NOT NULL,
  `server_type` int(11) NOT NULL,
  `smtp_settings_id` int(11) NOT NULL,
  `imap_settings_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `mail_server_server_type_server_type_fk_id` (`server_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server_type` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_server_server_x_tenant` (
  `id_tenant` int(11) NOT NULL,
  `id_server` int(11) NOT NULL,
  PRIMARY KEY (`id_tenant`,`id_server`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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

CREATE TABLE IF NOT EXISTS `mail_tag_addresses` (
  `id_tag` int(11) unsigned NOT NULL,
  `address` varchar(255) NOT NULL,
  `tenant` int(11) NOT NULL,
  PRIMARY KEY (`id_tag`,`address`,`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `mail_tag_mail` (
  `tenant` int(11) NOT NULL,
  `id_user` varchar(255) NOT NULL,
  `id_mail` int(11) NOT NULL,
  `id_tag` int(11) NOT NULL,
  `time_created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`id_user`,`id_mail`,`id_tag`),
  KEY `id_mail` (`id_mail`),
  KEY `id_tag` (`id_tag`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
)ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8;

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

CREATE TABLE IF NOT EXISTS `mobile_app_install` (
  `user_email` varchar(255) NOT NULL,
  `app_type` int(11) NOT NULL,
  `registered_on` datetime NOT NULL,
  `last_sign` datetime DEFAULT NULL,
  PRIMARY KEY (`user_email`,`app_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `notify_info` (
  `notify_id` int(10) NOT NULL,
  `state` int(10) NOT NULL DEFAULT '0',
  `attempts` int(10) NOT NULL DEFAULT '0',
  `modify_date` datetime NOT NULL,
  `priority` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`notify_id`),
  KEY `state` (`state`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `notify_promotion_watch` (
  `id` varchar(50) NOT NULL,
  `user_id` varchar(50) NOT NULL,
  `session_id` varchar(25) NOT NULL DEFAULT '',
  `viewcount` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`user_id`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  `attachments` text NULL,
  `auto_submitted` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`notify_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `notify_tip_watch` (
  `tip_id` varchar(50) NOT NULL,
  `user_id` varchar(38) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`tip_id`,`user_id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_comments` (
  `comment_id` int(11) NOT NULL AUTO_INCREMENT,
  `id` char(38) NOT NULL,
  `content` text,
  `inactive` tinyint(1) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `parent_id` char(38) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `target_uniq_id` varchar(50) NOT NULL,
  PRIMARY KEY (`comment_id`,`id`),
  KEY `target_uniq_id` (`tenant_id`,`target_uniq_id`),
  KEY `create_on` (`create_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_following_project_participant` (
  `project_id` int(11) NOT NULL,
  `participant_id` char(38) NOT NULL,
  PRIMARY KEY (`participant_id`,`project_id`),
  KEY `project_id` (`project_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_messages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `status` int(11) NOT NULL DEFAULT '0',
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `content` mediumtext,
  `project_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `project_id` (`project_id`),
  KEY `create_on` (`create_on`),
  KEY `last_modified_on` (`last_modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_milestones` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `deadline` datetime NOT NULL,
  `responsible_id` char(38) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `status_changed` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  `project_id` int(11) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `is_notify` tinyint(1) NOT NULL DEFAULT '0',
  `is_key` tinyint(1) DEFAULT '0',
  `create_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `project_id` (`project_id`),
  KEY `create_on` (`create_on`),
  KEY `last_modified_on` (`last_modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_projects` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `status` int(11) NOT NULL,
  `status_changed` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `responsible_id` char(38) NOT NULL,
  `tenant_id` int(11) NOT NULL,
  `private` int(10) NOT NULL DEFAULT '0',
  `create_on` datetime DEFAULT NULL,
  `create_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `responsible_id` (`responsible_id`),
  KEY `tenant_id` (`tenant_id`),
  KEY `create_on` (`create_on`),
  KEY `last_modified_on` (`last_modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_project_participant` (
  `project_id` int(11) NOT NULL,
  `participant_id` char(38) NOT NULL,
  `security` int(10) NOT NULL DEFAULT '0',
  `created` timestamp NOT NULL DEFAULT '2000-01-01 00:00:00',
  `updated` timestamp NOT NULL DEFAULT '2000-01-01 00:00:00',
  `removed` int(10) NOT NULL DEFAULT '0',
  `tenant` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant`,`project_id`,`participant_id`),
  KEY `participant_id` (`participant_id`),
  KEY `created` (`created`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_project_tag` (
  `tag_id` int(11) NOT NULL,
  `project_id` int(11) NOT NULL,
  PRIMARY KEY (`project_id`,`tag_id`),
  KEY `tag_id` (`tag_id`)
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

CREATE TABLE IF NOT EXISTS `projects_report_template` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `type` int(11) NOT NULL,
  `name` varchar(1024) NOT NULL,
  `filter` text,
  `cron` varchar(255) DEFAULT NULL,
  `create_on` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `create_by` varchar(38) DEFAULT NULL,
  `tenant_id` int(10) NOT NULL,
  `auto` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_subtasks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` varchar(255) NOT NULL,
  `responsible_id` char(38) NOT NULL,
  `task_id` int(11) NOT NULL,
  `status` int(11) NOT NULL,
  `status_changed` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  `tenant_id` int(11) NOT NULL,
  `create_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `responsible_id` (`responsible_id`),
  KEY `task_id` (`tenant_id`,`task_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  IF NOT EXISTS `projects_status` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(255) NOT NULL,
	`description` VARCHAR(255) NOT NULL,
	`statusType` TINYINT(2) NOT NULL,
	`image` TEXT NOT NULL,
	`imageType` VARCHAR(50) NOT NULL,
	`color` CHAR(7) NOT NULL,
	`order` TINYINT(3) UNSIGNED NOT NULL,
	`isDefault` TINYINT(1) NOT NULL,
	`available` TINYINT(1) NOT NULL,
	`tenant_id` INT(11) NOT NULL,
	PRIMARY KEY (`id`),
	INDEX `tenant` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE IF NOT EXISTS `projects_tags` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `tenant_id` int(11) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `create_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_tasks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `responsible_id` char(38) DEFAULT '00000000-0000-0000-0000-000000000000',
  `priority` int(11) NOT NULL,
  `status` int(11) NOT NULL,
  `status_id` SMALLINT(6) NULL DEFAULT NULL,
  `status_changed` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  `project_id` int(11) NOT NULL,
  `milestone_id` int(11) DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  `deadline` datetime DEFAULT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `start_date` datetime DEFAULT NULL,
  `progress` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `responsible_id` (`responsible_id`),
  KEY `project_id` (`project_id`),
  KEY `deadline` (`deadline`),
  KEY `create_on` (`create_on`),
  KEY `milestone_id` (`tenant_id`,`milestone_id`),
  KEY `tenant_id` (`tenant_id`,`project_id`),
  KEY `last_modified_on` (`last_modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_tasks_links` (
  `tenant_id` int(10) NOT NULL DEFAULT '0',
  `task_id` int(10) NOT NULL DEFAULT '0',
  `parent_id` int(10) NOT NULL DEFAULT '0',
  `link_type` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tenant_id`,`task_id`,`parent_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_tasks_order` (
  `tenant_id` int(10) NOT NULL,
  `project_id` int(10) NOT NULL,
  `task_order` text,
  PRIMARY KEY (`tenant_id`,`project_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_tasks_recurrence` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `task_id` int(11) NOT NULL,
  `cron` varchar(255) DEFAULT NULL,
  `start_date` datetime NOT NULL,
  `end_date` datetime NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `task_id` (`tenant_id`,`task_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_tasks_responsible` (
  `tenant_id` int(11) NOT NULL,
  `task_id` int(11) NOT NULL,
  `responsible_id` char(38) NOT NULL,
  PRIMARY KEY (`tenant_id`,`task_id`,`responsible_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_templates` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) DEFAULT NULL,
  `description` text,
  `create_by` char(38) NOT NULL,
  `last_modified_on` datetime DEFAULT NULL,
  `last_modified_by` char(38) DEFAULT NULL,
  `create_on` datetime DEFAULT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `projects_time_tracking` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `note` varchar(255) DEFAULT NULL,
  `date` datetime NOT NULL,
  `hours` float DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  `relative_task_id` int(11) DEFAULT NULL,
  `person_id` char(38) NOT NULL,
  `project_id` int(11) NOT NULL,
  `create_on` datetime DEFAULT NULL,
  `create_by` char(38) DEFAULT NULL,
  `payment_status` int(10) NOT NULL DEFAULT '0',
  `status_changed` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  PRIMARY KEY (`id`),
  KEY `person_id` (`person_id`),
  KEY `project_id` (`project_id`),
  KEY `relative_task_id` (`tenant_id`,`relative_task_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `res_authors` (
  `login` varchar(150) NOT NULL,
  `password` varchar(50) NOT NULL,
  `isAdmin` tinyint(1) NOT NULL DEFAULT '0',
  `online` int(10) NOT NULL DEFAULT '0',
  `lastVisit` datetime DEFAULT NULL,
  PRIMARY KEY (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AVG_ROW_LENGTH=4096;

CREATE TABLE IF NOT EXISTS `res_authorsfile` (
  `authorLogin` varchar(50) NOT NULL,
  `fileid` int(11) NOT NULL,
  `writeAccess` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`authorLogin`,`fileid`),
  KEY `res_authorsfile_FK2` (`fileid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `res_authorslang` (
  `authorLogin` varchar(50) NOT NULL,
  `cultureTitle` varchar(20) NOT NULL,
  PRIMARY KEY (`authorLogin`,`cultureTitle`),
  KEY `res_authorslang_FK2` (`cultureTitle`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AVG_ROW_LENGTH=1170;

CREATE TABLE IF NOT EXISTS `res_cultures` (
  `title` varchar(120) NOT NULL,
  `value` varchar(120) NOT NULL,
  `available` tinyint(1) NOT NULL DEFAULT '0',
  `creationDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`title`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `res_data` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `fileid` int(11) NOT NULL,
  `title` varchar(120) NOT NULL,
  `cultureTitle` varchar(20) NOT NULL,
  `textValue` text,
  `description` text,
  `timeChanges` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `resourceType` varchar(20) DEFAULT NULL,
  `flag` int(11) NOT NULL DEFAULT '0',
  `link` varchar(120) DEFAULT NULL,
  `authorLogin` varchar(50) NOT NULL DEFAULT 'Console',
  PRIMARY KEY (`fileid`,`cultureTitle`,`title`),
  UNIQUE KEY `id` (`id`),
  KEY `dateIndex` (`timeChanges`),
  KEY `resources_FK2` (`cultureTitle`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AVG_ROW_LENGTH=140;

CREATE TABLE IF NOT EXISTS `res_files` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `projectName` varchar(50) NOT NULL,
  `moduleName` varchar(50) NOT NULL,
  `resName` varchar(50) NOT NULL,
  `isLock` tinyint(1) NOT NULL DEFAULT '0',
  `lastUpdate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `creationDate` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  PRIMARY KEY (`id`),
  UNIQUE KEY `resname` (`resName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AVG_ROW_LENGTH=16384;

CREATE TABLE IF NOT EXISTS `res_reserve` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `fileid` int(11) NOT NULL,
  `title` varchar(120) NOT NULL,
  `cultureTitle` varchar(20) NOT NULL,
  `textValue` text,
  `flag` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`fileid`,`title`,`cultureTitle`),
  UNIQUE KEY `id` (`id`),
  KEY `resources_FK2` (`cultureTitle`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AVG_ROW_LENGTH=250;

CREATE TABLE IF NOT EXISTS `sso_links` (
  `id` varchar(200) NOT NULL,
  `uid` varchar(200) NOT NULL,
  `profile` varchar(200) NOT NULL,
  PRIMARY KEY (`id`,`uid`,`profile`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `sso_tokens` (
  `tokenType` varchar(50) NOT NULL,
  `tenant` int(11) NOT NULL,
  `tokenId` varchar(100) NOT NULL,
  `expirationDate` datetime NOT NULL,
  PRIMARY KEY (`tokenId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tenants_buttons` (
  `tariff_id` int(10) NOT NULL,
  `partner_id` varchar(50) NOT NULL,
  `button_url` text NOT NULL,
  PRIMARY KEY (`tariff_id`,`partner_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tenants_forbiden` (
  `address` varchar(50) NOT NULL,
  PRIMARY KEY (`address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tenants_iprestrictions` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `tenant` int(10) NOT NULL,
  `ip` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tenants_partners` (
  `tenant_id` int(10) NOT NULL,
  `partner_id` varchar(36) DEFAULT NULL,
  `affiliate_id` varchar(50) DEFAULT NULL,
  `campaign` VARCHAR(50) NULL DEFAULT NULL,
  PRIMARY KEY (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  PRIMARY KEY (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

CREATE TABLE IF NOT EXISTS `tenants_quotarow` (
  `tenant` int(11) NOT NULL,
  `path` varchar(255) NOT NULL,
  `counter` bigint(20) NOT NULL DEFAULT '0',
  `tag` varchar(1024) DEFAULT NULL,
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant`,`path`),
  KEY `last_modified` (`last_modified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
  `industry` int(11) NOT NULL DEFAULT '0',
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `spam` INT(10) NOT NULL DEFAULT '1',
  `calls` INT(10) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `alias` (`alias`),
  KEY `last_modified` (`last_modified`),
  KEY `mappeddomain` (`mappeddomain`),
  KEY `version` (`version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tenants_version` (
  `id` int(10) NOT NULL,
  `version` varchar(64) NOT NULL,
  `url` varchar(64) NOT NULL,
  `default_version` int(11) NOT NULL DEFAULT '0',
  `visible` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `webstudio_fckuploads` (
  `TenantID` int(11) NOT NULL,
  `StoreDomain` varchar(50) NOT NULL,
  `FolderID` varchar(100) NOT NULL,
  `ItemID` varchar(100) NOT NULL,
  PRIMARY KEY (`TenantID`,`StoreDomain`,`FolderID`,`ItemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `webstudio_index` (
  `index_name` varchar(50) NOT NULL,
  `last_modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`index_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `webstudio_settings` (
  `TenantID` int(11) NOT NULL,
  `ID` varchar(64) NOT NULL,
  `UserID` varchar(64) NOT NULL,
  `Data` mediumtext NOT NULL,
  PRIMARY KEY (`TenantID`,`ID`,`UserID`),
  KEY `ID` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `webstudio_uservisit` (
  `tenantid` int(11) NOT NULL,
  `visitdate` datetime NOT NULL,
  `productid` varchar(38) NOT NULL,
  `userid` varchar(38) NOT NULL,
  `visitcount` int(11) NOT NULL DEFAULT '0',
  `firstvisittime` datetime DEFAULT NULL,
  `lastvisittime` datetime DEFAULT NULL,
  PRIMARY KEY (`tenantid`,`visitdate`,`productid`,`userid`),
  KEY `visitdate` (`visitdate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `wiki_categories` (
  `Tenant` int(11) NOT NULL,
  `CategoryName` varchar(255) NOT NULL,
  `PageName` varchar(255) NOT NULL,
  PRIMARY KEY (`Tenant`,`CategoryName`,`PageName`),
  KEY `PageName` (`Tenant`,`PageName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `wiki_comments` (
  `Tenant` int(11) NOT NULL,
  `Id` char(38) NOT NULL,
  `ParentId` char(38) NOT NULL,
  `PageName` varchar(255) NOT NULL,
  `Body` text NOT NULL,
  `UserId` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `Inactive` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`Id`),
  KEY `PageName` (`Tenant`,`PageName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `wiki_files` (
  `Tenant` int(11) NOT NULL,
  `FileName` varchar(255) NOT NULL,
  `Version` int(11) NOT NULL,
  `UploadFileName` text NOT NULL,
  `UserID` char(38) NOT NULL,
  `Date` datetime NOT NULL,
  `FileLocation` text NOT NULL,
  `FileSize` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`FileName`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `wiki_pages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `pagename` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  PRIMARY KEY (`id`,`tenant`,`pagename`),
  KEY `modified_on` (`modified_on`),
  KEY `tenant` (`tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `wiki_pages_history` (
  `tenant` int(11) NOT NULL,
  `pagename` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `body` mediumtext,
  PRIMARY KEY (`tenant`,`pagename`,`version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `short_links` (
  `id` INT(21) NOT NULL AUTO_INCREMENT,
  `short` VARCHAR(12) COLLATE utf8_bin NULL DEFAULT NULL,
  `link` TEXT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `UNIQUE` (`short`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `telegram_users` (
  `portal_user_id` VARCHAR(38) NOT NULL,
  `tenant_id` INT(11) NOT NULL,
  `telegram_user_id` INT(11) NOT NULL,
  PRIMARY KEY (`portal_user_id`, `tenant_id`),
  INDEX `tgId` (`telegram_user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
