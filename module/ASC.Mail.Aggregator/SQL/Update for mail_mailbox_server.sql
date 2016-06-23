alter table mail_mailbox_server
add is_user_data tinyint Default 0

-- id 993 specific for local database
-- id 992 specific for info database
-- update mail_mailbox_server set is_user_data = 1 where id > 992