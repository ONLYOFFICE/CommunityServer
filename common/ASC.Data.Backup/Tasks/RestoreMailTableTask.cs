using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;

using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Service;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;
using ASC.Data.Storage.ZipOperators;

namespace ASC.Data.Backup.Tasks
{
    internal class RestoreMailTableTask
    {
        private const int TransactionLength = 10000;

        private readonly ILog _logger;
        private readonly IDataReadOperator _dataReader;
        private readonly MailTableSpecifics _module;
        private readonly DbFactory _factory;

        private readonly string[] tableCreateScripts = new string[] { "CREATE TABLE IF NOT EXISTS `dkim` (`id` int unsigned NOT NULL AUTO_INCREMENT, `domain_name` varchar(255) CHARACTER SET latin1 NOT NULL, `selector` varchar(63) NOT NULL, `private_key` text CHARACTER SET latin1, `public_key` text CHARACTER SET latin1, PRIMARY KEY (`id`),  KEY `domain_name` (`domain_name`)) ENGINE=InnoDB AUTO_INCREMENT=169 DEFAULT CHARSET=utf8mb3;",
        "CREATE TABLE `bayes_seen` (  `id` int NOT NULL DEFAULT '0',  `msgid` varchar(200) CHARACTER SET latin1 COLLATE latin1_bin NOT NULL DEFAULT '',  `flag` char(1) NOT NULL DEFAULT '',  PRIMARY KEY (`id`,`msgid`)) ENGINE=MyISAM DEFAULT CHARSET=latin1;",
        "CREATE TABLE `bayes_token` (  `id` int NOT NULL DEFAULT '0',  `token` char(5) NOT NULL DEFAULT '',  `spam_count` int NOT NULL DEFAULT '0',  `ham_count` int NOT NULL DEFAULT '0',  `atime` int NOT NULL DEFAULT '0',  PRIMARY KEY (`id`,`token`),  KEY `bayes_token_idx1` (`id`,`atime`)) ENGINE=MyISAM DEFAULT CHARSET=latin1;",
            "CREATE TABLE `maddr` (  `partition_tag` int DEFAULT '0',  `id` bigint unsigned NOT NULL AUTO_INCREMENT,  `email` varbinary(255) NOT NULL,  `domain` varchar(255) NOT NULL,  PRIMARY KEY (`id`),  UNIQUE KEY `part_email` (`partition_tag`,`email`),  KEY `maddr_idx_email` (`email`),  KEY `maddr_idx_domain` (`domain`)) ENGINE=InnoDB AUTO_INCREMENT=11498 DEFAULT CHARSET=utf8mb3;",
            "CREATE TABLE `msgrcpt` (  `partition_tag` int NOT NULL DEFAULT '0',  `mail_id` varbinary(16) NOT NULL,  `rseqnum` int NOT NULL DEFAULT '0',  `rid` bigint unsigned NOT NULL,  `is_local` char(1) NOT NULL DEFAULT '',  `content` char(1) NOT NULL DEFAULT '',  `ds` char(1) NOT NULL,  `rs` char(1) NOT NULL,  `bl` char(1) DEFAULT '',  `wl` char(1) DEFAULT '',  `bspam_level` float DEFAULT NULL,  `smtp_resp` varchar(255) DEFAULT '',  PRIMARY KEY (`partition_tag`,`mail_id`,`rseqnum`),  KEY `msgrcpt_idx_mail_id` (`mail_id`),  KEY `msgrcpt_idx_rid` (`rid`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;",
            "CREATE TABLE `msgs` (  `partition_tag` int NOT NULL DEFAULT '0',  `mail_id` varbinary(16) NOT NULL,  `secret_id` varbinary(16) DEFAULT '',  `am_id` varchar(20) NOT NULL,  `time_num` int unsigned NOT NULL,  `time_iso` char(16) NOT NULL,  `sid` bigint unsigned NOT NULL,  `policy` varchar(255) DEFAULT '',  `client_addr` varchar(255) DEFAULT '',  `size` int unsigned NOT NULL,  `originating` char(1) NOT NULL DEFAULT '',  `content` char(1) DEFAULT NULL,  `quar_type` char(1) DEFAULT NULL,  `quar_loc` varbinary(255) DEFAULT '',  `dsn_sent` char(1) DEFAULT NULL,  `spam_level` float DEFAULT NULL,  `message_id` varchar(255) DEFAULT '',  `from_addr` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT '',  `subject` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT '',  `host` varchar(255) NOT NULL,  PRIMARY KEY (`partition_tag`,`mail_id`),  KEY `msgs_idx_sid` (`sid`),  KEY `msgs_idx_mess_id` (`message_id`),  KEY `msgs_idx_time_num` (`time_num`),  KEY `msgs_idx_mail_id` (`mail_id`),  KEY `msgs_idx_content` (`content`),  KEY `msgs_idx_quar_type` (`quar_type`),  KEY `msgs_idx_content_time_num` (`content`,`time_num`),  KEY `msgs_idx_spam_level` (`spam_level`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;"
        };

