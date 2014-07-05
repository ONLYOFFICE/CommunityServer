insert into mail_imap_special_mailbox(server, name, folder_id, skip) values 
('imap.gmx.com', 'Drafts', 3, 1),
('imap.gmx.com', 'Sent', 2, 0),
('imap.gmx.com', 'Spam', 5, 0),
('imap.gmx.com', 'Trash', 4, 1),
('imap.gmx.com', 'OUTBOX', 2, 1),
('imap.hushmail.com', 'Drafts', 3, 1),
('imap.hushmail.com', 'Sent', 2, 0),
('imap.hushmail.com', 'Trash', 4, 1),
('imap.hushmail.com', 'Junk', 5, 0);

create table mail_chain_x_crm_entity
(
id_tenant int NOT NULL,
id_mailbox int NOT NULL,
id_chain varchar(255) NOT NULL,
entity_id int NOT NULL,
entity_type int NOT NULL comment '1 - contact, 2 - case, 3 - opportunity',
PRIMARY KEY(id_tenant, id_mailbox, id_chain, entity_id, entity_type)
);