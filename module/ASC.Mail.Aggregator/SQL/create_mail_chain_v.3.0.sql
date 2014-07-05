INSERT IGNORE INTO mail_chain_copy (`id`, `id_mailbox`, `tenant`, `id_user`, `folder`, `length`, `unread`, `has_attachments`, `importance`, `tags`)
SELECT m.chain_id `id`, m.id_mailbox, m.tenant, m.id_user, m.folder, COUNT(DISTINCT m.id) `length`, MAX(m.unread) `unread`, 
IF(MAX(m.attachments_count)> 0, 1, 0) `has_attachments`, MAX(m.importance) `importance`, '' `tags`
FROM mail_mail m
WHERE m.is_removed = 0 AND m.chain_id IS NOT NULL AND m.chain_id <> ""
GROUP BY m.chain_id, m.id_mailbox, m.tenant, m.id_user, m.folder;

INSERT IGNORE INTO mail_chain_copy (`id`, `id_mailbox`, `tenant`, `id_user`, `folder`, `length`, `unread`, `has_attachments`, `importance`, `tags`)
SELECT m.chain_id `id`, m.id_mailbox, m.tenant, m.id_user, cc.folder, cc.length, cc.unread `unread`, 
cc.has_attachments `has_attachments`, cc.importance `importance`, IFNULL(GROUP_CONCAT(DISTINCT tm.id_tag
ORDER BY tm.time_created SEPARATOR ','), '') `tags`
FROM mail_chain_copy cc
INNER JOIN
mail_mail m ON cc.id = m.chain_id AND cc.id_mailbox = m.id_mailbox AND cc.folder = m.folder AND cc.id_user = m.id_user AND cc.tenant = m.tenant
INNER JOIN
mail_tag_mail tm ON m.id = tm.id_mail
WHERE m.is_removed = 0 AND m.chain_id IS NOT NULL AND m.chain_id <> "" AND tm.id_tag IS NOT NULL
GROUP BY cc.id, cc.id_mailbox, cc.folder, cc.id_user, cc.tenant 
ON DUPLICATE KEY UPDATE tags = VALUES(tags);