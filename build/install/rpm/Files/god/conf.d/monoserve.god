module God
  module Conditions
    class SocketConnectedWithinTimeout < SocketResponding
      def test
        socket = nil
        self.info = []
        begin
          Timeout.timeout(5) do
            socket = UNIXSocket.new(self.path)
          end
        rescue Timeout::Error
          self.info = 'Socket connection timeout'
          return true
        rescue Exception => ex
          self.info = "Failed connected to socket with exception: #{ex}"
          socket.close if socket.is_a?(UNIXSocket)
          return true
        end
        socket.close
        self.info = 'Unix socket is responding'
        return false
      end
    end
  end
end

God.watch do |w|
  w.name = "monoserve"
  w.group = "onlyoffice"
  w.grace = 15.seconds
  w.start = "systemctl restart monoserve"
  w.stop = "systemctl stop monoserve"
  w.restart = "systemctl restart monoserve"
  w.unix_socket = "/var/run/onlyoffice/onlyoffice.socket"

  w.start_if do |start|
    start.condition(:socket_connected_within_timeout) do |c|
      c.family = 'unix'
      c.path = '/var/run/onlyoffice/onlyoffice.socket'
      c.times = 5
      c.interval = 5.seconds
    end
  end

  w.restart_if do |restart|
    restart.condition(:socket_connected_within_timeout) do |c|
      c.family = 'unix'
      c.path = '/var/run/onlyoffice/onlyoffice.socket'
      c.times = 5
      c.interval = 5.seconds
    end
    restart.condition(:http_response_code) do |c|
      c.host = '127.0.0.1'
      c.path = '/api/2.0/capabilities.json'
      c.code_is_not = 200
      c.times = 5
      c.interval = 5.seconds
    end
    restart.condition(:cpu_usage) do |c|
      c.above = 90.percent
      c.times = 5
      c.interval = 3.minutes
    end
  end
end
