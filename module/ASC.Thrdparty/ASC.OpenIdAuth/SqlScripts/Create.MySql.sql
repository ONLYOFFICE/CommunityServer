DROP TABLE IF EXISTS `test`.`openid_links`;
CREATE TABLE  `test`.`openid_links` (
  `id` varchar(50) NOT NULL,
  `uid` varchar(32) NOT NULL,
  `provider` nchar(32) DEFAULT NULL,
  `profile` text NOT NULL,
  `linked` datetime NOT NULL,
  PRIMARY KEY (`id`,`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;