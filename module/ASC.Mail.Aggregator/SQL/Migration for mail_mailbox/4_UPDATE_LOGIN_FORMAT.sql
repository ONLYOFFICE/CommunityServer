update mail_mailbox_server
set username = replace(username, address_temp, '%EMAILADDRESS%')
where address_temp is not null;

update mail_mailbox_server
set address_temp = NULL
where address_temp is not null and username = '%EMAILADDRESS%';


update mail_mailbox_server
set username = concat(Replace(left(username, locate(substr(address_temp, 1, LOCATE('@', address_temp)-1), username) + length(substr(address_temp, 1, LOCATE('@', address_temp)-1))-1),
					substr(address_temp, 1, LOCATE('@', address_temp)-1), '%EMAILLOCALPART%'),
	  				substr(username, locate(username, substr(address_temp, 1, LOCATE('@', address_temp)-1)) + length(substr(address_temp, 1, LOCATE('@', address_temp)-1))+1))
where address_temp is not null and locate(substr(address_temp, 1, LOCATE('@', address_temp)-1), username) > 0;

update mail_mailbox_server
set address_temp = NULL
where address_temp is not null and username = '%EMAILLOCALPART%';

update mail_mailbox_server
set username = replace(username, substr(address_temp, LOCATE('@', address_temp)+1), '%EMAILDOMAIN%')
where address_temp is not null;

update mail_mailbox_server
set username = replace(username,
					reverse(substr(reverse(substr(address_temp, LOCATE('@', address_temp)+1)),
		 			locate('.', reverse(substr(address_temp, LOCATE('@', address_temp)+1)))+1)), '%EMAILHOSTNAME%')
where address_temp is not null;

update mail_mailbox_server
set username = replace(username,
					substring_index(substr(address_temp, LOCATE('@', address_temp)+1), '.', 1)
		 			, '%EMAILHOSTNAME%')
where address_temp is not null;

update mail_mailbox_server
set address_temp = NULL

-- alter table mail_mailbox_server
-- drop column address_temp