-- Set providers for servers with knowing domains
update mail_mailbox_server mmss
		inner join (select udn.server_id as id_server, mmd.id_provider as id_provider
						from (select mm.id, mms.id as server_id, substring(mm.address, locate("@", mm.address)+ 1) as domain
								from	mail_mailbox mm
		 								inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
								where mms.id_provider = 0) as udn
								inner join mail_mailbox_domain as mmd on mmd.name = udn.domain
						group by udn.server_id, mmd.id_provider) as sXp on mmss.id = sXp.id_server
set mmss.id_provider = sXp.id_provider;

-- Add not existing domain to domains if hostname present in db
insert into mail_mailbox_domain(name, id_provider)
	select mms_ud.domain, mms_o.id_provider
	from
		(select mm.id, mms.id as id_server, mms.hostname as hostname, substring(mm.address, locate("@", mm.address)+ 1) as domain
		from	mail_mailbox mm
				inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
		where mms.id_provider = 0) as mms_ud
		inner join mail_mailbox_server as mms_o  on mms_ud.hostname = mms_o.hostname
	where mms_ud.id_server <> mms_o.id
	group by domain, id_provider;

-- Add provider id for recently added
update mail_mailbox_server mmss
		inner join (select udn.server_id as id_server, mmd.id_provider as id_provider
						from (select mm.id, mms.id as server_id, substring(mm.address, locate("@", mm.address)+ 1) as domain
								from	mail_mailbox mm
		 								inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
								where mms.id_provider = 0) as udn
								inner join mail_mailbox_domain as mmd on mmd.name = udn.domain
						group by udn.server_id, mmd.id_provider) as sXp on mmss.id = sXp.id_server
set mmss.id_provider = sXp.id_provider;

-- Add unknown providers and domains
insert into mail_mailbox_domain(id_provider, name)
	select 0, substring(mm.address, locate("@", mm.address)+ 1) as domain
	from	mail_mailbox mm
			inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
	where mms.id_provider = 0
	group by substring(mm.address, locate("@", mm.address)+ 1);


insert into mail_mailbox_provider(name)
	select substring(mm.address, locate("@", mm.address)+ 1) as domain
	from	mail_mailbox mm
			inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
	where mms.id_provider = 0
	group by substring(mm.address, locate("@", mm.address)+ 1);

--  Update domains table
update mail_mailbox_domain mmd
			inner join mail_mailbox_provider mmp on mmd.name = mmp.name
			inner join mail_mailbox mm on mmp.name = substring(mm.address, locate("@", mm.address)+ 1)
			inner join mail_mailbox_server mms on (mm.id_smtp_server = mms.id or mm.id_in_server = mms.id)
set mmd.id_provider = mmp.id, mms.id_provider = mmp.id
where mms.id_provider = 0;


