-- res_authors
CREATE TABLE IF NOT EXISTS `res_authors` (
  `login` varchar(150) NOT NULL,
  `password` varchar(50) NOT NULL,
  `isAdmin` tinyint(1) NOT NULL DEFAULT '0',
  `online` int(10) NOT NULL DEFAULT '0',
  `lastVisit` datetime DEFAULT NULL,
  PRIMARY KEY (`login`)
);
-- res_authorsfile
CREATE TABLE IF NOT EXISTS `res_authorsfile` (
  `authorLogin` varchar(50) NOT NULL,
  `fileid` int(11) NOT NULL,
  `writeAccess` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`authorLogin`,`fileid`),
  KEY `res_authorsfile_FK2` (`fileid`)
);
-- res_authorslang
CREATE TABLE IF NOT EXISTS `res_authorslang` (
  `authorLogin` varchar(50) NOT NULL,
  `cultureTitle` varchar(20) NOT NULL,
  PRIMARY KEY (`authorLogin`,`cultureTitle`),
  KEY `res_authorslang_FK2` (`cultureTitle`)
);
-- res_cultures
CREATE TABLE IF NOT EXISTS `res_cultures` (
  `title` varchar(120) NOT NULL,
  `value` varchar(120) NOT NULL,
  `available` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`title`)
);
-- res_data
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
  PRIMARY KEY (`fileid`,`title`,`cultureTitle`),
  UNIQUE KEY `id` (`id`),
  KEY `dateIndex` (`timeChanges`),
  KEY `resources_FK2` (`cultureTitle`)
);
-- res_files
CREATE TABLE IF NOT EXISTS `res_files` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `projectName` varchar(50) NOT NULL,
  `moduleName` varchar(50) NOT NULL,
  `resName` varchar(50) NOT NULL,
  `isLock` tinyint(1) NOT NULL DEFAULT '0',
  `lastUpdate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `index1` (`resName`)
);
-- res_reserve
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
);

