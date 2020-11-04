
#! /bin/bash
# /etc/init.d/spamtrainer
#
### BEGIN INIT INFO
# Provides: spamtrainer
# Required-Start:
# Should-Start:
# Required-Stop:
# Should-Stop:
# Default-Start:  3 5
# Default-Stop:   0 1 2 6
# Short-Description: Spam trainer daemon process
# Description:    Runs up the spam trainer daemon process
### END INIT INFO

case "$1" in
  start)
    echo "Starting server"
    # Start the daemon
    python /usr/share/spamtrainer/spamtrainer.py start
    ;;
  stop)
    echo "Stopping server"
    # Stop the daemon
    python /usr/share/spamtrainer/spamtrainer.py stop
    ;;
  restart)
    echo "Restarting server"
    python /usr/share/spamtrainer/spamtrainer.py restart
    ;;
  *)
    # Refuse to do other stuff
    echo "Usage: /etc/init.d/spamtrainer {start|stop|restart}"
    exit 1
    ;;
esac

exit 1
