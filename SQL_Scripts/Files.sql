-- files_bunch_objects
CREATE TABLE IF NOT EXISTS `files_bunch_objects` (
  `tenant_id` int(10) NOT NULL,
  `right_node` varchar(255) NOT NULL,
  `left_node` varchar(255) NOT NULL,
  PRIMARY KEY (`tenant_id`,`right_node`),
  KEY `left_node` (`left_node`)
);
-- files_converts
CREATE TABLE IF NOT EXISTS `files_converts` (
  `input` varchar(50) NOT NULL,
  `output` varchar(50) NOT NULL,
  PRIMARY KEY (`input`,`output`)
);
-- files_file
CREATE TABLE IF NOT EXISTS `files_file` (
  `id` int(11) NOT NULL,
  `version` int(11) NOT NULL,
  `version_group` int(11) NOT NULL DEFAULT '1',
  `current_version` int(11) NOT NULL DEFAULT '0',
  `folder_id` int(11) NOT NULL DEFAULT '0',
  `title` varchar(400) NOT NULL,
  `content_type` varchar(255) DEFAULT NULL,
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
  PRIMARY KEY (`tenant_id`,`id`,`version`),
  KEY `folder_id` (`folder_id`),
  KEY `id` (`id`),
  KEY `modified_on` (`modified_on`)
);
-- files_folder
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
);
-- files_folder_tree
CREATE TABLE IF NOT EXISTS `files_folder_tree` (
  `folder_id` int(11) NOT NULL,
  `parent_id` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  PRIMARY KEY (`parent_id`,`folder_id`),
  KEY `folder_id` (`folder_id`)
);
-- files_security
CREATE TABLE IF NOT EXISTS `files_security` (
  `tenant_id` int(10) NOT NULL,
  `entry_id` varchar(32) NOT NULL,
  `entry_type` int(10) NOT NULL,
  `subject` char(38) NOT NULL,
  `owner` char(38) NOT NULL,
  `security` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`tenant_id`,`entry_id`,`entry_type`,`subject`)
);
-- files_tag
CREATE TABLE IF NOT EXISTS `files_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `owner` varchar(38) NOT NULL,
  `flag` int(11) NOT NULL DEFAULT '0',
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `name` (`tenant_id`,`owner`,`name`,`flag`)
);
-- files_tag_link
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
);
-- files_thirdparty_account
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
);
-- files_thirdparty_app
CREATE TABLE IF NOT EXISTS `files_thirdparty_app` (
  `user_id` varchar(38) NOT NULL,
  `token` text,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`user_id`)
);
-- files_thirdparty_id_mapping
CREATE TABLE IF NOT EXISTS `files_thirdparty_id_mapping` (
  `hash_id` char(32) NOT NULL,
  `id` text NOT NULL,
  `tenant_id` int(11) NOT NULL,
  PRIMARY KEY (`hash_id`),
  KEY `index_1` (`tenant_id`,`hash_id`)
);

