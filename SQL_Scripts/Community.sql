-- blogs_comments
CREATE TABLE IF NOT EXISTS `blogs_comments` (
  `Tenant` int(11) NOT NULL,
  `id` char(38) NOT NULL,
  `post_id` char(38) NOT NULL,
  `content` text,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `parent_id` char(38) DEFAULT NULL,
  `inactive` int(11) DEFAULT NULL,
  PRIMARY KEY (`Tenant`,`id`),
  KEY `ixComments_PostId` (`Tenant`,`post_id`),
  KEY `ixComments_Created` (`created_when`,`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- blogs_posts
CREATE TABLE IF NOT EXISTS `blogs_posts` (
  `id` char(38) NOT NULL,
  `title` varchar(255) NOT NULL,
  `content` mediumtext NOT NULL,
  `created_by` char(38) NOT NULL,
  `created_when` datetime NOT NULL,
  `blog_id` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  `LastCommentId` char(38) DEFAULT NULL,
  `LastModified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Tenant`,`id`),
  KEY `ixPosts_CreatedBy` (`Tenant`,`created_by`),
  KEY `ixPosts_LastCommentId` (`Tenant`,`LastCommentId`),
  KEY `ixPosts_CreatedWhen` (`created_when`,`Tenant`),
  KEY `LastModified` (`LastModified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- blogs_reviewposts
CREATE TABLE IF NOT EXISTS `blogs_reviewposts` (
  `post_id` char(38) NOT NULL,
  `reviewed_by` char(38) NOT NULL,
  `timestamp` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Tenant`,`post_id`,`reviewed_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- blogs_tags
CREATE TABLE IF NOT EXISTS `blogs_tags` (
  `post_id` varchar(38) NOT NULL,
  `name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`Tenant`,`post_id`,`name`),
  KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- bookmarking_bookmark
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
-- bookmarking_bookmarktag
CREATE TABLE IF NOT EXISTS `bookmarking_bookmarktag` (
  `BookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`BookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- bookmarking_comment
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
-- bookmarking_tag
CREATE TABLE IF NOT EXISTS `bookmarking_tag` (
  `TagID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`TagID`),
  KEY `Name` (`Tenant`,`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- bookmarking_userbookmark
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
  KEY `LastModified` (`Tenant`,`LastModified`),
  KEY `BookmarkID` (`BookmarkID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- bookmarking_userbookmarktag
CREATE TABLE IF NOT EXISTS `bookmarking_userbookmarktag` (
  `UserBookmarkID` int(11) NOT NULL,
  `TagID` int(11) NOT NULL,
  `Tenant` int(11) NOT NULL,
  PRIMARY KEY (`UserBookmarkID`,`TagID`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- events_comment
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
-- events_feed
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
  KEY `LastModified` (`Tenant`,`LastModified`),
  KEY `Date` (`Date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- events_poll
CREATE TABLE IF NOT EXISTS `events_poll` (
  `Id` int(11) NOT NULL,
  `PollType` int(11) NOT NULL DEFAULT '0',
  `StartDate` datetime NOT NULL,
  `EndDate` datetime NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- events_pollanswer
CREATE TABLE IF NOT EXISTS `events_pollanswer` (
  `Variant` int(11) NOT NULL,
  `User` varchar(64) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Variant`,`User`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- events_pollvariant
CREATE TABLE IF NOT EXISTS `events_pollvariant` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Poll` int(11) NOT NULL,
  `Name` varchar(1024) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Poll` (`Tenant`,`Poll`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- events_reader
CREATE TABLE IF NOT EXISTS `events_reader` (
  `Feed` int(11) NOT NULL,
  `Reader` varchar(38) NOT NULL,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Feed`,`Reader`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_answer
CREATE TABLE IF NOT EXISTS `forum_answer` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `question_id` int(11) NOT NULL,
  `create_date` datetime DEFAULT NULL,
  `user_id` char(38) NOT NULL,
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_answer_variant
CREATE TABLE IF NOT EXISTS `forum_answer_variant` (
  `answer_id` int(11) NOT NULL,
  `variant_id` int(11) NOT NULL,
  PRIMARY KEY (`answer_id`,`variant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_attachment
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
-- forum_category
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
-- forum_lastvisit
CREATE TABLE IF NOT EXISTS `forum_lastvisit` (
  `tenantid` int(11) NOT NULL,
  `user_id` char(38) NOT NULL,
  `thread_id` int(11) NOT NULL,
  `last_visit` datetime NOT NULL,
  PRIMARY KEY (`tenantid`,`user_id`,`thread_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_post
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
  KEY `LastModified` (`TenantID`,`LastModified`),
  KEY `topic_id` (`TenantID`,`topic_id`),
  KEY `create_date` (`create_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_question
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
-- forum_tag
CREATE TABLE IF NOT EXISTS `forum_tag` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `is_approved` int(11) NOT NULL DEFAULT '0',
  `TenantID` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `TenantID` (`TenantID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_thread
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
-- forum_topic
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
  KEY `LastModified` (`TenantID`,`LastModified`),
  KEY `thread_id` (`thread_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_topic_tag
CREATE TABLE IF NOT EXISTS `forum_topic_tag` (
  `topic_id` int(11) NOT NULL,
  `tag_id` int(11) NOT NULL,
  PRIMARY KEY (`topic_id`,`tag_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_topicwatch
CREATE TABLE IF NOT EXISTS `forum_topicwatch` (
  `TenantID` int(11) NOT NULL,
  `UserID` char(38) NOT NULL,
  `TopicID` int(11) NOT NULL,
  `ThreadID` int(11) NOT NULL,
  PRIMARY KEY (`TenantID`,`UserID`,`TopicID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- forum_variant
CREATE TABLE IF NOT EXISTS `forum_variant` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `question_id` int(11) NOT NULL,
  `sort_order` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- photo_album
CREATE TABLE IF NOT EXISTS `photo_album` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Caption` varchar(255) DEFAULT NULL,
  `Event` int(11) NOT NULL,
  `User` varchar(38) DEFAULT NULL,
  `FaceImage` int(11) NOT NULL DEFAULT '0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ImagesCount` int(11) NOT NULL DEFAULT '0',
  `ViewsCount` int(11) NOT NULL DEFAULT '0',
  `CommentsCount` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Album_Index1` (`Tenant`,`Event`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- photo_comment
CREATE TABLE IF NOT EXISTS `photo_comment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Text` text NOT NULL,
  `User` varchar(38) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Image` int(11) NOT NULL,
  `Parent` int(11) NOT NULL DEFAULT '0',
  `Inactive` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Comment_Index1` (`Tenant`,`Image`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- photo_event
CREATE TABLE IF NOT EXISTS `photo_event` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `User` varchar(38) DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Tenant` (`Tenant`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- photo_image
CREATE TABLE IF NOT EXISTS `photo_image` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Album` int(11) NOT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `Location` varchar(1024) DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `User` varchar(38) DEFAULT NULL,
  `ThumbnailWidth` int(11) NOT NULL DEFAULT '0',
  `ThumbnailHeight` int(11) NOT NULL DEFAULT '0',
  `PreviewWidth` int(11) NOT NULL DEFAULT '0',
  `PreviewHeight` int(11) NOT NULL DEFAULT '0',
  `CommentsCount` int(11) NOT NULL DEFAULT '0',
  `ViewsCount` int(11) NOT NULL DEFAULT '0',
  `Tenant` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `Photo_Image_Index1` (`Tenant`,`Album`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- photo_imageview
CREATE TABLE IF NOT EXISTS `photo_imageview` (
  `Tenant` int(11) NOT NULL,
  `Image` int(11) NOT NULL,
  `User` varchar(38) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Tenant`,`Image`,`User`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- wiki_categories
CREATE TABLE IF NOT EXISTS `wiki_categories` (
  `Tenant` int(11) NOT NULL,
  `CategoryName` varchar(255) NOT NULL,
  `PageName` varchar(255) NOT NULL,
  PRIMARY KEY (`Tenant`,`CategoryName`,`PageName`),
  KEY `PageName` (`Tenant`,`PageName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- wiki_comments
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
-- wiki_files
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
-- wiki_pages
CREATE TABLE IF NOT EXISTS `wiki_pages` (
  `tenant` int(11) NOT NULL,
  `pagename` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `modified_by` char(38) NOT NULL,
  `modified_on` datetime NOT NULL,
  PRIMARY KEY (`tenant`,`pagename`),
  KEY `modified_on` (`tenant`,`modified_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- wiki_pages_history
CREATE TABLE IF NOT EXISTS `wiki_pages_history` (
  `tenant` int(11) NOT NULL,
  `pagename` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `create_by` char(38) NOT NULL,
  `create_on` datetime NOT NULL,
  `body` mediumtext,
  PRIMARY KEY (`tenant`,`pagename`,`version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

