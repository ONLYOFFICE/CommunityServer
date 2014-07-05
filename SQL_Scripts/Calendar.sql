-- calendar_calendar_item
CREATE TABLE IF NOT EXISTS `calendar_calendar_item` (
  `calendar_id` int(10) NOT NULL,
  `item_id` char(38) NOT NULL,
  `is_group` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`calendar_id`,`item_id`,`is_group`)
);
-- calendar_calendar_user
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
);
-- calendar_calendars
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
  PRIMARY KEY (`id`),
  KEY `owner_id` (`tenant`,`owner_id`)
);
-- calendar_event_item
CREATE TABLE IF NOT EXISTS `calendar_event_item` (
  `event_id` int(10) NOT NULL,
  `item_id` char(38) NOT NULL,
  `is_group` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`event_id`,`item_id`,`is_group`)
);
-- calendar_event_user
CREATE TABLE IF NOT EXISTS `calendar_event_user` (
  `event_id` int(10) NOT NULL,
  `user_id` char(38) NOT NULL,
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `is_unsubscribe` smallint(2) NOT NULL DEFAULT '0',
  PRIMARY KEY (`event_id`,`user_id`)
);
-- calendar_events
CREATE TABLE IF NOT EXISTS `calendar_events` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `tenant` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `calendar_id` int(11) NOT NULL,
  `start_date` datetime NOT NULL,
  `end_date` datetime NOT NULL,
  `all_day_long` smallint(6) NOT NULL DEFAULT '0',
  `repeat_type` smallint(6) NOT NULL DEFAULT '0',
  `owner_id` char(38) NOT NULL,
  `alert_type` smallint(6) NOT NULL DEFAULT '0',
  `rrule` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `calendar_id` (`tenant`,`calendar_id`)
);
-- calendar_notifications
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
);

