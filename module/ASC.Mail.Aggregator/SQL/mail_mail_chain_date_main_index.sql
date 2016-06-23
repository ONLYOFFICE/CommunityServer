ALTER TABLE `mail_mail`
	DROP INDEX `chain_date`,
	DROP INDEX `main`,
	ADD INDEX `main` (`tenant`, `id_user`, `is_removed`, `folder`, `chain_date`);