        public RestoreMailTableTask(ILog logger, IDataReadOperator dataReader, string configPath, string dbconnection)
        {
            _logger = logger;
            _dataReader = dataReader;
            _module = new MailTableSpecifics();
            _factory = new DbFactory(configPath) { ConnectionStringSettings = new ConnectionStringSettings("mailTable", dbconnection, "MySql.Data.MySqlClient") };
        }

        internal void RunJob()
        {
            _logger.DebugFormat("begin restore data for mail table");

            using (var connection = _factory.OpenConnection())
            {
                RestoreMailServerTables(connection);

                foreach (var table in _module.Tables)
                {
                    _logger.DebugFormat("begin restore table {0}", table.Name);

                    var transactionsCommited = 0;
                    var rowsInserted = 0;
                    ActionInvoker.Try(
                        state =>
                            RestoreTable(connection.Fix(), (MailTableInfo)state, ref transactionsCommited,
                                ref rowsInserted), table, 5,
                        onFailure: error => { throw ThrowHelper.CantRestoreTable(table.Name, error); });

                    _logger.DebugFormat("{0} rows inserted for table {1}", rowsInserted, table.Name);
                }
            }

            _logger.DebugFormat("end restore data for mail table");

            RestoreMailServerFiles();
        }

        private void RestoreMailServerFiles()
        {
            try
            {
                var backupMailServerFilesService = new BackupMailServerFilesService(_logger);
                backupMailServerFilesService.UploadMailDirectory(_dataReader);
            }
            catch (Exception ex)
            {
                _logger.DebugFormat($"RestoreMailServerFiles exception: {ex.Message}");
            }
        }

        private void RestoreTable(DbConnection connection, MailTableInfo tableInfo, ref int transactionsCommited, ref int rowsInserted)
        {
            SetColumns(connection, tableInfo);

            using (var stream = _dataReader.GetEntry(KeyHelper.GetMailTableZipKey(tableInfo.Name)))
            {
                foreach (
                    IEnumerable<DataRowInfo> rows in
                        GetRows(stream)
                            .Skip(transactionsCommited * TransactionLength)
                            .MakeParts(TransactionLength))
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        int rowsSuccess = 0;
                        foreach (DataRowInfo row in rows)
                        {
                            var insertCommand = _module.CreateInsertCommand(connection, tableInfo,
                                row);
                            if (insertCommand == null)
                            {
                                _logger.WarnFormat("Can't create command to insert row to {0} with values [{1}]", tableInfo,
                                    row);
                                continue;
                            }
                            insertCommand.WithTimeout(120).ExecuteNonQuery();
                            rowsSuccess++;
                        }

                        transaction.Commit();
                        transactionsCommited++;
                        rowsInserted += rowsSuccess;
                    }
                }
            }
        }

        private void SetColumns(DbConnection connection, MailTableInfo table)
        {
            var showColumnsCommand = _factory.CreateShowColumnsCommand(table.Name);
            showColumnsCommand.Connection = connection;
            table.Columns = showColumnsCommand.ExecuteList().Select(x => Convert.ToString(x[0])).ToArray();
        }

        private IEnumerable<DataRowInfo> GetRows(Stream xmlStream)
        {
            if (xmlStream == null)
                return Enumerable.Empty<DataRowInfo>();

            var rows = DataRowInfoReader.ReadFromStream(xmlStream);

            return rows;
        }

        private int RestoreMailServerTables(DbConnection connection)
        {
            int result = 0;

            using (var transaction = connection.BeginTransaction())
            {
                foreach (var script in tableCreateScripts)
                {
                    var insertCommand = connection.CreateCommand(script);

                    try
                    {
                        result += insertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"RestoreMailServerTables error: {ex.Message}");
                    }
                }

                transaction.Commit();
            }

            return result;
        }
    }
}
