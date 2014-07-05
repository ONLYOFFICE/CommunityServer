-- ADD NOT EXISTING SERVERS
insert into mail_mailbox_server(id_provider,`type`,hostname,port,socket_type,authentication, username, address_temp)
select 0, uis.`type`, uis.hostname, uis.port, uis.socket_type, uis.authentication, uis.account, uis.address
from (select
			id as id_mailbox,
			pop3_account as account,
			address,
			case imap when 0 then 'pop3' when 1 then 'imap' end as 'type',
			substring(pop3_server, 1, locate(":", pop3_server)- 1) as hostname,
			substring(pop3_server, locate(":", pop3_server)+ 1) as port,
			case incoming_encryption_type when 0 then 'plain' when 1 then 'SSL' when 2 then 'STARTTLS' end as socket_type,
			case auth_type_in when 0 then 'none' when 1 then '' when 4 then 'password-encrypted' when 5 then 'oauth2' end as authentication
		from mail_mailbox
		group by 'type', hostname, port, socket_type, authentication) as uis
	left outer join mail_mailbox_server as mms on (mms.`type` = uis.`type` and mms.hostname = uis.hostname and mms.port = uis.port and mms.authentication = uis.authentication and mms.socket_type = uis.socket_type)
where mms.id is null
union
select 0, uis.`type`, uis.hostname, uis.port, uis.socket_type, uis.authentication, uis.account, uis.address
from (select
			id as id_mailbox,
			smtp_account as account,
			address,
			'smtp' as 'type',
			substring(smtp_server, 1, locate(":", smtp_server)- 1) as hostname,
			substring(smtp_server, locate(":", smtp_server)+ 1) as port,
			case outcoming_encryption_type when 0 then 'plain' when 1 then 'SSL' when 2 then 'STARTTLS' end as socket_type,
			case auth_type_smtp when 0 then 'none' when 1 then '' when 4 then 'password-encrypted' when 5 then 'oauth2' end as authentication
		from mail_mailbox
		group by 'type', hostname, port, socket_type, authentication) as uis
left outer join mail_mailbox_server as mms on (mms.`type` = uis.`type` and mms.hostname = uis.hostname and mms.port = uis.port and mms.authentication = uis.authentication and mms.socket_type = uis.socket_type)
where mms.id is null;

-- ADD SERVER ID TO mail_mailbox
-- SMTP
update mail_mailbox as mm1
inner join
(
	select mm.id_mailbox as id_mailbox, mms.id as id_mailbox_server
	from (select
				id as id_mailbox,
				smtp_account as account,
				address,
				'smtp' as 'type',
				substring(smtp_server, 1, locate(":", smtp_server)- 1) as hostname,
				substring(smtp_server, locate(":", smtp_server)+ 1) as port,
				case outcoming_encryption_type when 0 then 'plain' when 1 then 'SSL' when 2 then 'STARTTLS' end as socket_type,
				case auth_type_smtp when 0 then 'none' when 1 then '' when 4 then 'password-encrypted' when 5 then 'oauth2' end as authentication,
				id_in_server			
			from mail_mailbox) as mm
	inner join mail_mailbox_server as mms  on (mms.`type` = mm.`type` and mms.hostname = mm.hostname and mms.port = mm.port and mms.authentication = mm.authentication and mms.socket_type = mm.socket_type)
) as mmXmms on mm1.id = mmXmms.id_mailbox
set mm1.id_smtp_server = mmXmms.id_mailbox_server;

-- POP3 AND IMAP
update mail_mailbox as mm1
inner join
(
	select mm.id_mailbox as id_mailbox, mms.id as id_mailbox_server
	from
			(select
				id as id_mailbox,
				pop3_account as account,
				address,
				case imap when 0 then 'pop3' when 1 then 'imap' end as 'type',
				substring(pop3_server, 1, locate(":", pop3_server)- 1) as hostname,
				substring(pop3_server, locate(":", pop3_server)+ 1) as port,
				case incoming_encryption_type when 0 then 'plain' when 1 then 'SSL' when 2 then 'STARTTLS' end as socket_type,
				case auth_type_in when 0 then 'none' when 1 then '' when 4 then 'password-encrypted' when 5 then 'oauth2' end as authentication
			from mail_mailbox) as mm
		inner join mail_mailbox_server as mms on (mms.`type` = mm.`type` and mms.hostname = mm.hostname and mms.port = mm.port and mms.authentication = mm.authentication and mms.socket_type = mm.socket_type)
) as mmXmms on mm1.id = mmXmms.id_mailbox
set mm1.id_in_server = mmXmms.id_mailbox_server;