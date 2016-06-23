create table mail_chain_x_crm_entity
(
id_tenant int NOT NULL,
id_mailbox int NOT NULL,
id_chain varchar(255) NOT NULL,
entity_id int NOT NULL,
entity_type int NOT NULL comment '1 - contact, 2 - case, 3 - opportunity',
PRIMARY KEY(id_tenant, id_mailbox, id_chain, entity_id, entity_type)
);