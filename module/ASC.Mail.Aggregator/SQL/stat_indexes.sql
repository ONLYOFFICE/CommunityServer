ALTER TABLE `mail_aggregators`
	ADD INDEX `ip_index` (`ip`);
ALTER TABLE `mail_log`
	DROP INDEX `id_aggregator`,
	ADD INDEX `id_aggregator` (`id_aggregator`, `processing_start_time`);