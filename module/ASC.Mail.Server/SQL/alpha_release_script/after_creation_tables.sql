SET @domain_name = 'teamlab.info';
SET @server_name = 'mx1.teamlab.info';
SET @server_connection_string = '{"DbConnection" : "Server=54.228.212.82; DATABASE=vmail; USER ID=vmailadmin; PASSWORD=ESbWgt5Xym1SRjGfHL0rsi4VVZbldk;Pooling= TRUE; CHARACTER SET=utf8"}';
SET @server_type_name = 'Postfix';
SET @server_type = 1;

INSERT INTO `mail_server_server_type` (`id`, `name`) VALUES (@server_type, @server_type_name);

SET @provider_id = INSERT INTO `mail_mailbox_provider` (`name`, `display_name`, `display_short_name`, `documentation`) VALUES (@domain_name, NULL, NULL, NULL);
INSERT INTO `mail_mailbox_domain` (`id_provider`, `name`) VALUES (@provider_id, @domain_name);

SET @imap_settings_id = INSERT INTO `mail_mailbox_server` (`id_provider`, `type`, `hostname`, `port`, `socket_type`, `username`, `authentication`, `is_user_data`) 
VALUES (@provider_id, 'imap', @server_name, 993, 'SSL', '%EMAILADDRESS%', 'password-cleartext', 0);

SET @smtp_settings_id = INSERT INTO `mail_mailbox_server` (`id_provider`, `type`, `hostname`, `port`, `socket_type`, `username`, `authentication`, `is_user_data`) 
VALUES (@provider_id, 'smtp', @server_name, 25, 'STARTTLS', '%EMAILADDRESS%', 'password-cleartext', 0);

INSERT INTO `mail_server_server` (`mx_record`, `connection_string`, `server_type`, `smtp_settings_id`, `imap_settings_id`) 
VALUES (@server_name, @server_connection_string, @server_type, @smtp_settings_id, @imap_settings_id);