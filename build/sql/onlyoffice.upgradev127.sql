DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade127 DLM00

CREATE PROCEDURE upgrade127()
BEGIN

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.djvu', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.oform', '.pdf');

    DELETE FROM `files_converts` WHERE `output`='.docxf';

    ALTER TABLE `short_links` CHANGE COLUMN `id` `id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT;

END DLM00

CALL upgrade127() DLM00

DELIMITER ;
