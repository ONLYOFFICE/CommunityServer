DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade121 DLM00

CREATE PROCEDURE upgrade121()
BEGIN

  CREATE TABLE IF NOT EXISTS `firebase_users` (
    `id` int(10) NOT NULL AUTO_INCREMENT,
    `user_id` varchar(36) NOT NULL,
    `tenant_id` int(11) NOT NULL,
    `firebase_device_token` varchar(255) NOT NULL,
    `is_subscribed` tinyint(1) NOT NULL DEFAULT '0',
    `application` varchar(20) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `user_id` (`user_id`)
  )ENGINE=InnoDB DEFAULT CHARSET=utf8;

END DLM00

CALL upgrade121() DLM00

DELIMITER ;
