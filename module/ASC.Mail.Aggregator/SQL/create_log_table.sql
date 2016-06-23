-- Mail aggregators table creation
create table if not exists mail_aggregators(
id int primary key auto_increment,
ip varchar(50) not null comment 'Aggregator ip',
start_work datetime not null,
end_work datetime default null 
);


-- Mail log table creation
create table if not exists mail_log(
id bigint primary key auto_increment,
id_aggregator int not null comment 'Reference to mail_aggregators',
id_thread int not null comment 'Aggregator''s thread id',
id_mailbox int not null comment 'Reference to mail_mailbox',
processing_start_time datetime not null,
processing_end_time datetime default null,
processed_mails_count int default null
);

-- Create needede indexes
CREATE INDEX mail_log_mail_aggregators_id_aggregator ON mail_log (id_aggregator);
CREATE INDEX mail_log_mail_mailbox_id_mailbox ON mail_log (id_mailbox);