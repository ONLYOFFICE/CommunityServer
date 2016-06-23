DELETE FROM tenants_quotarow WHERE path like '/mailaggregator/_%';
ALTER TABLE `mail_attachment` ADD INDEX `tenant` (`tenant`, `need_remove`);
UPDATE tenants_quotarow q SET counter = IFNULL((SELECT SUM(a.size) FROM mail_attachment a WHERE a.need_remove = 0 AND a.tenant = q.tenant),0), last_modified = NOW() WHERE q.path = '/mailaggregator/';