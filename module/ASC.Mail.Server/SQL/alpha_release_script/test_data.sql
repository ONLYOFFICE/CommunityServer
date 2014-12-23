insert into mail_server_server_type(id, name)
values(1, 'MailEnable');

insert into mail_server_server(id, mx_record,	connection_string, server_type, smtp_settings_id,	imap_settings_id)
values(1, 'mxg1.triple8.net', 'test_connection', -1, 529, 993); -- Absmedia local smtp, avsmedia_loacal_imap)

insert into mail_server_server_x_tenant(id_tenant, id_server)
values(0,1);

insert into mail_server_domain(id, tenant, name, date_added)
values(1, 0, 'avsmedia.com', '2012-12-21 16:50:51');

insert into mail_server_domain(id, tenant, name, date_added)
values(2, 0, 'avsmedia.net', '2012-12-21 16:50:51');