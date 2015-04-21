EXPLAIN
SELECT mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id
FROM mail_attachment
INNER JOIN mail_mail ON mail_mail.id = mail_attachment.id_mail
WHERE mail_attachment.id = 12271 AND mail_attachment.need_remove = 0 AND mail_mail.tenant = 10113 AND mail_mail.id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT * FROM mail_mail
WHERE id = 9306;

EXPLAIN
SELECT chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT chain_id, folder, id_mailbox
FROM mail_mail
WHERE id = 9306 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN 
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND chain_id = '9306' AND id_mailbox = 36 AND (folder = 1 OR folder = 2);

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1
ORDER BY chain_date DESC
LIMIT 75;

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 and unread = true
ORDER BY chain_date DESC
LIMIT 75;


EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 AND id_mailbox = 36
ORDER BY chain_date DESC
LIMIT 300;

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 AND chain_date <= '2012-12-28 15:55:28'
ORDER BY chain_date DESC
LIMIT 300;

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 AND id IN (9306,9307,9308)
ORDER BY chain_date DESC
LIMIT 75;

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 AND from_text LIKE '%mono.mail.4test@gmail.com%'
ORDER BY chain_date DESC
LIMIT 75;

EXPLAIN
SELECT id, from_text, to_text, reply_to, subject, importance, date_sent, size, attachments_count, unread, is_answered, is_forwarded, is_from_crm, is_from_tl, folder_restore, folder, chain_id, id_mailbox, chain_date
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND folder = 1 AND id IN (
SELECT id_mail
FROM (
SELECT id_mail
FROM mail_tag_mail
WHERE id_tag = -1015) AS a
GROUP BY id_mail
HAVING COUNT(a.id_mail) = 1)
ORDER BY chain_date DESC
LIMIT 75;

EXPLAIN
SELECT chain_id, id_mailbox, folder
FROM mail_mail
WHERE is_removed = false AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT chain_id, id_mailbox, folder, id, chain_id, folder, id_mailbox
FROM mail_mail
WHERE chain_id IN ('9306','9307') AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false;

EXPLAIN
SELECT chain_id, id_mailbox, folder, id, chain_id, folder, id_mailbox
FROM mail_mail
WHERE chain_id = '9306' AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false;

EXPLAIN
SELECT COUNT(*), MAX(date_sent), MAX(unread), MAX(attachments_count), MAX(importance)
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND chain_id = '9306' AND id_mailbox = 36 AND folder = 1;

EXPLAIN
SELECT * FROM mail_mail 
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND chain_id = '9306' AND id_mailbox = 36 AND folder = 1;

EXPLAIN
SELECT MAX(attachments_count) FROM mail_mail 
WHERE is_removed = false AND chain_id = '9306' AND id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND folder = 1;

EXPLAIN
SELECT MAX(importance)
FROM mail_mail
WHERE is_removed = false AND chain_id = '9306' AND id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND folder IN (1, 2);

EXPLAIN
SELECT MAX(unread)
FROM mail_mail 
WHERE is_removed = false AND chain_id = '9306' AND id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND folder IN (1, 2);

EXPLAIN
SELECT chain_id, folder, id_mailbox
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT DISTINCT tm.id_tag
FROM mail_tag_mail tm
INNER JOIN (
SELECT id
FROM mail_mail
WHERE chain_id = '9306' AND is_removed = 0 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND folder = 2 AND id_mailbox = 36) AS ch ON ch.id = tm.id_mail
ORDER BY tm.time_created ASC;

EXPLAIN
SELECT folder, COUNT(1)
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND unread = 1 AND is_removed = 0
GROUP BY folder;

EXPLAIN
SELECT folder, COUNT(1)
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = 0
GROUP BY folder;


EXPLAIN
SELECT *
FROM mail_mail
WHERE id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT SUM(a.size)
FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id
WHERE m.id_mailbox = 36 AND m.tenant = 10113 AND a.need_remove = true;

EXPLAIN
SELECT * FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id 
WHERE m.id_mailbox = 36;

