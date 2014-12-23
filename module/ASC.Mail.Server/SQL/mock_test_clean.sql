DELETE FROM `test`.`mail_server_address`
WHERE `id_mailbox` IN 
(SELECT id
FROM mail_mailbox
WHERE tenant = 0 AND id_user = 'fca7260e-3fd1-482e-9641-c04dc3e302be' AND 
address LIKE '%safronov8.com' OR 
address LIKE '%katkov.com' OR 
address LIKE '%novozhenin8.com' OR 
address LIKE '%yudanov8.com' OR 
address LIKE '%s-tlmail.com' AND 
is_removed = 0);

DELETE FROM `test`.`mail_server_domain`
WHERE 
`name` LIKE '%safronov8%' OR 
`name` LIKE '%katkov%' OR 
`name` LIKE '%novozhenin8%' OR 
`name` LIKE '%yudanov8%' OR
`name` LIKE '%s-tlmail%';

UPDATE mail_mailbox SET is_removed = 1
WHERE tenant = 0 AND id_user = 'fca7260e-3fd1-482e-9641-c04dc3e302be' AND 
address LIKE '%safronov8.com' OR 
address LIKE '%katkov.com' OR 
address LIKE '%novozhenin8.com' OR
address LIKE '%yudanov8.com' OR
address LIKE '%s-tlmail.com';

DELETE FROM `test`.`mail_server_address` WHERE `name` LIKE 'peter%';