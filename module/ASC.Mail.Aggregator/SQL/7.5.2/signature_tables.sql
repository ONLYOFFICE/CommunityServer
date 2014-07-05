create table if not exists mail_mailbox_signature(
tenant int not null,
id_mailbox int not null,
html text,
is_active tinyint not null default 0,
primary key(id_mailbox, tenant)
);