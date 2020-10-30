God.watch do |w|
  w.name = "onlyofficeMailAggregator"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "systemctl start onlyofficeMailAggregator"
  w.stop = "systemctl stop onlyofficeMailAggregator"
  w.restart = "systemctl restart onlyofficeMailAggregator"
  w.pid_file = "/tmp/onlyofficeMailAggregator"

  w.behavior(:clean_pid_file)

  w.start_if do |start|
    start.condition(:process_running) do |c|
      c.interval = 10.seconds
      c.running = false
    end
  end

  w.restart_if do |restart|
    restart.condition(:memory_usage) do |c|
      c.above = 1000.megabytes
      c.times = 5
      c.interval = 10.seconds
    end
    restart.condition(:cpu_usage) do |c|
      c.above = 90.percent
      c.times = 5
      c.interval = 10.seconds
    end
    restart.condition(:lambda) do |c|
      c.interval = 10.seconds
      c.lambda = lambda{!File.exist?("/var/log/onlyoffice/mail.agg.log")}
    end
    restart.condition(:file_mtime) do |c|
      c.path = "/var/log/onlyoffice/mail.agg.log"
      c.max_age = 1.minutes
      c.interval = 10.seconds
    end
  end
end
