%w{onlyofficeFeed onlyofficeThumb onlyofficeSocketIO onlyofficeIndex onlyofficeStorageMigrate onlyofficeStorageEncryption onlyofficeTelegram onlyofficeNotify onlyofficeBackup onlyofficeMailCleaner onlyofficeMailWatchdog onlyofficeRadicale onlyofficeUrlShortener}.each do |serviceName|
  God.watch do |w|
    w.name = serviceName
    w.group = "onlyoffice"
    w.grace = 15.seconds
    w.start = "systemctl start #{serviceName}"
    w.stop = "systemctl stop #{serviceName}"
    w.restart = "systemctl restart #{serviceName}"
    w.pid_file = "/tmp/#{serviceName}"

    w.start_if do |start|
      start.condition(:process_running) do |c|
        c.interval = 10.seconds
        c.running = false
      end
    end

    w.restart_if do |restart|
      restart.condition(:memory_usage) do |c|
        c.above = 700.megabytes
        c.times = 5
        c.interval = 10.seconds
      end
      restart.condition(:cpu_usage) do |c|
        c.above = 90.percent
        c.times = 5
        c.interval = 10.seconds
      end
    end
  end
end
