insert ignore into core_user(id, firstname, lastname, username, tenant, last_modified) values ('00000000-0000-0000-0000-000000000ace','Administrator','','administrator',0,utc_timestamp());
insert ignore into core_usersecurity(tenant, userid, pwdhash, pwdhashsha512) values (0,'00000000-0000-0000-0000-000000000ace','jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=','l/DFJ5yg4oh1F6Qp7uDhBw==');
insert ignore into core_usergroup(tenant, userid, groupid, ref_type) values (0,'00000000-0000-0000-0000-000000000ace','cd84e66b-b803-40fc-99f9-b2969a54a1de',0);

insert ignore into tenants_tenants(id, alias, name, version, creationdatetime, owner_id) values (0, 'localhost', 'Cloud Office Applications', 75, UTC_TIMESTAMP(), '00000000-0000-0000-0000-000000000ace');
update tenants_tenants set id = 0 where id = 1;
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-75, '#9 year + docs', NULL, 1024, 1048575, 400, 'backup,domain,docs,year', 9600.00, 0.00, '59', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-74, '#8 year + docs', NULL, 1024, 1048575, 300, 'backup,domain,docs,year', 7200.00, 0.00, '58', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-73, '#7 year + docs', NULL, 1024, 1048575, 200, 'backup,domain,docs,year', 4800.00, 0.00, '57', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-72, '#6 year + docs', NULL, 1024, 1048575, 100, 'backup,domain,docs,year', 3600.00, 0.00, '56', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-71, '#5 year + docs', NULL, 1024, 1048575, 70, 'backup,domain,docs,year', 2400.00, 0.00, '55', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-70, '#4 year + docs', NULL, 1024, 1048575, 45, 'backup,domain,docs,year', 1800.00, 0.00, '54', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-69, '#3 year + docs', NULL, 1024, 1048575, 30, 'backup,domain,docs,year', 1200.00, 0.00, '53', 1);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-2, 'trial', NULL, 1024, 1048575, 400, 'trial,backup,domain,docs,sms', 0.00, 0.00, '23', 0);
replace into tenants_quota (tenant, name, description, max_file_size, max_total_size, active_users, features, price, price2, avangate_id, visible) values (-1, 'default', NULL, 1024, 1048575, 10000, 'backup,domain,docs,sms', 0.00, 0.00, NULL, 0);

insert ignore into core_settings(tenant, id, value) values (-1, 'SmtpSettings', 0xF052E090A1A3750DADCD4E9961DA04AA51EF0197E2C0623CF12C5838BFA40A9B9FDBAD2E6E1D503F5C2ED4BF7337EFD042EDE740C88EDE16908870DE10ACD479887FC663C7BD90306DE811DD537A285B1A182626646DE5A51495C5DCC0D3509A0A4E9C876624C17C8EC9A97A9FD597B262F21C3E73F1B745BAD86E1AFFE02695);

