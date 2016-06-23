delete from mail_log 
where id_aggregator not in(
	select ma.id 
	from mail_aggregators ma inner join (select max(ma.id) as ID
										from mail_aggregators ma
										group by ma.ip) as T
									 on T.ID = ma.id
	where ma.end_work is null); 