EXPLAIN
SELECT t.id_tag
FROM mail_tag_mail t
INNER JOIN mail_mail m ON t.id_mail = m.id
WHERE m.id_mailbox = 36;

EXPLAIN
SELECT * FROM mail_mail
WHERE id = 9306 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT * FROM mail_mail
WHERE id IN (9306, 9307) AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT id
FROM mail_mail
WHERE is_removed = false AND folder = 2 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND folder = 2;

EXPLAIN
SELECT id, uidl
FROM mail_mail
WHERE id_mailbox = 36;

EXPLAIN
SELECT id, uidl, md5
FROM mail_mail
WHERE id_mailbox = 36;

EXPLAIN
SELECT * FROM mail_mail
WHERE id = 9306;


EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT * FROM mail_mail
WHERE chain_id = '9306' AND id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false;


EXPLAIN
SELECT DISTINCT folder
FROM mail_mail
WHERE chain_id = '9306' AND id_mailbox = 36 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false;

EXPLAIN
SELECT *
FROM mail_mail t
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;


EXPLAIN
SELECT address, chain_id, chain_date, importance, date_sent, from_text, to_text, cc, bcc, reply_to, stream, is_answered, is_forwarded, subject, attachments_count, size, is_from_tl, folder, unread, introduction, is_text_body_only, id_mailbox, folder_restore, has_parse_error, mime_message_id, mime_in_reply_to
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND id = 9306;

EXPLAIN
SELECT *
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT t.stream, t.attachments_count
FROM mail_mail t
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id
FROM mail_attachment
INNER JOIN mail_mail ON mail_mail.id = mail_attachment.id_mail
WHERE mail_mail.id = 9306 AND mail_attachment.need_remove = 0 AND content_id IS NULL AND mail_mail.tenant = 10113 AND mail_mail.id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT chain_id, id_mailbox, folder
FROM mail_mail
WHERE id = 9306 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT chain_id, id_mailbox, folder
FROM mail_mail
WHERE id IN (9306, 9307) AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT *
FROM mail_tag_mail t
INNER JOIN mail_mail m ON t.id_mail = m.id
WHERE m.id_mailbox = 36;

EXPLAIN
SELECT * FROM mail_mail
WHERE is_removed = false AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id IN (9306,9307);

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT * FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id IN (9306, 9307);

EXPLAIN
SELECT id, folder_restore, uidl
FROM mail_mail
WHERE mime_message_id = '9306' AND id_mailbox = 36;

EXPLAIN
SELECT id, folder_restore, uidl
FROM mail_mail
WHERE mime_message_id IS NULL AND id_mailbox = 36;

EXPLAIN
SELECT mt.id_tag
FROM mail_mail mm
INNER JOIN mail_tag_mail mt ON mm.id = mt.id_mail
WHERE mm.id_mailbox = 36 AND mm.folder = 2 AND mm.mime_message_id = '9306';

EXPLAIN
SELECT id_mailbox, chain_id, folder
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND id = 9306;

EXPLAIN
SELECT id, chain_id, folder, id_mailbox
FROM mail_mail
WHERE id = 9306 AND tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e';

EXPLAIN
SELECT DISTINCT chain_id
FROM mail_mail
WHERE tenant = 10113 AND id_user = 'a1d67170-b343-4046-8bf0-5f89a62d195e' AND is_removed = false AND id_mailbox = 36 AND mime_message_id ='9306';

EXPLAIN
SELECT m.id_user, SUM(a.size) AS size
FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id
WHERE m.tenant = 1013 AND a.need_remove = false
GROUP BY 1
ORDER BY 2 DESC;

EXPLAIN
SELECT id
FROM mail_mail
WHERE id_mailbox = 36 AND chain_id = '9306' AND folder = 2 AND is_removed = 0
ORDER BY date_sent ASC;

EXPLAIN
SELECT id
FROM mail_mail
WHERE id_mailbox = 36 AND chain_id = '9306' AND folder IN (1, 2) AND is_removed = 0;

EXPLAIN
SELECT id, folder_restore, uidl
FROM mail_mail
WHERE md5 = 'fc9b3fa09241d66ff70141422b4d79e0' AND id_mailbox = 36;


