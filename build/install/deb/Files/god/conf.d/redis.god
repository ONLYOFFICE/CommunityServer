God.watch do |w|
  w.name = "redis"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "/etc/init.d/redis-server start"
  w.stop = "/etc/init.d/redis-server stop"
  w.restart = "/etc/init.d/redis-server restart"
  w.pid_file = "/var/run/redis/redis-server.pid"
  w.behavior(:clean_pid_file)

  w.start_if do |start|
      start.condition(:process_running) do |c|
        c.interval = 5.seconds
        c.running = false
      end
    end
end