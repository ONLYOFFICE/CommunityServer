using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ASC.Common.Logging;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Storage.ZipOperators;

using Renci.SshNet;

namespace ASC.Data.Backup.Service
{
    public class BackupMailServerFilesService
    {
        /// <summary>
        /// State of the mail server connection
        /// </summary>
        public bool IsConnected { get; private set; }

        private SftpClient _sftpClient;

        public static string mailFilesDirectoryInArchiv = "mailServerFiles";

        private ILog _logger;

        public string pathToMailFilesOnHost;

        private string host;
        private string userName;
        private string pathToKey;
        private string password;

        public BackupMailServerFilesService(ILog Logger)
        {
            var config = BackupConfigurationSection.GetSection();

            _logger = Logger;

            host = config.MailServer.HostAddress;
            userName = config.MailServer.HostUsername;
            pathToKey = config.MailServer.HostKey;
            password = config.MailServer.HostPassword;
            pathToMailFilesOnHost = config.MailServer.PathToMailFilesOnHost;

            _logger.Debug($"BackupMailServerFilesService: Host = {host}, UserName={userName}, password={password}, pathToKey={pathToKey}, pathToMailFilesOnHost={pathToMailFilesOnHost}.");

            if (string.IsNullOrEmpty(host)
                || string.IsNullOrEmpty(userName))
            {
                _logger.Debug("MailServerHostAddress and MailServerHostUsername don't have values.");

                IsConnected = false;

                return;
            }

            IsConnected = Connect();
        }

        /// <summary>
        /// Connect to Mail Server
        /// </summary>
        /// <returns>True if connect establish, False if not.</returns>
        public bool Connect()
        {
            try
            {
                var methods = new List<Renci.SshNet.AuthenticationMethod>();

                if (!string.IsNullOrEmpty(pathToKey))
                {
                    PrivateKeyFile keyFile = new PrivateKeyFile(pathToKey);
                    var keyFiles = new[] { keyFile };

                    methods.Add(new PrivateKeyAuthenticationMethod(userName, keyFiles));
                }

                if (!string.IsNullOrEmpty(password))
                {
                    methods.Add(new PasswordAuthenticationMethod(userName, password));
                };

                if (methods.Count == 0)
                {
                    _logger.Debug($"BackupMailServerFilesService didn`t connect, becouse pathToKey and password are empty.");
                }

                ConnectionInfo con = new ConnectionInfo(host, 22, userName, methods.ToArray());

                _sftpClient = new SftpClient(con);
                _sftpClient.Connect();
            }
            catch (Exception ex)
            {
                _logger.Debug($"BackupMailServerFilesService didn`t connect, becouse: {ex}");
            }

            return _sftpClient.IsConnected;
        }

        /// <summary>
        /// Download single file from the mail server and return the path to file on the local temp directory 
        /// </summary>
        /// <param name="pathRemoteFile"></param>
        /// <returns></returns>
        public string GetSingleFile(string pathRemoteFile)
        {
            if (!IsConnected) return "";

            string result = TempPath.GetTempFileName();

            using (Stream fileStream = System.IO.File.OpenWrite(result))
            {
                _sftpClient.DownloadFile(pathRemoteFile, fileStream);
            }

            return result;
        }

        /// <summary>
        /// Get List of files pathes on the mail server directory, if connection establish
        /// </summary>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        public string[] GetFilesList(string remoteDirectory)
        {
            if (!IsConnected) return new string[0];

            var files = _sftpClient.ListDirectory(remoteDirectory);

            return files.Where(x => !x.IsDirectory).Select(f => f.Name).ToArray();
        }

