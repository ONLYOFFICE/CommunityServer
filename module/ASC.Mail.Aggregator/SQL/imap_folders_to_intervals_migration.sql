ALTER TABLE `mail_mailbox`
    ADD COLUMN `imap_intervals` MEDIUMTEXT NULL AFTER `imap_folders`;

UPDATE mail_mailbox SET imap_intervals =
REPLACE(
REPLACE(imap_folders, '}', ',2147483647]}}'), '","Value":', '","Value":{"BeginDateUid":1,"UnhandledUidIntervals":[');