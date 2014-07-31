-- account_links
CREATE TABLE IF NOT EXISTS `account_links` (
  `id` varchar(200) NOT NULL,
  `uid` varchar(200) NOT NULL,
  `provider` char(32) DEFAULT NULL,
  `profile` text NOT NULL,
  `linked` datetime NOT NULL,
  PRIMARY KEY (`id`,`uid`),
  KEY `uid` (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- webstudio_fckuploads
CREATE TABLE IF NOT EXISTS `webstudio_fckuploads` (
  `TenantID` int(11) NOT NULL,
  `StoreDomain` varchar(50) NOT NULL,
  `FolderID` varchar(100) NOT NULL,
  `ItemID` varchar(100) NOT NULL,
  PRIMARY KEY (`TenantID`,`StoreDomain`,`FolderID`,`ItemID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- webstudio_settings
CREATE TABLE IF NOT EXISTS `webstudio_settings` (
  `TenantID` int(11) NOT NULL,
  `ID` varchar(64) NOT NULL,
  `UserID` varchar(64) NOT NULL,
  `Data` mediumtext NOT NULL,
  PRIMARY KEY (`TenantID`,`ID`,`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
-- webstudio_uservisit
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

