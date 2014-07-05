DROP TABLE IF EXISTS `mail_imap_flags`;
CREATE TABLE IF NOT EXISTS `mail_imap_flags` (
  `name` varchar(50) NOT NULL,
  `folder_id` tinyint(3) unsigned NOT NULL,
  `skip` tinyint(1) unsigned NOT NULL DEFAULT '0',
  KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Imap mailbox flags with corresponding folder id + skip flag';

DELETE FROM `mail_imap_flags`;
INSERT INTO `mail_imap_flags` (`name`, `folder_id`, `skip`) VALUES
  ('inbox', 1, 0),
  ('sent', 2, 0),
  ('spam', 5, 0),
  ('junk', 5, 0),
  ('bulk', 5, 0),
  ('noselect', 0, 1),
  ('all', 0, 1),
  ('archive', 0, 1),
  ('drafts', 0, 1),
  ('flagged', 0, 1),
  ('trash', 0, 1),
  ('important', 0, 1),
  ('flagged', 0, 1);


DROP TABLE IF EXISTS `mail_imap_skip_mailbox`;
CREATE TABLE IF NOT EXISTS `mail_imap_skip_mailbox` (
  `server` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  KEY `key` (`server`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Mailboxes that should be skipped';

DROP TABLE IF EXISTS `mail_imap_special_mailbox`;
CREATE TABLE IF NOT EXISTS `mail_imap_special_mailbox` (
  `server` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `folder_id` tinyint(3) NOT NULL,
  `skip` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Special mailboxes cases for some mail domains';

DELETE FROM `mail_imap_special_mailbox`;
INSERT INTO `mail_imap_special_mailbox` (`server`, `name`, `folder_id`, `skip`) VALUES
	('imap.yandex.ru', 'исходящие', 0, 1),
	('imap.mail.yahoo.com', 'Bulk Mail', 5, 0),
	('imap.mail.yahoo.com', 'Draft', 3, 1),
	('imap.mail.yahoo.com', 'Sent', 2, 0),
	('imap.mail.yahoo.com', 'Trash', 4, 1);

DROP TABLE IF EXISTS `mail_pop_unordered_domain`;
CREATE TABLE IF NOT EXISTS `mail_pop_unordered_domain` (
  `server` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Domains with unordered messages';

DELETE FROM `mail_pop_unordered_domain`;
INSERT INTO `mail_pop_unordered_domain` (`server`) VALUES
  ('pop.yandex.ru');
