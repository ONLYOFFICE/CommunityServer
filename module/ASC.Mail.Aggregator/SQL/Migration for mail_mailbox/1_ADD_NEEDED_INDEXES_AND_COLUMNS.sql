-- New mailbox table scheme
-- ADD New Columns to mail_mailbox
alter table mail_mailbox
add id_smtp_server int not null;

alter table mail_mailbox
add id_in_server int not null;

create index main_mailbox_id_smtp_server_mail_mailbox_server_id
on mail_mailbox(id_smtp_server);

create index main_mailbox_id_in_server_mail_mailbox_server_id
on mail_mailbox(id_in_server);

alter table mail_mailbox_server
add address_temp varchar(255);