-- subscribe all users to what's new notifications
insert ignore into core_subscription(source, action, recipient, object, tenant) values('asc.web.studio','send_whats_new','abef62db-11a8-4673-9d32-ef1d8af19dc0','',-1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values('asc.web.studio','send_whats_new','abef62db-11a8-4673-9d32-ef1d8af19dc0','email.sender',-1);
-- subscribe all users to new events
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('6504977c-75af-4691-9099-084d3ddeea04','new feed','abef62db-11a8-4673-9d32-ef1d8af19dc0','',-1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values('6504977c-75af-4691-9099-084d3ddeea04','new feed','abef62db-11a8-4673-9d32-ef1d8af19dc0','email.sender|messanger.sender',-1);
-- subscribe all users to new blogs
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('6a598c74-91ae-437d-a5f4-ad339bd11bb2','new post','abef62db-11a8-4673-9d32-ef1d8af19dc0','',-1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values('6a598c74-91ae-437d-a5f4-ad339bd11bb2','new post','abef62db-11a8-4673-9d32-ef1d8af19dc0','email.sender|messanger.sender',-1);
-- subscribe all users to new forum
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('853b6eb9-73ee-438d-9b09-8ffeedf36234', 'new topic in forum', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('853b6eb9-73ee-438d-9b09-8ffeedf36234', 'new topic in forum', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to photos
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('9d51954f-db9b-4aed-94e3-ed70b914e101', 'new photo uploaded', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('9d51954f-db9b-4aed-94e3-ed70b914e101', 'new photo uploaded', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to bookmarks
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('28b10049-dd20-4f54-b986-873bc14ccfc7', 'new bookmark created', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('28b10049-dd20-4f54-b986-873bc14ccfc7', 'new bookmark created', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to wiki
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('742cf945-cbbc-4a57-82d6-1600a12cf8ca', 'new wiki page', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('742cf945-cbbc-4a57-82d6-1600a12cf8ca', 'new wiki page', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to new birthdays
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('37620ae5-c40b-45ce-855a-39dd7d76a1fa','BirthdayReminder','abef62db-11a8-4673-9d32-ef1d8af19dc0','',-1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values('37620ae5-c40b-45ce-855a-39dd7d76a1fa','BirthdayReminder','abef62db-11a8-4673-9d32-ef1d8af19dc0','email.sender|messanger.sender',-1);
-- subscribe all users to documents
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('6fe286a4-479e-4c25-a8d9-0156e332b0c0', 'sharedocument', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('6fe286a4-479e-4c25-a8d9-0156e332b0c0', 'sharefolder', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6fe286a4-479e-4c25-a8d9-0156e332b0c0', 'sharedocument', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6fe286a4-479e-4c25-a8d9-0156e332b0c0', 'sharefolder', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6fe286a4-479e-4c25-a8d9-0156e332b0c0', 'updatedocument', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to projects
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'invitetoproject', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'milestonedeadline', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'newcommentformessage', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'newcommentformilestone', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'newcommentfortask', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'projectcreaterequest', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'projecteditrequest', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'removefromproject', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'responsibleforproject', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'responsiblefortask', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('6045b68c-2c2e-42db-9e53-c272e814c4ad', 'taskclosed', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe all users to calendar events
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('40650da3-f7c1-424c-8c89-b9c115472e08', 'calendar_sharing', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('40650da3-f7c1-424c-8c89-b9c115472e08', 'event_alert', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('40650da3-f7c1-424c-8c89-b9c115472e08', 'calendar_sharing', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('40650da3-f7c1-424c-8c89-b9c115472e08', 'event_alert', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
-- subscribe admin group to admin notifications
insert ignore into core_subscription(source, action, recipient, object, tenant) values('asc.web.studio','admin_notify','cd84e66b-b803-40fc-99f9-b2969a54a1de','',-1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values('asc.web.studio','admin_notify','cd84e66b-b803-40fc-99f9-b2969a54a1de','email.sender',-1);
-- subscribe all users to crm
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'SetAccess', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'SetAccess', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ResponsibleForTask', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ResponsibleForTask', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'AddRelationshipEvent', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'AddRelationshipEvent', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ExportCompleted', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ExportCompleted', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ImportCompleted', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ImportCompleted', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'CreateNewContact', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'CreateNewContact', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);
insert ignore into core_subscription(source, action, recipient, object, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ResponsibleForOpportunity', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '', -1);
insert ignore into core_subscriptionmethod(source, action, recipient, sender, tenant) values ('13ff36fb-0272-4887-b416-74f52b0d0b02', 'ResponsibleForOpportunity', 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'email.sender|messanger.sender', -1);

-- default permissions
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, '5d5b7260-f7f7-49f1-a1c9-95fbb6a12604', 'ef5e6790-f346-4b6e-b662-722bc28cb0db', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, '5d5b7260-f7f7-49f1-a1c9-95fbb6a12604', 'f11e8f3f-46e6-4e55-90e3-09c22ec565bd', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '088d5940-a80f-4403-9741-d610718ce95c', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '08d66144-e1c9-4065-9aa1-aa4bba0a7bc8', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '08d75c97-cf3f-494b-90d1-751c941fe2dd', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '0d1f72a8-63da-47ea-ae42-0900e4ac72a9', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '13e30b51-5b4d-40a5-8575-cb561899eeb1', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '19f658ae-722b-4cd8-8236-3ad150801d96', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '2c6552b3-b2e0-4a00-b8fd-13c161e337b1', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '388c29d3-c662-4a61-bf47-fc2f7094224a', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '40bf31f4-3132-4e76-8d5c-9828a89501a3', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '49ae8915-2b30-4348-ab74-b152279364fb', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '9018c001-24c2-44bf-a1db-d1121a570e74', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '948ad738-434b-4a88-8e38-7569d332910a', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', '9d75a568-52aa-49d8-ad43-473756cd8903', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'a362fe79-684e-4d43-a599-65bc1f4e167f', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'c426c349-9ad4-47cd-9b8f-99fc30675951', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'd11ebcb9-0e6e-45e6-a6d0-99c41d687598', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'd1f3b53d-d9e2-4259-80e7-d24380978395', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd49f4e30-da10-4b39-bc6d-b41ef6e039d3', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'abef62db-11a8-4673-9d32-ef1d8af19dc0', 'd852b66f-6719-45e1-8657-18f0bb791690', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'e37239bd-c5b5-4f1e-a9f8-3ceeac209615', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'fbc37705-a04c-40ad-a68c-ce2f0423f397', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', 'fcac42b8-9386-48eb-a938-d19b3c576912', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '13e30b51-5b4d-40a5-8575-cb561899eeb1', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '49ae8915-2b30-4348-ab74-b152279364fb', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', '9018c001-24c2-44bf-a1db-d1121a570e74', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'd1f3b53d-d9e2-4259-80e7-d24380978395', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'e37239bd-c5b5-4f1e-a9f8-3ceeac209615', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'f11e88d7-f185-4372-927c-d88008d2c483', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'ba74ca02-873f-43dc-8470-8620c156bc67', 'f11e8f3f-46e6-4e55-90e3-09c22ec565bd', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '00e7dfc5-ac49-4fd3-a1d6-98d84e877ac4', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '14be970f-7af5-4590-8e81-ea32b5f7866d', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '18ecc94d-6afa-4994-8406-aee9dff12ce2', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '298530eb-435e-4dc6-a776-9abcd95c70e9', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '430eaf70-1886-483c-a746-1a18e3e6bb63', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '557d6503-633b-4490-a14c-6473147ce2b3', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '724cbb75-d1c9-451e-bae0-4de0db96b1f7', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '7cb5c0d1-d254-433f-abe3-ff23373ec631', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '91b29dcd-9430-4403-b17a-27d09189be88', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'a18480a4-6d18-4c71-84fa-789888791f45', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'b630d29b-1844-4bda-bbbe-cf5542df3559', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'c62a9e8d-b24c-4513-90aa-7ff0f8ba38eb', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', 'd7cdb020-288b-41e5-a857-597347618533', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, '712d9ec3-5d2b-4b13-824f-71f00191dcca', 'e0759a42-47f0-4763-a26a-d5aa665bec35', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'bba32183-a14d-48ed-9d39-c6b4d8925fbf', '0d68b142-e20a-446e-a832-0d6b0b65a164', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '6f05c382-8bca-4469-9424-c807a98c40d7', '', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|1e04460243b54d7982f3fd6208a11960', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|6743007c6f954d208c88a8601ce5e76d', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|e67be73df9ae4ce18fec1880cb518cb4', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|ea942538e68e49079394035336ee0ba8', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|32d24cb57ece46069c9419216ba42086', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|bf88953e3c434850a3fbb1e43ad53a3e', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|2a9230378b2d487b9a225ac0918acf3f', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|f4d98afdd336433287783c6945c81ea0', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|28b10049dd204f54b986873bc14ccfc7', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|3cfd481b46f24a4ab55cb8c0c9def02c', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|6a598c7491ae437da5f4ad339bd11bb2', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|742cf945cbbc4a5782d61600a12cf8ca', 0);
insert ignore into core_acl (tenant, subject, action, object, acetype) values (-1, 'c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e', '77777777-32ae-425f-99b5-83176061d1ae', 'ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|853b6eb973ee438d9b098ffeedf36234', 0);
