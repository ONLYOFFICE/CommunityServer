dictionary = { "teamlabNotify" => 9811, "teamlabBackup" => 9812, "teamlabIndex" => 9813, "teamlabStorageMigrate" => 9814, "teamlabTelegram" => 9815, "teamlabStorageEncryption" => 9816, "teamlabThumbnailBuilder" => 9817, "teamlabFeed" => 9808 };

%w{Notify Backup Index StorageMigrate Telegram StorageEncryption ThumbnailBuilder Feed}.each do |serviceName|
    God.watch do |w|
      w.name = "onlyoffice#{serviceName}"
      w.group = "onlyoffice"
      w.grace = 15.seconds
      w.start = "systemctl start onlyoffice#{serviceName}"
      w.stop = "systemctl stop onlyoffice#{serviceName}"
      w.restart = "systemctl restart onlyoffice#{serviceName}"
      w.pid_file = "/tmp/onlyoffice#{serviceName}"

      w.behavior(:clean_pid_file)

      w.start_if do |start|
        start.condition(:process_running) do |c|
          c.interval = 10.seconds
          c.running = false
        end
      end

      w.restart_if do |restart|
       restart.condition(:http_response_code) do |c|
          c.host = 'localhost'
          c.port = dictionary["teamlab#{serviceName}"]
          c.path = "/teamlab#{serviceName}/health/check"
          c.code_is_not = 200
          c.times = 5
          c.interval = 5.seconds
        end
      end
    end
end