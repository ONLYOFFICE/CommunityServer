<?xml version="1.0" encoding="UTF-8"?>
<clientConfig version="1.1">
  <emailProvider id="rr.com">
    <!-- per MX, as actual addresses are user@tampabay.rr.com -->
    <domain>rr.com</domain>
    <displayName>RoadRunner</displayName>
    <displayShortName>RR</displayShortName>
    <incomingServer type="pop3">
      <hostname>pop-server.%EMAILDOMAIN%</hostname>
      <port>110</port>
      <socketType>plain</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </incomingServer>
    <outgoingServer type="smtp">
      <hostname>smtp-server.%EMAILDOMAIN%</hostname>
      <port>25</port>
      <socketType>plain</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </outgoingServer>
    <documentation url="http://help.rr.com/HMSFaqs/e_emailserveraddys.aspx" />
    <documentation url="http://help.rr.com/HMSFaqs/e_SmtpAuth.aspx" />
  </emailProvider>
</clientConfig>
