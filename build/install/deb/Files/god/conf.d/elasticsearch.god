God.watch do |w|
  w.name = "elasticsearch"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "/etc/init.d/elasticsearch start"
  w.stop = "/etc/init.d/elasticsearch stop"
  w.restart = "/etc/init.d/elasticsearch restart"
  w.pid_file = "/var/run/elasticsearch/elasticsearch.pid"
  w.behavior(:clean_pid_file)

  w.start_if do |start|
      start.condition(:process_running) do |c|
        c.interval = 10.seconds
        c.running = false
      end
    end
end
