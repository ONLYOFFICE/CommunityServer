God.watch do |w|
  w.name = "nginx"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "/etc/init.d/nginx start"
  w.stop = "/etc/init.d/nginx stop"
  w.restart = "/etc/init.d/nginx restart"
  w.pid_file = "/var/run/nginx.pid"
  w.behavior(:clean_pid_file)
  w.keepalive
  
  w.start_if do |start|
      start.condition(:process_running) do |c|
        c.interval = 10.seconds
        c.running = false
      end
    end
end
