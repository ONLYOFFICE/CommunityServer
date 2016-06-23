alter table mail_mailbox
	drop column is_teamlab_mailbox;

alter table mail_mailbox
	drop column date_created;
	
drop table if exists mail_server_domain;
drop table if exists mail_server_address;
drop table if exists mail_server_server;
drop table if exists mail_server_server_x_tenant;
drop table if exists mail_server_server_type;
drop table if exists mail_server_mail_group;
drop table if exists mail_server_mail_group_x_mail_server_address;
drop table if exists mail_server_domain_x_cname;

alter table mail_server_address
	drop column is_alias;
	
alter table mail_server_address
	drop column date_created;