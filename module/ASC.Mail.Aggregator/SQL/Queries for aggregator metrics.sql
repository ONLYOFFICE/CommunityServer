-- set @id_aggregator = 142;
-- U can set this parameter as query or id list like '1, 2, 3'
-- set @id_aggregator_query = 'select id from mail_aggregators where end_work is null and id = 142';
set @id_aggregator_query = '33, 34, 35';


-- View for average performance per mailbox ============================================================================================
set @avg_perf_per_mailbox = concat('select 	
			id_mailbox,
 			count(id_mailbox),
 	 		sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) as ''SummaryTime (secs)'',
 	 		sum(processed_mails_count) as ''Total mails processed'',
 	 		(sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) + 0.0) / count(id_mailbox) as ''Averrage time for processing per mailbox(secs)'',
			sum(if(processed_mails_count > 0, TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time)), 0)) / (sum(processed_mails_count) + 0.0) as ''Seconds for one mail proccessing per mailbox'' 	 		
from mail_log
where id_aggregator in (', @id_aggregator_query, ')
group by id_mailbox;');

prepare avg_perf_per_mailbox  from @avg_perf_per_mailbox;
execute avg_perf_per_mailbox;
deallocate prepare avg_perf_per_mailbox;
-- =====================================================================================================================================

-- View for average performance per mailbox per aggregator ============================================================================================
set @avg_perf_per_mailbox_per_aggregator = concat('select 	
			id_aggregator,
			id_mailbox,
 			count(id_mailbox),
 	 		sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) as ''SummaryTime (secs)'',
 	 		sum(processed_mails_count) as ''Total mails processed'',
 	 		(sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) + 0.0) / count(id_mailbox) as ''Averrage time for processing per mailbox(secs)'',
			sum(if(processed_mails_count > 0, TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time)), 0)) / (sum(processed_mails_count) + 0.0) as ''Seconds for one mail proccessing per mailbox'' 	 		
from mail_log
where id_aggregator in (', @id_aggregator_query, ')
group by id_mailbox, id_aggregator;');

prepare avg_perf_per_mailbox_per_aggregator  from @avg_perf_per_mailbox_per_aggregator;
execute avg_perf_per_mailbox_per_aggregator;
deallocate prepare avg_perf_per_mailbox_per_aggregator;
-- =====================================================================================================================================


-- View for average performance for all mailboxes ======================================================================================
set @avg_perf_for_all_mailboxes = concat(
'select sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) / (count(id_mailbox) + 0.0) as ''Average time for processing one mailbox (sec)'',
		 sum(if(processed_mails_count > 0, TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time)), 0)) / (sum(processed_mails_count) + 0.0) as ''Seconds for one mail proccessing per mailbox'' 	 		
from mail_log
where id_aggregator in (', @id_aggregator_query,')');


prepare avg_perf_for_all_mailboxes  from @avg_perf_for_all_mailboxes;
execute avg_perf_for_all_mailboxes;
deallocate prepare avg_perf_for_all_mailboxes;
-- =====================================================================================================================================


-- View for average performance per aggregator =========================================================================================
set @avg_perf_per_aggregator = concat(
'select ml.id_aggregator,
		 ma.ip,
		 sum(TIME_TO_SEC(TIMEDIFF(ml.processing_end_time, ml.processing_start_time))) / (count(ml.id_mailbox) + 0.0) as ''Average time for processing one mailbox (sec)'',
		 sum(if(processed_mails_count > 0, TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time)), 0)) / (sum(processed_mails_count) + 0.0) as ''Seconds for one mail proccessing per mailbox'' 	 		
from mail_log as ml
		inner join mail_aggregators as ma on ml.id_aggregator = ma.id
where id_aggregator in (', @id_aggregator_query,')
group by ml.id_aggregator');

prepare avg_perf_per_aggregator  from @avg_perf_per_aggregator;
execute avg_perf_per_aggregator;
deallocate prepare avg_perf_per_aggregator;
-- ====================================================================================================================================


-- Metrics for empty mailboxes ========================================================================================================
set @empty_mailbox_metric = concat(
'select sum(TIME_TO_SEC(TIMEDIFF(processing_end_time, processing_start_time))) / (count(id_mailbox) + 0.0) as ''Average time for processing one mailbox (sec)''
from mail_log
where processed_mails_count = 0
and id_aggregator in (', @id_aggregator_query,')');

prepare empty_mailbox_metric  from @empty_mailbox_metric;
execute empty_mailbox_metric;
deallocate prepare empty_mailbox_metric;
-- ====================================================================================================================================


-- Threads performance in aggregators =================================================================================================
set @threads_in_aggr = concat(
'select ma.id, ma.ip, ma.start_work, ma.end_work, count(distinct ml.id_thread) as ''Threads count''
from mail_aggregators as ma
	 inner join mail_log as ml on ma.id = ml.id_aggregator
where ma.id in (', @id_aggregator_query,')
group by ma.id;');

prepare threads_in_aggr  from @threads_in_aggr;
execute threads_in_aggr;
deallocate prepare threads_in_aggr;
-- =====================================================================================================================================

-- Latest proccessed  mailboxes per thread =============================================================================================
set @latest_proccessed_mailbox_per_thread = concat('
select *
from mail_log ml1
inner join(
select max(id) as id
from mail_log ml
where ml.id_aggregator in (', @id_aggregator_query,')
group by id_thread, id_aggregator) q on q.id = ml1.id;');

prepare latest_proccessed_mailbox_per_thread  from @latest_proccessed_mailbox_per_thread;
execute latest_proccessed_mailbox_per_thread;
deallocate prepare latest_proccessed_mailbox_per_thread;
-- =====================================================================================================================================
-- Latest proccessed mailboxes duplicate in proccessing=================================================================================
set @latest_proccessed_mailbox_duplicates = concat('
select lpm.id_mailbox, mm.address, mm.imap, count(lpm.id_mailbox), count(distinct lpm.id_aggregator)
from (select ml1.id, ml1.id_mailbox, ml1.processing_end_time, ml1.id_aggregator
		from mail_log ml1
			inner join( select max(id) as id
							from mail_log ml
							where ml.id_aggregator in (', @id_aggregator_query,')
							group by id_thread, id_aggregator
			) q on q.id = ml1.id
		) lpm
		inner join mail_mailbox mm on mm.id = lpm.id_mailbox
		where lpm.processing_end_time is null
		group by lpm.id_mailbox
		having count(lpm.id_mailbox) > 1;');

prepare latest_proccessed_mailbox_duplicates  from @latest_proccessed_mailbox_duplicates;
execute latest_proccessed_mailbox_duplicates;
deallocate prepare latest_proccessed_mailbox_duplicates;
-- ======================================================================================================================================
