God.watch do |w|
  w.name = "mysql"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "/etc/init.d/mysql start"
  w.stop = "/etc/init.d/mysql stop"
  w.restart = "/etc/init.d/mysql restart"
  w.pid_file = "/var/run/mysqld/mysqld.pid"
  w.behavior(:clean_pid_file)

  w.start_if do |start|
    start.condition(:process_running) do |c|
      c.interval = 5.seconds
      c.running = false
    end
  end

  w.restart_if do |restart|
    restart.condition(:socket_responding) do |c|
      c.addr = '127.0.0.1'
      c.socket = 'tcp:3306'
      c.times = 5
      c.interval = 5.seconds
    end
  end
end