netsh interface portproxy reset
netsh interface portproxy add v4tov4 listenport=5222 listenaddress=0.0.0.0 connectport=5222 connectaddress=<IP>
netsh interface portproxy add v4tov4 listenport=5223 listenaddress=0.0.0.0 connectport=5223 connectaddress=<IP>
netsh interface portproxy add v4tov4 listenport=5280 listenaddress=0.0.0.0 connectport=5280 connectaddress=<IP>
