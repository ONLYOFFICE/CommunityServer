============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select count(id) from mail_attachment where id_mail = @p0 and need_remove = @p1 and content_id is null</field>
		<field name="count">4278</field>
		<field name="avg">2</field>
		<field name="max">1302</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT COUNT(id)
FROM mail_attachment
WHERE id_mail = 17672969 AND need_remove = 0 AND content_id IS NULL

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">id_mail</field>
		<field name="key">id_mail</field>
		<field name="key_len">772</field>
		<field name="ref">const,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select id from mail_attachment where id_mail = @p0 and need_remove = @p1 and content_id = @p2</field>
		<field name="count">121</field>
		<field name="avg">23</field>
		<field name="max">897</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT id
FROM mail_attachment
WHERE id_mail = 17672969 AND need_remove = 0 AND content_id = '0A1F2EDDA24C4660A1F1E4E6B8A6366B@avsmedia.net'

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">main</field>
		<field name="key_len">776</field>
		<field name="ref">const,const,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using where; Using index</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select m.id_user, sum(a.size) as size from mail_attachment a inner join mail_mail m on a.id_mail = m.id where m.tenant = @p0 and a.need_remove = @p1 group by 1 order by 2 desc</field>
		<field name="count">6</field>
		<field name="avg">13249</field>
		<field name="max">61097</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT m.id_user, SUM(a.size) AS size
FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id
WHERE a.tenant = 10112 AND a.need_remove = 0
GROUP BY 1
ORDER BY 2 DESC

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">m</field>
		<field name="type">ref</field>
		<field name="possible_keys">PRIMARY,time_modified,main</field>
		<field name="key">main</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">155336</field>
		<field name="Extra">Using where; Using index; Using temporary; Using filesort</field>
	</row>
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">a</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">8</field>
		<field name="ref">teamlab_info.m.id,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using index</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id from mail_attachment inner join mail_mail on mail_mail.id = mail_attachment.id_mail where mail_attachment.id = @p0 and mail_attachment.need_remove = @p1 and mail_mail.tenant = @p2 and mail_mail.id_user = @p3</field>
		<field name="count">143</field>
		<field name="avg">7</field>
		<field name="max">188</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id
FROM mail_attachment
INNER JOIN mail_mail ON mail_mail.id = mail_attachment.id_mail
WHERE mail_attachment.id = 4746930 AND mail_attachment.need_remove = 0 AND mail_mail.tenant = 10112 AND mail_mail.id_user = '7fba99c5-1291-41b0-aa32-5803d715473c'

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">const</field>
		<field name="possible_keys">PRIMARY,quota_index,main</field>
		<field name="key">PRIMARY</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">1</field>
		<field name="Extra"></field>
	</row>
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_mail</field>
		<field name="type">const</field>
		<field name="possible_keys">PRIMARY,time_modified,main</field>
		<field name="key">PRIMARY</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">1</field>
		<field name="Extra"></field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id from mail_attachment inner join mail_mail on mail_mail.id = mail_attachment.id_mail where mail_mail.id = @p0 and mail_attachment.need_remove = @p1 and content_id is null and mail_mail.tenant = @p2 and mail_mail.id_user = @p3</field>
		<field name="count">1591</field>
		<field name="avg">38</field>
		<field name="max">1104</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT mail_attachment.id, mail_attachment.name, mail_attachment.stored_name, mail_attachment.type, mail_attachment.size, mail_attachment.file_number, mail_mail.stream, mail_mail.tenant, mail_mail.id_user, mail_attachment.content_id
FROM mail_attachment
INNER JOIN mail_mail ON mail_mail.id = mail_attachment.id_mail
WHERE mail_mail.id = 17672969 AND mail_attachment.need_remove = 0 AND content_id IS NULL AND mail_mail.tenant = 10112 AND mail_attachment.tenant = 10112 AND mail_mail.id_user = '7fba99c5-1291-41b0-aa32-5803d715473c'

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_mail</field>
		<field name="type">const</field>
		<field name="possible_keys">PRIMARY,time_modified,main</field>
		<field name="key">PRIMARY</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">1</field>
		<field name="Extra"></field>
	</row>
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">tenant,id_mail</field>
		<field name="key">id_mail</field>
		<field name="key_len">772</field>
		<field name="ref">const,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select max(file_number) from mail_attachment where id_mail = @p0</field>
		<field name="count">38</field>
		<field name="avg">1</field>
		<field name="max">6</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT MAX(file_number)
FROM mail_attachment
WHERE id_mail = 12590 and tenant = 0

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">tenant,id_mail</field>
		<field name="key">tenant</field>
		<field name="key_len">8</field>
		<field name="ref">const,const</field>
		<field name="rows">6</field>
		<field name="Extra"></field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select sum(a.size) from mail_attachment a inner join mail_mail m on a.id_mail = m.id where m.id_mailbox = @mailbox_id and m.tenant = @tid and a.need_remove != @need_remove</field>
		<field name="count">12</field>
		<field name="avg">146</field>
		<field name="max">1455</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT SUM(a.size)
FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id
WHERE m.id_mailbox = 25997 AND m.tenant = 10112 AND a.tenant = 10112 AND a.need_remove = 0

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">m</field>
		<field name="type">ref</field>
		<field name="possible_keys">PRIMARY,time_modified,mime_message_id,main</field>
		<field name="key">mime_message_id</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">8580</field>
		<field name="Extra">Using where</field>
	</row>
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">a</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">8</field>
		<field name="ref">teamlab_info.m.id,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using index</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select sum(size) from mail_attachment where id_mail = @p0 and need_remove = @p1 and content_id is null</field>
		<field name="count">38</field>
		<field name="avg">5</field>
		<field name="max">150</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT SUM(size)
FROM mail_attachment
WHERE id_mail = 17672969 AND need_remove = 0 AND content_id IS NULL

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">main</field>
		<field name="key_len">776</field>
		<field name="ref">const,const,const</field>
		<field name="rows">1</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select sum(size) from mail_attachment where tenant = @p0 and need_remove = @p1 and id_mail = @p2</field>
		<field name="count">3693</field>
		<field name="avg">3</field>
		<field name="max">646</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT SUM(size)
FROM mail_attachment
WHERE tenant = 10112 AND need_remove = 0 AND id_mail = 17672969

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">8</field>
		<field name="ref">const,const</field>
		<field name="rows">2</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<?xml version="1.0" encoding="utf8"?>

<table_data name="sql_log">
	<row>
		<field name="sqltext">select sum(size) from mail_attachment where tenant = @p0 and need_remove = @p1 and id_mail in (@p2,@p3,@p4)</field>
		<field name="count">3</field>
		<field name="avg">273</field>
		<field name="max">384</field>
		<field name="min">90</field>
	</row>
	<row>
		<field name="sqltext">select sum(size) from mail_attachment where tenant = @p0 and need_remove = @p1 and id_mail in (@p2,@p3,@p4,@p5)</field>
		<field name="count">1</field>
		<field name="avg">409</field>
		<field name="max">409</field>
		<field name="min">409</field>
	</row>
</table_data>

EXPLAIN
SELECT SUM(size)
FROM mail_attachment
WHERE tenant = 10112 AND need_remove = 0 AND id_mail IN (17672969,17672968,17672967)

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">range</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">8</field>
		<field name="ref" xsi:nil="true" />
		<field name="rows">4</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">select sum(size) from mail_attachment where tenant = @p0 and need_remove = @p1 and id_mail = @p2 and id = @p3</field>
		<field name="count">2</field>
		<field name="avg">1</field>
		<field name="max">1</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT SUM(size)
FROM mail_attachment
WHERE tenant = 10112 AND need_remove = 0 AND id_mail = 17672969 AND id = 4746930

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">const</field>
		<field name="possible_keys">PRIMARY,quota_index,main</field>
		<field name="key">PRIMARY</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">1</field>
		<field name="Extra"></field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">update mail_attachment a inner join mail_mail m on a.id_mail = m.id set a.need_remove = @need_remove where m.id_mailbox = @mailbox_id</field>
		<field name="count">12</field>
		<field name="avg">308</field>
		<field name="max">3666</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT *
FROM mail_attachment a
INNER JOIN mail_mail m ON a.id_mail = m.id
WHERE m.id_mailbox = 25997

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">m</field>
		<field name="type">ref</field>
		<field name="possible_keys">PRIMARY,mime_message_id</field>
		<field name="key">mime_message_id</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">8580</field>
		<field name="Extra"></field>
	</row>
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">a</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">4</field>
		<field name="ref">teamlab_info.m.id</field>
		<field name="rows">1</field>
		<field name="Extra"></field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">update mail_attachment set need_remove = @p0 where tenant = @p1 and id_mail = @p2</field>
		<field name="count">3693</field>
		<field name="avg">1</field>
		<field name="max">1203</field>
		<field name="min">0</field>
	</row>
</table_data>

EXPLAIN
SELECT *
FROM mail_attachment
WHERE tenant = 10112 AND id_mail = 17672969

<table_data name="UnknownTable">
	<row>
		<field name="id">1</field>
		<field name="select_type">SIMPLE</field>
		<field name="table">mail_attachment</field>
		<field name="type">ref</field>
		<field name="possible_keys">quota_index,main</field>
		<field name="key">quota_index</field>
		<field name="key_len">4</field>
		<field name="ref">const</field>
		<field name="rows">2</field>
		<field name="Extra">Using where</field>
	</row>
</table_data>

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">update mail_attachment set need_remove = @p0 where tenant = @p1 and id_mail = @p2 and id = @p3</field>
		<field name="count">2</field>
		<field name="avg">24</field>
		<field name="max">48</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT *
FROM mail_attachment
WHERE tenant = 10112 AND id_mail = 17672969 AND id = 4746930

============================================================================================================================================================================

<table_data name="sql_log">
	<row>
		<field name="sqltext">update mail_attachment set need_remove = @p0 where tenant = @p1 and id_mail in (@p2,@p3,@p4)</field>
		<field name="count">3</field>
		<field name="avg">230</field>
		<field name="max">388</field>
		<field name="min">1</field>
	</row>
	<row>
		<field name="sqltext">update mail_attachment set need_remove = @p0 where tenant = @p1 and id_mail in (@p2,@p3,@p4,@p5)</field>
		<field name="count">1</field>
		<field name="avg">1</field>
		<field name="max">1</field>
		<field name="min">1</field>
	</row>
</table_data>

EXPLAIN
SELECT *
FROM mail_attachment
WHERE tenant = 10112 AND id_mail IN (17672969,17672968,17672967)

============================================================================================================================================================================