DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade952 DLM00

CREATE PROCEDURE upgrade952()
BEGIN

    IF EXISTS(SELECT * FROM information_schema.`STATISTICS` WHERE `TABLE_SCHEMA` = DATABASE() AND `TABLE_NAME` = 'mail_mailbox' AND `INDEX_NAME` = 'user_id_index') THEN
        ALTER TABLE `mail_mailbox`
            DROP INDEX `user_id_index`,
            ADD INDEX `tenant_user_id` (`tenant`, `id_user`);
    END IF;

END DLM00

CALL upgrade952() DLM00

DELIMITER ;