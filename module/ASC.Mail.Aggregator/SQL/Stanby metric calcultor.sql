select cur.id_aggregator, sum(TIME_TO_SEC(TIMEDIFF(next.processing_start_time, cur.processing_end_time))) as 'Standby time in seconds'
from mail_log as cur
LEFT JOIN   mail_log       AS next
      ON next.id = (SELECT MIN(id)
		 from mail_log
		 where id > cur.id and id_aggregator = cur.id_aggregator and id_thread = cur.id_thread
		 order by id_aggregator, id_thread, processing_start_time)
where next.id is not null and cur.processing_end_time is not null and cur.id_aggregator in (91, 90, 89, 76, 74, 73)
group by cur.id_aggregator
order by cur.id_aggregator, cur.id_thread, cur.processing_start_time;
