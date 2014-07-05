DROP TABLE IF EXISTS `account_links`;
CREATE TABLE  `account_links` (
  `id` varchar(200) NOT NULL,
  `uid` varchar(200) NOT NULL,
  `provider` nchar(32) DEFAULT NULL,
  `profile` text NOT NULL,
  `linked` datetime NOT NULL,
  PRIMARY KEY (`id`,`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;