--Table creation
alter table mail_mailbox
	add incoming_encryption_type int,
	add outcoming_encryption_type int;


update mail_mailbox
	set incoming_encryption_type = 1
	where ssl_enabled = 1;


update mail_mailbox
	set incoming_encryption_type = 0
	where ssl_enabled = 0;
	
	
update mail_mailbox
	set outcoming_encryption_type = 1
	where ssl_outgoing = 1;
	

update mail_mailbox
	set outcoming_encryption_type = 0
	where ssl_outgoing = 0;

-- comments for encryptyon fields
ALTER TABLE mail_mailbox
	CHANGE COLUMN `incoming_encryption_type` `incoming_encryption_type` INT(11) NULL DEFAULT NULL COMMENT '0 - none, 1 - SSL, 2 - STARTTLS' AFTER `imap_folders`,
	CHANGE COLUMN `outcoming_encryption_type` `outcoming_encryption_type` INT(11) NULL DEFAULT NULL COMMENT '0 - none, 1 - SSL, 2 - STARTTLS' AFTER `incoming_encryption_type`;


--fields for advanced authentication
ALTER TABlE mail_mailbox
	ADD COLUMN `auth_type_in` INT(11) NULL DEFAULT NULL COMMENT '1 - login, 4 - CremdMD5, 5 - oauh2',
	ADD COLUMN `auth_type_smtp` INT(11) NULL DEFAULT NULL COMMENT '1 - login, 4 - CremdMD5, 5 - oauh2';



-- set auth type for in mail - imap or pop3
update mail_mailbox
set auth_type_in = 1
where refresh_token is NULL;

update mail_mailbox
set auth_type_in = 5
where refresh_token is not NULL;


-- set auth type for out mail - smtp
update mail_mailbox
set auth_type_smtp = 0
where smtp_auth = 0 and refresh_token is NULL;

update mail_mailbox
set auth_type_smtp = 1
where smtp_auth = 1 and refresh_token is NULL;

update mail_mailbox
set auth_type_smtp = 5
where smtp_auth = 1 and refresh_token is NOT NULL;
