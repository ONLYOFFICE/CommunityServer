alter table mail_mailbox
drop column pop3_account; -- Is it needed?

alter table mail_mailbox
drop column pop3_server;

alter table mail_mailbox
drop column ssl_enabled;

alter table mail_mailbox
drop column ssl_outgoing;

alter table mail_mailbox
drop column smtp_auth;

alter table mail_mailbox
drop column smtp_server;

alter table mail_mailbox
drop column smtp_account; -- Is it needed?

alter table mail_mailbox
drop column incoming_encryption_type;

alter table mail_mailbox
drop column outcoming_encryption_type;

alter table mail_mailbox
drop column auth_type_in;

alter table mail_mailbox
drop column auth_type_smtp;

alter table mail_mailbox_server
drop column address_temp;