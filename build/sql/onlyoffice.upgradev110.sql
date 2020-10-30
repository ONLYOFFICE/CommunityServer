DELIMITER DLM00

DROP PROCEDURE IF EXISTS upgrade110 DLM00

CREATE PROCEDURE upgrade110()
BEGIN

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_item' AND COLUMN_NAME = 'quantity') <> 'decimal' THEN
        ALTER TABLE `crm_invoice_item` CHANGE COLUMN `quantity` `quantity` DECIMAL(10,2) NOT NULL DEFAULT '0.00' AFTER `price`;
    END IF;

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_item' AND COLUMN_NAME = 'stock_quantity') <> 'decimal' THEN
        ALTER TABLE `crm_invoice_item` CHANGE COLUMN `stock_quantity` `stock_quantity` DECIMAL(10,2) NOT NULL DEFAULT '0.00' AFTER `quantity`;
    END IF;

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_line' AND COLUMN_NAME = 'quantity') <> 'decimal' THEN
        ALTER TABLE `crm_invoice_line` CHANGE COLUMN `quantity` `quantity` DECIMAL(10,2) NOT NULL DEFAULT '0.00' AFTER `description`;
    END IF;

    IF (SELECT DATA_TYPE FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_line' AND COLUMN_NAME = 'discount') <> 'decimal' THEN
        ALTER TABLE `crm_invoice_line` CHANGE COLUMN `discount` `discount` DECIMAL(10,2) NOT NULL DEFAULT '0.00' AFTER `price`;
    END IF;

    IF EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'crm_invoice_item' AND COLUMN_NAME = 'quantity') THEN
        ALTER TABLE `crm_invoice_item` DROP COLUMN `quantity`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'notify_queue' AND COLUMN_NAME = 'auto_submitted') THEN
        ALTER TABLE `notify_queue` ADD COLUMN `auto_submitted` VARCHAR(64) NULL DEFAULT NULL AFTER `attachments`;
    END IF;

    IF NOT EXISTS(SELECT * FROM information_schema.`COLUMNS` WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'core_usersecurity' AND COLUMN_NAME = 'LastModified') THEN
        ALTER TABLE `core_usersecurity` ADD COLUMN `LastModified` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP() ON UPDATE CURRENT_TIMESTAMP() AFTER `pwdhashsha512`;
    END IF;

    UPDATE `tenants_quota` SET `features` = 'domain,audit,controlpanel,healthcheck,ldap,sso,whitelabel,branding,ssbranding,update,support,portals:10000,discencryption,privacyroom' WHERE `tenant` = -1 and `name` NOT LIKE '%saas%';
    UPDATE `tenants_quota` SET `features` = 'docs,domain,audit,controlpanel,healthcheck,ldap,sso,whitelabel,branding,ssbranding,update,support,portals:10000,discencryption,privacyroom' WHERE tenant = -1000;

    CREATE TABLE IF NOT EXISTS `telegram_users` (
        `portal_user_id` VARCHAR(38) NOT NULL,
        `tenant_id` INT(11) NOT NULL,
        `telegram_user_id` INT(11) NOT NULL,
        PRIMARY KEY (`tenant_id`, `portal_user_id`),
        INDEX `tgId` (`telegram_user_id`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;


    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/crm/default', 'Products/CRM/Default')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'CRMPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/crm/deals', 'Products/CRM/Deals')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'CRMPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/crm/tasks', 'Products/CRM/Tasks')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'CRMPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/crm', 'Products/CRM')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'CRMPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects/messages', 'Products/Projects/Messages')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'PatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects/tasks', 'Products/Projects/Tasks')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'PatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects/projects', 'Products/Projects/Projects')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'PatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects/milestones', 'Products/Projects/Milestones')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'PatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects', 'Products/Projects')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'PatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/files', 'Products/Files')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'FilesPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/files', 'Products/Files')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'WebstudioNotifyPatternResource.resx');	

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/projects', 'Products/Projects')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'WebstudioNotifyPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/community', 'Products/Community')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'WebstudioNotifyPatternResource.resx');

    UPDATE `res_data`
    SET `textValue` = REPLACE(`textValue`, 'products/crm', 'Products/CRM')
    WHERE `fileid` = (SELECT `id` FROM `res_files` WHERE `resName` = 'WebstudioNotifyPatternResource.resx');


END DLM00

CALL upgrade110() DLM00

DELIMITER ;