        /// <summary>
        /// Get List of directories on the mail server directory, if connection establish
        /// </summary>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        /// <exception cref="DbBackupException"></exception>
        public string[] GetDirectoryList(string remoteDirectory)
        {
            string[] result = new string[0];

            if (!IsConnected) return result;

            try
            {
                var files = _sftpClient.ListDirectory(remoteDirectory);

                result = files.Where(x => x.IsDirectory)
                    .Select(f => f.Name)
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.Debug($"Get Directory List error. Path={remoteDirectory}.{ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Recursively download and save by fileSaver action the directory from the mail server
        /// </summary>
        /// <param name="path">Directory on the mail server</param>
        /// <param name="fileSaver">Action for save file</param>
        /// <returns>Count of saved files</returns>
        public int SaveDirectory(string path, Action<String, Stream> fileSaver)
        {
            var result = 0;

            if (!IsConnected) return result;

            _logger.Debug($"Save Directory {path} start.");

            var files = GetFilesList(path);

            foreach (var file in files)
            {
                var filePath = path + '/' + file;

                try
                {
                    _logger.Debug($"Try save file {filePath}.");

                    using (Stream fileStream = new MemoryStream())
                    {
                        _sftpClient.DownloadFile(filePath, fileStream);

                        fileSaver(mailFilesDirectoryInArchiv + filePath, fileStream);

                        result++;
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error($"Save file {filePath} error. {ex.Message}");
                }
            }

            var directorys = GetDirectoryList(path);

            foreach (var directory in directorys)
            {
                if (directory == ".." || directory == ".") continue;

                result += SaveDirectory(path + '/' + directory, fileSaver);
            }

            _logger.Debug($"Upload Directory {path} ended. {result} files was saved.");

            return result;
        }

        /// <summary>
        /// Upload the directory to the mail server
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns>Count of uploaded files</returns>
        public int UploadMailDirectory(IDataReadOperator dataReader)
        {
            _logger.Debug($"Upload Mail Directory started.");

            return UploadDirectory(mailFilesDirectoryInArchiv + pathToMailFilesOnHost, dataReader);
        }
        /// <summary>
        /// Upload the directory to the mail server
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dataReader"></param>
        /// <returns>Count of uploaded files</returns>
        public int UploadDirectory(string path, IDataReadOperator dataReader)
        {
            var result = 0;

            if (!IsConnected) return result;

            _logger.Debug($"Upload Directory {path} start.");

            try
            {
                var abloluteFolderName = path.Substring(path.IndexOf(pathToMailFilesOnHost));

                if(_sftpClient.Exists(abloluteFolderName))
                {
                    _logger.Debug($"UploadDirectory: directory {abloluteFolderName} existed.");
                }
                else
                {
                    _sftpClient.CreateDirectory(abloluteFolderName);
                    _sftpClient.ChangePermissions(abloluteFolderName, 777);

                    _logger.Debug($"UploadDirectory: directory {abloluteFolderName} created.");
                }

                var directories = dataReader.GetDirectories(path);

                foreach (var directory in directories)
                {
                    try
                    {
                        result += UploadDirectory(directory, dataReader);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Upload Directory: upload directory {directory} exception: {ex.Message}");
                    }
                }

                var files = dataReader.GetEntries(path);

                foreach (var file in files)
                {
                    try
                    {
                        var destinationName = file.Substring(file.IndexOf(pathToMailFilesOnHost));

                        _logger.Debug($"Upload Directory: Begin start copy file {mailFilesDirectoryInArchiv + destinationName}");

                        using (var streamData = dataReader.GetEntry(mailFilesDirectoryInArchiv + destinationName))
                        {
                            if (streamData == null)
                            {
                                _logger.Debug($"Upload Directory: File didn't found: {file}");

                                continue;
                            }

                            _logger.Debug($"Upload Directory: place {streamData.Length} bytes file to {destinationName}");

                            using (var remoteFile = _sftpClient.Create(destinationName))
                            {
                                streamData.CopyTo(remoteFile);
                            }

                            _sftpClient.ChangePermissions(destinationName, 777);
                        }

                        result++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"UploadDirectory: upload file {file} exception: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"UploadDirectory exception: {ex.Message}");
            }

            _logger.Debug($"Upload Directory {path} stop. {result} files upload.");

            return result;
        }
    }
}
