DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade96 DLM00

CREATE PROCEDURE upgrade96()
BEGIN

    UPDATE `tenants_quota` SET `max_total_size`=10995116277760 WHERE `features` LIKE "%controlpanel%" AND `max_total_size` = 1048575;


    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.csv', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.doc', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.docx', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.docx');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.odt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.epub', '.txt');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.html', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.mht', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odp', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ods', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.odt', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.pps', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.ppt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.rtf', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.pdf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.txt', '.rtf');
    INSERT IGNORE INTO `files_converts` (`input`, `output`) VALUES ('.xls', '.pdf');

    DELETE FROM `files_converts` WHERE `input` = '.doct' AND `output` = '.odt';
    DELETE FROM `files_converts` WHERE `input` = '.doct' AND `output` = '.txt';
    DELETE FROM `files_converts` WHERE `input` = '.svgt' AND `output` = '.svg';
    DELETE FROM `files_converts` WHERE `input` = '.xlst' AND `output` = '.csv';
    DELETE FROM `files_converts` WHERE `input` = '.xlst' AND `output` = '.ods';

END DLM00

CALL upgrade96() DLM00

DELIMITER ;