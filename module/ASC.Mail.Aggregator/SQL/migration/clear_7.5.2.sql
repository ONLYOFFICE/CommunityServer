-- Удаление mail_mailbox_signature
drop table if exists mail_mailbox_signature;

-- Восстановление mail_folder
drop table if exists mail_folder;
rename table `mail_folder_old` to `mail_folder`;

-- Восстановление mail_mail
drop table if exists mail_mail;
rename table `mail_mail_old` to `mail_mail`;

-- Восстановление mail_attachment
drop table if exists mail_attachment;
rename table `mail_attachment_old` to `mail_attachment`;`