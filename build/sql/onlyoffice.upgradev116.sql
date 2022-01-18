DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade116 DLM00

CREATE PROCEDURE upgrade116()
BEGIN

    CREATE TABLE IF NOT EXISTS `files_link` (
        `source_id` varchar(32) NOT NULL,
        `linked_id` varchar(32) NOT NULL,
        `linked_for` char(38) NOT NULL,
        `tenant_id` int(10) NOT NULL,
        PRIMARY KEY (`tenant_id`, `source_id`, `linked_id`),
        KEY `linked_for` (`tenant_id`, `source_id`, `linked_id`, `linked_for`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.docxf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.oform');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docxf', '.txt');

END DLM00

CALL upgrade116() DLM00

DELIMITER ;
